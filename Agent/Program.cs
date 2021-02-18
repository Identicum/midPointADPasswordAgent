using System;
using System.ServiceProcess;

namespace Agent
{
    class Program
    {
        static void Main(string[] args)
        {
            string configpath = Environment.ExpandEnvironmentVariables("%ProgramFiles%\\ADPasswordFilter\\config.json");
            Configuration configuration = Configuration.LoadFromFile(configpath);
            Logger logger = new Logger(configuration.LogFile);

            string[] argvs = Environment.GetCommandLineArgs();

            if (argvs.Length == 4 && "-change".Equals(argvs[1], StringComparison.InvariantCultureIgnoreCase))
            {
                DB db = new DB(configuration.DatabaseFile);
                db.Add(new Credentials(argvs[2], argvs[3]));
                logger.Log(String.Format("Persisted password change for user '{0}'", argvs[2]));
            }
            else
            {
                if (Environment.UserInteractive) {
                    logger.Log("-------- Configuration --------");
                    logger.Log(String.Format("URL: {0}", configuration.BaseURL));
                    logger.Log(String.Format("Service account: {0}", configuration.Username));
                    logger.Log(String.Format("Ignore invalid certs: {0}", configuration.IgnoreCerts));
                    logger.Log(String.Format("Background synchronization interval: {0} millisecons", configuration.SyncInterval));
                    logger.Log(String.Format("DatabaseFile: {0}", configuration.DatabaseFile));
                    logger.Log(String.Format("LogFile: {0}", configuration.LogFile));
                    logger.Log("-------------------------------");
                    logger.Log("Usage: ");
                    logger.Log(String.Format("  {0} -change \"username\" \"password\"", argvs[0]));
                    logger.Log("-------------------------------");
                } else {
                    ServiceBase.Run(new BackgroundService());
                }
            }
        }
    }
}
