using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using CCompiler.ObjectDefinitions;

namespace CCompiler.Extensions
{
    public static class ExpressionExtension
    {
        public static Stack<CParser.AssignmentExpressionContext>
            RBAAssignmentExpressionStack(
            this CParser.ExpressionContext expressionContext)
        {
            var localExpressionContext = expressionContext;
            var assignmentExpressionStack
                = new Stack<CParser.AssignmentExpressionContext>();

            while (localExpressionContext.expression() != null)
            {
                assignmentExpressionStack
                    .Push(localExpressionContext.assignmentExpression());

                localExpressionContext = localExpressionContext.expression();
            }

            assignmentExpressionStack
                .Push(localExpressionContext.assignmentExpression());

            return assignmentExpressionStack;
        }
    }
}
