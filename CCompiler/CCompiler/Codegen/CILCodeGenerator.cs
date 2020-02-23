using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading.Tasks;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using CCompiler.Extensions;
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
                _programName + "." + _programName, 
                TypeAttributes.Public | TypeAttributes.BeforeFieldInit, 
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
            var externalDeclarationStack = translationUnit.RBAExternalDeclarationStack();

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
            var typeSpecifier = functionDefinition.RBATypeSpecifier();
            var identifier = functionDefinition.RBAIdentifier();
            var parameters = functionDefinition.RBAParameters();
            var compoundStatement = functionDefinition?.compoundStatement();

            DefineFunction(typeSpecifier, identifier, parameters);
            EmitFunction(typeSpecifier, identifier, parameters, compoundStatement);
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
            var functionReturnType = typeSpecifier.RBAType(_moduleBuilder);
            var args = new Dictionary<string, MethodArgDef>
            {
                { "this", new MethodArgDef(_programClass, 0, "this") }
            };

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

        protected void EmitFunction(
            CParser.TypeSpecifierContext typeSpecifier,
            ITerminalNode identifier,
            CParser.ParameterTypeListContext parameters,
            CParser.CompoundStatementContext compoundStatement)
        {
            var functionName = identifier.ToString();

            //CurrentArgs_ = Functions_[CurrentTypeBuilder_.Name][functionName].Args;
            _generatorIL = _functions[functionName].MethodBuilder.GetILGenerator();

            LocalObjectDef.InitGenerator(_generatorIL);

            if (compoundStatement.blockItemList() != null)
            {
                var returnObjectDef = EmitCompoundStatement(compoundStatement);

                returnObjectDef.Load();

                if (_functions[functionName].MethodBuilder.ReturnType == typeof(void))
                {
                    _generatorIL.Emit(OpCodes.Pop);
                }

                _generatorIL.Emit(OpCodes.Ret);

                returnObjectDef.Remove();
            }
            else
            {
                _generatorIL.Emit(OpCodes.Ret);
            }
        }

        protected ObjectDef EmitCompoundStatement(CParser.CompoundStatementContext compoundStatement)
        {
            ObjectDef returnObjectDef = null;

            var blockItemStack = compoundStatement.blockItemList().RBABlockItemStack();

            while (blockItemStack.Count > 0)
            {
                returnObjectDef = EmitBlockItem(blockItemStack.Pop());
            }

            return returnObjectDef;
        }

        protected ObjectDef EmitBlockItem(CParser.BlockItemContext blockItem)
        {
            ObjectDef returnObjectDef;

            if (blockItem.statement() != null)
            {
                returnObjectDef = EmitStatement(blockItem.statement());
            }
            else if (blockItem.declaration() != null)
            {
                returnObjectDef = EmitDeclaration(blockItem.declaration());
            }
            else
            {
                returnObjectDef = null;
            }

            return returnObjectDef;
        }

        protected ObjectDef EmitDeclaration(CParser.DeclarationContext declaration)
        {
            return null;
        }

        protected ObjectDef EmitStatement(CParser.StatementContext statement)
        {
            ObjectDef returnObjectDef;

            if (statement.compoundStatement() != null)
            {
                returnObjectDef = EmitCompoundStatement(statement.compoundStatement());
            }
            else if (statement.expressionStatement() != null)
            {
                returnObjectDef = EmitExpressionStatement(statement.expressionStatement());
            }
            else if (statement.selectionStatement() != null)
            {
                returnObjectDef = EmitSelectionStatement(statement.selectionStatement());
            }
            else if (statement.iterationStatement() != null)
            {
                returnObjectDef = EmitIterationStatement(statement.iterationStatement());
            }
            else if (statement.jumpStatement() != null)
            {
                returnObjectDef = EmitJumpStatement(statement.jumpStatement());
            }
            else
            {
                returnObjectDef = null;
            }

            return returnObjectDef;
        }

        protected ObjectDef EmitExpressionStatement(CParser.ExpressionStatementContext expressionStatement)
        {
            ObjectDef returnObjectDef;

            if (expressionStatement.expression() != null)
            {
                returnObjectDef = EmitExpression(expressionStatement.expression());
            }
            else
            {
                returnObjectDef = null;
            }

            return returnObjectDef;
        }

        protected ObjectDef EmitSelectionStatement(CParser.SelectionStatementContext selectionStatement)
        {
            return null;
        }

        protected ObjectDef EmitIterationStatement(CParser.IterationStatementContext iterationStatement)
        {
            return null;
        }

        protected ObjectDef EmitJumpStatement(CParser.JumpStatementContext jumpStatement)
        {
            return null;
        }

        protected ObjectDef EmitExpression(CParser.ExpressionContext expression)
        {
            return null;
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
    }
}
