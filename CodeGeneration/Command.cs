using NCCompiler_CompilersCourse.Semantics;

namespace NCCompiler_CompilersCourse.CodeGeneration;

public abstract class BaseCommand
{
    public int CommandIndex { get; set; }

    public BaseCommand(int index)
    {
        CommandIndex = index;
    }

    protected string FormattedIndex()
    {
        return FormattedAddress(CommandIndex) + ": ";
    }

    protected string FormattedAddress(int index)
    {
        var intermediateString = index.ToString("X");
        while (intermediateString.Length < 4) intermediateString = '0' + intermediateString;
        return $"IL_{intermediateString}";
    }
    public abstract string Translate();
}

public class CallCommand : BaseCommand
{
    public string FunctionName { get; set; }

    public CallCommand(string functionName, int index) : base(index)
    {
        FunctionName = functionName;
    }

    public override string Translate()
    {
        return FormattedIndex() + "call" + '\t' + FunctionName;
    }
}

public class JumpCommand : BaseCommand
{
    public int Address { get; set; } = -1;
    
    public JumpCommand(int index) : base(index) {}

    public void SetAddress(int address)
    {
        Address = address;
    }

    public override string Translate()
    {
        // TODO Write format command that will translate address to correct label
        return FormattedIndex()+ "br.s" + '\t' + FormattedAddress(Address);
    }
}

public class JumpForBreakCommand : JumpCommand
{
    public JumpForBreakCommand(int index) : base(index) {}
}

public class JumpIfTrue : JumpCommand
{
    public JumpIfTrue(int index) : base(index) {}
    public override string Translate()
    {
        return FormattedIndex() + "brtrue.s" + '\t' + FormattedAddress(Address);
    }
}

public class JumpIfFalse : JumpCommand
{
    public JumpIfFalse(int index) : base(index) {}
    public override string Translate()
    {
        return FormattedIndex() + "brfalse.s" + "\t" + FormattedAddress(Address);
    }
}

public abstract class LocalVarCommand : BaseCommand
{
    public string Name { get; set; }
    public int Index { get; }

    public LocalVarCommand(int index, string name, int commandIndex) : base(commandIndex)
    {
        Name = name;
        Index = index;
    }
}

public class LoadLocalCommand : LocalVarCommand
{
    public LoadLocalCommand(int index, string name, int commandIndex) : base(index, name, commandIndex)
    {
    }

    public override string Translate()
    {
        return FormattedIndex() + $"ldloc.s\t'{Name}'";
    }
}

public class SetLocalCommand : LocalVarCommand
{
    public SetLocalCommand(int index, string name, int commandIndex) : base(index, name, commandIndex)
    {
    }

    public override string Translate()
    {
        return FormattedIndex() + $"stloc.s\t'{Name}'";
    }
}

public class ReturnCommand : BaseCommand
{
    public ReturnCommand(int index) : base(index) {}
    public override string Translate()
    {
        return FormattedIndex() + "ret";
    }
}

public class DuplicateCommand : BaseCommand
{
    public DuplicateCommand(int index) : base(index) {}
    public override string Translate()
    {
        return FormattedIndex() + "dup";
    }
}
public class LoadLocalAddressToStackCommand : BaseCommand
{
    public string VarName { get; }

    public LoadLocalAddressToStackCommand(string varName, int index) : base(index)
    {
        VarName = varName;
    }

    public override string Translate()
    {
        return FormattedIndex() + "ldloc.s" + '\t' + $"'{VarName}'";
    }
}
public class SetFieldCommand : BaseCommand
{
    public string Type { get; }
    // Struct name with necessary namespaces
    public string StructName { get; }
    public string FieldName { get; }

    public SetFieldCommand(string type, string structName, string fieldName, int index) : base(index)
    {
        Type = type;
        StructName = structName;
        FieldName = fieldName;
    }

    public override string Translate()
    {
        return FormattedIndex() + $"stfld\t{Type} {StructName}::{FieldName}";
    }
}

public class InitObjectCommand : BaseCommand
{
    // Pops address from stack and initializes object type to it
    
    // Together with necessary namespaces
    public string ObjectName { get; }

    public InitObjectCommand(string objectName, int index) : base(index)
    {
        ObjectName = objectName;
    }

    public override string Translate()
    {
        return FormattedIndex() + $"initobj\t{ObjectName}";
    }
}

public class NopCommand : BaseCommand
{
    public NopCommand(int index) : base(index) {}
    public override string Translate()
    {
        return FormattedIndex() + "nop";
    }
}

public class LoadConstantCommand : BaseCommand
{
    public object Value;

    public LoadConstantCommand(object value, int index) : base(index)
    {
        Value = value;
    }

    public override string Translate()
    {
        var type = "i4";

        if (Value is float or double) type = "r4";
        
        return FormattedIndex() + $"ldc.{type}\t{Value}";
    }
}

public class OperationCommand : BaseCommand
{
    public string Operation;
    private OperationType opType; 

    public string FromOperationType(OperationType operationType)
    {
        return operationType switch
        {
            OperationType.UnaryMinus => "neg",
            OperationType.UnaryPlus => "nop",
            OperationType.Not => "ldc.i4.0\nceq",
            OperationType.And => "and",
            OperationType.Or => "or",
            OperationType.Xor => "xor",
            OperationType.Ge => "clt.un\nldc.i4.0\nceq",
            OperationType.Gt => "cgt",
            OperationType.Le => "cgt.un\nldc.i4.0\nceq",
            OperationType.Lt => "clt",
            OperationType.Eq => "ceq",
            OperationType.Ne => "ceq\nldc.i4.0\nceq",
            OperationType.Plus => "add",
            OperationType.Minus => "sub",
            OperationType.Mul => "mul",
            OperationType.Div => "div",
            OperationType.Rem => "rem",
            _ => throw new ArgumentOutOfRangeException(nameof(operationType), operationType, null)
        };
    }

