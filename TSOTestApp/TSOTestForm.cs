using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Xml;
using System.Security.Cryptography;
using System.Net.Security;

// This is the code for your desktop app.
// Press Ctrl+F5 (or go to Debug > Start Without Debugging) to run your app.

namespace TSOTestApp
{
    public partial class TSOTester : Form
    {
        #region Certificate storage
        const string CONTENT_TYPE_XML = "application/xml; charset=utf-8";
        const string CONTENT_TYPE_JSON = "application/json; charset=utf-8";
        const string CONTENT_TYPE_MS_EXCEL_XLSX =
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

        private X509Certificate2Collection collection = new X509Certificate2Collection();
        private X509Certificate2Collection certsToAuthWith = new X509Certificate2Collection();

        private string _content = CONTENT_TYPE_XML;
        #endregion

        #region ctor
        public TSOTester()
        {

            InitializeComponent();
            btnGetData.Enabled = cbNoCerts.Checked;
        }
        #endregion

        #region Get Data
        private void btnGetData_Click(object sender, EventArgs e)
        {
            HttpWebResponse response = ExecuteWebRequest(txtURL.Text + ((cbAlternateURL.SelectedIndex <= 1) ? txtRequest.Text : ""));

            if (response != null)
            {
                if ((response.StatusCode == HttpStatusCode.OK) || (response.StatusCode == HttpStatusCode.Accepted))
                {
                    // Gets the stream associated with the response.
                    Stream receiveStream = response.GetResponseStream();
                    Encoding encode = System.Text.Encoding.GetEncoding("utf-8");
                    // Pipes the stream to a higher level stream reader with the required encoding format.
                    StreamReader readStream = new StreamReader(receiveStream, encode);

                    Char[] read = new Char[256];
                    // Reads 256 characters at a time.
                    int count = readStream.Read(read, 0, 256);
                    txtResponse.Text += "\r\nresponse = ";
                    while (count > 0)
                    {
                        // Dumps the 256 characters on a string and displays the string to the console.
                        String str = new String(read, 0, count);
                        txtResponse.Text += str.Replace("></", ">\r</").Replace(",\"", ",\r\"");
                        count = readStream.Read(read, 0, 256);
                    }

                    // Releases the resources of the response.
                    response.Close();
                    // Releases the resources of the Stream.
                    readStream.Close();
                }
                else
                {
                    txtResponse.Text += "\r\n\nWS Responded but Response Code = " + response.StatusCode.ToString();
                }
            }
        }

