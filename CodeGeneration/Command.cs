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
    public string Address { get; set; }

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