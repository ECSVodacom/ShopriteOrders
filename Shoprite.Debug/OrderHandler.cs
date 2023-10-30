using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Shoprite.Debug
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
            string Response = "";
            try
            {
                var postToAcknowledge = true;

                dataHandler.UpdateLastCheckDate(VendorID);
                Response = restHandler.GetOrders(url, VendorName, Password);

                List<OrderResend> resends = dataHandler.GetOrderBatches();
                foreach (var send in resends)
                {
                    VendorID = send.VendorId.ToString();
                    VendorName = send.VendorName;
                    int BatchID = send.BatchId;

                    XDocument xDocument = XDocument.Load(new System.IO.MemoryStream(Encoding.UTF8.GetBytes(send.OriginalXml)));

                    var batchId = BatchID.ToString();//dataHandler.InsertDownloadedFile(VendorID, doc, orders);
                    IEnumerable<XElement> buyerAssignedOrderHeaderIds;
                    string orderNumbersToAcknowledge = string.Empty;
                    string buyerAssignedOrderHeaderIdsToAcknowledge = string.Empty;

                    var orders = xDocument.Element("orderMessage").Elements("order");
                    foreach (var order in orders)
                    {
                        string orderNumber = order.Element("orderIdentification").Element("entityIdentification").Value;

                        XElement Sender = xDocument.Descendants("Sender").FirstOrDefault();
                        XElement Receiver = xDocument.Descendants("Receiver").FirstOrDefault();
                        XElement OrderNumber = new XElement("OrderNumber", orderNumber);
                        XElement OrderDate = new XElement("orderDate");
                        XElement OrderType = new XElement("OrderType");
                        XElement buyer = new XElement("buyer");
                        XElement seller = new XElement("seller");
                        XElement Store = new XElement("Store");
                        XElement deliveryDate = new XElement("deliveryDate");
                        XElement orderLines = new XElement("orderLines");


                        OrderType.Value = order.Element("orderTypeCode").Attribute("codeListVersion").Value;
                        buyer = order.Element("buyer");
                        seller = order.Element("seller");
                        OrderDate.Value = order.Element("creationDateTime").Value;

                        buyerAssignedOrderHeaderIds = from id in order.Elements("orderIdentification")?.Elements("contentOwner")?.Elements("additionalPartyIdentification")
                                                      where (string)id.Attribute("additionalPartyIdentificationTypeCode") == "BUYER_ASSIGNED_ORDER_HEADERID"
                                                      select id;

                        foreach (var orderHeaderId in buyerAssignedOrderHeaderIds)
                        {
                            buyerAssignedOrderHeaderIdsToAcknowledge = orderHeaderId.Value + "," + buyerAssignedOrderHeaderIdsToAcknowledge;
                        }

                        int i = 0;

                        string shipToGln = string.Empty;
                        Dictionary<string, List<XElement>> dictionaryOrderLineItem = new Dictionary<string, List<XElement>>();
                        foreach (var orderLineItem in order.Elements("orderLineItem"))
                        {
                            Store = orderLineItem.Element("orderLineItemDetail").Element("orderLogisticalInformation").Element("shipTo");
                            shipToGln = Store.Element("gln").Value;

                            foreach (var orderLineItemDetail in orderLineItem.Descendants("orderLineItemDetail"))
                            {
                                deliveryDate.Value = orderLineItemDetail.Element("orderLogisticalInformation")
                                    .Element("orderLogisticalDateInformation")
                                    .Element("requestedDeliveryDateTime")
                                    .Element("date").Value;
                                XElement lineCostInc = new XElement("lineCostIncludingTax",
                                    (Convert.ToDecimal(orderLineItemDetail.Element("requestedQuantity").Value, CultureInfo.InvariantCulture) * Convert.ToDecimal(orderLineItem.Element("monetaryAmountIncludingTaxes").Value, CultureInfo.InvariantCulture)).ToString());
                                XElement lineCostExcl = new XElement("lineCostExcludingTax", Convert.ToDecimal(orderLineItemDetail.Element("requestedQuantity").Value, CultureInfo.InvariantCulture) * Convert.ToDecimal(orderLineItem.Element("monetaryAmountExcludingTaxes").Value, CultureInfo.InvariantCulture));

                                i += 1;
                                XElement orderLine = new XElement("OrderLine");
                                orderLines.Add(orderLine);
                                orderLine.Add(new XElement("lineItem", i));
                                orderLine.Add(orderLineItem.Element("netAmount"));
                                orderLine.Add(orderLineItem.Element("netPrice"));
                                orderLine.Add(orderLineItem.Element("monetaryAmountExcludingTaxes"));
                                orderLine.Add(orderLineItem.Element("monetaryAmountIncludingTaxes"));
                                orderLine.Add(orderLineItem.Element("requestedQuantity"));
                                orderLine.Add(orderLineItem.Element("additionalOrderLineInstruction"));
                                orderLine.Add(lineCostInc);
                                orderLine.Add(lineCostExcl);
                                orderLine.Add(orderLineItem.Element("transactionalTradeItem"));
                                orderLine.Add(orderLineItem.Element("promotionalDeal"));
                                orderLine.Add(orderLineItem.Element("avpList"));
                                orderLine.Add(orderLineItemDetail.Element("avpList"));

                            }


                            if (dictionaryOrderLineItem.TryGetValue(shipToGln, out List<XElement> savedOrderLineItem))
                                savedOrderLineItem.Add(orderLineItem);
                            else
                            {
                                savedOrderLineItem = new List<XElement>
                                {
                                    orderLineItem
                                };
                                dictionaryOrderLineItem.Add(shipToGln, savedOrderLineItem);
                            }
                        }

                        foreach (KeyValuePair<string, List<XElement>> keyValuePair in dictionaryOrderLineItem)
                        {
                            order.Elements("orderLineItem").Remove();
                            order.Add(keyValuePair.Value);
                            string additionalOrderLineInstruction = keyValuePair.Value.FirstOrDefault().Element("additionalOrderLineInstruction").Value;

                            XElement orderXml = new XElement("Order",
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

                            dataHandler.InsertOrder(orderNumber, orderXml, keyValuePair.Key, batchId, additionalOrderLineInstruction);
                        }

                        orderNumbersToAcknowledge = orderNumbersToAcknowledge + "," + orderNumber;
                    }
                    orderNumbersToAcknowledge = orderNumbersToAcknowledge.Substring(0, orderNumbersToAcknowledge.Length - 1);
                    buyerAssignedOrderHeaderIdsToAcknowledge = buyerAssignedOrderHeaderIdsToAcknowledge.Substring(0, buyerAssignedOrderHeaderIdsToAcknowledge.Length - 1);

                    if (postToAcknowledge == false)
                    {
                        restHandler.AcknowledgeOrders("[" + orderNumbersToAcknowledge + "]", url + "?action=acknowledge&vendorId=" + VendorName, VendorName, Password);
                    }
                    else
                    {
                        restHandler.AcknowledgeOrdersPost("[" + buyerAssignedOrderHeaderIdsToAcknowledge + "]", url + "?action=acknowledge&vendorId=" + VendorName, VendorName, Password);
                    }
                }
            }
            catch (Exception ex)
            {
                dataHandler.writeLog("Vendor : " + VendorName + ex.Message + Environment.NewLine + ex.InnerException + Environment.NewLine + ex.StackTrace, VendorID);
                dataHandler.writeLog("Response Message : " + Response, VendorID);
                //Console.WriteLine(ex.ToString());
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
        public string SplitOrders(String Customer, String Order, XDocument xdoc, string batchId)
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
            XElement orderNumbers = new XElement("orderNumbers");


            string additionalOrderLineInstruction = string.Empty;
            string assignedHeaderIds = string.Empty;

            int i = 0;

            //Get the specific order
            var orders = xdoc.Descendants("orderMessage")
                         .Elements("order").Elements("orderIdentification")
                         .Elements("entityIdentification")
                         .Where(o => o.Value == Order);

            foreach (var order in orders)
            {
                var ord = order.Parent.Parent;
                OrderType.Value = ord.Element("orderTypeCode").Attribute("codeListVersion").Value;
                buyer = ord.Element("buyer");
                seller = ord.Element("seller");
                OrderDate.Value = ord.Element("creationDateTime").Value;

                var headerId = from id in ord.Elements("orderIdentification")?.Elements("contentOwner")?.Elements("additionalPartyIdentification")
                               where (string)id.Attribute("additionalPartyIdentificationTypeCode") == "BUYER_ASSIGNED_ORDER_HEADERID"
                               select id;

                if (!assignedHeaderIds.Contains(headerId.FirstOrDefault().Value))
                {
                    assignedHeaderIds += headerId.FirstOrDefault().Value + ",";
                }

                foreach (var line in ord.Descendants("orderLineItem"))
                {
                    foreach (var lineItem in line.Descendants("orderLineItemDetail"))
                    {

                        if (lineItem.Element("orderLogisticalInformation").Element("shipTo").Element("gln").Value == Customer)
                        {
                            additionalOrderLineInstruction = line.Element("additionalOrderLineInstruction")?.Value;

                            Store = lineItem.Element("orderLogisticalInformation").Element("shipTo");
                            deliveryDate.Value = lineItem.Element("orderLogisticalInformation").Element("orderLogisticalDateInformation").Element("requestedDeliveryDateTime").Element("date").Value;
                            XElement lineCostInc = new XElement("lineCostIncludingTax", (Convert.ToDecimal(lineItem.Element("requestedQuantity").Value, CultureInfo.InvariantCulture) * Convert.ToDecimal(line.Element("monetaryAmountIncludingTaxes").Value, CultureInfo.InvariantCulture)).ToString());
                            XElement lineCostExcl = new XElement("lineCostExcludingTax", Convert.ToDecimal(lineItem.Element("requestedQuantity").Value, CultureInfo.InvariantCulture) * Convert.ToDecimal(line.Element("monetaryAmountExcludingTaxes").Value, CultureInfo.InvariantCulture));

                            i += 1;
                            XElement orderLine = new XElement("OrderLine");
                            orderLines.Add(orderLine);
                            orderLine.Add(new XElement("lineItem", i));
                            orderLine.Add(line.Element("monetaryAmountExcludingTaxes"));
                            orderLine.Add(line.Element("monetaryAmountIncludingTaxes"));
                            orderLine.Add(lineItem.Element("requestedQuantity"));
                            orderLine.Add(line.Element("additionalOrderLineInstruction"));
                            orderLine.Add(lineCostInc);
                            orderLine.Add(lineCostExcl);
                            orderLine.Add(line.Element("transactionalTradeItem"));
                            orderLine.Add(line.Element("promotionalDeal"));
                            orderLine.Add(line.Element("avpList"));
                            orderLine.Add(lineItem.Element("avpList"));
                        }
                    }
                }



                XElement copyOrderXml = new XElement("Order",
                      Sender,
                      Receiver,
                      OrderNumber,
                      OrderDate,
                      OrderType,
                      buyer,
                      seller,
                      Store,
                      deliveryDate,
                      orderLines
                );


                dataHandler.InsertOrder(Order, copyOrderXml, Customer, batchId, additionalOrderLineInstruction);
                Console.WriteLine("Saved order " + Order + " to the database");

            }

            if (i == 0)
            {
                return null;
            }


            return assignedHeaderIds;

        }

    }
}
