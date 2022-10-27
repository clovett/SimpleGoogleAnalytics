using GoogleAnalytics;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

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

            var m = new TestEventMeasurement()
            {
                Action = "test",
                Result = "passed",
                Bugs = 10,
                TestTime = 23
            };

            analytics.Events.Add(m);


            var errors = await HttpProtocol.ValidateMeasurements(analytics);
            if (errors.ValidationMessages?.Length > 0)
            {
                foreach (var error in errors.ValidationMessages)
                {
                    Console.WriteLine("{0}: {1}", error.ValidationCode, error.Description);
                }
            }
            else
            {
                Console.WriteLine("measurement validated!!");
                await HttpProtocol.PostMeasurements(analytics);
                Console.WriteLine("measurement sent!!");
            }
        }
    }
}
