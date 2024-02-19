using Pulumi;
using Pulumi.AzureNative.Network;
using Network = Pulumi.AzureNative.Network;

namespace PulumiTools.AzureResources.Networking
{
    /// <summary>
    /// Class for creating Azure Private Endpoints
    /// </summary>
    public class PrivateEndpointCreator : AzureResourceCreatorBase
    {
        public PrivateEndpointCreator(string deploymentName, Input<string> location, Input<string> resourceGroupName)
            : base(deploymentName, location, resourceGroupName, "pve")
        {
        }

        public PrivateEndpoint Create(string endpointName, Input<string> subnetId)
        {
            var args = new PrivateEndpointArgs
            {
                Location = this.Location,
                ResourceGroupName = this.ResourceGroupName,
                Subnet = new Network.Inputs.SubnetArgs
                {
                    Id = subnetId,
                },
            };
            return Create(endpointName, args);
        }

        /// <summary>
        /// Creates a private endpoint on a subnet for a service
        /// </summary>
        /// <param name="endpointName"></param>
        /// <param name="subnetId"></param>
        /// <param name="linkedServiceId"></param>
        /// <param name="kvSecret"></param>
        /// <param name="groupId"></param>
        /// <returns></returns>
        public PrivateEndpoint CreateForLinkedService(string endpointName, Input<string> subnetId, Input<string> linkedServiceId, Resource kvSecret, string groupId)
        {
            var args = new PrivateEndpointArgs
            {
                Location = this.Location,
                ResourceGroupName = this.ResourceGroupName,
                PrivateLinkServiceConnections = new Network.Inputs.PrivateLinkServiceConnectionArgs
                {
                    Name = endpointName,
                    PrivateLinkServiceId = linkedServiceId,
                    GroupIds =
                    {
                        groupId
                    }

                },
                Subnet = new Network.Inputs.SubnetArgs
                {
                    Id = subnetId,
                },
            };
            var opts = new CustomResourceOptions
            {
                DependsOn = { kvSecret }
            };
            return Create(endpointName, args, opts);
        }

        private PrivateEndpoint Create(string endpointName, PrivateEndpointArgs args, CustomResourceOptions opts = null)
        {
            endpointName = $"{this.Prefix}-{endpointName}";
            return new PrivateEndpoint(endpointName, args, opts);
        }
    }
}
