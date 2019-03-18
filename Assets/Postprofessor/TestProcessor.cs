using UnityEngine;

namespace Postprofessor
{
    public class TestProcessor
    {
        public TestProcessor(int intField)
        {
                
        }
        
        public TestProcessor()
        {
            Debug.Log("TestProcessor ctor");
        }
        
        public void Process()
        {
            Debug.Log("TestProcessor Process");
        }
    }
}