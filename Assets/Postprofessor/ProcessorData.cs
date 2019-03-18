using System;
using System.Collections.Generic;

namespace Postprofessor
{
    [Serializable]
    public class ProcessorData
    {
        public List<ConstructorParameter> Params;
        public Type Type;
    }
}