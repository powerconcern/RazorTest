using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Options;
using MQTTnet.Diagnostics;

namespace powerconcern.mqtt.services
{
    public class MQTTService: BackgroundService
    {
        public ILogger Logger { get; }
        public MqttFactory Factory { get; }

        public IMqttClient MqttClnt {get; }
        //automatically passes the logger factory in to the constructor via dependency injection
        public MQTTService(ILoggerFactory loggerFactory)
        {
            
            Factory=new MqttFactory();

            MqttClnt=Factory.CreateMqttClient();

            
            Logger = loggerFactory?.CreateLogger("MQTTSvc");
            if(Logger == null)
            {
                throw new ArgumentNullException(nameof(loggerFactory));
            }
            MqttNetGlobalLogger.LogMessagePublished += OnTraceMessagePublished;

            Logger.LogInformation("MQTTService created");
        }

        private void OnTraceMessagePublished(object sender, MqttNetLogMessagePublishedEventArgs e)
        {
            Logger.LogTrace(e.TraceMessage.Message);
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Logger.LogInformation("Background thread started");
            
            var options = new MqttClientOptionsBuilder()
            .WithClientId(Guid.NewGuid().ToString())
            .WithTcpServer("192.168.222.20", 1883)
            .WithCleanSession()
            .Build();

            stoppingToken.Register(() => Console.WriteLine("Background svc is stopping."));

            // Connecting
            var result = MqttClnt.ConnectAsync(options);

            MqttClient client=(MqttClient)MqttClnt;
            client.UseConnectedHandler(async e =>
            {
                Console.WriteLine("### CONNECTED WITH SERVER ###");

                // Subscribe to a topic
                await client.SubscribeAsync(new TopicFilterBuilder().WithTopic("mytopic").Build());

                Console.WriteLine("### SUBSCRIBED ###");
            });

            client.UseApplicationMessageReceivedHandler(e =>
            {
                Console.WriteLine("### RECEIVED APPLICATION MESSAGE ###");
                Console.WriteLine($"+ Topic = {e.ApplicationMessage.Topic}");
                Console.WriteLine($"+ Payload = {Encoding.UTF8.GetString(e.ApplicationMessage.Payload)}");
                Console.WriteLine($"+ QoS = {e.ApplicationMessage.QualityOfServiceLevel}");
                Console.WriteLine($"+ Retain = {e.ApplicationMessage.Retain}");
                Console.WriteLine();

                if(client != null) {
                    Task.Run(() => client.PublishAsync("hello/world"));
                }
            });

            Console.WriteLine("Background svc is stopping.");
            
            return Task.CompletedTask;
        }
    }
}