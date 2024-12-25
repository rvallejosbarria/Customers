using Customers.Web.Customers;

namespace Customers.Web;

public class ValidationHelpers
{
    internal static async ValueTask<object?> ValidateAddCustomer(EndpointFilterInvocationContext context,
        EndpointFilterDelegate next)
    {
        var customer = context.GetArgument<Customer>(0);
        if (customer is not null && string.IsNullOrEmpty(customer.CompanyName))
        {
            return Results.ValidationProblem(new Dictionary<string, string[]>()
            {
                { "CompanyName", new[] { "Please enter a valid company name" } }
            });
        }

        return await next(context);
    }

    internal static async ValueTask<object?> ValidateUpdateCustomer(EndpointFilterInvocationContext context,
        EndpointFilterDelegate next)
    {
        var customer = context.GetArgument<Customer>(1);
        if (customer is not null && string.IsNullOrEmpty(customer.CompanyName))
        {
            return Results.ValidationProblem(new Dictionary<string, string[]>()
            {
                { "CompanyName", new[] { "Please enter a valid company name" } }
            });
        }

        return await next(context);
    }
}