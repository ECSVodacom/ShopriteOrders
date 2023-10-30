using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopriteBizlinkSender
{
    public class DataHandler
    {
        public DataTable GetVendorOrdersToSend()
        {
            try
            {


                using (SqlConnection connection = new SqlConnection(Properties.Settings.Default.ConnectionString))
                {
                    connection.Open();
                    SqlDataAdapter dt = new SqlDataAdapter("[GetOrdersToPost]", connection);
                    DataSet ds = new DataSet();
                    dt.Fill(ds);
                    return ds.Tables[0];
                }

            }
            catch (Exception ex)
            {
                
                throw ex;
            }
        }

        public DataTable GetVendorClaimsToSend()
        {
            try
            {


                using (SqlConnection connection = new SqlConnection(Properties.Settings.Default.ConnectionString))
                {
                    connection.Open();
                    SqlDataAdapter dt = new SqlDataAdapter("[GetClaimsToPost]", connection);
                    DataSet ds = new DataSet();
                    dt.Fill(ds);
                    return ds.Tables[0];
                }   

            }
            catch (Exception ex)
            {
                
                throw ex;
            }
        }

        public string UpdateOrder(string ID)
        {

            try
            {
                using (SqlConnection connection = new SqlConnection(Properties.Settings.Default.ConnectionString))
                {
                    connection.Open();
                    using (SqlCommand cmd = new SqlCommand("UpdateOrder", connection))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("@LineID", SqlDbType.Int).Value = ID;
                        cmd.ExecuteNonQuery();
                    }
                }

                return "Order Updated";
            }
            catch (Exception ex)
            {
                
                throw ex;
            }
        }

        public string UpdateClaim(string ID)
        {

            try
            {
                using (SqlConnection connection = new SqlConnection(Properties.Settings.Default.ConnectionString))
                {
                    connection.Open();
                    using (SqlCommand cmd = new SqlCommand("UpdateClaim", connection))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("@LineID", SqlDbType.Int).Value = ID;
                        cmd.ExecuteNonQuery();
                    }
                }

                return "Claim Updated";
            }
            catch (Exception ex)
            {
                writeLog("BizlinkSender : " + ex.ToString(),ID.ToString());
                //throw ex;
              
            }
            return "With Errors";
        }

        public void writeLog(String ErrorMessage, String Vendorid)
        {

            using (SqlConnection connection = new SqlConnection(Properties.Settings.Default.ConnectionString))
            {
                connection.Open();
                using (SqlCommand cmd = new SqlCommand("INSERT INTO ErrorLog(ErrorMessage, VendorID) VALUES('CLAIMS - " + 
                        ErrorMessage + "','" + 
                        Vendorid + 
                        "')", connection))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.ExecuteNonQuery();
                }

            }
        }
    }
}
