using System.IO;
using System.Net.Http.Json;
using System.Reflection.Metadata;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using AutoMapper;
using Azure.Storage.Blobs;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using OfficeOpenXml;
using TestFunction1.Models;
using Azure.Messaging.ServiceBus;

namespace TestFunction1
{
    public class SFTPMonitor
    {
        private readonly ILogger<SFTPMonitor> _logger;
        private readonly IMapper _mapper;
        private readonly string serviceBusConnectionString = Environment.GetEnvironmentVariable("SFTPServiceBusConnectionString");
        private readonly string queueName = "incoming";

        public SFTPMonitor(ILogger<SFTPMonitor> logger, IMapper mapper)
        {
            _logger = logger;
            _mapper = mapper;
        }

        [Function(nameof(SFTPMonitor))]
        public async Task Run([BlobTrigger("incoming/{name}", Connection = "AzureWebJobsStorage")] Stream myBlob, 
            string name,
            string blobTrigger)
        {
            // Check if the file extension is .xlsx
            if (!name.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogInformation($"Skipping file: {name} as it is not an Excel file.");
                return; // Skip processing non-XLSX files
            }
            _logger.LogInformation($"C# Blob trigger function Processed blob\n Name:{name} \n Size: {myBlob.Length} Bytes");

            try
            {
                var relativePath = blobTrigger.StartsWith("incoming/")
                    ? blobTrigger.Substring("incoming/".Length)
                    : blobTrigger;

                // Extract the folder path from the relative path
                var folderPath = Path.GetDirectoryName(relativePath) ?? string.Empty;

                var settingsBlobPath = Path.Combine(folderPath, "settings.json");
                var settings = await LoadSettingsFile(settingsBlobPath);

                // Process the blob content and convert it to a list of ExcelRecord objects
                List<BetterTrucksOrder> records = ProcessExcelFile(myBlob);
                List<AmerishipOrder> amerishipOrders = ConvertOrders(records);
                List<CustomerOrderDTO> orderDtos = new List<CustomerOrderDTO>();

                foreach(var order  in amerishipOrders)
                {
                    CustomerOrderDTO newOrder = new CustomerOrderDTO
                    {
                        Settings = settings,
                        Order = order
                    };

                    orderDtos.Add(newOrder);

                }

                await SendMessagesToServiceBus(orderDtos);

                // Log the number of records processed
                _logger.LogInformation($"Successfully processed {records.Count} records.");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error processing blob {name}: {ex.Message}");
            }
        }

        private async Task<CustomerSettings> LoadSettingsFile(string settingsFilePath)
        {
            try
            {
                // Read settings.json from the same folder
                var settingsContent = await ReadBlobContentAsync("incoming", settingsFilePath);
                _logger.LogInformation($"settings.json content: {settingsContent}");
                CustomerSettings settings = JsonSerializer.Deserialize<CustomerSettings>(settingsContent);
                return settings ?? throw new Exception("Failed to deserialize settings.json");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error accessing settings.json: {ex.Message}");
            }

            return null;
        }

        /// <summary>
        /// Reads the content of a blob from Azure Storage.
        /// </summary>
        /// <param name="containerName">The container name.</param>
        /// <param name="blobName">The blob name.</param>
        /// <returns>The blob content as a string.</returns>
        private static async Task<string> ReadBlobContentAsync(string containerName, string blobName)
        {
            var blobServiceClient = new BlobServiceClient(Environment.GetEnvironmentVariable("AzureWebJobsStorage"));
            var blobContainerClient = blobServiceClient.GetBlobContainerClient(containerName);
            var blobClient = blobContainerClient.GetBlobClient(blobName);

            if (await blobClient.ExistsAsync())
            {
                var response = await blobClient.DownloadContentAsync();
                return response.Value.Content.ToString();
            }

            throw new FileNotFoundException($"Blob '{blobName}' not found in container '{containerName}'.");
        }


