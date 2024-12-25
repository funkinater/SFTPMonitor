using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace TestFunction1.Models
{
    public class CustomerSettings
    {
        [JsonPropertyName("apiKey")]
        public string ApiKey { get; set; }
        [JsonPropertyName("priceSet")]
        public string PriceSet { get; set; }
        [JsonPropertyName("location")]
        public Location Location { get; set; }
    }
}
