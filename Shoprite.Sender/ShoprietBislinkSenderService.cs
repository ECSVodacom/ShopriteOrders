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

namespace ShopriteBizlinkSender
{
    public partial class ShoprietBislinkSenderService : ServiceBase
    {
        public ShoprietBislinkSenderService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            var worker = new Thread(ProcessOrders);
            worker.Name = "Bizlink";
            worker.IsBackground = false;
            worker.Start();
        }

        protected override void OnStop()
        {
        }

        static void ProcessOrders()
        {

            DataHandler dataHandler = new DataHandler();
            BizHandler bizHandler = new BizHandler();
            try
            {
                Parallel.ForEach(dataHandler.GetVendorOrdersToSend().AsEnumerable(), row =>
                {
                    bizHandler.PostToBiz(row["OrderFile"].ToString(), row["Vendor"].ToString() + "-" + row["OrderNumber"].ToString() + "-" + DateTime.Now.Ticks + ".xml", Properties.Settings.Default.Orders, row["baseUrl"].ToString());
                    dataHandler.UpdateOrder(row["id"].ToString());
                });
                Thread.Sleep(5000);

                Parallel.ForEach(dataHandler.GetVendorClaimsToSend().AsEnumerable(), row =>
                {
                    bizHandler.PostToBiz(row["Claim"].ToString(), row["SenderGLN"].ToString() + "-" + row["ReceiverGLN"].ToString() + "-" + row["ClaimNumber"].ToString() + DateTime.Now.Ticks + ".xml", Properties.Settings.Default.Claims, row["baseUrl"].ToString());
                    dataHandler.UpdateClaim(row["id"].ToString());
                });
                Thread.Sleep(5000);
            }
            catch (Exception ex)
            {
                dataHandler.writeLog("BizlinkSender : " + ex.ToString(), "0");
                throw ex;
            }
            finally
            { ProcessOrders(); }





        }
    }
}
