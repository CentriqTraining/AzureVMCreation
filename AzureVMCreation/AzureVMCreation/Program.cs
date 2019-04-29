using AzureManagement;
using Microsoft.Azure.Management.Compute.Fluent.Models;
using Microsoft.Azure.Management.ResourceManager.Fluent.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureVMCreation
{
    class Program
    {
        static void Main(string[] args)
        {
            var az = new AzureAcct();
            var rg = az.CreateResourceGroup("AzureCentriqDemo", Region.USNorthCentral);
            az.CreateVM("Bubba2", Region.USNorthCentral, "AzureCentriqDemo", VirtualMachineSizeTypes.BasicA0);
        }
    }
}
