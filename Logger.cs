using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace S3Tool
{
    internal static class Logger
    {
        internal static readonly ILog log = LogManager.GetLogger(typeof(Program));
    }
}
