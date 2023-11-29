﻿using NCCompiler_CompilersCourse.Semantics;

namespace NCCompiler_CompilersCourse.CodeGeneration;

public class TranslationVisitorCodeGeneration : IVisitorCodeGeneration
{
    private CodeGenerationScopeStack ScopeStack { get; } = new ();

    private List<string> _structDeclarations = new();
    private List<string> _routinesCode = new();
    public string ResultingProgram = "";
    
    public void VisitProgramNode(ProgramNode programNode, Queue<BaseCommand> commands)
    {
        var programString = "";
        var mainClassName = "MainClass";

        programString +=
            $".class private auto ansi beforefieldinit {mainClassName} extends [System.Runtime]System.Object";

        foreach (var declaration in programNode.Declarations)
        {
            declaration.Accept(this, commands);
        }

        // Structs
        programString += "{\n";

        foreach (var structStr in _structDeclarations)
        {
            programString += structStr;
        }
        
        // Global variables
        foreach (var (_, globalVar) in ScopeStack.GetLastScope().LocalVariables)
        {
            programString += $".field public {_getTypeFromTypeNode(globalVar.Type)} {globalVar.GetName()}\n";
        }
        
        // Functions
        foreach (var routineCode in _routinesCode)
        {
            programString += routineCode;
        }
        
        // Constructor
        int maxStack = 50;
        
        programString += ".method public hidebysig specialname rtspecialname instance void .ctor() cil managed";
        programString += "{\n";
        programString += $".maxstack {maxStack}";

        foreach (var command in commands)
        {
            programString += command.Translate() + '\n';
        }

        programString += new ReturnCommand(commands.Count).Translate();
        
        programString += "}\n";
        
        // End of main class
        programString += "}";
        ResultingProgram = programString;
    }

    public void VisitGetFieldNode(GetFieldNode getFieldNode, Queue<BaseCommand> commands)
    {
        getFieldNode.StructVarNode.Accept(this, commands);
        commands.Enqueue(new LoadFieldCommand(getFieldNode.Type, getFieldNode.StructVarNode.Name!,
            getFieldNode.FieldName, commands.Count));
    }

    public void VisitGetByIndexNode(GetByIndexNode getByIndexNode, Queue<BaseCommand> commands)
    {
        getByIndexNode.ArrayVarNode.Accept(this, commands);
        getByIndexNode.Index.Accept(this, commands);
        commands.Enqueue(new LoadByIndexCommand(getByIndexNode.Type, commands.Count));
    }

    private (string, TypeNode) _createNewVar(VarNode varNode, TypeNode typeNode)
    {
        var name = varNode.Name!;
        ScopeStack.AddVariableInLastScope(name, typeNode);
        return (name, typeNode);
    }

    private void _handleDeclaration(DeclarationNode declarationNode, Queue<BaseCommand> commands)
    {
        var (name, _) = _createNewVar(declarationNode.Variable, declarationNode.Type);
        
        if (declarationNode is TypeVariableDeclaration) return;

        var codeGenVar = ScopeStack.GetVariable(name)!;
        
        // getting value on top of the stack
        ((ValueNode) declarationNode.Variable.Value!).Accept(this, commands);
        
        // setting value to variable
        commands.Enqueue(new SetLocalCommand(codeGenVar.Id, commands.Count));
    }

    public void VisitValueVariableDeclaration(ValueVariableDeclaration valueVariableDeclaration,
        Queue<BaseCommand> commands)
    {
        _handleDeclaration(valueVariableDeclaration, commands);
    }

    public void VisitTypeVariableDeclaration(TypeVariableDeclaration typeVariableDeclaration,
        Queue<BaseCommand> commands)
    {
        _handleDeclaration(typeVariableDeclaration, commands);
    }

    public void VisitFullVariableDeclaration(FullVariableDeclaration fullVariableDeclaration,
        Queue<BaseCommand> commands)
    {
        _handleDeclaration(fullVariableDeclaration, commands);
    }

    public void VisitSortedArrayNode(SortedArrayNode sortedArrayNode, Queue<BaseCommand> commands)
    {
        // ..., arr -> ..., arr
        // + Declared special variable
        string type = _getTypeOfArrayElement(((ArrayTypeNode)sortedArrayNode.Type).ElementTypeNode);
        var (_, specialVar) = _makeCopyOfArrayAndPerformFunctionCall(
            $"void [System.Runtime]System.Array::Sort<{type}>(!!0/*{type}*/[])",
            sortedArrayNode, commands);
        // Load for return
        commands.Enqueue(new LoadLocalCommand(specialVar.Id, commands.Count));
    }

