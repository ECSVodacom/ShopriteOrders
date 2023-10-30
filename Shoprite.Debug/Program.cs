using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace Shoprite.Debug
{
    [XmlRootAttribute("", Namespace = "", IsNullable = false)]
    public class ArrayOfstring
    {
        [XmlArrayAttribute("string")]
        public List<string> Items = new List<string>();

        public void Add(string id)
        {
            Items.Add(id);
        }
    }

    static class Program
    {


        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {


            //DataHandler dataHandler = new DataHandler();
            ////XDocument doc = XDocument.Load(new System.IO.MemoryStream(System.Text.Encoding.UTF8.GetBytes("{DSDS")));

            ////Console.WriteLine(row["VendorName"].ToString());
            string URL = "https://externalservices.shopriteholdings.co.za/b2bservice/api/VendorOrder";
            //string claimUrl = "https://externalservices.shopriteholdings.co.za/b2bservice/api/VendorClaim";

            ////RestHandler handler = new RestHandler();
            ////handler.ResetOrders("[1071755824]", URL, "danone1", "Welcome@123");

            ////ClaimHandler handler = new ClaimHandler();
            ////handler.DownloadClaims("85", "Namibia650764QA1", "Welcome@123", claimUrl);

            OrderHandler orderHandler = new OrderHandler();

            orderHandler.DownloadOrder("1110", "vodacomdownload", "Welcome@123", URL);

            ////orderHandler.DownloadOrder("1105", "ParmalatQAediOKFD", "Welcome@123", URL);

            ////orderHandler.DownloadOrder("1105", "NBNamibiaQA", "Welcome@123", URL);
            ////orderHandler.DownloadOrder("41", "PioneerQA686620", "Welcome@123", URL);
            ////orderHandler.DownloadOrder("1", "CHCH1249", "er@@20X3", URL);
        }
    }
}
