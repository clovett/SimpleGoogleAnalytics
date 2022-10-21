using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Threading.Tasks;
using GoogleAnalytics;

namespace Test
{
    class Program
    {
        static async Task Main(string[] args)
        {
            if (args.Length < 3)
            {
                Console.WriteLine("Usage Test <Google Analytics Measurement Id> <Api Secret> <uri>");
                return;
            }

            string clientId = Guid.NewGuid().ToString();
            string trackingId = args[0];
            string apiSecret = args[1];
            string uri = args[2];

            var analytics = new Analytics()
            {
                MeasurementId = trackingId,
                ApiSecret = apiSecret,
                ClientId = clientId
            };

            var m = new PageMeasurement()
            {
                Path = uri,
                Title = "Test"
            };

#if NETFRAMEWORK
            analytics.UserProperties["dotnet"] = ".NET Framework";
#else
            analytics.UserProperties["dotnet"] = RuntimeInformation.FrameworkDescription;
#endif

            m.Params["debug_mode"] = "1";

            analytics.Events.Add(m);

            await GoogleAnalytics.HttpProtocol.PostMeasurements(analytics);

            Console.WriteLine("measurement sent!!");
        }
    }
}
