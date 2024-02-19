using Pulumi;
using Network = Pulumi.AzureNative.Network;

namespace PulumiTools.AzureResources.Networking
{
    /// <summary>
    /// Class for creating the Azure Bastion service
    /// </summary>
    public class BastionCreator : AzureResourceCreatorBase
    {
        public BastionCreator(string deploymentName, Input<string> location, Input<string> resourceGroupName)
            : base(deploymentName, location, resourceGroupName, "Bastion")
        {
        }

        /// <summary>
        /// Create an Azure Bastion with a specified public ip addres and subnet
        /// </summary>
        /// <param name="bastionHostName"></param>
        /// <param name="publicIpAddressId"></param>
        /// <param name="subnetId"></param>
        /// <returns></returns>
        public Network.BastionHost Create(string bastionHostName, Input<string> publicIpAddressId, Input<string> subnetId)
        {
            bastionHostName = $"{this.Prefix}-{bastionHostName}";
            var bastionHostArgs = new Network.BastionHostArgs
            {
                ResourceGroupName = this.ResourceGroupName,
                IpConfigurations = new Network.Inputs.BastionHostIPConfigurationArgs
                {
                    Name = bastionHostName,
                    PublicIPAddress = new Network.Inputs.SubResourceArgs
                    {
                        Id = publicIpAddressId
                    },
                    Subnet = new Network.Inputs.SubResourceArgs
                    {
                        Id = subnetId
                    }
                }
            };
            return new Network.BastionHost(bastionHostName, bastionHostArgs);
        }
    }
}
