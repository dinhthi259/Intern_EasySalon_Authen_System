using System.Text;
using System.Text.Json;

public class OpenAiService : IOpenAiService
{
    private readonly IConfiguration _config;
    private readonly HttpClient _httpClient;

    public OpenAiService(IConfiguration config, HttpClient httpClient)
    {
        _config = config;
        _httpClient = httpClient;
    }

    public async Task<string> AskAsync(string prompt)
    {
        var apiKey = _config["Gemini:ApiKey"];
        var model = _config["Gemini:Model"] ?? "gemini-1.5-flash";

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            throw new Exception("Missing Gemini:ApiKey");
        }

        var requestBody = new
        {
            contents = new[]
            {
                new
                {
                    parts = new[]
                    {
                        new { text = prompt }
                    }
                }
            },
            generationConfig = new
            {
                temperature = 0.3,
                maxOutputTokens = 500
            }
        };

        var json = JsonSerializer.Serialize(requestBody);

        var content = new StringContent(
            json,
            Encoding.UTF8,
            "application/json"
        );

        var url =
            $"https://generativelanguage.googleapis.com/v1beta/models/{model}:generateContent?key={apiKey}";

        var response = await _httpClient.PostAsync(url, content);

        var responseString = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception(responseString);
        }

        using var doc = JsonDocument.Parse(responseString);

        return doc.RootElement
            .GetProperty("candidates")[0]
            .GetProperty("content")
            .GetProperty("parts")[0]
            .GetProperty("text")
            .GetString() ?? "";
    }
}