using System;
using Microsoft.Data.Entity;

namespace UpdateGraph
{
    public static class DbContextExtensions
    {
        public static void UpdateGraph<T>(this DbContext context, T entity) where T : class {
            UpdateGraph(context, entity, null);
        }

        public static T UpdateGraph<T>(this DbContext context, T entity, Action<GraphOptions<T>> options) where T : class {
            var graphOptions = new GraphOptions<T>();

            options?.Invoke(graphOptions);

            return new InternalUpdateGraph<T>().Update(context, entity, graphOptions);
        }
    }
}