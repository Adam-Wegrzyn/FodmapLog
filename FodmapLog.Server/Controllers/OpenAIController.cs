using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OpenAI.Chat;

namespace FodmapLog.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OpenAIController : ControllerBase
    {
        [HttpPost]
        [Route("GeneratemealLogFromAI")]
        public async Task<IActionResult> GeneratemealLogFromAI([FromBody] TranscribedInput input)
        {
            var jsonExample = @"[
   {
        ""date"": ""2025-05-10T06:11:08.945"",
        ""mealLog"": {
          ""date"": ""2025-05-10T06:11:08.945"",
          ""productQuantity"": [
            {
              ""product"": {
                ""name"": ""płatki ryżowe"",
                ""id"": 6
              },
              ""quantity"": 100,
              ""unit"": ""Glass"",
              ""id"": 6
            },
            {
              ""product"": {
                ""name"": ""mleko"",
                ""id"": 7
              },
              ""quantity"": 1,
              ""unit"": 250,
              ""id"": 7
            }
          ],
          ""id"": 5
        },
        ""symptomsLog"": null,
        ""id"": 0
      },
      {
        ""date"": ""2025-05-10T08:31:00"",
        ""mealLog"": null,
        ""symptomsLog"": {
          ""date"": ""2025-05-10T08:31:00"",
          ""symptoms"": [
            {
              ""symptomType"": ""Nausea"",
              ""symptomScale"": 1
            },
            {
              ""symptomType"": ""Burping"",
              ""symptomScale"": 2
            }
          ],
          ""id"": 4
        },
        ""id"": 0
      }
    ]";
            var apiKey = "";
            ChatClient client = new(model: "gpt-4o", apiKey: apiKey);

            var prompt =
                $@"Convert the following user input (Meal and symptom dairy) into a JSON format with no additional text,
                without any formatting, code blocks, or extra characters.. Return only the JSON.
                User Input: '{input.Transcript}'
                JSON Format Example:
                {jsonExample}
                Output only the JSON in this format based on the provided input.";

            ChatCompletion completion = await client.CompleteChatAsync(prompt);

                

            var result = completion.Content[0].Text;
            return Ok(result);
        }
    }
    public class TranscribedInput
    {
        public string Transcript { get; set; }
    }
}

