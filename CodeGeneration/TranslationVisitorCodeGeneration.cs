﻿using NCCompiler_CompilersCourse.Semantics;

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
        // Loading should be performed in Array Variable
        // load Array to stack
        //      if from struct:
        //          getField from struct
        //      if just array:
        //          get from variable

        // Assuming that Array on top of the stack

        // call sort function
        // result is written by address (No need to specify address)

        // TODO! Recursive call for Array

        string type = ((ArrayTypeNode)sortedArrayNode.Type).ElementTypeNode switch
        {
            var elem => elem.MyType switch
            {
                MyType.Integer => "int32",
                MyType.Real => "float32",
                MyType.Boolean => "bool",
                _ => throw new Exception($"Unexpected type for sorting: {elem.GetType()}"),
            }
        };
        commands.Enqueue(new CallCommand($"void [System.Runtime]System.Array::Sort<{type}>(!!0/*{type}*/[])"));
    }

    public void VisitArraySizeNode(ArraySizeNode arraySizeNode, Queue<BaseCommand> commands)
    {
        throw new NotImplementedException();
    }

    public void VisitReversedArrayNode(ReversedArrayNode reversedArrayNode, Queue<BaseCommand> commands)
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
        // result is written by address (No need to specify address)

        // TODO! Recursive call for Array + Copying array

        reversedArrayNode.Array.Accept(this, commands);

        string type = ((ArrayTypeNode)reversedArrayNode.Type).ElementTypeNode switch
        {
            var elem => elem.MyType switch
            {
                MyType.Integer => "int32",
                MyType.Real => "float32",
                MyType.Boolean => "bool",
                _ => throw new Exception($"Unexpected type for sorting: {elem.GetType()}"),
            }
        };
        commands.Enqueue(new CallCommand($"void [System.Runtime]System.Array::Reverse<{type}>(!!0/*{type}*/[])"));
    }

    public void VisitTypeVariableDeclaration(TypeVariableDeclaration typeVariableDeclaration, Queue<BaseCommand> commands)
    {
        throw new NotImplementedException();
    }

    public void VisitFullVariableDeclaration(FullVariableDeclaration fullVariableDeclaration, Queue<BaseCommand> commands)
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
        throw new NotImplementedException();
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
        rangeNode.LeftBound.Accept(this, commands);
        rangeNode.RightBound.Accept(this, commands);
    }

    public void VisitForLoopNode(ForLoopNode forLoopNode, Queue<BaseCommand> commands)
    {
        RangeNode range = forLoopNode.Range;
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
        throw new NotImplementedException();
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
        throw new NotImplementedException();
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
        // TODO Load expressions. Call by name then
        throw new NotImplementedException();
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

        ScopeStack.AddSpecialVariableInLastScope(arrayConst.Type);
        
        var nameOfTemp = ScopeStack.AddSpecialVariableInLastScope(arrayConst.Type);
        var counter = 0;

        var specialVariable = ScopeStack.GetVariable(nameOfTemp)!;
        
        // create new array
        commands.Enqueue(new LoadConstantCommand(arrayConst.Expressions.Expressions.Count));
        commands.Enqueue(new NewArrayCommand(arrayConst.Expressions.Type));
        commands.Enqueue(new SetLocalCommand(specialVariable.Id));
        
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
        if (varNode.Name == null) throw new Exception("Not found name of variable during struct initialization");
        ScopeStack.AddVariableInLastScope(varNode.Name, varNode.Type);
        if (varNode.Value != null)
        {
            
            if (!(varNode.Value is ValueNode || varNode.Value.GetType().IsSubclassOf(typeof(ValueNode))))
                throw new Exception("Found var node value with incorrect type during struct initialization");
            queue.Enqueue(new LoadArgumentFromFunction(0));
            ((ValueNode) varNode.Value).Accept(this, queue);
            queue.Enqueue(new StoreStackField());
        }
    }
}