using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Amazon.Textract;
using Amazon.Textract.Model;
using SixLabors.ImageSharp.Formats.Jpeg;

namespace AIServicesDemoApp.Pages
{
    public class DetectSignaturesModel(IAmazonTextract textractClient, IWebHostEnvironment hostEnvironment)
        : PageModel
    {
        [BindProperty]
        public IFormFile? FormFile { get; set; }
        public string FileName { get; set; } = string.Empty;

        public void OnGet()
        {
        }
        
        public async Task OnPostDetectSignaturesAsync()
        {
            if (FormFile == null)
                return;

            // save document image to display it
            FileName = $"{Guid.NewGuid().ToString()}{System.IO.Path.GetExtension(FormFile.FileName)}";
            var fullFileName = System.IO.Path.Combine(hostEnvironment.WebRootPath, "uploads", FileName);

            await using (var stream = new FileStream(fullFileName, FileMode.Create))
            {
                await FormFile.CopyToAsync(stream);
            }

            var memoryStream = new MemoryStream();
            await FormFile.CopyToAsync(memoryStream);

            var analyzeDocumentRequest = new AnalyzeDocumentRequest
            {
                Document = new Document { Bytes = memoryStream },
                FeatureTypes = new List<string> {"SIGNATURES"}
            };

            var analyzeDocumentResponse = await textractClient.AnalyzeDocumentAsync(analyzeDocumentRequest);
            
            // Load image to modify with bounding box rectangle
            using (var image = await SixLabors.ImageSharp.Image.LoadAsync(fullFileName))
            {
                foreach (var block in analyzeDocumentResponse.Blocks)
                {
                    if (block.BlockType.Value == "SIGNATURE")
                    {
                        // Get the bounding box
                        var boundingBox = block.Geometry.BoundingBox;

                        // Draw the rectangle using the bounding box values
                        image.DrawRectangleUsingBoundingBox(boundingBox);
                    }
                }

                // Save the new image
                await image.SaveAsJpegAsync(fullFileName, new JpegEncoder { ColorType = JpegEncodingColor.Rgb});
            }
        }
    }
}
