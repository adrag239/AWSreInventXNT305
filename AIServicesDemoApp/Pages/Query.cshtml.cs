using Amazon.Textract;
using Amazon.Textract.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;

namespace AIServicesDemoApp.Pages
{
    public class QueryModel : PageModel
    {
        [BindProperty]
        public string Query { get; set; } = string.Empty;

        [BindProperty]
        public IFormFile? FormFile { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string Result { get; set; } = string.Empty;

        private readonly IAmazonTextract _textractClient;
        private readonly IWebHostEnvironment _hostEnvironment;

        public QueryModel(IAmazonTextract textractClient, IWebHostEnvironment hostEnvironment)
        {
            _textractClient = textractClient;
            _hostEnvironment = hostEnvironment;
        }

        public void OnGet()
        {
        }

        public async Task OnPostAsync()
        {
            if (FormFile == null)
                return;

            // save document image to display it
            FileName = $"{Guid.NewGuid().ToString()}{System.IO.Path.GetExtension(FormFile.FileName)}";
            var fullFileName = System.IO.Path.Combine(_hostEnvironment.WebRootPath, "uploads", FileName);

            await using (var stream = new FileStream(fullFileName, FileMode.Create))
            {
                await FormFile.CopyToAsync(stream);
            }
            
            var memoryStream = new MemoryStream();
            await FormFile.CopyToAsync(memoryStream);

            var queryRequest = new AnalyzeDocumentRequest()
            {
                Document = new Document { Bytes = memoryStream },
                FeatureTypes = new List<string> { "QUERIES" },
                QueriesConfig = new QueriesConfig
                {
                    Queries = new List<Query> { new Query { Text = Query } }
                }
            };

            var queryResponse = await _textractClient.AnalyzeDocumentAsync(queryRequest);

            var stringBuilder = new StringBuilder();
            stringBuilder.AppendFormat("Query: <b>{0}</b><br>", Query);

            // Load image to modify with bounding box rectangle
            using (var image = await SixLabors.ImageSharp.Image.LoadAsync(fullFileName))
            {
                foreach (var block in queryResponse.Blocks)
                {
                    if (block.BlockType.Value == "QUERY_RESULT")
                    {
                        stringBuilder.AppendFormat(
                            "Answer: <b>{0}</b>, Confidence: <b>{1}</b><br>",
                            block.Text,
                            block.Confidence);

                        // Get the bounding box
                        var boundingBox = block.Geometry.BoundingBox;

                        // Draw the rectangle using the bounding box values
                        image.DrawRectangleUsingBoundingBox(boundingBox);
                    }
                }
                
                // Save the new image to display it
                await image.SaveAsJpegAsync(fullFileName, new JpegEncoder { ColorType = JpegEncodingColor.Rgb});
            }

            Result = stringBuilder.ToString();
        }
    }
}
