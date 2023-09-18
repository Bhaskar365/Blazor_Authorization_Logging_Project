using LearnAPI.Helper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LearnAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly IWebHostEnvironment environment;
        public ProductController(IWebHostEnvironment environment) 
        {
            this.environment = environment;
        }


        //Upload Image
        [HttpPut("UploadImage")]
        public async Task<IActionResult> UploadImage(IFormFile formFile, string productCode) 
        {
            APIResponse response = new APIResponse();
            try
            {
                string FilePath = GetFilePath(productCode);
                if (!System.IO.Directory.Exists(FilePath))
                {
                    System.IO.Directory.CreateDirectory(FilePath);
                }

                string imagepath = FilePath + "\\" + productCode + ".png";
                if (System.IO.File.Exists(imagepath)) 
                {
                    System.IO.File.Delete(imagepath);
                }
                using (FileStream stream = System.IO.File.Create(imagepath)) 
                {
                    await formFile.CopyToAsync(stream);
                    response.ResponseCode = 200;
                    response.Result = "pass";
                }
            }
            catch (Exception ex)
            {
                response.ErrorMessage = ex.Message;
            }
            return Ok(response);
        }


        //Upload Multiple Image
        [HttpPut("MultiUploadImage")]
        public async Task<IActionResult> MultiUploadImage(IFormFileCollection fileCollection, string productCode)
        {

            APIResponse response = new APIResponse();
            int passCount = 0;
            int errorCount = 0;
             
            try
            {
                string FilePath = GetFilePath(productCode);
                
                if (!System.IO.Directory.Exists(FilePath))
                {
                    System.IO.Directory.CreateDirectory(FilePath);
                }

                foreach (var file in fileCollection) 
                {
                    string imagePath = FilePath + "\\" + file.FileName;

                    if (System.IO.File.Exists(imagePath))
                    {
                        System.IO.File.Delete(imagePath);
                    }
                    using (FileStream stream = System.IO.File.Create(imagePath))
                    {
                        await file.CopyToAsync(stream);
                        passCount++;
                    }
                }
            }
            catch (Exception ex)
            {
                errorCount++;
                response.ErrorMessage = ex.Message;
            }
            response.ResponseCode = 200;
            response.Result = passCount + " Files uploaded & " + errorCount + " files failed";
            return Ok(response);
        }


        //Get Image
        [HttpGet("GetImage")]
        public async Task<IActionResult> GetImage(string productCode)
        {
            string ImageUrl = string.Empty;
            string hostUrl = $"{this.Request.Scheme}://{this.Request.Host}{this.Request.PathBase}";
            try
            {
                string FilePath = GetFilePath(productCode);
                string imagePath = FilePath + "\\" + productCode + ".png";
                if (System.IO.File.Exists(imagePath)) 
                {
                    ImageUrl = hostUrl + "/Upload/product1/" + productCode + "/" + productCode + ".png";
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception)
            {
                throw;
            }
            return Ok(ImageUrl);
        }


        //Get Multiple Image
        [HttpGet("GetMultiImage")]
        public async Task<IActionResult> GetMultiImage(string productCode)
        {
            List<string> ImageUrl = new List<string>();
            string hostUrl = $"{this.Request.Scheme}://{this.Request.Host}{this.Request.PathBase}";
            try
            {
                string Filepath = GetFilePath(productCode);

                if (System.IO.Directory.Exists(Filepath)) 
                {
                    DirectoryInfo directoryInfo = new DirectoryInfo(Filepath);
                    FileInfo[] fileInfos = directoryInfo.GetFiles();
                    // System.IO.FileInfo
                    foreach (FileInfo fileInfo in fileInfos)
                    {
                        string filename = fileInfo.Name;
                        string imagepath = Filepath + "\\" + filename;
                        if (System.IO.File.Exists(imagepath)) 
                        {
                            string _imageUrl = hostUrl + "/Upload/product" + productCode + "/" + filename;
                            ImageUrl.Add(_imageUrl);
                        }  
                    }
                }
            }
            catch (Exception ex)
            {  
                throw;  
            }
            return Ok(ImageUrl);
        }

        //Download Image 
        [HttpGet("download")]
        public async Task<IActionResult> Download(string productCode)
        {
            try
            {
                string FilePath = GetFilePath(productCode);
                string imagePath = FilePath + "\\" + productCode + ".png";
                if (System.IO.File.Exists(imagePath))
                {
                    MemoryStream stream = new MemoryStream();
                    using(FileStream fileStream = new FileStream(imagePath,FileMode.Open)) 
                    {
                        await fileStream.CopyToAsync(stream);
                    }

                    stream.Position = 0;
                    return File(stream, "image/png" , productCode + ".png");
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception)
            {
                return NotFound();
            }
        }

        //Remove Image 
        [HttpGet("remove")]
        public async Task<IActionResult> Remove(string productCode)
        {
            try
            {
                string FilePath = GetFilePath(productCode);
                string imagePath = FilePath + "\\" + productCode + ".png";
                if (System.IO.File.Exists(imagePath))
                {
                    System.IO.File.Delete(imagePath);
                    return Ok("pass");
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception)
            {
                return NotFound();
            }
        }

        //Remove Multiple Image 
        [HttpGet("multiremove")]
        public async Task<IActionResult> MultiRemove(string productCode)
        {
            try
            {
                string Filepath = GetFilePath(productCode);
                if (System.IO.Directory.Exists(Filepath))
                {
                    DirectoryInfo directoryInfo = new DirectoryInfo(Filepath);
                    FileInfo[] fileInfos = directoryInfo.GetFiles();
                    // System.IO.FileInfo
                    foreach (FileInfo fileInfo in fileInfos)
                    {
                        fileInfo.Delete();
                    }
                    return Ok("pass");
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception)
            {
                return NotFound();
            }
        }


        [NonAction]
        public string GetFilePath(string productCode) 
        {
            return this.environment.WebRootPath + "\\Upload\\product" + productCode;
        }
    }
}
