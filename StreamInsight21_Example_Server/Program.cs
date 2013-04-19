using System;
using System.ServiceModel;
using Microsoft.ComplexEventProcessing;
using Microsoft.ComplexEventProcessing.Linq;
using Microsoft.ComplexEventProcessing.ManagementService;
using System.Reactive;
using System.Reactive.Linq;

namespace StreamInsight21_example_Server

    /* This example:
     * creates an embedded server instance and makes it available to other clients
     * defines, deploys, binds, and runs a simple source, query, and sink
     * waits for the user to stop the server
     */
{
    class Program {
        static void Main(string[] args) {
            // Create an embedded StreamInsight server
            using (var server = Server.Create("default")) {
                // Create a local end point for the server embedded in this program
                var host = new ServiceHost(server.CreateManagementService());
                host.AddServiceEndpoint(typeof(IManagementService), new WSHttpBinding(SecurityMode.Message), "http://localhost/StreamInsight/MyInstance");
                host.Open();

                /* The following entities will be defined and available in the server for other clients:
                 * serverApp
                 * serverSource
                 * serverSink
                 * serverProcess
                 */

                // CREATE a StreamInsight APPLICATION in the server
                var myApp = server.CreateApplication("serverApp");

                // DEFINE a simple SOURCE (returns a point event every second)
                var mySource = myApp.DefineObservable(() => Observable.Interval(TimeSpan.FromSeconds(1))).ToPointStreamable(x => 
                    PointEvent.CreateInsert(DateTimeOffset.Now, x), 
                    AdvanceTimeSettings.StrictlyIncreasingStartTime);

                // DEPLOY the source to the server for clients to use
                mySource.Deploy("serverSource");

                // Compose a QUERY over the source (return every even-numbered event)
                var myQuery = from e in mySource
                              where e % 2 == 0
                              select e;

                // DEFINE a simple observer SINK (writes the value to the server console)
                var mySink = myApp.DefineObserver(() => Observer.Create<long>(x => Console.WriteLine("sink_Server..: {0}", x)));

                // DEPLOY the sink to the server for clients to use
                mySink.Deploy("serverSink");

                // BIND the query to the sink and RUN it
                using (var proc = myQuery.Bind(mySink).Run("serverProcess")) {
                    // Wait for the user stops the server
                    Console.WriteLine("----------------------------------------------------------------");
                    Console.WriteLine("MyStreamInsightServer is running, press Enter to stop the server");
                    Console.WriteLine("----------------------------------------------------------------");
                    Console.WriteLine(" ");
                    Console.ReadLine();
                }
                host.Close();
            }
        }
    }
}
