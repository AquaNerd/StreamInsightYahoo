using System;
using System.ServiceModel;
using Microsoft.ComplexEventProcessing;
using Microsoft.ComplexEventProcessing.Linq;
using Microsoft.ComplexEventProcessing.ManagementService;
using System.Reactive;
using System.Reactive.Linq;
using System.Windows;

using SiDualMode;
using SiDualMode.Base;
using SiDualMode.InputAdapter.YahooFinanceAdapter;
using SiDualMode.OutputAdapter.WpfAdapter;

namespace ComplexStreamSI_Client.Model {
    public class SIViewModel : DependencyObject {
        public SIViewModel() {
            using (Server cepServer = Server.Connect(new EndpointAddress(@"http://localhost/StreamInsight/MyInstance"))) {
                var cepApplication = cepServer.Applications["DualMode"];

                var cepSource = cepApplication.GetStreamable<StreamInputEvent<YahooDataEvent>>("DualModeSource");

                var mySink = cepApplication.GetObserver<StreamOutputEvent<YahooDataEvent>>("PointSink");

                var sinkConfig = new WpfOutputConfig() {
                };

                
            }
        }
    }
}
