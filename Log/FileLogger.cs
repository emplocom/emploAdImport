using System.Collections.Generic;
using System.IO;

namespace EmploAdImport.Log
{
    class FileLogger : BaseLogger
    {
        private readonly string _filePath;

        public FileLogger(string filePath)
        {
            _filePath = filePath;
        }

        public override void WriteLine(string line)
        {
            File.AppendAllLines(_filePath, new List<string> { PrependTimeToOutputLine(line) });
        }
    }
}