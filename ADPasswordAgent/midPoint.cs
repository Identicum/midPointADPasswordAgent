using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace ADPasswordAgent
{
    class midPoint
    {
        private static HttpClient client = new HttpClient();

        public midPoint(string wsbaseurl, string wsauthusr, string wsauthpwd)
        {
            string cleanurl = wsbaseurl.EndsWith("/") ? wsbaseurl : wsbaseurl + '/'; // the url must end with '/'
            client.BaseAddress = new Uri(cleanurl);
            string authval = Convert.ToBase64String(Encoding.ASCII.GetBytes(wsauthusr + ":" + wsauthpwd)); // encode user/pass for basic auth
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authval);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/xml"));
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

            HttpResponseMessage response = client.PostAsync("ws/rest/users/search", content).Result;
            if (response.IsSuccessStatusCode)
            {
                string xmlobj = response.Content.ReadAsStringAsync().Result;

                // get oid from returned user object
                XmlDocument xmldoc = new XmlDocument();
                xmldoc.LoadXml(xmlobj);
                return xmldoc.FirstChild.FirstChild.Attributes.GetNamedItem("oid").Value;
            }
            return null;
        }

        public bool UpdateUserPasswordByName(string name, string password)
        {
            string oid = this.GetUserOIDByName(name);
            return UpdateUserPasswordByOID(oid, password);
        }

        public bool UpdateUserPasswordByOID(string OID, string password)
        {
            if (OID == null || password == null) { return false; }
            // query for password changing. ToDo: Send encrypted value instead of plaintext
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

            HttpResponseMessage response = client.PostAsync("ws/rest/users/" + OID, content).Result;
            return response.IsSuccessStatusCode;
        }
    }
}
