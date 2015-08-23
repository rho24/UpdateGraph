using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.Data.Entity;
using Microsoft.Data.Entity.Metadata;

namespace UpdateGraph
{
    public class InternalUpdateGraph<T>
        where T : class
    {
        public T Update(DbContext context, T entity, GraphOptions<T> graphOptions) {
            var meta = context.Model.FindEntityType(typeof(T));

            var primaryKeyExpression = PrimaryKeyExpression(entity, meta);
            var relationshipExpressions = RelationShipExpressions(meta);

            var query = (IQueryable<T>)context.Set<T>();

            var queryWithIncludes = relationshipExpressions.Aggregate(query, (q, r) => q.Include(r));

            var trackedEntity = queryWithIncludes.SingleOrDefault(primaryKeyExpression);

            if(trackedEntity == null) {
                context.Add(entity);
                return entity;
            }

            if(graphOptions.UpdateValues) {
                foreach(var property in meta.GetProperties()) {
                    if(property.IsKey())
                        continue;

                    if(property.IsShadowProperty)
                        continue;

                    var typeProperty = typeof(T).GetProperty(property.Name);

                    var newValue = typeProperty.GetValue(entity);
                    typeProperty.SetValue(trackedEntity, newValue);
                }
            }

            foreach(var navigation in meta.GetNavigations()) {
                if(graphOptions.NonOwnedReferences.All(p => p.Name != navigation.Name))
                    continue;

                var propertyInfo = graphOptions.NonOwnedReferences.Single(p => p.Name == navigation.Name);

                var newValue = propertyInfo.GetValue(entity);
                var trackedNewValue = DynamicUpdateGraph(context, newValue);

                propertyInfo.SetValue(trackedEntity, trackedNewValue);
            }

            context.Update(trackedEntity);

            return trackedEntity;
        }

        static IEnumerable<Expression<Func<T, object>>> RelationShipExpressions(IEntityType meta) {
            return meta.GetNavigations().Where(n => false).Select(
                n => {
                    var entityParamExp = Expression.Parameter(typeof(T));
                    return Expression.Lambda<Func<T, object>>(Expression.Property(entityParamExp, n.Name), entityParamExp);
                });
        }

        static Expression<Func<T, bool>> PrimaryKeyExpression(T entity, IEntityType meta) {
            var key = meta.FindPrimaryKey();

            var property = key.Properties.First();
            var keyValue = typeof(T).GetProperty(property.Name).GetValue(entity);

            var entityParamExp = Expression.Parameter(typeof(T));
            var primaryKeyExpression = Expression.Lambda<Func<T, bool>>(Expression.Equal(Expression.Property(entityParamExp, property.Name), Expression.Constant(keyValue)), entityParamExp);
            return primaryKeyExpression;
        }

        static object DynamicUpdateGraph(DbContext context, object entity) {
            var entityType = entity.GetType();
            var updater = Activator.CreateInstance(typeof(InternalUpdateGraph<>).MakeGenericType(entityType));
            var options = (typeof(GraphOptions<>).MakeGenericType(entityType)).GetMethod("NoUpdates").Invoke(null, null);
            return updater.GetType().GetMethod("Update").Invoke(updater, new []{context, entity, options});
        }
    }
}