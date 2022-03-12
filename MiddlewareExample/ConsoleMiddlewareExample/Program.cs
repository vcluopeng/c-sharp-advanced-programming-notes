// See https://aka.ms/new-console-template for more information

var pipe = new PipeBuilder((x) => x.Dump("mainAction =============="))
                    .AddPipe(typeof(FirstPipe))
                    .AddPipe(typeof(Second))
                    .AddPipe(typeof(Try))
                    .Build();
pipe.Invoke("hi");
Console.ReadKey();


public class FirstPipe : Pipe
{
    public FirstPipe(Action<string> action) : base(action)
    {

    }

    public override void Handle(string s)
    {
        s.Dump("First start===");
        _action(s);
        s.Dump("First end=====");
    }
}
public class Second : Pipe
{
    public Second(Action<string> action) : base(action)
    {

    }

    public override void Handle(string s)
    {
        s.Dump("Second start=====");
        _action(s);
        s.Dump("Second end=======");
    }
}

public class Try : Pipe
{
    public Try(Action<string> action) : base(action)
    {

    }

    public override void Handle(string s)
    {
        try
        {
            s.Dump("Try start===========");
            _action(s);
            s.Dump("Try end=============");
        }
        catch (Exception ex)
        {
            ex.Message.Dump("catch");
        }
    }
}


public abstract class Pipe
{
    protected Action<string> _action;
    public Pipe(Action<string> action)
    {
        _action = action;
    }
    public abstract void Handle(string s);
}

public class PipeBuilder
{
    Action<string> _mainAction;
    List<Type> _pipeTypes = new List<Type>();
    public PipeBuilder(Action<string> mainAction)
    {
        _mainAction = mainAction;
    }
    public PipeBuilder AddPipe(Type pipeType)
    {
        if (!pipeType.IsSubclassOf(typeof(Pipe)))
        {
            throw new NotImplementedException($"参数{nameof(pipeType)}需要继承{nameof(Pipe)}");
        }
        _pipeTypes.Add(pipeType);
        return this;
    }
    private Action<string> CreatePipe(int index)
    {
        if (index < _pipeTypes.Count - 1)
        {
            var nextPipe = CreatePipe(index + 1);
            var pipe = (Pipe)Activator.CreateInstance(_pipeTypes[index], nextPipe)!;
            return pipe.Handle;
        }
        else
        {
            var finalPipe = (Pipe)Activator.CreateInstance(_pipeTypes[index], _mainAction)!;
            return finalPipe.Handle;
        }
    }
    public Action<string> Build()
    {
        if (!_pipeTypes.Any()) return _mainAction;
        return CreatePipe(0);
    }
}

public static class ConsoleDump
{
    public static void Dump(this string s, string? t = null)
    {
        if (t == null)
        {
            Console.WriteLine(s);
            return;
        }
        Console.WriteLine($"{t} => {s}");
    }
}