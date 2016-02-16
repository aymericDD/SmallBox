using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WCFServiceWebRole1.Classes
{
    class ArchiveFile
    {
        /// <summary>
        /// Save CloudBlobDirectory in temp folder
        /// </summary>
        /// <param name="path"></param>
        /// <param name="blobDirectory"></param>
        public void tempDirectory(string path, CloudBlobDirectory blobDirectory)
        {
            // Init variable
            string initialPath = path;
            path = path + "/" + blobDirectory.Prefix;

            // Create directory
            Directory.CreateDirectory(path);

            // Save files
            foreach (CloudBlockBlob b in blobDirectory.ListBlobs().Where(b => b as CloudBlockBlob != null).ToList())
            {
                string filePath = (path + "/" + Path.GetFileName(b.Name));
                b.DownloadToFile(filePath, FileMode.Create);
            }

            // Save directories
            foreach (CloudBlobDirectory b in blobDirectory.ListBlobs().Where(b => b as CloudBlobDirectory != null).ToList())
            {
                this.tempDirectory(initialPath, b);
            }
        }

    }
}
