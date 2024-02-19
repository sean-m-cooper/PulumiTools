using Pulumi;
using System.Threading.Tasks;
using PulumiStorage = Pulumi.AzureNative.Storage;

namespace PulumiTools.AzureResources.Storage
{
    public class StorageAccountCreator : AzureResourceCreatorBase
    {
        public PulumiStorage.SkuName SkuName { get; private set; }
        public PulumiStorage.Kind Kind { get; private set; }

        /// <summary>
        /// Class for creating storage accounts in a specified resource group. 
        /// </summary>
        /// <param name="deploymentName"></param>
        /// <param name="location"></param>
        /// <param name="resourceGroupName"></param>
        /// <remarks>Defaults SkuName to Premium LRS and Storage Kind to V2</remarks>
        public StorageAccountCreator(string deploymentName, Input<string> location, Input<string> resourceGroupName) :
            this(deploymentName, location, resourceGroupName, PulumiStorage.SkuName.Premium_LRS, PulumiStorage.Kind.StorageV2)
        { }

        /// <summary>
        /// Class for creating storage accounts in a specified resource group with a specified sku and storage kind.
        /// </summary>
        /// <param name="deploymentName"></param>
        /// <param name="location"></param>
        /// <param name="resourceGroupName"></param>
        /// <param name="skuName"></param>
        /// <param name="storageKind"></param>
        public StorageAccountCreator(string deploymentName, Input<string> location, Input<string> resourceGroupName, PulumiStorage.SkuName skuName, PulumiStorage.Kind storageKind)
            : base(deploymentName, location, resourceGroupName, "sa")
        {
            this.SkuName = skuName;
            this.Kind = storageKind;
        }

        /// <summary>
        /// Create a storage account.
        /// </summary>
        /// <param name="suffix">Use a unique suffix if the storage account will be deleted and recreated frequently to prevent name collisions.</param>
        /// <returns></returns>
        public PulumiStorage.StorageAccount Create(string suffix = "")
        {
            var storageSkuArgs = new PulumiStorage.Inputs.SkuArgs
            {
                Name = PulumiStorage.SkuName.Standard_LRS
            };

            string saName = $"{this.Prefix}{DeploymentName}{suffix}";
            return new PulumiStorage.StorageAccount(saName, new PulumiStorage.StorageAccountArgs
            {
                ResourceGroupName = this.ResourceGroupName,
                Sku = storageSkuArgs,
                Kind = this.Kind,
            });
        }

        public Output<string> GetPrimaryStorageKey(Input<string> storageAccountName)
        {
            return Output.Tuple(this.ResourceGroupName, storageAccountName).Apply(names =>
                Output.CreateSecret(GetStorageAccountPrimaryKeyAsync(names.Item1, names.Item2)));
        }

        public static async Task<string> GetStorageAccountPrimaryKeyAsync(string resourceGroupName, string accountName)
        {
            var accountKeys = await PulumiStorage.ListStorageAccountKeys.InvokeAsync(new PulumiStorage.ListStorageAccountKeysArgs
            {
                ResourceGroupName = resourceGroupName,
                AccountName = accountName
            });
            return accountKeys.Keys[0].Value;
        }

        public static async Task<string> GetStorageAccountSecondaryKeyAsync(string resourceGroupName, string accountName)
        {
            var accountKeys = await PulumiStorage.ListStorageAccountKeys.InvokeAsync(new PulumiStorage.ListStorageAccountKeysArgs
            {
                ResourceGroupName = resourceGroupName,
                AccountName = accountName
            });
            return accountKeys.Keys[1].Value;
        }

        /// <summary>
        /// Returns the Shared Access Token for a blob
        /// </summary>
        /// <param name="storageAccountName"></param>
        /// <param name="storageContainerName"></param>
        /// <param name="blobName"></param>
        /// <param name="resourceGroupName"></param>
        /// <returns></returns>
        public static Output<string> GetBlobSASToken(string storageAccountName, string storageContainerName, string blobName, string resourceGroupName)
        {
            Output<PulumiStorage.ListStorageAccountServiceSASResult> blobSAS = PulumiStorage.ListStorageAccountServiceSAS.Invoke(new PulumiStorage.ListStorageAccountServiceSASInvokeArgs
            {
                AccountName = storageAccountName,
                Protocols = PulumiStorage.HttpProtocol.Https,
                SharedAccessStartTime = "2021-01-01",
                SharedAccessExpiryTime = "2030-01-01",
                Resource = PulumiStorage.SignedResource.C,
                ResourceGroupName = resourceGroupName,
                Permissions = PulumiStorage.Permissions.R,
                CanonicalizedResource = $"/blob/{storageAccountName}/{storageContainerName}",
                ContentType = "application/json",
                CacheControl = "max-age=5",
                ContentDisposition = "inline",
                ContentEncoding = "deflate"
            });

            return blobSAS.Apply(blob => $"https://{storageAccountName}.blob.core.windows.net/{storageContainerName}/{blobName}?{blob.ServiceSasToken}");
        }

        /// <summary>
        /// Get the Shared Access Token for a file share
        /// </summary>
        /// <param name="resourceGroupName"></param>
        /// <param name="storageAccountName"></param>
        /// <param name="fileShareName"></param>
        /// <returns></returns>
        public static Output<string> GetFileShareSASToken(Output<string> resourceGroupName, Output<string> storageAccountName, Output<string> fileShareName)
        {
            Input<string> canonicalUrl = Output.Tuple(resourceGroupName, storageAccountName, fileShareName).Apply(items => $"/file/{items.Item1}/{items.Item2}/{items.Item3}");
            Output<PulumiStorage.ListStorageAccountServiceSASResult> fileSAS = PulumiStorage.ListStorageAccountServiceSAS.Invoke(new PulumiStorage.ListStorageAccountServiceSASInvokeArgs
            {
                AccountName = storageAccountName,
                Protocols = PulumiStorage.HttpProtocol.Https,
                SharedAccessStartTime = "2021-01-01",
                SharedAccessExpiryTime = "2030-01-01",
                Resource = PulumiStorage.SignedResource.C,
                ResourceGroupName = resourceGroupName,
                Permissions = PulumiStorage.Permissions.R,
                CanonicalizedResource = canonicalUrl,
                ContentType = "application/json",
                CacheControl = "max-age=5",
                ContentDisposition = "inline",
                ContentEncoding = "deflate"
            });

            return Output.Tuple(storageAccountName, fileShareName, fileSAS).Apply(items => $"https://{items.Item1}.file.core.windows.net/{items.Item2}?{items.Item3.ServiceSasToken}");
        }
    }
}
