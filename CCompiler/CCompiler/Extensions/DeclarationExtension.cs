namespace CCompiler.Extensions
{
    public static class DeclarationExtension
    {
        public static CParser.TypeSpecifierContext RBATypeSpecifier(
            this CParser.DeclarationContext declaration)
        {
            return declaration
                ?.declarationSpecifiers()
                ?.declarationSpecifier()?[0]
                ?.typeSpecifier();
        }
    }
}
