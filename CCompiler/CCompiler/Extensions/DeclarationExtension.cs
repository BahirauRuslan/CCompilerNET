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
