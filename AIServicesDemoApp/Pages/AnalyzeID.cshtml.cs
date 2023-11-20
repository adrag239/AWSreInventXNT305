using Amazon.Textract;
using Amazon.Textract.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text;
using System.Text.Encodings.Web;

namespace AIServicesDemoApp.Pages
{
    public class AnalyzeIDModel(IAmazonTextract textractClient, IWebHostEnvironment hostEnvironment)
        : PageModel
    {
        [BindProperty]
        public IFormFile? FormFile { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string Result { get; set; } = string.Empty;

        public void OnGet()
        {
        }

        public async Task OnPostAsync()
        {
            if (FormFile == null)
                return;
            
            // save image to display it
            FileName = $"{Guid.NewGuid().ToString()}{System.IO.Path.GetExtension(FormFile.FileName)}";
            var fullFileName = System.IO.Path.Combine(hostEnvironment.WebRootPath, "uploads", FileName);

            await using (var stream = new FileStream(fullFileName, FileMode.Create))
            {
                await FormFile.CopyToAsync(stream);
            }

            var memoryStream = new MemoryStream();
            await FormFile.CopyToAsync(memoryStream);

            var request = new AnalyzeIDRequest()
            {
                DocumentPages = new List<Document> { new Document { Bytes = memoryStream }}
            };

            var response = await textractClient.AnalyzeIDAsync(request);

            var stringBuilder = new StringBuilder();

            foreach (var document in response.IdentityDocuments)
            {
                stringBuilder.AppendLine("==========================<br>");
                foreach (var field in document.IdentityDocumentFields)
                {
                    stringBuilder.AppendFormat(
                        "Type: <b>{0}</b>, Text: <b>{1}</b><br>",
                        field.Type.Text,
                        field.ValueDetection.Text);   
                }
            }

            Result = stringBuilder.ToString();
        }
    }
}
