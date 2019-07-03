using System;
using Microsoft.Extensions.Logging;

namespace powerconcern.mqtt.services
{
    public interface IMQTTService
    {
        
    }
    public class MQTTService: IMQTTService
    {
        public ILogger Logger { get; }

        //automatically passes the logger factory in to the constructor via dependency injection
        public MQTTService(ILoggerFactory loggerFactory)
        {
            Logger = loggerFactory?.CreateLogger("MQTTLogger");
            if(Logger == null)
            {
                throw new ArgumentNullException(nameof(loggerFactory));
            }
            Logger.LogInformation("MQTTService created");
        }
    }
}