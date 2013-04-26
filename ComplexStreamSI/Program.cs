using System;
using System.ServiceModel;
using Microsoft.ComplexEventProcessing;
using Microsoft.ComplexEventProcessing.Linq;
using Microsoft.ComplexEventProcessing.ManagementService;

using SiDualMode;
using SiDualMode.Base;
using SiDualMode.InputAdapter.TestDataAdapter;
using SiDualMode.InputAdapter.YahooFinanceAdapter;
using SiDualMode.OutputAdapter.ConsoleAdapter;

namespace ComplexStreamSI {
    internal class Program {
        private static ServiceHost _managementServiceHost;

        private static void Main(string[] args) {
            //NOTE: You can change to one of the CreateServer overloads below for different hosting models.
            //TODO: Change the instance name to an installed instance.
            using (Server cepServer = CreateServer("default", false, new Uri("http://localhost/StreamInsight/MyInstance"))) {
                //TODO: Create/connect to your application
                var application = cepServer.CreateApplication("DualMode");
                //TODO: Create/connect to your queries.
                //RunTestDataProcess(application);
                //RunQuery(application);
                RunYahooDataProcess(application);

                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.WriteLine("StreamInsight instance is running.");
                Console.WriteLine("Press any key to stop.");
                Console.ReadLine();
                Console.ResetColor();

                if (cepServer.IsEmbedded && _managementServiceHost != null) {
                    //Close the management service if created.
                    _managementServiceHost.Close();
                }
            }

            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine("StreamInsight instance has stopped running.");
            Console.WriteLine("Press any key to exit.");
            Console.ReadLine();
            Console.ResetColor();
        }

        /// <summary>
        /// Creates a StreamInsight server object using the in-process (hosted DLL) model.
        /// </summary>
        /// <param name="instanceName">Name of the StreamInsight instance</param>
        /// <param name="persistMetadata">True to persis metadata in SQL Server CE.</param>
        /// <param name="managementServiceUri">Uri of the management service. May be null.</param>
        /// <returns>The created StreamInsight server object</returns>
        /// <remarks>
        /// See http://msdn.microsoft.com/en-us/library/ff518487.aspx for information on
        /// setting security for the WCF management service url.
        /// </remarks>
        private static Server CreateServer(string instanceName, bool persistMetadata, Uri managementServiceUri) {
            Server server = null;

            if (persistMetadata) {
                //Create the metadata configuration.
                //Use the assembly name for the sdf.
                SqlCeMetadataProviderConfiguration metadataProviderConfiguration = new SqlCeMetadataProviderConfiguration() {
                    CreateDataSourceIfMissing = true,
                    DataSource = System.Reflection.Assembly.GetEntryAssembly().GetName().Name + ".sdf"
                };

                server = Server.Create(instanceName, metadataProviderConfiguration);
            } else {
                server = Server.Create(instanceName);
            }

            if (managementServiceUri != null) {
                //NOTE: You may get an exception her if you do not have the required permissions.
                try {
                    _managementServiceHost = new ServiceHost(server.CreateManagementService());
                    _managementServiceHost.AddServiceEndpoint(typeof(IManagementService), new WSHttpBinding(SecurityMode.Message), managementServiceUri);
                    _managementServiceHost.Open();
                } catch (Exception ex) {
                    //TODO: Continue or throw exception when management service cannot be created?
                    Console.WriteLine("Failed to open management service: ");
                    Console.WriteLine(ex.GetType().Name + ":" + ex.Message);
                }
            }
            return server;
        }

        /// <summary>
        /// Creates a StreamInsight server object that connects to a remote, running StreamInsight instance.
        /// </summary>
        /// <param name="instanceUri">Management Uri for the remote instance</param>
        /// <returns>The created StreamInsight server object</returns>
        private static Server CreateServer(Uri instanceUri) {
            Server server = Server.Connect(new EndpointAddress(instanceUri));
            return server;
        }

