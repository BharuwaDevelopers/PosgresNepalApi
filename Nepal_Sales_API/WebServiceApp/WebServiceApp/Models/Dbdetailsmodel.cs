using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebServiceApp.Models
{
    public class Dbdetailsmodel
    {
        public string unitcode { get; set; }
        public string dbname { get; set; }
        public string dbuser { get; set; }
        public string dbpassword { get; set; }
        public string serverip { get; set; }
    }
}