    public void VisitArraySizeNode(ArraySizeNode arraySizeNode, Queue<BaseCommand> commands)
    {
        arraySizeNode.Array.Accept(this, commands);
        // ..., arr -> ..., length
        commands.Enqueue(new ArrayLength(commands.Count));
    }


    private (string, CodeGenerationVariable) _makeCopyOfArrayAndPerformFunctionCall(string function,
        ArrayFunctions arrayFunctions, Queue<BaseCommand> commands)
    {
        /* Needs array on the stack, makes new variable and assigns value to it */
        // ..., array -> ..., array

        // Loading should be performed in Array Variable
        // load Array to stack
        //      if from struct:
        //          getField from struct
        //      if just array:
        //          get from variable

        // Assuming that Array on top of the stack

        // Make a copy of array on the stack (Using clone function)
        // Cast it to necessary type
        // Assign modified copy to new variable
        // And load it to stack
        // call reverse function
        // result is on top of the stack

        // ... -> ..., arr
        arrayFunctions.Array.Accept(this, commands);

        // make new temp variable
        var nameOfTemp = ScopeStack.AddSpecialVariableInLastScope(arrayFunctions.Type);
        var specialVar = ScopeStack.GetVariable(nameOfTemp)!;

        // Clone
        commands.Enqueue(new CallVirtualCommand("instance object [System.Runtime]System.Array::Clone()", commands.Count));

        // Cast
        commands.Enqueue(new CastClassCommand(arrayFunctions.Type, commands.Count));

        // Set to special variable
        commands.Enqueue(new SetLocalCommand(specialVar.Id, commands.Count));

        // Load for call
        commands.Enqueue(new LoadLocalCommand(specialVar.Id, commands.Count));

        // Reverse func
        commands.Enqueue(new CallCommand(function, commands.Count));
        return (nameOfTemp, specialVar);
    }

    private string _getTypeOfArrayElement(TypeNode typeNode)
    {
        return typeNode switch
        {
            var elem => elem.MyType switch
            {
                MyType.Integer => "int32",
                MyType.Real => "float32",
                MyType.Boolean => "bool",
                _ => throw new Exception($"Unexpected type for sorting: {elem.GetType()}"),
            }
        };
    }

    public void VisitReversedArrayNode(ReversedArrayNode reversedArrayNode, Queue<BaseCommand> commands)
    {
        // ..., arr -> ..., arr
        // + Declared special variable
        string type = _getTypeOfArrayElement(((ArrayTypeNode)reversedArrayNode.Type).ElementTypeNode);
        var (_, specialVar) = _makeCopyOfArrayAndPerformFunctionCall(
            $"void [System.Runtime]System.Array::Reverse<{type}>(!!0/*{type}*/[])",
            reversedArrayNode, commands);
        // Load for return
        commands.Enqueue(new LoadLocalCommand(specialVar.Id, commands.Count));
    }

    public void VisitVariableDeclarations(VariableDeclarations variableDeclarations, Queue<BaseCommand> commands)
    {
        foreach (VarNode variableDeclaration in variableDeclarations.Declarations.Values)
        {
            variableDeclaration.AcceptStructField(this, commands);
        }
    }

    public void VisitBreakNode(BreakNode breakNode, Queue<BaseCommand> commands)
    {
        commands.Enqueue(new JumpForBreakCommand(commands.Count));
    }

    public void VisitAssertNode(AssertNode assertNode, Queue<BaseCommand> commands)
    {
        assertNode.LeftExpression.Accept(this, commands);
        assertNode.RightExpression.Accept(this, commands);
        commands.Enqueue(new OperationCommand(OperationType.Eq, commands.Count));
        commands.Enqueue(new CallCommand("void [System.Runtime]System.Diagnostics.Debug::Assert(bool, string)", commands.Count));
    }


    public void VisitValueReturnNode(ValueReturnNode valueReturnNode, Queue<BaseCommand> commands)
    {
        valueReturnNode.Value.Accept(this, commands);
        commands.Enqueue(new ReturnCommand(commands.Count));
    }

