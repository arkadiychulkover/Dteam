using System.Security.Claims;
using Microsoft.AspNetCore.SignalR;

namespace DteamBackend.Hubs
{
    public class CustomUserIdProvider : IUserIdProvider
    {
        public string? GetUserId(HubConnectionContext connection)
        {
            var userId = connection.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? connection.User?.FindFirst("sub")?.Value
                ?? connection.User?.FindFirst("nameid")?.Value
                ?? connection.UserIdentifier;

            if (!string.IsNullOrEmpty(userId))
            {
                return userId.ToLowerInvariant();
            }

            try
            {
                var httpContext = connection.GetHttpContext();
                var queryToken = httpContext?.Request.Query["access_token"].ToString();
                if (string.IsNullOrEmpty(queryToken))
                {
                    queryToken = httpContext?.Request.Query["token"].ToString();
                }

                if (!string.IsNullOrEmpty(queryToken))
                {
                    var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
                    if (handler.CanReadToken(queryToken))
                    {
                        var jwt = handler.ReadJwtToken(queryToken);
                        var sub = jwt.Claims.FirstOrDefault(c =>
                            c.Type == System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub ||
                            c.Type == ClaimTypes.NameIdentifier ||
                            c.Type == "nameid")?.Value;

                        if (!string.IsNullOrEmpty(sub))
                        {
                            return sub.ToLowerInvariant();
                        }
                    }
                }
            }
            catch
            {
            }

            return null;
        }
    }
}

