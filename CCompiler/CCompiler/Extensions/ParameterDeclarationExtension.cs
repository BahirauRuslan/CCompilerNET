using Antlr4.Runtime.Tree;

namespace CCompiler.Extensions
{
    public static class ParameterDeclarationExtension
    {
        public static CParser.TypeSpecifierContext RBATypeSpecifier(
            this CParser.ParameterDeclarationContext parameterDeclaration)
        {
            return parameterDeclaration
                ?.declarationSpecifiers()
                ?.declarationSpecifier()?[0]
                ?.typeSpecifier();
        }

        public static ITerminalNode RBAIdentifier(
            this CParser.ParameterDeclarationContext parameterDeclaration)
        {
            return parameterDeclaration
                ?.declarator()
                ?.directDeclarator()
                ?.Identifier();
        }
    }
}
