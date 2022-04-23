using ImagePack.ProjectImages.Domain.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using System.IO.Compression;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Runtime.InteropServices;
using ImagePack.ProjectImages.Domain.Models;

namespace ImagePack.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProjectImagesController : ControllerBase
    {
        private IImageLocatorService _svc;

        public ProjectImagesController(IImageLocatorService svc)
        {
            _svc = svc;
        }

        [HttpGet("{projectId}")]
        public IActionResult Get(int projectId)
        {
            IEnumerable<ImageLocator>locators =  _svc.GetAllByProjectId(projectId);
            return DownloadAll(projectId, locators);
            //throw new NotImplementedException();
            //return _svc.GetAllByProjectId(projectId);
        }

        private IActionResult DownloadAll(int projectId, IEnumerable<ImageLocator> imageLocators)
        {
            var syncIOFeature = HttpContext.Features.Get<IHttpBodyControlFeature>();
            if (syncIOFeature != null)
            {
                syncIOFeature.AllowSynchronousIO = true; 
            }
            
            return new FileCallbackResult(new MediaTypeHeaderValue("application/octet-stream"),
                async (outputStream, _) =>
                {
                    using (var zipArchive = new ZipArchive(outputStream, ZipArchiveMode.Create))
                    {
                        foreach (ImageLocator locator in imageLocators)
                        {
                            var zipEntry = zipArchive.CreateEntry(locator.filename, CompressionLevel.NoCompression);
                            await using (var zipStream = zipEntry.Open())
                            {

                                    try
                                    {
                                        MemoryStream imgStream = GetStreamFromUrl(locator.url);
                                        await imgStream.CopyToAsync(zipStream);
                                    }
                                    catch
                                    {
                                        throw;
                                    }
                                
                            }
                           
                        }
                    }
                })
            {
                FileDownloadName = $"ProjectImageCollection_{projectId}.zip"
            };
        }

        private static MemoryStream GetStreamFromUrl(string url)
        {
            byte[] imageData = null;

            using (var wc = new System.Net.WebClient())
                imageData = wc.DownloadData(url);

            return new MemoryStream(imageData);
        }

        /// <summary>
        /// Represents an <see cref="ActionResult"/> that when executed will
        /// execute a callback to write the file content out as a stream.
        /// </summary>
        public class FileCallbackResult : FileResult
        {
            private Func<Stream, ActionContext, Task> _callback;

            /// <summary>
            /// Creates a new <see cref="FileCallbackResult"/> instance.
            /// </summary>
            /// <param name="contentType">The Content-Type header of the response.</param>
            /// <param name="callback">The stream with the file.</param>
            public FileCallbackResult(string contentType, Func<Stream, ActionContext, Task> callback)
                : this(MediaTypeHeaderValue.Parse(contentType), callback)
            {
            }

            /// <summary>
            /// Creates a new <see cref="FileCallbackResult"/> instance.
            /// </summary>
            /// <param name="contentType">The Content-Type header of the response.</param>
            /// <param name="callback">The stream with the file.</param>
            public FileCallbackResult(MediaTypeHeaderValue contentType, Func<Stream, ActionContext, Task> callback)
                : base(contentType?.ToString())
            {
                Callback = callback ?? throw new ArgumentNullException(nameof(callback));
            }

            /// <summary>
            /// Gets or sets the callback responsible for writing the file content to the output stream.
            /// </summary>
            public Func<Stream, ActionContext, Task> Callback
            {
                get { return _callback; }
                set { _callback = value ?? throw new ArgumentNullException(nameof(value)); }
            }

            /// <inheritdoc />
            public override Task ExecuteResultAsync(ActionContext context)
            {
                if (context == null)
                    throw new ArgumentNullException(nameof(context));

                var executor =
                    new FileCallbackResultExecutor(context.HttpContext.RequestServices
                        .GetRequiredService<ILoggerFactory>());
                return executor.ExecuteAsync(context, this);
            }
        }


        /// <summary>
        /// An action result handler of type file
        /// </summary>
        internal sealed class FileCallbackResultExecutor : FileResultExecutorBase
        {
            /// <summary>
            /// Creating an instance of a class <see cref="FileCallbackResultExecutor"/>
            /// </summary>
            /// <param name="loggerFactory"></param>
            public FileCallbackResultExecutor(ILoggerFactory loggerFactory)
                : base(CreateLogger<FileCallbackResultExecutor>(loggerFactory))
            {
            }

            /// <summary>
            /// Handler execution
            /// </summary>
            /// <param name="context">Action context</param>
            /// <param name="result">Action result</param>
            /// <returns><see cref="Task"/></returns>
            public Task ExecuteAsync(ActionContext context, FileCallbackResult result)
            {
                SetHeadersAndLog(context, result, null, result.EnableRangeProcessing);
                return result.Callback(context.HttpContext.Response.Body, context);
            }
        }
    }
}
