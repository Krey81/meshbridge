using meshbridge.configuration;
using meshbridge.format;
using meshbridge.interfaces;
using meshbridge.processors;
using meshbridge.services;
using Meshtastic.Protobufs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Serilog;
using System;
using System.Threading.Channels;
using System.Threading.Tasks;
using Channel = System.Threading.Channels.Channel;
namespace meshbridge
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var builder = Host.CreateApplicationBuilder(args);
            ConfigureLogging(builder);
            ConfigureOptions(builder);
            RegisterChannel(builder);

            builder.Services.AddScoped<IMqttIngress<FormatSensorSpot>>(ingressFactory);
            builder.Services.AddSingleton<MqttSubscriberService<FormatSensorSpot>>();
            builder.Services.AddHostedService(sp => sp.GetRequiredService<MqttSubscriberService<FormatSensorSpot>>());

            builder.Services.AddScoped<IMqttEgress<ServiceEnvelope>>(egressFactory);
            builder.Services.AddSingleton<MqttPublisherService<ServiceEnvelope>>();
            builder.Services.AddHostedService(sp => sp.GetRequiredService<MqttPublisherService<ServiceEnvelope>>());

            builder.Services.AddHostedService<BridgeService>();

            var host = builder.Build();

            try
            {
                Log.Information("Запуск сервиса MeshBridge...");
                await host.RunAsync();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Сервис неожиданно завершил работу (Fatal Error)");
            }
            finally
            {
                Log.Information("Остановка сервиса MeshBridge...");
                await Log.CloseAndFlushAsync();
            }


            //Console.WriteLine(Convert.ToHexString(EnvelopeConverter.NodeInfo(nodeInfo).ToByteArray()));
            //Console.WriteLine(Convert.ToHexString(EnvelopeConverter.Position(position).ToByteArray()));
            ;
        }

        private static IMqttEgress<ServiceEnvelope> egressFactory(IServiceProvider provider)
        {
            return new EnvelopeProcessor();
        }

        private static void RegisterChannel(HostApplicationBuilder builder)
        {
            builder.Services.AddSingleton(Channel.CreateBounded<FormatSensorSpot>(100));
            builder.Services.AddSingleton(sp => sp.GetRequiredService<Channel<FormatSensorSpot>>().Writer);
            builder.Services.AddSingleton(sp => sp.GetRequiredService<Channel<FormatSensorSpot>>().Reader);
        }

        private static IMqttIngress<FormatSensorSpot> ingressFactory(IServiceProvider provider)
        {
            return new JsonProcessor<FormatSensorSpot>(NullLogger.Instance);
        }

        private static void ConfigureOptions(HostApplicationBuilder builder)
        {
            ConfigureOption<BrokerConfig>(builder, Constants.BrokerInSectionName);
            ConfigureOption<BrokerConfig>(builder, Constants.BrokerOutSectionName);
        }

        private static void ConfigureOption<T>(HostApplicationBuilder builder, string sectionName) where T : class
        {
            IConfigurationSection section = builder.Configuration.GetSection(sectionName);
            builder.Services.Configure<T>(sectionName, section);
        }

        private static void ConfigureLogging(HostApplicationBuilder builder)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .Enrich.FromLogContext()
                .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
                .WriteTo.File(
                    path: "logs/meshbridge-.log",
                    rollingInterval: RollingInterval.Day,
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
                .CreateLogger();

            builder.Logging.ClearProviders();
            builder.Logging.AddSerilog(Log.Logger, dispose: true);
        }
    }
}
