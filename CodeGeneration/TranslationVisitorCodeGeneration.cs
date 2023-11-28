using NCCompiler_CompilersCourse.Semantics;

namespace NCCompiler_CompilersCourse.CodeGeneration;

public class TranslationVisitorCodeGeneration : IVisitorCodeGeneration
{
    public CodeGenerationScopeStack ScopeStack { get; }

    public void VisitProgramNode(ProgramNode programNode, Queue<BaseCommand> commands)
    {
        throw new NotImplementedException();
    }

    public void VisitGetFieldNode(GetFieldNode getFieldNode, Queue<BaseCommand> commands)
    {
        throw new NotImplementedException();
    }

    public void VisitGetByIndexNode(GetByIndexNode getByIndexNode, Queue<BaseCommand> commands)
    {
        throw new NotImplementedException();
    }

    public void VisitValueVariableDeclaration(ValueVariableDeclaration arraySizeNode, Queue<BaseCommand> commands)
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
        throw new NotImplementedException();
    }

    public void VisitBreakNode(BreakNode breakNode, Queue<BaseCommand> commands)
    {
        throw new NotImplementedException();
    }

    public void VisitAssertNode(AssertNode assertNode, Queue<BaseCommand> commands)
    {
        throw new NotImplementedException();
    }

    public void VisitValueReturnNode(ValueReturnNode valueReturnNode, Queue<BaseCommand> commands)
    {
        throw new NotImplementedException();
    }

    public void VisitRangeNode(RangeNode rangeNode, Queue<BaseCommand> commands)
    {
        throw new NotImplementedException();
    }

    public void VisitForLoopNode(ForLoopNode forLoopNode, Queue<BaseCommand> commands)
    {
        RangeNode range = forLoopNode.Range;
        // int left = range.LeftBound;
        // int right = range.RightBound;
    }

    public void VisitForEachLoopNode(ForEachLoopNode forEachLoopNode, Queue<BaseCommand> commands)
    {
        throw new NotImplementedException();
    }

    public void VisitWhileLoopNode(WhileLoopNode whileLoopNode, Queue<BaseCommand> commands)
    {
        throw new NotImplementedException();
    }

    public void VisitIfStatement(IfStatement ifStatement, Queue<BaseCommand> commands)
    {
        throw new NotImplementedException();
    }

    public void VisitIfElseStatement(IfElseStatement ifElseStatement, Queue<BaseCommand> commands)
    {
        throw new NotImplementedException();
    }

    public void VisitBodyNode(BodyNode bodyNode, Queue<BaseCommand> commands)
    {
        throw new NotImplementedException();
    }

    public void VisitAssignmentNode(AssignmentNode assignmentNode, Queue<BaseCommand> commands)
    {
        // ... -> ...
        
        
        // ... -> ..., address
        assignmentNode.Variable.Accept(this, commands);

        // ..., address -> ...
        // commands.Enqueue();
    }

    public void VisitTypeNode(TypeNode typeNode, Queue<BaseCommand> commands)
    {
        throw new NotImplementedException();
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
        // TODO check
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
        throw new NotImplementedException();
    }

    public void VisitRoutineCallNode(RoutineCallNode routineCallNode, Queue<BaseCommand> commands)
    {
        throw new NotImplementedException();
    }

    public void VisitExpressionsNode(ExpressionsNode expressionsNode, Queue<BaseCommand> commands)
    {
        throw new NotImplementedException();
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
        // If visited as var node in expression -> make send address of arrayVarNode to top of the stack
        // TODO: check if argument of function or localVariable

        // as value in expression
        var codeGenVariable = ScopeStack.GetVariable(arrayVarNode.Name!);
        if (codeGenVariable is null)
        {
            throw new Exception($"Cannot find variable of this name {arrayVarNode.Name}");
        }

        if (codeGenVariable.IsArgument) queue.Enqueue(new LoadArgumentFromFunction(codeGenVariable.Id));
        else queue.Enqueue(new LoadLocalCommand(codeGenVariable.Id));
    }

    public void VisitStructVarNode(StructVarNode structVarNode, Queue<BaseCommand> queue)
    {
        throw new NotImplementedException();
    }

    // Redundant Visit
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
        throw new NotImplementedException();
    }

    public void VisitStructFieldNode(VarNode varNode, Queue<BaseCommand> queue)
    {
        throw new NotImplementedException();
    }
}