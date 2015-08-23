using System;
using Microsoft.Data.Entity;

namespace UpdateGraph
{
    public static class DbContextExtensions
    {
        public static void UpdateGraph<T>(this DbContext context, T entity) where T : class {
            context.Update(entity);
        }
    }
}