    public OperationCommand(OperationType operationType, int index) : base(index)
    {
        Operation = FromOperationType(operationType);
        opType = operationType;
    }

    public List<BaseCommand> GetOperations()
    {
        return opType switch
        {
            OperationType.Not => new List<BaseCommand>
                {new LoadConstantCommand(0, CommandIndex), new OperationCommand(OperationType.Eq, CommandIndex + 1)},
            OperationType.Ge => new List<BaseCommand>
            {
                new OperationCommand(OperationType.Lt, CommandIndex), new LoadConstantCommand(0, CommandIndex + 1),
                new OperationCommand(OperationType.Eq, CommandIndex + 2)
            },
            OperationType.Le => new List<BaseCommand>
            {
                new OperationCommand(OperationType.Gt, CommandIndex), new LoadConstantCommand(0, CommandIndex + 1),
                new OperationCommand(OperationType.Eq, CommandIndex + 2)
            },
            OperationType.Ne => new List<BaseCommand>
            {
                new OperationCommand(OperationType.Eq, CommandIndex), new LoadConstantCommand(0, CommandIndex + 1),
                new OperationCommand(OperationType.Eq, CommandIndex + 2)
            },
            _ => new List<BaseCommand> {this},
        };

    }
    public override string Translate()
    {
        return FormattedIndex() + Operation;
    }
}

public class SetElementByIndex : BaseCommand
{
    public SetElementByIndex(int index) : base(index) {}
    public override string Translate()
    {
        return FormattedIndex() + "stelem.i4";
    }
}

public class LoadFunctionArgument : BaseCommand
{
    public int Index = 1;

    public LoadFunctionArgument(int index, int commandIndex) : base(commandIndex)
    {
        Index = index;
    }
    public override string Translate()
    {
        if (Index < 0) throw new Exception("Index for argument from function should be non-negative");
        if (Index < 10) return FormattedIndex() + $"ldarg.{Index}";
        return FormattedIndex() + $"ldarg.s\t{Index}";
    }
}

public class StoreStructField : BaseCommand
{
    public StoreStructField(int index) : base(index) {}
    public override string Translate()
    {
        return FormattedIndex() + "stfld";
    }
}

public class NewArrayCommand : BaseCommand
{
    public string Type { get; }

    public NewArrayCommand(string type, int index) : base(index)
    {
        Type = type;
    }

    public override string Translate()
    {
        return FormattedIndex() + $"newarr\t{Type}";
    }
}

public class CallVirtualCommand : CallCommand
{
    public CallVirtualCommand(string function, int index) : base(function, index) {}
    public override string Translate()
    {
        return FormattedIndex() + $"callvirt\t{FunctionName}";
    }
}

public class CastClassCommand : BaseCommand
{
    public string Type { get; }

    public CastClassCommand(string type, int index) : base(index)
    {
        Type = type;
    }

    public override string Translate()
    {
        return FormattedIndex() + $"castclass\t{Type}";
    }
}

public class ArrayLength : BaseCommand
{
    // ..., arr -> ..., length
 
    public ArrayLength(int index) : base(index) {}
    public override string Translate()
    {
        return FormattedIndex() + "ldlen";
    }
}

public class PrimitiveCastCommand : BaseCommand
{
    public string Type { get; }

    private string fromTypeNode(TypeNode typeNode)
    {
        if (typeNode.MyType == MyType.Integer || typeNode.MyType == MyType.Boolean) return "i4";
        if (typeNode.MyType == MyType.Real) return "r4";
        throw new Exception($"Unsupported type: {typeNode.MyType}-{typeNode.GetType()}");
    }

    public PrimitiveCastCommand(TypeNode typeNode, int index) : base(index)
    {
        Type = fromTypeNode(typeNode);
    }

    public override string Translate()
    {
        return FormattedIndex() + $"conv.{Type}";
    }
}

// public class SetArgumentByIndexCommand : BaseCommand
// {
//     public int Index { get; }
//
//     public SetArgumentCommand(int index)
//     {
//         Index = index;
//     }
//
//     public override string Translate()
//     {
//         return $"stelem"
//     }
// }

public class SetArgumentByNameCommand : BaseCommand
{
    public string Name { get; }

    public SetArgumentByNameCommand(string name, int index) : base(index)
    {
        Name = name;
    }

    public override string Translate()
    {
        return FormattedIndex() + $"starg.s\t{Name}";
    }
}

public class LoadFieldCommand : BaseCommand
{
    public string Type { get; }
    public string Struct { get; }
    public string Field { get; }

    public LoadFieldCommand(string type, string @struct, string field, int index) : base(index)
    {
        Type = type;
        Struct = @struct;
        Field = field;
    }

    public override string Translate()
    {
        return FormattedIndex() + $"ldfld\t{Type} {Struct}::{Field}";
    }
}

public class LoadByIndexCommand : BaseCommand
{
    // ..., arr, index -> ..., value
    public string Type { get; }
    
    public LoadByIndexCommand(string type, int index) : base(index) 
    {
        Type = type;
    }
    public override string Translate()
    {
        return FormattedIndex() + "ldelem.i4";
    }
}

public class LoadStringCommand : BaseCommand
{
    public string String { get; }

    public LoadStringCommand(string @string, int index) : base(index)
    {
        String = @string;
    }

    public override string Translate()
    {
        return FormattedIndex() + $"ldstr\t\"{String}\"";
    }
}