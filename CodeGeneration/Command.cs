using NCCompiler_CompilersCourse.Semantics;

namespace NCCompiler_CompilersCourse.CodeGeneration;

public abstract class BaseCommand
{
    public abstract string Translate();
}

public class CallCommand : BaseCommand
{
    public string FunctionName { get; set; }

    public CallCommand(string functionName)
    {
        FunctionName = functionName;
    }

    public override string Translate()
    {
        return "call" + '\t' + FunctionName;
    }
}

public class JumpCommand : BaseCommand
{
    public string? Address { get; set; }

    public void SetAddress(string address)
    {
        Address = address;
    }

    public override string Translate()
    {
        return "br.s" + '\t' + Address;
    }
}

public class JumpForBreakCommand : JumpCommand
{
}

public class JumpIfTrue : JumpCommand
{
    public override string Translate()
    {
        return "brtrue.s" + '\t' + Address;
    }
}

public abstract class LocalVarCommand : BaseCommand
{
    public int Index { get; set; }

    public LocalVarCommand(int index)
    {
        Index = index;
    }
}

public class LoadLocalCommand : LocalVarCommand
{
    public LoadLocalCommand(int index) : base(index)
    {
    }

    public override string Translate()
    {
        return $"ldloc.{Index}";
    }
}

public class SetLocalCommand : LocalVarCommand
{
    public SetLocalCommand(int index) : base(index)
    {
    }

    public override string Translate()
    {
        return $"stloc.{Index}";
    }
}

public class ReturnCommand : BaseCommand
{
    public override string Translate()
    {
        return "ret";
    }
}

public class DuplicateCommand : BaseCommand
{
    public override string Translate()
    {
        return "dup";
    }
}
public class LoadLocalAddressToStackCommand : BaseCommand
{
    public string VarName { get; }

    public LoadLocalAddressToStackCommand(string varName)
    {
        VarName = varName;
    }

    public override string Translate()
    {
        return "ldloca.s" + '\t' + VarName;
    }
}
public class SetFieldCommand : BaseCommand
{
    public string Type { get; }
    // Struct name with necessary namespaces
    public string StructName { get; }
    public string FieldName { get; }

    public SetFieldCommand(string type, string structName, string fieldName)
    {
        Type = type;
        StructName = structName;
        FieldName = fieldName;
    }

    public override string Translate()
    {
        return $"stfld\t{Type} {StructName}::{FieldName}";
    }
}

public class LoadFieldCommand : BaseCommand
{
    // Getting object from stack and pushes value of specified field
    
    public string Type { get; }
    // Struct name with necessary namespaces
    public string StructName { get; }
    public string FieldName { get; }
    
    public LoadFieldCommand(string type, string structName, string fieldName)
    {
        Type = type;
        StructName = structName;
        FieldName = fieldName;
    }
    
    public override string Translate()
    {
        return $"ldfld\t{Type} {StructName}::{FieldName}";
    }
}

public class InitObjectCommand : BaseCommand
{
    // Pops address from stack and initializes object type to it
    
    // Together with necessary namespaces
    public string ObjectName { get; }

    public InitObjectCommand(string objectName)
    {
        ObjectName = objectName;
    }

    public override string Translate()
    {
        return $"initobj\t{ObjectName}";
    }
}

public class LoadConstantCommand : BaseCommand
{
    public object Value;

    public LoadConstantCommand(object value)
    {
        Value = value;
    }

    public override string Translate()
    {
        var type = "i4";

        if (Value is float) type = "r4";
        
        return $"ldc.{type}\t{Value}";
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

    public OperationCommand(OperationType operationType)
    {
        Operation = FromOperationType(operationType);
        opType = operationType;
    }

    public List<BaseCommand> GetOperations()
    {
        return opType switch
        {
            OperationType.Not => new List<BaseCommand>
                {new LoadConstantCommand(0), new OperationCommand(OperationType.Eq)},
            OperationType.Ge => new List<BaseCommand>
            {
                new OperationCommand(OperationType.Lt), new LoadConstantCommand(0),
                new OperationCommand(OperationType.Eq)
            },
            OperationType.Le => new List<BaseCommand>
            {
                new OperationCommand(OperationType.Gt), new LoadConstantCommand(0),
                new OperationCommand(OperationType.Eq)
            },
            OperationType.Ne => new List<BaseCommand>
            {
                new OperationCommand(OperationType.Eq), new LoadConstantCommand(0),
                new OperationCommand(OperationType.Eq)
            },
            _ => new List<BaseCommand> {this},
        };

    }
    public override string Translate()
    {
        return Operation;
    }
}

public class SetElementByIndex : BaseCommand
{
    public override string Translate()
    {
        return "stelem.i4";
    }
}

public class LoadArgumentFromFunction : BaseCommand
{
    public int Index = 1;

    public LoadArgumentFromFunction(int index)
    {
        Index = index;
    }
    public override string Translate()
    {
        if (Index < 0) throw new Exception("Index for argument from function should be non-negative");
        if (Index < 10) return $"ldarg.{Index}";
        return $"ldarg.s\t{Index}";
    }
}

public class StoreStackField : BaseCommand
{
    public override string Translate()
    {
        return "stfld";
    }
}

public class NewArrayCommand : BaseCommand
{
    public string Type { get; }

    public string FromTypeNode(TypeNode typeNode)
    {
        throw new NotImplementedException();
    }

    public NewArrayCommand(TypeNode typeNode)
    {
        Type = FromTypeNode(typeNode);
    }

    public override string Translate()
    {
        return $"newarr\t{Type}";
    }
}