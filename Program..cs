/*

Program: Query data from AspenTech InfoPlus.21 using the SQLplus Web
         Service. 

Language: C#

*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Xml;
using System.Xml.Linq;

namespace QueryAspenIP21
{
    class Program
    {
        static void Main(string[] args)
        {
            // Replace [HOST_NAME] below with the details of your
            // IP.21 server
            const string webSvc = "http://[HOST_NAME]"
                + "/SQLPlusWebService/SQLplusWebService.asmx";
            
            const string soap12Req =
                "<?xml version=\"1.0\" encoding=\"utf-8\"?>"
                + "<soap12:Envelope xmlns:xsi="
                + "\"http://www.w3.org/2001/XMLSchema-instance\" "
                + "xmlns:xsd="
                + "\"http://www.w3.org/2001/XMLSchema\" xmlns:soap12="
                + "\"http://www.w3.org/2003/05/soap-envelope\">"
                + "<soap12:Body>"
                + "<ExecuteSQL xmlns="
                + "\"http://www.aspentech.com/SQLplus.WebService\">"
                + "<command>{0}</command>"
                + "</ExecuteSQL>"
                + "</soap12:Body>"
                + "</soap12:Envelope>";
            
            // Build the SQL query string
            const string sqlCmd = "SELECT NAME, IP_DESCRIPTION, "
                + "IP_INPUT_VALUE, IP_INPUT_TIME FROM IP_AnalogDef "
                + "WHERE NAME = 'ATCAI'";
            
            HttpWebRequest webReq = (HttpWebRequest)WebRequest.Create(
                webSvc);
            
            // Set credentials if needed
            webReq.Credentials = CredentialCache.DefaultCredentials;
            webReq.ContentType = "application/soap+xml; charset=utf-8";
            webReq.Method = "POST";

            XmlDocument soapEnvDoc;
            soapEnvDoc = new XmlDocument();
            soapEnvDoc.LoadXml(string.Format(soap12Req, sqlCmd));

            byte[] bytes;
            bytes = Encoding.UTF8.GetBytes(soapEnvDoc.OuterXml);
            webReq.ContentLength = bytes.Length;
            using (Stream stream = webReq.GetRequestStream())
            {
                stream.Write(bytes, 0, bytes.Length);
            }

            HttpWebResponse webRes = (HttpWebResponse)webReq.
                GetResponse();
            Stream dataStream = webRes.GetResponseStream();
            StreamReader reader = new StreamReader(dataStream);

            XmlDocument soapResXml = new XmlDocument();
            soapResXml.Load(reader);

            // Decode encoded values in the XML string
            soapResXml.InnerXml = HttpUtility.HtmlDecode(
                soapResXml.InnerXml);
            
            // Use LINQ for format XML for print
            XDocument beautifulXml = XDocument.Parse(soapResXml.InnerXml);
            soapResXml.InnerXml = beautifulXml.ToString();

            // Build the file path to write
            string filePath = Path.GetDirectoryName(
                Assembly.GetExecutingAssembly().Location) +
                "\\response.xml";

            using(StreamWriter stream = new StreamWriter(filePath, false,
                Encoding.GetEncoding("iso-8859-7")))
            {
                soapResXml.Save(stream);
            }

            // Clean up
            reader.Close();
            dataStream.Close();
            webRes.Close();
        }
    }
}

