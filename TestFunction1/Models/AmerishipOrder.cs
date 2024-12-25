using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestFunction1.Models
{

    public class AmerishipOrder
    {
        public string trackingNumber { get; set; }
        public string requestedBy { get; set; }
        public string description { get; set; }
        public string comments { get; set; }
        public bool collectionCODRequired { get; set; }
        public bool collectionSignatureRequired { get; set; }
        public float? height { get; set; }
        public float? width { get; set; }
        public float? length { get; set; }
        public string routeName { get; set; }
        public string collectionContactName { get; set; }
        public float? weight { get; set; }
        public int quantity { get; set; }
        public string priceSet { get; set; }
        public bool deliverySignatureRequired { get; set; }
        public Location? collectionLocation { get; set; }
        public Location? deliveryLocation { get; set; }
        public int declaredValue { get; set; }
        public string signatureType { get; set; }
        public bool? isAdultSignature { get; set; }
    }

    public class Location
    {
        public string contactName { get; set; }
        public string companyName { get; set; }
        public string addressLine1 { get; set; }
        public string addressLine2 { get; set; }
        public string city { get; set; }
        public string state { get; set; }
        public string postalCode { get; set; }
        public string country { get; set; }
        public string comments { get; set; }
        public string email { get; set; }
        public string phone { get; set; }
        public string category { get; set; }
    }

}
