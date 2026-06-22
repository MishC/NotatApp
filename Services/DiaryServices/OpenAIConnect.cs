using Microsoft.OpenApi.Models;
using OpenAI.Responses;

string apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY")!;
var client = new OpenAIResponseClient(model: "gpt-5.5", apiKey: apiKey);

OpenApiResponse response = client.CreateResponse(
    "Write a short bedtime story about a unicorn."
);

Console.WriteLine(response.GetOutputText());