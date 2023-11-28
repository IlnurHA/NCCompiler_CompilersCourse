using NCCompiler_CompilersCourse.Semantics;

namespace NCCompiler_CompilersCourse.CodeGeneration;

public class TranslationVisitorCodeGeneration : IVisitorCodeGeneration
{
    private CodeGenerationScopeStack ScopeStack { get; } = new();

    public void VisitProgramNode(ProgramNode programNode, Queue<BaseCommand> commands)
    {
        throw new NotImplementedException();
    }

    public void VisitGetFieldNode(GetFieldNode getFieldNode, Queue<BaseCommand> commands)
    {
        getFieldNode.StructVarNode.Accept(this, commands);
        commands.Enqueue(new LoadFieldCommand(getFieldNode.Type, getFieldNode.StructVarNode.Name!, getFieldNode.FieldName));
    }

    public void VisitGetByIndexNode(GetByIndexNode getByIndexNode, Queue<BaseCommand> commands)
    {
        getByIndexNode.ArrayVarNode.Accept(this, commands);
        getByIndexNode.Index.Accept(this, commands);
        commands.Enqueue(new LoadByIndexCommand(getByIndexNode.Type));
    }

    public void VisitValueVariableDeclaration(ValueVariableDeclaration valueVariableDeclaration, Queue<BaseCommand> commands)
    {
        throw new NotImplementedException();
    }

    public void VisitSortedArrayNode(SortedArrayNode sortedArrayNode, Queue<BaseCommand> commands)
    {
        // ..., arr -> ..., arr
        // + Declared special variable
        string type = _getTypeOfArrayElement(((ArrayTypeNode) sortedArrayNode.Type).ElementTypeNode);
        var (_, specialVar) = _makeCopyOfArrayAndPerformFunctionCall(
            $"void [System.Runtime]System.Array::Sort<{type}>(!!0/*{type}*/[])",
            sortedArrayNode, commands);
        // Load for return
        commands.Enqueue(new LoadLocalCommand(specialVar.Id));
    }

    public void VisitArraySizeNode(ArraySizeNode arraySizeNode, Queue<BaseCommand> commands)
    {
        // ..., arr -> ..., length
        commands.Enqueue(new ArrayLength());
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

        // TODO! Recursive call for Array + Copying array

        // ... -> ..., arr
        arrayFunctions.Array.Accept(this, commands);

        // make new temp variable
        var nameOfTemp = ScopeStack.AddSpecialVariableInLastScope(arrayFunctions.Type);
        var specialVar = ScopeStack.GetVariable(nameOfTemp)!;

        // Clone
        commands.Enqueue(new CallVirtualCommand("instance object [System.Runtime]System.Array::Clone()"));

        // Cast
        commands.Enqueue(new CastClassCommand(arrayFunctions.Type));

        // Set to special variable
        commands.Enqueue(new SetLocalCommand(specialVar.Id));

        // Load for call
        commands.Enqueue(new LoadLocalCommand(specialVar.Id));

        // Reverse func
        commands.Enqueue(new CallCommand(function));
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
        string type = _getTypeOfArrayElement(((ArrayTypeNode) reversedArrayNode.Type).ElementTypeNode);
        var (_, specialVar) = _makeCopyOfArrayAndPerformFunctionCall(
            $"void [System.Runtime]System.Array::Reverse<{type}>(!!0/*{type}*/[])",
            reversedArrayNode, commands);
        // Load for return
        commands.Enqueue(new LoadLocalCommand(specialVar.Id));
    }

    public void VisitTypeVariableDeclaration(TypeVariableDeclaration typeVariableDeclaration,
        Queue<BaseCommand> commands)
    {
        throw new NotImplementedException();
    }

    public void VisitFullVariableDeclaration(FullVariableDeclaration fullVariableDeclaration,
        Queue<BaseCommand> commands)
    {
        throw new NotImplementedException();
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
        commands.Enqueue(new JumpForBreakCommand());
    }

    public void VisitAssertNode(AssertNode assertNode, Queue<BaseCommand> commands)
    {
        assertNode.LeftExpression.Accept(this, commands);
        assertNode.RightExpression.Accept(this, commands);
        commands.Enqueue(new OperationCommand(OperationType.Eq));
        commands.Enqueue(new CallCommand("void [System.Runtime]System.Diagnostics.Debug::Assert(bool, string)"));
    }


