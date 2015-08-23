using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.Data.Entity;
using Microsoft.Data.Entity.Metadata;

namespace UpdateGraph
{
    public static class DbContextExtensions
    {
        public static void UpdateGraph<T>(this DbContext context, T entity) where T : class {
            var meta = context.Model.FindEntityType(typeof(T));

            var primaryKeyExpression = PrimaryKeyExpression(entity, meta);
            var relationshipExpressions = RelationShipExpressions<T>(meta);

            var query = (IQueryable<T>)context.Set<T>();

            var queryWithIncludes = relationshipExpressions.Aggregate(query, (q, r) => q.Include(r));

            var trackedEntity = queryWithIncludes.Single(primaryKeyExpression);

            if(trackedEntity == null)
                context.Add(entity);
            else {

                foreach(var property in meta.GetProperties()) {
                    if(property.IsKey())
                        continue;

                    if(property.IsShadowProperty)
                        continue;

                    var typeProperty = typeof(T).GetProperty(property.Name);

                    var newValue = typeProperty.GetValue(entity);
                    typeProperty.SetValue(trackedEntity, newValue);
                }

                context.Update(trackedEntity);
            }
        }

        static IEnumerable<Expression<Func<T, object>>> RelationShipExpressions<T>(IEntityType meta) where T : class {
            return meta.GetNavigations().Where(n => false).Select(
                n => {
                    var entityParamExp = Expression.Parameter(typeof(T));
                    return Expression.Lambda<Func<T, object>>(Expression.Property(entityParamExp, n.Name), entityParamExp);
                });
        }

        static Expression<Func<T, bool>> PrimaryKeyExpression<T>(T entity, IEntityType meta) where T : class {
            var key = meta.FindPrimaryKey();

            var property = key.Properties.First();
            var keyValue = typeof(T).GetProperty(property.Name).GetValue(entity);

            var entityParamExp = Expression.Parameter(typeof(T));
            var primaryKeyExpression = Expression.Lambda<Func<T, bool>>(Expression.Equal(Expression.Property(entityParamExp, property.Name), Expression.Constant(keyValue)), entityParamExp);
            return primaryKeyExpression;
        }
    }
}