using ImagePack.Projects.Domain.Models;
using ImagePack.Projects.Domain.Repositories;
using ImagePack.Projects.Domain.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImagePack.Projects.Services
{
    public class ImagePackProjectService : IImagePackProjectService
    {
        private IImagePackProjectsRepository _imagePackProjectRepository;
        public ImagePackProjectService(IImagePackProjectsRepository repo)
        {
            _imagePackProjectRepository = repo;
        }
        public IEnumerable<Project> GetAll()
        {
           return _imagePackProjectRepository.GetAll();
        }

        public Project GetById(int id)
        {
            return _imagePackProjectRepository.GetById(id);
        }
    }
}
