using System.Collections.Generic;

namespace CCompiler.Extensions
{
    public static class InitDeclaratorListExtension
    {
        public static Stack<CParser.InitDeclaratorContext> RBAInitDeclaratorStack(
            this CParser.InitDeclaratorListContext initDeclaratorList)
        {
            var localInitDeclaratorList = initDeclaratorList;
            var initDeclaratorStack
                = new Stack<CParser.InitDeclaratorContext>();

            while (localInitDeclaratorList.initDeclaratorList() != null)
            {
                initDeclaratorStack
                    .Push(localInitDeclaratorList.initDeclarator());

                localInitDeclaratorList = localInitDeclaratorList.initDeclaratorList();
            }

            initDeclaratorStack
                .Push(localInitDeclaratorList.initDeclarator());

            return initDeclaratorStack;
        }
    }
}
