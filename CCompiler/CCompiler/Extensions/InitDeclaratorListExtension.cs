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
