// Copyright (c) Giancarlo Maniscalco. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Linq.Expressions;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EntityModelBinder
{
    public class EntityModelBinder<T>
    : IModelBinder where T : class
    {
        public async Task BindModelAsync(ModelBindingContext bindingContext)
        {
            if (bindingContext == null)
                throw new ArgumentNullException(nameof(bindingContext));

            var httpContext = bindingContext.HttpContext;
            var services = httpContext.RequestServices;

            var dbContext = services.GetRequiredService<DbContext>();
            var dbSet = dbContext.Set<T>();

            var modelName = bindingContext.ModelName;
            var valueProviderResult = bindingContext.ValueProvider.GetValue(modelName);

            if (valueProviderResult == ValueProviderResult.None)
                valueProviderResult = bindingContext.ValueProvider.GetValue("id");

            if (valueProviderResult == ValueProviderResult.None)
            {
                bindingContext.Result = ModelBindingResult.Failed();
                return;
            }

            bindingContext.ModelState.SetModelValue(modelName, valueProviderResult);
            var value = valueProviderResult.FirstValue;

            if (string.IsNullOrEmpty(value))
            {
                bindingContext.Result = ModelBindingResult.Failed();
                return;
            }

            var (columnName, keyType) = GetKeyInfo(bindingContext);

            Console.WriteLine($"Binding entity of type {typeof(T).Name} using column '{columnName}' with value '{value}'");

            object keyValue;
            if (!TryConvertKeyValue(value, keyType, out keyValue, out var errorMessage))
            {
                bindingContext.ModelState.TryAddModelError(modelName, errorMessage);
                return;
            }

            var entity = await FindEntityAsync(dbSet, columnName, keyValue);

            if (entity == null)
            {
                bindingContext.Result = ModelBindingResult.Failed();
                return;
            }

            // In this way we can suppress validation for the bound entity
            // otherwise, if the model is defined like this:
            // public string Name { get; set; }
            // it will fail validation because the Name property is required by default and we don't want that
            bindingContext.ModelState.ClearValidationState(bindingContext.ModelName);

            bindingContext.ValidationState.Add(entity, new ValidationStateEntry
            {
                SuppressValidation = true
            });

            bindingContext.Result = ModelBindingResult.Success(entity);
        }

        private static (string columnName, EntityKeyType keyType) GetKeyInfo(ModelBindingContext bindingContext)
        {
            var entityType = typeof(T);
            var actionContext = bindingContext.ActionContext;
            var controllerActionDescriptor = actionContext.ActionDescriptor as ControllerActionDescriptor;

            if (controllerActionDescriptor != null)
            {
                var parameterDescriptor = controllerActionDescriptor.Parameters
                    .FirstOrDefault(p => p.Name == bindingContext.ModelName);

                if (parameterDescriptor is ControllerParameterDescriptor controllerParameterDescriptor)
                {
                    var parameterInfo = controllerParameterDescriptor.ParameterInfo;

                    if (parameterInfo != null)
                    {
                        var entityKeyAttr = parameterInfo.GetCustomAttribute<EntityKeyAttribute>(inherit: false);
                        if (entityKeyAttr != null)
                        {
                            var property = entityType.GetProperties()
                                .FirstOrDefault(p => p.Name.Equals(entityKeyAttr.ColumnName, StringComparison.OrdinalIgnoreCase));

                            if (property != null)
                            {
                                var keyType = DetermineKeyType(property.PropertyType);
                                return (entityKeyAttr.ColumnName, keyType);
                            }
                        }
                    }
                }
            }

            var idProperty = entityType.GetProperties()
                .FirstOrDefault(p => p.Name.Equals("Id", StringComparison.OrdinalIgnoreCase));

            if (idProperty != null)
            {
                return ("Id", DetermineKeyType(idProperty.PropertyType));
            }

            return ("Id", EntityKeyType.Int);
        }

        private static EntityKeyType DetermineKeyType(Type propertyType)
        {
            if (propertyType == typeof(Guid) || propertyType == typeof(Guid?))
                return EntityKeyType.Guid;
            if (propertyType == typeof(string))
                return EntityKeyType.String;
            return EntityKeyType.Int;
        }

        private static bool TryConvertKeyValue(string value, EntityKeyType keyType, out object keyValue, out string errorMessage)
        {
            keyValue = null!;
            errorMessage = string.Empty;

            switch (keyType)
            {
                case EntityKeyType.Int:
                    if (!int.TryParse(value, out var intId))
                    {
                        errorMessage = "Id must be an integer.";
                        return false;
                    }
                    keyValue = intId;
                    return true;

                case EntityKeyType.Guid:
                    if (!Guid.TryParse(value, out var guidId))
                    {
                        errorMessage = "Id must be a valid GUID.";
                        return false;
                    }
                    keyValue = guidId;
                    return true;

                case EntityKeyType.String:
                    keyValue = value;
                    return true;

                default:
                    errorMessage = "Unsupported key type.";
                    return false;
            }
        }

        private static async Task<T?> FindEntityAsync(DbSet<T> dbSet, string columnName, object keyValue)
        {
            var parameter = Expression.Parameter(typeof(T), "e");
            var property = Expression.Property(parameter, columnName);
            var constant = Expression.Constant(keyValue, keyValue.GetType());
            var equality = Expression.Equal(property, constant);
            var lambda = Expression.Lambda<Func<T, bool>>(equality, parameter);

            return await dbSet.FirstOrDefaultAsync(lambda);
        }
    }

    /// <summary>
    /// Defines the supported key types for entity binding.
    /// </summary>
    internal enum EntityKeyType
    {
        Int,
        Guid,
        String
    }
}
