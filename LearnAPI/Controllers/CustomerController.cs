using LearnAPI.Modal;
using LearnAPI.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ClosedXML.Excel;
using System.Data;

namespace LearnAPI.Controllers
{
    //[Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class CustomerController : ControllerBase
    {
        private readonly ICustomerService service;
        private readonly IWebHostEnvironment environment;
        public CustomerController(ICustomerService service, IWebHostEnvironment environment) 
        {
            this.service = service;
            this.environment = environment;
        }

        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAll() 
        {
            var data = await this.service.GetAll();
            if (data == null) 
            {
                return NotFound();
            }
            return Ok(data);
        }

        [HttpGet("Getbycode")]
        public async Task<IActionResult> Getbycode(string code)
        {
            var data = await this.service.GetByCode(code);
            if (data == null)
            {
                return NotFound();
            }
            return Ok(data);
        }

        [HttpPost("Create")]
        public async Task<IActionResult> Create(Customermodal _data)
        {
            var data = await this.service.Create(_data);
            return Ok(data);
        }

        [HttpPut("Update")]
        public async Task<IActionResult> Update(Customermodal _data, string code) 
        {
            var data = await this.service.Update(_data, code);
            return Ok(data);
        }

        [HttpDelete("Remove")]
        public async Task<IActionResult> Remove(string code) 
        {
            var data = await this.service.Remove(code);
            return Ok(data);
        }

        [AllowAnonymous]    
        [HttpGet("exportExcel")]
        public async Task<IActionResult> Exportexcel() 
        {
            try
            {
                DataTable dt = new DataTable();
                dt.Columns.Add("Code", typeof(string));
                dt.Columns.Add("Name", typeof(string));
                dt.Columns.Add("Email", typeof(string));
                dt.Columns.Add("Phone", typeof(string));
                dt.Columns.Add("CreditLimit", typeof(int));
                var data = await this.service.GetAll();
                if (data != null && data.Count>0) 
                {
                    data.ForEach(item => 
                    {
                        dt.Rows.Add(item.Code, item.Name, item.Email, item.Phone, item.CreditLimit);
                    });
                }
                using (XLWorkbook wb = new XLWorkbook()) 
                {
                    wb.AddWorksheet(dt,"Customer Info");
                    using (MemoryStream stream = new MemoryStream()) 
                    {
                        wb.SaveAs(stream);
                        return File(stream.ToArray(),"application/vnd.openxmlformats-officedocument.spreadsheet.sheet","Customer.xlsx");
                    }
                     
                }
            }
            catch (Exception)
            {
                return NotFound();
            }
        }

        [AllowAnonymous]
        [HttpGet("ExportexcelInProjSolution")]
        public async Task<IActionResult> ExportexcelInProjSolution()
        {
            try
            {
                string Filepath = GetFilePath();
                string excelpath = Filepath + "\\customerInfo.xlsx";
                DataTable dt = new DataTable();
                dt.Columns.Add("Code", typeof(string));
                dt.Columns.Add("Name", typeof(string));
                dt.Columns.Add("Email", typeof(string));
                dt.Columns.Add("Phone", typeof(string));
                dt.Columns.Add("CreditLimit", typeof(int));
                var data = await this.service.GetAll();
                if (data != null && data.Count > 0)
                {
                    data.ForEach(item =>
                    {
                        dt.Rows.Add(item.Code, item.Name, item.Email, item.Phone, item.CreditLimit);
                    });
                }
                using (XLWorkbook wb = new XLWorkbook())
                {
                    wb.AddWorksheet(dt, "Customer Info");
                    using (MemoryStream stream = new MemoryStream())
                    {
                        wb.SaveAs(stream);

                        if (System.IO.File.Exists(excelpath)) 
                        {
                            System.IO.File.Delete(excelpath);
                        }
                        wb.SaveAs(excelpath);

                        return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheet.sheet", "Customer.xlsx");
                    }

                }
            }
            catch (Exception)
            {
                return NotFound();
            }
        }

        [NonAction]
        public string GetFilePath()
        {
            return this.environment.WebRootPath + "\\Export";
        }

    }
}
 