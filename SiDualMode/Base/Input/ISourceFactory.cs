using System;
using Microsoft.ComplexEventProcessing;

using SiDualMode.Base;

namespace SiDualMode.Base.Input {
    /// <summary>
    /// Interface for data source factories.
    /// </summary>
    public interface ISourceFactory {
        /// <summary>
        /// Creates an observable source.
        /// </summary>
        /// <typeparam name="TPayload">Type of the payload.</typeparam>
        /// <param name="config">Configuration class.</param>
        /// <param name="eventShape">Shape of the event.</param>
        /// <returns></returns>
        IObservable<StreamInputEvent<TPayload>> CreateObservableSource<TPayload>(object config, EventShape eventShape);
    }
}
