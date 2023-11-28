using NCCompiler_CompilersCourse.Semantics;

namespace NCCompiler_CompilersCourse.CodeGeneration;

public class TranslationVisitorCodeGeneration : IVisitorCodeGeneration
{
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

    public void VisitSortedArrayNode(SortedArrayNode sortedArrayNode, Queue<BaseCommand> commands)
    {
        throw new NotImplementedException();
    }

    public void VisitArraySizeNode(ArraySizeNode arraySizeNode, Queue<BaseCommand> commands)
    {
        throw new NotImplementedException();
    }

    public void VisitReversedArrayNode(ReversedArrayNode reversedArrayNode, Queue<BaseCommand> commands)
    {
        throw new NotImplementedException();
    }

    public void VisitTypeVariableDeclaration(TypeVariableDeclaration typeVariableDeclaration, Queue<BaseCommand> commands)
    {
        throw new NotImplementedException();
    }

    public void VisitValueVariableDeclaration(ValueVariableDeclaration valueVariableDeclaration, Queue<BaseCommand> commands)
    {
        throw new NotImplementedException();
    }

    public void VisitFullVariableDeclaration(FullVariableDeclaration fullVariableDeclaration, Queue<BaseCommand> commands)
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
        throw new NotImplementedException();
    }

    public void VisitExpressionsNode(ExpressionsNode expressionsNode, Queue<BaseCommand> commands)
    {
        throw new NotImplementedException();
    }

    public void VisitConstNode(ConstNode constNode, Queue<BaseCommand> commands)
    {
        throw new NotImplementedException();
    }

    public void VisitOperationNode(OperationNode operationNode, Queue<BaseCommand> commands)
    {
        throw new NotImplementedException();
    }

    public void VisitArrayConst(ArrayConst arrayConst, Queue<BaseCommand> commands)
    {
        throw new NotImplementedException();
    }

    public void VisitPrimitiveVarNode(PrimitiveVarNode primitiveVarNode, Queue<BaseCommand> commands)
    {
        throw new NotImplementedException();
    }
    
    public void VisitArrayVarNode(ArrayVarNode arrayVarNode, Queue<BaseCommand> queue)
    {
        throw new NotImplementedException();
    }

    public void VisitStructVarNode(StructVarNode structVarNode, Queue<BaseCommand> queue)
    {
        throw new NotImplementedException();
    }

    public void VisitArrayFunctions(ArrayFunctions arrayFunctions, Queue<BaseCommand> queue)
    {
        throw new NotImplementedException();
    }

    public void VisitEmptyReturnNode(EmptyReturnNode emptyReturnNode, Queue<BaseCommand> queue)
    {
        throw new NotImplementedException();
    }
}