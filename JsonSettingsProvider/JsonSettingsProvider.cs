using System;
using System.IO;
using ImageInfrastructure.Abstractions.Interfaces;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace ImageInfrastructure.JsonSettingsProvider
{
    public class JsonSettingsProvider<T> : ISettingsProvider<T> where T : new()
    {
        public static string SettingsPath = "C:\\Users\\da3ds\\Desktop\\temp\\Settings";
        private T Settings { get; }
        private readonly ILogger<JsonSettingsProvider<T>> _logger;
    
        public JsonSettingsProvider(ILogger<JsonSettingsProvider<T>> logger)
        {
            _logger = logger;
            try
            {
                var path = Path.Combine(SettingsPath, typeof(T).FullName + ".json");
                if (!File.Exists(path))
                {
                    Settings = new T();
                    SaveSettings();
                }
                if (File.Exists(path))
                {
                    var text = File.ReadAllText(path);
                    Settings = JsonConvert.DeserializeObject<T>(text);
                }
            }
            catch (Exception e)
            {
                logger.LogError(e, "Unable to load settings for {Type}", typeof(T).Name);
            }
        }

        public TResult Get<TResult>(Func<T, TResult> func)
        {
            return func.Invoke(Settings);
        }
    
        public void Update(Action<T> func)
        {
            // update via lambda
            func.Invoke(Settings);

            SaveSettings();
        }

        private void SaveSettings()
        {
            try
            {
                if (!Directory.Exists(SettingsPath)) Directory.CreateDirectory(SettingsPath);
                var path = Path.Combine(SettingsPath, typeof(T).FullName + ".json");
                var text = JsonConvert.SerializeObject(Settings, new JsonSerializerSettings
                {
                    Formatting = Formatting.Indented,
                    DefaultValueHandling = DefaultValueHandling.Include,
                    NullValueHandling = NullValueHandling.Include,
                    StringEscapeHandling = StringEscapeHandling.EscapeNonAscii
                });
                File.WriteAllText(path, text);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Unable to save settings for {Type}", typeof(T).Name);
            }
        }
    }
}