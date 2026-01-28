using BookLibrary.Application.Interfaces;
using BookLibrary.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace BookLibrary.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IBookService, BookService>();
        services.AddScoped<IPlaceService, PlaceService>();
        
        return services;
    }
}
