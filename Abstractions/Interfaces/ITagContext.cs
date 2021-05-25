using ImageInfrastructure.Abstractions.Poco;

namespace ImageInfrastructure.Abstractions.Interfaces
{
    public interface ITagContext
    {
        bool GetTag(ImageTag tag, out ImageTag existing);
        void FlushTags();
    }
}