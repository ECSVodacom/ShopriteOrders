using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;

namespace Shoprite.Debug
{
    public class RestHandler
    {

        public string ResetOrders(string orders, string url, string UserID, string Password)
        {


            HttpClient client = SetClient(UserID, Password, url);
            client.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json; charset=utf-8");

            var result = client.PutAsync(url, new StringContent(orders, Encoding.UTF8, "application/json")).Result;
            string resultContent = result.Content.ReadAsStringAsync().Result;

            return resultContent;

        }


        public string AcknowledgeOrdersPost(string buyerAssignedOrderHeaderIds, string url, string vendorName, string password)
        {
            HttpClient client = SetClient(vendorName, password, url);
            client.DefaultRequestHeaders
                .TryAddWithoutValidation("Content-Type",
                    "application/xml; charset=utf-8");



            //HttpContent content = new StringContent("[" + buyerAssignedOrderHeaderIds + "]");

            var response = client.PostAsync(url, new StringContent(buyerAssignedOrderHeaderIds, Encoding.UTF8, "application/json")).Result;

            string resultContent = response.Content.ReadAsStringAsync().Result;

            return resultContent;
        }


        public string AcknowledgeOrders(string orders, string url, string UserID, string Password)
        {


            HttpClient client = SetClient(UserID, Password, url);
            client.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json; charset=utf-8");

            var result = client.PutAsync(url, new StringContent(orders, Encoding.UTF8, "application/json")).Result;
            string resultContent = result.Content.ReadAsStringAsync().Result;

            return resultContent;

        }

        public string GetOrders(string url, string UserID, string Password)
        {
            try
            {

                HttpClient client = SetClient(UserID, Password, url);
                client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/xml"));

                HttpResponseMessage response = client.GetAsync("").Result;
                var dataObjects = response.Content.ReadAsStringAsync();
                return dataObjects.Result.ToString();

            }
            catch (Exception ex)
            {
                return ex.ToString();
            }

        }

        private HttpClient SetClient(String UserID, String Password, String Url)
        {
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri(Url);


            string sCombinedCredentials = UserID.Trim() + ":" + Password.Trim();
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(sCombinedCredentials);
            string sAuthorization = "Basic " + System.Convert.ToBase64String(plainTextBytes);

            client.DefaultRequestHeaders.Add("Authorization", sAuthorization);
            client.DefaultRequestHeaders.Add("ContractID", "aa659aa2-4175-471f-8c82-59ca416723cf");
            client.DefaultRequestHeaders.Add("UIUser", UserID.Trim());

            client.Timeout = TimeSpan.FromSeconds(300);
            return client;

        }

        public string AcknowledgeClaims(string Claims, string url, string UserID, string Password)
        {


            HttpClient client = SetClient(UserID, Password, url);
            client.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json; charset=utf-8");

            var result = client.PutAsync(url, new StringContent(Claims, Encoding.UTF8, "application/json")).Result;
            string resultContent = result.Content.ReadAsStringAsync().Result;

            return resultContent;

        }

        public string GetClaims(string url, string UserID, string Password)
        {
            try
            {

                HttpClient client = SetClient(UserID, Password, url);
                client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/xml"));

                HttpResponseMessage response = client.GetAsync("").Result;
                var dataObjects = response.Content.ReadAsStringAsync();
                return dataObjects.Result.ToString();

            }
            catch (Exception ex)
            {
                return ex.ToString();
            }

        }


    }
}
