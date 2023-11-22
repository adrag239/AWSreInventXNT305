using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Amazon.Comprehend;
using Amazon.Comprehend.Model;
using System.Text;
using Amazon.Translate;
using Amazon.Translate.Model;

namespace AIServicesDemoApp.Pages;

public class IndexModel(IAmazonComprehend comprehendClient, IAmazonTranslate translateClient)
    : PageModel
{
    [BindProperty]
        public string Text { get; set; } = string.Empty; 
        public string Result { get; set; } = string.Empty;

        public void OnGet()
        {
            
        }

        public async Task OnPostLanguageAsync()
        {
            // detect language
            var detectLanguageRequest = new DetectDominantLanguageRequest()
            {
                Text = Text
            };
            var detectLanguageResponse = await comprehendClient.DetectDominantLanguageAsync(detectLanguageRequest);
            var languageCode = detectLanguageResponse.Languages[0].LanguageCode;
            
            // translate text from detected language to English
            var translateRequest = new TranslateTextRequest()
            {
                Text = Text,
                SourceLanguageCode = languageCode,
                TargetLanguageCode = "en"
            };
            var translateResponse = await translateClient.TranslateTextAsync(translateRequest);
            Result = translateResponse.TranslatedText;  
            
            
        }

        public async Task OnPostEntitiesAsync()
        {
            // detect entities
            var detectEntitiesRequest = new DetectEntitiesRequest()
            {
                Text = Text,
                LanguageCode = "en"
            };
            var detectEntitiesResponse = await comprehendClient.DetectEntitiesAsync(detectEntitiesRequest);
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("Entities:<br>");
            stringBuilder.AppendLine("==========================<br>");
            foreach (var entity in detectEntitiesResponse.Entities)
            {
                stringBuilder.AppendFormat(
                    "Text: <b>{0}</b>, Type: <b>{1}</b>, Score: <b>{2}</b>, Offset: {3}-{4}<br>",
                    Text.Substring(entity.BeginOffset, entity.EndOffset - entity.BeginOffset),
                    entity.Type,
                    entity.Score,
                    entity.BeginOffset,
                    entity.EndOffset);
            }
            Result = stringBuilder.ToString();
        }

        public async Task OnPostPIIAsync()
        {
            var request = new DetectPiiEntitiesRequest()
            {
                Text = Text,
                LanguageCode = "en"
            };

            var response = await comprehendClient.DetectPiiEntitiesAsync(request);

            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("PII:<br>");
            stringBuilder.AppendLine("==========================<br>");

            foreach (var entity in response.Entities)
            {
                stringBuilder.AppendFormat(
                    "Text: <b>{0}</b>, Type: <b>{1}</b>, Score: <b>{2}</b>, Offset: {3}-{4}<br>",
                    Text.Substring(entity.BeginOffset, entity.EndOffset - entity.BeginOffset),
                    entity.Type,
                    entity.Score,
                    entity.BeginOffset,
                    entity.EndOffset);
            }

            Result = stringBuilder.ToString();

        }
}