    public void VisitValueReturnNode(ValueReturnNode valueReturnNode, Queue<BaseCommand> commands)
    {
        valueReturnNode.Value.Accept(this, commands);
        commands.Enqueue(new ReturnCommand());
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
        commands.Enqueue(new SetLocalCommand(fromId));
        int toId = ScopeStack.GetVariable(to)!.Id;
        commands.Enqueue(new SetLocalCommand(toId));
        var conditionJumper = new JumpCommand();
        commands.Enqueue(conditionJumper);
        int startBodyAddress = commands.Count();
        commands.Enqueue(new NopCommand());
        forLoopNode.Body.Accept(this, commands);
        int endBodyAddress = commands.Count() + 3;
        conditionJumper.SetAddress(endBodyAddress);
        var commandsList = commands.ToArray();
        for (int i = startBodyAddress; i < endBodyAddress - 3; ++i)
        {
            if(commandsList[i] is JumpForBreakCommand jumpForBreakCommand)
                jumpForBreakCommand.SetAddress(endBodyAddress);
        }
        commands.Enqueue(new LoadLocalCommand(fromId));
        commands.Enqueue(new LoadConstantCommand(1));
        commands.Enqueue(range.Reversed
            ? new OperationCommand(OperationType.Minus)
            : new OperationCommand(OperationType.Plus));
        commands.Enqueue(new LoadLocalCommand(toId));
        commands.Enqueue(range.Reversed
            ? new OperationCommand(OperationType.Gt)
            : new OperationCommand(OperationType.Lt));
        var beginJumper = new JumpIfTrue();
        beginJumper.SetAddress(startBodyAddress);
        commands.Enqueue(beginJumper);
    }

    public void VisitForEachLoopNode(ForEachLoopNode forEachLoopNode, Queue<BaseCommand> commands)
    {
        string identifier = forEachLoopNode.IdName.Name!;
        var type = forEachLoopNode.Array.Type;
        string to = ScopeStack.AddSpecialVariableInLastScope(new TypeNode(myType: MyType.Integer));
        throw new NotImplementedException();
    }

    public void VisitWhileLoopNode(WhileLoopNode whileLoopNode, Queue<BaseCommand> commands)
    {
        var conditionJumper = new JumpCommand();
        commands.Enqueue(conditionJumper);
        int startBodyAddress = commands.Count();
        commands.Enqueue(new NopCommand());
        whileLoopNode.Body.Accept(this, commands);
        int endBodyAddress = commands.Count();
        conditionJumper.SetAddress(endBodyAddress);
        var commandsList = commands.ToArray();
        for (int i = startBodyAddress; i < endBodyAddress; ++i)
        {
            if(commandsList[i] is JumpForBreakCommand jumpForBreakCommand)
                jumpForBreakCommand.SetAddress(endBodyAddress);
        }
        whileLoopNode.Condition.Accept(this, commands);
        var beginJumper = new JumpIfTrue();
        beginJumper.SetAddress(startBodyAddress);
        commands.Enqueue(beginJumper);
    }

    public void VisitIfStatement(IfStatement ifStatement, Queue<BaseCommand> commands)
    {
        // ... -> ...
        
        // ... -> ..., bool
        ifStatement.Condition.Accept(this, commands);
        
        // ..., bool -> ...
        var jump = new JumpIfFalse();
        commands.Enqueue(jump);
        ifStatement.Body.Accept(this, commands);
        commands.Enqueue(new NopCommand());
        jump.SetAddress(commands.Count);
    }

    public void VisitIfElseStatement(IfElseStatement ifElseStatement, Queue<BaseCommand> commands)
    {
        // ... -> ...
        
        // ... -> ..., bool
        ifElseStatement.Condition.Accept(this, commands);
        
        var jumpToElse = new JumpIfFalse();
        var jumpToEnd = new JumpCommand();
        
        // ..., bool -> ...
        commands.Enqueue(jumpToElse);
        ifElseStatement.Body.Accept(this, commands);
        
        commands.Enqueue(jumpToEnd);
        commands.Enqueue(new NopCommand());
        jumpToElse.SetAddress(commands.Count);
        
        ifElseStatement.BodyElse.Accept(this, commands);
        commands.Enqueue(new NopCommand());
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
                commands.Enqueue(new SetFieldCommand(value.Type, getFieldNode.StructVarNode.Name!, getFieldNode.FieldName));
                break;
            case GetByIndexNode getByIndexNode:
                getByIndexNode.Accept(this, commands);
            
                // Loaded index to stack
                getByIndexNode.Index.Accept(this, commands);
            
                // loaded value to assign to stack
                value.Accept(this, commands);
            
                // Setting element by index
                commands.Enqueue(new SetElementByIndex());
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
                if (isArgument) commands.Enqueue(new SetArgumentByNameCommand(codeGenVar.GetName()));
                else commands.Enqueue(new SetLocalCommand(codeGenVar.Id));
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
        throw new NotImplementedException();
    }

