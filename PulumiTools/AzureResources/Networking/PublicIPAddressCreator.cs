using Pulumi;
using Network = Pulumi.AzureNative.Network;

namespace PulumiTools.AzureResources.Networking
{
    /// <summary>
    /// Class for creating Azure Public IP Addresses
    /// </summary>
    public class PublicIPAddressCreator : AzureResourceCreatorBase
    {
        public PublicIPAddressCreator(string deploymentName, Input<string> location, Input<string> resourceGroupName)
            : base(deploymentName, location, resourceGroupName, "ip")
        {
        }

        /// <summary>
        /// Create a Public IP Address with static IP allocation and a Standard sku
        /// </summary>
        /// <param name="ipAddressName"></param>
        /// <returns></returns>
        public Network.PublicIPAddress CreateStaticStandardIp(string ipAddressName)
        {
            return Create(ipAddressName, "Static", "Standard");
        }

        /// <summary>
        /// Create a Public IP Address
        /// </summary>
        /// <param name="ipAddressName"></param>
        /// <param name="ipAllocationMethod"></param>
        /// <param name="skuName"></param>
        /// <returns></returns>
        public Network.PublicIPAddress Create(string ipAddressName, string ipAllocationMethod, string skuName)
        {
            ipAddressName = $"{this.Prefix}-{ipAddressName}";
            var publicIPAddressArgs = new Network.PublicIPAddressArgs
            {
                ResourceGroupName = this.ResourceGroupName,
                PublicIPAllocationMethod = ipAllocationMethod,
                Sku = new Network.Inputs.PublicIPAddressSkuArgs
                {
                    Name = skuName,
                }
            };
            return new Network.PublicIPAddress(ipAddressName, publicIPAddressArgs);
        }
    }
}
