namespace Miruken.EntityFramework
{
    using System.Data;
    using System.Linq;
    using System.Text;
    using Microsoft.Data.SqlClient;
    using Microsoft.EntityFrameworkCore;

    public static class IDBContextExtensions
    {
        public static IQueryable<T> ExecuteStoredProcedure<T>(
            this IDbContext       context,
            string                procedureName,
            params SqlParameter[] parameters) 
            where T : class
        {
            return context.Set<T>()
                .FromSqlRaw($"EXECUTE {procedureName} {StringifySqlParameters(parameters)}", parameters);
        }

        public static int ExecuteStoredProcedure(
            this IDbContext       context, 
            string                procedureName,
            params SqlParameter[] parameters)
        {
            return context.Database
                .ExecuteSqlRaw($"EXECUTE {procedureName} {StringifySqlParameters(parameters)}", parameters);
        }

        private static string StringifySqlParameters(SqlParameter[] dbParams)
        {
            if (dbParams == null || dbParams.Length == 0)
                return string.Empty;
            var sb = new StringBuilder();
            for (var i = 0; i < dbParams.Length; i++)
            {
                var param = dbParams[i];
                sb.Append(i == 0 ? " @" : ", @");
                sb.Append($"{param.ParameterName} = @{param.ParameterName}");
                if (param.Direction == ParameterDirection.InputOutput ||
                    param.Direction == ParameterDirection.Output)
                {
                    sb.Append(" OUTPUT");
                }
            }
            return sb.ToString();
        }
    }
}