        private HttpWebResponse ExecuteWebRequest(string url)
        {
            if (!string.IsNullOrEmpty(url)) // This counts on Username now carrying the certificate name!!
            {
                // Create a web request that points to our SSL-enabled client certificate required web site
                txtResponse.Text += "\r\nExecuteWebRequest with Cert:: Generate request for " + url;

                HttpWebRequest webRequest = WebRequest.Create(url) as HttpWebRequest;
                if (null != webRequest)
                {
                    webRequest.ProtocolVersion = HttpVersion.Version10;
                    webRequest.UseDefaultCredentials = cbNoCerts.Checked;
                    webRequest.PreAuthenticate = false;
                    webRequest.ContentType = _content;
                    webRequest.Method = cbPost.Checked ? "POST" : "GET";
                    webRequest.ContentLength = 0;
                    webRequest.UserAgent = @"Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/51.0.2704.106 Safari/537.36";
                    // Set Security protocol to TLS1.2
                    // & Set Basic Authentication
                    SecurityProtocolType _securityProtocolType = SecurityProtocolType.Tls;

                    const SecurityProtocolType Tls11 = (SecurityProtocolType)0x00000300;
                    const SecurityProtocolType Tls12 = (SecurityProtocolType)0x00000C00;

                    _securityProtocolType |= Tls11 | Tls12; // All protocols


                    ServicePointManager.SecurityProtocol = (cbTLS12.Checked) ? SecurityProtocolType.Tls12 : _securityProtocolType;
                    ServicePointManager.Expect100Continue = true;
                    ServicePointManager.ServerCertificateValidationCallback +=
                        (sender, certificate, chain, sslPolicyErrors) =>
                        {
                            if (sslPolicyErrors == SslPolicyErrors.None)
                            {
                                return true;
                            }
                            if (sslPolicyErrors == SslPolicyErrors.RemoteCertificateChainErrors) txtResponse.Text += "\r\nServerCertificateValidationCallback ::  SslPolicyErrors = RemoteCertificateChainErrors";
                            if (sslPolicyErrors == SslPolicyErrors.RemoteCertificateNameMismatch) txtResponse.Text += "\r\nServerCertificateValidationCallback ::  SslPolicyErrors = RemoteCertificateNameMismatch";
                            if (sslPolicyErrors == SslPolicyErrors.RemoteCertificateNotAvailable) txtResponse.Text += "\r\nServerCertificateValidationCallback ::  SslPolicyErrors = RemoteCertificateNotAvailable";

                            return false;
                        };
                    txtResponse.Text += $"\r\nSecurity Protocol = 0x{ServicePointManager.SecurityProtocol.ToString("X")}";

                    if (!cbNoCerts.Checked)
                    {
                        if (certsToAuthWith.Count > 0)
                        {
                            txtResponse.Text += string.Format("\r\n{0} Certificate(s) used. 1st is {1}", certsToAuthWith.Count, certsToAuthWith[0].SubjectName.Name);
                            // Associate the certificates with the request
                            webRequest.ClientCertificates.AddRange(certsToAuthWith);
                            txtResponse.Text += String.Format("\r\nAdded cert collection to WebRequest. Returning with response ... here we go");
                        }
                        else
                        {
                            // Fallback II - try the old way
                            txtResponse.Text += String.Format("\r\nCertificate auth failed!");
                        }
                    }
                    // DEBUGGING CODE :::::::::::::::::::::::::::::::::::::::
                    HttpWebResponse response = null;
                    try
                    {
                        txtResponse.Text += String.Format("\r\nHitting service now ...");
                        response = webRequest.GetResponse() as HttpWebResponse;
                        txtResponse.Text += String.Format("\r\n... I'm back");
                        txtResponse.Text += $"\r\nreturn from call - Response Description:   {response.StatusDescription}";
                    }
                    catch (WebException e)
                    {
                        txtResponse.Text += $"\r\nWeb Exception occured. Details: {e.Message}";
                        txtResponse.Text += $"\r\n         More details (Status): {e.Status.ToString()}\r\n\r\n";


                        if (e.Response != null)
                        {
                            using (WebResponse eResponse = e.Response)
                            {
                                HttpWebResponse httpResponse = (HttpWebResponse)eResponse;
                                Console.WriteLine("Error code: {0}", httpResponse.StatusCode);
                                using (Stream data = e.Response.GetResponseStream())
                                using (var reader = new StreamReader(data))
                                {
                                    string text = reader.ReadToEnd();
                                    txtResponse.Text += text.Replace(">", ">\r");
                                }
                            }
                        }
                        else
                        {
                            txtResponse.Text += "\r\ne.Response was null, so nothing more.";
                        }
                    }
                    catch (Exception ex)
                    {
                        txtResponse.Text += String.Format("\r\nGetResponse Failed:", ex);
                    }
                    if (null != response) txtResponse.Text += $"\r\n                   Mutually Authenticated: {(response.IsMutuallyAuthenticated ? "Yes" : "No")}";
                    // Make the web request
                    return response;

                }
            }
            return null;
        }

        private void cbAlternateURL_SelectedIndexChanged(object sender, EventArgs e)
        {
            txtURL.Text = cbAlternateURL.Text;
            if(cbAlternateURL.SelectedIndex == 4) // sample code
            {
                rbExcel.Checked = true;
            }
            else
            {
                rbXml.Checked = true;
            }
        }

