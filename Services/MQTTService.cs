using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace powerconcern.mqtt.services
{
    public class MQTTService: BackgroundService
    {
        public ILogger Logger { get; }

        //automatically passes the logger factory in to the constructor via dependency injection
        public MQTTService(ILoggerFactory loggerFactory)
        {
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
            return Task.CompletedTask;
        }
    }
}