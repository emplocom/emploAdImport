using System;
using System.Linq;
using EmploAdImport.Log;

namespace EmploAdImport.Validators
{
    class CommandValidator
    {
        public static void ValidateUserCommand(string[] args)
        {
            if (!args.Any())
            {
                LoggerFactory.Instance.WriteLine(String.Format("No command provided"));
                Environment.Exit(-1);
            }

            var commandName = args[0];
            if (commandName != "block")
            {
                LoggerFactory.Instance.WriteLine(String.Format("Command {0} is not known. Supported commands: block", commandName));
                LoggerFactory.Instance.WriteLine(String.Format("Example: EmploAdImport.exe block NameId"));
                LoggerFactory.Instance.WriteLine(String.Format("NameId is the NameId attribute value of the user to block"));
                Environment.Exit(-1);
            }

            if (args.Count() == 1 || String.IsNullOrWhiteSpace(args[1]))
            {
                LoggerFactory.Instance.WriteLine("Command 'block' expects 1 parameter: NameId of the user to block");
                LoggerFactory.Instance.WriteLine(String.Format("Example: EmploAdImport.exe block NameId"));
                Environment.Exit(-1);
            }
        }        
    }
}