        private void rbXml_CheckedChanged(object sender, EventArgs e)
        {
            if (rbJSON.Checked)
            {
                txtURL.Text = txtURL.Text.Replace("format=xml", "format=json");
                txtRequest.Text = txtRequest.Text.Replace("format=xml", "format=json");
                _content = CONTENT_TYPE_JSON;
            }
            if (rbXml.Checked)
            {
                txtURL.Text = txtURL.Text.Replace("format=xml", "format=json");
                txtRequest.Text = txtRequest.Text.Replace("format=json", "format=xml");
                _content = CONTENT_TYPE_XML;
            }
            if (rbExcel.Checked)
            {
                _content = CONTENT_TYPE_MS_EXCEL_XLSX;
            }
        }
        #endregion

        #region Certificate retrieval
        private X509Certificate2Collection GetCertCollection(bool UseCAC)
        {
            lbCerts.Items.Clear();
            lbCerts.Items.Add("Pick one .. if authenticating with 1 cert");

            X509Certificate2Collection collection1 = new X509Certificate2Collection();
            X509Certificate2Collection collection2 = new X509Certificate2Collection();

            if (UseCAC)
            {
                var smartCerts = GetSmartCardCertifcates();

                foreach (KeyValuePair<string, X509Certificate2> cert in smartCerts)
                {
                    collection1.Add(cert.Value);
                }
            }
            else
            {
                X509Store store;

                // Use the X509Store class to get a handle to the local certificate stores. "My" is the "Personal" store.
                store = new X509Store(cbStore.Text, rbLocal.Checked ? StoreLocation.LocalMachine : StoreLocation.CurrentUser);

                // Open the store to be able to read from it.
                store.Open(OpenFlags.ReadOnly);

                // Use the X509Certificate2Collection class to get a list of certificates that match our criteria (in this case, we should only pull back one).
                collection1 = store.Certificates.Find(X509FindType.FindBySubjectDistinguishedName, txtCertCriteria1.Text, true);
                collection2 = store.Certificates.Find(X509FindType.FindByIssuerName, txtCertCriteria2.Text, true); // rest of the chain of trust

                if (collection2.Count > 0)
                {
                    foreach (var cert in collection2)
                    {
                        collection1.Add(cert);
                    }
                }
                store.Close();
            }
            txtResponse.Text = $"{collection1.Count} certificates added to collection.\r\n";
            foreach (var cert in collection1)
            {
                lbCerts.Items.Add($"{cert.Issuer} / {(cert.HasPrivateKey ? "has private key" : "no private key")} / {cert.SubjectName.Name}");
            }
            return collection1;
        }

        static public Dictionary<string, X509Certificate2> GetSmartCardCertifcates()
        {
            Dictionary<string, X509Certificate2> smartCardCerts = new Dictionary<string, X509Certificate2>();

            var myStore = new X509Store(StoreName.My, StoreLocation.CurrentUser);
            myStore.Open(OpenFlags.ReadOnly);
            int certNo = 0;
            foreach (X509Certificate2 cert in myStore.Certificates)
            {
                if (!cert.HasPrivateKey) continue; // not smartcard for sure
                try
                {
                    var rsa = cert.PrivateKey as RSACryptoServiceProvider;
                    if (rsa == null) continue; // not smart card cert again
                    if (rsa.CspKeyContainerInfo.HardwareDevice) // sure - smartcard
                    {
                        // inspect rsa.CspKeyContainerInfo.KeyContainerName Property
                        // or rsa.CspKeyContainerInfo.ProviderName (your smartcard provider, such as 
                        // "Schlumberger Cryptographic Service Provider" for Schlumberger Cryptoflex 4K
                        // card, etc
                        certNo++;
                        smartCardCerts.Add($"{certNo} - {cert.Issuer} / {cert.PublicKey.Oid.FriendlyName}", cert);
                        //var data = rsa.SignData(); // to confirm presence of private key - to finally authenticate
                    }
                }
                catch
                {

                }
            }
            myStore.Close();
            return smartCardCerts;
        }

