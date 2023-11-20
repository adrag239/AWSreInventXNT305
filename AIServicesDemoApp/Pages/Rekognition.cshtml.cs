using Amazon.Rekognition;
using Amazon.Rekognition.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text;
using SixLabors.ImageSharp;

namespace AIServicesDemoApp.Pages
{
    public class RekognitionModel : PageModel
    {
        [BindProperty]
        public IFormFile? FormFile { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string NewFileName { get; set; } = string.Empty;
        public string Result { get; set; } = string.Empty;

        private readonly IAmazonRekognition _rekognitionClient;
        private readonly IWebHostEnvironment _hostEnvironment;

        public RekognitionModel(IAmazonRekognition rekognitionClient, IWebHostEnvironment hostEnvironment)
        {
            _rekognitionClient = rekognitionClient;
            _hostEnvironment = hostEnvironment;
        }

        public void OnGet()
        {
        }

        public async Task OnPostFacesAsync()
        {
            if (FormFile == null)
                return;
            
            // save image to display it
            FileName = String.Format("{0}{1}", Guid.NewGuid().ToString(), System.IO.Path.GetExtension(FormFile.FileName));
            var fullFileName = System.IO.Path.Combine(_hostEnvironment.WebRootPath, "uploads", FileName);
            var newFileName = String.Format("{0}_faces{1}", Guid.NewGuid().ToString(), System.IO.Path.GetExtension(FormFile.FileName));

            await using (var stream = new FileStream(fullFileName, FileMode.Create))
            {
                await FormFile.CopyToAsync(stream);
            }

            var memoryStream = new MemoryStream();
            await FormFile.CopyToAsync(memoryStream);

            var detectFacesRequest = new DetectFacesRequest()
            {
                Image = new Amazon.Rekognition.Model.Image { Bytes = memoryStream }
            };

            var detectFacesResponse = await _rekognitionClient.DetectFacesAsync(detectFacesRequest);

            if (detectFacesResponse.FaceDetails.Count > 0)
            {
                // Load image to modify with face bounding box rectangle
                using (var image = SixLabors.ImageSharp.Image.Load(fullFileName))
                {
                    foreach (var faceDetail in detectFacesResponse.FaceDetails)
                    {
                        // Get the bounding box
                        var boundingBox = faceDetail.BoundingBox;

                        // Draw the rectangle using the bounding box values
                        image.DrawRectangleUsingBoundingBox(boundingBox);
                    }

                    // Save the new image
                    await image.SaveAsJpegAsync(System.IO.Path.Combine(_hostEnvironment.WebRootPath, "uploads", newFileName));
                    NewFileName = newFileName;
                }
            }
        }

        public async Task OnPostEntitiesAsync()
        {
            if (FormFile == null)
                return;
            
            // save image to display it
            FileName = String.Format("{0}{1}", Guid.NewGuid().ToString(), System.IO.Path.GetExtension(FormFile.FileName));
            var fullFileName = System.IO.Path.Combine(_hostEnvironment.WebRootPath, "uploads", FileName);

            await using (var stream = new FileStream(fullFileName, FileMode.Create))
            {
                await FormFile.CopyToAsync(stream);
            }

            var memoryStream = new MemoryStream();
            await FormFile.CopyToAsync(memoryStream);

            var detectLabelsRequest = new DetectLabelsRequest()
            {
                Image = new Amazon.Rekognition.Model.Image { Bytes = memoryStream }
            };

            var detectLabelsResponse = await _rekognitionClient.DetectLabelsAsync(detectLabelsRequest);


            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("Labels:<br>");
            stringBuilder.AppendLine("==========================<br>");

            foreach (var label in detectLabelsResponse.Labels)
            {
                stringBuilder.AppendFormat(
                    "Label: <b>{0}</b>, Confidence: <b>{1}</b><br>",
                    label.Name,
                    label.Confidence);
            }

            Result = stringBuilder.ToString();

        }

        public async Task OnPostPPEAsync()
        {
            if (FormFile == null)
                return;
            
            // save image to display it
            FileName = String.Format("{0}{1}", Guid.NewGuid().ToString(), System.IO.Path.GetExtension(FormFile.FileName));
            var fullFileName = System.IO.Path.Combine(_hostEnvironment.WebRootPath, "uploads", FileName);

            await using (var stream = new FileStream(fullFileName, FileMode.Create))
            {
                await FormFile.CopyToAsync(stream);
            }

            var memoryStream = new MemoryStream();
            await FormFile.CopyToAsync(memoryStream);

            var detectPPERequest = new DetectProtectiveEquipmentRequest()
            {
                Image = new Amazon.Rekognition.Model.Image { Bytes = memoryStream }
            };

            var detectPPEResponse = await _rekognitionClient.DetectProtectiveEquipmentAsync(detectPPERequest);

            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("PPE:<br>");
            stringBuilder.AppendLine("==========================<br>");

            foreach (var person in detectPPEResponse.Persons)
            {
                foreach (var bodyPart in person.BodyParts)
                {
                    foreach (var eq in bodyPart.EquipmentDetections)
                    {
                        stringBuilder.AppendFormat(
                            "Body part: <b>{0}</b>, Type: <b>{1}</b>, Covered: <b>{2}</b><br>",
                            bodyPart.Name.Value,
                            eq.Type.Value,
                            eq.CoversBodyPart.Value);
                    }
                }

                Result = stringBuilder.ToString();

            }
        }

        public async Task OnPostTextAsync()
        {
            if (FormFile == null)
                return;
            
            // save image to display it
            FileName = String.Format("{0}{1}", Guid.NewGuid().ToString(), System.IO.Path.GetExtension(FormFile.FileName));
            var fullFileName = System.IO.Path.Combine(_hostEnvironment.WebRootPath, "uploads", FileName);

            await using (var stream = new FileStream(fullFileName, FileMode.Create))
            {
                await FormFile.CopyToAsync(stream);
            }

            var memoryStream = new MemoryStream();
            await FormFile.CopyToAsync(memoryStream);

            var detectTextRequest = new DetectTextRequest()
            {
                Image = new Amazon.Rekognition.Model.Image { Bytes = memoryStream }
            };

            var detectTextResponse = await _rekognitionClient.DetectTextAsync(detectTextRequest);
            
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("Text:<br>");
            stringBuilder.AppendLine("==========================<br>");

            foreach (var text in detectTextResponse.TextDetections)
            {
                if (text.Type == TextTypes.LINE)
                {
                    stringBuilder.AppendFormat(
                        "Detected text: <b>{0}</b><br>",
                        text.DetectedText);
                }

                Result = stringBuilder.ToString();
            }
        }
    }
}
