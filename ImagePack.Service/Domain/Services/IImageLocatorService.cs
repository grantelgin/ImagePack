using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImagePack.ProjectImages.Domain.Models;

namespace ImagePack.ProjectImages.Domain.Services
{
    public interface IImageLocatorService
    {
        IEnumerable<ImageLocator> GetAllByProjectId(int projectId);
    }
}
