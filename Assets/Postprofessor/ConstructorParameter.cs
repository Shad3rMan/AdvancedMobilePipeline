using System;
using UnityEngine;

namespace Postprofessor
{
    [System.Serializable]
    public struct ConstructorParameter
    {
        [SerializeField]
        private Type _type;

        [SerializeField]
        private object _value;

        [SerializeField]
        private string _name;

        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        public object Value
        {
            get { return _value; }
            set { _value = value; }
        }

        public Type Type
        {
            get { return _type; }
            set { _type = value; }
        }
    }
}