        /// <summary>
        /// Processes the Excel file and converts it into a list of ExcelRecord objects.
        /// </summary>
        /// <param name="stream">Stream of the Excel file.</param>
        /// <returns>List of ExcelRecord objects.</returns>
        private static List<BetterTrucksOrder> ProcessExcelFile(Stream stream)
        {
            var records = new List<BetterTrucksOrder>();
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            // Load the Excel file using EPPlus
            using (var package = new ExcelPackage(stream))
            {
                // Access the first worksheet
                var worksheet = package.Workbook.Worksheets[0];
                if (worksheet == null)
                    throw new Exception("The Excel file does not contain any worksheets.");

                int rows = worksheet.Dimension.Rows;
                int cols = worksheet.Dimension.Columns;

                // Retrieve the headers from the first row
                var headers = new Dictionary<string, int>(); // Maps column name to column index (1-based)

                // Assume the first row is the header row
                for (int col = 1; col <= cols; col++)
                {
                    string headerText = worksheet.Cells[1, col].Text.Trim();
                    if (!string.IsNullOrEmpty(headerText))
                    {
                        headers[headerText] = col;
                    }
                }

                // List of required columns
                var requiredColumns = new List<string>
                {
                    "Tracking Number", "Company", "Contact Name", "City", "State", "Postal Code", "Address Line 1",
                    "Country", "Email", "Phone", "Date", "Signature Required", "Signature Type", "Adult Signature"
                };

                // Check that all required columns are present
                foreach (var column in requiredColumns)
                {
                    if (!headers.ContainsKey(column))
                    {
                        throw new Exception($"Missing required column: {column}");
                    }
                }

                // Parse rows starting from the second row (assuming the first row is headers)
                for (int i = 2; i <= rows; i++)
                {
                    var record = new BetterTrucksOrder
                    {
                        TrackingNumber = worksheet.Cells[i, headers["Tracking Number"]].Text,
                        Company = worksheet.Cells[i, headers["Company"]].Text,
                        ContactName = worksheet.Cells[i, headers["Contact Name"]].Text,
                        City = worksheet.Cells[i, headers["City"]].Text,
                        State = worksheet.Cells[i, headers["State"]].Text,
                        PostalCode = worksheet.Cells[i, headers["Postal Code"]].Text,
                        Country = worksheet.Cells[i, headers["Country"]].Text,
                        Email = worksheet.Cells[i, headers["Email"]].Text,
                        Phone = worksheet.Cells[i, headers["Phone"]].Text,
                        Date = DateTime.TryParse(worksheet.Cells[i, headers["Date"]].Text, out DateTime date) ? date : default,
                        SignatureRequired = bool.TryParse(worksheet.Cells[i, headers["Signature Required"]].Text, out bool signatureRequired) ? signatureRequired : false,
                        SignatureType = worksheet.Cells[i, headers["Signature Type"]].Text,
                        AdultSignature = bool.TryParse(worksheet.Cells[i, headers["Adult Signature"]].Text, out bool adultSignature) ? adultSignature : false,
                        AddressLine1 = worksheet.Cells[i, headers["Address Line 1"]].Text
                    };

                    records.Add(record);
                }
            }

            return records;
        }

        private async Task SendMessagesToServiceBus(List<CustomerOrderDTO> orderDtos)
        {
            await using var client = new ServiceBusClient(serviceBusConnectionString);
            var sender = client.CreateSender(queueName);

            var messages = new List<ServiceBusMessage>();

            foreach (var orderDto in orderDtos)
            {
                string messageBody = JsonSerializer.Serialize(orderDto);
                var message = new ServiceBusMessage(messageBody)
                {
                    ContentType = "application/json",
                    MessageId = Guid.NewGuid().ToString()
                };

                messages.Add(message);
            }

            try
            {
                await sender.SendMessagesAsync(messages);
                _logger.LogInformation($"Sent {messages.Count} messages to Service Bus queue: {queueName}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error sending messages to Service Bus: {ex.Message}");
            }
        }

        private List<AmerishipOrder> ConvertOrders(List<BetterTrucksOrder> sourceList)
        {
            // Map the list of SourceModel to a list of DestinationModel
            var destinationList = _mapper.Map<List<AmerishipOrder>>(sourceList);

            return destinationList;
        }

        /// <summary>
        /// Validates that the worksheet contains the expected columns.
        /// </summary>
        /// <param name="worksheet">The worksheet to validate.</param>
        private static void ValidateColumns(ExcelWorksheet worksheet)
        {
            // Check header names (Row 1)
            if (worksheet.Cells[1, 1].Text != "Column1" ||
                worksheet.Cells[1, 2].Text != "Column2" ||
                worksheet.Cells[1, 3].Text != "Column3")
            {
                throw new Exception("The Excel file does not have the required columns: Column1, Column2, Column3.");
            }
        }
    }
}
