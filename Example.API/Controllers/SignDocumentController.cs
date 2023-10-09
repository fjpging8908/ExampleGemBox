using ExapleGembox.API.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace Example.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SignDocumentController : ControllerBase
    {
        private readonly ISignDocumentService _signDocumentService;

        public SignDocumentController(ISignDocumentService signDocumentService)
        {
            _signDocumentService = signDocumentService;
        }

        [HttpPost("GeneratePDFDocument")]
        public async Task<IActionResult> UploadFile()
        {            
            var result = await _signDocumentService.GenerateDocument();
            if (result != null)
                return File(result.FileContent, result.ContentType, result.FileName);
            return NotFound();

        }

    }
}