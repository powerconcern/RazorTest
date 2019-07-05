using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Options;

namespace powerconcern.mqtt.services
{
    public class MQTTService: BackgroundService
    {
        public ILogger Logger { get; }
        public MqttFactory Factory { get; }

        public IMqttClient MqttClnt {get;
        }
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
            Logger.LogInformation("MQTTService created");
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Logger.LogInformation("Background thread started");
            

            var options = new MqttClientOptionsBuilder()
            .WithClientId(Guid.NewGuid().ToString())
            .WithTcpServer("m2m.eclipse.org", 1883)
            .WithCleanSession()
            .Build();

        // Connecting
        var result = MqttClnt.ConnectAsync(options);

        MqttClient clnt=(MqttClient)MqttClnt;
        while(clnt.IsConnected {
            Console.WriteLine("### CONNECTED WITH SERVER ###");

            clnt.SubscribeAsync(new TopicFilterBuilder().WithTopic("/mytopic").Build());

            Console.WriteLine("### SUBSCRIBED ###");
        };
            return Task.CompletedTask;
        }
    }
}