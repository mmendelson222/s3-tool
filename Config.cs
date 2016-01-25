using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace S3Tool
{
    internal static class Config
    {
        internal static Amazon.RegionEndpoint AWSRegion
        {
            get
            {
                return Amazon.RegionEndpoint.GetBySystemName(GetRequiredValue("S3Tool.AwsRegion"));
            }
        }

        private static string s3bucket = null;
        internal static string S3Bucket
        {
            get
            {
                if (s3bucket == null)
                    s3bucket = GetRequiredValue("S3Tool.S3Bucket");
                return s3bucket;
            }
        }

        private static bool? lower;
        internal static bool LowerCaseEnforce
        {
            get
            {
                if (lower == null)
                    lower = GetValueB("S3Tool.LowerCaseDirectories", true);
                return (bool)lower;
            }
        }

        #region private helper functions
        private static string GetRequiredValue(string key)
        {
            string val = ConfigurationManager.AppSettings[key.Trim()];
            if (val == null)
                throw new Exception("Configuration error: Add a setting for key " + key + " to web.config.  Thank you.");
            else
                return val;
        }

        private static bool GetValueB(string key, bool defaultValue)
        {
            string val = ConfigurationManager.AppSettings[key];
            bool opt;
            if (val == null) opt = defaultValue;
            else opt = ParseBool(val, key);
            return opt;
        }

        private static bool ParseBool(string val, string key)
        {
            if (string.Compare(val, "true", true) == 0)
                return true;
            else if (string.Compare(val, "false", true) == 0)
                return false;
            else
                throw new Exception(string.Format("True/False value expected for configuration option {0}", key));
        } 
        #endregion
    }
}
