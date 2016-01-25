using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using S3.Common.Clients;

namespace S3Tool
{
    class Program
    {
        static void Main(string[] args)
        {
            //Parse command line options, show help if necessary.
            var options = new Options(); 
            if (!CommandLine.Parser.Default.ParseArguments(args, options))
            {
                PauseIf();
                Environment.Exit(CommandLine.Parser.DefaultExitCodeFail);
            }

            try
            {
                log4net.Config.XmlConfigurator.Configure(); //read logging configuration.

                switch (options.Action)
                {
                    case Options.eAction.s3Upload:
                        (new S3Client(Config.S3Bucket)).UploadFiles(options.LocalPath, options.TargetPath, options.Recurse);
                        break;

                    case Options.eAction.s3Download:
                        (new S3Client(Config.S3Bucket)).DownloadFiles(options.LocalPath, options.TargetPath);
                        break;

                    case Options.eAction.s3List:
                        (new S3Client(Config.S3Bucket)).ListS3();
                        break;
                }
            }
            catch (Exception ex)
            {
                string msg = string.Format("{0} Exception: {1}", ex.GetType().Name, ex.Message);
                if (options.ShowFullException)
                    Logger.log.Error(msg, ex);
                else
                    Logger.log.Error(msg);
            }

            PauseIf();
        }

        static void PauseIf()
        {
            if (System.Diagnostics.Debugger.IsAttached)
            {
                Console.WriteLine("Press any key to continue...");
                Console.ReadKey();
            }
        }
    }
}
