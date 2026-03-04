// Copyright (c) Giancarlo Maniscalco. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace EntityModelBinder
{
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
    public class EntityKeyAttribute : Attribute
    {
        public string ColumnName { get; }

        public EntityKeyAttribute(string columnName)
        {
            ColumnName = columnName ?? throw new ArgumentNullException(nameof(columnName));
        }
    }
}
