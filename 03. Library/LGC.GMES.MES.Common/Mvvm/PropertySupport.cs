using LGC.GMES.MES.Common.Mvvm.Properties;
using System;
using System.Linq.Expressions;
using System.Reflection;

namespace LGC.GMES.MES.Common.Mvvm
{
    /// <summary>
    ///  
    /// </summary>
    public static class PropertySupport
    {
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="propertyExpression"></param>
        /// <returns></returns>
        /// <exception cref="T:System.ArgumentNullException"></exception>
        /// <exception cref="T:System.ArgumentException"></exception>
        public static string ExtractPropertyName<T>(Expression<Func<T>> propertyExpression)
        {
            if (propertyExpression == null)
                throw new ArgumentNullException("propertyExpression");

            MemberExpression body = propertyExpression.Body as MemberExpression;

            if (body == null)
                throw new ArgumentException(Resources.PropertySupport_NotMemberAccessExpression_Exception, "propertyExpression");

            PropertyInfo member = body.Member as PropertyInfo;

            if (member == null)
                throw new ArgumentException(Resources.PropertySupport_ExpressionNotProperty_Exception, "propertyExpression");

            if (member.GetMethod.IsStatic)
                throw new ArgumentException(Resources.PropertySupport_StaticExpression_Exception, "propertyExpression");

            return body.Member.Name;
        }
    }
}