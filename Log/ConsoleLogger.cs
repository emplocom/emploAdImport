using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmploAdImport.Log
{
    class ConsoleLogger : BaseLogger
    {
        public override void WriteLine(string line)
        {
            Console.WriteLine(PrependTimeToOutputLine(line));
        }
    }
}
