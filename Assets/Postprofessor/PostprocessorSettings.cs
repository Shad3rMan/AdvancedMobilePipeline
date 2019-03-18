using System.Collections.Generic;
using UnityEngine;

namespace Postprofessor
{
    [System.Serializable]
    public class PostprocessorSettings
    {
        public List<ProcessorData> Data;

        public PostprocessorSettings()
        {
            Data = new List<ProcessorData>();
        }

        public void AddProcessorData(ProcessorData data)
        {
            Data.Add(data);
        }
    }
}