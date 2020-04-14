namespace Miruken.EntityFramework.PostgresSQL
{
    using System;
    using System.Collections.Generic;
    using Npgsql;
    using NpgsqlTypes;

    public static class PostgresSQLParameterExtensions
    {
        public static ICollection<NpgsqlParameter> AddParameter(
            this ICollection<NpgsqlParameter> paramList,
            NpgsqlParameter                   param)
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

        public static ICollection<NpgsqlParameter> AddParameter(
            this ICollection<NpgsqlParameter> paramList,
            string                            paramName,
            NpgsqlDbType                      type,
            object                            value,
            Action<NpgsqlParameter>           configure = null)
        {
            if (paramList == null)
                throw new ArgumentNullException(nameof(paramList));

            if (paramList.IsReadOnly)
            {
                throw new InvalidOperationException(
                    "Cannot add parameters to a Read - Only Collection.");
            }

            var param = new NpgsqlParameter
            {
                ParameterName = paramName,
                NpgsqlDbType  = type, 
                Value         = value ?? DBNull.Value
            };
            configure?.Invoke(param);
            return paramList.AddParameter(param);
        }

        public static ICollection<NpgsqlParameter> AddParameter<T>(
            this ICollection<NpgsqlParameter> paramList, 
            string                            paramName, 
            NpgsqlDbType                      type, 
            T?                                value,
            Action<NpgsqlParameter>           configure = null
            ) where T : struct
        {
            if (paramList == null)
                throw new ArgumentNullException(nameof(paramList));

            if (paramList.IsReadOnly)
            {
                throw new InvalidOperationException(
                    "Cannot add parameters to a Read - Only Collection.");
            }

            var param = new NpgsqlParameter(paramName, type);

            if (value.HasValue)
                param.Value = value.Value;
            else
                param.Value = DBNull.Value;

            configure?.Invoke(param);
            return paramList.AddParameter(param);
        }
    }
}
