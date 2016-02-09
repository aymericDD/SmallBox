using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using System.IO;
using WCFServiceWebRole1.Classes;

namespace WCFServiceWebRole1
{
    // REMARQUE : vous pouvez utiliser la commande Renommer du menu Refactoriser pour changer le nom de classe "Service1" dans le code, le fichier svc et le fichier de configuration.
    // REMARQUE : pour lancer le client test WCF afin de tester ce service, sélectionnez Service1.svc ou Service1.svc.cs dans l'Explorateur de solutions et démarrez le débogage.
    public class Service1 : IService1
    {
        private CloudBlobClient blobClient;
        private CloudBlobContainer rootContainer;

        /// <summary>
        /// Constructor
        /// </summary>
        public Service1()
        {
            // Connect with development account.
            CloudStorageAccount storageAccount = CloudStorageAccount.DevelopmentStorageAccount;
            // Create the blob client.
            this.blobClient = storageAccount.CreateCloudBlobClient();
            // Retrieve a reference to smallbox container.
            this.rootContainer = blobClient.GetContainerReference("smallbox");
        }
        
        /// <summary>
        /// Return all files in a specific folder
        /// </summary>
        /// <param name="folder">String directory to find</param>
        /// <returns>Return all files in the derectory</returns>
        public List<string> ListByFolder(string folder)
        {
            // Get all objects in the specified directory
            var list = this.rootContainer
                            .GetDirectoryReference(folder)
                            .ListBlobs();

            // Get only file in list
            var listFiles = list.Where(b => b as CloudBlockBlob != null)
                            .Select(b => (b as CloudBlockBlob).Name)
                            .ToList();

            // Get only directory in list
            var listDirectories = list.Where(b => b as CloudBlobDirectory != null)
                                .Select(b => (b as CloudBlobDirectory).Prefix)
                                .ToList();
            
            // Merge two lists and return it
            return new List<string>()
                                    .Concat(listFiles)
                                    .Concat(listDirectories)
                                    .ToList();
        }

        /// <summary>
        /// Return all folders in the first level in root folder
        /// </summary>
        /// <returns>List root folders</returns>
        public List<String> ListFolders()
        {
            return this.rootContainer
                            .ListBlobs(null, false, BlobListingDetails.None)    // Get list blobs
                            .Where(b => b as CloudBlobDirectory != null)        // Get only directories
                            .Select(b => (b as CloudBlobDirectory).Prefix)      // Get prefix(name) directories
                            .ToList();                                          // Convert to list and return result
        }

        public bool Upload(string path, Stream stream)
        {
            if (path.Length > 1)
            {
                ParserFile parserFIle = new ParserFile(stream);
                if (parserFIle.Success)
                {
                    try
                    {
                        // Create temp file
                        string tempFile = Path.GetTempPath() + parserFIle.FileName;
                        using (System.IO.FileStream output = new System.IO.FileStream(tempFile, FileMode.Create))
                        {
                            output.Write(parserFIle.FileContents, 0, parserFIle.FileContents.Count());
                        }

                        // Create
                        CloudBlockBlob blockBlob = this.rootContainer
                                    .GetDirectoryReference(path)
                                    .GetBlockBlobReference(parserFIle.FileName);

                        // Set content type
                        blockBlob.Properties.ContentType = parserFIle.FileType;

                        // Save content file
                        blockBlob.UploadFromFile(tempFile, FileMode.Open);
                        return true;
                    }
                    catch (Exception)
                    {
                        return false;
                    }
                }
                
            }
            return false;

            // Do whatever you want with the file here
            
        }
    }
}
 