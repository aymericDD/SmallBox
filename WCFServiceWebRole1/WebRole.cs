using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Diagnostics;
using Microsoft.WindowsAzure.ServiceRuntime;

namespace WCFServiceWebRole1
{
    public class WebRole : RoleEntryPoint
    {
        public override bool OnStart()
        {
            // Pour plus d'informations sur la gestion des modifications de configuration
            // consultez la rubrique MSDN à l'adresse http://go.microsoft.com/fwlink/?LinkId=166357.

            return base.OnStart();
        }
    }
}
