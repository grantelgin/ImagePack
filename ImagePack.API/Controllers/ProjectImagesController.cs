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
using System.Text;
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
            return CreateImageZipStream(projectId, locators);
        }

        /// <summary>
        /// Adapted from https://blog.stephencleary.com/2016/11/streaming-zip-on-aspnet-core.html
        /// Allows the user to start downloading a zip of project images immediately. The zip is populated while the user downloads the file.
        /// </summary>
        /// <param name="projectId">THe projectId for which images shall be returned.</param>
        /// <param name="imageLocators">A collection of image locators for this project.</param>
        /// <returns>A FileCallbackResult. Represents an ActionResult that when executed will execute a callback to write the file content out as a stream.</returns>
        private IActionResult CreateImageZipStream(int projectId, IEnumerable<ImageLocator> imageLocators)
        {
            var syncIOFeature = HttpContext.Features.Get<IHttpBodyControlFeature>();
            if (syncIOFeature != null)
            {
                syncIOFeature.AllowSynchronousIO = true; // An InvalidOperation exception is thrown without this! 
            }
            
            return new FileCallbackResult(new MediaTypeHeaderValue("application/zip"), StreamZipFileAsync(imageLocators))
            {
                FileDownloadName = $"ProjectImageCollection_{projectId}.zip"
            };
        }

        /// <summary>
        /// Creates a zip of project images. The Zip is populated as a stream. The stream is returned for use in a FileCallbackResult.
        /// If an error occurs loading an image, a {filename}_Error.log file will be included containing error details.  
        /// </summary>
        /// <param name="imageLocators">Collection of images to be included in the Zip.</param>
        /// <returns>A Zip file getting populated asynchronously as a Stream</returns>
        private Func<Stream, ActionContext, Task> StreamZipFileAsync(IEnumerable<ImageLocator> imageLocators)
        {

            return async (outputStream, _) =>
            {
                // create a new ZipArchive using the Stream defined in the 'local' method signature.
                using (var zipArchive = new ZipArchive(outputStream, ZipArchiveMode.Create))
                {
                    // add an entry to the zip file for each image in the imageLocators collection.
                    foreach (ImageLocator locator in imageLocators)
                    {
                        // the file compression should complete as quickly as possible, even if the file is not optimally compressed.
                        var zipEntry = zipArchive.CreateEntry(locator.filename, CompressionLevel.Fastest);

                        await using (var zipStream = zipEntry.Open())
                        {
                            try
                            {
                                MemoryStream imgStream = GetStreamFromUrl(locator.url);
                                await imgStream.CopyToAsync(zipStream);
                            }
                            catch (Exception ex)
                            {
                                // something went wrong with this file. Add an error log.
                                zipStream.Dispose();
                                string erroLogFileName =
                                    $"{Path.GetFileNameWithoutExtension(locator.filename)}_Error.log";

                                var zipErrorEntry = zipArchive.CreateEntry(erroLogFileName, CompressionLevel.Fastest);

                                await using (var errorStream = zipErrorEntry.Open())
                                {
                                    string errorContents =
                                        $"An error occurred adding image '{locator.filename}' to the archive. {ex}";
                                    await errorStream.WriteAsync(Encoding.UTF8.GetBytes(errorContents));
                                }
                            }

                        }

                    }
                }
            };
        }

        /// <summary>
        /// Helper method for retrieving image data from a CDN. Returns the image data as an im-memory stream.
        /// </summary>
        /// <param name="url">url to the image data on a CDN.</param>
        /// <returns>image data as an im-memory stream</returns>
        private static MemoryStream GetStreamFromUrl(string url)
        {
            byte[] imageData = null;

            using (var wc = new System.Net.WebClient())
            {
                imageData = wc.DownloadData(url);
            }

            return new MemoryStream(imageData);
        }

        /// <summary>
        /// Adapted from https://blog.stephencleary.com/2016/11/streaming-zip-on-aspnet-core.html
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
