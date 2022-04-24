using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using ImagePack.Projects.Domain.Models;
using ImagePack.Projects.Domain.Services;
using ImagePack.Projects.Persistence;
using ImagePack.Projects.Services;
using NUnit.Framework;

namespace ImagePack.Tests;

[TestFixture]
public class ProjectsTests : TestBase
{
    private IImagePackProjectService _svc;
    
    [OneTimeSetUp]
    protected override void SetUp()
    {
        base.SetUp();
        _svc = new ImagePackProjectService(new ImagePackProjectsJsonFileRepository(base._config));
    }

    [OneTimeTearDown]
    protected override void TearDown() { base.TearDown(); }

    [Test]
    public void GetAll()
    {
        IEnumerable<Project> result = _svc.GetAll();
        Assert.True(result != null && result.Count() > 0);
    }

    [TestCase(-1)]
    [TestCase(1000)] 
    public void GivenBadProjectId_ReturnsProjectErrorObject(int projectId)
    {
        var result = _svc.GetById(projectId);
        Assert.True(result is ProjectErrorObject);
        ProjectErrorObject errObj = (ProjectErrorObject)result;
        Assert.True(errObj.ErrorMessage.Length > 0);
    }

    
    [TestCase(1)]
    [TestCase(2)]
    [TestCase(3)]
    public void GivenValidProjectId_ReturnsValidProjectObject(int projectId)
    {
        var result = _svc.GetById(projectId);
        Assert.True(result.name.Length > 0);
    }
}