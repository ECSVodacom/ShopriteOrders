using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;
using System.Xml.Linq;

namespace Shoprite.Debug
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
                    SqlDataAdapter dt = new SqlDataAdapter("GetVendorOrdersToDowload", connection);
                    DataSet ds = new DataSet();
                    dt.Fill(ds);
                    return ds.Tables[0];
                }

            }
            catch (Exception ex)
            {
                writeLog("Could not load Vendores", "0");
                throw ex;
            }
        }

        public void writeLog(String ErrorMessage, String Vendorid)
        {

            using (SqlConnection connection = new SqlConnection(Properties.Settings.Default.ConnectionString))
            {
                connection.Open();
                using (SqlCommand cmd = new SqlCommand("INSERT INTO ErrorLog(ErrorMessage, VendorID) VALUES('"
                    + ErrorMessage.Replace("'","''") + "','" + Vendorid + "')", connection))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.ExecuteNonQuery();
                }

            }
        }

        internal List<OrderResend> GetOrderBatches()
        {
            List<OrderResend> resends = new List<OrderResend>();

            using (SqlConnection connection = new SqlConnection(Properties.Settings.Default.ConnectionString))
            {
                connection.Open();
                using (SqlCommand cmd = new SqlCommand("SELECT * FROM OrderBatch  OB " +
                    "INNER JOIN Vendors V ON OB.VendorID = V.Id WHERE OB.Id IN (461)", connection))
                {

                    OrderResend resend;
                    cmd.CommandType = CommandType.Text;
                    var reader = cmd.ExecuteReader();
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            resend = new OrderResend
                            {
                                OriginalXml = reader["OriginalOrderXml"].ToString(),
                                VendorId = (int)reader["VendorID"],
                                BatchId = (int)reader["Id"],
                                VendorName = (string)reader["VendorUserName"],
                                VendorPassword = (string)reader["VendorPassWord"]


                            };
                            Console.WriteLine(String.Format("{0}", reader[0]));
                            resends.Add(resend);
                        }
                    }
                }

                return resends;

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
                            //cmd.ExecuteNonQuery();
                            return cmd.ExecuteScalar().ToString();
                        }
                    }
                }
                else
                {
                    return "0";
                }
                //return "Success";
            }


            catch (Exception ex)
            {
                writeLog("Could not insert file " + ex.ToString(), Customerid);
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


        public string InsertOrder(string OrderNumber, XElement xml, String Vendor, int BatchID)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(Properties.Settings.Default.ConnectionString))
                {
                    connection.Open();
                    using (SqlCommand cmd = new SqlCommand("AddOrder", connection))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        cmd.Parameters.Add("@OrderNumber", SqlDbType.Text).Value = OrderNumber;
                        cmd.Parameters.Add("@XmlFile", SqlDbType.Xml).Value = xml.ToString();
                        cmd.Parameters.Add("@Vendor", SqlDbType.Text).Value = Vendor;
                        cmd.Parameters.Add("@BatchID", SqlDbType.Int).Value = BatchID;
                        cmd.ExecuteNonQuery();
                    }
                }

            }
            catch (Exception ex)
            {
                writeLog("Could not insert order " + ex.ToString(), Vendor);
                throw ex;
            }

            return "Order Saved";

        }

        public DataTable GetVendorClaims()
        {
            try
            {


                using (SqlConnection connection = new SqlConnection(Properties.Settings.Default.ConnectionString))
                {
                    connection.Open();
                    SqlDataAdapter dt = new SqlDataAdapter("GetVendorClaimsToDowload", connection);
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



        public String InsertDownloadedClaim(string Customerid, XDocument xml, string Claims, String sender, String receiver)
        {
            try
            {
                if (CheckClaims(Customerid, Claims) == false)
                {
                    using (SqlConnection connection = new SqlConnection(Properties.Settings.Default.ConnectionString))
                    {
                        connection.Open();
                        using (SqlCommand cmd = new SqlCommand("AddClaimBatch", connection))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;

                            cmd.Parameters.Add("@VendorID", SqlDbType.Int).Value = Convert.ToInt16(Customerid);
                            cmd.Parameters.Add("@Claims", SqlDbType.VarChar).Value = Claims;
                            cmd.Parameters.Add("@XmlFile", SqlDbType.Xml).Value = xml.ToString();
                            cmd.Parameters.Add("@Sender", SqlDbType.VarChar).Value = sender;
                            cmd.Parameters.Add("@Receiver", SqlDbType.VarChar).Value = receiver;
                            return cmd.ExecuteScalar().ToString();
                        }
                    }
                }
            }


            catch (Exception ex)
            {
                writeLog("CLAIMS INSERT BATCH " + ex.ToString(), Customerid);
                writeLog(ex.ToString(), Customerid);
                throw ex;
            }
            return "0";

        }

        public string InsertClaimDocument(string ClaimNumber, XElement xml, String VendorID, String BatchID, string SenderGLN, String ReceiverGLN, String BranchGLN)
        {
            try
            {

                using (SqlConnection connection = new SqlConnection(Properties.Settings.Default.ConnectionString))
                {
                    connection.Open();
                    using (SqlCommand cmd = new SqlCommand("AddClaim", connection))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        cmd.Parameters.Add("@ClaimNumber", SqlDbType.Int).Value = ClaimNumber;
                        cmd.Parameters.Add("@XmlFile", SqlDbType.Xml).Value = xml.ToString();
                        cmd.Parameters.Add("@VendorID", SqlDbType.VarChar).Value = VendorID;
                        cmd.Parameters.Add("@BatchID", SqlDbType.Int).Value = BatchID;
                        cmd.Parameters.Add("@Sender", SqlDbType.VarChar).Value = SenderGLN;
                        cmd.Parameters.Add("@Receiver", SqlDbType.VarChar).Value = ReceiverGLN;
                        cmd.Parameters.Add("@Branch", SqlDbType.VarChar).Value = BranchGLN;
                        cmd.ExecuteNonQuery();
                    }
                }
            }


            catch (Exception ex)
            {
                writeLog("SINGLE CLAIMS " + ex.ToString(), VendorID);
                throw ex;
            }
            return "Claims Saved";

        }

        public bool CheckClaims(string CustomerID, string Claims)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(Properties.Settings.Default.ConnectionString))
                {
                    connection.Open();
                    SqlDataAdapter dt = new SqlDataAdapter("Select Count(*) as records from ClaimBatch where Claims = '" + Claims + "'", connection);
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
                writeLog("CHECK CLAIMS " + ex.ToString(), CustomerID);
                return false;
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
