using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;

namespace WCFServiceWebRole1
{
    // REMARQUE : vous pouvez utiliser la commande Renommer du menu Refactoriser pour changer le nom d'interface "IService1" à la fois dans le code et le fichier de configuration.
    [ServiceContract]
    public interface IService1
    {

        [OperationContract]
        [WebGet(    ResponseFormat = WebMessageFormat.Json)]
        List<String> ListFolders();

        [OperationContract]
        [WebGet(    UriTemplate = "ListAllByFolder?folder={folder}", 
                    ResponseFormat = WebMessageFormat.Json)]
        List<String> ListAllByFolder(string folder);
        
        [OperationContract]
        [WebGet(    UriTemplate = "ListFilesByFolder?folder={folder}", 
                    ResponseFormat = WebMessageFormat.Json)]
        List<String> ListFilesByFolder(string folder);

        [OperationContract]
        [WebGet(    UriTemplate = "ListDirectoriesByFolder?folder={folder}",
                    ResponseFormat = WebMessageFormat.Json)]
        List<String> ListDirectoriesByFolder(string folder);

        [OperationContract]
        [WebInvoke( Method = "POST", 
                    UriTemplate = "Upload?path={path}", 
                    ResponseFormat = WebMessageFormat.Json)]
        Boolean Upload(String path, Stream  uploading);

        [OperationContract]
        [WebGet(    UriTemplate = "GetFile?folder={folder}&file={file}",
                    ResponseFormat = WebMessageFormat.Json)]
        Stream GetFile(string folder, string file);

        [OperationContract]
        [WebGet(    UriTemplate= "ArchiveDirectory?folder={folder}",
                    ResponseFormat = WebMessageFormat.Json)]
        Boolean ArchiveDirectory(string folder);

    }

}
