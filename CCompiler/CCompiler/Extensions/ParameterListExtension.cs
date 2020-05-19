using System.Collections.Generic;

namespace CCompiler.Extensions
{
    public static class ParameterListExtension
    {
        public static Stack<CParser.ParameterDeclarationContext> RBAParameterDeclarationStack(
            this CParser.ParameterListContext parameterList)
        {
            var localParameterList = parameterList;
            var parameterDeclarationStack
                = new Stack<CParser.ParameterDeclarationContext>();

            while (localParameterList.parameterList() != null)
            {
                parameterDeclarationStack
                    .Push(localParameterList.parameterDeclaration());

                localParameterList = localParameterList.parameterList();
            }

            parameterDeclarationStack
                .Push(localParameterList.parameterDeclaration());

            return parameterDeclarationStack;
        }
    }
}