    /*
     * Range node visit.
     * Puts left then right bounds on the stack.
     */
    public void VisitRangeNode(RangeNode rangeNode, Queue<BaseCommand> commands)
    {
        rangeNode.RightBound.Accept(this, commands);
        rangeNode.LeftBound.Accept(this, commands);
    }

    public void VisitForLoopNode(ForLoopNode forLoopNode, Queue<BaseCommand> commands)
    {
        RangeNode range = forLoopNode.Range;
        range.Accept(this, commands);
        string from = forLoopNode.IdName.Name!;
        string to = ScopeStack.AddSpecialVariableInLastScope(new TypeNode(myType: MyType.Integer));
        if (range.Reversed) (from, to) = (to, from);
        int fromId = ScopeStack.GetVariable(from)!.Id;
        ScopeStack.AddVariableInLastScope(from, new TypeNode(myType: MyType.Integer));
        commands.Enqueue(new SetLocalCommand(fromId, commands.Count));
        int toId = ScopeStack.GetVariable(to)!.Id;
        commands.Enqueue(new SetLocalCommand(toId, commands.Count));
        var conditionJumper = new JumpCommand(commands.Count);
        commands.Enqueue(conditionJumper);
        int startBodyAddress = commands.Count;
        commands.Enqueue(new NopCommand(commands.Count));
        forLoopNode.Body.Accept(this, commands);
        int endBodyAddress = commands.Count() + 3;
        conditionJumper.SetAddress(endBodyAddress);
        var commandsList = commands.ToArray();
        for (int i = startBodyAddress; i < endBodyAddress - 3; ++i)
        {
            if (commandsList[i] is JumpForBreakCommand { Address: null } jumpForBreakCommand)
                jumpForBreakCommand.SetAddress(endBodyAddress);
        }

        commands.Enqueue(new LoadLocalCommand(fromId, commands.Count));
        commands.Enqueue(new LoadConstantCommand(1, commands.Count));
        commands.Enqueue(range.Reversed
            ? new OperationCommand(OperationType.Minus, commands.Count)
            : new OperationCommand(OperationType.Plus, commands.Count));
        commands.Enqueue(new LoadLocalCommand(toId, commands.Count));
        commands.Enqueue(range.Reversed
            ? new OperationCommand(OperationType.Gt, commands.Count)
            : new OperationCommand(OperationType.Lt, commands.Count));
        var beginJumper = new JumpIfTrue(commands.Count);
        beginJumper.SetAddress(startBodyAddress);
        commands.Enqueue(beginJumper);
    }

    public void VisitForEachLoopNode(ForEachLoopNode forEachLoopNode, Queue<BaseCommand> commands)
    {
        string identifier = forEachLoopNode.IdName.Name!;
        var type = forEachLoopNode.Array.Type.ElementTypeNode;
        var counter = ScopeStack.AddSpecialVariableInLastScope(new TypeNode(myType: MyType.Integer));
        var counterId = ScopeStack.GetVariable(counter)!.Id;
        commands.Enqueue(new LoadConstantCommand(0, commands.Count));
        commands.Enqueue(new SetLocalCommand(counterId, commands.Count));
        var size = ScopeStack.AddSpecialVariableInLastScope(new TypeNode(myType: MyType.Integer));
        var sizeId = ScopeStack.GetVariable(size)!.Id;
        forEachLoopNode.Array.Type.Size!.Accept(this, commands);
        commands.Enqueue(new SetLocalCommand(sizeId, commands.Count));
        ScopeStack.AddVariableInLastScope(identifier, type);
        
        int identifierId = ScopeStack.GetVariable(identifier)!.Id;
        string array = ScopeStack.AddSpecialVariableInLastScope(forEachLoopNode.Array.Type);
        int arrayId = ScopeStack.GetVariable(array)!.Id;
        forEachLoopNode.Array.Accept(this, commands);
        commands.Enqueue(new SetLocalCommand(arrayId, commands.Count));
        int jumpBeforeAddress = commands.Count;
        
        commands.Enqueue(new LoadLocalCommand(counterId, commands.Count));
        commands.Enqueue(new LoadLocalCommand(sizeId, commands.Count));
        commands.Enqueue(new OperationCommand(OperationType.Lt, commands.Count));
        var jumperAfter = new JumpIfFalse(commands.Count);
        commands.Enqueue(jumperAfter);
        commands.Enqueue(new LoadLocalCommand(arrayId, commands.Count));
        commands.Enqueue(new LoadLocalCommand(counterId, commands.Count));
        commands.Enqueue(new LoadByIndexCommand(type, commands.Count));
        commands.Enqueue(new SetLocalCommand(identifierId, commands.Count));
        var beforeBodyAddress = commands.Count;
        commands.Enqueue(new NopCommand(commands.Count));
        forEachLoopNode.Body.Accept(this, commands);
        var afterBodyAddress = commands.Count;
        var commandsList = commands.ToArray();
        for (int i = beforeBodyAddress; i < afterBodyAddress; ++i)
        {
            if (commandsList[i] is JumpForBreakCommand { Address: null } jumpForBreakCommand)
                jumpForBreakCommand.SetAddress(afterBodyAddress);
        }
        commands.Enqueue(new LoadLocalCommand(counterId, commands.Count));
        commands.Enqueue(new LoadConstantCommand(1, commands.Count));
        commands.Enqueue(new OperationCommand(OperationType.Plus, commands.Count));
        commands.Enqueue(new SetLocalCommand(counterId, commands.Count));
        
        commands.Enqueue(new JumpCommand (commands.Count) { Address = jumpBeforeAddress });
        var jumpAfterAddress = commands.Count;
        jumperAfter.SetAddress(jumpAfterAddress);
        commands.Enqueue(new NopCommand(commands.Count));
    }

