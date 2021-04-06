namespace Miruken.EntityFramework.MySql
{
    using System;
    using System.Collections.Generic;
    using MySqlConnector;

    public static class MySqlParameterExtensions
    {
        public static ICollection<MySqlParameter> AddParameter(
            this ICollection<MySqlParameter> paramList,
            MySqlParameter                   param)
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

        public static ICollection<MySqlParameter> AddParameter(
            this ICollection<MySqlParameter> paramList,
            string                           paramName,
            MySqlDbType                      type,
            object                           value,
            Action<MySqlParameter>           configure = null)
        {
            if (paramList == null)
                throw new ArgumentNullException(nameof(paramList));

            if (paramList.IsReadOnly)
            {
                throw new InvalidOperationException(
                    "Cannot add parameters to a Read - Only Collection.");
            }

            var param = new MySqlParameter
            {
                ParameterName = paramName,
                MySqlDbType   = type, 
                Value         = value ?? DBNull.Value
            };
            configure?.Invoke(param);
            return paramList.AddParameter(param);
        }

        public static ICollection<MySqlParameter> AddParameter<T>(
            this ICollection<MySqlParameter> paramList, 
            string                           paramName, 
            MySqlDbType                      type, 
            T?                               value,
            Action<MySqlParameter>           configure = null
            ) where T : struct
        {
            if (paramList == null)
                throw new ArgumentNullException(nameof(paramList));

            if (paramList.IsReadOnly)
            {
                throw new InvalidOperationException(
                    "Cannot add parameters to a Read - Only Collection.");
            }

            var param = new MySqlParameter(paramName, type);

            if (value.HasValue)
                param.Value = value.Value;
            else
                param.Value = DBNull.Value;

            configure?.Invoke(param);
            return paramList.AddParameter(param);
        }
    }
}
