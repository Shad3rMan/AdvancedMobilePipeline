using System;

namespace Postprofessor
{
    public class ProcessorFactory
    {
        public static IProcessor GetProcessor(ProcessorData data)
        {
            return (IProcessor)Activator.CreateInstance(data.Type, data.Params);
        }
    }
}