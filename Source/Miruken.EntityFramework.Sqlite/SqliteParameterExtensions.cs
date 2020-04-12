namespace Miruken.EntityFramework.Sqlite
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Data.Sqlite;

    public static class SqliteParameterExtensions
    {
        public static ICollection<SqliteParameter> AddParameter(
            this ICollection<SqliteParameter> paramList,
            SqliteParameter                   param)
        {
            if (paramList == null)
                throw new ArgumentNullException(nameof(paramList));

            if (paramList.IsReadOnly)
            {
                throw new InvalidOperationException(
                    "Cannot add parameters to a Read - Only Collection.");
            }

            if (param != null) paramList.Add(param);
            return paramList;
        }

        public static ICollection<SqliteParameter> AddParameter(
            this ICollection<SqliteParameter> paramList,
            string                            paramName,
            SqliteType                        type,
            object                            value,
            Action<SqliteParameter>           configure = null)
        {
            if (paramList == null)
                throw new ArgumentNullException(nameof(paramList));

            if (paramList.IsReadOnly)
            {
                throw new InvalidOperationException(
                    "Cannot add parameters to a Read - Only Collection.");
            }

            var param = new SqliteParameter
            {
                ParameterName = paramName,
                SqliteType     = type, 
                Value         = value ?? DBNull.Value
            };
            configure?.Invoke(param);
            return paramList.AddParameter(param);
        }

        public static ICollection<SqliteParameter> AddParameter<T>(
            this ICollection<SqliteParameter> paramList, 
            string                            paramName, 
            SqliteType                        type, 
            T?                                value,
            Action<SqliteParameter>           configure = null
            ) where T : struct
        {
            if (paramList == null)
                throw new ArgumentNullException(nameof(paramList));

            if (paramList.IsReadOnly)
            {
                throw new InvalidOperationException(
                    "Cannot add parameters to a Read - Only Collection.");
            }

            var param = new SqliteParameter(paramName, type);

            if (value.HasValue)
                param.Value = value.Value;
            else
                param.Value = DBNull.Value;

            configure?.Invoke(param);
            return paramList.AddParameter(param);
        }
    }
}
