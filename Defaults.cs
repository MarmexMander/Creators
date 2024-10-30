using Creators.Models;
namespace Creators;

public static class Defaults
{
    public static readonly Media DefaultPFP = new (){
        Guid = Guid.Parse("d13c0dee-d9a1-4309-974a-487385f6a284"),
        Author = "System",
        OriginalName = "defaultpfp.png",
    };
}