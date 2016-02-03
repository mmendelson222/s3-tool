using CommandLine;
using CommandLine.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace S3Tool
{
    internal class Options
    {
        public enum eAction { s3Upload, s3Download, s3List, signed }
        [Option('a', "action", HelpText = "Action: s3Upload, s3Download, s3List", Required=true)]
        public eAction Action { get; set; }

        [Option('l', "local", HelpText = "Local directory path for upload or download.")]
        public string LocalPath { get; set; }

        [Option('t', "target", HelpText = "S3 Target path (subdirectory within bucket) upload or download.", Required = false)]
        public string TargetPath { get; set; }

        [Option('r', "recurse", HelpText = "Recurse directories.  Only applies to uploading.", DefaultValue = false)]
        public bool Recurse { get; set; }

        [Option('e', "exceptions", HelpText = "Exception logging show stack trace.  Affects logs and console messages.", DefaultValue = false)]
        public bool ShowFullException { get; set; }

        [HelpOption]
        public string GetUsage()
        {
            HelpText ht =  HelpText.AutoBuild(this, current => HelpText.DefaultParsingErrorsHandler(this, current));

            return string.Format("{0}\r\n{1}",
                ht.ToString().Replace("\r\n\r\n", "\r\n"),
                ADDITIONAL_HELP);
        }

        const string ADDITIONAL_HELP = @"
    Examples:
    Copy all files recursively to the root of the s3 bucket.
    s3tool -l %filesource% -r 

    Copy all files recursively to a folder within the s3 bucket.
    s3tool -l %filesource% -r -t %s3folder%

    Report any bugs to: me@smartronix.com

";

    }
}
