using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Threading;
using System.Xml.Linq;

namespace ShopriteOrders
{
    class Program
    {

        static void Main(string[] args)
        {

            PerformDownload();

        }

        static void PerformDownload()
        {

            DataHandler dataHandler = new DataHandler();
            if (Properties.Settings.Default.Enabled == true)
            {
                Console.WriteLine("Trying to get the customer list");


                Parallel.ForEach(dataHandler.GetVendorOrders().AsEnumerable(), row =>
                {
                    Console.WriteLine(row["VendorName"].ToString());
                    string URL = row["baseUrl"].ToString();
                    bool postToAcknowledge = (bool)row["PostToAcknowledge"] == true;
                    OrderHandler orderHandler = new OrderHandler();
                    Console.WriteLine(orderHandler.DownloadOrder(row["ID"].ToString(), row["VendorUsername"].ToString(), row["VendorPassWord"].ToString(), URL, postToAcknowledge));
                });
            }
            else
            {
                Console.WriteLine("Process Disabled");

            }
            Console.WriteLine("Last run : " + DateTime.Now.ToLongTimeString());
            //Thread.Sleep(5000);
            //PerformDownload();
            //Console.ReadLine();

        }

    }
}
