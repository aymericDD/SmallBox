using Microsoft.WindowsAzure.Storage.Blob;
using System.IO;
using System.Linq;
using System.Web;

namespace WCFServiceWebRole1.Classes
{
    class ArchiveHelper
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

        /// <summary>
        /// Save folder in blob
        /// </summary>
        /// <param name="directory"></param>
        /// <param name="directoryBlob"></param>
        public void saveFolder(string directory, CloudBlobDirectory directoryBlob)
        {
            string directoryName = Path.GetFileName(directory);

            directoryBlob = directoryBlob.GetDirectoryReference(directoryName);

            string[] filePaths = Directory.GetFiles(directory);

            string[] directoryPaths = Directory.GetDirectories(directory);

            foreach (var filePath in filePaths)
            {
                CloudBlockBlob blockBlob = directoryBlob.GetBlockBlobReference(Path.GetFileName(filePath));

                blockBlob.Properties.ContentType = MimeMapping.GetMimeMapping(Path.GetFileName(filePath));

                // Save file into block blob
                blockBlob.UploadFromFile(filePath, FileMode.Open);
            }

            foreach (string dir in directoryPaths)
            {
                this.saveFolder(dir, directoryBlob);
            }

        }

    }
}
