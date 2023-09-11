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

        [NonAction]
        public string GetFilePath(string productCode) 
        {
            return this.environment.WebRootPath + "\\Upload\\product" + productCode;
        }
    }
}
