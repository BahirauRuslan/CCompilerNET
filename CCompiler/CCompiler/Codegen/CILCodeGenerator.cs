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
    public class CILCodeGenerator
    {
        public static readonly string DOTEXE = ".exe";
        public static readonly string DOTDLL = ".dll";

        private readonly bool _hasEntryPoint;

        private readonly string _fileName;
        private readonly string _programName;

        private AssemblyName _assemblyName;
        private AssemblyBuilder _assemblyBuilder;
        private ModuleBuilder _moduleBuilder;

        private TypeBuilder _programClass;
        private ConstructorBuilder _programConstructor;
        //// private MethodBuilder _entryPoint;
        private ILGenerator _generatorIL;

        private CParser.CompilationUnitContext _compilationUnit;

        public CILCodeGenerator(
            string fileName,
            CParser.CompilationUnitContext compilationUnit)
        {
            _fileName = fileName;
            _programName = Path.GetFileNameWithoutExtension(_fileName);
            _compilationUnit = compilationUnit;
            _hasEntryPoint = true;
        }

        public string ProgramFileName
        {
            get
            {
                return _programName + (_hasEntryPoint ? DOTEXE : DOTDLL);
            }
        }

        public void Generate()
        {
            var translationUnit = _compilationUnit.translationUnit();

            GenerateAssemblyAndModuleBuilders();
            DefineProgramClass();

            if (translationUnit != null)
            {
                GenerateTranslationUnit(_compilationUnit.translationUnit());
            }

            EmitProgramClass();
            SaveAssembly();
        }

        protected void GenerateAssemblyAndModuleBuilders()
        {
            _assemblyName = new AssemblyName()
            {
                Name = _programName
            };

            _assemblyBuilder = AppDomain
                .CurrentDomain
                .DefineDynamicAssembly(
                _assemblyName,
                AssemblyBuilderAccess.Save);

            _moduleBuilder = _assemblyBuilder
                .DefineDynamicModule(_programName, ProgramFileName, false);
        }

        protected void DefineProgramClass()
        {
            _programClass = _moduleBuilder.DefineType(
                "Program", 
                TypeAttributes.NotPublic | TypeAttributes.BeforeFieldInit, 
                typeof(object));
            
            _programClass.DefineDefaultConstructor(MethodAttributes.Public);

            _programConstructor = _programClass.DefineConstructor(
                MethodAttributes.Static
                | MethodAttributes.Private
                | MethodAttributes.HideBySig, 
                CallingConventions.Standard, 
                Type.EmptyTypes);
        }

        protected void GenerateTranslationUnit(
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
                GenerateExternalDeclaration(externalDeclarationStack.Pop());
            }
        }

        protected void GenerateExternalDeclaration(
            CParser.ExternalDeclarationContext externalDeclaration)
        {
            var functionDefinition = externalDeclaration.functionDefinition();
            var declaration = externalDeclaration.declaration();

            if (functionDefinition != null)
            {
                GenerateFunctionDefinition(functionDefinition);
            }
            else if (declaration != null)
            {
                GenerateDeclaration(declaration);
            }
        }

        protected void GenerateFunctionDefinition(
            CParser.FunctionDefinitionContext functionDefinition)
        {
        }

        protected void GenerateDeclaration(
            CParser.DeclarationContext declaration)
        {
        }

        protected void EmitProgramClass()
        {
            _generatorIL = _programConstructor.GetILGenerator();

            _generatorIL.Emit(OpCodes.Ret);
            _programClass.CreateType();
        }

        protected void SaveAssembly()
        {
            var saveFileError = false;

            if (File.Exists(ProgramFileName))
            {
                try
                {
                    File.Delete(ProgramFileName);
                }
                catch
                {
                    saveFileError = true;
                }
            }

            if (!saveFileError)
            {
                _assemblyBuilder.Save(ProgramFileName);
            }
        }
    }
}
