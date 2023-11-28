using NCCompiler_CompilersCourse.Semantics;

namespace NCCompiler_CompilersCourse.CodeGeneration;

public interface IVisitorCodeGeneration
{
    // public void VisitTypeNode(TypeNode typeNode, Queue<string> commands);
    public void VisitProgramNode(ProgramNode programNode, Queue<BaseCommand> commands);
    
    public void VisitGetFieldNode(GetFieldNode getFieldNode, Queue<BaseCommand> commands);
    public void VisitGetByIndexNode(GetByIndexNode getByIndexNode, Queue<BaseCommand> commands);
    public void VisitSortedArrayNode(SortedArrayNode sortedArrayNode, Queue<BaseCommand> commands);
    public void VisitArraySizeNode(ArraySizeNode arraySizeNode, Queue<BaseCommand> commands);
    public void VisitReversedArrayNode(ReversedArrayNode reversedArrayNode, Queue<BaseCommand> commands);

    public void VisitTypeVariableDeclaration(TypeVariableDeclaration typeVariableDeclaration,
        Queue<BaseCommand> commands);

    public void VisitValueVariableDeclaration(ValueVariableDeclaration valueVariableDeclaration,
        Queue<BaseCommand> commands);

    public void VisitFullVariableDeclaration(FullVariableDeclaration fullVariableDeclaration,
        Queue<BaseCommand> commands);

    public void VisitVariableDeclarations(VariableDeclarations variableDeclarations, Queue<BaseCommand> commands);
    public void VisitBreakNode(BreakNode breakNode, Queue<BaseCommand> commands);
    public void VisitAssertNode(AssertNode assertNode, Queue<BaseCommand> commands);
    public void VisitValueReturnNode(ValueReturnNode valueReturnNode, Queue<BaseCommand> commands);
    public void VisitRangeNode(RangeNode rangeNode, Queue<BaseCommand> commands);
    public void VisitForLoopNode(ForLoopNode forLoopNode, Queue<BaseCommand> commands);
    public void VisitForEachLoopNode(ForEachLoopNode forEachLoopNode, Queue<BaseCommand> commands);
    public void VisitWhileLoopNode(WhileLoopNode whileLoopNode, Queue<BaseCommand> commands);
    public void VisitIfStatement(IfStatement ifStatement, Queue<BaseCommand> commands);
    public void VisitIfElseStatement(IfElseStatement ifElseStatement, Queue<BaseCommand> commands);
    public void VisitBodyNode(BodyNode bodyNode, Queue<BaseCommand> commands);
    public void VisitAssignmentNode(AssignmentNode assignmentNode, Queue<BaseCommand> commands);
    public void VisitTypeNode(TypeNode typeNode, Queue<BaseCommand> commands);
    public void VisitArrayTypeNode(ArrayTypeNode arrayTypeNode, Queue<BaseCommand> commands);
    public void VisitStructTypeNode(StructTypeNode structTypeNode, Queue<BaseCommand> commands);
    public void VisitCastNode(CastNode castNode, Queue<BaseCommand> commands);

    public void VisitRoutineDeclarationNode(RoutineDeclarationNode routineDeclarationNode, Queue<BaseCommand> commands);

    // TODO - be aware of falling on ParameterNode
    public void VisitParametersNode(ParametersNode parametersNode, Queue<BaseCommand> commands);
    public void VisitRoutineCallNode(RoutineCallNode routineCallNode, Queue<BaseCommand> commands);
    public void VisitExpressionsNode(ExpressionsNode expressionsNode, Queue<BaseCommand> commands);
    public void VisitConstNode(ConstNode constNode, Queue<BaseCommand> commands);
    public void VisitOperationNode(OperationNode operationNode, Queue<BaseCommand> commands);
    public void VisitArrayConst(ArrayConst arrayConst, Queue<BaseCommand> commands);
    public void VisitPrimitiveVarNode(PrimitiveVarNode primitiveVarNode, Queue<BaseCommand> commands);
    void VisitArrayVarNode(ArrayVarNode arrayVarNode, Queue<BaseCommand> queue);
    void VisitStructVarNode(StructVarNode structVarNode, Queue<BaseCommand> queue);
    void VisitArrayFunctions(ArrayFunctions arrayFunctions, Queue<BaseCommand> queue);
    void VisitEmptyReturnNode(EmptyReturnNode emptyReturnNode, Queue<BaseCommand> queue);
    void VisitVarNode(VarNode varNode, Queue<BaseCommand> queue);
    void VisitStructFieldNode(VarNode varNode, Queue<BaseCommand> queue);
}