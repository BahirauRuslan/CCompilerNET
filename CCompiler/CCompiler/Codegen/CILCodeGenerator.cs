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
                /*var returnObjectDef =*/ EmitCompoundStatement(compoundStatement);

                //returnObjectDef.Load();

                if (_functions[functionName].MethodBuilder.ReturnType == typeof(void))
                {
                    _generatorIL.Emit(OpCodes.Pop);
                }

                //_generatorIL.Emit(OpCodes.Ret);

                //returnObjectDef.Remove();
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
            ObjectDef returnObjectDef = null;

            if (blockItem.statement() != null)
            {
                returnObjectDef = EmitStatement(blockItem.statement());
            }
            else if (blockItem.declaration() != null)
            {
                returnObjectDef = EmitDeclaration(blockItem.declaration());
            }

            return returnObjectDef;
        }

        protected ObjectDef EmitDeclaration(CParser.DeclarationContext declaration)
        {
            ObjectDef returnObjectDef = null;

            if (declaration.RBATypeSpecifier() != null &&
                declaration.initDeclaratorList() != null)
            {
                returnObjectDef = EmitLocalVarDeclarations(
                                      declaration.RBATypeSpecifier(),
                                      declaration.initDeclaratorList().RBAInitDeclaratorStack());
            }

            return returnObjectDef;
        }

        protected ObjectDef EmitLocalVarDeclarations(CParser.TypeSpecifierContext typeSpecifier,
            Stack<CParser.InitDeclaratorContext> initDeclarators)
        {
            Type type = typeSpecifier.RBAType(_moduleBuilder);
            ObjectDef returnObjectDef = null;

            while (initDeclarators.Count > 0)
            {
                returnObjectDef = EmitLocalVarDeclaration(type, initDeclarators.Pop());
            }

            return returnObjectDef;
        }

        protected ObjectDef EmitLocalVarDeclaration(Type type, CParser.InitDeclaratorContext initDeclarator)
        {
            ObjectDef localObjDef = null;

            var name = initDeclarator.declarator()?.directDeclarator()?.Identifier()?.ToString();

            if (initDeclarator.Assign() != null)
            {
                localObjDef = EmitAssignmentExpression(initDeclarator.initializer()?.assignmentExpression());
            }
            else
            {
                localObjDef = EmitDefaultValue(type);
            }

            localObjDef.Load();

            return LocalObjectDef.AllocateLocal(type, name);
        }

        protected ObjectDef EmitStatement(CParser.StatementContext statement)
        {
            ObjectDef returnObjectDef = null;

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

            return returnObjectDef;
        }

        protected ObjectDef EmitExpressionStatement(CParser.ExpressionStatementContext expressionStatement)
        {
            ObjectDef returnObjectDef = null;

            if (expressionStatement.expression() != null)
            {
                returnObjectDef = EmitExpression(expressionStatement.expression());
            }

            return returnObjectDef;
        }

        protected ObjectDef EmitSelectionStatement(CParser.SelectionStatementContext selectionStatement)
        {
            ObjectDef returnObjectDef = null;

            if (selectionStatement.If() != null &&
                selectionStatement.expression() != null &&
                selectionStatement.statement() != null &&
                selectionStatement.statement().Length > 0)
            {
                returnObjectDef = EmitSelectionIfStatement(selectionStatement);
            }

            return returnObjectDef;
        }

        protected ObjectDef EmitSelectionIfStatement(CParser.SelectionStatementContext selectionStatement)
        {
            var checkObjectDef = EmitExpression(selectionStatement.expression());

            checkObjectDef.Load();
            checkObjectDef.Remove();

            var exitLabel = _generatorIL.DefineLabel();
            var elseLabel = _generatorIL.DefineLabel();

            _generatorIL.Emit(OpCodes.Brfalse, elseLabel);

            EmitStatement(selectionStatement.statement()[0]);

            _generatorIL.Emit(OpCodes.Br, exitLabel);
            _generatorIL.MarkLabel(elseLabel);

            if (selectionStatement.Else() != null &&
                selectionStatement.statement().Length == 2)
            {
                EmitStatement(selectionStatement.statement()[1]);
            }

            _generatorIL.MarkLabel(exitLabel);
            _generatorIL.Emit(OpCodes.Nop);

            return null;
        }

        protected ObjectDef EmitIterationStatement(CParser.IterationStatementContext iterationStatement)
        {
            if (iterationStatement.While() != null)
            {
                if (iterationStatement.Do() != null)
                {
                    EmitDoWhileIterationStatement(iterationStatement);
                }
                else
                {
                    EmitWhileIterationStatement(iterationStatement);
                }
            }

            return null;
        }

        protected ObjectDef EmitWhileIterationStatement(CParser.IterationStatementContext iterationStatement)
        {
            var checkLabel = _generatorIL.DefineLabel();
            var exitLabel = _generatorIL.DefineLabel();

            _generatorIL.MarkLabel(checkLabel);

            var checkObjectDef = EmitExpression(iterationStatement.expression());

            checkObjectDef.Load();
            checkObjectDef.Remove();
            _generatorIL.Emit(OpCodes.Brfalse, exitLabel);
            EmitStatement(iterationStatement.statement());
            _generatorIL.Emit(OpCodes.Br, checkLabel);
            _generatorIL.MarkLabel(exitLabel);
            _generatorIL.Emit(OpCodes.Nop);

            return null;
        }

        protected ObjectDef EmitDoWhileIterationStatement(CParser.IterationStatementContext iterationStatement)
        {
            var repeatLabel = _generatorIL.DefineLabel();

            _generatorIL.MarkLabel(repeatLabel);
            EmitStatement(iterationStatement.statement());

            var checkObjectDef = EmitExpression(iterationStatement.expression());

            checkObjectDef.Load();
            checkObjectDef.Remove();

            _generatorIL.Emit(OpCodes.Brtrue, repeatLabel);

            return null;
        }

        protected ObjectDef EmitJumpStatement(CParser.JumpStatementContext jumpStatement)
        {
            ObjectDef returnObjectDef = null;

            if (jumpStatement.expression() != null && jumpStatement.Return() != null)
            {
                returnObjectDef = EmitExpression(jumpStatement.expression());
                /////
                returnObjectDef.Load();
                _generatorIL.Emit(OpCodes.Ret);
                returnObjectDef.Remove();
            }

            return returnObjectDef;
        }

        protected ObjectDef EmitExpression(CParser.ExpressionContext expression)
        {
            ObjectDef returnObjectDef = null;

            var assignmentExpressionStack = expression.RBAAssignmentExpressionStack();

            while (assignmentExpressionStack.Count > 0)
            {
                returnObjectDef = EmitAssignmentExpression(assignmentExpressionStack.Pop());
            }

            return returnObjectDef;
        }

        protected ObjectDef EmitAssignmentExpression(CParser.AssignmentExpressionContext assignmentExpression)
        {
            ObjectDef returnObjectDef = null;

            if (assignmentExpression.conditionalExpression() != null)
            {
                returnObjectDef = EmitConditionalExpression(assignmentExpression.conditionalExpression());
            }
            else if (assignmentExpression.unaryExpression() != null &&
                assignmentExpression.assignmentExpression() != null)
            {
                var identifier = assignmentExpression.unaryExpression()
                    ?.postfixExpression()
                    ?.primaryExpression()
                    ?.Identifier();
                var localVar = LocalObjectDef.GetLocalObjectDef(identifier.ToString());

                EmitUnaryExpression(assignmentExpression.unaryExpression());

                var assignmentObjDef = EmitAssignmentExpression(assignmentExpression.assignmentExpression());

                assignmentObjDef.Load();

                returnObjectDef = LocalObjectDef.AllocateLocal(localVar.Type, localVar.Name); // TODO: Emit assignment expressions
            }

            return returnObjectDef;
        }

        protected ObjectDef EmitConditionalExpression(CParser.ConditionalExpressionContext conditionalExpression)
        {
            ObjectDef returnObjectDef = null;

            if (conditionalExpression.logicalOrExpression() != null)
            {
                returnObjectDef = EmitLogicalOrExpression(conditionalExpression.logicalOrExpression());
            }

            if (conditionalExpression.logicalOrExpression() != null &&
                conditionalExpression.expression() != null &&
                conditionalExpression.conditionalExpression() != null)
            {
                returnObjectDef = null; // TODO: Emit conditional expression
            }

            return returnObjectDef;
        }

        protected ObjectDef EmitLogicalOrExpression(CParser.LogicalOrExpressionContext logicalOrExpression)
        {
            ObjectDef returnObjectDef = null;

            if (logicalOrExpression.logicalAndExpression() != null)
            {
                returnObjectDef = EmitLogicalAndExpression(logicalOrExpression.logicalAndExpression());
            }

            if (logicalOrExpression.logicalAndExpression() != null &&
                logicalOrExpression.logicalOrExpression() != null)
            {
                returnObjectDef = null; // TODO: Emit logical 'OR' expression
            }

            return returnObjectDef;
        }

        protected ObjectDef EmitLogicalAndExpression(CParser.LogicalAndExpressionContext logicalAndExpression)
        {
            ObjectDef returnObjectDef = null;

            if (logicalAndExpression.inclusiveOrExpression() != null)
            {
                returnObjectDef = EmitInclusiveOrExpression(logicalAndExpression.inclusiveOrExpression());
            }

            if (logicalAndExpression.inclusiveOrExpression() != null &&
                logicalAndExpression.logicalAndExpression() != null)
            {
                returnObjectDef = null; // TODO: Emit logical 'AND' expression
            }

            return returnObjectDef;
        }

        protected ObjectDef EmitInclusiveOrExpression(CParser.InclusiveOrExpressionContext inclusiveOrExpression)
        {
            ObjectDef returnObjectDef = null;

            if (inclusiveOrExpression.exclusiveOrExpression() != null)
            {
                returnObjectDef = EmitExclusiveOrExpression(inclusiveOrExpression.exclusiveOrExpression());
            }

            if (inclusiveOrExpression.exclusiveOrExpression() != null &&
                inclusiveOrExpression.inclusiveOrExpression() != null)
            {
                var orObj = EmitInclusiveOrExpression(inclusiveOrExpression.inclusiveOrExpression());

                orObj.Load();
                returnObjectDef.Load();
                _generatorIL.Emit(OpCodes.Or);

                returnObjectDef = LocalObjectDef.AllocateLocal(typeof(int));
            }

            return returnObjectDef;
        }

        protected ObjectDef EmitExclusiveOrExpression(CParser.ExclusiveOrExpressionContext exclusiveOrExpression)
        {
            ObjectDef returnObjectDef = null;

            if (exclusiveOrExpression.andExpression() != null)
            {
                returnObjectDef = EmitAndExpression(exclusiveOrExpression.andExpression());
            }

            if (exclusiveOrExpression.andExpression() != null &&
                exclusiveOrExpression.exclusiveOrExpression() != null)
            {
                var orObj = EmitExclusiveOrExpression(exclusiveOrExpression.exclusiveOrExpression());

                orObj.Load();
                returnObjectDef.Load();
                _generatorIL.Emit(OpCodes.Xor);

                returnObjectDef = LocalObjectDef.AllocateLocal(typeof(int));
            }

            return returnObjectDef;
        }

        protected ObjectDef EmitAndExpression(CParser.AndExpressionContext andExpression)
        {
            ObjectDef returnObjectDef = null;

            if (andExpression.equalityExpression() != null)
            {
                returnObjectDef = EmitEqualityExpression(andExpression.equalityExpression());
            }

            if (andExpression.equalityExpression() != null &&
                andExpression.andExpression() != null)
            {
                var andObj = EmitAndExpression(andExpression.andExpression());

                andObj.Load();
                returnObjectDef.Load();
                _generatorIL.Emit(OpCodes.And);

                returnObjectDef = LocalObjectDef.AllocateLocal(typeof(int));
            }

            return returnObjectDef;
        }

        protected ObjectDef EmitEqualityExpression(CParser.EqualityExpressionContext equalityExpression)
        {
            ObjectDef returnObjectDef = null;

            if (equalityExpression.relationalExpression() != null)
            {
                returnObjectDef = EmitRelationalExpression(equalityExpression.relationalExpression());
            }

            if (equalityExpression.relationalExpression() != null &&
                equalityExpression.equalityExpression() != null)
            {
                var equalityObj = EmitEqualityExpression(equalityExpression.equalityExpression());

                equalityObj.Load();
                returnObjectDef.Load();

                if (equalityExpression.Equal() != null)
                {
                    _generatorIL.Emit(OpCodes.Ceq);
                }
                else if (equalityExpression.NotEqual() != null)
                {
                    var zero = new ValueObjectDef(typeof(int), 0);

                    _generatorIL.Emit(OpCodes.Ceq);
                    zero.Load();
                    _generatorIL.Emit(OpCodes.Ceq);
                }

                returnObjectDef = LocalObjectDef.AllocateLocal(typeof(int));
            }

            return returnObjectDef;
        }

        protected ObjectDef EmitRelationalExpression(CParser.RelationalExpressionContext relationalExpression)
        {
            ObjectDef returnObjectDef = null;

            if (relationalExpression.shiftExpression() != null)
            {
                returnObjectDef = EmitShiftExpression(relationalExpression.shiftExpression());
            }

            if (relationalExpression.shiftExpression() != null &&
                relationalExpression.relationalExpression() != null)
            {
                var relationalObj = EmitRelationalExpression(relationalExpression.relationalExpression());

                if (relationalExpression.Less() != null)
                {
                    relationalObj.Load();
                    returnObjectDef.Load();

                    _generatorIL.Emit(OpCodes.Clt);
                }
                else if (relationalExpression.LessEqual() != null)
                {
                    ObjectDef less;
                    ObjectDef equal;

                    relationalObj.Load();
                    returnObjectDef.Load();

                    _generatorIL.Emit(OpCodes.Clt);

                    less = LocalObjectDef.AllocateLocal(typeof(int));

                    returnObjectDef.Load();
                    relationalObj.Load();

                    _generatorIL.Emit(OpCodes.Ceq);

                    equal = LocalObjectDef.AllocateLocal(typeof(int));

                    less.Load();
                    equal.Load();

                    _generatorIL.Emit(OpCodes.Or);
                }
                else if (relationalExpression.Greater() != null)
                {
                    relationalObj.Load();
                    returnObjectDef.Load();

                    _generatorIL.Emit(OpCodes.Cgt);
                }
                else if (relationalExpression.GreaterEqual() != null)
                {
                    ObjectDef greater;
                    ObjectDef equal;

                    relationalObj.Load();
                    returnObjectDef.Load();

                    _generatorIL.Emit(OpCodes.Cgt);

                    greater = LocalObjectDef.AllocateLocal(typeof(int));

                    relationalObj.Load();
                    returnObjectDef.Load();

                    _generatorIL.Emit(OpCodes.Ceq);

                    equal = LocalObjectDef.AllocateLocal(typeof(int));

                    greater.Load();
                    equal.Load();

                    _generatorIL.Emit(OpCodes.Or);
                }

                returnObjectDef = LocalObjectDef.AllocateLocal(typeof(int));
            }

            return returnObjectDef;
        }

        protected ObjectDef EmitShiftExpression(CParser.ShiftExpressionContext shiftExpression)
        {
            ObjectDef returnObjectDef = null;

            if (shiftExpression.additiveExpression() != null)
            {
                returnObjectDef = EmitAdditiveExpression(shiftExpression.additiveExpression());
            }

            if (shiftExpression.additiveExpression() != null &&
                shiftExpression.shiftExpression() != null)
            {
                var shiftObj = EmitShiftExpression(shiftExpression.shiftExpression());

                shiftObj.Load();
                returnObjectDef.Load();

                if (shiftExpression.LeftShift() != null)
                {
                    _generatorIL.Emit(OpCodes.Shl);
                }
                else if (shiftExpression.RightShift() != null)
                {
                    _generatorIL.Emit(OpCodes.Shr);
                }

                returnObjectDef = LocalObjectDef.AllocateLocal(typeof(int));
            }

            return returnObjectDef;
        }

        protected ObjectDef EmitAdditiveExpression(CParser.AdditiveExpressionContext additiveExpression)
        {
            ObjectDef returnObjectDef = null;

            if (additiveExpression.multiplicativeExpression() != null)
            {
                returnObjectDef = EmitMultiplicativeExpression(additiveExpression.multiplicativeExpression());
            }

            if (additiveExpression.multiplicativeExpression() != null &&
                additiveExpression.additiveExpression() != null)
            {
                var additiveObj = EmitAdditiveExpression(additiveExpression.additiveExpression());

                additiveObj.Load();
                returnObjectDef.Load();

                if (additiveExpression.Plus() != null)
                {
                    _generatorIL.Emit(OpCodes.Add);
                }
                else if (additiveExpression.Minus() != null)
                {
                    _generatorIL.Emit(OpCodes.Sub);
                }
                
                returnObjectDef = LocalObjectDef.AllocateLocal(typeof(int));
            }

            return returnObjectDef;
        }

        protected ObjectDef EmitMultiplicativeExpression(CParser.MultiplicativeExpressionContext multiplicativeExpression)
        {
            ObjectDef returnObjectDef = null;

            if (multiplicativeExpression.castExpression() != null)
            {
                returnObjectDef = EmitCastExpression(multiplicativeExpression.castExpression());
            }

            if (multiplicativeExpression.castExpression() != null &&
                multiplicativeExpression.multiplicativeExpression() != null)
            {
                var multiplicativeObj = EmitMultiplicativeExpression(multiplicativeExpression.multiplicativeExpression());

                multiplicativeObj.Load();
                returnObjectDef.Load();

                if (multiplicativeExpression.Star() != null)
                {
                    _generatorIL.Emit(OpCodes.Mul);
                }
                else if (multiplicativeExpression.Div() != null)
                {
                    _generatorIL.Emit(OpCodes.Div);
                }
                else if (multiplicativeExpression.Mod() != null)
                {
                    _generatorIL.Emit(OpCodes.Rem);
                }
                
                returnObjectDef = LocalObjectDef.AllocateLocal(typeof(int));
            }

            return returnObjectDef;
        }

        protected ObjectDef EmitCastExpression(CParser.CastExpressionContext castExpression)
        {
            ObjectDef returnObjectDef = null;

            if (castExpression.unaryExpression() != null)
            {
                returnObjectDef = EmitUnaryExpression(castExpression.unaryExpression());
            }
            else
            {
                returnObjectDef = null; // Skip cast expression
            }

            return returnObjectDef;
        }

        protected ObjectDef EmitUnaryExpression(CParser.UnaryExpressionContext unaryExpression)
        {
            ObjectDef returnObjectDef = null;

            if (unaryExpression.postfixExpression() != null)
            {
                returnObjectDef = EmitPostfixExpression(unaryExpression.postfixExpression());
            }
            else
            {
                returnObjectDef = null; // Skip unary expressions
            }

            return returnObjectDef;
        }

        protected ObjectDef EmitPostfixExpression(CParser.PostfixExpressionContext postfixExpression)
        {
            ObjectDef returnObjectDef = null;

            if (postfixExpression.primaryExpression() != null)
            {
                returnObjectDef = EmitPrimaryExpression(postfixExpression.primaryExpression());
            }
            else
            {
                returnObjectDef = null; // Skip postfix expressions
            }

            return returnObjectDef;
        }

        protected ObjectDef EmitPrimaryExpression(CParser.PrimaryExpressionContext primaryExpression)
        {
            ObjectDef returnObjectDef = null;

            if (primaryExpression.Identifier() != null)
            {
                returnObjectDef = LocalObjectDef.GetLocalObjectDef(primaryExpression.Identifier().ToString());   // TODO: load from identifier
            }
            else if (primaryExpression.Constant() != null)
            {
                returnObjectDef = EmitConstant(primaryExpression.Constant());
            }
            else if (primaryExpression.expression() != null)
            {
                returnObjectDef = EmitExpression(primaryExpression.expression());
            }
            else if (primaryExpression.StringLiteral() != null)
            {
                ;
            }

            return returnObjectDef;
        }

        protected ObjectDef EmitConstant(ITerminalNode constant)
        {
            ObjectDef returnObjectDef = null;

            if (int.TryParse(constant.ToString(), out _))
            {
                returnObjectDef = EmitInteger(constant);
            }
            else if (double.TryParse(constant.ToString(), out _))
            {
                returnObjectDef = EmitDouble(constant);
            }

            return returnObjectDef;
        }

        protected ObjectDef EmitInteger(ITree expressionNode)
        {
            var result = new ValueObjectDef(typeof(int), int.Parse(expressionNode.ToString()));
            return result;
        }

        protected ObjectDef EmitDouble(ITree expressionNode)
        {
            var result = new ValueObjectDef(typeof(double), double.Parse(expressionNode.ToString()));
            return result;
        }

        protected ObjectDef EmitString(ITree expressionNode)
        {
            var result = new ValueObjectDef(typeof(string), expressionNode.ToString());
            return result;
        }

        protected ObjectDef EmitBoolean(ITree expressionNode)
        {
            var result = new ValueObjectDef(typeof(bool), bool.Parse(expressionNode.ToString()));
            return result;
        }

        protected ObjectDef EmitVoid(ITree expressionNode)
        {
            var result = new ValueObjectDef(typeof(Nullable), null);
            return result;
        }

        protected static ValueObjectDef EmitDefaultValue(Type type)
        {
            object value;

            if (type == typeof(bool))
            {
                value = false;
            }
            else if (type == typeof(int))
            {
                value = 0;
            }
            else if (type == typeof(string))
            {
                value = "";
            }
            else
            {
                value = null;
            }

            var result = new ValueObjectDef(type, value);

            return result;
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
