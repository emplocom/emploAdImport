using System.Linq;
using EmploAdImport.Validators;
using System;
using EmploApiSDK.Logger;

namespace EmploAdImport
{
    class Program
    {
        static void Main(string[] args)
        {
            ILogger logger = LoggerFactory.CreateLogger(args);
            logger.WriteLine("Import to emplo started");

            SettingsValidator.ValidateSettings();

            try
            {
                var importLogic = new Importer.EmployeeAdImportLogic(logger);
                if (args.Any())
                {
                    CommandValidator.ValidateUserCommand(args);
                    var nameId = args[1];
                    importLogic.BlockUser(nameId);
                }
                else
                {
                    importLogic.ImportUsers();
                }
            }
            catch (Exception ex)
            {
                logger.WriteLine(ex.Message);
                throw ex;
            }           
        }
    }
}
