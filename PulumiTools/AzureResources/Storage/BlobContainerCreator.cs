using Pulumi;
using PStorage = Pulumi.AzureNative.Storage;

namespace PulumiTools.AzureResources.Storage
{
    /// <summary>
    /// Class for creating Blob Storage Containers
    /// </summary>
    public class BlobContainerCreator: AzureResourceCreatorBase
    {
        public Input<string> StorageAccountName { get; private set; }

        public BlobContainerCreator(string deploymentName, Input<string> location, Input<string> resourceGroupName, Input<string> storageAccountName)
            :base(deploymentName, location, resourceGroupName,"bc")
        {
            this.StorageAccountName = storageAccountName;
        }

        /// <summary>
        /// Creates a blob storage container
        /// </summary>
        /// <param name="accessLevel"></param>
        /// <param name="containerName">If left blank defaults to bd-[deploymentName]</param>
        /// <returns></returns>
        public PStorage.BlobContainer Create(PStorage.PublicAccess accessLevel, string containerName = "")
        {
            if (string.IsNullOrEmpty(containerName))
            {
                containerName = $"{this.Prefix}-{DeploymentName}";
            }
            return  new PStorage.BlobContainer(containerName, new PStorage.BlobContainerArgs
            {
                ResourceGroupName = this.ResourceGroupName,
                AccountName = this.StorageAccountName,
                PublicAccess = accessLevel
            });
        }
    }
}