        private void btnCheckCerts_Click(object sender, EventArgs e)
        {
            collection = GetCertCollection(chkUseCAC.Checked);
            SetCertSet();
            btnGetData.Enabled = collection.Count != 0 || cbNoCerts.Checked;
            cbJustOne.Enabled = btnGetData.Enabled;
        }
        #endregion

        #region Sample code
        /// <summary>
        /// This Visual Studio 2008 .NET sample page with embedded C# code
        /// shows how to use TS-MATS RESTful Web Service with TSIMS service
        /// </summary>
        private void ExampleCode_NotUsedDirectly(EventArgs e)
        {
            base.OnLoad(e);

            const string SERVICE = "TSIMS";

            const string RESOURCE_TSMATS =
                "https:/tsmats.atsc.army.mil/TSMATS_Tools/api/tsims";
            const string CERTIFICATE_TSMATS = "tsmats.atsc.army.mil";

            const string METHOD_GET = "GET";

            const string CONTENT_TYPE_JSON = "application/json";
            const string CONTENT_TYPE_MS_EXCEL_XLSX =
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            const string CONTENT_TYPE_XML = "application/xml";

            string _service = null;
            string _resource = null, _method = null, _contentType = null, _postData = null;
            string _certificate = null;
            bool _doSendFile = false;

            _service = SERVICE;

            // Set RESTful Web Service Request Parameters:
            // *******************************************

            // I. Get TSIMS data from TS-MATS in XML format...
            // ===============================================

            _resource = RESOURCE_TSMATS;
            _method = METHOD_GET;
            _contentType = CONTENT_TYPE_XML;
            _certificate = CERTIFICATE_TSMATS;

            // ...and display it on this page:

            _doSendFile = false;

            // II. Get TSIMS data from TS-MATS in JSON format...
            // =================================================

            _resource = RESOURCE_TSMATS;
            _method = METHOD_GET;
            _contentType = CONTENT_TYPE_JSON;
            _certificate = CERTIFICATE_TSMATS;

            // ...and display it on this page:

            _doSendFile = false;

            // III. Get TSIMS data from TS-MATS in MS Excel format...
            // ======================================================

            _resource = RESOURCE_TSMATS;
            _method = METHOD_GET;
            _contentType = CONTENT_TYPE_MS_EXCEL_XLSX;
            _certificate = CERTIFICATE_TSMATS;

            // ...and open it as a file:

            _doSendFile = true;

            // ******************************************

            if (!string.IsNullOrEmpty(_resource) &&
                !string.IsNullOrEmpty(_method) &&
                !string.IsNullOrEmpty(_contentType))
            {
                // // Response.Write(string.Concat(
                //    "<b><u><i>TS-MATS RESTful Web Service Sample</i></u></b>"));
                // // Response.Write("<p />");

                // // Response.Write(string.Concat("<b>REQUEST</b>", ": "));
                // // Response.Write("<br />");

                // // Response.Write(string.Concat("<br /><b>_resource</b>", ": ",
                //    _resource));
                // // Response.Write(string.Concat("<br /><b>_method</b>", ": ",
                //    _method));
                // // Response.Write(string.Concat("<br /><b>_contentType</b>", ": ",
                //    _contentType));
                //if (!string.IsNullOrEmpty(_postData))
                //     // Response.Write(string.Concat("<br /><b>_postData</b>", ": ",
                //        _postData));
                //if (!string.IsNullOrEmpty(_certificate))
                //     // Response.Write(string.Concat("<br /><b>_certificate</b>", ": ",
                //        _certificate));

                try
                {
                    HttpWebRequest _request = WebRequest.Create(_resource) as HttpWebRequest;
                    if (_request != null)
                    {
                        // Set Basic Authentication

                        _request.ProtocolVersion = HttpVersion.Version10;
                        _request.UseDefaultCredentials = false;
                        _request.PreAuthenticate = false;

                        SecurityProtocolType _securityProtocolType = SecurityProtocolType.Tls;

                        const SecurityProtocolType Tls11 = (SecurityProtocolType)0x00000300;
                        const SecurityProtocolType Tls12 = (SecurityProtocolType)0x00000C00;

                        _securityProtocolType |= Tls11 | Tls12;

                        ServicePointManager.SecurityProtocol = _securityProtocolType;
                        ServicePointManager.Expect100Continue = true;
                        ServicePointManager.ServerCertificateValidationCallback +=
                            (sender, certificate, chain, sslPolicyErrors) =>
                            {
                                if (sslPolicyErrors == SslPolicyErrors.None)
                                    return true;

                                return false;
                            };

                        // Add Certificate

                        if (!string.IsNullOrEmpty(_certificate))
                        {
                            _request.ClientCertificates.Clear();

                            StoreLocation _storeLocation = StoreLocation.LocalMachine;
                            X509Store _store = new X509Store(StoreName.My, _storeLocation);
                            X509Certificate2 _cert;

                            _store.Open(OpenFlags.ReadOnly | OpenFlags.OpenExistingOnly);

                            X509Certificate2Collection _certColl = _store.Certificates.Find
                                (X509FindType.FindBySubjectName, _certificate,
                                true); // validOnly
                            if (_certColl.Count > 0)
                            {
                                _cert = _certColl[0];

                                _request.ClientCertificates.Add(_cert);

                                // Response.Write(string.Concat(
                                //   " (<b>found</b> and <b>added</b>)"));
                            }

                            _store.Close();
                        }

                        // Set Method and ContentType

                        _request.Method = _method;
                        _request.ContentType = _contentType;

                        // Add Post Data

                        if (!string.IsNullOrEmpty(_postData))
                        {
                            using (Stream _requestStream = _request.GetRequestStream())
                            {
                                byte[] _bytes = Encoding.UTF8.GetBytes(_postData);

                                _requestStream.Write(_bytes, 0, _bytes.Length);
                                _requestStream.Close();
                            }
                        }

                        // Get HttpWebResponse

                        using (HttpWebResponse _response =
                            _request.GetResponse() as HttpWebResponse)
                        {
                            // Response.Write("<br />");
                            // Response.Write("<br />");
                            // Response.Write(string.Concat("<b>RESPONSE</b>", ": "));

                            // Response.Write("<br />");
                            // Response.Write("<br />");
                            // Response.Write(string.Concat("<b>StatusCode</b>", ": ",
                            //_response.StatusCode.ToString()));

                            if (_response.StatusCode == HttpStatusCode.OK ||
                                _response.StatusCode == HttpStatusCode.Accepted)
                            {
                                string _fileName = null;

                                if (_doSendFile)
                                {
                                    // File Name

                                    _fileName = String.Concat(
                                        "TS-MATS",
                                        (string.IsNullOrEmpty(_service) ? string.Empty :
                                        String.Concat(
                                        "_", _service)),
                                        "_", DateTime.Now.ToString(@"yyMMddHHmmss"));

                                    if (_request.ContentType.Equals(
                                        CONTENT_TYPE_MS_EXCEL_XLSX))
                                        _fileName = String.Concat(_fileName, ".xlsx");
                                    else if (_request.ContentType.Equals(CONTENT_TYPE_JSON))
                                        _fileName = String.Concat(_fileName, ".json");
                                    else if (_request.ContentType.Equals(CONTENT_TYPE_XML))
                                        _fileName = String.Concat(_fileName, ".xml");

                                    // Start Sending File

                                    // Response.Clear();

                                    // Response.Charset = string.Empty;
                                    // Response.Cache.SetCacheability(HttpCacheability.Private);
                                    // Response.ContentType = _response.ContentType;

                                    // Response.AppendHeader("Content-Disposition",
                                    //   String.Concat("attachment;filename=", _fileName));
                                }
                                else
                                {
                                    // Response.Write("<br />");
                                    // Response.Write(string.Concat("<b>ContentType</b>", ": ",
                                    //   _response.ContentType));
                                }

                                if (_request.ContentType.Equals(CONTENT_TYPE_MS_EXCEL_XLSX))
                                {
                                    // Get MemoryStream

                                    MemoryStream _mstream = new MemoryStream();

                                    byte[] _buffer = new byte[16384];
                                    int _count = 0;

                                    using (Stream _stream = _response.GetResponseStream())
                                    {
                                        do
                                        {
                                            _count = _stream.Read(_buffer, 0, _buffer.Length);
                                            if (_count > 0)
                                                _mstream.Write(_buffer, 0, _count);
                                        } while (_count > 0);
                                    }

                                    if (_doSendFile)
                                    {
                                        // Send MS Excel File

                                        //_mstream.WriteTo(Response.OutputStream);
                                    }
                                    else
                                    {
                                        // Write Info to Response

                                        // Response.Write("<br />");
                                        // Response.Write(string.Concat("<b>Contents</b>", ":"));
                                        // Response.Write("<br />");

                                        // Response.Write(string.Concat("<b>MemoryStream</b>: ",
                                        //   "Length = ", _mstream.Length.ToString()));
                                    }
                                }
                                else
                                {
                                    if (_doSendFile)
                                    {
                                        // Send File

                                        using (StreamReader _reader =
                                            new StreamReader(_response.GetResponseStream()))
                                        {
                                            // Response.Write(_reader.ReadToEnd());
                                        }
                                    }
                                    else
                                    {
                                        // Write Contents to Response

                                        // Response.Write("<br />");
                                        // Response.Write(string.Concat("<b>Contents</b>", ":"));
                                        // Response.Write("<br />");

                                        using (StreamReader _reader =
                                            new StreamReader(_response.GetResponseStream()))
                                        {
                                            // Response.Write(
                                            //   Server.HtmlEncode(_reader.ReadToEnd()));
                                        }
                                    }
                                }

                                if (_doSendFile)
                                {
                                    // Finish Sending File

                                    // Response.Flush();
                                    // Response.End();
                                }
                            }
                        }
                    }
                }
                catch// (Exception _ex)
                {
                    // Response.Write("<br />");
                    // Response.Write("<br />");
                    // Response.Write(string.Concat("<b>ERROR<b/>", ": ", _ex.Message));
                }
            }
        }
        #endregion

