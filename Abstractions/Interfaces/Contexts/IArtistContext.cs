using System.Threading.Tasks;
using ImageInfrastructure.Abstractions.Poco;

namespace ImageInfrastructure.Abstractions.Interfaces.Contexts
{
    public interface IArtistContext
    {
        Task<ArtistAccount> GetArtist(ArtistAccount artist);
    }
}