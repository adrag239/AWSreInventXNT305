using Amazon.Rekognition;
using Amazon.Rekognition.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text;
using SixLabors.ImageSharp;

namespace AIServicesDemoApp.Pages
{
    public class IdentityCheckModel(IAmazonRekognition rekognitionClient, IWebHostEnvironment hostEnvironment)
        : PageModel
    {
        [BindProperty]
        public IFormFile? IdentityFormFile { get; set; }
        [BindProperty]
        public IFormFile? PhotoFormFile { get; set; }
        public string IdentityFileName { get; set; } = String.Empty;
        public string PhotoFileName { get; set; } = String.Empty;
        public string Result { get; set; } = String.Empty;

        public void OnGet()
        {
        }

        public async Task OnPostIdentitiesAsync()
        {
            if ((IdentityFormFile == null) || (PhotoFormFile == null))
            {
                return;
            }
            // save id to display it
            IdentityFileName = String.Format("{0}{1}", Guid.NewGuid().ToString(), System.IO.Path.GetExtension(IdentityFormFile.FileName));
            var fullIdentityFileName = System.IO.Path.Combine(hostEnvironment.WebRootPath, "uploads", IdentityFileName);

            await using (var stream = new FileStream(fullIdentityFileName, FileMode.Create))
            {
                await IdentityFormFile.CopyToAsync(stream);
            }

            // save photo to display it
            PhotoFileName = String.Format("{0}{1}", Guid.NewGuid().ToString(), System.IO.Path.GetExtension(PhotoFormFile.FileName));
            var fullPhotoFileName = System.IO.Path.Combine(hostEnvironment.WebRootPath, "uploads", PhotoFileName);

            await using (var stream = new FileStream(fullPhotoFileName, FileMode.Create))
            {
                await PhotoFormFile.CopyToAsync(stream);
            }

            var identityMemoryStream = new MemoryStream();
            await IdentityFormFile.CopyToAsync(identityMemoryStream);

            var photoMemoryStream = new MemoryStream();
            await PhotoFormFile.CopyToAsync(photoMemoryStream);

            // create request
            var compareFacesRequest = new CompareFacesRequest()
            {
                SourceImage = new Amazon.Rekognition.Model.Image { Bytes = identityMemoryStream },
                TargetImage = new Amazon.Rekognition.Model.Image { Bytes = photoMemoryStream }
            };

            var compareFacesResponse = await rekognitionClient.CompareFacesAsync(compareFacesRequest);

            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("Text:<br>");
            stringBuilder.AppendLine("==========================<br>");

            if (compareFacesResponse.FaceMatches.Count > 0)
            {
                stringBuilder.AppendFormat(
                    "Face similarity: <b>{0}</b><br>",
                    compareFacesResponse.FaceMatches[0].Similarity);

                // Load image to modify with face bounding box rectangle
                using (var image = await SixLabors.ImageSharp.Image.LoadAsync(fullPhotoFileName))
                {
                    var faceDetail = compareFacesResponse.FaceMatches[0].Face;

                    // Get the bounding box
                    var boundingBox = faceDetail.BoundingBox;

                    // Draw the rectangle using the bounding box values
                    image.DrawRectangleUsingBoundingBox(boundingBox);

                    // Save the new image
                    await image.SaveAsJpegAsync(fullPhotoFileName);
                }
            }
            else
            {
                stringBuilder.AppendLine("No matching faces");
            }

            Result = stringBuilder.ToString();

        }
    }
}
