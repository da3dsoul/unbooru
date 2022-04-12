using System;
using System.ComponentModel;

namespace unbooru.Abstractions.Poco.Events;

public class StartupEventArgs : CancelEventArgs
{
    public IServiceProvider Services { get; set; }
    public string[] Args { get; set; }
}