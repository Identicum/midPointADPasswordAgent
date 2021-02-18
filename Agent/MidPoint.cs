using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security;
using System.Text;
using System.Xml;

namespace Agent
{
    class MidPoint
    {
        public Configuration configuration { get; set; }
        private HttpClient httpClient;

        public MidPoint(Configuration configuration)
        {
            this.configuration = configuration;
            this.InitHttpClient();
        }

        private void InitHttpClient()
        {
            if (this.configuration.IgnoreCerts)
            {
                HttpClientHandler handler = new HttpClientHandler();
                handler.ServerCertificateCustomValidationCallback = (message, Convert, ContextMarshalException, sslPolicyErrors) => {
                    return true;
                };
                this.httpClient = new HttpClient(handler);
            }
            else
            {
                this.httpClient = new HttpClient();
            }

            Uri baseAddress = new Uri(this.configuration.BaseURL.EndsWith("/") ? this.configuration.BaseURL : this.configuration.BaseURL + '/');
            this.httpClient.BaseAddress = baseAddress;
            AuthenticationHeaderValue authHeaderValue = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.ASCII.GetBytes(this.configuration.Username + ":" + this.configuration.Password)));
            this.httpClient.DefaultRequestHeaders.Authorization = authHeaderValue;
            this.httpClient.DefaultRequestHeaders.Accept.Clear();
            this.httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/xml"));
        }

        public string GetUserOIDByName(string name)
        {
            if (name == null) { return null; }
            // query for searching users by name
            string query = string.Format(
                @"<query>
                    <filter>
                        <equal>
                            <path>name</path>
                            <value>{0}</value>
                        </equal>
                    </filter>
                </query>",
                SecurityElement.Escape(name));

            HttpContent content = new StringContent(query, Encoding.UTF8);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/xml");

            HttpResponseMessage response = httpClient.PostAsync("ws/rest/users/search", content).Result;
            if (response.IsSuccessStatusCode)
            {
                string xmlobj = response.Content.ReadAsStringAsync().Result;

                // get oid from returned user object
                XmlDocument xmldoc = new XmlDocument();
                xmldoc.LoadXml(xmlobj);
                try {
                    return xmldoc.FirstChild.FirstChild.Attributes.GetNamedItem("oid").Value;
                }
                catch (NullReferenceException) {
                    throw new HttpRequestException("User not found");
                }
            }
            else
            {
                throw new HttpRequestException(string.Format("The server responded with a status of {0} {1}", (int)response.StatusCode, response.StatusCode));
            }
        }

        public bool UpdateUserPasswordByOID(string OID, string password)
        {
            if (OID == null || password == null) { return false; }

            string query = string.Format(
                @"<objectModification
                    xmlns='http://midpoint.evolveum.com/xml/ns/public/common/api-types-3'
                    xmlns:c='http://midpoint.evolveum.com/xml/ns/public/common/common-3'
                    xmlns:t='http://prism.evolveum.com/xml/ns/public/types-3'>
                    <itemDelta>
                        <t:modificationType>replace</t:modificationType>
                        <t:path>c:credentials/c:password/c:value</t:path>
                        <t:value>
                            <clearValue>{0}</clearValue>
                        </t:value>
                    </itemDelta>
                </objectModification>",
                SecurityElement.Escape(password));

            HttpContent content = new StringContent(query, Encoding.UTF8);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/xml");

            HttpResponseMessage response = httpClient.PostAsync("ws/rest/users/" + OID, content).Result;
            
            if (response.IsSuccessStatusCode)
            {
                return response.IsSuccessStatusCode;
            }
            else
            {
                throw new HttpRequestException(string.Format("The server responded with a status of {0} {1}", (int)response.StatusCode, response.StatusCode));
            }
        }

        public bool UpdateUserPasswordByName(string name, string password)
        {
            string oid = this.GetUserOIDByName(name);
            return UpdateUserPasswordByOID(oid, password);
        }
    }
}