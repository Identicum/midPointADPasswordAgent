using System;
using System.Diagnostics;
using System.ServiceProcess;
using System.Threading;
using System.Threading.Tasks;

namespace Agent
{
    public class BackgroundService : ServiceBase
    {
        CancellationTokenSource cancellationTokenSource;
        Task task;

        public BackgroundService()
        {
            AutoLog = true;
            CanPauseAndContinue = true;
            CanStop = true;
            ServiceName = "BackgroundAgentService";
        }

        protected override void OnStart(string[] args)
        {
            RunTask();
        }

        protected override void OnStop()
        {
            StopTask();
        }

        protected override void OnPause() {
            StopTask();
        }

        protected override void OnContinue () {
            RunTask();
        }

        private static async Task AsyncTask(CancellationToken token, EventLog eventLog)
        {
            string configpath = Environment.ExpandEnvironmentVariables("%ProgramFiles%\\ADPasswordFilter\\config.json");
            Configuration configuration = Configuration.LoadFromFile(configpath);
            Logger logger = new Logger(configuration.LogFile);
            DB db = new DB(configuration.DatabaseFile);
            MidPoint mp = new MidPoint(configuration);

            do
            {
                foreach (Credentials c in db.GetAll())
                {
                    try {
                        mp.UpdateUserPasswordByName(c.Username, c.Password);
                        String log = String.Format("Credentials for user '{0}' successfully synchronized", c.Username);
                        logger.Log(log);
                        eventLog.WriteEntry(log, EventLogEntryType.SuccessAudit, 1000);
                        db.Remove(c);
                    }
                    catch (Exception e) {
                        String log = String.Format("Error synchronizing credentials for user '{0}': '{1}'", c.Username, e.ToString());
                        logger.Log(log);
                        eventLog.WriteEntry(log, EventLogEntryType.FailureAudit, 1001);
                    }
                }

                await Task.Delay(TimeSpan.FromMilliseconds(configuration.SyncInterval), token);
            }
            while (token.IsCancellationRequested != true);
        }

        private void RunTask()
        {
            cancellationTokenSource = new CancellationTokenSource();
            task = Task.Run(() =>
                AsyncTask(cancellationTokenSource.Token, EventLog)
                .ContinueWith(t =>
                    {
                        cancellationTokenSource.Dispose();
                        t.Dispose();
                    }
                )
            );
        }

        private void StopTask()
        {
            cancellationTokenSource.Cancel();
        }
    }
}
