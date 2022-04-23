using ImagePack.ProjectImages.Domain.Models;
using ImagePack.ProjectImages.Domain.Repositories;
using ImagePack.ProjectImages.Domain.Services;

namespace ImagePack.ProjectImages.Services;

public class ImagePathLocatorService : IImageLocatorService
{
    private IImageLocatorRepository _repo;
    public ImagePathLocatorService(IImageLocatorRepository repo)
    {
        _repo = repo;
    }
    public IEnumerable<ImageLocator> GetAllByProjectId(int projectId)
    {
        return _repo.GetAllByProjectId(projectId);
    }
}