    public void VisitCastNode(CastNode castNode, Queue<BaseCommand> commands)
    {
        // ... -> ..., value
        ((ValueNode) castNode.Value!).Accept(this, commands);
        
        // Casting Value
        // ..., value -> ..., castedValue
        commands.Enqueue(new PrimitiveCastCommand(castNode.Type));
    }

    public void VisitRoutineDeclarationNode(RoutineDeclarationNode routineDeclarationNode, Queue<BaseCommand> commands)
    {
        throw new NotImplementedException();
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
            $" Program::{routineCallNode.Routine.Name}({string.Join(" ", types.ToArray())})"));
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

        commands.Enqueue(new LoadConstantCommand(constNode.Value!));
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

        foreach (var command in new OperationCommand(operationNode.OperationType).GetOperations())
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
        commands.Enqueue(new LoadConstantCommand(arrayConst.Expressions.Expressions.Count));
        commands.Enqueue(new NewArrayCommand(arrayConst.Expressions.Type));
        commands.Enqueue(new SetLocalCommand(specialVariable.Id));

        var counter = 0;
        foreach (var elements in arrayConst.Expressions.Expressions)
        {
            commands.Enqueue(new LoadLocalAddressToStackCommand(nameOfTemp));
            commands.Enqueue(new LoadConstantCommand(counter));
            elements.Accept(this, commands);
            commands.Enqueue(new SetElementByIndex());

            counter++;
        }

        commands.Enqueue(new LoadLocalAddressToStackCommand(nameOfTemp));
    }

    // Redundant Visit
    public void VisitPrimitiveVarNode(PrimitiveVarNode primitiveVarNode, Queue<BaseCommand> commands)
    {
        throw new NotImplementedException();
    }

    public void VisitArrayVarNode(ArrayVarNode arrayVarNode, Queue<BaseCommand> queue)
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
        
        if (isArgument) queue.Enqueue(new LoadArgumentFromFunction(codeGenVar.Id));
        else queue.Enqueue(new LoadLocalCommand(codeGenVar.Id));
    }

    public void VisitStructVarNode(StructVarNode structVarNode, Queue<BaseCommand> queue)
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
        
        if (isArgument) queue.Enqueue(new LoadArgumentFromFunction(codeGenVar.Id));
        else queue.Enqueue(new LoadLocalCommand(codeGenVar.Id));
    }

    public void VisitArrayFunctions(ArrayFunctions arrayFunctions, Queue<BaseCommand> queue)
    {
        throw new NotImplementedException();
    }

    public void VisitEmptyReturnNode(EmptyReturnNode emptyReturnNode, Queue<BaseCommand> queue)
    {
        queue.Enqueue(new ReturnCommand());
    }

    public void VisitVarNode(VarNode varNode, Queue<BaseCommand> queue)
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
        
        if (isArgument) queue.Enqueue(new LoadArgumentFromFunction(codeGenVar.Id));
        else queue.Enqueue(new LoadLocalCommand(codeGenVar.Id));
    }

    public void VisitStructFieldNode(VarNode varNode, Queue<BaseCommand> queue)
    {
        if (varNode.Name == null) throw new Exception("Not found name of variable during struct initialization");
        ScopeStack.AddVariableInLastScope(varNode.Name, varNode.Type);
        if (varNode.Value != null)
        {
            if (!(varNode.Value is ValueNode || varNode.Value.GetType().IsSubclassOf(typeof(ValueNode))))
                throw new Exception("Found var node value with incorrect type during struct initialization");
            queue.Enqueue(new LoadArgumentFromFunction(0));
            ((ValueNode)varNode.Value).Accept(this, queue);
            queue.Enqueue(new StoreStructField());
        }
    }
}