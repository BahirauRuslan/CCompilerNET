using System.Collections.Generic;

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
