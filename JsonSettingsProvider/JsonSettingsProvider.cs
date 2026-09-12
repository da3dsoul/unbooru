using System;
using System.IO;
using unbooru.Abstractions;
using unbooru.Abstractions.Interfaces;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace unbooru.JsonSettingsProvider
{
    public class JsonSettingsProvider<T> : ISettingsProvider<T> where T : new()
    {
        private T Settings { get; set; }
        private readonly ILogger<JsonSettingsProvider<T>> _logger;
        private readonly object _lock = new object();
        private readonly string _path;

        public JsonSettingsProvider(ILogger<JsonSettingsProvider<T>> logger)
        {
            _logger = logger;
            _path = Path.Combine(Arguments.DataPath, "Settings", typeof(T).FullName + ".json");

            lock (_lock)
            {
                try
                {
                    if (File.Exists(_path))
                    {
                        var text = File.ReadAllText(_path);
                        if (!string.IsNullOrWhiteSpace(text))
                            Settings = JsonConvert.DeserializeObject<T>(text);
                    }
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Unable to load settings for {Type}: {Error}", typeof(T).Name, e);
                }

                if (Settings == null)
                {
                    // missing, empty, or corrupt file: fall back to defaults and self-heal the file
                    // instead of leaving Settings permanently null on disk.
                    _logger.LogWarning("Settings for {Type} could not be loaded from {Path}; resetting to defaults", typeof(T).Name, _path);
                    Settings = new T();
                    SaveSettingsLocked();
                }
            }
        }

        public TResult Get<TResult>(Func<T, TResult> func)
        {
            lock (_lock)
            {
                return func.Invoke(Settings);
            }
        }

        public void Update(Action<T> func)
        {
            lock (_lock)
            {
                // update via lambda
                func.Invoke(Settings);

                SaveSettingsLocked();
            }
        }

        private void SaveSettingsLocked()
        {
            try
            {
                var directory = Path.GetDirectoryName(_path);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory)) Directory.CreateDirectory(directory);

                var text = JsonConvert.SerializeObject(Settings, new JsonSerializerSettings
                {
                    Formatting = Formatting.Indented,
                    DefaultValueHandling = DefaultValueHandling.Include,
                    NullValueHandling = NullValueHandling.Include,
                    StringEscapeHandling = StringEscapeHandling.EscapeNonAscii
                });

                // Write to a temp file and rename over the real one so a crash or a
                // concurrent writer can never observe (or leave behind) a truncated/empty file.
                var tempPath = _path + ".tmp";
                File.WriteAllText(tempPath, text);
                if (File.Exists(_path)) File.Replace(tempPath, _path, null);
                else File.Move(tempPath, _path);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Unable to save settings for {Type}", typeof(T).Name);
            }
        }
    }
}
