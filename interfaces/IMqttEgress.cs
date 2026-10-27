using System.Threading.Tasks;

namespace meshbridge.interfaces
{
    /// <summary>
    /// Интерфейс для исходящих mqtt
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IMqttEgress<T>
    {
        /// <summary>
        /// Сериализация
        /// </summary>
        /// <param name="topic"></param>
        /// <param name="input"></param>
        /// <returns></returns>
        Task<byte[]> Serialize(string topic, T input);
    }
}
