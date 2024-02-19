using Pulumi;
using Pulumi.AzureNative.Compute;
using PulumiAzure = Pulumi.Azure;

namespace PulumiTools.AzureResources.Provisioning
{
    /// <summary>
    /// Class for creating an Azure Virtual Machine Extension for mapping a shared drive
    /// </summary>
    public class VirtualMachineRunCommandCreator : AzureResourceCreatorBase
    {
        [Output]
        public Output<string> SettingsText { get; private set; }
        public VirtualMachineRunCommandCreator(string deploymentName, Input<string> location, Input<string> resourceGroupName) 
            : base(deploymentName, location, resourceGroupName,"vmrc")
        {
        }


        public VirtualMachineExtension CreateSharedDriveCommand(int index, Output<string> virtualMachineName, Input<string> scriptBlobUrl, int timeoutInSec = 3600)
        {
            var settings = scriptBlobUrl.Apply(script => @"	{
    ""commandToExecute"": ""powershell -ExecutionPolicy Unrestricted -File test_script.ps1"",
    ""fileUris"": [" + "\"" + script + "\"" + "]}");

            var args = new VirtualMachineExtensionArgs
            {
                VmExtensionName = $"cmdVMountShare{index}",
                ResourceGroupName = this.ResourceGroupName,
                VmName = virtualMachineName,
                Publisher = "Microsoft.Compute",
                Type = "CustomScriptExtension",
                TypeHandlerVersion = "1.10",
                Location = this.Location,
                Settings = settings
            };
            return new VirtualMachineExtension($"cmdVMountShare{index}", args);
        }

        public PulumiAzure.Compute.Extension CreateBasicSharedDriveCommand(int index, Input<string> scriptFileName, Input<string> scriptBlobUrl, Input<string> virtualMachineId, Input<string> storageAccountName, Input<string> storageAccountKey)
        {
            var settings = Output.Tuple(scriptFileName, scriptBlobUrl, storageAccountName, storageAccountKey).Apply(items => $@"	{{
            ""commandToExecute"": ""powershell.exe -ExecutionPolicy Unrestricted -File {items.Item1}"",
            ""storageAccountName"": ""{items.Item3}"",
            ""storageAccountKey"": ""{items.Item4}"",
            ""fileUris"": [" + "\"" + items.Item2 + "\"" + "]}");

            this.SettingsText = settings;

            string commandName = $"cmdMountShare-{index}";
            var args = new PulumiAzure.Compute.ExtensionArgs
            {
                Name = commandName,
                VirtualMachineId = virtualMachineId,
                Publisher = "Microsoft.Compute",
                Type = "CustomScriptExtension",
                TypeHandlerVersion = "1.10",
                AutoUpgradeMinorVersion = true,
                ProtectedSettings = settings
            };
            this.SettingsText = settings;
            return new PulumiAzure.Compute.Extension(commandName, args);
        }

        public static string WriteMountCommandFile(string fileName, string shareName, string shareKey, string driveLetter = "Z")
        {
            var templateText=System.IO.File.ReadAllText($"Templates/{fileName}");
            var newText = templateText.Replace("[StorageAccount]", shareName).Replace("[StorageAccountKey]", shareKey).Replace("[DriveLetter]",driveLetter);

            System.IO.File.WriteAllText($"Assets/{fileName}", newText);
            return fileName;
        }

        public static string WriteStartupScript(string shareName, string containerName, string mountScriptName, string fileName)
        {
            var templateText = System.IO.File.ReadAllText($"Templates/{fileName}");
            var newText = templateText.Replace("[StorageAccount]", shareName)
                .Replace("[Container]", containerName)
                .Replace("[MountScript]",mountScriptName);
            
            System.IO.File.WriteAllText($"Assets/{fileName}", newText);
            return fileName;
        }
    }
}
