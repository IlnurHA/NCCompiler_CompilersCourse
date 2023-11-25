using NCCompiler_CompilersCourse.Semantics;

namespace NCCompiler_CompilersCourse.CodeGeneration;

public interface IVisitor
{
    // public void VisitTypeNode(TypeNode typeNode, Queue<string> commands);
    public void VisitProgramNode(ProgramNode programNode, Queue<string> commands);
    public void VisitGetFieldNode(GetFieldNode getFieldNode, Queue<string> commands);
    public void VisitGetByIndexNode(GetByIndexNode getByIndexNode, Queue<string> commands);
    public void VisitSortedArrayNode(SortedArrayNode sortedArrayNode, Queue<string> commands);
    public void VisitArraySizeNode(ArraySizeNode arraySizeNode, Queue<string> commands);
    public void VisitReversedArrayNode(ReversedArrayNode reversedArrayNode, Queue<string> commands);
    public void VisitTypeVariableDeclaration(TypeVariableDeclaration typeVariableDeclaration, Queue<string> commands);

    public void VisitValueVariableDeclaration(ValueVariableDeclaration valueVariableDeclaration,
        Queue<string> commands);

    public void VisitFullVariableDeclaration(FullVariableDeclaration fullVariableDeclaration, Queue<string> commands);
    public void VisitVariableDeclarations(VariableDeclarations variableDeclarations, Queue<string> commands);
    public void VisitBreakNode(BreakNode breakNode, Queue<string> commands);
    public void VisitAssertNode(AssertNode assertNode, Queue<string> commands);
    public void VisitValueReturnNode(ValueReturnNode valueReturnNode, Queue<string> commands);
    public void VisitRangeNode(RangeNode rangeNode, Queue<string> commands);
    public void VisitForLoopNode(ForLoopNode forLoopNode, Queue<string> commands);
    public void VisitForEachLoopNode(ForEachLoopNode forEachLoopNode, Queue<string> commands);
    public void VisitWhileLoopNode(WhileLoopNode whileLoopNode, Queue<string> commands);
    public void VisitIfStatement(IfStatement ifStatement, Queue<string> commands);
    public void VisitIfElseStatement(IfElseStatement ifElseStatement, Queue<string> commands);
    public void VisitBodyNode(BodyNode bodyNode, Queue<string> commands);
    public void VisitAssignmentNode(AssignmentNode assignmentNode, Queue<string> commands);
    public void VisitArrayTypeNode(ArrayTypeNode arrayTypeNode, Queue<string> commands);
    public void VisitStructTypeNode(StructTypeNode structTypeNode, Queue<string> commands);
    public void VisitCastNode(CastNode castNode, Queue<string> commands);

    public void VisitRoutineDeclarationNode(RoutineDeclarationNode routineDeclarationNode, Queue<string> commands);

    // TODO - be aware of falling on ParameterNode
    public void VisitParametersNode(ParametersNode parametersNode, Queue<string> commands);
    public void VisitRoutineCallNode(RoutineCallNode routineCallNode, Queue<string> commands);
    public void VisitExpressionsNode(ExpressionsNode expressionsNode, Queue<string> commands);
    public void VisitConstNode(ConstNode constNode, Queue<string> commands);
    public void VisitOperationNode(OperationNode operationNode, Queue<string> commands);
    public void VisitArrayConst(ArrayConst arrayConst, Queue<string> commands);
    public void VisitPrimitiveVarNode(PrimitiveVarNode primitiveVarNode, Queue<string> commands);
}