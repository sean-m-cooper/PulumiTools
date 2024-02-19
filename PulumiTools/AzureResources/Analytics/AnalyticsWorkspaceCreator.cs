using Pulumi;
using Insights = Pulumi.Azure.OperationalInsights;

namespace PulumiTools.AzureResources.Analytics
{
    /// <summary>
    /// Class for creating an analytics workspace
    /// </summary>
    public class AnalyticsWorkspaceCreator : AzureResourceCreatorBase
    {
        public AnalyticsWorkspaceCreator(string deploymentName, Input<string> location, Input<string> resourceGroupName)
            : base(deploymentName, location, resourceGroupName, "aws")
        {

        }

        /// <summary>
        /// Create an analytics workspace with a given name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public Insights.AnalyticsWorkspace Create(string name)
        {
            name = $"{this.Prefix}{name}";
            var args = new Insights.AnalyticsWorkspaceArgs
            {
                Location = this.Location,
                ResourceGroupName = this.ResourceGroupName,
                RetentionInDays = 90,
            };
            return new Insights.AnalyticsWorkspace(name, args);
        }


    }
}
