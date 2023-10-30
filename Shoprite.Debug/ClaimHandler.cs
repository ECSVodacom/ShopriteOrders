using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Shoprite.Debug
{
    public class ClaimHandler
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
        public string DownloadClaims(string VendorID, string VendorName, string Password, string url)
        {
            bool NoMoreClaims = false;

            try
            {
                do
                {

                    //dataHandler.UpdateLastCheckDate(VendorID);
                    string Response = restHandler.GetClaims(url, VendorName, Password);
                    //if (Response.StartsWith("<string>No downloadable Claims")) { return "No Claim To Download"; }

                    //XDocument doc = XDocument.Load(@"C:\temp\OriginalClaimXML1.xml", LoadOptions.None);

                    XDocument doc = XDocument.Load(new System.IO.MemoryStream(System.Text.Encoding.UTF8.GetBytes(Response)));

                    var claims = from c in doc.Descendants()
                                 where c.Name.LocalName == "claimsNotification"
                                 select c;

                    XNamespace xsd = "http://www.w3.org/2001/XMLSchema";
                    XNamespace xsi = "http://www.w3.org/2001/XMLSchema-instance";
                    XNamespace urn = "urn:gs1:ecom:claims_notification:xsd:3";

                    var StandardBusinessDocument = from d in doc.Descendants()
                                                   where d.Name.LocalName == "StandardBusinessDocumentHeader"
                                                   select d;

                    XNamespace ns = "http://www.unece.org/cefact/namespaces/StandardBusinessDocumentHeader";

                    //foreach (XElement Xelement in doc.Root.DescendantsAndSelf())
                    //{
                    //    // Stripping the namespace by setting the name of the element to it's localname only
                    //    Xelement.Name = Xelement.Name.LocalName;
                    //    // replacing all attributes with attributes that are not namespaces and their names are set to only the localname
                    //    Xelement.ReplaceAttributes((from xattrib in Xelement.Attributes().Where(xa => !xa.IsNamespaceDeclaration) select new XAttribute(xattrib.Name.LocalName, xattrib.Value)));
                    //}

                    string sender = StandardBusinessDocument.Descendants(ns + "Sender").SingleOrDefault().Value;  // xdoc.Element("claimsNotificationMessage").Element("StandardBusinessDocumentHeader").Element("Sender").Element("Identifier").Value.ToString();
                    string receiver = StandardBusinessDocument.Descendants(ns + "Receiver").SingleOrDefault().Value; // xdoc.Element("claimsNotificationMessage").Element("StandardBusinessDocumentHeader").Element("Receiver").Element("Identifier").Value.ToString();
                    string claimsNumbers = StandardBusinessDocument.Descendants(ns + "InstanceIdentifier").SingleOrDefault().Value;// xdoc.Element("claimsNotificationMessage").Element("StandardBusinessDocumentHeader").Element("DocumentIdentification").Element("InstanceIdentifier").Value.ToString();

                    //string sender = doc.Element("claimsNotificationMessage").Element("StandardBusinessDocumentHeader").Element("Sender").Element("Identifier").Value.ToString();
                    //string receiver = doc.Element("claimsNotificationMessage").Element("StandardBusinessDocumentHeader").Element("Receiver").Element("Identifier").Value.ToString();
                    //string claims = doc.Element("claimsNotificationMessage").Element("StandardBusinessDocumentHeader").Element("DocumentIdentification").Element("InstanceIdentifier").Value.ToString();

                    String BatchID = "0";
                    try
                    {
                        BatchID = dataHandler.InsertDownloadedClaim(VendorID, doc, claimsNumbers, sender, receiver);
                    }
                    catch (Exception ClaimDownloadEX)
                    {
                        BatchID = "0";
                        dataHandler.writeLog("BATCH ID " + BatchID + " " + VendorID + "  " + claimsNumbers + " " + sender + " " + sender, VendorID);
                        dataHandler.writeLog("Could not download claims TEST : " + ClaimDownloadEX.ToString(), VendorID);
                    }



                    foreach (var claimDocument in claims)
                    {

                        var ClaimsNotificationMessage = new XElement(urn + "claimsNotificationMessage",
                              new XAttribute(XNamespace.Xmlns + "xsd", xsd),
                              new XAttribute(XNamespace.Xmlns + "xsi", xsi),
                              new XAttribute(XNamespace.Xmlns + "ns0", urn));
                        var DocNum = claimDocument.Descendants("claimsNotificationIdentification").Elements("entityIdentification").SingleOrDefault().Value;
                        String Branch = "";

                        try
                        {
                            Branch = claimDocument.Descendants("buyer").Elements("gln").SingleOrDefault().Value.ToString();
                        }
                        catch (Exception BranchEX)
                        {

                            dataHandler.writeLog("Could not find Branch " + BranchEX.ToString(), VendorID);
                        }


                        StandardBusinessDocument.Descendants(ns + "InstanceIdentifier").SingleOrDefault().Value = DocNum;
                        StandardBusinessDocument.Descendants(ns + "NumberOfItems").SingleOrDefault().Value = "1";
                        ClaimsNotificationMessage.Add(StandardBusinessDocument);
                        ClaimsNotificationMessage.Add(claimDocument);

                        //INSERT CLAIM IN DATABASE HERE
                        try
                        {
                            dataHandler.InsertClaimDocument(DocNum, ClaimsNotificationMessage, VendorID, BatchID, sender, receiver, Branch);
                        }
                        catch (Exception INsertClaimEX)
                        {
                            dataHandler.writeLog("Could Not Insert Claim Document " + INsertClaimEX.ToString(), VendorID);
                            throw INsertClaimEX;
                        }

                    }

                    try
                    {
                        restHandler.AcknowledgeClaims("[" + claimsNumbers + "]", url + "?action=acknowledge&vendorId=" + VendorName, VendorName, Password);

                    }
                    catch (Exception ex)
                    {

                        throw ex;
                    }

                    //Console.WriteLine(restHandler.AcknowledgeOrders("[" + orders + "]", url + "?action=acknowledge&vendorId=" + VendorName, VendorName, Password));

                } while (NoMoreClaims == false);


            }
            catch (Exception ex)
            {
                //Console.WriteLine(ex.ToString());
                dataHandler.writeLog(ex.ToString(), VendorID);
                NoMoreClaims = true;
            }
            return "Claims Successfully downloaded";

        }


        private void SplitClaims(String xml)
        {
            XDocument xdoc = XDocument.Parse(xml);
            try
            {
                var claims = from c in xdoc.Descendants()
                             where c.Name.LocalName == "claimsNotification"
                             select c;

                XNamespace xsd = "http://www.w3.org/2001/XMLSchema";
                XNamespace xsi = "http://www.w3.org/2001/XMLSchema-instance";
                XNamespace urn = "urn:gs1:ecom:claims_notification:xsd:3";


                var ClaimsNotificationMessage = new XElement(urn + "claimsNotificationMessage",
                      new XAttribute(XNamespace.Xmlns + "xsd", xsd),
                      new XAttribute(XNamespace.Xmlns + "xsi", xsi),
                      new XAttribute(XNamespace.Xmlns + "ns0", urn));


                var StandardBusinessDocument = from d in xdoc.Descendants()
                                               where d.Name.LocalName == "StandardBusinessDocumentHeader"
                                               select d;

                XNamespace ns = "http://www.unece.org/cefact/namespaces/StandardBusinessDocumentHeader";


                foreach (var claimDocument in claims)
                {
                    var DocNum = claimDocument.Descendants("claimsNotificationIdentification").Elements("entityIdentification").SingleOrDefault().Value;
                    StandardBusinessDocument.Descendants(ns + "InstanceIdentifier").SingleOrDefault().Value = DocNum;
                    StandardBusinessDocument.Descendants(ns + "NumberOfItems").SingleOrDefault().Value = "1";
                    var Branch = claimDocument.Descendants("buyer").Elements("gln").SingleOrDefault().Value;
                    ClaimsNotificationMessage.Add(StandardBusinessDocument);
                    ClaimsNotificationMessage.Add(claimDocument);
                }
            }
            catch (Exception EX)
            {
                Exception t = new Exception("Claims Split - " + EX.Message, EX.InnerException);
                dataHandler.writeLog("Claims Split : " + EX.ToString(), "All Vendors");
                throw t;
            }


        }
    }
}
