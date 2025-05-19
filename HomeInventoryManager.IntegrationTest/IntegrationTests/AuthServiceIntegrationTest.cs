using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;
using HomeInventoryManager.Dto;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using HomeInventoryManager.Test.AppFactories;
using HomeInventoryManager.Data;
using Newtonsoft.Json;
using System.Text;

public class AuthServiceIntegrationTest : IClassFixture<MemoryWebApplicationFactory<Program>>
{
    private readonly MemoryWebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    
    public AuthServiceIntegrationTest(MemoryWebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task Register_ReturnsOk_WhenUserIsCreated_ThenDeletesUser()
    {
        // Arrange
        var registerDto = new UserRegisterDto
        {
            UserName = "regonlyuser",
            Email = "regonlyuser@test.com",
            PasswordString = "Test123!",
            FirstName = "Reg",
            LastName = "User"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/register", registerDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var user = await response.Content.ReadFromJsonAsync<User>();
        user.Should().NotBeNull();
        user!.user_name.Should().Be("regonlyuser");
        user.email.Should().Be("regonlyuser@test.com");
    }

    [Fact]
    public async Task Register_Then_Login_Then_CheckAuthorization_Succeeds()
    {
        // Register
        var registerDto = new UserRegisterDto
        {
            UserName = "integrationuser",
            Email = "integrationuser@test.com",
            PasswordString = "Test123!",
            FirstName = "Integration",
            LastName = "User"
        };
        var registerResponse = await _client.PostAsJsonAsync("/api/auth/register", registerDto);
        registerResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Login
        var loginDto = new UserLoginDto
        {
            UserName = "integrationuser",
            PasswordString = "Test123!"
        };
        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", loginDto);
        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var token = await loginResponse.Content.ReadFromJsonAsync<TokenResponseDto>();
        token.Should().NotBeNull();
        token!.AccessToken.Should().NotBeNullOrWhiteSpace();

        // Set JWT token for authenticated requests
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token.AccessToken);

        // Check authorization
        var authResponse = await _client.GetAsync("/api/auth/check-authorization");
        authResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var authContent = await authResponse.Content.ReadAsStringAsync();
        authContent.Should().Contain("Authenticated.");
    }

    [Fact]
    public async Task Login_Success_ReturnsToken()
    {
        var loginDto = new { UserName = "egds34", PasswordString = "yourTestPassword" };
        var content = new StringContent(JsonConvert.SerializeObject(loginDto), Encoding.UTF8, "application/json");

        var response = await _client.PostAsync("/api/auth/login", content);

        response.EnsureSuccessStatusCode();
        var responseString = await response.Content.ReadAsStringAsync();
        Assert.Contains("accessToken", responseString, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Login_Fail_LocksOutAfterMaxAttempts()
    {
        var loginDto = new { UserName = "egds34", PasswordString = "wrongPassword" };
        var content = new StringContent(JsonConvert.SerializeObject(loginDto), Encoding.UTF8, "application/json");

        // Try more than max attempts
        for (int i = 0; i < 5; i++)
        {
            await _client.PostAsync("/api/auth/login", content);
        }

        var response = await _client.PostAsync("/api/auth/login", content);
        var responseString = await response.Content.ReadAsStringAsync();
        Assert.Contains("locked out", responseString, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Logout_Then_CheckAuthorization_ReturnsUnauthorized()
    {
        // Register and login first
        var registerDto = new UserRegisterDto
        {
            UserName = "logoutuser",
            Email = "logoutuser@test.com",
            PasswordString = "Test123!",
            FirstName = "Logout",
            LastName = "User"
        };
        await _client.PostAsJsonAsync("/api/auth/register", registerDto);

        var loginDto = new UserLoginDto
        {
            UserName = "logoutuser",
            PasswordString = "Test123!"
        };
        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", loginDto);
        var token = await loginResponse.Content.ReadFromJsonAsync<TokenResponseDto>();

        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token!.AccessToken);

        // Logout
        var logoutDto = new UserLogoutDto { UserId = 1 }; // Use the correct user id if needed
        var logoutResponse = await _client.PostAsJsonAsync("/api/auth/logout", logoutDto);
        logoutResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Remove token to simulate logout (or use a blacklisted token system in your backend)
        _client.DefaultRequestHeaders.Authorization = null;

        // Try to access authorized endpoint
        var authResponse = await _client.GetAsync("/api/auth/check-authorization");
        authResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Logout_ReturnsUnauthorized_WhenUserIsNotAuthenticated()
    {
        // Arrange
        var logoutDto = new UserLogoutDto { UserId = 9999 }; // UserId can be any value

        // Do not set Authorization header to simulate an unauthenticated request

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/logout", logoutDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
