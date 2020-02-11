namespace Miruken.EntityFramework
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using Microsoft.Data.SqlClient;

    public static class SqlParameterExtensions
    {
        public static ICollection<SqlParameter> AddParameter(
            this ICollection<SqlParameter> paramList,
            SqlParameter                   param)
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

        public static ICollection<SqlParameter> AddParameter(
            this ICollection<SqlParameter> paramList,
            string                         paramName,
            SqlDbType                      type,
            object                         value,
            Action<SqlParameter>           configure = null
            )
        {
            if (paramList == null)
                throw new ArgumentNullException(nameof(paramList));

            if (paramList.IsReadOnly)
            {
                throw new InvalidOperationException(
                    "Cannot add parameters to a Read - Only Collection.");
            }

            var param = new SqlParameter { ParameterName = paramName, SqlDbType = type, Value = value ?? DBNull.Value };
            configure?.Invoke(param);
            return paramList.AddParameter(param);
        }

        public static ICollection<SqlParameter> AddParameter<T>(
            this ICollection<SqlParameter> paramList, 
            string                         paramName, 
            SqlDbType                      type, 
            T?                             value,
            Action<SqlParameter>           configure = null
            ) where T : struct
        {
            if (paramList == null)
                throw new ArgumentNullException(nameof(paramList));

            if (paramList.IsReadOnly)
            {
                throw new InvalidOperationException(
                    "Cannot add parameters to a Read - Only Collection.");
            }

            var param = new SqlParameter(paramName, type);

            if (value.HasValue)
                param.Value = value.Value;
            else
                param.Value = DBNull.Value;

            configure?.Invoke(param);
            return paramList.AddParameter(param);
        }
    }
}
