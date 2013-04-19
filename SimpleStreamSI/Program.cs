using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.ServiceModel;
using System.Threading;
using System.Xml.Linq;
using Microsoft.ComplexEventProcessing;
using Microsoft.ComplexEventProcessing.Linq;
using Microsoft.ComplexEventProcessing.ManagementService;

using SimpleStreamSI.Events.Input;
using SimpleStreamSI.Request;

namespace SimpleStreamSI {
    class Program {
        private static string[] _symbols = new string[] { "AAPL", "DELL", "MSFT" };

        static void Main(string[] args) {

            //Connect to a remote SI Server instance
            //using (Server server = Server.Connect(new System.ServiceModel.EndpointAddress(@"http://localhost/StreamInsight/default"))) {

            //Create the Embedded SI Server
            using (Server server = Server.Create("default")) {
                //Create a local end point for the server embedded into this program
                //This exposes SI to allow other applications to read from this SI server
                var myHost = new ServiceHost(server.CreateManagementService());
                myHost.AddServiceEndpoint(typeof(IManagementService), new WSHttpBinding(SecurityMode.Message), "http://localhost/StreamInsight/MyInstance");
                myHost.Open();

                //Create the SI application
                Application application = server.CreateApplication("StreamYahooSI");

                //Create Stream of YahooQuote Objects
                //this stream requests a quote per interval TimeSpan specified
                var inputStream = application.DefineObservable(() => Observable.Interval(TimeSpan.FromSeconds(1))).ToPointStreamable(x =>
                    PointEvent<YahooQuote>.CreateInsert(DateTime.Now, YahooFinance.GetSingleQuote("AAPL")),
                    AdvanceTimeSettings.IncreasingStartTime);         

                //Query the stream
                var myQuery = from evt in inputStream
                              select evt;

                //setup sink as observer and specify onNext action to write to console
                var consoleObserver = application.DefineObserver(() => Observer.Create<PointEvent<YahooQuote>>(ConsoleWritePoint));
                
                //bind sink to observe query and run the process
                using (var proc = myQuery.Bind(consoleObserver).Run("serverProcess")) {

                    Console.WriteLine();
                    Console.WriteLine("*** Hit Enter to exit after viewing query output ***");
                    Console.WriteLine();
                    Console.ReadLine();
                }

                myHost.Close();
                Console.WriteLine();
                Console.WriteLine("Process Stopped.  Hit Enter to quit.");
                Console.ReadLine();
            }
        }

        //use for sink creation if sink consists of PointEvents
        static void ConsoleWritePoint<TPayload>(PointEvent<TPayload> e) {
            if (e.EventKind == EventKind.Insert) {
                Console.WriteLine("INSERT <{0}> {1}", e.StartTime.ToLocalTime().ToString("MM/dd/yyyy HH:mm:ss.fff"), e.Payload.ToString());
            } else {
                Console.WriteLine("CTI    {0}", e.StartTime.ToLocalTime().ToString("MM/dd/yyyy HH:mm:ss.fff"));
            }
        }

        //use for sink creation if sink consists of IntervalEvents
        static void ConsoleWriteInterval<TPayload>(IntervalEvent<TPayload> e) {
            if (e.EventKind == EventKind.Insert)
                Console.WriteLine(string.Format(CultureInfo.InvariantCulture, "INSERT <{0} - {1}> {2}", e.StartTime.ToLocalTime().ToString("MM/dd/yyyy HH:mm:ss.fff"), e.EndTime.ToLocalTime().ToString("MM/dd/yyyy HH:mm:ss.fff"), e.Payload.ToString()));
            else
                Console.WriteLine(string.Format(CultureInfo.InvariantCulture, "CTI    <{0}>", e.StartTime.ToLocalTime().ToString("MM/dd/yyyy HH:mm:ss.fff")));
        }

        //use for sink creation if sink consists of YahooQuote events
        static void ConsoleWriteQuote<TPayload>(YahooQuote e) {
            Console.WriteLine(e);
        }
    }
}
