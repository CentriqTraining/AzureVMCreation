using Microsoft.Azure.Management.AppService.Fluent;
using Microsoft.Azure.Management.Compute.Fluent;
using Microsoft.Azure.Management.Compute.Fluent.Models;
using Microsoft.Azure.Management.Fluent;
using Microsoft.Azure.Management.Network.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent.Authentication;
using Microsoft.Azure.Management.ResourceManager.Fluent.Core;
using Microsoft.Azure.Management.Storage.Fluent;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureManagement
{
    public class AzureAcct
    {
        private IAzure _Azure;
        public AzureAcct()
        {
            var credentials = SdkContext.AzureCredentialsFactory.FromFile(@"../../Auth/my.auth");
            _Azure = Azure
                    .Configure()
                    .WithLogLevel(HttpLoggingDelegatingHandler.Level.Basic)
                    .Authenticate(credentials)
                    .WithDefaultSubscription();
        }

        #region Virtual Machines
        public IEnumerable<IVirtualMachine> GetVirtualMachines()
        {
            return _Azure.VirtualMachines.List();
        }
        public void CreateVM(string machineName, Region region, string groupName, VirtualMachineSizeTypes size)
        {
            var network = CreateVirtualNetwork(machineName.ToLower() + "-vnet", region, groupName, "10.0.0.0/16");
            var iP = CreateIPAddress(machineName.ToLower() + "ip", region, groupName);
            var Nic = CreateNetworkInterface(machineName.ToLower() + "nic",
                                region,
                                GetResourceGroups().First(g => g.Name == groupName),
                                network,
                                iP);

            CreateVM(machineName, region,
                "Centriq", Nic, size);


        }
        public void CreateVM(string machineName,
                            Region region,
                            string groupName,
                            INetworkInterface networkInterface,
                            VirtualMachineSizeTypes size)
        {
            _Azure.VirtualMachines.Define(machineName)
                .WithRegion(region)
                .WithExistingResourceGroup(groupName)
                .WithExistingPrimaryNetworkInterface(networkInterface)
                .WithLatestWindowsImage("MicrosoftWindowsServer", "WindowsServer", "2012-R2-Datacenter")
                .WithAdminUsername("azureuser")
                .WithAdminPassword("Azure12345678")
                .WithComputerName(machineName)
                .WithSize(size)
                .Create();
        }
        #endregion
        #region VHDs
        public IEnumerable<IDisk> GetVHDs()
        {
            return _Azure.Disks.List();
        }
        #endregion
        #region Web Apps
        public IEnumerable<IWebApp> GetWebApps()
        {
            return _Azure.WebApps.List();
        }
        public void UpdateWebAppToExclusiveTier(IWebApp app, PricingTier tier)
        {
            var App = _Azure.WebApps.GetById(app.Id);
            var settings = App
                .Update()
                .WithNewAppServicePlan(tier)
                .Apply();
        }
        public void UpdateWebAppToSharedTier(IWebApp app, PricingTier tier)
        {
            var App = _Azure.WebApps.GetById(app.Id);
            App.Update()
                .WithNewSharedAppServicePlan()
                .Apply();

        }
        public void UpdateWebAppToFreeTier(IWebApp app)
        {
            var App = _Azure.WebApps.GetById(app.Id);
            App.Update()
                .WithNewFreeAppServicePlan()
                .Apply();
        }
        public void CreateWebApp(string appName, Region region, IResourceGroup resourceGroup)
        {
            _Azure.WebApps.Define(appName)
                 .WithRegion(region)
                 .WithExistingResourceGroup(resourceGroup)
                 .WithNewFreeAppServicePlan()
                 .Create();
        }
        #endregion
        #region Storage Accounts
        public IStorageAccount CreateGeneralPurposeStorageAccountV2(string accountName, Region region, IResourceGroup resourceGroup)
        {
            return _Azure.StorageAccounts.Define(accountName)
                .WithRegion(region)
                .WithExistingResourceGroup(resourceGroup)
                .WithGeneralPurposeAccountKindV2()
                .Create();
        }
        public IStorageAccount CreateGeneralPurposeStorageAccount(string accountName, Region region, IResourceGroup resourceGroup)
        {
            return _Azure.StorageAccounts.Define(accountName)
                .WithRegion(region)
                .WithExistingResourceGroup(resourceGroup)
                .WithGeneralPurposeAccountKind()
                .Create();
        }
        public IStorageAccount CreateBlobStorageAccount(string accountName, Region region, IResourceGroup resourceGroup)
        {
            return _Azure.StorageAccounts.Define(accountName)
                .WithRegion(region)
                .WithExistingResourceGroup(resourceGroup)
                .WithBlobStorageAccountKind()
                .Create();
        }
        #endregion
        #region Resource Groups
        public IEnumerable<IResourceGroup> GetResourceGroups()
        {
            return _Azure.ResourceGroups.List();
        }
        public IResourceGroup CreateResourceGroup(string resourceGroupName,
                                            Region regionName)
        {
            return _Azure.ResourceGroups
                .Define(resourceGroupName)
                .WithRegion(regionName)
                .Create();
        }
        #endregion
        #region Availability Sets
        public IEnumerable<IAvailabilitySet> GetAvailabilitySets()
        {
            return _Azure.AvailabilitySets.List();
        }
        public IAvailabilitySet CreateAvailabilitySet(string availabilitySetName,
                                            Region regionName,
                                            string resourceGroupName)
        {
            return _Azure.AvailabilitySets.Define(availabilitySetName)
                .WithRegion(regionName)
                .WithExistingResourceGroup(resourceGroupName)
                .WithSku(AvailabilitySetSkuTypes.Managed)
                .Create();
        }
        #endregion
        #region Public IP Addresses
        public IEnumerable<IPublicIPAddress> GetPublicIPAddresses()
        {
            return _Azure.PublicIPAddresses.List();
        }
        public IPublicIPAddress CreateIPAddress(string ipAddressName,
                                        Region regionName,
                                        string resourceGroupName)
        {
            return _Azure.PublicIPAddresses.Define(ipAddressName)
                .WithRegion(regionName)
                .WithExistingResourceGroup(resourceGroupName)
                .WithDynamicIP()
                .Create();
        }
        #endregion
        #region Virtual Networks
        public IEnumerable<INetwork> GetVirtualNetworks()
        {
            return _Azure.Networks.List();
        }
        public INetwork CreateVirtualNetwork(string vnetName,
                                    Region regionName,
                                    string resourceGroupName,
                                    string addressSpace)
        {
            Console.WriteLine("Creating virtual network...");
            return _Azure.Networks.Define(vnetName)
                .WithRegion(regionName)
                .WithExistingResourceGroup(resourceGroupName)
                .WithAddressSpace("10.0.0.0/16")
                .WithSubnet("sub1", "10.0.0.0/24")
                .Create();
        }
        #endregion
        #region Network Interfaces
        public IEnumerable<INetworkInterface> GetNetworkInterfaces()
        {
            return _Azure.NetworkInterfaces.List();
        }
        public INetworkInterface CreateNetworkInterface(string nicName,
                    Region regionName,
                    IResourceGroup group,
                    INetwork network,
                    IPublicIPAddress IPAddress)
        {
            Console.WriteLine("Creating network interface...");
            return _Azure.NetworkInterfaces.Define(nicName)
                .WithRegion(regionName)
                .WithExistingResourceGroup(group)
                .WithExistingPrimaryNetwork(network)
                .WithSubnet("sub1")
                .WithPrimaryPrivateIPAddressDynamic()
                .WithExistingPrimaryPublicIPAddress(IPAddress)
                .Create();
        }
        #endregion

        #region Asynchronous Methods
        public async Task<IPagedCollection<IResourceGroup>> GetResourceGroupsAsync()
        {
            return await _Azure.ResourceGroups.ListAsync();
        }
        public async Task<IResourceGroup> CreateResourceGroupAsync(string resourceGroupName,
                                                                Region regionName)
        {
            return await _Azure.ResourceGroups
                .Define(resourceGroupName)
                .WithRegion(regionName)
                .CreateAsync();

        }
        public async Task<IAvailabilitySet> CreateAvailabilitySetAsync(string availabilitySetName,
                                            Region regionName,
                                            string resourceGroupName)
        {
            return await _Azure.AvailabilitySets.Define(availabilitySetName)
                .WithRegion(regionName)
                .WithExistingResourceGroup(resourceGroupName)
                .WithSku(AvailabilitySetSkuTypes.Managed)
                .CreateAsync();
        }

        public async Task<IPublicIPAddress> CreateIPAddressAsync(string ipAddressName,
                                    Region regionName, string resourceGroupName)
        {
            return await _Azure.PublicIPAddresses.Define(ipAddressName)
                .WithRegion(regionName)
                .WithExistingResourceGroup(resourceGroupName)
                .WithDynamicIP()
                .CreateAsync();
        }

        public async Task<INetwork> CreateVirtualNetworkAsync(string vnetName,
                                    Region regionName,
                                    string resourceGroupName,
                                    string addressSpace)
        {
            Console.WriteLine("Creating virtual network...");
            return await _Azure.Networks.Define(vnetName)
                .WithRegion(regionName)
                .WithExistingResourceGroup(resourceGroupName)
                .WithAddressSpace("10.0.0.0/16")
                .WithSubnet("mySubnet", "10.0.0.0/24")
                .CreateAsync();
        }
        public async Task<INetworkInterface> CreateNetworkInterfaceAsync(string nicName,
            Region regionName,
            IResourceGroup group,
            INetwork network,
            IPublicIPAddress IPAddress)
        {
            Console.WriteLine("Creating network interface...");
            return await _Azure.NetworkInterfaces.Define(nicName)
                .WithRegion(regionName)
                .WithExistingResourceGroup(group)
                .WithExistingPrimaryNetwork(network)
                .WithSubnet("sub1")
                .WithPrimaryPrivateIPAddressDynamic()
                .WithExistingPrimaryPublicIPAddress(IPAddress)
                .CreateAsync();
        }
        #endregion
    }
}
