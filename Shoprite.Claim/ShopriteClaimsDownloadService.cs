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

namespace ShopriteClaimsDownloadService
{
    public partial class ShopriteClaimsDownloadService : ServiceBase
    {
        public ShopriteClaimsDownloadService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {

            var worker = new Thread(PerformDownload);
            worker.Name = "ClaimsDownloader";
            worker.IsBackground = false;
            worker.Start();

        }

        protected override void OnStop()
        {
        }

        protected void PerformDownload()
        {
            try
            {
                DataHandler dataHandler = new DataHandler();
                if (Properties.Settings.Default.Enabled == true)
                {

                    Parallel.ForEach(dataHandler.GetVendorClaims().AsEnumerable(), row =>
                    {
                        string URL = row["baseUrl"].ToString();
                        ClaimHandler claimHandler = new ClaimHandler();
                        claimHandler.DownloadClaims(row["ID"].ToString(), row["VendorUsername"].ToString(), row["VendorPassWord"].ToString(), URL);
                    });
                    Thread.Sleep(200000);
                }
                else
                {
                    Thread.Sleep(200000);
                }
            }
            catch (Exception ex)
            {
                
                throw ex;
            }

            PerformDownload();

        }
    }
}
