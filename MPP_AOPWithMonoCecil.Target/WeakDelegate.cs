using System;
using System.Reflection;
using System.Linq.Expressions;

namespace MPP_AOPWithMonoCecil.Target
{
    /// <summary>
    /// Class for geterate WeakDelegate using Expression Trees
    /// </summary>
    public class WeakDelegate
    {
        private WeakReference weakRef;
        private MethodInfo listenerMethodInfo;
        private Delegate weak;
        public Delegate Weak
        {
            get
            {
                return weak;
            }
        }

        public WeakReference WeakRef
        {
            get
            {
                return weakRef;
            }
        }

        public WeakDelegate(Delegate listenerHandler)
        {
            this.listenerMethodInfo = listenerHandler.Method;
            this.weakRef = new WeakReference(listenerHandler.Target);
            initProxyDelegate();
        }

        private void initProxyDelegate()
        {
            Expression weakRefExpression = Expression.Constant(weakRef);
            Type typeToCastProperty = weakRef.Target.GetType();
            Expression targetObject = GetPropertyExpression(weakRefExpression, "Target", typeToCastProperty);
            ConstantExpression nullObject = Expression.Constant(null);
            Expression isTargetObjectNotNull = Expression.NotEqual(targetObject, nullObject);
            ParameterExpression[] targetMethodArgsExpressions = getTargetMethodArgsExpressions();
            Expression callTargetHandler = Expression.Call(targetObject, listenerMethodInfo, targetMethodArgsExpressions);
            Expression checkTargetObjectNotNullAndCallTargetHandler = Expression.IfThen(isTargetObjectNotNull, callTargetHandler);
            LambdaExpression labmdaExpression = Expression.Lambda(checkTargetObjectNotNullAndCallTargetHandler, targetMethodArgsExpressions);
            weak = labmdaExpression.Compile();
        }

        private ParameterExpression[] getTargetMethodArgsExpressions()
        {
            ParameterInfo[] methodParams = listenerMethodInfo.GetParameters();
            ParameterExpression[] argsExpressions = new ParameterExpression[methodParams.Length];
            for (int i = 0; i < argsExpressions.Length; i++)
            {
                argsExpressions[i] = Expression.Parameter(methodParams[i].ParameterType, methodParams[i].Name);
            }
            return argsExpressions;
        }

        private Expression GetPropertyExpression(Expression objectExpression, String propertyName, Type typeToCastProperty = null)
        {
            Type objectClassType = objectExpression.Type;
            PropertyInfo targetPropertyInfo = objectClassType.GetProperty(propertyName);
            Expression targetObjectExpression = Expression.Property(objectExpression, targetPropertyInfo);
            if (typeToCastProperty != null)
            {
                targetObjectExpression = Expression.Convert(targetObjectExpression, typeToCastProperty);
            }
            return targetObjectExpression;
        }
    }
}
