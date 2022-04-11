using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Hosting;
class MyWebApplication : WebApplicationFactory<Program>
{
    protected override IHost CreateHost(IHostBuilder builder)
    {
        // shared extra set up goes here
        return base.CreateHost(builder);
    }
}