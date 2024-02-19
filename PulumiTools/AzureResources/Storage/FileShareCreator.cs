using Pulumi;
using PulumiStorage = Pulumi.AzureNative.Storage;

namespace PulumiTools.AzureResources.Storage
{
    /// <summary>
    /// Class for creating Azure File Shares
    /// </summary>
    public class FileShareCreator : AzureResourceCreatorBase
    {
        public FileShareCreator(string deploymentName, Input<string> location, Input<string> resourceGroupName)
            : base(deploymentName, location, resourceGroupName, "fs")
        {
        }

        /// <summary>
        /// Create an Azure File Share
        /// </summary>
        /// <param name="fileShareName"></param>
        /// <param name="storageAccountName"></param>
        /// <param name="sizeInGb"></param>
        /// <returns></returns>
        public PulumiStorage.FileShare Create(string fileShareName, Input<string> storageAccountName, int sizeInGb)
        {
            var fileShareArgs = new PulumiStorage.FileShareArgs
            {
                ShareQuota = sizeInGb,
                AccountName = storageAccountName,
                ResourceGroupName = this.ResourceGroupName,
            };
            return new PulumiStorage.FileShare($"{this.Prefix }{ fileShareName }", fileShareArgs);
        }
    }
}
