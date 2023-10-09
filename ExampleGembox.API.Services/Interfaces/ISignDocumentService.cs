

using ExapleGembox.Data.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExapleGembox.API.Services.Interfaces
{
    public interface ISignDocumentService
    {
        Task<FileDto> GenerateDocument();
    }
}