        //Process for Test Data
        private static void RunTestDataProcess(Application cepApplication) {
            var config = new TestDataInputConfig() {
                NumberOfItems = 20,
                RefreshInterval = TimeSpan.FromMilliseconds(500),
                TimestampIncrement = TimeSpan.FromMilliseconds(500),
                AlwaysUseNow = true,
                EnqueueCtis = false
            };

            AdvanceTimeSettings ats = new AdvanceTimeSettings(new AdvanceTimeGenerationSettings(
                                                                    TimeSpan.FromMilliseconds(750),
                                                                    TimeSpan.FromMilliseconds(200)), 
                null, AdvanceTimePolicy.Drop);

            var data = RxStream<TestDataEvent>.Create(cepApplication, typeof(TestDataInputFactory), config,
                EventShape.Point, ats);

            var sinkConfig = new ConsoleOutputConfig() {
                ShowCti = true,
                CtiEventColor = ConsoleColor.Blue,
                InsertEventColor = ConsoleColor.Green
            };

            var binding = data.ToBinding(cepApplication, typeof(ConsoleOutputFactory), sinkConfig, EventShape.Point);

            binding.Run("Hello");
        }

        //Process for Yahoo Data
        private static void RunYahooDataProcess(Application cepApplication) {
            var config = new YahooDataInputConfig() {
                Symbols = new string[] { "AAPL", "DELL", "MSFT", "GOOG", "GE" },
                RefreshInterval = TimeSpan.FromSeconds(5),
                TimestampIncrement = TimeSpan.FromSeconds(5),
                AlwaysUseNow = true,
                EnqueueCtis = false
            };

            AdvanceTimeSettings ats = new AdvanceTimeSettings(new AdvanceTimeGenerationSettings(
                                                                    TimeSpan.FromMilliseconds(750),
                                                                    TimeSpan.FromMilliseconds(200)),
                null, AdvanceTimePolicy.Drop);

            var data = RxStream<YahooDataEvent>.Create(cepApplication, typeof(YahooDataInputFactory), config,
                EventShape.Point, ats);

            var sinkConfig = new ConsoleOutputConfig() {
                ShowCti = true,
                ShowPayloadToString = false,
                CtiEventColor = ConsoleColor.Blue,
                InsertEventColor = ConsoleColor.Green
            };

            var myQuery = from x in data.ToPointEventStream()
                          select new {
                              x.Symbol,
                              x.LastTradePrice,
                              x.LastUpdateTime
                          };

            var binding = myQuery.ToBinding(cepApplication, typeof(ConsoleOutputFactory), sinkConfig, EventShape.Point);

            binding.Run("Hello");
        }

        private static void RunQuery(Application cepApplication) {
            var config = new YahooDataInputConfig() {
                Symbols = new string[] { "AAPL", "DELL", "MSFT", "GOOG", "GE" },
                RefreshInterval = TimeSpan.FromSeconds(0.5),
                TimestampIncrement = TimeSpan.FromSeconds(0.5),
                AlwaysUseNow = true,
                EnqueueCtis = false
            };

            AdvanceTimeSettings ats = new AdvanceTimeSettings(new AdvanceTimeGenerationSettings(
                                                                                TimeSpan.FromMilliseconds(1000),
                                                                                TimeSpan.FromMilliseconds(200)),
                                                null, AdvanceTimePolicy.Drop);

            var data = CepStream<YahooDataEvent>.Create(cepApplication, "TestData", typeof(YahooDataInputFactory), config,
                EventShape.Point, ats);

            var query = data.ToQuery(cepApplication, "Test", "Test", typeof(ConsoleOutputFactory),
                new ConsoleOutputConfig() {
                    ShowCti = true,
                    CtiEventColor = ConsoleColor.Yellow,
                    InsertEventColor = ConsoleColor.Magenta
                },
                EventShape.Point, StreamEventOrder.FullyOrdered);

            query.Start();

        }
    }
}
