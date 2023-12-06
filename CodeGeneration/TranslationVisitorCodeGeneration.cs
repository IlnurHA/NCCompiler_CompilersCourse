using System.Diagnostics;
using NCCompiler_CompilersCourse.Semantics;

namespace NCCompiler_CompilersCourse.CodeGeneration;

public class TranslationVisitorCodeGeneration : IVisitorCodeGeneration
{
    private CodeGenerationScopeStack ScopeStack { get; } = new();

    private List<string> _structDeclarations = new();
    private List<string> _routinesCode = new();
    public string ResultingProgram = "";

    public void VisitProgramNode(ProgramNode programNode, Queue<BaseCommand> commands)
    {
        var programString = "";

        // dependencies
        programString += ".assembly extern System.Runtime\n{\n\t.ver 7:0:0:0\n}\n";
        programString += ".assembly compiledProgram{}\n";
        programString += ".module compiledProgram.dll\n";

        var mainClassName = "Program";

        programString +=
            $".class private auto {mainClassName}";

        foreach (var declaration in programNode.Declarations)
        {
            if (declaration is RoutineDeclarationNode)
            {
                declaration.Accept(this, new Queue<BaseCommand>());
                continue;
            }

            if (declaration is null) continue;
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

        programString += "\t.method public hidebysig specialname rtspecialname instance void .ctor() cil managed";
        programString += " {\n";
        programString += $"\t\t.maxstack {maxStack}\n";

        foreach (var command in commands)
        {
            programString += "\t\t" + command.Translate() + '\n';
        }

        programString += "\t\t" + new ReturnCommand(commands.Count).Translate() + "\n";

        programString += "\t}\n";

        // End of main class
        programString += "}\n";
        ResultingProgram = programString;
    }

    public void VisitGetFieldNode(GetFieldNode getFieldNode, Queue<BaseCommand> commands)
    {
        getFieldNode.StructVarNode.Accept(this, commands);
        var structName = ScopeStack.GetByStructType((StructTypeNode) getFieldNode.StructVarNode.Type.GetFinalTypeNode())!;
        commands.Enqueue(new LoadFieldCommand(_getTypeFromTypeNode(getFieldNode.Type),
            $"Program/{structName.GetName()}",
            getFieldNode.FieldName, commands.Count));
    }

    public void VisitSetFieldNode(GetFieldNode getFieldNode, Queue<BaseCommand> commands)
    {
        getFieldNode.StructVarNode.Accept(this, commands);
    }

    public void VisitGetByIndexNode(GetByIndexNode getByIndexNode, Queue<BaseCommand> commands)
    {
        // ... -> ..., array
        // pushing array to stack
        switch (getByIndexNode.ArrayVarNode)
        {
            case ArrayVarNode arrayVarNode:
                arrayVarNode.AcceptByValue(this, commands);
                break;
            case GetFieldNode getFieldNode:
                getFieldNode.Accept(this, commands);
                break;
            case GetByIndexNode subGetByIndexNode:
                subGetByIndexNode.Accept(this, commands);
                break;
            default:
                throw new Exception($"Unexpected type: {getByIndexNode.ArrayVarNode.GetType()}");
        }

        // ..., array -> ..., array, index
        // pushing index to stack
        getByIndexNode.Index.Accept(this, commands);
        // But we need to decrease it by one (because in our language indexes are starting from 1)
        commands.Enqueue(new LoadConstantCommand(1, commands.Count));
        commands.Enqueue(new OperationCommand(OperationType.Minus, commands.Count));


        // ..., array, index -> ..., value
        // Getting from array and index then push value to stack
        if (getByIndexNode.Type is ArrayTypeNode) commands.Enqueue(new LoadFromArrayRefCommand(commands.Count)); 
        else commands.Enqueue(new LoadByIndexCommand(_getTypeFromTypeNode(getByIndexNode.Type), commands.Count));
    }

    public void VisitSetByIndex(GetByIndexNode getByIndexNode, Queue<BaseCommand> commands)
    {
        // ... -> ..., array
        // pushing array to stack
        switch (getByIndexNode.ArrayVarNode)
        {
            case ArrayVarNode arrayVarNode:
                arrayVarNode.AcceptByValue(this, commands);
                break;
            case GetFieldNode getFieldNode:
                getFieldNode.Accept(this, commands);
                break;
            case GetByIndexNode subGetByIndexNode:
                subGetByIndexNode.Accept(this, commands);
                break;
            default:
                throw new Exception($"Unexpected type: {getByIndexNode.ArrayVarNode.GetType()}");
        }
        // ..., array -> ..., array, index
        // pushing index to stack
        getByIndexNode.Index.Accept(this, commands);
        // But we need to decrease it by one (because in our language indexes are starting from 1)
        commands.Enqueue(new LoadConstantCommand(1, commands.Count));
        commands.Enqueue(new OperationCommand(OperationType.Minus, commands.Count));
    }

    private (string, TypeNode) _createNewVar(VarNode varNode, TypeNode typeNode)
    {
        var name = varNode.Name!;
        ScopeStack.AddVariableInLastScope(name, typeNode);
        return (name, typeNode);
    }

    private void _handleDeclaration(DeclarationNode declarationNode, Queue<BaseCommand> commands)
    {
        if (declarationNode.Variable.Type is StructTypeNode) declarationNode.Variable.Type.Accept(this, commands);
        var (name, _) = _createNewVar(declarationNode.Variable, declarationNode.Variable.Type);
        var codeGenVar = ScopeStack.GetVariable(name)!;

        if (declarationNode is TypeVariableDeclaration)
        {
            if (declarationNode.Variable.Type is StructTypeNode structType)
            {
                var structVar = ScopeStack.GetByStructType(structType)!;


                commands.Enqueue(new LoadLocalAddressToStackCommand(codeGenVar.Id, codeGenVar.GetName(), commands.Count));
                commands.Enqueue(new InitObjectCommand($"Program/{structVar.GetName()}", commands.Count));

                // Putting default values if any
                foreach (var (_, varNode) in structType.DefaultValues)
                {
                    if (varNode.Value is not ValueNode valueNode) break;
                    commands.Enqueue(new LoadLocalCommand(codeGenVar.Id, codeGenVar.GetName(), commands.Count));
                    valueNode.Accept(this, commands);
                    commands.Enqueue(new SetFieldCommand(_getTypeFromTypeNode(valueNode.Type),
                        $"Program/{structVar.GetName()}", varNode.Name!, commands.Count));
                }

                // commands.Enqueue(new LoadLocalCommand(codeGenVar.Id, codeGenVar.GetName(), commands.Count));
                // if (withDefaultValue)
                //     commands.Enqueue(new CallCommand($"instance void Program/{structVar.GetName()}::.ctor()",
                //         commands.Count));
                // else
                //     commands.Enqueue(new InitObjectCommand($"Program/{structVar.GetName()}", commands.Count));
                return;
            }

            if (declarationNode.Variable.Type is ArrayTypeNode arrayTypeNode)
            {
                arrayTypeNode.Size!.Accept(this, commands);
                commands.Enqueue(new NewArrayCommand(_getTypeFromTypeNode(arrayTypeNode.ElementTypeNode), commands.Count));
            }
            else
            {
                return;
            }
            
            
        }
        else
        {
            // getting value on top of the stack
            if (declarationNode.DeclarationValue is ArrayVarNode arrayVarNode) arrayVarNode.AcceptByValue(this, commands);
            if (declarationNode.DeclarationValue is StructVarNode structVarNode) structVarNode.AcceptByValue(this, commands);
            else declarationNode.DeclarationValue.Accept(this, commands);
        }

        // setting value to variable
        commands.Enqueue(new SetLocalCommand(codeGenVar.Id, codeGenVar.GetName(), commands.Count));
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
        string type = _getTypeOfArrayElement(((ArrayTypeNode) sortedArrayNode.Type).ElementTypeNode);
        var (_, specialVar) = _makeCopyOfArrayAndPerformFunctionCall(
            $"void [System.Runtime]System.Array::Sort<{type}>(!!0/*{type}*/[])",
            sortedArrayNode, commands);
        // Load for return
        commands.Enqueue(new LoadLocalCommand(specialVar.Id, specialVar.GetName(), commands.Count));
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
        commands.Enqueue(
            new CallVirtualCommand("instance object [System.Runtime]System.Array::Clone()", commands.Count));

        // Cast
        commands.Enqueue(new CastClassCommand(_getTypeFromTypeNode(arrayFunctions.Type), commands.Count));

        // Set to special variable
        commands.Enqueue(new SetLocalCommand(specialVar.Id, specialVar.GetName(), commands.Count));

        // Load for call
        commands.Enqueue(new LoadLocalCommand(specialVar.Id, specialVar.GetName(), commands.Count));

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
        string type = _getTypeOfArrayElement(((ArrayTypeNode) reversedArrayNode.Type).ElementTypeNode);
        var (_, specialVar) = _makeCopyOfArrayAndPerformFunctionCall(
            $"void [System.Runtime]System.Array::Reverse<{type}>(!!0/*{type}*/[])",
            reversedArrayNode, commands);
        // Load for return
        commands.Enqueue(new LoadLocalCommand(specialVar.Id, specialVar.GetName(), commands.Count));
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
        commands.Enqueue(new LoadStringCommand("Assertion error", commands.Count));
        commands.Enqueue(new CallCommand("void [System.Runtime]System.Diagnostics.Debug::Assert(bool, string)",
            commands.Count));
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
        // Pushing Range boundaries to top of the stack 
        // ... -> ..., rightBoundary, leftBoundary
        RangeNode range = forLoopNode.Range;
        range.Accept(this, commands);

        // Loading range boundaries to variables
        string from = ScopeStack.AddSpecialVariableInLastScope(new TypeNode(MyType.Integer));
        string to = ScopeStack.AddSpecialVariableInLastScope(new TypeNode(MyType.Integer));

        if (range.Reversed) (from, to) = (to, from);

        var fromVar = ScopeStack.GetVariable(from)!;
        var toVar = ScopeStack.GetVariable(to)!;

        // Adding identifier to local scope
        var identifier = forLoopNode.IdName.Name!;
        ScopeStack.AddVariableInLastScope(identifier, new TypeNode(MyType.Integer));
        var identifierVar = ScopeStack.GetVariable(identifier)!;

        // Setting left and right boundaries
        commands.Enqueue(new SetLocalCommand(fromVar.Id, fromVar.GetName(), commands.Count));
        commands.Enqueue(new SetLocalCommand(toVar.Id, toVar.GetName(), commands.Count));

        // initializing identifier
        commands.Enqueue(new LoadLocalCommand(fromVar.Id, fromVar.GetName(), commands.Count));
        commands.Enqueue(new SetLocalCommand(identifierVar.Id, identifierVar.GetName(), commands.Count));

        // Jump to condition
        var conditionJumper = new JumpCommand(commands.Count);
        commands.Enqueue(conditionJumper);

        int startBodyAddress = commands.Count;
        commands.Enqueue(new NopCommand(commands.Count));

        // Body commands
        forLoopNode.Body.Accept(this, commands);

        // Address to jump depending on condition
        int endBodyAddress = commands.Count;
        commands.Enqueue(new NopCommand(commands.Count)); // End of body

        // Updating identifier
        commands.Enqueue(new LoadLocalCommand(identifierVar.Id, identifierVar.GetName(), commands.Count));
        commands.Enqueue(new LoadConstantCommand(1, commands.Count));
        commands.Enqueue(range.Reversed
            ? new OperationCommand(OperationType.Minus, commands.Count)
            : new OperationCommand(OperationType.Plus, commands.Count));
        commands.Enqueue(new SetLocalCommand(identifierVar.Id, identifierVar.GetName(), commands.Count));

        conditionJumper.SetAddress(commands.Count);
        commands.Enqueue(new NopCommand(commands.Count));

        // Checking condition
        commands.Enqueue(new LoadLocalCommand(identifierVar.Id, identifierVar.GetName(), commands.Count));
        commands.Enqueue(new LoadLocalCommand(toVar.Id, toVar.GetName(), commands.Count));
        foreach (var command in range.Reversed
                     ? new OperationCommand(OperationType.Ge, commands.Count).GetOperations()
                     : new OperationCommand(OperationType.Le, commands.Count).GetOperations())
        {
            commands.Enqueue(command);
        }

        // Jump if true
        var beginJumper = new JumpIfTrue(commands.Count);
        beginJumper.SetAddress(startBodyAddress);
        commands.Enqueue(beginJumper);

        // Updating jump addresses for breaks
        var toExitLoopIndex = commands.Count;
        var commandsList = commands.ToArray();
        for (int i = startBodyAddress; i < endBodyAddress - 3; ++i)
        {
            if (commandsList[i] is JumpForBreakCommand {Address: -1} jumpForBreakCommand)
                jumpForBreakCommand.SetAddress(toExitLoopIndex);
        }

        commands.Enqueue(new NopCommand(commands.Count));
    }

    public void VisitForEachLoopNode(ForEachLoopNode forEachLoopNode, Queue<BaseCommand> commands)
    {
        // new identifier for element from array
        string identifier = forEachLoopNode.IdName.Name!;

        // type of identifier
        var type = forEachLoopNode.Array.Type.ElementTypeNode;

        ScopeStack.AddVariableInLastScope(identifier, type);
        var identifierVar = ScopeStack.GetVariable(identifier)!;

        // counter for array
        var counter = ScopeStack.AddSpecialVariableInLastScope(new TypeNode(MyType.Integer));
        var counterVar = ScopeStack.GetVariable(counter)!;

        commands.Enqueue(new LoadConstantCommand(0, commands.Count));
        commands.Enqueue(new SetLocalCommand(counterVar.Id, counterVar.GetName(), commands.Count));

        string array = ScopeStack.AddSpecialVariableInLastScope(forEachLoopNode.Array.Type);
        var arrayVar = ScopeStack.GetVariable(array)!;
        forEachLoopNode.Array.Accept(this, commands);
        commands.Enqueue(new SetLocalCommand(arrayVar.Id, arrayVar.GetName(), commands.Count));

        // Size of array
        var size = ScopeStack.AddSpecialVariableInLastScope(new TypeNode(MyType.Integer));
        var sizeVar = ScopeStack.GetVariable(size)!;

        // Loading array from local command, Getting length of array, Setting size to identifier
        commands.Enqueue(new LoadLocalCommand(arrayVar.Id, arrayVar.GetName(), commands.Count));
        commands.Enqueue(new ArrayLength(commands.Count));
        commands.Enqueue(new SetLocalCommand(sizeVar.Id, sizeVar.GetName(), commands.Count));


        var comparisonAddress = commands.Count;
        // Comparison to exit from loop
        commands.Enqueue(new LoadLocalCommand(counterVar.Id, counterVar.GetName(), commands.Count));
        commands.Enqueue(new LoadLocalCommand(sizeVar.Id, sizeVar.GetName(), commands.Count));
        commands.Enqueue(new OperationCommand(OperationType.Lt, commands.Count));

        // Jump to exit loop
        var jumperAfter = new JumpIfFalse(commands.Count);
        commands.Enqueue(jumperAfter);

        // Loading element from array and setting it to identifier
        commands.Enqueue(new LoadLocalCommand(arrayVar.Id, arrayVar.GetName(), commands.Count));
        commands.Enqueue(new LoadLocalCommand(counterVar.Id, counterVar.GetName(), commands.Count));
        commands.Enqueue(new LoadByIndexCommand(_getTypeFromTypeNode(type), commands.Count));
        commands.Enqueue(new SetLocalCommand(identifierVar.Id, identifierVar.GetName(), commands.Count));
        commands.Enqueue(new NopCommand(commands.Count));

        // body commands
        forEachLoopNode.Body.Accept(this, commands);

        var afterBodyAddress = commands.Count;

        // Increment counter by 1
        commands.Enqueue(new LoadLocalCommand(counterVar.Id, counterVar.GetName(), commands.Count));
        commands.Enqueue(new LoadConstantCommand(1, commands.Count));
        commands.Enqueue(new OperationCommand(OperationType.Plus, commands.Count));
        commands.Enqueue(new SetLocalCommand(counterVar.Id, counterVar.GetName(), commands.Count));

        // Jump to comparison
        commands.Enqueue(new JumpCommand(commands.Count) {Address = comparisonAddress});

        // Jump to the end of for loop
        jumperAfter.SetAddress(commands.Count);

        // setting addresses for breaks inside for-loop
        var commandsList = commands.ToArray();
        for (int i = comparisonAddress; i < afterBodyAddress; ++i)
        {
            if (commandsList[i] is JumpForBreakCommand {Address: -1} jumpForBreakCommand)
                jumpForBreakCommand.SetAddress(jumperAfter.Address);
        }

        commands.Enqueue(new NopCommand(commands.Count));
    }

    public void VisitWhileLoopNode(WhileLoopNode whileLoopNode, Queue<BaseCommand> commands)
    {
        // Jump to condition
        var conditionJumper = new JumpCommand(commands.Count);
        commands.Enqueue(conditionJumper);

        // Start body address
        int startBodyAddress = commands.Count;
        commands.Enqueue(new NopCommand(commands.Count));

        // Body commands
        whileLoopNode.Body.Accept(this, commands);

        // End body address
        int endBodyAddress = commands.Count;

        conditionJumper.SetAddress(endBodyAddress);
        commands.Enqueue(new NopCommand(commands.Count));

        // Condition commands
        whileLoopNode.Condition.Accept(this, commands);

        // Jump to body
        var beginJumper = new JumpIfTrue(commands.Count);
        beginJumper.SetAddress(startBodyAddress);
        commands.Enqueue(beginJumper);

        // Jump after body
        var exitAddress = commands.Count;
        commands.Enqueue(new NopCommand(exitAddress));

        var commandsList = commands.ToArray();
        for (int i = startBodyAddress; i < endBodyAddress; ++i)
        {
            if (commandsList[i] is JumpForBreakCommand {Address: -1} jumpForBreakCommand)
                jumpForBreakCommand.SetAddress(exitAddress);
        }
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

        jump.SetAddress(commands.Count);
        commands.Enqueue(new NopCommand(commands.Count));
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

        jumpToElse.SetAddress(commands.Count);
        commands.Enqueue(new NopCommand(commands.Count));

        ifElseStatement.BodyElse.Accept(this, commands);
        jumpToEnd.SetAddress(commands.Count);
        commands.Enqueue(new NopCommand(commands.Count));
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

        var value = assignmentNode.AssignmentValue;

        switch (assignmentNode.Variable)
        {
            case GetFieldNode getFieldNode:
                // Getting object from field of struct
                getFieldNode.AcceptSetField(this, commands);

                switch (value)
                {
                    // Pushing value to top of stack
                    case ArrayVarNode arrayVarNode:
                        arrayVarNode.AcceptByValue(this, commands);
                        break;
                    case StructVarNode structVarNode:
                        structVarNode.AcceptByValue(this, commands);
                        break;
                    default:
                        value.Accept(this, commands);
                        break;
                }

                // Setting to field command
                var structName = ScopeStack.GetByStructType((StructTypeNode) getFieldNode.StructVarNode.Type)!;
                commands.Enqueue(new SetFieldCommand(_getTypeFromTypeNode(value.Type),
                    $"Program/{structName.GetName()}",
                    getFieldNode.FieldName, commands.Count));
                break;
            case GetByIndexNode getByIndexNode:
                // Loading value (from array) to top of the stack
                getByIndexNode.AcceptSetByIndex(this, commands);

                switch (value)
                {
                    // Value to assign to top of stack
                    case ArrayVarNode arrayVarNodeGetByIndex:
                        arrayVarNodeGetByIndex.AcceptByValue(this, commands);
                        break;
                    case StructVarNode structVarNodeGetByIndex:
                        structVarNodeGetByIndex.AcceptByValue(this, commands);
                        break;
                    default:
                        value.Accept(this, commands);
                        break;
                }

                // Setting element by index
                if (getByIndexNode.Type is ArrayTypeNode) commands.Enqueue(new SetElementByIndexRef(commands.Count));
                else commands.Enqueue(new SetElementByIndex(_getTypeFromTypeNode(value.Type), commands.Count));
                break;
            case VarNode varNode:
                switch (value)
                {
                    // Pushing to value to stack
                    case ArrayVarNode arrayVar:
                        arrayVar.AcceptByValue(this, commands);
                        break;
                    case StructVarNode structVar:
                        structVar.AcceptByValue(this, commands);
                        break;
                    default:
                        value.Accept(this, commands);
                        break;
                }

                // Getting name to set value
                var name = varNode.Name!;
                var isArgument = false;
                var codeGenVar = ScopeStack.GetVariable(name);
                if (codeGenVar is null)
                {
                    codeGenVar = ScopeStack.GetArgumentInLastScope(name);
                    isArgument = true;
                }

                if (codeGenVar is null) throw new Exception("Cannot assign to undeclared variable");

                // Setting value to variable
                if (isArgument) commands.Enqueue(new SetArgumentByNameCommand(codeGenVar.GetName(), commands.Count));
                else commands.Enqueue(new SetLocalCommand(codeGenVar.Id, codeGenVar.GetName(), commands.Count));
                break;
            default:
                throw new Exception($"Unhandled node type: {assignmentNode.Variable}");
        }
    }

    public void VisitArrayTypeNode(ArrayTypeNode arrayTypeNode, Queue<BaseCommand> commands)
    {
        arrayTypeNode.ElementTypeNode.Accept(this, commands);
    }

    public void VisitTypeNode(TypeNode typeNode, Queue<BaseCommand> commands)
    {
    }

    public void VisitStructTypeNode(StructTypeNode structTypeNode, Queue<BaseCommand> commands)
    {
        if (ScopeStack.GetByStructType(structTypeNode) is not null) return;
        var structCounter = ScopeStack.GetStructCounter();
        var structName = "STRUCT_TYPE_" + structCounter;

        ScopeStack.GetGlobalScope().AddStruct(structName, structTypeNode, structCounter);

        var structDeclaration =
            $"\t.class nested public sealed sequential ansi beforefieldinit {structName} extends [System.Runtime]System.ValueType";

        // fields
        structDeclaration += "{\n";

        foreach (var (name, type) in structTypeNode.StructFields)
        {
            type.Accept(this, commands);
            structDeclaration += $"\t\t.field {_getTypeFromTypeNode(type)} {name}\n";
        }

        /*
        // Default values
        var commandCounter = 0;
        var constructorString = "";
        
        foreach (var (key, varNode) in structTypeNode.DefaultValues)
        {
            varNode.Type.Accept(this, commands);
            if (varNode.Value is null) continue;
            constructorString += "\t\t" + new LoadFunctionArgument(0, commandCounter).Translate() + "\n";
            commandCounter++;
            if (varNode.Type is StructTypeNode structType)
            {
                var subStructVar = ScopeStack.GetByStructType(structType)!;
                constructorString += "\t\t" + new NewObjectCommand($"Program/{subStructVar.GetName()}", commandCounter).Translate() + "\n";
                commandCounter++;
            } else
            {
                var tempCommand = new Queue<BaseCommand>();
                ((ValueNode) varNode.Value!).Accept(this, tempCommand);
                foreach (var command in tempCommand)
                {
                    command.CommandIndex = commandCounter;
                    constructorString += "\t\t" + command.Translate() + "\n";
                    commandCounter++;
                }
            }

            constructorString += "\t\t" + new SetFieldCommand(_getTypeFromTypeNode(varNode.Type), $"Program/{structName}", key, commandCounter).Translate() + "\n";
            commandCounter++;
        }
        if (constructorString != "")
            structDeclaration += "\n\t.method public hidebysig specialname rtspecialname instance void" +
                                 " \n\t\t.ctor() cil managed \n\t{\n" +
                                 "\t\t.maxstack 50\n" +
                                 $"{constructorString}" +
                                 $"\t\t{new ReturnCommand(commandCounter).Translate()}\n" +
                                 "\n\t}\n";
        */
        structDeclaration += "\t}\n";

        _structDeclarations.Add(structDeclaration);
    }

    public void VisitCastNode(CastNode castNode, Queue<BaseCommand> commands)
    {
        // ... -> ..., value
        ((ValueNode) castNode.Value!).Accept(this, commands);

        // Casting Value
        // ..., value -> ..., castedValue
        commands.Enqueue(new PrimitiveCastCommand(castNode.Type, commands.Count));
    }

    private string _getTypeFromTypeNode(TypeNode typeNode)
    {
        if (typeNode is StructTypeNode structTypeCheck) structTypeCheck.Accept(this, new Queue<BaseCommand>());
        return typeNode switch
        {
            UserDefinedTypeNode userDefinedTypeNode => _getTypeFromTypeNode(userDefinedTypeNode.Type),
            StructTypeNode structTypeNode =>
                $"valuetype Program/{ScopeStack.GetByStructType(structTypeNode)!.GetName()}",
            ArrayTypeNode arrayTypeNode => _getTypeFromTypeNode(arrayTypeNode.ElementTypeNode) + "[]",
            { } node => node.MyType switch
            {
                MyType.Integer => "int32",
                MyType.Boolean => "bool",
                MyType.Real => "float32",
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
        var typeStr = _getTypeFromTypeNode(routineDeclarationNode.ReturnType!);
        var routineName = routineDeclarationNode.FunctionName.Name;
        var isMain = routineName == "main";

        string routineCode = "";
        if (isMain) routineCode += $"\t.method public static hidebysig {typeStr} {routineName}";
        else routineCode += $"\t.method public static hidebysig {typeStr} {routineName}";

        ScopeStack.CreateNewScope(SemanticsScope.ScopeContext.Routine);

        if (routineDeclarationNode.Parameters != null)
            routineDeclarationNode.Parameters.Accept(this, commands);


        var lastScope = ScopeStack.GetLastScope();

        // Format function arguments
        routineCode += "(";
        var counter = 0;
        var len = lastScope.Arguments.Count;
        foreach (var (name, codeGenerationVariable) in lastScope.Arguments)
        {
            routineCode += "\n\t\t" + _getTypeFromTypeNode(codeGenerationVariable.Type) + ' ' + $"'{name}'";
            if (counter < len - 1) routineCode += ", ";
            else routineCode += "\n\t\t";
            counter++;
        }

        routineCode += ")";

        // Cil managed
        routineCode += " cil managed ";

        routineDeclarationNode.Body.Accept(this, commands);

        // function data
        routineCode += "{\n";

        // entry point
        if (isMain) routineCode += "\t\t.entrypoint\n";

        // MaxStack
        int maxStack = 50;

        routineCode += $"\t\t.maxstack {maxStack}\n";

        // locals
        if (lastScope.LocalVariables.Count != 0)
        {
            routineCode += "\t\t.locals init (\n";

            var sortedLocals = new List<CodeGenerationVariable>(lastScope.LocalVariables.Count);

            for (int i = 0; i < lastScope.LocalVariables.Count; i++)
            {
                foreach (var codeGenerationVariable in lastScope.LocalVariables.Values)
                {
                    if (i == codeGenerationVariable.Id) sortedLocals.Add(codeGenerationVariable);
                }
            }

            len = lastScope.LocalVariables.Count;

            for (int i = 0; i < len; i++)
            {
                var local = sortedLocals[i];

                routineCode += $"\t\t\t[{local.Id}] {_getTypeFromTypeNode(local.Type)} '{local.GetName()}'";
                if (i < len - 1) routineCode += ",\n";
                else routineCode += "\n\t\t";
            }

            routineCode += ")\n";
        }

        // Consume body
        foreach (var command in commands)
        {
            routineCode += "\t\t" + command.Translate() + '\n';
        }

        if (commands.Count == 0 || commands.Last() is not ReturnCommand)
            routineCode += "\t\t" + new ReturnCommand(commands.Count).Translate() + '\n';

        routineCode += "\t}\n";

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
            : $"{CodeGenerationVariable.NodeToType(routineCallNode.Routine.ReturnType)}";
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
        var value = constNode.Value!;
        if (value is bool boolValue) commands.Enqueue(new LoadConstantCommand(boolValue ? 1 : 0, commands.Count));
        else commands.Enqueue(new LoadConstantCommand(value, commands.Count));
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
        commands.Enqueue(new NewArrayCommand(_getTypeFromTypeNode(((ArrayTypeNode) arrayConst.Type).ElementTypeNode),
            commands.Count));
        commands.Enqueue(new SetLocalCommand(specialVariable.Id, specialVariable.GetName(), commands.Count));

        var counter = 0;
        foreach (var elements in arrayConst.Expressions.Expressions)
        {
            commands.Enqueue(new LoadLocalCommand(specialVariable.Id, specialVariable.GetName(), commands.Count));
            commands.Enqueue(new LoadConstantCommand(counter, commands.Count));
            elements.Accept(this, commands);
            commands.Enqueue(new SetElementByIndex(_getTypeFromTypeNode(elements.Type), commands.Count));

            counter++;
        }

        commands.Enqueue(new LoadLocalCommand(specialVariable.Id, specialVariable.GetName(), commands.Count));
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
        else commands.Enqueue(new LoadLocalAddressToStackCommand(codeGenVar.Id, codeGenVar.GetName(), commands.Count));
    }

    public void VisitArrayVarByValueNode(ArrayVarNode arrayVarNode, Queue<BaseCommand> commands)
    {
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
        else commands.Enqueue(new LoadLocalCommand(codeGenVar.Id, codeGenVar.GetName(), commands.Count));
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
        else commands.Enqueue(new LoadLocalAddressToStackCommand(codeGenVar.Id, codeGenVar.GetName(), commands.Count));
    }
    
    public void VisitStructVarByValueNode(StructVarNode structVarNode, Queue<BaseCommand> commands)
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
        else commands.Enqueue(new LoadLocalCommand(codeGenVar.Id, codeGenVar.GetName(), commands.Count));
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
        else commands.Enqueue(new LoadLocalCommand(codeGenVar.Id, codeGenVar.GetName(), commands.Count));
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
            ((ValueNode) varNode.Value).Accept(this, commands);
            commands.Enqueue(new StoreStructField(commands.Count));
        }
    }
}