    public void VisitWhileLoopNode(WhileLoopNode whileLoopNode, Queue<BaseCommand> commands)
    {
        var conditionJumper = new JumpCommand(commands.Count);
        commands.Enqueue(conditionJumper);
        int startBodyAddress = commands.Count;
        commands.Enqueue(new NopCommand(commands.Count));
        whileLoopNode.Body.Accept(this, commands);
        int endBodyAddress = commands.Count;
        conditionJumper.SetAddress(endBodyAddress);
        var commandsList = commands.ToArray();
        for (int i = startBodyAddress; i < endBodyAddress; ++i)
        {
            if (commandsList[i] is JumpForBreakCommand { Address: null } jumpForBreakCommand)
                jumpForBreakCommand.SetAddress(endBodyAddress);
        }

        whileLoopNode.Condition.Accept(this, commands);
        var beginJumper = new JumpIfTrue(commands.Count);
        beginJumper.SetAddress(startBodyAddress);
        commands.Enqueue(beginJumper);
    }

    public void VisitIfStatement(IfStatement ifStatement, Queue<BaseCommand> commands)
    {
        // ... -> ...

        // ... -> ..., bool
        ifStatement.Condition.Accept(this, commands);

        // ..., bool -> ...
        var jump = new JumpIfFalse(commands.Count);
        commands.Enqueue(jump);
        ifStatement.Body.Accept(this, commands);
        commands.Enqueue(new NopCommand(commands.Count));
        jump.SetAddress(commands.Count);
    }

    public void VisitIfElseStatement(IfElseStatement ifElseStatement, Queue<BaseCommand> commands)
    {
        // ... -> ...

        // ... -> ..., bool
        ifElseStatement.Condition.Accept(this, commands);

        var jumpToElse = new JumpIfFalse(-1);
        var jumpToEnd = new JumpCommand(commands.Count);

        // ..., bool -> ...
        commands.Enqueue(jumpToElse);
        ifElseStatement.Body.Accept(this, commands);

        jumpToEnd.CommandIndex = commands.Count;
        commands.Enqueue(jumpToEnd);
        commands.Enqueue(new NopCommand(commands.Count));
        jumpToElse.SetAddress(commands.Count);

        ifElseStatement.BodyElse.Accept(this, commands);
        commands.Enqueue(new NopCommand(commands.Count));
        jumpToEnd.SetAddress(commands.Count);
    }

    public void VisitBodyNode(BodyNode bodyNode, Queue<BaseCommand> commands)
    {
        foreach (var statement in bodyNode.Statements)
        {
            statement.Accept(this, commands);
        }
    }

