using Pulumi;
using System;
using System.Collections.Generic;
using System.Linq;
using PulumiStorage = Pulumi.AzureNative.Storage;

namespace PulumiTools.AzureResources.Storage
{
    /// <summary>
    /// Class for creating Azure Storage Blobs
    /// </summary>
    public class StorageBlobCreator : AzureResourceCreatorBase
    {
        public Input<string> StorageAccountName { get; private set; }
        public Input<string> BlobContainerName { get; private set; }

        public StorageBlobCreator(string deploymentName, Input<string> location, Input<string> resourceGroupName, Input<string> storageAccountName, Input<string> blobContainerName) 
            : base(deploymentName, location, resourceGroupName,"")
        {
            this.StorageAccountName = storageAccountName;
            this.BlobContainerName = blobContainerName;
        }

        /// <summary>
        /// Create an Azure Storage Blob
        /// </summary>
        /// <param name="blobName"></param>
        /// <param name="blobSource"></param>
        /// <param name="dependencies"></param>
        /// <returns></returns>
        public PulumiStorage.Blob Create(string blobName, Input<AssetOrArchive> blobSource, List<Resource> dependencies=null)
        {
            PulumiStorage.BlobArgs args = new PulumiStorage.BlobArgs
            {
                ResourceGroupName = ResourceGroupName,
                AccountName = StorageAccountName,
                ContainerName = BlobContainerName,
                Source = blobSource,

            };
            var customOptions = new CustomResourceOptions();
            if(dependencies!=null && dependencies.Any())
            {
                dependencies.ForEach(d => customOptions.DependsOn.Add(d));
            }
            return new PulumiStorage.Blob(blobName, args,customOptions);
        }

        public PulumiStorage.Blob WriteAndCreate(string blobName, Input<AssetOrArchive> blobSource, Func<Output<string>> fileCreateFunction)
        {
            _ = fileCreateFunction();
            return this.Create(blobName, blobSource);
        }
    }
}
