using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace ShopriteOrderDownloadService
{
    public class OrderHandler
    {
        RestHandler restHandler = new RestHandler();
        DataHandler dataHandler = new DataHandler();

        /// <summary>
        /// Downloads the order, splits the file into seperate orders for customers and saves the data to the database
        /// </summary>
        /// <param name="VendorID">The Vendor ID in the database</param>
        /// <param name="VendorName">Vendor name used to log on to the service</param>
        /// <param name="Password">Password used when invoking the service</param>
        /// <param name="url">This is the base URL for the service</param>
        /// <returns>Return informational message string</returns>
        public string DownloadOrder(string VendorID, string VendorName, string Password, string url)
        {
            bool NoMoreOrders = false;
            string Response = "";
            try
            {
                do
                {
                    dataHandler.UpdateLastCheckDate(VendorID);
                    Response = restHandler.GetOrders(url, VendorName, Password);
                    if (Response.StartsWith("<string>No downloadable Orders")) 
                    { 
                        if(Properties.Settings.Default.LogResponse)
                        {
                            dataHandler.writeLog(Response, VendorID); 
                        }
                       
                        return "No Orders To Download"; 
                    }
                    if (Response.Contains("fault")) { dataHandler.writeLog(Response, VendorID); return ""; }
                    // Console.WriteLine("Succesfully called site");

                    
                
                    XDocument doc = XDocument.Load(new System.IO.MemoryStream(System.Text.Encoding.UTF8.GetBytes(Response)));


                    foreach (XElement Xelement in doc.Root.DescendantsAndSelf())
                    {
                        // Stripping the namespace by setting the name of the element to it's localname only
                        Xelement.Name = Xelement.Name.LocalName;
                        // replacing all attributes with attributes that are not namespaces and their names are set to only the localname
                        Xelement.ReplaceAttributes((from xattrib in Xelement.Attributes().Where(xa => !xa.IsNamespaceDeclaration) select new XAttribute(xattrib.Name.LocalName, xattrib.Value)));
                    }

                    string orders = doc.Element("orderMessage")
                        .Element("StandardBusinessDocumentHeader")
                        .Element("DocumentIdentification")
                        .Element("InstanceIdentifier").Value.ToString();
                    Console.WriteLine("Downloaded orders " + orders);

                    string Sender = doc.Descendants("Sender").Elements("Identifier").FirstOrDefault().Value;
                    string Receiver = doc.Descendants("Receiver").Elements("Identifier").FirstOrDefault().Value;
                    int BatchID = 0;
                    try
                    {
                        BatchID = Convert.ToInt32(dataHandler.InsertDownloadedFile(VendorID, doc, orders));
                        restHandler.AcknowledgeOrders("[" + orders + "]", url + "?action=acknowledge&vendorId=" + VendorName, VendorName, Password);
                    }
                    catch (Exception exBatchID)
                    {

                        NoMoreOrders = true;
                        dataHandler.writeLog(exBatchID.ToString(), VendorID);
                    }
                    
                   

                    //if (dataHandler.InsertDownloadedFile(VendorID, doc, orders) != "Orders Allready Downloaded")
                    if (BatchID != 0)
                    {
                        if (Sender == "0000000000000" || Receiver == "0000000000000")
                        {
                            //throw new Exception("Receiver GLN is incorrect for orders " + orders + " and vendor " + VendorID);
                            dataHandler.writeLog("Receiver GLN is incorrect for orders " + orders + " and vendor " + VendorID, VendorID);
                        }

                        //// Get All the distinct customers from the XML Document
                        var customers = GetCustomers(doc);

                        ////Loop through all the customers and get the orders for that customer from the document
                        foreach (var customer in customers)
                        {
                            var customerOrder = GetOdersPerCustomer(doc, customer);

                            foreach (var order in customerOrder)
                            {
                                SplitOrders(customer, order, doc, BatchID);
                            }
                        }
                    }
                   


                } while (NoMoreOrders == false);


            }
            catch (Exception ex)
            {
                dataHandler.writeLog("Vendor : " + VendorName + ex.Message + Environment.NewLine + ex.InnerException + Environment.NewLine + ex.StackTrace , VendorID);
                dataHandler.writeLog("Response Message : " + Response, VendorID);
                //Console.WriteLine(ex.ToString());
                NoMoreOrders = true;
            }
            return "Orders Successfully downloaded";

        }

        /// <summary>
        /// Gets all the customers in the order
        /// </summary>
        /// <param name="xdoc">The document</param>
        /// <returns>Returns a list of customers</returns>
        public List<String> GetCustomers(XDocument xdoc)
        {
            var customers = xdoc.Descendants("shipTo")
                                  .GroupBy(c => c.Element("gln").ToString())
                                  .Select(g => g.Elements("gln").Last().Value).ToList();
            return customers;
        }

        /// <summary>
        /// This gets all the orders per customer
        /// </summary>
        /// <param name="xdoc">The xml Document</param>
        /// <param name="CustomerNumber">The customer number</param>
        /// <returns>Returns a list of orders</returns>
        public List<string> GetOdersPerCustomer(XDocument xdoc, String CustomerNumber)
        {
            var orders = xdoc.Descendants("shipTo")
                .Where(i => i.Element("gln").Value == CustomerNumber)
                .Select(i => i.Parent.Parent.Parent.Parent.Elements("orderIdentification").Elements("entityIdentification").First().Value).Distinct().ToList();

            return orders;
        }


        /// <summary>
        /// This will split the order file into seperate orders for a customer
        /// </summary>
        /// <param name="Customer">The Customer to generate the order for</param>
        /// <param name="Order">Order number</param>
        /// <param name="xdoc">The xml File containing the orders</param>
        /// <returns></returns>
        public XElement SplitOrders(String Customer, String Order, XDocument xdoc, int BatchID)
        {

            XElement Sender = xdoc.Descendants("Sender").FirstOrDefault();
            XElement Receiver = xdoc.Descendants("Receiver").FirstOrDefault();
            XElement OrderNumber = new XElement("OrderNumber", Order);
            XElement OrderDate = new XElement("orderDate");
            XElement OrderType = new XElement("OrderType");
            XElement buyer = new XElement("buyer");
            XElement seller = new XElement("seller");
            XElement Store = new XElement("Store");
            XElement deliveryDate = new XElement("deliveryDate");

            XElement orderLines = new XElement("orderLines");

            int i = 0;

            //Get the specific order
            foreach (var ord in xdoc.Descendants("orderMessage").Elements("order"))
            {
                try
                {


                    if (ord.Element("orderIdentification").Element("entityIdentification").Value == Order)
                    {
                        OrderType.Value = ord.Element("orderTypeCode").Attribute("codeListVersion").Value;
                        buyer = ord.Element("buyer");
                        seller = ord.Element("seller");
                        OrderDate.Value = ord.Element("creationDateTime").Value;


                        foreach (var line in ord.Descendants("orderLineItem"))
                        {
                            foreach (var lineItem in line.Descendants("orderLineItemDetail"))
                            {
                                if (lineItem.Element("orderLogisticalInformation").Element("shipTo").Element("gln").Value == Customer)
                                {
                                    Store = lineItem.Element("orderLogisticalInformation").Element("shipTo");
                                    deliveryDate.Value = lineItem.Element("orderLogisticalInformation").Element("orderLogisticalDateInformation").Element("requestedDeliveryDateTime").Element("date").Value;
                                    
                                     XElement lineCostInc = null;
                                    try
                                    {
                                        lineCostInc = new XElement("lineCostIncludingTax", (Convert.ToDecimal(lineItem.Element("requestedQuantity").Value) * Convert.ToDecimal(line.Element("monetaryAmountIncludingTaxes").Value)).ToString());
                                    }
                                    catch (Exception)
                                    {

                                        //throw;
                                    }
                                    XElement lineCostExcl = null;
                                    try
                                    {
                                        lineCostExcl = new XElement("lineCostExcludingTax", Convert.ToDecimal(lineItem.Element("requestedQuantity").Value) * Convert.ToDecimal(line.Element("monetaryAmountExcludingTaxes").Value));

                                    }
                                    catch (Exception)
                                    {

                                       // throw;
                                    }

                                    i += 1;
                                    XElement orderLine = new XElement("OrderLine");
                                    orderLines.Add(orderLine);
                                    orderLine.Add(new XElement("lineItem", i));
                                    orderLine.Add(line.Element("netAmount"));
                                    orderLine.Add(line.Element("netPrice"));
                                    orderLine.Add(line.Element("monetaryAmountExcludingTaxes"));
                                    orderLine.Add(line.Element("monetaryAmountIncludingTaxes"));
                                    orderLine.Add(lineItem.Element("requestedQuantity"));
                                    orderLine.Add(line.Element("additionalOrderLineInstruction"));
                                    if (lineCostInc != null) { orderLine.Add(lineCostInc); };
                                    if(lineCostExcl != null){orderLine.Add(lineCostExcl);}
                                    orderLine.Add(line.Element("transactionalTradeItem"));
                                    orderLine.Add(line.Element("promotionalDeal"));
                                    orderLine.Add(line.Element("avpList"));
                                    orderLine.Add(lineItem.Element("avpList"));

                                }
                            }
                        }

                    }
                }
                catch (Exception OEx)
                {

                    dataHandler.writeLog(OEx.Message + " for order " + OrderNumber, "001");
                }
            }


            if (i == 0)
            {
                return null;
            }


            XElement OrderXML = new XElement("Order",
                Sender,
                Receiver,
                OrderNumber,
                OrderDate,
                OrderType,
                buyer,
                seller,
                Store,
                deliveryDate,
                orderLines);

            dataHandler.InsertOrder(Order, OrderXML, Customer, BatchID);
            //Console.WriteLine("Saved order " + Order + " to the database");
            return OrderXML;

        }

    }
}
