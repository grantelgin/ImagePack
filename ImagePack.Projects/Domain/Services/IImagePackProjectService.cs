using ImagePack.Projects.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImagePack.Projects.Domain.Services
{
    public interface IImagePackProjectService
    {
        IEnumerable<Project> GetAll();
        Project GetById(int id);
    }
}
