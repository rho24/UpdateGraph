using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace UpdateGraph
{
    public class GraphOptions<T>
    {
        public bool UpdateValues { get; set; }

        public List<PropertyInfo> NonOwnedReferences { get; set; }

        public GraphOptions() {
            UpdateValues = true;
            NonOwnedReferences = new List<PropertyInfo>();
        }

        public void NonOwnedReference(Expression<Func<T, object>> property) {
            var propertyInfo = ((property.Body as MemberExpression)?.Member as PropertyInfo);
            NonOwnedReferences.Add(propertyInfo);
        }

        public void NoValueUpdates() {
            UpdateValues = false;
        }

        public static GraphOptions<T> NoUpdates() {
            return new GraphOptions<T>() { UpdateValues = false };
        }
    }
}