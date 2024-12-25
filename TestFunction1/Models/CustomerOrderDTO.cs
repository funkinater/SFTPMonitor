using Microsoft.AspNetCore.Authentication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestFunction1.Models
{
    public class CustomerOrderDTO
    {
        public CustomerSettings Settings { get; set; }
        public AmerishipOrder Order { get; set; }
    }
}
