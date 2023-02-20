using System.Net;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Hosting;
using Web;
namespace CustomMiddleware.Tests;
public class MyAuthTests : IAsyncLifetime
{
    IHost? host;
    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }
    public async Task InitializeAsync()
    {
        host = await new HostBuilder()
            .ConfigureWebHost(webBuilder =>
            {
                webBuilder
                    .UseTestServer()
                    .ConfigureServices(services =>
                    {
                    })
                    .Configure(app =>
                    {
                        app.UseMiddleware<MyAuth>();
                        app.Run(async context =>
                        {
                            await context.Response.WriteAsync("Authenticated!");
                        });
                    });
            })
            .StartAsync();
    }
    [Fact]
    public async Task MiddlewareTest_FailWhenNotAuthenticatedBase()
    {
        var response = await host.GetTestClient().GetAsync("/");
        Assert.NotEqual(HttpStatusCode.NotFound, response.StatusCode);
        var result = await response.Content.ReadAsStringAsync();
        Assert.Equal("Failed!", result);
    }
    [Fact]
    public async Task MiddlewareTest_FailWhenNotAuthenticatedUser()
    {
        var response = await host.GetTestClient().GetAsync("/?username=user1");
        Assert.NotEqual(HttpStatusCode.NotFound, response.StatusCode);
        var result = await response.Content.ReadAsStringAsync();
        Assert.Equal("Failed!", result);
    }
    [Fact]
    public async Task MiddlewareTest_FailWhenNotAuthenticatedPassword()
    {
        var response = await host.GetTestClient().GetAsync("/?password=password1");
        Assert.NotEqual(HttpStatusCode.NotFound, response.StatusCode);
        var result = await response.Content.ReadAsStringAsync();
        Assert.Equal("Failed!", result);
    }
    [Fact]
    public async Task MiddlewareTest_FailWhenNotAuthenticatedWrongUserAndPass()
    {
        var response = await host.GetTestClient().GetAsync("/?username=user2&password=password2");
        Assert.NotEqual(HttpStatusCode.NotFound, response.StatusCode);
        var result = await response.Content.ReadAsStringAsync();
        Assert.Equal("Failed!", result);
    }
    [Fact]
    public async Task MiddlewareTest_Authenticated()
    {
        var response = await host.GetTestClient().GetAsync("/?username=user1&password=password1");
        var result = await response.Content.ReadAsStringAsync();
        Assert.Equal("Authenticated!", result);
    }
}
