using Google.Protobuf;
using Serilog;
using Serilog.Core;
using Serilog.Parsing;

public class DIContainer
{
    public Logger Logger { get; set; }

    public static readonly DIContainer Instance = new DIContainer();

   DIContainer()
    {
        Logger = new LoggerConfiguration()
            .Destructure.With(new MessageDestructuringPolicy())
            .Destructure.WhenNoOperator<IMessage>(new DestructuringFallback(Destructuring.Destructure, true))
            .WriteTo.Console()
            .CreateLogger();
    }
}
