using GoogleAnalytics;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Test
{
    class Program
    {
        private const int Sessions = 2;
        private const int Pages = 2;
        private const int Events = 2;

        static async Task Main(string[] args)
        {
            if (args.Length < 3)
            {
                Console.WriteLine("Usage Test <Google Analytics Measurement Id> <Api Secret> <clientId>");
                return;
            }

            Console.WriteLine("Please adjust launchSettings.json for you own needs!{0} Close this window if not yet done. {1} Press return to continue",Environment.NewLine, Environment.NewLine);
            Console.ReadLine();


            string trackingId = args[0];
            string apiSecret = args[1];
            string clientId = args[2];
            
            var analytics = new Analytics()
            {
                MeasurementId = trackingId,
                ApiSecret = apiSecret,
                ClientId = clientId
            };

            for (var iSession = 1; iSession <= Sessions; iSession++)
            {
                var s = new SessionStartMeasurement();
                await ProcessMeasurement(analytics, s);

                for (var pageNr = 1; pageNr <= Pages; pageNr++)
                {
                    await EmulatePageInteractions(analytics, pageNr);
                }
            }
        }

        private static async Task EmulatePageInteractions(Analytics analytics, int pageNr)
        {
            var pv = new PageMeasurement()
            {
                Path = $"Page {pageNr}",
                Title = $"Page {pageNr}",
                HostName = "www.test99.ch",
                UserAgent = "Target"
            };
            await ProcessMeasurement(analytics, pv);

            for (var eventNr = 1; eventNr <= Events; eventNr++)
            {
                var m = new TestEventMeasurement()
                {
                    Action = $"Action {pageNr}.{eventNr}",
                    TestTime = DateTime.Now.Ticks,
                    Result = "passed",
                    Bugs = 0,
                };
                await ProcessMeasurement(analytics, m);
            }
        }

        private static async Task ProcessMeasurement(Analytics analytics, Measurement s)
        {
            //Todo RoS: ValidateMeasurements failed posting more than one event
            //So we send them one by one for now
            analytics.Events.Add(s);
            await ProcessMeasurement(analytics);
            analytics.Events.Clear();
        }

        private static async Task ProcessMeasurement(Analytics analytics)
        {
            var errors = await HttpProtocol.ValidateMeasurements(analytics);
            if (errors.ValidationMessages?.Length > 0)
            {
                foreach (var error in errors.ValidationMessages)
                {
                    Console.WriteLine("{0}: {1}", error.ValidationCode, error.Description);
                }
            }
            //Todo RoS: skipping failed validation for now, to try out session_start (currently fails)
            //else
            {
                await HttpProtocol.PostMeasurements(analytics);
                Console.WriteLine("measurement sent!!");
            }
        }
    }
}
