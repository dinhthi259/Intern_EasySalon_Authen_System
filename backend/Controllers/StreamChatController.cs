using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StreamChat.Clients;

[ApiController]
[Route("api/stream-chat")]
public class StreamChatController : ControllerBase
{
    private readonly IConfiguration _config;
    private readonly AppDbContext _context;

    public StreamChatController(IConfiguration config, AppDbContext context)
    {
        _config = config;
        _context = context;
    }

    [HttpGet("token/{userId}")]
    public async Task<IActionResult> GetToken(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            return BadRequest("userId không hợp lệ");
        }

        var user = await _context.Users
            .FirstOrDefaultAsync(x => x.Id.ToString() == userId);

        if (user == null)
        {
            return NotFound("Không tìm thấy user");
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
            userId,
            email = user.Email,
            sellerId = "admin_1"
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