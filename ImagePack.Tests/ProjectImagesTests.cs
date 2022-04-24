using System.Collections.Generic;
using System.Linq;
using ImagePack.ProjectImages.Domain.Models;
using ImagePack.ProjectImages.Domain.Services;
using ImagePack.ProjectImages.Persistence;
using ImagePack.ProjectImages.Services;
using ImagePack.Projects.Domain.Models;
using ImagePack.Projects.Persistence;
using ImagePack.Projects.Services;
using NUnit.Framework;

namespace ImagePack.Tests;

[TestFixture]
public class ProjectImagesTests : TestBase
{
private IImageLocatorService _svc;

[OneTimeSetUp]
protected override void SetUp()
{
    base.SetUp();
    _svc = new ImagePathLocatorService(new ImageLocatorJsonFileRepository(base._config));
}

[OneTimeTearDown]
protected override void TearDown()
{
    base.TearDown();
}

[TestCase(1)]
[TestCase(2)]
[TestCase(3)]
public void GetAllByProjectId(int projectId)
{
    IEnumerable<ImageLocator> result = _svc.GetAllByProjectId(projectId);
    Assert.True(result != null && result.Count() > 0);
}

[TestCase(-1)]
[TestCase(1000)]
public void GivenBadProjectId_ReturnsEmptyCollection(int projectId)
{
    var result = _svc.GetAllByProjectId(projectId);
    Assert.True(result != null && result.Count() == 0);
}

}