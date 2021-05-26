using System.Threading.Tasks;
using ImageInfrastructure.Abstractions.Poco;

namespace ImageInfrastructure.Abstractions.Interfaces.Contexts
{
    public interface ITagContext
    {
        Task<ImageTag> GetTag(ImageTag tag);
    }
}