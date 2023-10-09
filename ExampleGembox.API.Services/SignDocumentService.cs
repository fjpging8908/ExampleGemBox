
using ExapleGembox.API.Services.Interfaces;
using ExapleGembox.Data.Dto;
using GemBox.Pdf;
using GemBox.Pdf.Annotations;
using GemBox.Pdf.Content;
using GemBox.Pdf.Forms;
using GemBox.Pdf.Security;
using Microsoft.Extensions.Configuration;

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using SysX509 = System.Security.Cryptography.X509Certificates;

namespace ExapleGembox.API.Services
{
    public class SignDocumentService : ISignDocumentService
    {

        private readonly IConfiguration _config;
        private readonly string _licensePDF;
        private readonly string _certificate;
        private readonly string _privateKey;
        private readonly string _reason;
        private readonly string _location;
        private readonly string _logoLP;
        private readonly bool _signEnabled;

        public SignDocumentService()
        {
                      
        }

        /// <summary>
        /// Sign PDF Documents with digital certificate from array of internalId's
        /// </summary>
        /// <param name="internalIds"></param>
        /// <returns>return a .Zip file with all PDF signed documents</returns>
        public async Task<FileDto> GenerateDocument()
        {            
            MemoryStream resultPDF = new MemoryStream();
            var process = new List<dynamic>();
            try
            {                                               
                    PdfDocument _pdfDocument = BuildPDF();                   
                    _pdfDocument.Save(resultPDF);                                    
                return new FileDto()
                {
                    FileName = "Export_" + DateTime.Now.ToString("yyyyMMddhhmmss") + ".pdf",
                    FileContent = resultPDF.ToArray(),
                    ContentType = "application/pdf"
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR in Pop generation. detail exception: " + ex.Message + "\n inner exception: " + ex.InnerException?.Message);
                return null;
            }
        }


        #region BuildingPDF with GemBox Library
        public PdfDocument BuildPDF()
        {

            ComponentInfo.SetLicense("FREE-LIMITED-KEY");

            using (var document = new PdfDocument())
            {
                // Add a page.
                var page = document.Pages.Add();
                AddTittle(page);
                AddLine(page);
                AddRectangle(page);                
                AddImage(page);


                return document;
            }
        }
     
        public void AddTittle(PdfPage page)
        {
            using (var formattedText = new PdfFormattedText())
            {
                // Add a Title
                formattedText.FontFamily = new PdfFontFamily("Calibri");
                formattedText.FontSize = 18;
                formattedText.TextAlignment = PdfTextAlignment.Right;
                formattedText.FontWeight = PdfFontWeight.Bold;
                formattedText.AppendLine("Example GemBox");

                page.Content.DrawText(formattedText, new PdfPoint(page.CropBox.Width - formattedText.MaxTextWidth - 25,
                       page.CropBox.Top - 106));

            }
        }

        public void AddRectangle(PdfPage page)
        {
            var pageBounds = page.CropBox;
            // Add a filled and stroked rectangle in the middle of the page.
            var rectangle = page.Content.Elements.AddPath();
            // NOTE: The start point of the rectangle is the bottom left corner of the rectangle.
            rectangle.AddRectangle(new PdfPoint(0, pageBounds.Top - 300),
                new PdfSize(pageBounds.Width, 120));
            var rectangleFormat = rectangle.Format;
            rectangleFormat.Fill.IsApplied = true;
            rectangleFormat.Fill.Color = PdfColor.FromGray(0.95);
            rectangleFormat.Stroke.IsApplied = true;
            rectangleFormat.Stroke.Width = 0.5;
            rectangleFormat.Stroke.Color = PdfColor.FromGray(1);
        }

        public void AddLine(PdfPage page)
        {
            var pageBounds = page.CropBox;

            var line = page.Content.Elements.AddPath();
            line.BeginSubpath(new PdfPoint(10, pageBounds.Top - 110)).
                LineTo(new PdfPoint(pageBounds.Right - 10, pageBounds.Top - 110));
            var lineFormat = line.Format;
            lineFormat.Stroke.IsApplied = true;
            lineFormat.Stroke.Width = 2;
            lineFormat.Stroke.Color = PdfColor.FromRgb(0.2078, 0.5058, 0.6509);
        }
      
        /// <summary>
        /// Add a image from base64 representation at top of the document. 
        /// </summary>
        /// <param name="page"></param>
        public void AddImage(PdfPage page)
        {
            int P = 0;
            try
            {

                string base64ImgRep = "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";
                P = 10;
                byte[] bytes = Convert.FromBase64String(base64ImgRep);
                P = 20;
                MemoryStream imgByes = new MemoryStream(bytes);
                P = 25;
                var image = PdfImage.Load(imgByes);

                double x = 25, y = page.CropBox.Top - 90;
                P = 30;
                // Draw the image to the page.
                page.Content.DrawImage(image, new PdfPoint(x, y));
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR in Pop generation, Drawing Imagen.  detail exception: P=" + P.ToString() + "; Message: " + ex.Message + "\n inner exception: " + ex.InnerException?.Message);
            }
        }
       
        #endregion


     
    }
}
