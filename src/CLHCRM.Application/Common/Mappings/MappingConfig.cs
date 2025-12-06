using System.Reflection;
using Mapster;
using Microsoft.Extensions.DependencyInjection;

namespace CLHCRM.Application.Common.Mappings;

/// <summary>
/// Configuration for Mapster mappings
/// </summary>
public static class MappingConfig
{
    public static void RegisterMappings(this IServiceCollection services)
    {
        var config = TypeAdapterConfig.GlobalSettings;
        config.Scan(Assembly.GetExecutingAssembly());
    }
}
