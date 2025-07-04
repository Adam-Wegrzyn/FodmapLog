using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OpenAI.Chat;

namespace FodmapLog.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OpenAIController : ControllerBase
    {
        private string? _apiKey;

        public OpenAIController(IConfiguration configuration)
        {
            _apiKey = configuration["openAIApiKey"];
        }
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
              },
              ""quantity"": 100,
              ""unit"":{
                    ""name"" : ""Kilogram""
                },
            },
            {
              ""product"": {
                ""name"": ""mleko"",
              },
              ""quantity"": 1,
              ""unit"": {
                    ""name"" : ""Liter""
                },
            }
          ],
        },
        ""symptomsLog"": null,
      },
      {
        ""date"": ""2025-05-10T08:31:00"",
        ""mealLog"": null,
        ""symptomsLog"": {
          ""date"": ""2025-05-10T08:31:00"",
          ""symptoms"": [
            {
              ""symptomType"": {
                    ""name"" : ""Nausea""
                },
              ""symptomScale"": 1
            },
            {
              ""symptomType"": {
                    ""name"" : ""Burping""
                },
              ""symptomScale"": 2
            }
          ],
        },
      }
    ]";
            ChatClient client = new(model: "gpt-4o", apiKey: _apiKey);

            var prompt =
                $@"Convert the following user input (Meal and symptom dairy) into a JSON format with no additional text,
                without any formatting, code blocks, or extra characters.. Return only the JSON.
                User Input: '{input.Transcript}'
                Symptom scale please convert to int -> 0 (excellent) - 10 (the worst)
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

