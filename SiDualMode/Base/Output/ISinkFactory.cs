using System;
using Microsoft.ComplexEventProcessing;

namespace SiDualMode.Base.Output {
    internal interface ISinkFactory {
        IObserver<PointEvent<TPayload>> CreatePointObserverSink<TPayload>(object config);
        IObserver<IntervalEvent<TPayload>> CreateIntervalObserverSink<TPayload>(object config);
        IObserver<EdgeEvent<TPayload>> CreateEdgeObserverSink<TPayload>(object config);
    }
}
