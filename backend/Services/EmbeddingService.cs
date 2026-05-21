using System.Text;
using System.Text.Json;

public class EmbeddingService : IEmbeddingService
{
    private readonly HttpClient _http;
    private readonly IConfiguration _config;

    public EmbeddingService(HttpClient http, IConfiguration config)
    {
        _http = http;
        _config = config;
    }

    public async Task<List<float>> CreateEmbeddingAsync(string text)
    {
        var apiKey = _config["Gemini:ApiKey"];

        if (string.IsNullOrWhiteSpace(apiKey))
            throw new Exception("Missing Gemini:ApiKey");

        var body = new
        {
            model = "models/gemini-embedding-001",
            content = new
            {
                parts = new[]
                {
                    new { text = text }
                }
            }
        };

        var url =
            $"https://generativelanguage.googleapis.com/v1beta/models/gemini-embedding-001:embedContent?key={apiKey}";

        var res = await _http.PostAsync(
            url,
            new StringContent(
                JsonSerializer.Serialize(body),
                Encoding.UTF8,
                "application/json"
            )
        );

        var json = await res.Content.ReadAsStringAsync();

        if (!res.IsSuccessStatusCode)
            throw new Exception(json);

        using var doc = JsonDocument.Parse(json);

        var arr = doc.RootElement
            .GetProperty("embedding")
            .GetProperty("values");

        return arr.EnumerateArray()
            .Select(x => x.GetSingle())
            .ToList();
    }
}