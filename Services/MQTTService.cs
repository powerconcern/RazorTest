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
using MQTTnet.Protocol;

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

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Logger.LogInformation("Background thread started");
            
            

            var options = new MqttClientOptionsBuilder()
            .WithClientId(Guid.NewGuid().ToString())
            .WithTcpServer("mqtt.symlink.se")
            .WithCredentials("fredrik:fredrik", "aivUGL6no")
            .WithCleanSession()
            .Build();
            /*
            var options = new MqttClientOptionsBuilder()
            .WithClientId(Guid.NewGuid().ToString())
            .WithTcpServer("192.168.222.20", 1883)
            .WithCleanSession()
            .Build();
*/
            stoppingToken.Register(() => Console.WriteLine("Background svc is stopping."));

            // Connecting
            var result = MqttClnt.ConnectAsync(options);

            MqttClient client=(MqttClient)MqttClnt;
            client.UseConnectedHandler(async e =>
            {
                Console.WriteLine("### CONNECTED WITH SERVER ###");

                // Subscribe to a topic
                await client.SubscribeAsync(new TopicFilterBuilder().WithTopic("CurrentMeter/#").Build());
                await client.SubscribeAsync(new TopicFilterBuilder().WithTopic("EVCharger/#").Build());

                Console.WriteLine("### SUBSCRIBED ###");
            });

            stoppingToken.Register(() =>
            Logger.LogDebug($" MQTTSvc background task is stopping."));


/* Python rules
    client.subscribe("EVCharger/#")
    client.subscribe("CurrentMeter/#")


    if msg.topic == "EVCharger/status/current":
        charge_current = float(msg.payload)
        print("Charge current %.1f A" % charge_current)
    if msg.topic == "CurrentMeter/status/current":
        current = float(msg.payload)
        mean_current = (9*mean_current + current) / 10
        print("Current %.1f (mean %.1f)" % (current, mean_current))
        if mean_current > max_current:
            new_charge_current = charge_current - (mean_current-max_current)
            client.publish("EVCharger/set/current", payload=str(int(new_charge_current)), qos=0, retain=False)
            if new_charge_current < 2:
                client.publish("EVCharger/set/enable", payload=str(0), qos=0, retain=False)
        else:
            client.publish("EVCharger/set/current", payload=str(int(max_current)), qos=0, retain=False)
            client.publish("EVCharger/set/enable", payload=str(1), qos=0, retain=False)
 */
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(250, stoppingToken);
                //Logger.LogDebug("Waiting for messages");
                client.UseApplicationMessageReceivedHandler(e =>
                {
                    Console.WriteLine("### RECEIVED APPLICATION MESSAGE ###");
                    Console.WriteLine($"+ Topic = {e.ApplicationMessage.Topic}");
                    Console.WriteLine($"+ Payload = {Encoding.UTF8.GetString(e.ApplicationMessage.Payload)}");
                    Console.WriteLine($"+ QoS = {e.ApplicationMessage.QualityOfServiceLevel}");
                    Console.WriteLine($"+ Retain = {e.ApplicationMessage.Retain}");
                    Console.WriteLine();

                    if(client != null) {
                        /*Task.Run(() => client.PublishAsync("hello/world",
                                                            "4",
                                                            MqttQualityOfServiceLevel.AtLeastOnce,
                                                            true));
                        */
                    }
                });
            }

            Console.WriteLine("Background svc is stopping.");
            
            //return Task.CompletedTask;
        }
    }
}