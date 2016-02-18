using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.ServiceModel.Web;
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
        /// </summary>
        /// <param name="folder">Folder where get file</param>
        /// <param name="file">File to download</param>
        /// <returns>Return stream file</returns>
        public Stream GetStreamFile(string folder, string file)
        {
            try
            {
                // Get file
                CloudBlockBlob blobBlock = this.rootContainer
                                                    .GetDirectoryReference(folder)
                                                    .GetBlockBlobReference(file);

                if (blobBlock.Exists())
                {
                    Stream returnStream = new MemoryStream();

                    blobBlock.DownloadToStream(returnStream);

                    returnStream.Seek(0, SeekOrigin.Begin);

                    WebOperationContext.Current.OutgoingResponse.Headers["Content-Disposition"] = "attachment; filename=" + blobBlock.Uri.Segments.Last();
                    WebOperationContext.Current.OutgoingResponse.ContentType = blobBlock.Properties.ContentType;

                    return returnStream;
                }
            }
            catch (Exception)
            {
                return null;
            }
            return null;
        }

    }
}
