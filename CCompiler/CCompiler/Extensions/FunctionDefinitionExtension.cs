﻿using Antlr4.Runtime.Tree;

namespace CCompiler.Extensions
{
    public static class FunctionDefinitionExtension
    {
        public static CParser.TypeSpecifierContext RBATypeSpecifier(
            this CParser.FunctionDefinitionContext functionDefinition)
        {
            return functionDefinition
                ?.declarationSpecifiers()
                ?.declarationSpecifier()?[0]
                ?.typeSpecifier();
        }

        public static ITerminalNode RBAIdentifier(
            this CParser.FunctionDefinitionContext functionDefinition)
        {
            return functionDefinition
                ?.declarator()
                ?.directDeclarator()
                ?.directDeclarator()
                ?.Identifier();
        }

        public static CParser.ParameterTypeListContext RBAParameters(
            this CParser.FunctionDefinitionContext functionDefinition)
        {
            return functionDefinition
                ?.declarator()
                ?.directDeclarator()
                ?.parameterTypeList();
        }
    }
}
