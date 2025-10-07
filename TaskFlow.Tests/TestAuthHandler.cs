using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

public class TestAuthHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    public const string AuthenticationScheme = "Test";
    public const string TestClaimsHeader = "Authorization-Claims";
    public const string UserId = "1a1a1a1a-1a1a-1a1a-1a1a-1a1a1a1a1a1a";

    private class ClaimDTO
    {
        public required string Type { get; set; }
        public required string Value { get; set; }
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (Context.Request.Headers.TryGetValue(TestClaimsHeader, out var claimsValue))
        {
            try
            {
                var jsonBytes = Convert.FromBase64String(claimsValue.ToString());
                var json = Encoding.UTF8.GetString(jsonBytes);
                var claimDTOs = JsonSerializer.Deserialize<IEnumerable<ClaimDTO>>(
                    json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (claimDTOs != null)
                {
                    var claims = claimDTOs.Select(dto => new Claim(dto.Type, dto.Value));
                    var identity = new ClaimsIdentity(claims, AuthenticationScheme);
                    var principal = new ClaimsPrincipal(identity);
                    var ticket = new AuthenticationTicket(principal, AuthenticationScheme);
                    return Task.FromResult(AuthenticateResult.Success(ticket));
                }
            }
            catch (Exception ex)
            {
                return Task.FromResult(AuthenticateResult.Fail(ex));
            }
        }

        return Task.FromResult(AuthenticateResult.Fail("No claims found in request headers."));
    }
}