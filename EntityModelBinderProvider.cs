// Copyright (c) Giancarlo Maniscalco. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using EntityModelBinder;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EntityModelBinder
{
    public class EntityModelBinderProvider<TDatabaseContext>
        : IModelBinderProvider where TDatabaseContext : class
    {
        public IModelBinder? GetBinder(ModelBinderProviderContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var modelType = context.Metadata.ModelType;

            if (typeof(IEntity).IsAssignableFrom(modelType) && !modelType.IsAbstract)
            {
                var binderType = typeof(EntityModelBinder<>).MakeGenericType(modelType);
                return new BinderTypeModelBinder(binderType);
            }
            return null;
        }
    }
}
