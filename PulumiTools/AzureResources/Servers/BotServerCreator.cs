using Pulumi;
using Compute = Pulumi.AzureNative.Compute;

namespace PulumiTools.AzureResources.Servers
{
    /// <summary>
    /// Class for creating Azure Virtual Machines set up for being Bot servers
    /// </summary>
    public class BotServerCreator : AzureResourceCreatorBase
    {
        public BotServerCreator(string deploymentName, Input<string> location, Input<string> resourceGroupName)
            : base(deploymentName, location, resourceGroupName, "vm")
        {
        }

        /// <summary>
        /// Creates a virtual machine and the associate with the supplied Network Interace
        /// </summary>
        /// <param name="serverName"></param>
        /// <param name="networkInterfaceId"></param>
        /// <param name="appSecurityGroupId"></param>
        /// <param name="adminUserName"></param>
        /// <param name="adminPassword"></param>
        /// <returns></returns>
        public Compute.VirtualMachine CreateWithDefaultImage(string serverName, Input<string> networkInterfaceId, Input<string> keyVaultId, Input<string> secretUri, string adminUserName, string adminPassword)
        {
            var networkInterfaceRefArgs = new Compute.Inputs.NetworkInterfaceReferenceArgs
            {
                Id = networkInterfaceId,
                Primary = true
            };

            var vmNetworkProfileArgs = new Compute.Inputs.NetworkProfileArgs
            {
                NetworkInterfaces = networkInterfaceRefArgs
            };

            Compute.Inputs.OSProfileArgs vmOsProfileArgs = CreateOSProfileArgs(serverName, adminUserName, adminPassword, DefaultWindowsConfigurationArgs);

            var encryptionSettingsArgs = CreateDiskEcryptionSettings(keyVaultId, secretUri);

            return this.Create(serverName, DefaultHardwareProfileArgs, vmNetworkProfileArgs, vmOsProfileArgs, DefaultImageReferenceArgs, encryptionSettingsArgs);
        }

        /// <summary>
        /// Creates a virtual machine based on a custom image
        /// </summary>
        /// <param name="serverName"></param>
        /// <param name="vmImageId"></param>
        /// <param name="networkInterfaceId"></param>
        /// <param name="keyVaultId"></param>
        /// <param name="secretUri"></param>
        /// <param name="adminUserName"></param>
        /// <param name="adminPassword"></param>
        /// <returns></returns>
        public Compute.VirtualMachine CreateFromCustomImage(string serverName, Input<string> vmImageId, Input<string> networkInterfaceId, Input<string> keyVaultId, Input<string> secretUri, string adminUserName, string adminPassword)
        {
            var networkInterfaceRefArgs = new Compute.Inputs.NetworkInterfaceReferenceArgs
            {
                Id = networkInterfaceId,
                Primary = true
            };

            var vmNetworkProfileArgs = new Compute.Inputs.NetworkProfileArgs
            {
                NetworkInterfaces = networkInterfaceRefArgs
            };

            var vmOsProfileArgs = CreateOSProfileArgs(serverName, adminUserName, adminPassword, DefaultWindowsConfigurationArgs);

            var imageReferenceArgs = new Compute.Inputs.ImageReferenceArgs
            {
                Id = vmImageId
            };

            var encryptionSettingsArgs = CreateDiskEcryptionSettings(keyVaultId, secretUri);

            return this.Create(serverName, DefaultHardwareProfileArgs, vmNetworkProfileArgs, vmOsProfileArgs, imageReferenceArgs, encryptionSettingsArgs);
        }

        /// <summary>
        /// Create a virtual machine based on supplied arguments
        /// </summary>
        /// <param name="serverName"></param>
        /// <param name="vmHardwareProfileArgs"></param>
        /// <param name="vmNetworkProfileArgs"></param>
        /// <param name="vmOsProfileArgs"></param>
        /// <param name="imageReferenceArgs"></param>
        /// <param name="diskEncryptionSettingsArgs"></param>
        /// <returns></returns>
        public Compute.VirtualMachine Create(string serverName, Compute.Inputs.HardwareProfileArgs vmHardwareProfileArgs, Compute.Inputs.NetworkProfileArgs vmNetworkProfileArgs, Compute.Inputs.OSProfileArgs vmOsProfileArgs, Compute.Inputs.ImageReferenceArgs imageReferenceArgs, Compute.Inputs.DiskEncryptionSettingsArgs diskEncryptionSettingsArgs)
        {
            serverName = $"{this.Prefix}-{serverName}";

            var osDiskArgs = new Compute.Inputs.OSDiskArgs
            {
                Caching = Compute.CachingTypes.ReadWrite,
                CreateOption = "FromImage",
                ManagedDisk = DefaultManagedDiskParametersArgs,
                //Name = $"{serverName}_os_disk",
                EncryptionSettings = diskEncryptionSettingsArgs
            };

            var storageProfileArgs = new Compute.Inputs.StorageProfileArgs
            {
                ImageReference = imageReferenceArgs,
                OsDisk = osDiskArgs,
            };

            var virtualMachineArgs = new Compute.VirtualMachineArgs
            {
                //VmName = serverName,
                ResourceGroupName = this.ResourceGroupName,
                HardwareProfile = vmHardwareProfileArgs,
                NetworkProfile = vmNetworkProfileArgs,
                OsProfile = vmOsProfileArgs,
                StorageProfile = storageProfileArgs,
                Location = this.Location,
            };

            return new Compute.VirtualMachine(serverName, virtualMachineArgs);
        }

        private static Compute.Inputs.WindowsConfigurationArgs DefaultWindowsConfigurationArgs => new Compute.Inputs.WindowsConfigurationArgs
        {
            ProvisionVMAgent = true
        };

        private static Compute.Inputs.ImageReferenceArgs DefaultImageReferenceArgs => new Compute.Inputs.ImageReferenceArgs
        {
            Offer = "Windows-10",
            Publisher = "MicrosoftWindowsDesktop",
            Sku = "20h2-pro-g2",
            Version = "latest",
        };

        private static Compute.Inputs.HardwareProfileArgs DefaultHardwareProfileArgs => new Compute.Inputs.HardwareProfileArgs
        {
            VmSize = "Standard_D2s_v3"
        };

        private static Compute.Inputs.ManagedDiskParametersArgs DefaultManagedDiskParametersArgs => new Compute.Inputs.ManagedDiskParametersArgs
        {
            StorageAccountType = "Premium_LRS",
        };

        private static Compute.Inputs.DiskEncryptionSettingsArgs CreateDiskEcryptionSettings(Input<string> keyVaultId, Input<string> secretUri)
        {
            return new Compute.Inputs.DiskEncryptionSettingsArgs
            {
                Enabled = true,
                DiskEncryptionKey = new Compute.Inputs.KeyVaultSecretReferenceArgs
                {
                    SecretUrl = secretUri,
                    SourceVault = new Compute.Inputs.SubResourceArgs
                    {
                        Id = keyVaultId
                    }
                },
            };
        }

        private static Compute.Inputs.OSProfileArgs CreateOSProfileArgs(string serverName, string adminUserName, string adminPassword, Compute.Inputs.WindowsConfigurationArgs windowsConfigArgs)
        {
            return new Compute.Inputs.OSProfileArgs
            {
                AdminPassword = adminPassword,
                AdminUsername = adminUserName,
                ComputerName = serverName,
                WindowsConfiguration = windowsConfigArgs,
            };
        }


    }
}