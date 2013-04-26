using System;

namespace SiDualMode.OutputAdapter.ConsoleAdapter {
    //Configuration used by the factory
    public class ConsoleOutputConfig {
        public bool ShowCti { get; set; }
        public ConsoleColor InsertEventColor { get; set; }
        public ConsoleColor CtiEventColor { get; set; }
        public bool ShowPayloadToString { get; set; }
    }
}