    public void VisitAssignmentNode(AssignmentNode assignmentNode, Queue<BaseCommand> commands)
    {
        // ... -> ...

        // Not array
        // ..., address -> ..., address, value
        var value = ((ValueNode)assignmentNode.Variable.Value!);

        switch (assignmentNode.Variable)
        {
            case GetFieldNode getFieldNode:
                getFieldNode.Accept(this, commands);
                value.Accept(this, commands);
                commands.Enqueue(new SetFieldCommand(value.Type, getFieldNode.StructVarNode.Name!,
                    getFieldNode.FieldName, commands.Count));
                break;
            case GetByIndexNode getByIndexNode:
                getByIndexNode.Accept(this, commands);

                // Loaded index to stack
                getByIndexNode.Index.Accept(this, commands);

                // loaded value to assign to stack
                value.Accept(this, commands);

                // Setting element by index
                commands.Enqueue(new SetElementByIndex(commands.Count));
                break;
            case VarNode varNode:
                value.Accept(this, commands);
                var name = varNode.Name!;
                var isArgument = false;
                var codeGenVar = ScopeStack.GetVariable(name);
                if (codeGenVar is null)
                {
                    codeGenVar = ScopeStack.GetArgumentInLastScope(name);
                    isArgument = true;
                }

                if (codeGenVar is null) throw new Exception("Cannot assign to undeclared variable");
                if (isArgument) commands.Enqueue(new SetArgumentByNameCommand(codeGenVar.GetName(), commands.Count));
                else commands.Enqueue(new SetLocalCommand(codeGenVar.Id, commands.Count));
                break;
            default:
                throw new Exception($"Unhandled node type: {assignmentNode.Variable}");
        }
    }

    public void VisitArrayTypeNode(ArrayTypeNode arrayTypeNode, Queue<BaseCommand> commands)
    {
        throw new NotImplementedException();
    }

    public void VisitStructTypeNode(StructTypeNode structTypeNode, Queue<BaseCommand> commands)
    {
        if (ScopeStack.GetByStructType(structTypeNode) is not null) return;
        var structName = "STRUCT_TYPE_" + ScopeStack.GetStructCounter();
        
        ScopeStack.GetGlobalScope().AddVariable(structName, structTypeNode);
        
        var structDeclaration = $".class nested sealed sequential ansi beforefieldinit {structName} extends [System.Runtime]System.ValueType";
        
        // fields
        structDeclaration += "{\n";

        foreach (var (name, type) in structTypeNode.StructFields)
        {
            structDeclaration += $".field {_getTypeFromTypeNode(type)} {name}\n";
        }

        structDeclaration += "}\n";
        
        _structDeclarations.Add(structDeclaration);
    }

    public void VisitCastNode(CastNode castNode, Queue<BaseCommand> commands)
    {
        // ... -> ..., value
        ((ValueNode)castNode.Value!).Accept(this, commands);

        // Casting Value
        // ..., value -> ..., castedValue
        commands.Enqueue(new PrimitiveCastCommand(castNode.Type, commands.Count));
    }

    private string _getTypeFromTypeNode(TypeNode typeNode)
    {
        return typeNode switch
        {
            StructTypeNode structTypeNode => ScopeStack.GetByStructType(structTypeNode)!.GetName(),
            ArrayTypeNode arrayTypeNode => _getTypeFromTypeNode(arrayTypeNode.ElementTypeNode) + "[]",
            { } node => node.MyType switch
            {
                MyType.Integer => "int32",
                MyType.Boolean => "bool",
                MyType.Real => "r4",
                MyType.Undefined => "void",
                _ => throw new Exception($"Cannot convert type node: {node}")
            },
            null => "void"
        };
    }

