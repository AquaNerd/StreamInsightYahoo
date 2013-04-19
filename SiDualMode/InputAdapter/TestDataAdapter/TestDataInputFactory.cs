using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.ComplexEventProcessing;
using Microsoft.ComplexEventProcessing.Adapters;

using SiDualMode.Base;
using SiDualMode.Base.Input;

namespace SiDualMode.InputAdapter.TestDataAdapter {
    /// <summary>
    /// Factory to instantiate a data generator input adapter.
    /// </summary>
    public sealed class TestDataInputFactory : ITypedInputAdapterFactory<TestDataInputConfig>, ISourceFactory {
        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Justification = "By Design")]
        public InputAdapterBase Create<TPayload>(TestDataInputConfig configInfo, EventShape eventShape) {
            CheckPayloadType<TPayload>();
            return new ObservableTypedPointInputAdapter<TestDataEvent, TestDataInputConfig>(
                CreateProducer(configInfo, eventShape));
        }

        public IObservable<StreamInputEvent<TPayload>> CreateObservableSource<TPayload>(object config, EventShape eventShape) {
            //Check the payload type.
            CheckPayloadType<TPayload>();
            //Check config class for the proper type.
            TestDataInputConfig typedConfig = config as TestDataInputConfig;
            if (typedConfig == null) {
                //Invalid cast
                throw new ArgumentException("Configuration class must be of type TestDataInputConfig");
            }
            return (IObservable<StreamInputEvent<TPayload>>)CreateProducer(typedConfig, eventShape);
        }

        /// <summary>
        /// Dispose method
        /// </summary>
        public void Dispose() {
        }

        private static void CheckPayloadType<TPayload>() {
            //Check the payload.
            if (typeof(TPayload) != typeof(TestDataEvent)) {
                //this won't work.
                //throw an exception.
                throw new InvalidOperationException("Specified type must be of " + typeof(TestDataEvent).FullName);
            }
        }

        private TestDataProducer CreateProducer(TestDataInputConfig config, EventShape eventShape) {
            switch (eventShape) {
                case EventShape.Point:
                    //Create the publisher.
                    return new TestDataProducer(config);
                default:
                    throw new ArgumentException(string.Format(
                        System.Globalization.CultureInfo.InvariantCulture,
                        "TestDataInputFactory cannot instantiate adapter with event shape {0}",
                        eventShape.ToString()));
            }
        }
    }
}
