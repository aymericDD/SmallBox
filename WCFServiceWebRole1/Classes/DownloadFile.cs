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

        public DownloadFile(CloudBlobContainer cloudBlobContainer)
        {
            this.rootContainer = cloudBlobContainer;
        }

        public Stream GetStreamFile(string folder, string file)
        {
            MemoryStream ms = new MemoryStream();

            Stream returnStream = new MemoryStream();

            CloudBlockBlob blobBlock = this.rootContainer.GetDirectoryReference(folder).GetBlockBlobReference(file);

            if (blobBlock.Exists())
            {
                blobBlock.DownloadToStream(ms);
                
                ms.Seek(0, SeekOrigin.Begin);

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

        public Stream GetSteamZipFile(string folder, string file)
        {
            string[] splitFile = file.Split('.');
            string extension = splitFile[splitFile.Length - 1];

            if (extension == "zip")
            {
                CloudBlockBlob blobBlock = this.rootContainer.GetDirectoryReference(folder).GetBlockBlobReference(file);
            }
            return null;
        }

    }
}
