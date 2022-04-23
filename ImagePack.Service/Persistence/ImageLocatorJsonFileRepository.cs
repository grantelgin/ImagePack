using ImagePack.ProjectImages.Domain.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImagePack.ProjectImages.Domain.Models;
using Newtonsoft.Json;

namespace ImagePack.ProjectImages.Persistence
{
    public class ImageLocatorJsonFileRepository : IImageLocatorRepository
    {
        private string _jsonFilePathProjectCollectionLocator;
        private string _projectCollectionDataFolder;
        
        public ImageLocatorJsonFileRepository(IConfiguration config)
        {
            string configEntry = "imagePack:projectImages:jsonFileRepo:jsonRepoFile";
            _jsonFilePathProjectCollectionLocator = config[configEntry];
            if (!File.Exists(_jsonFilePathProjectCollectionLocator))
            {
                throw new FileNotFoundException($"The repository file '{_jsonFilePathProjectCollectionLocator}' for '{configEntry}' could not be found.");
            }

            string configEntry2 = "imagePack:projectImages:jsonFileRepo:imageLocatorDataDirectory";
            _projectCollectionDataFolder = config[configEntry2];
            if (!Directory.Exists(_projectCollectionDataFolder))
            {
                throw new DirectoryNotFoundException(
                    $"The repository data directory '{_projectCollectionDataFolder}' for '{configEntry2}' could not be found.");
            }
        }
        public IEnumerable<ImageLocator> GetAllByProjectId(int projectId)
        {
            IEnumerable<ImageLocator> result = new List<ImageLocator>();
            string imageLocatorCollectionFilePath = GetImageCollectionFileForProject(projectId);
            string fullPath = Path.Combine(_projectCollectionDataFolder, imageLocatorCollectionFilePath);
            if (string.IsNullOrEmpty(imageLocatorCollectionFilePath) || ! File.Exists(fullPath))
            {
                // not found. return empty collection. Consider handling the file not found case separately. 
                // This is a json file repository. A more robust SQL or redis cache repo would be a better solution. A separate Persistence class should be developed for that.
                return result;
            }

            string contents = File.ReadAllText(fullPath);
            result = JsonConvert.DeserializeObject<IEnumerable<ImageLocator>>(contents);
            
            return result;
        }

        private string GetImageCollectionFileForProject(int projectId)
        {
            string projectImageFileCollectionFilePath = string.Empty;
            string repoContents = File.ReadAllText(_jsonFilePathProjectCollectionLocator);
            IEnumerable<ProjectImageCollectionLocator> projectImageCollections = JsonConvert.DeserializeObject < IEnumerable<ProjectImageCollectionLocator>>(repoContents);

            foreach (ProjectImageCollectionLocator locator in projectImageCollections)
            {
                if (locator.ProjectId == projectId)
                {
                    // found
                    return locator.ImageCollectionJsonFile;
                }
            }
            
            return projectImageFileCollectionFilePath;
        }

        internal class ProjectImageCollectionLocator
        {
            public string ImageCollectionJsonFile { get; set; }
            public int ProjectId { get; set; }
        }
    }
}
