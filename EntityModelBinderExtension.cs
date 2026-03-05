// Copyright (c) Giancarlo Maniscalco. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EntityModelBinder
{
    public static class EntityModelBinderExtensions
    {
        public static IServiceCollection AddEntityModelBinder<TDatabaseContext>(
            this IServiceCollection services,
            Action<EntityModelBinderOptions>? configure = null) where TDatabaseContext : DbContext
        {
            var configuration = new EntityModelBinderOptions();
            configure?.Invoke(configuration);

            if (configuration.ControllerConfiguration != null)
            {
                services.Configure(configuration.ControllerConfiguration);
            }

            services.AddScoped<DbContext, TDatabaseContext>();

            services.AddControllers(options =>
            {
                options.ModelBinderProviders.Insert(0, new EntityModelBinderProvider<TDatabaseContext>());

                if (configuration.SuppressModelStateInvalidFilter)
                {
                    options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;
                }
            });

            return services;
        }
    }
}
