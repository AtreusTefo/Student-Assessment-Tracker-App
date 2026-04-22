using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace StudentAssessmentTracker.Presentation.Swagger
{
    /// <summary>
    /// Operation filter that appends the Bearer security requirement ONLY to actions
    /// that have <see cref="AuthorizeAttribute"/> (and are not overridden with
    /// <see cref="AllowAnonymousAttribute"/>). Public endpoints remain unlocked in
    /// Swagger UI so they can be called directly without a token.
    /// </summary>
    public class SwaggerAuthOperationFilter : IOperationFilter
    {
        /// <inheritdoc />
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            // Collect all [Authorize] and [AllowAnonymous] attributes on the action + controller
            var hasAllowAnonymous = context.MethodInfo.GetCustomAttributes(true)
                .OfType<AllowAnonymousAttribute>().Any()
                || (context.MethodInfo.DeclaringType?.GetCustomAttributes(true)
                    .OfType<AllowAnonymousAttribute>().Any() ?? false);

            if (hasAllowAnonymous)
                return;

            var hasAuthorize = context.MethodInfo.GetCustomAttributes(true)
                .OfType<AuthorizeAttribute>().Any()
                || (context.MethodInfo.DeclaringType?.GetCustomAttributes(true)
                    .OfType<AuthorizeAttribute>().Any() ?? false);

            if (!hasAuthorize)
                return;

            // Add 401 / 403 responses if not already documented
            if (!operation.Responses.ContainsKey("401"))
                operation.Responses.Add("401", new OpenApiResponse { Description = "Unauthorized – valid JWT required" });
            if (!operation.Responses.ContainsKey("403"))
                operation.Responses.Add("403", new OpenApiResponse { Description = "Forbidden – insufficient role" });

            // Attach the Bearer security requirement to this specific operation
            operation.Security = new List<OpenApiSecurityRequirement>
            {
                new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id   = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                }
            };
        }
    }
}
