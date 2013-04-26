using System;
using Microsoft.ComplexEventProcessing;
using Microsoft.ComplexEventProcessing.Adapters;

using SiDualMode.Base.Output;

namespace SiDualMode.OutputAdapter.ConsoleAdapter {
    //This is the factory Class that initializes and creates an instance of the ConsoleDataConsumer object
    //Rx-only model uses ISinkFactory ONLY but for Dual-Mode (Adapter-Model support) ITypedOutputAdapterFactory is used too
    public class ConsoleOutputFactory : ISinkFactory { //, ITypedOutputAdapterFactory<ConsoleOutputConfig> {
        public IObserver<PointEvent<TPayload>> CreatePointObserverSink<TPayload>(object config) {
            return new ConsoleDataConsumer<TPayload>((ConsoleOutputConfig)config);
        }

        public IObserver<IntervalEvent<TPayload>> CreateIntervalObserverSink<TPayload>(object config) {
            return new ConsoleDataConsumer<TPayload>((ConsoleOutputConfig)config);
        }

        public IObserver<EdgeEvent<TPayload>> CreateEdgeObserverSink<TPayload>(object config) {
            return new ConsoleDataConsumer<TPayload>((ConsoleOutputConfig)config);
        }

        public OutputAdapterBase Create<TPayload>(ConsoleOutputConfig configInfo, EventShape eventShape) {
            switch (eventShape) {
                case EventShape.Point:
                    return new ObserverTypedPointOutputAdapter<TPayload>(CreatePointObserverSink<TPayload>(configInfo));
                case EventShape.Interval:
                    return new ObserverTypedIntervalOutputAdapter<TPayload>(CreateIntervalObserverSink<TPayload>(configInfo));
                case EventShape.Edge:
                    return new ObserverTypedEdgeOutputAdapter<TPayload>(CreateEdgeObserverSink<TPayload>(configInfo));
                default:
                    throw new ArgumentOutOfRangeException("eventShape");
            }
        }

        public void Dispose() {
            //throw new NotImplementedException();
        }
    }
}
