using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ShopriteBizlinkSender
{
    public class BizHandler
    {
        public String PostToBiz(string Data, string FileName, string TradingPartner, string baseUrl)
        {
            string postData = Data;
            string response = string.Empty;

            string url = baseUrl;
            if (string.IsNullOrEmpty(url))
                url = Properties.Settings.Default.BizLinkURL + TradingPartner + "&filename=" + FileName;

            HttpWebRequest req = WebRequest.Create(new Uri(url)) as HttpWebRequest;
            req.AllowWriteStreamBuffering = true;
            req.Method = "POST";
            req.ContentType = "text/xml";

            try
            {



                byte[] formData = UTF8Encoding.UTF8.GetBytes(postData);
                req.ContentLength = formData.Length;

                using (Stream post = req.GetRequestStream())
                {
                    post.Write(formData, 0, formData.Length);
                    post.Close();
                }

            }
            catch (Exception ex)
            {
                //return "Failed";
                throw ex;
                //throw;
            }
            try
            {
                using (HttpWebResponse resp = req.GetResponse() as HttpWebResponse)
                {
                    StreamReader reader = new StreamReader(resp.GetResponseStream());
                    string resContent = string.Empty;
                    resContent = reader.ReadToEnd();
                    response = resContent;
                }
            }
            catch (Exception postEx)
            {

                throw postEx;
            }

            return response;

        }
    }
}
