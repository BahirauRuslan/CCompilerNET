using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Reflection.Emit;
using System.IO;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;

namespace CCompiler.Codegen
{
    public class CILCodeGenerator
    {
        private string _fileName;
        private string _programName;

        private AssemblyName _assemblyName;
        private AssemblyBuilder _assemblyBuilder;
        private ModuleBuilder _moduleBuilder;

        private TypeBuilder _programClass;
        private ConstructorBuilder _programConstructor;
        private MethodBuilder _entryPoint;
        private ILGenerator _iLGenerator;

        private CParser.CompilationUnitContext _compilationUnit;

        public CILCodeGenerator(string fileName, CParser.CompilationUnitContext compilationUnit)
        {
            _fileName = fileName;
            _programName = Path.GetFileNameWithoutExtension(_fileName);
            _compilationUnit = compilationUnit;
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
            _assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(_assemblyName, AssemblyBuilderAccess.Save);
            _moduleBuilder = _assemblyBuilder.DefineDynamicModule(_programName, _programName + ".exe", false);
        }

        protected void DefineProgramClass()
        {
            _programClass = _moduleBuilder.DefineType(
                "Program", 
                TypeAttributes.NotPublic | TypeAttributes.BeforeFieldInit, 
                typeof(object));
            
            _programClass.DefineDefaultConstructor(MethodAttributes.Public);

            _programConstructor = _programClass.DefineConstructor(
                MethodAttributes.Static | MethodAttributes.Private | MethodAttributes.HideBySig, 
                CallingConventions.Standard, 
                Type.EmptyTypes);
        }

        protected void GenerateTranslationUnit(CParser.TranslationUnitContext translationUnit)
        {
            var localTranslationUnit = translationUnit;
            var externalDeclarationStack = new Stack<CParser.ExternalDeclarationContext>();

            while (localTranslationUnit.translationUnit() != null)
            {
                externalDeclarationStack.Push(localTranslationUnit.externalDeclaration());
                
                localTranslationUnit = localTranslationUnit.translationUnit();
            }

            externalDeclarationStack.Push(localTranslationUnit.externalDeclaration());

            while (externalDeclarationStack.Count > 0)
            {
                GenerateExternalDeclaration(externalDeclarationStack.Pop());
            }
        }

        protected void GenerateExternalDeclaration(CParser.ExternalDeclarationContext externalDeclaration)
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

        protected void GenerateFunctionDefinition(CParser.FunctionDefinitionContext functionDefinition)
        {
            ;
        }

        protected void GenerateDeclaration(CParser.DeclarationContext declaration)
        {
            ;
        }

        protected void EmitProgramClass()
        {
            _iLGenerator = _programConstructor.GetILGenerator();

            _iLGenerator.Emit(OpCodes.Ret);
            _programClass.CreateType();
        }

        protected void SaveAssembly()
        {
            var saveFileError = false;

            if (File.Exists(_programName + ".exe"))
            {
                try
                {
                    File.Delete(_programName + ".exe");
                }
                catch
                {
                    saveFileError = true;
                }
            }

            if (!saveFileError)
            {
                _assemblyBuilder.Save(_programName + ".exe");
            }
        }
    }
}
