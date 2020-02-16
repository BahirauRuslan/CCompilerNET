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

namespace CCompiler.Codegen
{
    public class CPreBuilder
    {
        public CPreBuilder(
            string fileName,
            CParser.CompilationUnitContext compilationUnit)
        {
            FileName = fileName;
            ProgramName = Path.GetFileNameWithoutExtension(FileName);
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

            ProgramFileName = ProgramName + (HasEntryPoint ? ".exe" : ".dll");
        }

        private void AnalyzeTranslationUnit(
            CParser.TranslationUnitContext translationUnit)
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
            else if (declaration != null)
            {
                AnalyzeDeclaration(declaration);
            }
        }

        private void AnalyzeFunctionDefinition(
            CParser.FunctionDefinitionContext functionDefinition)
        {
            var identifier = functionDefinition
                ?.declarator()
                ?.directDeclarator()
                ?.directDeclarator()
                ?.Identifier();

            if (identifier?.ToString() == "main")
            {
                HasEntryPoint = true;
            }
        }

        private void AnalyzeDeclaration(
            CParser.DeclarationContext declaration)
        {
        }
    }
}
