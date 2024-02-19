using Pulumi;
using System.Threading.Tasks;

class Program
{
    static Task<int> Main()
    {
        return Deployment.RunAsync<DefaultStack>();
    }
}
