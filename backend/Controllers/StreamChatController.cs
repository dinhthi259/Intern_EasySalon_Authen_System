using Microsoft.AspNetCore.Mvc;
using StreamChat.Clients;

[ApiController]
[Route("api/stream-chat")]
public class StreamChatController : ControllerBase
{
    private readonly IConfiguration _config;

    public StreamChatController(IConfiguration config)
    {
        _config = config;
    }

    [HttpGet("token/{userId}")]
    public IActionResult GetToken(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            return BadRequest("userId không hợp lệ");
        }

        var apiKey = _config["Stream:ApiKey"];
        var apiSecret = _config["Stream:ApiSecret"];

        var clientFactory = new StreamClientFactory(apiKey, apiSecret);
        var userClient = clientFactory.GetUserClient();

        var token = userClient.CreateToken(
            userId,
            expiration: DateTimeOffset.UtcNow.AddHours(24)
        );

        return Ok(new
        {
            apiKey,
            token,
            userId
        });
    }
    [HttpGet("admin-token")]
    public IActionResult GetAdminToken()
    {
        var apiKey = _config["Stream:ApiKey"];
        var apiSecret = _config["Stream:ApiSecret"];

        var clientFactory = new StreamClientFactory(apiKey, apiSecret);
        var userClient = clientFactory.GetUserClient();

        var userId = "admin_1";
        var token = userClient.CreateToken(userId);

        return Ok(new
        {
            apiKey,
            token,
            userId,
            fullName = "Nhân viên tư vấn"
        });
    }
}