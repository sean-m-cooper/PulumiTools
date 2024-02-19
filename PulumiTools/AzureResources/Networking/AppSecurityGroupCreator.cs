using Pulumi;
using Network = Pulumi.AzureNative.Network;
namespace PulumiTools.AzureResources.Networking
{
    /// <summary>
    /// Class for creating Application Security Groups
    /// </summary>
    public class AppSecurityGroupCreator : AzureResourceCreatorBase
    {
        public AppSecurityGroupCreator(string deploymentName, Input<string> location, Input<string> resourceGroupName) 
            : base(deploymentName, location, resourceGroupName, "asg")
        {

        }

        /// <summary>
        /// Create an Application Security Group
        /// </summary>
        /// <param name="applicationSecurityGroupName"></param>
        /// <returns></returns>
        public Network.ApplicationSecurityGroup Create(string applicationSecurityGroupName)
        {
            applicationSecurityGroupName = $"{this.Prefix}-{applicationSecurityGroupName}";
            var appSecGroupArgs = new Network.ApplicationSecurityGroupArgs
            {
                ResourceGroupName = this.ResourceGroupName, 
                Location=this.Location,
            };

            return new Network.ApplicationSecurityGroup(applicationSecurityGroupName, appSecGroupArgs);
        }
    }
}
