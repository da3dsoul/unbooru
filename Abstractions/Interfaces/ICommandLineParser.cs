using CommandLine;
using unbooru.Abstractions.Poco.Events;
namespace unbooru.Abstractions.Interfaces;

public interface ICommandLineParser
{
    ParserResult<object> ParseArguments(StartupEventArgs args, ParserResult<object> result);
}
