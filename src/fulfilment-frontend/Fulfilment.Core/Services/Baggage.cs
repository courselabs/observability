using Microsoft.Extensions.DependencyInjection;

namespace Fulfilment.Core.Services
{
    public static class Baggage
    {
        public static IServiceCollection AddBaggageHeaderPropagation(this IServiceCollection services)
        {
            return services.AddHeaderPropagation(options => options.Headers.Add("Correlation-Context", "baggage"));
        }
    }
}

// TODO - check if the propagation works with java 
//TODO - middleware to read incoming "baggage" and set to baggage items