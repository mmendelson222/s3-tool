using Amazon.S3;
using Amazon.S3.Model;
using S3Tool;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace S3.Common.Clients
{
    internal class S3Client : IDisposable
    {
        private IAmazonS3 _client;
        private string _bucketName;

        internal S3Client(string bucketName)
        {
            this._client = new AmazonS3Client(S3Tool.Config.AWSRegion);
            this._bucketName = bucketName;
        }

        /// <summary>
        /// upload either a single file OR an entire directory. 
        /// if directory, can recurse.
        /// </summary>
        internal void UploadFiles(string localSource, string s3directory, bool recurse)
        {
            if (System.IO.File.Exists(localSource))
            {
                Logger.log.Info("Uploading file " + localSource);
                UploadFile(localSource, s3directory);
            }
            else if (System.IO.Directory.Exists(localSource))
            {
                //start with standardized directory name.
                localSource = (new DirectoryInfo(localSource)).FullName;
                UploadFiles_internal(localSource, s3directory, recurse);
            }
            else
            {
                throw new Exception("Invalid local file or directory: " + localSource);
            }
        }

        private void UploadFiles_internal(string localDir, string startingS3Dir, bool recurse)
        {
            //if recursing, make sure we're setting the correct s3 key (i.e. file path) for each file.
            Logger.log.Info("Reading files from " + localDir);
            var files = new DirectoryInfo(localDir).EnumerateFiles();
            foreach (var file in files)
            {
                UploadFile(file.FullName, startingS3Dir);
            }

            if (recurse)
            {
                foreach (var dir in new DirectoryInfo(localDir).EnumerateDirectories())
                {
                    UploadFiles_internal(dir.FullName, startingS3Dir + dir.FullName.Substring(localDir.Length), recurse);
                }
            }
        }

        private bool UploadFile(string filePath, string s3directory)
        {
            //clean up s3 directory
            s3directory = s3directory.Replace('\\', '/');
            if (s3directory.StartsWith("/")) s3directory = s3directory.Substring(1);
            if (Config.LowerCaseEnforce) s3directory = s3directory.ToLower();

            var key = string.Format("{0}{1}{2}", s3directory, (string.IsNullOrEmpty(s3directory) ? null : "/"), Path.GetFileName(filePath));

            PutObjectRequest request = new PutObjectRequest()
            {
                BucketName = _bucketName,
                Key = key,
                FilePath = filePath,
            };

            PutObjectResponse response = this._client.PutObject(request);

            bool success = (response.HttpStatusCode == System.Net.HttpStatusCode.OK);
            if (success)
                Logger.log.Info("Successfully uploaded file " + filePath + " to " + key);
            else
                Logger.log.Info("Failed to upload file " + filePath);
            return success;
        }

        /// <summary>
        /// This does not support individual files.  Directories only. 
        /// </summary>
        internal void DownloadFiles(string downloadLocation, string s3directory)
        {
            try
            {
                if (s3directory == null)
                    s3directory = string.Empty;
                else if (!s3directory.EndsWith("/"))
                    s3directory += "/";

                ListObjectsRequest listRequest = new ListObjectsRequest()
                {
                    BucketName = _bucketName,
                    Prefix = s3directory
                };

                ListObjectsResponse listResponse = _client.ListObjects(listRequest);

                if (!downloadLocation.EndsWith("\\")) downloadLocation += "\\";

                //result will be recursive.
                foreach (S3Object obj in listResponse.S3Objects)
                {
                    //Logger.log.Debug("Inspecting " + obj.Key);

                    //listed item is within a folder, so skip it.
                    if (obj.Key.Substring(s3directory.Length).Contains('/')) continue;

                    //s3 will list the directory name as an object, which causes an error when you try to download it.
                    if (s3directory == obj.Key) continue;

                    try
                    {
                        Logger.log.Debug("Downloading " + obj.Key + " " + downloadLocation);
                        DownloadFile(obj.Key, downloadLocation, true);
                    }
                    catch (Exception ex)
                    {
                        //There is an error on creating folder with empty file.  Need to evaluate
                        Logger.log.Error(ex);
                    }
                }
            }
            catch (Exception e)
            {
                throw new Exception(e.Message + " " + downloadLocation);
            }
        }

        private bool DownloadFile(string key, string downloadLocation, bool omitPath)
        {
            GetObjectRequest request = new GetObjectRequest()
            {
                BucketName = _bucketName,
                Key = key
            };

            using (GetObjectResponse response = this._client.GetObject(request))
            {

                if (omitPath && key.Contains('/'))
                    key = key.Substring(key.LastIndexOf('/'));

                string fullFileNamePath = downloadLocation + key;

                response.WriteResponseStreamToFile(fullFileNamePath);

                if (response.HttpStatusCode == System.Net.HttpStatusCode.OK)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// get a list of all items -> to log
        /// </summary>
        internal void ListS3()
        {
            var objectList = GetS3ObjectList();
            foreach (var o in objectList)
            {
                Logger.log.Info(o.Key);
            }
        }

        private List<S3Object> GetS3ObjectList()
        {
            ListObjectsRequest request = new ListObjectsRequest()
            {
                BucketName = this._bucketName
            };

            Logger.log.InfoFormat("Listing contents of bucket {0}", this._bucketName);

            var response = _client.ListObjects(request);

            var objectList = response.S3Objects;
            return objectList;
        }

        /// <summary>
        /// git the first item in the list, and return a signed url for it, 
        /// which expires in 5 minutes.
        /// </summary>
        internal void SigningDemo()
        {
            var objectList = GetS3ObjectList();
            var s3obj = objectList[0];

            GetPreSignedUrlRequest urlRequest = new GetPreSignedUrlRequest()
            {
                BucketName = this._bucketName,
                Key = s3obj.Key,
                Expires = DateTime.Now.AddMinutes(5)
            };

            string url = _client.GetPreSignedURL(urlRequest);
            Console.WriteLine(url);
        }

        [Obsolete("in progress.")]
        private void Publish(string stagingFolder, string fileName, string publishFolder, string bucketName)
        {
            fileName = System.IO.Path.GetFileName(fileName);
            CopyObjectRequest copyRequest = new CopyObjectRequest()
            {
                SourceBucket = bucketName,
                SourceKey = stagingFolder + "/" + fileName,
                DestinationBucket = bucketName,
                DestinationKey = publishFolder + "/" + fileName
            };
            CopyObjectResponse result = _client.CopyObject(copyRequest);
            //TODO: inspect result.
        }

        [Obsolete("in progress.")]
        private bool MoveToArchive(string fileName, string archiveLocation, string bucketName)
        {
            CopyObjectRequest copyRequest = new CopyObjectRequest()
            {
                SourceBucket = bucketName,
                SourceKey = fileName,
                DestinationBucket = bucketName,
                DestinationKey = archiveLocation + fileName
            };

            CopyObjectResponse copyResponse = _client.CopyObject(copyRequest);

            if (copyResponse.HttpStatusCode == System.Net.HttpStatusCode.OK)
            {
                DeleteObjectRequest deleteRequest = new DeleteObjectRequest()
                {
                    BucketName = bucketName,
                    Key = fileName
                };

                DeleteObjectResponse deleteResponse = _client.DeleteObject(deleteRequest);

                if (deleteResponse.HttpStatusCode == System.Net.HttpStatusCode.NoContent)
                {
                    return true;
                }
            }

            return false;
        }

        public void Dispose()
        {
            this._client.Dispose();
        }

    }
}