        #region cert UI
        private void cbJustOne_CheckedChanged(object sender, EventArgs e)
        {
            SetCertSet();
        }

        private void lbCerts_SelectedIndexChanged(object sender, EventArgs e)
        {
            cbJustOne.Checked = lbCerts.SelectedIndex > 0;
            SetCertSet();
        }

        private void SetCertSet()
        {
            if (!cbJustOne.Checked) lbCerts.SelectedIndex = -1;
            certsToAuthWith.Clear();

            if (cbJustOne.Checked && lbCerts.SelectedIndex > 0)
            {
                certsToAuthWith.Add(collection[lbCerts.SelectedIndex - 1]);
                txtResponse.Text += $"\r\n{certsToAuthWith[0].SubjectName.Name} selected";
            }
            else
            {
                certsToAuthWith.AddRange(collection);
            }
            txtResponse.Text += $"\r\nAuthenticating with {certsToAuthWith.Count} cert(s)";
        }
        #endregion

        private void btnQuit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void cbStore_SelectedIndexChanged(object sender, EventArgs e)
        {
            //txtCertCriteria1.Text = (cbStore.SelectedIndex == 1) ? "CN=EUSTNB4351911, OU=Computers, OU=G-6, OU=TRADOC HQ, OU=TRADOC, OU=Eustis, OU=Installations, DC=nae, DC=ds, DC=army, DC=mil" : "CN=portal2.tradoc.army.mil, OU=USA, OU=PKI, OU=DoD, O=U.S. Government, C=US";
        }

        private void cbNoCerts_CheckedChanged(object sender, EventArgs e)
        {
            btnGetData.Enabled = collection.Count != 0 || cbNoCerts.Checked;
        }
    }
}
