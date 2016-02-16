using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WCFServiceWebRole1.Classes
{
    class DownloadFile
    {
        private CloudBlobContainer rootContainer;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="cloudBlobContainer">Get the reference container</param>
        public DownloadFile(CloudBlobContainer cloudBlobContainer)
        {
            this.rootContainer = cloudBlobContainer;
        }

        /// <summary>
        /// Get stream file in blob block
        /// Extract the file if is a zip file
        /// </summary>
        /// <param name="folder">Folder where get file</param>
        /// <param name="file">File to download</param>
        /// <returns>Return stream file</returns>
        public Stream GetStreamFile(string folder, string file)
        {
            MemoryStream ms = new MemoryStream();

            Stream returnStream = new MemoryStream();

            // Get file
            CloudBlockBlob blobBlock = this.rootContainer
                                                .GetDirectoryReference(folder)
                                                .GetBlockBlobReference(file);
            
            if (blobBlock.Exists())
            {
                blobBlock.DownloadToStream(ms);
                
                ms.Seek(0, SeekOrigin.Begin);

                // Extract file if it exist
                if (blobBlock.Properties.ContentType.Equals("application/x-zip-compressed"))
                {
                    ZipArchive archive = new ZipArchive(ms);

                    foreach (ZipArchiveEntry entry in archive.Entries)
                    {
                        returnStream = entry.Open();
                    }

                } else
                {
                    returnStream = ms;
                }
            }
            return returnStream;
        }

    }
}
