using Pulumi;
using SQL = Pulumi.AzureNative.Sql;

namespace PulumiTools.AzureResources.SqlServer
{
    /// <summary>
    /// Class for creating an Azure SQL Server
    /// </summary>
    public class SqlServerCreator : AzureResourceCreatorBase
    {
        public SqlServerCreator(string deploymentName, Input<string> location, Input<string> resourceGroupName)
            : base(deploymentName, location, resourceGroupName, "sql-srv")
        { }

        /// <summary>
        /// Creates an Azure SQL Server
        /// </summary>
        /// <param name="serverName"></param>
        /// <param name="adminUsername"></param>
        /// <param name="adminPassword"></param>
        /// <param name="adminGroupName"></param>
        /// <param name="adminGroupId"></param>
        /// <param name="tenantId"></param>
        /// <param name="serverVersion"></param>
        /// <returns></returns>
        public SQL.Server Create(string serverName, Input<string> adminUsername, Input<string> adminPassword, Input<string> adminGroupName, Input<string> adminGroupId, string tenantId, string serverVersion = "12.0")
        {
            serverName = $"{this.Prefix}-{serverName}";
            SQL.Inputs.ServerExternalAdministratorArgs serverExternalAdministratorArgs = new SQL.Inputs.ServerExternalAdministratorArgs
            {
                AdministratorType = SQL.AdministratorType.ActiveDirectory,
                PrincipalType = SQL.PrincipalType.Group,
                Sid = adminGroupId,
                Login = adminGroupName,
                AzureADOnlyAuthentication = false,
                TenantId = tenantId
            };

            SQL.ServerArgs args = new SQL.ServerArgs
            {
                ResourceGroupName = ResourceGroupName,
                AdministratorLogin = adminUsername,
                AdministratorLoginPassword = adminPassword,
                Version = serverVersion,
                Location = Location,
                MinimalTlsVersion = "1.2",
                PublicNetworkAccess = "Disabled",
                Administrators = serverExternalAdministratorArgs
            };

            return new SQL.Server(serverName, args);
        }
    }
}
