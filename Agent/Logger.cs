using System;
using System.IO;

namespace Agent
{
    class Logger
    {
        string path;
        public Logger(string path)
        {
            this.path = Environment.ExpandEnvironmentVariables(path);
        }

        public void Log(string message)
        {
            Console.WriteLine("[{0}] {1}", DateTime.Now, message);
            using (StreamWriter logger = File.AppendText(this.path))
            {
                logger.WriteLine(String.Format("[{0}] {1}", DateTime.Now, message));
            }
        }
    }
}