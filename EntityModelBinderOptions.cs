// Copyright (c) Giancarlo Maniscalco. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.AspNetCore.Mvc;

namespace EntityModelBinder
{
    public class EntityModelBinderOptions
    {
        public bool EnableSwaggerSchemaIds { get; set; } = false;
        /// <summary>
        /// Set to true to suppress the automatic 400 response when model validation fails. 
        /// This allows you to handle validation errors manually in your controller actions.
        /// </summary>
        /// <remarks>
        /// When set to true, the ModelState will still be populated with validation errors, but the framework will not automatically return a 400 Bad Request response. 
        /// This is useful when you want to implement custom error handling logic in your controller actions, such as returning a different status code or a custom error response format.
        /// </remarks>
        public bool SuppressModelStateInvalidFilter { get; set; } = false;
        public Action<MvcOptions>? ControllerConfiguration { get; set; }
    }
}
