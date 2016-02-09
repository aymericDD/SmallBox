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
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        List<String> ListFolders();

        [OperationContract]
        [WebGet(UriTemplate = "ListByFolder?folder={folder}", ResponseFormat = WebMessageFormat.Json)]
        List<String> ListByFolder(string folder);

        [OperationContract]
        [WebInvoke( Method = "POST", 
                    ResponseFormat = WebMessageFormat.Json, 
                    UriTemplate = "Upload/{path}")]
        Boolean Upload(String path, Stream  uploading);
        // TODO: ajoutez vos opérations de service ici
    }


    // Utilisez un contrat de données comme indiqué dans l'exemple ci-après pour ajouter les types composites aux opérations de service.
    [DataContract]
    public class CompositeType
    {
    }
}
