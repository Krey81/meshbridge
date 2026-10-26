using meshbridge.interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Buffers;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using System.Threading.Tasks;
namespace meshbridge.processors
{
    public class JsonProcessor<T> : IMqttIngress<T>
    {
        private readonly ILogger _logger;

        private readonly JsonTypeInfo _jsonTypeInfo;

        public JsonProcessor(ILogger logger) 
        { 
            _logger = logger;
            JsonTypeInfo? typeInfo = AppJsonContext.Default.GetTypeInfo(typeof(T));
            if (typeInfo == null) throw new ArgumentNullException(nameof(JsonTypeInfo), $"precompiled JsonTypeInfo not found for type {typeof(T).GetType().Name}");
            _jsonTypeInfo = typeInfo;
        }

        public Task<T> Deserialize(string topic, ReadOnlySequence<byte> payload)
        {
            T? result = default;
            try
            {
                Utf8JsonReader reader = new(payload);
                result = (T?)JsonSerializer.Deserialize(ref reader, _jsonTypeInfo);
                if (result == null) throw new Exception("null result");
            }
            catch (Exception e)
            {
                _logger.LogDebug("{service}: ошибка десериализации json: {err}", GetType().Name, e.Message);
                throw;
            }
            return Task.FromResult(result);
        }
    }
}
