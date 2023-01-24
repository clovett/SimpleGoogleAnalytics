using GoogleAnalytics;
using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
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
                EmulateSession(analytics, sessionNr);
                PostMeasurements(analytics).Wait();
            }
        }

        private static void EmulateSession(Analytics analytics, int iSession)
        {
            //-generate ga_session_id and ga_session_number
            //-and pass with all events
            var sessionId = DateTimeOffset.Now.ToUnixTimeSeconds();

            var s = new SessionStartMeasurement()
            {
                SessionId = sessionId.ToString(),
                SessionNumber = iSession.ToString(),
            };

            //Todo:
            //Posting session_start results in validation error: "valid NAME_RESERVED: Event at index: [0] has name [session_start] which is reserved."
            //Maybe we should not send session_start?
            AddMeasurement(analytics, s, s);

            for (var pageNr = 1; pageNr <= Pages; pageNr++)
            {
                EmulatePageInteractions(analytics, s, pageNr);
            }
        }

        private static void EmulatePageInteractions(Analytics analytics, SessionStartMeasurement sessionStart, int pageNr)
        {
            var pv = new PageMeasurement()
            {
                Path = $"Page {pageNr}",
                Title = $"Page {pageNr}",
                HostName = "www.test99.ch",
                UserAgent = "Target"
            };
            AddMeasurement(analytics, sessionStart, pv);

            for (var eventNr = 1; eventNr <= Events; eventNr++)
            {
                var m = new TestEventMeasurement()
                {
                    Action = $"Action {pageNr}.{eventNr}",
                    Result = "passed",
                };
                AddMeasurement(analytics, sessionStart , m);
            }
        }

        private static void AddMeasurement(Analytics analytics, SessionStartMeasurement sessionStart, Measurement s)
        {
            s.SessionId = sessionStart.SessionId;
            s.SessionNumber = sessionStart.SessionNumber; 
            
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
            //Todo RoS: anyways track - test only
            //else
            {
                await HttpProtocol.PostMeasurements(analytics);
                Console.WriteLine("measurement sent!!");
            }

            analytics.Events.Clear();
        }
    }
}