    private string _getTypeWithInstance(TypeNode? typeNode)
    {
        var typeStr = _getTypeFromTypeNode(typeNode ?? new TypeNode());
        if (typeStr != "void") typeStr = "instance " + typeStr;
        return typeStr;
    }
    public void VisitRoutineDeclarationNode(RoutineDeclarationNode routineDeclarationNode, Queue<BaseCommand> commands)
    {
        var typeStr = _getTypeWithInstance(routineDeclarationNode.ReturnType);
        var routineName = routineDeclarationNode.FunctionName.Name;
        var isMain = routineName == "main";
        
        string routineCode = "";
        routineCode += $".method public hidebysig {typeStr} {routineName}";
        
        ScopeStack.CreateNewScope(SemanticsScope.ScopeContext.Routine);
        
        if (routineDeclarationNode.Parameters != null)
            routineDeclarationNode.Parameters.Accept(this, commands);


        var lastScope = ScopeStack.GetLastScope();
        
        // Format function arguments
        routineCode += '(';
        var counter = 0;
        var len = lastScope.Arguments.Count;
        foreach (var (name, codeGenerationVariable) in lastScope.Arguments)
        {
            routineCode += _getTypeFromTypeNode(codeGenerationVariable.Type) + ' ' + name;
            if (counter < len - 1) routineCode += ", ";
            counter++;
        }
        routineCode += ')';
        
        // Cil managed
        routineCode += " cil managed";
        
        routineDeclarationNode.Body.Accept(this, commands);
        
        // function data
        routineCode += "{";
        
        // entry point
        if (isMain) routineCode += ".entrypoint\n";
        
        // MaxStack
        int maxStack = 50;
        
        routineCode += $".maxstack {maxStack}\n";
        
        // locals
        routineCode += ".locals init (";

        var sortedLocals = new List<CodeGenerationVariable>(lastScope.LocalVariables.Count); 
        
        foreach (var codeGenerationVariable in lastScope.LocalVariables.Values)
        {
            sortedLocals[codeGenerationVariable.Id] = codeGenerationVariable;
        }
        
        len = lastScope.LocalVariables.Count;

        for (int i = 0; i < len; i++)
        {
            var local = sortedLocals[i];
            routineCode += $"[{local.Id}] {_getTypeWithInstance(local.Type)} '{local.GetName()}'";
            if (i < len - 1) routineCode += ", ";
        }

        routineCode += ")\n";
        
        // Consume body
        foreach (var command in commands)
        {
            routineCode += command.Translate() + '\n';
        }

        routineCode += new ReturnCommand(commands.Count).Translate();
        
        routineCode += "}\n";
        
        ScopeStack.DeleteLastScope();
        
        _routinesCode.Add(routineCode);
    }

    public void VisitParametersNode(ParametersNode parametersNode, Queue<BaseCommand> commands)
    {
        foreach (ParameterNode parameter in parametersNode.Parameters)
        {
            ScopeStack.AddArgumentInLastScope(parameter.Variable.Name!, parameter.Variable.Type);
        }
    }

    public void VisitRoutineCallNode(RoutineCallNode routineCallNode, Queue<BaseCommand> commands)
    {
        List<string> types = new List<string>();
        if (routineCallNode.Expressions != null)
        {
            ExpressionsNode expressions = routineCallNode.Expressions;
            expressions.Accept(this, commands);
            foreach (var expression in expressions.Expressions)
            {
                types.Add($"{CodeGenerationVariable.NodeToType(expression.Type)}");
            }
        }

        var returnTypeString = routineCallNode.Routine.ReturnType == null
            ? "void"
            : $"instance {CodeGenerationVariable.NodeToType(routineCallNode.Routine.ReturnType)}";
        commands.Enqueue(new CallCommand(
            returnTypeString +
            $" Program::{routineCallNode.Routine.Name}({string.Join(" ", types.ToArray())})", commands.Count));
    }

    // Should be called only for routine call

    public void VisitExpressionsNode(ExpressionsNode expressionsNode, Queue<BaseCommand> commands)
    {
        foreach (ValueNode expression in expressionsNode.Expressions)
        {
            expression.Accept(this, commands);
        }
    }

    public void VisitConstNode(ConstNode constNode, Queue<BaseCommand> commands)
    {
        // Adds value to stack

        commands.Enqueue(new LoadConstantCommand(constNode.Value!, commands.Count));
    }

    public void VisitOperationNode(OperationNode operationNode, Queue<BaseCommand> commands)
    {
        // Operands add to stack operands
        // Adds Necessary Commands To perform operation
        // Result on the stack

        foreach (var operand in operationNode.Operands)
        {
            operand.Accept(this, commands);
        }

        foreach (var command in new OperationCommand(operationNode.OperationType, commands.Count).GetOperations())
        {
            commands.Enqueue(command);
        }
    }

