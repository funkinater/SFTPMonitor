using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestFunction1.Models
{
    public class BetterTrucksOrder
    {
        public string TrackingNumber { get; set; }
        public string AddressLine1 { get; set; }
        public string Company { get; set; }
        public string ContactName { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string PostalCode { get; set; }
        public string Country { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public DateTime Date { get; set; }
        public bool SignatureRequired { get; set; }
        public string SignatureType { get; set; }
        public bool AdultSignature { get; set; }
    }
}
