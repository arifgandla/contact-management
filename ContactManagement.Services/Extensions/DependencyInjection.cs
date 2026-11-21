using AutoMapper;
using ContactManagement.Core.Interfaces.Services;
using ContactManagement.Services.Mappings;
using ContactManagement.Services.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ContactManagement.Services.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddSingleton<IMapper>(sp =>
            new MapperConfiguration(
                    config => config.AddProfile<MappingProfile>(),
                    sp.GetRequiredService<ILoggerFactory>())
                .CreateMapper());

        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IContactService, ContactService>();
        services.AddScoped<ICategoryService, CategoryService>();

        return services;
    }
}
