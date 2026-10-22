using meshbridge.interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Buffers;
using System.Text.Json;
using System.Threading.Tasks;

namespace meshbridge.processors
{
    public class JsonProcessor<T> : IMqttIngress<T>
    {
        private static JsonSerializerOptions _serializerOptions => GetOptions();

        private readonly ILogger _logger;


        public JsonProcessor(ILogger logger) 
        { 
            _logger = logger;
        }

        public Task<T> Deserialize(string topic, ReadOnlySequence<byte> payload)
        {
            T? result = default;
            try
            {
                Utf8JsonReader reader = new(payload);
                result = JsonSerializer.Deserialize<T>(ref reader, _serializerOptions);
                if (result == null) throw new Exception("null result");
            }
            catch (Exception e)
            {
                _logger.LogDebug("{service}: ошибка десериализации json: {err}", GetType().Name, e.Message);
                throw;
            }
            return Task.FromResult(result);
        }

        private static JsonSerializerOptions GetOptions()
        {
            return new JsonSerializerOptions()
            {
                PropertyNameCaseInsensitive = true
            };
        }
    }
}
