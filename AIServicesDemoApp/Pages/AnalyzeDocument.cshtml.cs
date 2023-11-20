using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text;
using Amazon.Textract;
using Amazon.Textract.Model;
using SixLabors.ImageSharp.Formats.Jpeg;

namespace AIServicesDemoApp.Pages
{
    public class AnalyzeDocumentModel(IAmazonTextract textractClient, IWebHostEnvironment hostEnvironment)
        : PageModel
    {
        [BindProperty]
        public IFormFile? FormFile { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string Result { get; set; } = string.Empty;

        public void OnGet()
        {
        }
        
        public async Task OnPostDetectTextAsync()
        {
            if (FormFile == null)
            {
                return;
            }
            // save document image to display it
            FileName = $"{Guid.NewGuid().ToString()}{System.IO.Path.GetExtension(FormFile.FileName)}";
            var fullFileName = System.IO.Path.Combine(hostEnvironment.WebRootPath, "uploads", FileName);

            await using (var stream = new FileStream(fullFileName, FileMode.Create))
            {
                await FormFile.CopyToAsync(stream);
            }

            var memoryStream = new MemoryStream();
            await FormFile.CopyToAsync(memoryStream);

            var detectDocumentTextRequest = new DetectDocumentTextRequest()
            {
                Document = new Document { Bytes = memoryStream }
            };

            var detectDocumentTextResponse = await textractClient.DetectDocumentTextAsync(detectDocumentTextRequest);

            var stringBuilder = new StringBuilder();

            // Load image to modify with face bounding box rectangle
            using (var image = await SixLabors.ImageSharp.Image.LoadAsync(fullFileName))
            {
                foreach (var block in detectDocumentTextResponse.Blocks)
                {
                    if (block.BlockType.Value == "LINE")
                    {
                        stringBuilder.AppendFormat("{0}<br>", block.Text);

                        // Get the bounding box
                        var boundingBox = block.Geometry.BoundingBox;

                        // Draw the rectangle using the bounding box values
                        image.DrawRectangleUsingBoundingBox(boundingBox);
                    }
                }

                // Save the new image
                await image.SaveAsJpegAsync(fullFileName, new JpegEncoder { ColorType = JpegEncodingColor.Rgb});
            }
            
            Result = stringBuilder.ToString();
        }

        public async Task OnPostDetectFormsAsync()
        {
            if (FormFile == null)
            {
                return;
            }
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
                FeatureTypes = new List<string> {"FORMS"}
            };

            var analyzeDocumentResponse = await textractClient.AnalyzeDocumentAsync(analyzeDocumentRequest);

            var stringBuilder = new StringBuilder();

            // Load image to modify with face bounding box rectangle
            using (var image = await SixLabors.ImageSharp.Image.LoadAsync(fullFileName))
            {
                foreach (var block in analyzeDocumentResponse.Blocks)
                {
                    if (block.BlockType.Value == "KEY_VALUE_SET")
                    {
                        foreach (var relation in block.Relationships)
                        {
                            if (relation.Type == RelationshipType.CHILD) {
                                foreach (var id in relation.Ids)
                                {
                                    var related = analyzeDocumentResponse.Blocks.First(b => b.Id == id);

                                    stringBuilder.AppendFormat("{0} ", related.Text);    
                                }
                            }
                            
                            stringBuilder.AppendFormat("<br>");    
                        }
                        
                        // Get the bounding box
                        var boundingBox = block.Geometry.BoundingBox;

                        // Draw the rectangle using the bounding box values
                        image.DrawRectangleUsingBoundingBox(boundingBox);
                    }
                }

                // Save the new image
                await image.SaveAsJpegAsync(fullFileName, new JpegEncoder { ColorType = JpegEncodingColor.Rgb});
            }

            Result = stringBuilder.ToString();
        }
        
        public async Task OnPostDetectTablesAsync()
        {
            if (FormFile == null)
            {
                return;
            }
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
                FeatureTypes = new List<string> {"TABLES"}
            };

            var analyzeDocumentResponse = await textractClient.AnalyzeDocumentAsync(analyzeDocumentRequest);

            var stringBuilder = new StringBuilder();

            // Load image to modify with face bounding box rectangle
            using (var image = await SixLabors.ImageSharp.Image.LoadAsync(fullFileName))
            {
                foreach (var block in analyzeDocumentResponse.Blocks)
                {
                    if (block.BlockType.Value == "TABLE")
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

            Result = stringBuilder.ToString();
        }
       
    }
}
