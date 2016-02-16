using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System.IO;
using System.IO.Compression;

namespace WorkerRole1
{
    public class WorkerRole : RoleEntryPoint
    {
        private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        private readonly ManualResetEvent runCompleteEvent = new ManualResetEvent(false);

        public override void Run()
        {
            Trace.TraceInformation("WorkerRole1 is running");

            try
            {
                this.RunAsync(this.cancellationTokenSource.Token).Wait();
            }
            finally
            {
                this.runCompleteEvent.Set();
            }
        }

        public override bool OnStart()
        {
            // Définir le nombre maximum de connexions simultanées
            ServicePointManager.DefaultConnectionLimit = 12;

            // Pour plus d'informations sur la gestion des modifications de configuration
            // consultez la rubrique MSDN à l'adresse http://go.microsoft.com/fwlink/?LinkId=166357.

            bool result = base.OnStart();

            Trace.TraceInformation("WorkerRole1 has been started");

            return result;
        }

        public override void OnStop()
        {
            Trace.TraceInformation("WorkerRole1 is stopping");

            this.cancellationTokenSource.Cancel();
            this.runCompleteEvent.WaitOne();

            base.OnStop();

            Trace.TraceInformation("WorkerRole1 has stopped");
        }

        private async Task RunAsync(CancellationToken cancellationToken)
        {
            // TODO: Remplacez le texte suivant par votre propre logique.
            while (!cancellationToken.IsCancellationRequested)
            {
                // Connect with development account.
                CloudStorageAccount storageAccount = CloudStorageAccount.DevelopmentStorageAccount;
                // Create the blob client.
                CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
                // Retrieve a reference to smallbox container.
                CloudBlobContainer rootContainer = blobClient.GetContainerReference("smallbox");
                
                string backupName = "backup";
                string backupZipName = "backup.zip";
                string path = Path.GetTempPath() + "/" + backupName;
                string zipPath = Path.GetTempPath() + "/" + backupZipName;

                Directory.CreateDirectory(path);

                foreach (CloudBlockBlob b in rootContainer.ListBlobs().Where(b => b as CloudBlockBlob != null).ToList())
                {
                    string filePath = (path + "/" + Path.GetFileName(b.Name));
                    b.DownloadToFile(filePath, FileMode.Create);
                }

                foreach (CloudBlobDirectory b in rootContainer.ListBlobs().Where(b => b as CloudBlobDirectory != null).ToList())
                {
                    this.tempDirectory(path, b);
                }

                CloudBlockBlob blobBackup = rootContainer
                                                    .GetDirectoryReference("backups")
                                                    .GetBlockBlobReference(backupName);

                // Zip directory if it exist
                if (!File.Exists(zipPath))
                {
                    ZipFile.CreateFromDirectory(path, zipPath);
                }

                blobBackup.UploadFromFile(zipPath, FileMode.Open);

                await Task.Delay(60000);
            }
        }

        private void tempDirectory(string path, CloudBlobDirectory blobDirectory)
        {   
            string initialPath = path;
            path = path + "/" + blobDirectory.Prefix;
            Directory.CreateDirectory(path);

            if (!blobDirectory.Prefix.Equals("backups/"))
            {
                foreach (CloudBlockBlob b in blobDirectory.ListBlobs().Where(b => b as CloudBlockBlob != null).ToList())
                {
                    string filePath = (path + "/" + Path.GetFileName(b.Name));
                    b.DownloadToFile(filePath, FileMode.Create);
                }

            }

            foreach (CloudBlobDirectory b in blobDirectory.ListBlobs().Where(b => b as CloudBlobDirectory != null).ToList())
            {
                this.tempDirectory(initialPath, b);
            }
        }
    }
}
