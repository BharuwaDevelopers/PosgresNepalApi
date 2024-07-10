using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using WebServiceApp.Models;
using ExcelDataReader;
using Microsoft.AspNetCore.Authorization;
using System.IO;
using Oracle.ManagedDataAccess.Client;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Data;
using System.Net.Http.Json;

namespace WebServiceApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }
        //[AllowAnonymous]
        public IActionResult Index(String unitCode, String identifier,String empCode)
        {
           // unitCode = "10001"; identifier = "7980-001-0000920";  empCode = "E-001";

            Console.WriteLine("unitcode--" + unitCode);

            if (string.IsNullOrEmpty(unitCode)|| string.IsNullOrEmpty(identifier) || string.IsNullOrEmpty(empCode))
            {
                ViewBag.Processed = "Parameter provided should not be null or Empty";
                return View();
            }
           String unitCd = unitCode.Substring(0, 3);
            string dbname = "", Pwd = "", DbUser = "", dbServer = "";
            List<Dbdetailsmodel> dbdtl = new List<Dbdetailsmodel>();
            var path = Path.Combine(Directory.GetCurrentDirectory(), "ExcelFile", "dbserverdtl.xlsx");
            if (System.IO.File.Exists(path))
            {
                System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
                using (var stream = System.IO.File.Open(path, FileMode.Open, FileAccess.Read))
                {
                    using (var reader = ExcelReaderFactory.CreateReader(stream))
                    {
                        while (reader.Read()) //Each ROW
                        {
                            dbdtl.Add(new Dbdetailsmodel
                            {
                                unitcode = reader.GetValue(0).ToString(),
                                dbname = reader.GetValue(1).ToString(),
                                dbuser = reader.GetValue(2).ToString(),
                                dbpassword = reader.GetValue(3).ToString(),
                                serverip = reader.GetValue(4).ToString()
                            });
                        }
                    }
                }
               
            }

            var filtereddbdtl = dbdtl.Where(user => user.unitcode == unitCd);
            foreach (Dbdetailsmodel user in filtereddbdtl)
            {
                dbname = user.dbname;
                DbUser = user.dbuser;
                Pwd = user.dbpassword;
                dbServer = user.serverip;

            }
            DataTable dt = null;
           // Npgsql.NpgsqlConnection conn = null;
           // (string userid, string Pwd, string datasource, string dbName);
            OraDBConnection.SetConnectiondata(DbUser,Pwd, dbServer,dbname);
            using (Npgsql.NpgsqlConnection conn  = new Npgsql.NpgsqlConnection(OraDBConnection.OrclConnection))
            {
                conn.Open();
                //call the overload that takes a connection in place of the connection string
                try
                {
                    String queryStr = "SELECT A.EWAY_USERNAME username, A.EWAY_PASSWORD password, A.PAN_NO seller_pan, C.PAN_NO buyer_pan,SUBSTR(E.NAPALI_DATE, -4, 2) || SUBSTR(B.FIN_YEAR, 1, 2) || '.0' || SUBSTR(B.FIN_YEAR, -2) fiscal_year,C.NAME buyer_name, B.IDENTIFIER invoice_number, SUBSTR(E.NAPALI_DATE, -4)|| '.' || LPAD(SUBSTR(E.NAPALI_DATE,INSTR(E.NAPALI_DATE, '/', 1, 1) + 1, INSTR(E.NAPALI_DATE, '/', 1, 2) - INSTR(E.NAPALI_DATE, '/', 1, 1) - 1), 2, '0') || '.' || LPAD(SUBSTR(E.NAPALI_DATE, 1, INSTR(E.NAPALI_DATE, '/', 1, 1) - 1), 2, '0') invoice_date,B.NET_AMOUNT total_sales,B.GROSS_AMOUNT + IGST taxable_sales_vat,B.SGST vat, B.GROSS_AMOUNT excisable_amount,B.IGST excise,0 taxable_sales_hst,0 hst,0 amount_for_esf,0 esf,0 export_sales,(CASE B.SGST + B.IGST WHEN 0 THEN B.NET_AMOUNT ELSE 0 END) tax_exempted_sales,'true' isrealtime,NOW() datetimeClient, B.IDENTIFIER inv_head_identifier, B.CREATED_BY created_by,B.CREATION_DATE creation_date FROM UNIT A JOIN INVOICE_HEADER B ON(A.CODE = B.UNIT_CODE) JOIN CUSTOMER C ON(B.UNIT_CODE = C.UNIT_CODE AND B.CUST_CODE = C.CODE) JOIN FIN_YEAR D ON(D.FIN_YEAR_CODE = B.FIN_YEAR) JOIN NEPALI_CAL E ON(B.DATES = E.ENG_DATE) WHERE B.UNIT_CODE = '"+unitCode+"' AND B.IDENTIFIER = '" + identifier +"'";
                         Npgsql.NpgsqlCommand cmd = new Npgsql.NpgsqlCommand(queryStr, conn);
                        Npgsql.NpgsqlDataAdapter oda = new Npgsql.NpgsqlDataAdapter(cmd);
                       //  OracleDataAdapter oda = new OracleDataAdapter(cmd);
                         dt = new DataTable();
                         oda.Fill(dt);
                         cmd.Dispose();
                }
                catch (Exception ex)
                {
                    ex.StackTrace.ToString();
                }
                finally{
                    conn.Close();
                }

            }

            DateTime currentDateTime = DateTime.Now;
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new
                MediaTypeWithQualityHeaderValue("application/json"));
                if (dt.Rows.Count > 0)
                {
                  //  tableValue(DataTable dt)
                    Decimal tax_exempted_sales = (Decimal)dt.Rows[0]["tax_exempted_sales"];
                    BillViewModel p = new BillViewModel()
                    {
                        username = dt.Rows[0]["username"].ToString(),
                       // username = "Test_CBMS",
                        password = dt.Rows[0]["password"].ToString(),
                        // password = "test@321",
                        seller_pan = dt.Rows[0]["seller_pan"].ToString(),
                        // seller_pan = "999999999",
                        buyer_pan = dt.Rows[0]["buyer_pan"].ToString(),
                        // buyer_pan = "123456789",
                        buyer_name = dt.Rows[0]["buyer_name"].ToString(),
                        // buyer_name = "",
                        fiscal_year = dt.Rows[0]["fiscal_year"].ToString(),
                        // fiscal_year = "2073.074",
                        invoice_number = dt.Rows[0]["invoice_number"].ToString(),
                        // invoice_number = "102",
                        invoice_date = dt.Rows[0]["invoice_date"].ToString(),
                        // invoice_date = "2074.07.06",
                        total_sales = (Decimal)dt.Rows[0]["total_sales"],
                        //total_sales = 1130,
                        taxable_sales_vat = (Decimal)dt.Rows[0]["taxable_sales_vat"],
                        //taxable_sales_vat = 1000,
                        vat = (Decimal)dt.Rows[0]["vat"],
                        //vat = 130,
                        excisable_amount = (Decimal)dt.Rows[0]["excisable_amount"],
                        //excisable_amount = 0,
                        excise = (Decimal)dt.Rows[0]["excise"],
                        // excise = 0,
                        taxable_sales_hst = (Int32)dt.Rows[0]["taxable_sales_hst"],
                        //taxable_sales_hst = 0,
                        hst = (Int32)dt.Rows[0]["hst"],
                        // hst = 0,
                        amount_for_esf = (Int32)dt.Rows[0]["amount_for_esf"],
                        // amount_for_esf = 0,
                        esf = (Int32)dt.Rows[0]["esf"],
                        //esf = 0,
                        export_sales = (Int32)dt.Rows[0]["export_sales"],
                        //  export_sales = 0,
                        tax_exempted_sales = (Decimal)dt.Rows[0]["tax_exempted_sales"],
                       // tax_exempted_sales = 0,
                        isrealtime = true,
                        datetimeclient = currentDateTime
                    };
                    try
                    {
                        client.BaseAddress = new Uri("https://cbapi.ird.gov.np");
                        var response = client.PostAsJsonAsync("api/bill", p).Result;
                        if (response.IsSuccessStatusCode)
                        {
                           // var responseCode = response.Content.ReadAsStringAsync();
                            string statusCode = response.Content.ReadAsStringAsync().Result;
                            var result = response.Content.ReadAsStringAsync();
                            if (result.Result == "200")
                            {
                                UpdateTable(result.Result, identifier, statusCode, empCode);
                                ViewBag.Processed = "Generate invoice successfully";
                                //   Console.Write(result.ToString());//responseCode 200 means successful
                                // Console.ReadLine();
                            }
                            else
                            {
                                UpdateTable(result.Result, identifier, statusCode, empCode);
                                ViewBag.Processed = "Generate invoice Error Code-"+ result.Result;
                                // Console.Write("Error code " + responseCode.Result);
                                // Console.ReadLine();
                            }
                        }
                        else
                        {
                            ViewBag.Processed = "No web page was found for the web address: https://cbapi.ird.gov.np/";
                            return View();
                            // Console.Write("Error");
                            //  Console.ReadLine();
                        }
                    }catch(Exception ex)
                    {
                        ex.StackTrace.ToString();
                        UpdateTable(ex.StackTrace.ToString(), identifier, "505", empCode);
                        ViewBag.Processed = ex.StackTrace.ToString();
                    }
                }
            }
            //ViewBag.Processed = "Einvoice Canceled";
            return View();
        }

        public void UpdateTable(String remarks, String identifier, String statusCode,String empCode)
        {
            int sr = 0;
            //  DateTime currentDateTime = DateTime.Now;
            Npgsql.NpgsqlConnection conn = null;
            using (conn = new Npgsql.NpgsqlConnection(OraDBConnection.OrclConnection.ToString()))
            {
                conn.Open();
                try
                {
                    string sqlstr = "update INVOICE_HEADER set API_REMARKS='" + remarks + "', API_STATUS='" + statusCode + "',API_SENT_BY='" + empCode + "', API_SENT_ON=SYSDATE   where IDENTIFIER='" + identifier + "' ";
                    Npgsql.NpgsqlCommand cmd = new Npgsql.NpgsqlCommand(sqlstr, conn);
                    // cmd.CommandType = CommandType.Text;
                    cmd.ExecuteNonQuery();
                    cmd.Dispose();
                }
                catch (Exception ex)
                {
                    ex.StackTrace.ToString();
                }
                finally
                {
                    conn.Close();
                }
            }
            //int SR = DataLayer.ExecuteNonQuery(OraDBConnection.OrclConnection, CommandType.Text, sqlstr);
            // return sr;
        }

        public void tableValue( DataTable dt)
        {
            String username = dt.Rows[0]["username"].ToString();

            String password = dt.Rows[0]["password"].ToString();

            String seller_pan = dt.Rows[0]["seller_pan"].ToString();

            String buyer_pan = dt.Rows[0]["buyer_pan"].ToString();

            String buyer_name = dt.Rows[0]["buyer_name"].ToString();

            String fiscal_year = dt.Rows[0]["fiscal_year"].ToString();

            String invoice_number = dt.Rows[0]["invoice_number"].ToString();

            String invoice_date = dt.Rows[0]["invoice_date"].ToString();

            double total_sales = (double)dt.Rows[0]["total_sales"];

            Decimal taxable_sales_vat = (Decimal)dt.Rows[0]["taxable_sales_vat"];

            Decimal vat = (Decimal)dt.Rows[0]["vat"];

            double excisable_amount = (double)dt.Rows[0]["excisable_amount"];

            Decimal excise = (Decimal)dt.Rows[0]["excise"];

            Decimal taxable_sales_hst = (Decimal)dt.Rows[0]["taxable_sales_hst"];

            Decimal hst = (Decimal)dt.Rows[0]["hst"];

            Decimal amount_for_esf = (Decimal)dt.Rows[0]["amount_for_esf"];

            Decimal esf = (Decimal)dt.Rows[0]["esf"];

            Decimal export_sales = (Decimal)dt.Rows[0]["export_sales"];
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
