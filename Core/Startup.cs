using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using ImageInfrastructure.Abstractions;
using ImageInfrastructure.Abstractions.Attributes;
using ImageInfrastructure.Abstractions.Interfaces;
using ImageInfrastructure.Abstractions.Poco;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog;
using NLog.Extensions.Logging;
using NLog.Targets;
using ILogger = Microsoft.Extensions.Logging.ILogger;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace ImageInfrastructure.Core
{
    public class Startup
    {
        private List<IInfrastructureStartup> Modules { get; set; }
        private List<(IModule Instance, MethodInfo Method)> _shutdownCallbacks;
        private IServiceProvider _serviceProvider;
        
        public async Task Main(string[] args)
        {
            try
            {
                using var host = CreateHostBuilder(args).Build();

                var lifetime = host.Services.GetService<IHostApplicationLifetime>();
                _serviceProvider = host.Services;
                if (lifetime == null)
                {
                    Action<string> log;
                    var logger = host.Services.GetService<ILogger>();
                    if (logger == null) log = Console.WriteLine;
                    else log = s => logger.LogError("{Message}", s);

                    log("Could not get application lifetime");
                    return;
                }

                var cancellationToken = lifetime.ApplicationStopping;

                var coreContext = _serviceProvider.GetService<CoreContext>();
                if (coreContext == null)
                {
                    var logger = host.Services.GetService<ILogger>();
                    logger?.LogError("Unable to get Database Context");
                    return;
                }

                await coreContext.Database.MigrateAsync(cancellationToken);

                var processes = host.Services.GetServices<IModule>().ToList();
                _shutdownCallbacks = processes
                    .SelectMany(type => type.GetType().GetMethods(),
                        (type, method) => new
                        {
                            Type = type,
                            Method = method,
                            Attribute = method.GetCustomAttribute<ModuleShutdownAttribute>()
                        })
                    .Where(a =>
                    {
                        if (a.Attribute == null) return false;
                        var parameters = a.Method.GetParameters();
                        if (parameters.Length != 1) return false;
                        var parameter = parameters.FirstOrDefault();
                        if (parameter == null) return false;
                        if (parameter.IsIn || parameter.IsLcid || parameter.IsOut || parameter.IsRetval) return false;
                        return typeof(IServiceProvider).IsAssignableFrom(parameter.ParameterType);
                    }).Select(a => (a.Type, a.Method)).ToList();
                cancellationToken.Register(Shutdown);

                await host.StartAsync(cancellationToken);

                // get ready for post init. find the modules that define a post init method and sort by priority
                // group by priority, and run each priority stage concurrently to speed things up
                List<Task> tasks = new();
                var postMethods = processes
                    .SelectMany(type => type.GetType().GetMethods(),
                        (type, method) => new
                        {
                            Type = type,
                            Method = method,
                            Attribute = method.GetCustomAttribute<ModulePostConfigurationAttribute>()
                        })
                    .Where(a =>
                    {
                        if (a.Attribute == null) return false;
                        var parameters = a.Method.GetParameters();
                        if (parameters.Length != 1) return false;
                        var parameter = parameters.FirstOrDefault();
                        if (parameter == null) return false;
                        if (parameter.IsIn || parameter.IsLcid || parameter.IsOut || parameter.IsRetval) return false;
                        return typeof(IServiceProvider).IsAssignableFrom(parameter.ParameterType);
                    })
                    .GroupBy(a => a.Attribute.Priority)
                    .OrderBy(a => a.Key).ToList();

                foreach (var grouping in postMethods)
                {
                    foreach (var postInit in grouping)
                    {
                        if (postInit.Method.ReturnType.IsAssignableFrom(typeof(Task)) ||
                            postInit.Method.ReturnType.IsAssignableFrom(typeof(Task<>)))
                            tasks.Add((Task) postInit.Method.Invoke(postInit.Type, new object[] {_serviceProvider}));
                        else
                            tasks.Add(Task.Run(
                                () => postInit.Method.Invoke(postInit.Type, new object[] {_serviceProvider}),
                                cancellationToken));
                    }

                    await Task.WhenAll(tasks);
                }

                foreach (var task in processes.Select(process => process.RunAsync(host.Services, cancellationToken)))
                {
                    await task;
                }

                await host.WaitForShutdownAsync(cancellationToken);
            }
            catch (TaskCanceledException)
            {
                // ignore
            }
        }

        private void Shutdown()
        {
            var tasks = _shutdownCallbacks.Select(shutdown =>
            {
                if (shutdown.Method.ReturnType.IsAssignableFrom(typeof(Task)) ||
                    shutdown.Method.ReturnType.IsAssignableFrom(typeof(Task<>)))
                    return (Task) shutdown.Method.Invoke(shutdown.Instance, new object[] {_serviceProvider});
                return Task.FromResult(shutdown.Method.Invoke(shutdown.Instance, new object[] {_serviceProvider}));
            });
            Task.WhenAll(tasks).Wait();
        }

        public IHostBuilder CreateHostBuilder(string[] args)
        {
            FindTypes();
            var hostBuilder = Host.CreateDefaultBuilder(args);
            foreach (var startup in Modules)
            {
                if (startup is IHostProvider hostProvider)
                {
                    hostBuilder = hostProvider.Build(hostBuilder);
                }
            }
            hostBuilder = hostBuilder.ConfigureServices((_, services) =>
            {
                services.AddDbContext<CoreContext>(options =>
                {
                    options.EnableSensitiveDataLogging();
                    options.UseLazyLoadingProxies();
                    options.ConfigureWarnings(builder =>
                    {
                        builder.Log((Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.CommandExecuted, LogLevel.None));
                    });
                });
                services.AddScoped<IContext<ImageTag>>(x => x.GetRequiredService<CoreContext>());
                services.AddScoped<IContext<ArtistAccount>>(x => x.GetRequiredService<CoreContext>());
                services.AddScoped<IContext<Image>>(x => x.GetRequiredService<CoreContext>());
                services.AddScoped<IReadWriteContext<Image>>(x => x.GetRequiredService<CoreContext>());
                services.AddScoped<IReadWriteContext<ResponseCache>>(x => x.GetRequiredService<CoreContext>());
                services.AddLogging(a =>
                {
                    a.ClearProviders();
                    a.AddNLog();
                    ConfigureNLog();
                });
                foreach (var module in Modules)
                {
                    module.ConfigureServices(services);
                }
            });
            return hostBuilder;
        }

        private void ConfigureNLog()
        {
            var configuration = new NLog.Config.LoggingConfiguration();
            Target target = new FileTarget("file")
            {
                FileName = Path.Combine(Arguments.DataPath, "Logs", "${shortdate}.log")
            };
            configuration.AddTarget(target);
            configuration.AddRule(NLog.LogLevel.Info, NLog.LogLevel.Fatal, target);

            target = new ConsoleTarget("console");
            configuration.AddTarget(target);
            configuration.AddRule(NLog.LogLevel.Trace, NLog.LogLevel.Fatal, target);

            LogManager.Configuration = configuration;
        }
        
        private void FindTypes()
        {
            // get all modules.
            Modules = new List<IInfrastructureStartup>();

            // load other assemblies
            // get all assembly names
            var assemblies = new HashSet<string>(AppDomain.CurrentDomain.GetAssemblies().Select(a => a.Location));
            var dir = new DirectoryInfo(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? throw new InvalidOperationException("Executing Assembly is null..."));
            var dlls = dir.GetFiles().Where(a =>
                a.Name.StartsWith("ImageInfrastructure.") && a.Name.EndsWith(".dll") &&
                !assemblies.Contains(a.FullName)).ToList();
            foreach (var dll in dlls)
            {
                Assembly.LoadFrom(dll.FullName);
            }

            foreach (var module in AppDomain.CurrentDomain.GetAssemblies().SelectMany(x => x.GetTypes()))
            {
                if (!typeof(IInfrastructureStartup).IsAssignableFrom(module)) continue;
                if (!module.IsClass) continue;
                if (module.IsAbstract) continue;

                Modules.Add((IInfrastructureStartup) Activator.CreateInstance(module));
            }
        }
    }
}