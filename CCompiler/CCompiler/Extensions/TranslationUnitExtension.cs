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
    public static class TranslationUnitExtension
    {
        public static Stack<CParser.ExternalDeclarationContext>
            RBAExternalDeclarationStack(
            this CParser.TranslationUnitContext translationUnit)
        {
            var localTranslationUnit = translationUnit;
            var externalDeclarationStack
                = new Stack<CParser.ExternalDeclarationContext>();

            while (localTranslationUnit.translationUnit() != null)
            {
                externalDeclarationStack
                    .Push(localTranslationUnit.externalDeclaration());

                localTranslationUnit = localTranslationUnit.translationUnit();
            }

            externalDeclarationStack
                .Push(localTranslationUnit.externalDeclaration());

            return externalDeclarationStack;
        }
    }
}
