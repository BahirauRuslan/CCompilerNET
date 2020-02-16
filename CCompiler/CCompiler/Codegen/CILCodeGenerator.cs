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

namespace CCompiler.Codegen
{
    public class CILCodeGenerator
    {
        private readonly bool _hasEntryPoint;

        private readonly string _fileName;
        private readonly string _programName;
        private readonly string _programFileName;

        private AssemblyName _assemblyName;
        private AssemblyBuilder _assemblyBuilder;
        private ModuleBuilder _moduleBuilder;

        private TypeBuilder _programClass;
        private ConstructorBuilder _programConstructor;
        private MethodBuilder _entryPoint;
        private ILGenerator _generatorIL;

        private Dictionary<string, MethodDef> _functions = new Dictionary<string, MethodDef>();

        private CParser.CompilationUnitContext _compilationUnit;

        public CILCodeGenerator(CPreBuilder preBuilder)
        {
            _fileName = preBuilder.FileName;
            _programName = preBuilder.ProgramName;
            _programFileName = preBuilder.ProgramFileName;
            _hasEntryPoint = preBuilder.HasEntryPoint;
            _compilationUnit = preBuilder.CompilationUnit;
        }

        public void Generate()
        {
            var translationUnit = _compilationUnit.translationUnit();

            GenerateAssemblyAndModuleBuilders();
            DefineProgramClass();

            if (translationUnit != null)
            {
                GenerateTranslationUnit(translationUnit);
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
                .DefineDynamicModule(_programName, _programFileName, false);
        }

        protected void DefineProgramClass()
        {
            _programClass = _moduleBuilder.DefineType(
                _programName, 
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
            var typeSpecifier = functionDefinition
                ?.declarationSpecifiers()
                ?.declarationSpecifier()?[0]
                ?.typeSpecifier();
            var identifier = functionDefinition
                ?.declarator()
                ?.directDeclarator()
                ?.directDeclarator()
                ?.Identifier();
            var parameters = functionDefinition
                ?.declarator()
                ?.directDeclarator()
                ?.parameterTypeList();
            var compoundStatement = functionDefinition?.compoundStatement();

            DefineFunction(typeSpecifier, identifier, parameters);

            if (identifier.ToString() == "main")
            {
                ;
            }
        }

        protected void GenerateDeclaration(
            CParser.DeclarationContext declaration)
        {
        }

        protected void DefineFunction(
            CParser.TypeSpecifierContext typeSpecifier,
            ITerminalNode identifier,
            CParser.ParameterTypeListContext parameters)
        {
            Type[] inputTypes = Type.EmptyTypes;

            var functionName = identifier.ToString();
            var functionReturnType = GetType(typeSpecifier.ToString());
            var args = new Dictionary<string, MethodArgDef>();

            args.Add("this", new MethodArgDef(_programClass, 0, "this"));

            if (parameters != null)
            {
                //var parametersStack
                //= new Stack<CParser.ExternalDeclarationContext>();

                //parameters.
                //var functionListNode = treeNode.GetChild(3);
                //inputTypes = new Type[functionListNode.ChildCount];
                //for (int k = 0; k < functionListNode.ChildCount; k++)
                //{
                //    inputTypes[k] = GetType(functionListNode.GetChild(k).GetChild(1).Text);
                //    var argName = functionListNode.GetChild(k).GetChild(0).Text;
                //    args.Add(argName, new ArgObjectDef(inputTypes[k], k + 1, argName));
                //}
            }
            else
            {
                inputTypes = Type.EmptyTypes;
            }

            MethodBuilder methodBuilder;

            if (functionName == "main")
            {
                methodBuilder = _programClass.DefineMethod(functionName,
                    MethodAttributes.Private | MethodAttributes.Static | MethodAttributes.HideBySig, typeof(void), Type.EmptyTypes);
                methodBuilder.SetCustomAttribute(new CustomAttributeBuilder(
                    typeof(STAThreadAttribute).GetConstructor(Type.EmptyTypes), new object[] { }));
                _assemblyBuilder.SetEntryPoint(methodBuilder);
                _entryPoint = methodBuilder;
            }
            else
            {
                methodBuilder = _programClass.DefineMethod(functionName, MethodAttributes.Public | MethodAttributes.HideBySig, functionReturnType, inputTypes);
            }


            _functions.Add(functionName, new MethodDef(functionName, args, methodBuilder));
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

            if (File.Exists(_programFileName))
            {
                try
                {
                    File.Delete(_programFileName);
                }
                catch
                {
                    saveFileError = true;
                }
            }

            if (!saveFileError)
            {
                _assemblyBuilder.Save(_programFileName);
            }
        }

        protected Type GetType(string typeName)
        {
            Type result;

            switch (typeName)
            {
                case "void":
                    result = typeof(void);
                    break;
                case "char":
                    result = typeof(char);
                    break;
                case "short":
                    result = typeof(short);
                    break;
                case "int":
                    result = typeof(int);
                    break;
                case "float":
                    result = typeof(float);
                    break;
                case "double":
                    result = typeof(double);
                    break;
                case "bool":
                    result = typeof(bool);
                    break;
                default:
                    result = _moduleBuilder.GetType(typeName);
                    break;
            }

            return result;
        }
    }
}
