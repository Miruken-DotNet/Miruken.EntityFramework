// ReSharper disable CoVariantArrayConversion
namespace Miruken.EntityFramework
{
    using System.Data;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.EntityFrameworkCore;

    // ReSharper disable once InconsistentNaming
    public static class IDbContextExtensions
    {
        public static IQueryable<T> ExecuteStoredProcedure<T>(
            this IDbContext         context,
            string                  procedureName,
            params IDataParameter[] parameters) 
            where T : class
        {
            return context.Set<T>()
                .FromSqlRaw(Sql(procedureName, parameters), parameters);
        }

        public static int ExecuteStoredProcedure(
            this IDbContext         context, 
            string                  procedureName,
            params IDataParameter[] parameters)
        {
            return context.Database
                .ExecuteSqlRaw(Sql(procedureName, parameters), parameters);
        }

        public static Task<int> ExecuteStoredProcedureAsync(
            this IDbContext         context, 
            string                  procedureName,
            params IDataParameter[] parameters)
        {
            return context.Database
                .ExecuteSqlRawAsync(Sql(procedureName, parameters), parameters);
        }

        private static string Sql(
            string                  procedureName,
            params IDataParameter[] parameters)
        {
            return $"EXECUTE {procedureName} {StringifyIDataParameters(parameters)}";
        }

        private static string StringifyIDataParameters(IDataParameter[] dbParams)
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
