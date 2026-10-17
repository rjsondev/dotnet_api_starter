using DotnetApiStarter.Application.Common.Authentication;

namespace DotnetApiStarter.Application.Common.Interfaces;

public interface IJwtTokenService
{
    JwtTokenResult GenerateToken(int userId, string username, IEnumerable<string> roles);
}
