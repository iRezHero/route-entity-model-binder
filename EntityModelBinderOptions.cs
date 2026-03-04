// Copyright (c) Giancarlo Maniscalco. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.AspNetCore.Mvc;

namespace EntityModelBinder
{
    public class EntityModelBinderOptions
    {
        public bool EnableSwaggerSchemaIds { get; set; } = false;

        public Action<MvcOptions>? ControllerConfiguration { get; set; }
    }
}
