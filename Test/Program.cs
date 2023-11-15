using GoogleAnalytics;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Test
{
    class Program
    {
        static Analytics GetTestEvent(string clientId, string trackingId, string apiSecret)
        {
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
            return analytics;
        }

        static Analytics GetPageMeasurementEvent(string clientId, string trackingId, string apiSecret)
        {
            var analytics = new Analytics()
            {
                MeasurementId = trackingId,
                ApiSecret = apiSecret,
                ClientId = clientId
            };

            analytics.Events.Add(new PageMeasurement()
            {
                Path = "https://www.domain.tld",
                Title = "test",
            });
            return analytics;
        }

        static async Task ValidateAndPostEvent(Analytics analytics)
        {
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

            await ValidateAndPostEvent(GetTestEvent(clientId, trackingId, apiSecret));
            await ValidateAndPostEvent(GetPageMeasurementEvent(clientId, trackingId, apiSecret));

        }
    }
}
