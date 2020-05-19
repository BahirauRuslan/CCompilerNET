using System.IO;
using System.Linq;
using CCompiler.Extensions;

namespace CCompiler.Codegen
{
    public class CPreBuilder
    {
        public CPreBuilder(
            string fileName,
            CParser.CompilationUnitContext compilationUnit)
        {
            FileName = fileName;
            ProgramName = Path.GetFileNameWithoutExtension(FileName).First().ToString().ToUpper()
                + Path.GetFileNameWithoutExtension(FileName).Substring(1);
            CompilationUnit = compilationUnit;
            HasEntryPoint = false;

            PrepareToCompile();
        }

        public string FileName { get; }

        public string ProgramName { get; }

        public CParser.CompilationUnitContext CompilationUnit { get; }

        public bool HasEntryPoint { get; private set; }

        public string ProgramFileName { get; private set; }

        private void PrepareToCompile()
        {
            if (CompilationUnit != null &&
                CompilationUnit.translationUnit() != null)
            {
                AnalyzeTranslationUnit(CompilationUnit.translationUnit());
            }

            ProgramFileName = Path.GetFileNameWithoutExtension(FileName) + (HasEntryPoint ? ".exe" : ".dll");
        }

        private void AnalyzeTranslationUnit(
            CParser.TranslationUnitContext translationUnit)
        {
            var externalDeclarationStack
                = translationUnit.RBAExternalDeclarationStack();

            while (externalDeclarationStack.Count > 0)
            {
                AnalyzeExternalDeclaration(externalDeclarationStack.Pop());
            }
        }

        private void AnalyzeExternalDeclaration(
            CParser.ExternalDeclarationContext externalDeclaration)
        {
            var functionDefinition = externalDeclaration.functionDefinition();
            var declaration = externalDeclaration.declaration();

            if (functionDefinition != null)
            {
                AnalyzeFunctionDefinition(functionDefinition);
            }
        }

        private void AnalyzeFunctionDefinition(
            CParser.FunctionDefinitionContext functionDefinition)
        {
            var identifier = functionDefinition.RBAIdentifier();

            if (identifier?.ToString() == "main")
            {
                HasEntryPoint = true;
            }
        }
    }
}
