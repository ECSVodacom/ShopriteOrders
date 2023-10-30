using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace ShopriteOrderDownloadService
{
    public partial class ShopriteDownloadService : ServiceBase
    {
        public ShopriteDownloadService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {

            var worker = new Thread(PerformDownload);
            worker.Name = "Downloader";
            worker.IsBackground = false;
            worker.Start();

        }

        protected void PerformDownload()
        {

            DataHandler dataHandler = new DataHandler();
            var exceptions = new ConcurrentQueue<Exception>();
            if (Properties.Settings.Default.Enabled == true)
            {
                //Console.WriteLine("Trying to get the customer list");
                Parallel.ForEach(dataHandler.GetVendorOrders().AsEnumerable(), row =>
                {
                    try
                    {
                        //Console.WriteLine(row["VendorName"].ToString());
                        string URL = row["baseUrl"].ToString();
                        OrderHandler orderHandler = new OrderHandler();
                        orderHandler.DownloadOrder(row["ID"].ToString(), row["VendorUsername"].ToString(), row["VendorPassWord"].ToString(), URL);
                    }
                    catch (Exception e)
                    {
                        exceptions.Enqueue(e);
                        dataHandler.writeLog(e.Message, row["ID"].ToString());
                       // throw;
                    }

                });

                if (exceptions.Count > 0) { dataHandler.writeLog("Errors occurred " + exceptions.FirstOrDefault().ToString(), "00"); };

                Thread.Sleep(600000);
            }
            else { //Console.WriteLine("Process Disabled");
                
            }
            ///Console.WriteLine("Last run : " + DateTime.Now.ToLongTimeString());
            PerformDownload();
            //Console.ReadLine();

        }
        protected override void OnStop()
        {
        }
    }
}
