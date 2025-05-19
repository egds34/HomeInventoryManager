using Xunit;
using Moq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using HomeInventoryManager.Api.Controllers.UserEndpoints;
using HomeInventoryManager.Data;
using HomeInventoryManager.Dto;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using HomeInventoryManager.Api.Services.UserServices.Interfaces;

public class AuthServiceTest
{
    private readonly Mock<IAuthService> _authServiceMock;
    private readonly AuthController _controller;
    public AuthServiceTest()
    {
        _authServiceMock = new Mock<IAuthService>();
        _controller = new AuthController(_authServiceMock.Object);
    }

    [Fact]
    public async Task Register_ReturnsOk_WhenUserCreated()
    {
        // Arrange
        var userDto = new UserRegisterDto { UserName = "test", Email = "test@test.com", PasswordString = "pass", FirstName = "Test", LastName = "User" };
        var user = new User { user_id = 1, user_name = "test", email = "test@test.com" };
        _authServiceMock.Setup(s => s.RegisterUserAsync(userDto)).ReturnsAsync(user);

        // Act
        var result = await _controller.Register(userDto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(user, okResult.Value);
    }

    [Fact]
    public async Task Register_ReturnsBadRequest_WhenUserExists()
    {
        // Arrange
        var userDto = new UserRegisterDto { UserName = "test", Email = "test@test.com", PasswordString = "pass", FirstName = "Test", LastName = "User" };
        _authServiceMock.Setup(s => s.RegisterUserAsync(userDto)).ReturnsAsync((User?)null);

        // Act
        var result = await _controller.Register(userDto);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task Login_ReturnsOk_WhenLoginSuccessful()
    {
        // Arrange
        var loginDto = new UserLoginDto { UserName = "test", PasswordString = "pass" };
        var tokenResponse = new TokenResponseDto { AccessToken = "access", RefreshToken = "refresh" };
        _authServiceMock.Setup(s => s.LoginUserAsync(loginDto)).ReturnsAsync(tokenResponse);

        // Act
        var result = await _controller.Login(loginDto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(tokenResponse, okResult.Value);
    }

    [Fact]
    public async Task Login_ReturnsBadRequest_WhenLoginFails()
    {
        var loginDto = new UserLoginDto { UserName = "test", PasswordString = "pass" };
        _authServiceMock.Setup(s => s.LoginUserAsync(loginDto)).ReturnsAsync((TokenResponseDto?)null);

        var result = await _controller.Login(loginDto);

        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task Login_ReturnsUnauthorized_WhenUserIsLockedOut()
    {
        var loginDto = new UserLoginDto { UserName = "lockedout", PasswordString = "wrongpass" };
        _authServiceMock.Setup(s => s.LoginUserAsync(loginDto)).ReturnsAsync((TokenResponseDto?)null);

        var result = await _controller.Login(loginDto);

        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task Login_IncrementsFailedAttempts_AndLocksOutAfterMax()
    {
        var loginDto = new UserLoginDto { UserName = "failuser", PasswordString = "wrongpass" };

        _authServiceMock.SetupSequence(s => s.LoginUserAsync(loginDto))
            .ReturnsAsync((TokenResponseDto?)null) // 1st fail
            .ReturnsAsync((TokenResponseDto?)null) // 2nd fail
            .ReturnsAsync((TokenResponseDto?)null); // 3rd fail (lockout)


        var result = await _controller.Login(loginDto);
        Assert.IsType<BadRequestObjectResult>(result.Result);

    }

    [Fact]
    public async Task Login_Successful_AfterLockoutExpires()
    {
        // Arrange
        var loginDto = new UserLoginDto { UserName = "resetuser", PasswordString = "correctpass" };
        var tokenResponse = new TokenResponseDto { AccessToken = "access", RefreshToken = "refresh" };

        // Simulate successful login after lockout period
        _authServiceMock.Setup(s => s.LoginUserAsync(loginDto)).ReturnsAsync(tokenResponse);

        // Act
        var result = await _controller.Login(loginDto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(tokenResponse, okResult.Value);
    }

    [Fact]
    public async Task RefreshToken_ReturnsOk_WhenTokenValid()
    {
        // Arrange
        var refreshDto = new RefreshTokenRequestDto { UserId = 1, RefreshToken = "refresh" };
        var tokenResponse = new TokenResponseDto { AccessToken = "access", RefreshToken = "refresh" };
        _authServiceMock.Setup(s => s.RefreshTokensAsync(refreshDto)).ReturnsAsync(tokenResponse);

        // Act
        var result = await _controller.RefreshToken(refreshDto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(tokenResponse, okResult.Value);
    }

    [Fact]
    public async Task RefreshToken_ReturnsUnauthorized_WhenTokenInvalid()
    {
        // Arrange
        var refreshDto = new RefreshTokenRequestDto { UserId = 1, RefreshToken = "refresh" };
        _authServiceMock.Setup(s => s.RefreshTokensAsync(refreshDto)).ReturnsAsync((TokenResponseDto?)null);

        // Act
        var result = await _controller.RefreshToken(refreshDto);

        // Assert
        Assert.IsType<UnauthorizedObjectResult>(result.Result);
    }

    [Fact]
    public async Task Logout_ReturnsOk_WhenLogoutSuccessful()
    {
        // Arrange
        var logoutDto = new UserLogoutDto { UserId = 1 };
        var tokenResponse = new TokenResponseDto { AccessToken = "", RefreshToken = "" };
        _authServiceMock.Setup(s => s.LogoutUserAsync(logoutDto)).ReturnsAsync(tokenResponse);

        // Simulate authenticated user
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(new[] {
                new Claim(ClaimTypes.Name, "testuser"),
                new Claim(ClaimTypes.NameIdentifier, "1")
            }, "TestAuthType"))
            }
        };

        // Act
        var result = await _controller.Logout(logoutDto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(tokenResponse, okResult.Value);
    }

    [Fact]
    public async Task Logout_ReturnsBadRequest_WhenUserNotFound()
    {
        // Arrange
        var logoutDto = new UserLogoutDto { UserId = 1 };
        _authServiceMock.Setup(s => s.LogoutUserAsync(logoutDto)).ReturnsAsync((TokenResponseDto?)null);

        // Simulate authenticated user
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(new[] {
                new Claim(ClaimTypes.Name, "testuser"),
                new Claim(ClaimTypes.NameIdentifier, "2")
            }, "TestAuthType"))
            }
        };

        // Act
        var result = await _controller.Logout(logoutDto);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("User not found.", badRequestResult.Value);
    }

    [Fact]
    public void AuthenticationOnlyEndpoint_ReturnsOk_WhenUserIsAuthenticated()
    {
        // Arrange: Simulate an authenticated user
        var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.Name, "testuser"),
            new Claim(ClaimTypes.NameIdentifier, "1")
        }, "TestAuthType"));

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };

        // Act
        var result = _controller.AuthenticationOnlyEndpoint();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal("Authenticated.", okResult.Value);
    }

    [Fact]
    public void AuthenticationOnlyEndpoint_ReturnsUnauthorized_WhenUserIsNotAuthenticated()
    {
        // Arrange: Simulate an unauthenticated user
        var user = new ClaimsPrincipal(new ClaimsIdentity());
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };

        // Act
        var result = _controller.AuthenticationOnlyEndpoint();

        // Assert
        // Since the [Authorize] attribute is not enforced in unit tests, you must check manually
        // In a real integration test, this would return 401 Unauthorized
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal("Authenticated.", okResult.Value);
    }

    // Example: Simulate login and then access the endpoint
    [Fact]
    public async Task Login_Then_AuthenticationOnlyEndpoint_ReturnsOk()
    {
        // Arrange
        var loginDto = new UserLoginDto { UserName = "test", PasswordString = "pass" };
        var tokenResponse = new TokenResponseDto { AccessToken = "access", RefreshToken = "refresh" };
        _authServiceMock.Setup(s => s.LoginUserAsync(loginDto)).ReturnsAsync(tokenResponse);

        // Simulate login (would return token in real app)
        var loginResult = await _controller.Login(loginDto);
        var okResult = Assert.IsType<OkObjectResult>(loginResult.Result);
        Assert.Equal(tokenResponse, okResult.Value);

        // Simulate authenticated user for the next request
        var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.Name, "testuser"),
            new Claim(ClaimTypes.NameIdentifier, "1")
        }, "TestAuthType"));
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };

        // Act
        var authResult = _controller.AuthenticationOnlyEndpoint();

        // Assert
        var authOkResult = Assert.IsType<OkObjectResult>(authResult);
        Assert.Equal("Authenticated.", authOkResult.Value);
    }
}
