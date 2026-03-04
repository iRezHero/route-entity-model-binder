// Copyright (c) Giancarlo Maniscalco. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.DependencyInjection;

namespace EntityModelBinder
{
    public static class EntityModelBinderExtensions
    {
        public static IServiceCollection AddEntityModelBinder(
            this IServiceCollection services,
            Action<EntityModelBinderOptions>? configure = null)
        {
            var options = new EntityModelBinderOptions();
            configure?.Invoke(options);

            services.Insert(0, ServiceDescriptor.Singleton<IModelBinderProvider, EntityModelBinderProvider>());

            if (options.ControllerConfiguration != null)
            {
                services.Configure(options.ControllerConfiguration);
            }

            if (options.SuppressModelStateInvalidFilter)
            {
                services.AddControllers(options =>
                {
                    options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;
                });
            }

            return services;
        }
    }
}
