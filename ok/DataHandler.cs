using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;
using System.Xml.Linq;

namespace ShopriteOrders
{
    public class DataHandler
    {
        public DataTable GetVendorOrders()
        {
            try
            {


                using (SqlConnection connection = new SqlConnection(Properties.Settings.Default.ConnectionString))
                {
                    connection.Open();
                    SqlDataAdapter dt = new SqlDataAdapter("GetVendorOrdersToDowloadPost", connection);
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

        public void UpdateLastCheckDate(string VendorID)
        {

            using (SqlConnection connection = new SqlConnection(Properties.Settings.Default.ConnectionString))
            {
                connection.Open();
                using (SqlCommand cmd = new SqlCommand("Update Vendors set LastActionDate = '" + DateTime.Now.ToString() + "' where id = '" + VendorID + "'", connection))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.ExecuteNonQuery();
                }

            }
        }

        public string InsertDownloadedFile(string Customerid, XDocument xml, string Orders)
        {
            try
            {
                if (CheckOrders(Customerid, Orders) == false)
                {
                    using (SqlConnection connection = new SqlConnection(Properties.Settings.Default.ConnectionString))
                    {
                        connection.Open();
                        using (SqlCommand cmd = new SqlCommand("AddOrderBatch", connection))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;

                            cmd.Parameters.Add("@VendorID", SqlDbType.Int).Value = Customerid;
                            cmd.Parameters.Add("@Orders", SqlDbType.VarChar).Value = Orders;
                            cmd.Parameters.Add("@XmlFile", SqlDbType.Xml).Value = xml.ToString();
                            return cmd.ExecuteScalar().ToString();
                        }
                    }
                }
                else
                {
                    return "0";
                }
            }


            catch (Exception ex)
            {

                throw ex;
            }
        }

        public bool CheckOrders(string CustomerID, string Orders)
        {
            try
            {


                using (SqlConnection connection = new SqlConnection(Properties.Settings.Default.ConnectionString))
                {
                    connection.Open();
                    SqlDataAdapter dt = new SqlDataAdapter("Select Count(*) as records from OrderBatch where Vendorid = " + CustomerID + " and Orders = '" + Orders + "'", connection);
                    DataSet ds = new DataSet();
                    dt.Fill(ds);
                    if (ds.Tables[0].Rows[0]["Records"].ToString() == "1")
                    {
                        return true;
                    }
                    return false;
                }

            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public string InsertOrder(string OrderNumber, XElement xml, String Vendor, string batchId, string additionalOrderLineInstruction)
        {
            try
            {


                using (SqlConnection connection = new SqlConnection(Properties.Settings.Default.ConnectionString))
                {
                    connection.Open();
                    using (SqlCommand cmd = new SqlCommand("AddOrder", connection))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("@BatchId", SqlDbType.Int).Value = batchId;
                        cmd.Parameters.Add("@OrderNumber", SqlDbType.Text).Value = OrderNumber;
                        cmd.Parameters.Add("@XmlFile", SqlDbType.Xml).Value = xml.ToString();
                        cmd.Parameters.Add("@Vendor", SqlDbType.Text).Value = Vendor;
                        cmd.Parameters.Add("@AdditionalOrderLineInstruction", SqlDbType.Text).Value = additionalOrderLineInstruction;
                        cmd.ExecuteNonQuery();
                    }
                }

            }
            catch (Exception ex)
            {

                throw ex;
            }

            return "Order Saved";

        }
    }
}
