using ImagePack.Projects.Domain.Models;
using ImagePack.Projects.Domain.Repositories;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ImagePack.Projects.Persistence
{
    public class ImagePackProjectsJsonFileRepository : IImagePackProjectsRepository
    {
        private string _jsonFilePath;
        
        public ImagePackProjectsJsonFileRepository(IConfiguration config)
        {
            string jsonFileConfigEntry = "imagePack:projects:jsonFile";
            _jsonFilePath = config[jsonFileConfigEntry];
            if (!File.Exists(_jsonFilePath))
            {
                throw new FileNotFoundException($"The repository file '{_jsonFilePath}' for '{jsonFileConfigEntry}' could not be found.");
            }
        }
        public IEnumerable<Project> GetAll()
        {
            string contents = File.ReadAllText(_jsonFilePath);
            Project[] projects = JsonConvert.DeserializeObject<Project[]>(contents);
            return projects;
        }

        public Project GetById(int id)
        {
            // Traverse the projects collection to find a project id match. 
            // This repository is a simple json file.
            // If a large number of projects shall be supported, implement a separate repository that accesses a SQL or cache server (redis). 
            IEnumerable<Project> projects = GetAll();
            foreach (var project in projects)
            {
                if (project.id == id)
                {
                    return project;
                }
            }

            // if we get here, the project id was not found in the project repository. 
            return new ProjectErrorObject() { id = id, ErrorMessage = $"Project id '{id}' not found." };
        }
        
    }
}
