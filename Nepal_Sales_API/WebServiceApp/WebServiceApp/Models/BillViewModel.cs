using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebServiceApp.Models
{
    public class BillViewModel
    {
        public string username { get; set; }
        public string password { get; set; }
        public string seller_pan { get; set; }
        public string buyer_pan { get; set; }
        public string fiscal_year { get; set; }
        public string buyer_name { get; set; }
        public string ref_invoice_number { get; set; }
        public string credit_note_number { get; set; }
        public string credit_note_date { get; set; }
        public string reason_for_return { get; set; }
        public Decimal total_sales { get; set; }
        public Decimal taxable_sales_vat { get; set; }
        public Decimal vat { get; set; }
        public Decimal excisable_amount { get; set; }
        public Decimal excise { get; set; }
        public Int32 taxable_sales_hst { get; set; }
        public Int32 hst { get; set; }
        public Int32 amount_for_esf { get; set; }
        public Int32 esf { get; set; }
        public Int32 export_sales { get; set; }
        public Decimal tax_exempted_sales { get; set; }
        public bool isrealtime { get; set; }
        public DateTime datetimeclient { get; set; }

        public string invoice_number { get; set; }
        public string  invoice_date { get; set; }

    }
}
