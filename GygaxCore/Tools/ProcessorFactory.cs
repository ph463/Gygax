using System;
using System.CodeDom;
using System.Collections.Generic;
using GygaxCore.Processors;

namespace GygaxCore.DataStructures
{

    public sealed class ProcessorFactory
    {
        private static readonly Lazy<ProcessorFactory> lazy =
            new Lazy<ProcessorFactory>(() => new ProcessorFactory());

        public static ProcessorFactory Instance { get { return lazy.Value; } }

        public List<string> Processors = new List<string>();

        private ProcessorFactory()
        {
            Processors.Add("Edge Detector");
            Processors.Add("Threshold");
            Processors.Add("My Method");
        }

        public Processor GetProcessor(string name)
        {
            switch (name)
            {
                case "Edge Detector":
                    return new EdgeDetector();
                case "Threshold":
                    return new Threshold();
                case "My Method":
                    return new MyMethod();
                default:
                    throw new NotSupportedException();
            }
        }
    }
}