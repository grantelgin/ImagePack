using ImagePack.Projects.Domain.Models;
using ImagePack.Projects.Domain.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ImagePack.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProjectsController : ControllerBase
    {
        private IImagePackProjectService _svc;
        public ProjectsController(IImagePackProjectService svc)
        {
            _svc = svc;
        }
        [HttpGet]
        public IEnumerable<Project> Get()
        {
            return _svc.GetAll();
        }

        [HttpGet("{id}")]
        public Project Get(int id)
        {
            return _svc.GetById(id);
        }
    }
}
