using System;
using System.Reactive.Linq;
using Microsoft.ComplexEventProcessing;
using Microsoft.ComplexEventProcessing.Linq;

using SiDualMode.Base;
using SiDualMode.Base.Input;
using SiDualMode.InputAdapter.TestDataAdapter;

namespace SiDualMode.Base {
    public static class RxStream<TPayload> {

        private static Func<Type, object, EventShape, IQbservable<StreamInputEvent<TPayload>>> _observable;

        public static IQbservable<StreamInputEvent<TPayload>> CreateObservable(Application cepApplication,
            Type sourceFactoryType, object configInfo, EventShape eventShape) {
            string entityName = "Observable:" + typeof(TPayload).FullName;
            if (_observable == null) {
                //Check the application.
                if (cepApplication.Entities.ContainsKey(entityName)) {
                    _observable = cepApplication.GetObservable<Type, object, EventShape, StreamInputEvent<TPayload>>(entityName);
                } else {
                    //Define and deploy.
                    _observable = cepApplication.DefineObservable(
                        (Type t, object c, EventShape e) => InstantiateObservable(t, c, e));
                    _observable.Deploy(entityName);
                }
            }
            return _observable.Invoke(sourceFactoryType, configInfo, eventShape);
        }

        private static IObservable<StreamInputEvent<TPayload>> InstantiateObservable(Type sourceFactoryType, object configInfo, EventShape eventShape) {
            var sourceFactory = Activator.CreateInstance(sourceFactoryType) as ISourceFactory;
            if (sourceFactory == null) {
                throw new ArgumentException("Specified type is not a source factory.");
            }
            return sourceFactory.CreateObservableSource<TPayload>(configInfo, eventShape);
        }

        public static IQStreamable<TPayload> Create(Application cepApplication, Type sourceFactoryType, object configInfo, EventShape eventShape) {
            return Create(cepApplication, sourceFactoryType, configInfo, eventShape, null);
        }

        public static IQStreamable<TPayload> Create(Application cepApplication, Type sourceFactoryType, object configInfo, EventShape eventShape, AdvanceTimeSettings advanceTimeSettings) {
            var observable = CreateObservable(cepApplication, sourceFactoryType, configInfo, eventShape);

            switch (eventShape) {
                case EventShape.Point:
                    return observable.ToPointStreamable(e => e.GetPointEvent(), advanceTimeSettings);
                case EventShape.Interval:
                    return observable.ToIntervalStreamable(e => e.GetIntervalEvent(), advanceTimeSettings);
                case EventShape.Edge:
                    return observable.ToEdgeStreamable(e => e.GetEdgeEvent(), advanceTimeSettings);
                default:
                    throw new ArgumentOutOfRangeException("eventShape");
            }
        }
    }
}
