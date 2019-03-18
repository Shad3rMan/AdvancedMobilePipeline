using UnityEditor;
using UnityEngine;

namespace Postprofessor
{
    public class MaxSizeProcessor : TextureProcessor
    {
        private int _maxSize;

        public override void Process(Texture2D texture, AssetPostprocessor assetPostprocessor)
        {
            
        }

        public override void DrawGui()
        {
            _maxSize = EditorGUILayout.IntField("MaxSize: ", _maxSize);
        }
    }
}