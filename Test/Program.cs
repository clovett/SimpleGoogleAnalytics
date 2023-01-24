using GoogleAnalytics;
using System;
using System.Threading.Tasks;

namespace Test
{
    class Program
    {
        private const int Sessions = 2;
        private const int Pages = 2;
        private const int Events = 2;
        private const bool PostImmediately=true;

        static void Main(string[] args)
        {
            if (args.Length < 3)
            {
                Console.WriteLine("Usage Test <Google Analytics Measurement Id> <Api Secret> <clientId>");
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

            for (var sessionNr = 1; sessionNr <= Sessions; sessionNr++)
            {
                EmulateSession(analytics);
                PostMeasurements(analytics).Wait();
            }
        }

        private static void EmulateSession(Analytics analytics)
        {
            //generate SessionId (ga_session_id) and pass with all events
            var sessionInfo = new SessionInfo { SessionId = DateTimeOffset.Now.ToUnixTimeSeconds().ToString() };

            for (var pageNr = 1; pageNr <= Pages; pageNr++)
            {
                EmulatePageInteractions(analytics, sessionInfo, pageNr);
            }
        }

        private static void EmulatePageInteractions(Analytics analytics, SessionInfo sessionInfo, int pageNr)
        {
            var pv = new PageMeasurement()
            {
                Path = $"Page {pageNr}",
                Title = $"Page {pageNr}",
                HostName = "www.test99.ch",
                UserAgent = "Target"
            };
            AddMeasurement(analytics, sessionInfo, pv);

            for (var eventNr = 1; eventNr <= Events; eventNr++)
            {
                var m = new TestEventMeasurement()
                {
                    Action = $"Action {pageNr}.{eventNr}",
                    Result = "passed",
                };
                AddMeasurement(analytics, sessionInfo , m);
            }
        }

        private static void AddMeasurement(Analytics analytics, SessionInfo sessionInfo, Measurement s)
        {
            s.SessionId = sessionInfo.SessionId;
            
            analytics.Events.Add(s);

            if (PostImmediately)
            {
                PostMeasurements(analytics).Wait();
            }
        }

        private static async Task PostMeasurements(Analytics analytics)
        {
            if (analytics.Events.Count == 0)
                return;

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
                await HttpProtocol.PostMeasurements(analytics);
                Console.WriteLine("measurement sent!!");
            }

            analytics.Events.Clear();
        }
    }

    public class SessionInfo
    {
        public string SessionId { get; set; }
    }
}
