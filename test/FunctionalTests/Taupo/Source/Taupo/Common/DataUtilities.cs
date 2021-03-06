﻿//---------------------------------------------------------------------
// <copyright file="DataUtilities.cs" company="Microsoft">
//      Copyright (C) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.
// </copyright>
//---------------------------------------------------------------------

namespace Microsoft.Test.Taupo.Common
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// Common Data Utilities
    /// </summary>
    public static class DataUtilities
    {
        /// <summary>
        /// Determines whether the specified type is anonymous.
        /// </summary>
        /// <param name="type">The type to check</param>
        /// <returns>
        /// <c>true</c> if it is anonymous type, otherwise, <c>false</c>.
        /// </returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "Need to enforce that only the derived type(Type) is passed to the method instead of base type 'MemberInfo'")]
        public static bool IsAnonymousType(Type type)
        {
            ExceptionUtilities.CheckArgumentNotNull(type, "type");

            // A type is anonymous if it has been generated by the compiler
            return type.IsDefined(typeof(CompilerGeneratedAttribute));
        }

        /// <summary>
        /// Gets values of the specified enum type.
        /// </summary>
        /// <typeparam name="TEnum">The enum type.</typeparam>
        /// <returns>Values of the specified enum type.</returns>
        public static IEnumerable<TEnum> GetEnumValues<TEnum>()
        {
            return GetEnumValues(typeof(TEnum)).Cast<TEnum>();
        }

        /// <summary>
        /// Gets values of the specified enum type.
        /// </summary>
        /// <param name="enumType">The enum type.</param>
        /// <returns>Values of the specified enum type.</returns>
        public static IEnumerable<object> GetEnumValues(Type enumType)
        {
            ExceptionUtilities.CheckArgumentNotNull(enumType, "enumType");
            ExceptionUtilities.Assert(enumType.IsEnum(), "Type is not a enum type: {0}.", enumType.FullName);

            // Note: don't use Enum.GetValues as it's not supported on Silverlight
            var fields = enumType.GetFields(true, true);

            var values = fields.Select(f => f.GetValue(null)).ToArray();

            return values;
        }

        /// <summary>
        /// Converts specified enum value to the value of the underlying type.
        /// </summary>
        /// <param name="enumValue">The enum value to convert from.</param>
        /// <returns>Converted value.</returns>
        public static object ConvertFromEnum(object enumValue)
        {
            if (enumValue == null)
            {
                return null;
            }

            Type enumType = enumValue.GetType();
            ExceptionUtilities.Assert(enumType.IsEnum(), "Type is not an enum type: {0}.", enumType.FullName);

            Type underlyingType = Enum.GetUnderlyingType(enumType);

            return Convert.ChangeType(enumValue, underlyingType, CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Converts specified value to the value of the given enum type.
        /// The value can be a string representing the name of an enumerated member or value of the integral type.
        /// </summary>
        /// <param name="enumType">The enum type.</param>
        /// <param name="value">The value to convert.</param>
        /// <returns>Converted value.</returns>
        public static object ConvertToEnum(Type enumType, object value)
        {
            var namedMember = value as string;

            if (value == null)
            {
                return null;
            }
            else if (namedMember != null)
            {
                return Enum.Parse(enumType, namedMember, false);
            }
            else
            {
                long unnamedMember;
                try
                {
                    unnamedMember = Convert.ToInt64(value, CultureInfo.InvariantCulture);
                }
                catch (InvalidCastException e)
                {
                    throw new TaupoInvalidOperationException("The provided values for the enum type \'" + enumType.FullName + "\' should only contain string or integer values", e);
                }

                var v = Enum.ToObject(enumType, unnamedMember);
                return v;
            }
        }
    }
}