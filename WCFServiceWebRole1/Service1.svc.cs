using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.IO;
using WCFServiceWebRole1.Classes;
using System.IO.Compression;

namespace WCFServiceWebRole1
{
    // REMARQUE : vous pouvez utiliser la commande Renommer du menu Refactoriser pour changer le nom de classe "Service1" dans le code, le fichier svc et le fichier de configuration.
    // REMARQUE : pour lancer le client test WCF afin de tester ce service, sélectionnez Service1.svc ou Service1.svc.cs dans l'Explorateur de solutions et démarrez le débogage.
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class Service1 : IService1
    {
        private CloudBlobClient blobClient;
        private CloudBlobContainer rootContainer;
        private DownloadFile downloadFile;
        private ArchiveFile archiveFile;

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
            
            this.downloadFile = new DownloadFile(this.rootContainer);

            this.archiveFile = new ArchiveFile();
        }
        
        /// <summary>
        /// Return all files and directories in a specific folder
        /// </summary>
        /// <param name="folder">String directory to find</param>
        /// <returns>Return all files and directories in the derectory</returns>
        public List<string> ListAllByFolder(string folder)
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
        /// Return all files in a specific folder
        /// </summary>
        /// <param name="folder">String directory to find</param>
        /// <returns>Return all files in the derectory</returns>
        public List<string> ListFilesByFolder(string folder)
        {
            return this.rootContainer
                            .GetDirectoryReference(folder)
                            .ListBlobs()
                            .Where(b => b as CloudBlockBlob != null)
                            .Select(b => (b as CloudBlockBlob).Name)
                            .ToList();
        }

        /// <summary>
        /// Return all directories in a specific folder
        /// </summary>
        /// <param name="folder">String directory to find</param>
        /// <returns>Return all direcectories in the derectory</returns>
        public List<string> ListDirectoriesByFolder(string folder)
        {
            return this.rootContainer
                            .GetDirectoryReference(folder)
                            .ListBlobs()
                            .Where(b => b as CloudBlobDirectory != null)
                            .Select(b => (b as CloudBlobDirectory).Prefix)
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

        /// <summary>
        /// Upload file in a specified folder
        /// </summary>
        /// <param name="path">Fodler</param>
        /// <param name="stream">Filder</param>
        /// <returns>True if created else false</returns>
        public bool Upload(string path, Stream stream)
        {
            if (path != null && path.Length >= 1)
            {
                ParserFile parserFIle = new ParserFile();
                parserFIle.Parse(stream);

                if (parserFIle.Success)
                {
                    try
                    {
                        // Create temp file
                        string tempFile = Path.GetTempPath() + parserFIle.FileName;

                        // Write content stream in temp file
                        using (System.IO.FileStream output = new System.IO.FileStream(tempFile, FileMode.Create))
                        {
                            output.Write(parserFIle.FileContents, 0, parserFIle.FileContents.Count());
                        }

                        // Create block blob
                        CloudBlockBlob blockBlob = this.rootContainer
                                    .GetDirectoryReference(path)
                                    .GetBlockBlobReference(parserFIle.FileName);

                        // Set block blob content type
                        blockBlob.Properties.ContentType = parserFIle.FileType;

                        // Save file into block blob
                        blockBlob.UploadFromFile(tempFile, FileMode.Open);

                        // Return success save
                        return true;
                    }
                    catch (Exception)
                    {
                        return false;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Get file by path and file name
        /// </summary>
        /// <param name="folder">Folder where find file</param>
        /// <param name="file">File to download</param>
        /// <returns>Stream file</returns>
        public Stream GetFile(string folder, string file)
        {
            return this.downloadFile.GetStreamFile(folder, file);
        }

        /// <summary>
        /// Archive directory
        /// </summary>
        /// <param name="folder"></param>
        /// <returns>Ture if archived else false</returns>
        public bool ArchiveDirectory(string folder)
        {
            try
            {
                // Get directory
                CloudBlobDirectory directory = this.rootContainer.GetDirectoryReference(folder);
                
                if (directory.ListBlobs().Count() <= 0)
                {
                    return false;
                }

                // Init paths
                string zipName = directory.Prefix + ".zip";
                zipName = zipName.Replace("/", "");
                string zipPath = Path.GetTempPath() + zipName;
                string path = Path.GetTempPath();

                // Save CloudBlobDirectory into temp directoy
                this.archiveFile.tempDirectory(path, directory);

                // Zip directory if it exist
                if (!File.Exists(zipPath))
                {
                    ZipFile.CreateFromDirectory(path + "/" + directory.Prefix, zipPath);
                }

                // Create archive directory if does not exist and create blob zip
                CloudBlockBlob blobZip = this.rootContainer
                                                    .GetDirectoryReference("archives")
                                                    .GetBlockBlobReference(zipName);

                // Save content
                blobZip.UploadFromFile(zipPath, FileMode.Open);
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }
        
    }
}
 