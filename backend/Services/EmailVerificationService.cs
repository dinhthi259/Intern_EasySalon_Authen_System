using BCrypt.Net;

public class EmailVerificationService : IEmailVerificationService
{
    private readonly AppDbContext _context;
    private readonly IEmailSender _emailSender;
    public EmailVerificationService(AppDbContext context, IEmailSender emailSender)
    {
        _context = context;
        _emailSender = emailSender;
    }

    public async Task SendVerificationEmailAsync(User user)
    {
        var rawToken = Convert.ToBase64String(Guid.NewGuid().ToByteArray());
        var token = BCrypt.Net.BCrypt.HashPassword(rawToken);
        var verificationToken = new EmailVerificationToken
        {
            UserId = user.Id,
            Token = token,
            ExpiresAt = DateTime.Now.AddHours(24),
            IsUsed = false,
            CreatedAt = DateTime.Now
        };
        await _context.EmailVerificationTokens.AddAsync(verificationToken);
        await _context.SaveChangesAsync();

        var verificationLink = $"https://intern-easy-salon-authen-system.vercel.app/email-verify?token={Uri.EscapeDataString(rawToken)}";

        await _emailSender.SendEmailAsync(user.Email, "Verify Your Email", $"Please verify your email by clicking the following link: {verificationLink}");
    }

    public async Task VerifyEmailTokenAsync(string token)
    {
        if (string.IsNullOrEmpty(token))
        {
            throw new Exception("Token is required.");
        }
        var tokens = _context.EmailVerificationTokens.Where(t => !t.IsUsed && t.ExpiresAt > DateTime.Now).ToList();
        var matchingToken = tokens.FirstOrDefault(x => BCrypt.Net.BCrypt.Verify(token, x.Token));
        if (matchingToken == null)
        {
            throw new Exception("Invalid or expired token.");
        }
        var user = await _context.Users.FindAsync(matchingToken.UserId);
        if (user == null)
        {
            throw new Exception("User not found.");
        }
        user.EmailVerified = true;
        user.EmailVerifiedAt = DateTime.Now;
        matchingToken.IsUsed = true;
        await _context.SaveChangesAsync();
    }
    
    public async Task sendResetPasswordEmailAsync(User user)
    {
        var rawToken = Convert.ToBase64String(Guid.NewGuid().ToByteArray());
        var tokenHash = BCrypt.Net.BCrypt.HashPassword(rawToken);
        var resetToken = new ResetPasswordToken
        {
            UserId = user.Id,
            TokenHash = tokenHash,
            ExpiresAt = DateTime.Now.AddHours(1),
            IsUsed = false,
            CreatedAt = DateTime.Now
        };
        await _context.ResetPasswordTokens.AddAsync(resetToken);
        await _context.SaveChangesAsync();

        var verificationLink = $"https://intern-easy-salon-authen-system.vercel.app/reset-password?token={Uri.EscapeDataString(rawToken)}";

        await _emailSender.SendEmailAsync(user.Email, "Reset Your Password", $"Please click the following link to reset your password: {verificationLink}, This link will expire in 1 hour.");
    }
}
