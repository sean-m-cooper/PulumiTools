using Pulumi;
using Pulumi.AzureNative.Compute;
using Pulumi.AzureNative.Insights;
using Pulumi.AzureNative.Storage;
using Silverwork.PulumiOps;
using PulumiTools.AzureResources.Analytics;
using PulumiTools.AzureResources.KeyVaults;
using PulumiTools.AzureResources.Networking;
using PulumiTools.AzureResources.Provisioning;
using PulumiTools.AzureResources.ResourceGroup;
using PulumiTools.AzureResources.Servers;
using PulumiTools.AzureResources.SqlServer;
using PulumiTools.AzureResources.Storage;
using PulumiTools.AzureResources.WebApps;
using System.Collections.Generic;
public class DefaultStack : Stack
{
    [Output]
    public Output<string> PrimaryStorageKey { get; set; }

    [Output]
    public Output<int> VMCount { get; set; }

    [Output]
    public Output<string> StorageAccountName { get; set; }

    [Output]
    public Output<string> DeploymentName { get; set; }
    [Output]
    public Output<string> ConfigPath { get; set; }

    private readonly InfrastructureConfig config;

    public DefaultStack()
    {
        #region Config and Setup
        var stackYaml = new Config();
        string clientName = stackYaml.Require("clientName");
        string environment = stackYaml.Require("env");
        string deploymentName = $"{clientName}{environment}";
        DeploymentName = Output.Create(deploymentName);

        string configPath = $"Config/{typeof(InfrastructureConfig).Name}.{clientName}-{environment}.json";
        ConfigPath = Output.Create(configPath);

        config = InfrastructureConfig.DeserializeJsonFile(configPath);

        string mountDriveScriptName = "MountDrive.ps1";
        string startupScriptName = "Startup.ps1";
        VMCount = Output.Create<int>(config.BotVmCount);

        var clientConfig = Pulumi.AzureNative.Authorization.GetClientConfig.InvokeAsync().GetAwaiter().GetResult();
        #endregion


        #region Resource Group and Storage
        // Create an Azure Resource Group
        var ResourceGroupCreator = new ResourceGroupCreator(deploymentName, config.Location, deploymentName);
        var resourceGroup = ResourceGroupCreator.Create();

        //create a storage account
        var storageAccountCreator = new StorageAccountCreator(deploymentName, config.Location, resourceGroup.Name);
        var storageAccount = storageAccountCreator.Create(config.DeploymentSuffix);
        // Export the primary key of the Storage Account
        this.PrimaryStorageKey = storageAccountCreator.GetPrimaryStorageKey(storageAccount.Name);

        this.StorageAccountName = storageAccount.Name;
        var storageAccountKey = Output.Tuple(resourceGroup.Name, storageAccount.Name).Apply(i => StorageAccountCreator.GetStorageAccountPrimaryKeyAsync(i.Item1, i.Item2));

        //create the primary blob container
        var blobContainerCreator = new BlobContainerCreator(deploymentName, config.Location, resourceGroup.Name, storageAccount.Name);
        var mainBlobContainer = blobContainerCreator.Create(PublicAccess.Container);

        var blobCreator = new StorageBlobCreator(deploymentName, config.Location, resourceGroup.Name, storageAccount.Name, mainBlobContainer.Name);

        //create the file share
        var fileShareCreator = new FileShareCreator(deploymentName, config.Location, resourceGroup.Name);
        var fileShare = fileShareCreator.Create($"botshare", storageAccount.Name, 100);

        Output<string> mountScriptFunc()
        {
            return Output.Tuple(StorageAccountName, storageAccountKey)
            .Apply(i => VirtualMachineRunCommandCreator.WriteMountCommandFile(mountDriveScriptName, i.Item1, i.Item2));
        }

        var mountScriptBlob = blobCreator.WriteAndCreate($"{mountDriveScriptName}", new FileAsset($"Assets/{mountDriveScriptName}"), mountScriptFunc);

        //write script to create startup job
        var startupScriptCommandFileName = Output.Tuple(storageAccount.Name, mainBlobContainer.Name)
             .Apply(i => VirtualMachineRunCommandCreator.WriteStartupScript(shareName: i.Item1, containerName: i.Item2, mountScriptName: mountDriveScriptName, fileName: startupScriptName));

        //upload script to create startup job
        var startupScriptBlob = blobCreator.Create(startupScriptName, new FileAsset($"Assets/{startupScriptName}"));
        #endregion

        #region KeyVault
        //create the keyvault
        var keyVaultCreator = new KeyVaultCreator(deploymentName, config.Location, resourceGroup.Name);
        var keyVault = keyVaultCreator.Create(clientConfig.TenantId, clientConfig.ObjectId, config.DeploymentSuffix);

        var endPointCreator = new PrivateEndpointCreator(deploymentName, config.Location, resourceGroup.Name);
        #endregion

        #region VirtualNetwork
        //create virtual network
        var networkCreator = new AzureNetworkCreator(deploymentName, config.Location, resourceGroup.Name);
        var virtualNetwork = networkCreator.Create(config.VNetAddressSpace);

        //create application security group(s)
        var appSecGroupCreator = new AppSecurityGroupCreator(deploymentName, config.Location, resourceGroup.Name);
        var botAppSecGroup = appSecGroupCreator.Create($"bots-{deploymentName}");
        var webAppSecGroup = appSecGroupCreator.Create($"web-{deploymentName}");
        var sqlAppSecGroup = appSecGroupCreator.Create($"sql-{deploymentName}");

        //create network security group(s)
        var networkSecGroupCreator = new NetworkSecurityGroupCreator(deploymentName, config.Location, resourceGroup.Name);
        var defaultSecurityRules = NetworkSecurityGroupCreator.GenerateDefaultInboundBotSecurityRules(config.BastionAddressSpace, new List<Input<string>> { botAppSecGroup.Id });
        var webSecurityRules = NetworkSecurityGroupCreator.GenerateDefaultWebSecurityRules(new List<Input<string>> { webAppSecGroup.Id });
        var sqlSecurityRules = NetworkSecurityGroupCreator.GenerateDefaultSQLInboundSecurityRules(new List<Input<string>> { sqlAppSecGroup.Id });

        var botNetworkSecGroup = networkSecGroupCreator.Create($"bots-{deploymentName}", defaultSecurityRules);
        var webNetworkSecGroup = networkSecGroupCreator.Create($"web-{deploymentName}", webSecurityRules);
        var sqlNetworkSecGroup = networkSecGroupCreator.Create($"sql-{deploymentName}", sqlSecurityRules);

        var bastionSecurityRules = NetworkSecurityGroupCreator.GenerateDefaultBastionSecurityRules();
        var bastionNetworkSecGroup = networkSecGroupCreator.Create("bastion", bastionSecurityRules);

        //create subnets
        var subnetCreator = new SubnetCreator(deploymentName, config.Location, resourceGroup.Name, virtualNetwork.Name);

        //var firewallSubnet = subnetCreator.Create("AzureFirewallSubnet", firewallAddressSpace, true);
        var bastionSubnet = subnetCreator.Create("AzureBastionSubnet", config.BastionAddressSpace, bastionNetworkSecGroup.Id, true);
        var v1Subnet = subnetCreator.Create($"v1-{deploymentName}", config.Subnet1AddressSpace, botNetworkSecGroup.Id);
        var v2Subnet = subnetCreator.Create($"v2-{deploymentName}", config.V2SubnetAddressSpace);

        var publicIpCreator = new PublicIPAddressCreator(deploymentName, config.Location, resourceGroup.Name);

        //create public IP address for Bastion
        var bastionPublicIp = publicIpCreator.CreateStaticStandardIp($"bst-{deploymentName}");

        //SC: uncomment lines below to create firewall and public IP
        //create public IP for the firewall
        //var firewallPulblicIP = publicIpCreator.CreateStaticStandardIp($"fw-{deploymentName}")

        //create firewall
        //var firewallCreator = new FirewallCreator(deploymentName, location, resourceGroup.Name);
        //var firewall = firewallCreator.Create(deploymentName, firewallSubnet.Id, firewallPulblicIP.Id);

        //create bastion
        var bastionCreator = new BastionCreator(deploymentName, config.Location, resourceGroup.Name);
        var bastionHost = bastionCreator.Create($"{deploymentName}", bastionPublicIp.Id, bastionSubnet.Id);

        //create private endpoints
        var storageEncryptionSecret = keyVaultCreator.CreateEncryptionSecret($"sec-sto-{deploymentName}{config.DeploymentSuffix}", keyVault.Name, storageAccount);
        var storageEndPoint = endPointCreator.CreateForLinkedService($"stor-{deploymentName}{config.DeploymentSuffix}", v1Subnet.Id, storageAccount.Id, storageEncryptionSecret, "file");
        #endregion

        #region SQL DB
        var sqlServerCreator = new SqlServerCreator(deploymentName, config.Location, resourceGroup.Name);
        var sqlServer = sqlServerCreator.Create($"{deploymentName}{config.DeploymentSuffix}", config.SqlUsername, config.SqlPassword, config.SqlAdminGroupName, config.SqlAdminGroupId, clientConfig.TenantId);

        var elasticPoolCreator = new ElasticPoolCreator(deploymentName, config.Location, resourceGroup.Name, sqlServer.Name);
        var elasticPool = elasticPoolCreator.Create(deploymentName);

        var databaseCreator = new SqlDatabaseCreator(deploymentName, config.Location, resourceGroup.Name, sqlServer.Name, elasticPool.Id);
        var database = databaseCreator.CreateWithBackupPolicy(deploymentName);

        var sqlEncryptionSecret = keyVaultCreator.CreateEncryptionSecret($"sec-sql-{deploymentName}{config.DeploymentSuffix}", keyVault.Name, database);
        var sqlEndPoint = endPointCreator.CreateForLinkedService($"sql-{deploymentName}", v1Subnet.Id, sqlServer.Id, sqlEncryptionSecret, "sqlserver");
        #endregion

        #region AppService
        var appServicePlanCreator = new AppServicePlanCreator(deploymentName, config.Location, resourceGroup.Name);
        var appServicePlan = appServicePlanCreator.CreateBasicAppPlan(deploymentName);

        var wwwBlob = blobCreator.Create("portalBlob", new FileArchive(config.PortalLocation));

        var codeBlobUrl = Output.Tuple(storageAccount.Name, mainBlobContainer.Name, wwwBlob.Name, resourceGroup.Name)
            .Apply(items => StorageAccountCreator.GetBlobSASToken(items.Item1, items.Item2, items.Item3, items.Item4));

        var webAppCreator = new WebAppCreator(deploymentName, config.Location, resourceGroup.Name);
        var webApp = webAppCreator.CreateWithDbConnection($"{deploymentName}-portal", appServicePlan.Id, codeBlobUrl, sqlServer.Name, database.Name, config.SqlUsername, config.SqlPassword);
        #endregion

        #region Analytics & Application Insights
        var workspaceCreator = new AnalyticsWorkspaceCreator(deploymentName, config.Location, resourceGroup.Name);
        var insightsCreator = new AppInsightsCreator(deploymentName, config.Location, resourceGroup.Name);

        var workspace = workspaceCreator.Create(deploymentName);
        var webAppInsights = insightsCreator.CreateWebInsights($"web-{deploymentName}", webApp.Name);
        var sqlInsights = insightsCreator.Create($"sql-{deploymentName}", sqlServer.Name, "sqlserver", ApplicationType.Other);
        #endregion

        #region Virtual Machine
        var niCreator = new NetworkInterfaceCreator(deploymentName, config.Location, resourceGroup.Name);
        var botServerCreator = new BotServerCreator(deploymentName, config.Location, resourceGroup.Name);
        var vmCommandCreator = new VirtualMachineRunCommandCreator(deploymentName, config.Location, resourceGroup.Name);

        string serverIdentifier = "";

        var diskEncryptionSecret = keyVaultCreator.CreateDiskEncryptionSecret($"sec-kv-{deploymentName}{config.DeploymentSuffix}", keyVault.Name, storageEncryptionSecret);
        var diskEncryptionSecretUri = diskEncryptionSecret.Properties.Apply(i => i.SecretUriWithVersion);
        var keyVaultEndPoint = endPointCreator.CreateForLinkedService($"kv-{deploymentName}", v1Subnet.Id, keyVault.Id, diskEncryptionSecret, "vault");

        Dictionary<int, VirtualMachine> botVMNames = new Dictionary<int, VirtualMachine>();
        Dictionary<int, VirtualMachine> svcVMNames = new Dictionary<int, VirtualMachine>();

        Output<string> botVmImageId = Output.Create("");

        if (config.UseBotImage)
        {
            var botVmImage = this.GetVmImage(config.VmImageResourceGroup, config.VmImageGallery, config.VmBotImageName);
            botVmImageId = botVmImage.Apply(i => i.Id);
        }

        for (int a = 1; a <= config.BotVmCount; a++)
        {
            serverIdentifier = $"bot-{deploymentName}-{a}";

            var azNic = niCreator.Create($"{serverIdentifier}", botNetworkSecGroup.Id, v1Subnet.Id, botAppSecGroup.Id);
            VirtualMachine vm;
            if (config.UseBotImage)
            {
                vm = botServerCreator.CreateFromCustomImage(serverIdentifier, botVmImageId, azNic.Id, keyVault.Id, diskEncryptionSecretUri, config.AdminUsername, config.AdminPassword);
            }
            else
            {
                vm = botServerCreator.CreateWithDefaultImage(serverIdentifier, azNic.Id, keyVault.Id, diskEncryptionSecretUri, config.AdminUsername, config.AdminPassword);
            }
            var vmInsights = insightsCreator.CreateVmInsights($"vm-{serverIdentifier}", vm.Name);
            botVMNames.Add(a, vm);
        }

        //foreach (var vmEntry in botVMNames)
        //{
        //    var vmCommand = vmCommandCreator.CreateBasicSharedDriveCommand(vmEntry.Key, startupScriptName, startupScriptBlob.Url, vmEntry.Value.Id, storageAccount.Name, storageAccountKey);
        //}

        for (int a = 1; a <= config.ServiceVmCount; a++)
        {
            serverIdentifier = $"svc-{deploymentName}-{a}";

            var azNic = niCreator.Create($"{serverIdentifier}", botNetworkSecGroup.Id, v1Subnet.Id, botAppSecGroup.Id);
            var vm = botServerCreator.CreateWithDefaultImage(serverIdentifier, azNic.Id, keyVault.Id, diskEncryptionSecretUri, config.AdminUsername, config.AdminPassword);
            var vmInsights = insightsCreator.CreateVmInsights($"vm-{serverIdentifier}", vm.Name);
            svcVMNames.Add(a, vm);
        }

        //foreach (var vmEntry in svcVMNames)
        //{
        //    var vmCommand = vmCommandCreator.CreateBasicSharedDriveCommand(vmEntry.Key, startupScriptName, startupScriptBlob.Url, vmEntry.Value.Id, storageAccount.Name, storageAccountKey);
        //}

        #endregion
    }

    private Output<GetGalleryImageResult> GetVmImage(Input<string> resourceGroupName, string galleryName, string imageName)
    {
        GetGalleryImageInvokeArgs args = new GetGalleryImageInvokeArgs
        {
            GalleryImageName = imageName,
            GalleryName = galleryName,
            ResourceGroupName = resourceGroupName,
        };
        return GetGalleryImage.Invoke(args);
    }
}
