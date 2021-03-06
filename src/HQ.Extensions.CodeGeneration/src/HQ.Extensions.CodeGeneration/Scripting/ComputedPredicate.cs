#region LICENSE

// Unless explicitly acquired and licensed from Licensor under another
// license, the contents of this file are subject to the Reciprocal Public
// License ("RPL") Version 1.5, or subsequent versions as allowed by the RPL,
// and You may not copy or use this file in either source code or executable
// form, except in compliance with the terms and conditions of the RPL.
// 
// All software distributed under the RPL is provided strictly on an "AS
// IS" basis, WITHOUT WARRANTY OF ANY KIND, EITHER EXPRESS OR IMPLIED, AND
// LICENSOR HEREBY DISCLAIMS ALL SUCH WARRANTIES, INCLUDING WITHOUT
// LIMITATION, ANY WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR
// PURPOSE, QUIET ENJOYMENT, OR NON-INFRINGEMENT. See the RPL for specific
// language governing rights and limitations under the RPL.

#endregion

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;

namespace HQ.Extensions.CodeGeneration.Scripting
{
    public static class ComputedPredicate<T>
    {
        // ReSharper disable once StaticMemberInGenericType
        private static readonly Dictionary<Type, ScriptOptions> Options = new Dictionary<Type, ScriptOptions>();
            
        static ComputedPredicate()
        {
            Options[typeof(T)] = ScriptOptions.Default
                .WithReferences(typeof(T).Assembly, typeof(MemberExpression).Assembly)
                .WithImports("System", "System.Text", "System.Linq", "System.Collections.Generic");
        }

        public static Func<T, bool> AsPredicate(string expression)
        {
            return CSharpScript.EvaluateAsync<Func<T, bool>>(expression, Options[typeof(T)]).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        public static Expression<Func<T, bool>> AsExpression(string memberName, ExpressionOperator @operator, object value)
        {
            var parameter = Expression.Parameter(typeof(T), memberName);
            var memberExpression = Expression.PropertyOrField(parameter, memberName);

            BinaryExpression expression;
            switch (@operator)
            {
                case ExpressionOperator.Equal:
                    expression = Expression.Equal(memberExpression, Expression.Constant(value));
                    break;
                case ExpressionOperator.NotEqual:
                    expression = Expression.NotEqual(memberExpression, Expression.Constant(value));
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(@operator), @operator, null);
            }

            return Expression.Lambda<Func<T, bool>>(expression, parameter);
        }
    }
}
