using ImagePack.ProjectImages.Domain.Repositories;
using ImagePack.ProjectImages.Domain.Services;
using ImagePack.ProjectImages.Persistence;
using ImagePack.ProjectImages.Services;
using ImagePack.Projects.Domain.Repositories;
using ImagePack.Projects.Domain.Services;
using ImagePack.Projects.Persistence;
using ImagePack.Projects.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddScoped<IImagePackProjectsRepository, ImagePackProjectsJsonFileRepository>();
builder.Services.AddScoped<IImagePackProjectService, ImagePackProjectService>();
builder.Services.AddScoped<IImageLocatorRepository, ImageLocatorJsonFileRepository>();
builder.Services.AddScoped<IImageLocatorService, ImagePathLocatorService>();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
