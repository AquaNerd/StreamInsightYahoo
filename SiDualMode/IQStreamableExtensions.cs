using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.ComplexEventProcessing;
using Microsoft.ComplexEventProcessing.Linq;

using SiDualMode.Base.Output;

namespace SiDualMode {
    public static class IQStreamableExtensions {
        public static IRemoteStreamableBinding ToBinding<TPayload>(this IQStreamable<TPayload> stream, Application cepApplication,
            Type consumerFactoryType, object configInfo, EventShape eventShape) {
            var factory = Activator.CreateInstance(consumerFactoryType) as ISinkFactory;

            if (factory == null) {
                throw new ArgumentException("Factory cannot be created or does not implement ISinkFactory");
            }

            switch (eventShape) {
                case EventShape.Point:
                    var pointObserver = cepApplication.DefineObserver(() =>
                        factory.CreatePointObserverSink<TPayload>(configInfo));
                    return stream.Bind(pointObserver);
                case EventShape.Interval:
                    var intervalObserver = cepApplication.DefineObserver(() =>
                        factory.CreateIntervalObserverSink<TPayload>(configInfo));
                    return stream.Bind(intervalObserver);
                case EventShape.Edge:
                    var edgeObserver = cepApplication.DefineObserver(() =>
                        factory.CreateEdgeObserverSink<TPayload>(configInfo));
                    return stream.Bind(edgeObserver);
                default:
                    throw new ArgumentOutOfRangeException("eventShape");
            }
        }
    }
}
