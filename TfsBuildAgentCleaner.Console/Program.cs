using System;
using log4net;

namespace TfsBuildAgentCleaner.Console
{
    class Program
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        static void Main()
        {
            try
            {
                log4net.Config.XmlConfigurator.Configure();

                var cleaner = new BuildAgentCleaner.Tfs.FileSystemCleaner();
                cleaner.Clean();
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }
            System.Console.ReadLine();
        }
    }
}
