using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.ComplexEventProcessing;

using SiDualMode.Base;

namespace SiDualMode.OutputAdapter.ConsoleAdapter {
    //This is what is initialized and created by the Factory
    public class ConsoleDataConsumer<TPayloadType> : StreamEventConsumer<TPayloadType, ConsoleOutputConfig> {
        public ConsoleDataConsumer(ConsoleOutputConfig configuration)
            : base(configuration) {
        }

        public override void Completed() {
            //Nothing necessary.
        }

        public override void Error(Exception error) {
            Console.WriteLine("Error occured: {0}", error.ToString());
        }

        public override void EventReceived(StreamOutputEvent<TPayloadType> outputEvent) {
            if (outputEvent.EventKind == EventKind.Insert) {
                Console.ForegroundColor = Configuration.InsertEventColor;
                Dictionary<string, object> eventValues = outputEvent.Payload.GetPropertyValues();
                StringBuilder output = new StringBuilder(2048);
                //foreach (var eventValue in eventValues) {
                //    output.Append(eventValue.Key).Append(":").Append(eventValue.Value.ToString()).Append("\t");
                //}
                //use payload tostring method instead of getting all of the properties of the event.
                output = output.Append(outputEvent.Payload.ToString());
                Console.WriteLine("Insert Event Received at {0} \t {1}", outputEvent.StartTime, output.ToString());
            } else if (Configuration.ShowCti) {
                Console.ForegroundColor = Configuration.CtiEventColor;
                Console.WriteLine("CTI Event Received at {0}", outputEvent.StartTime);
            }
            Console.ResetColor();
        }
    }
}
