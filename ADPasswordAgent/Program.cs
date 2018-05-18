using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;

namespace ADPasswordAgent
{
    class Program
    {
        static void Main(string[] args)
        {
            string wsbaseurl = null;
            string wsauthusr = null;
            string wsauthpwd = null;

            string[] argvs = Environment.GetCommandLineArgs();

            try
            {
                wsbaseurl = ConfigurationManager.AppSettings["BASEURL"];
                wsauthusr = ConfigurationManager.AppSettings["AUTHUSR"];
                wsauthpwd = ConfigurationManager.AppSettings["AUTHPWD"];
            }
            catch
            {
                Console.WriteLine("Usage: {0} \"username\" \"password\"", argvs[0]);
            }

            try
            {
                midPoint mp = new midPoint(wsbaseurl, wsauthusr, wsauthpwd);
                if (mp.UpdateUserPasswordByName(argvs[1], argvs[2]))
                {
                    Console.WriteLine("Password changed");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
    }
}
