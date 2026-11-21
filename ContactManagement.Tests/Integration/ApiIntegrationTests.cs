using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using ContactManagement.Core.DTOs.Auth;
using ContactManagement.Core.DTOs.Categories;
using ContactManagement.Core.DTOs.Common;
using ContactManagement.Core.DTOs.Contacts;

namespace ContactManagement.Tests.Integration;

public sealed class ApiIntegrationTests
{
    [Fact]
    public async Task ProtectedEndpoint_WithoutToken_ReturnsUnauthorized()
    {
        using var factory = new TestWebApplicationFactory();
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/contacts");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task RegisterThenLogin_ReturnsJwtToken()
    {
        using var factory = new TestWebApplicationFactory();
        using var client = factory.CreateClient();

        var register = await client.PostAsJsonAsync("/api/auth/register", new
        {
            fullName = "Test User",
            email = "test.user@example.com",
            password = "Str0ng!Password"
        });

        register.EnsureSuccessStatusCode();
        var registered = await register.Content.ReadFromJsonAsync<ApiResponse<AuthResponse>>();
        Assert.True(registered!.Success);
        Assert.False(string.IsNullOrWhiteSpace(registered.Data!.Token));

        var login = await client.PostAsJsonAsync("/api/auth/login", new
        {
            email = "test.user@example.com",
            password = "Str0ng!Password"
        });

        login.EnsureSuccessStatusCode();
        var loggedIn = await login.Content.ReadFromJsonAsync<ApiResponse<AuthResponse>>();
        Assert.True(loggedIn!.Success);
        Assert.Equal(registered.Data.UserId, loggedIn.Data!.UserId);
    }

    [Fact]
    public async Task CreateContact_ThenReadAndFavorite_ReturnsOwnedData()
    {
        using var factory = new TestWebApplicationFactory();
        using var client = factory.CreateClient();

        var token = await RegisterAndGetTokenAsync(client, "owner@example.com", "Str0ng!Password");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var created = await client.PostAsJsonAsync("/api/contacts", new
        {
            fullName = "John Doe",
            email = "john@example.com",
            phoneNumber = "+15551234567",
            company = "Acme",
            isFavorite = false
        });

        created.EnsureSuccessStatusCode();
        var createdBody = await created.Content.ReadFromJsonAsync<ApiResponse<ContactDto>>();
        var contactId = createdBody!.Data!.ContactId;

        var fetched = await client.GetFromJsonAsync<ApiResponse<ContactDto>>($"/api/contacts/{contactId}");
        Assert.Equal("John Doe", fetched!.Data!.FullName);
        Assert.Equal("Acme", fetched.Data.Company);

        var favorite = await client.PatchAsync($"/api/contacts/{contactId}/favorite?isFavorite=true", null);
        favorite.EnsureSuccessStatusCode();
        var favoriteBody = await favorite.Content.ReadFromJsonAsync<ApiResponse<ContactDto>>();
        Assert.True(favoriteBody!.Data!.IsFavorite);

        var favorites = await client.GetFromJsonAsync<ApiResponse<IReadOnlyList<ContactDto>>>("/api/contacts/favorites");
        Assert.Contains(favorites!.Data!, c => c.ContactId == contactId);
    }

    [Fact]
    public async Task CategoryAssignment_AndOwnershipIsolation_WorkCorrectly()
    {
        using var factory = new TestWebApplicationFactory();
        using var client = factory.CreateClient();

        var token = await RegisterAndGetTokenAsync(client, "alice@example.com", "Str0ng!Password");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var categoryResponse = await client.PostAsJsonAsync("/api/categories", new
        {
            categoryName = "Work",
            description = "Work contacts"
        });
        categoryResponse.EnsureSuccessStatusCode();
        var category = (await categoryResponse.Content.ReadFromJsonAsync<ApiResponse<CategoryDto>>())!.Data!;

        var contactResponse = await client.PostAsJsonAsync("/api/contacts", new
        {
            fullName = "Bob Smith",
            email = "bob@example.com"
        });
        contactResponse.EnsureSuccessStatusCode();
        var contact = (await contactResponse.Content.ReadFromJsonAsync<ApiResponse<ContactDto>>())!.Data!;

        var assign = await client.PostAsync($"/api/categories/{category.CategoryId}/contacts/{contact.ContactId}", null);
        assign.EnsureSuccessStatusCode();

        var inCategory = await client.GetFromJsonAsync<ApiResponse<IReadOnlyList<ContactDto>>>(
            $"/api/categories/{category.CategoryId}/contacts");
        Assert.Contains(inCategory!.Data!, c => c.ContactId == contact.ContactId);

        using var otherClient = factory.CreateClient();
        var otherToken = await RegisterAndGetTokenAsync(otherClient, "bob@example.com", "Str0ng!Password");
        otherClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", otherToken);

        var forbiddenContact = await otherClient.GetAsync($"/api/contacts/{contact.ContactId}");
        Assert.Equal(HttpStatusCode.NotFound, forbiddenContact.StatusCode);

        var forbiddenCategory = await otherClient.GetAsync($"/api/categories/{category.CategoryId}");
        Assert.Equal(HttpStatusCode.NotFound, forbiddenCategory.StatusCode);
    }

    private static async Task<string> RegisterAndGetTokenAsync(HttpClient client, string email, string password)
    {
        var response = await client.PostAsJsonAsync("/api/auth/register", new
        {
            fullName = "Integration User",
            email,
            password
        });

        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<AuthResponse>>();
        return body!.Data!.Token;
    }
}
