using System;
using System.Configuration;
using System.IO;

namespace EmploAdImport.Log
{
    internal class LoggerFactory
    {
        public static ILogger Instance { get; private set; }

        public static ILogger CreateLogger(string[] args)
        {
            var path = ConfigurationManager.AppSettings["LogFilePath"];

            if (ConfigurationManager.AppSettings["LogOutput"] == LogOutput.File.ToString()
                && !String.IsNullOrWhiteSpace(path))
            {
                if (!path.EndsWith("\\"))
                    path += "\\";

                CreateIfMissing(path);
                return Instance = new FileLogger(path + "EmploImportLog-" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".txt");
            }

            return Instance = new ConsoleLogger();
        }

        private static void CreateIfMissing(string path)
        {
            bool folderExists = Directory.Exists(path);
            if (!folderExists)
                Directory.CreateDirectory(path);
        }
    }
}
