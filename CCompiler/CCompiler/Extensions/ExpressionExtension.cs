using System.Collections.Generic;

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