    public void VisitArrayConst(ArrayConst arrayConst, Queue<BaseCommand> commands)
    {
        // Creating new temp variable for array with specified type
        // For each element -> load index and load value (make visit) and stelem.i4
        // loads address to the top of the stack

        var nameOfTemp = ScopeStack.AddSpecialVariableInLastScope(arrayConst.Type);

        var specialVariable = ScopeStack.GetVariable(nameOfTemp)!;

        // create new array
        commands.Enqueue(new LoadConstantCommand(arrayConst.Expressions.Expressions.Count, commands.Count));
        commands.Enqueue(new NewArrayCommand(arrayConst.Expressions.Type, commands.Count));
        commands.Enqueue(new SetLocalCommand(specialVariable.Id, commands.Count));

        var counter = 0;
        foreach (var elements in arrayConst.Expressions.Expressions)
        {
            commands.Enqueue(new LoadLocalAddressToStackCommand(nameOfTemp, commands.Count));
            commands.Enqueue(new LoadConstantCommand(counter, commands.Count));
            elements.Accept(this, commands);
            commands.Enqueue(new SetElementByIndex(commands.Count));

            counter++;
        }

        commands.Enqueue(new LoadLocalAddressToStackCommand(nameOfTemp, commands.Count));
    }

    // Redundant Visit
    public void VisitPrimitiveVarNode(PrimitiveVarNode primitiveVarNode, Queue<BaseCommand> commands)
    {
        throw new NotImplementedException();
    }

    public void VisitArrayVarNode(ArrayVarNode arrayVarNode, Queue<BaseCommand> commands)
    {
        // Assuming that VarNode is declared variable
        var name = arrayVarNode.Name!;
        var isArgument = false;

        var codeGenVar = ScopeStack.GetVariable(name);
        if (codeGenVar is null)
        {
            codeGenVar = ScopeStack.GetArgumentInLastScope(name);
            isArgument = true;
        }

        if (codeGenVar is null) throw new Exception("Variable is not declared");

        if (isArgument) commands.Enqueue(new LoadFunctionArgument(codeGenVar.Id, commands.Count));
        else commands.Enqueue(new LoadLocalCommand(codeGenVar.Id, commands.Count));
    }

    public void VisitStructVarNode(StructVarNode structVarNode, Queue<BaseCommand> commands)
    {
        // Assuming that VarNode is declared variable
        var name = structVarNode.Name!;
        var isArgument = false;

        var codeGenVar = ScopeStack.GetVariable(name);
        if (codeGenVar is null)
        {
            codeGenVar = ScopeStack.GetArgumentInLastScope(name);
            isArgument = true;
        }

        if (codeGenVar is null) throw new Exception("Variable is not declared");

        if (isArgument) commands.Enqueue(new LoadFunctionArgument(codeGenVar.Id, commands.Count));
        else commands.Enqueue(new LoadLocalCommand(codeGenVar.Id, commands.Count));
    }

    public void VisitArrayFunctions(ArrayFunctions arrayFunctions, Queue<BaseCommand> commands)
    {
        throw new NotImplementedException();
    }

    public void VisitEmptyReturnNode(EmptyReturnNode emptyReturnNode, Queue<BaseCommand> commands)
    {
        commands.Enqueue(new ReturnCommand(commands.Count));
    }

    public void VisitVarNode(VarNode varNode, Queue<BaseCommand> commands)
    {
        // Assuming that VarNode is declared variable
        var name = varNode.Name!;
        var isArgument = false;

        var codeGenVar = ScopeStack.GetVariable(name);
        if (codeGenVar is null)
        {
            codeGenVar = ScopeStack.GetArgumentInLastScope(name);
            isArgument = true;
        }

        if (codeGenVar is null) throw new Exception("Variable is not declared");

        if (isArgument) commands.Enqueue(new LoadFunctionArgument(codeGenVar.Id, commands.Count));
        else commands.Enqueue(new LoadLocalCommand(codeGenVar.Id, commands.Count));
    }

    public void VisitStructFieldNode(VarNode varNode, Queue<BaseCommand> commands)
    {
        if (varNode.Name == null) throw new Exception("Not found field name of variable during struct initialization");
        ScopeStack.AddVariableInLastScope(varNode.Name, varNode.Type);
        if (varNode.Value != null)
        {
            if (!(varNode.Value is ValueNode || varNode.Value.GetType().IsSubclassOf(typeof(ValueNode))))
                throw new Exception("Found var node value with incorrect type during struct initialization");
            commands.Enqueue(new LoadFunctionArgument(0, commands.Count));
            ((ValueNode)varNode.Value).Accept(this, commands);
            commands.Enqueue(new StoreStructField(commands.Count));
        }
    }
}