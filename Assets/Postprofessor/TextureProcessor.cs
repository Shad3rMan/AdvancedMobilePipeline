using UnityEditor;
using UnityEngine;

namespace Postprofessor
{
    public abstract class TextureProcessor : IProcessor
    {
        public abstract void Process(Texture2D texture, AssetPostprocessor assetPostprocessor);

        public void Process()
        {
        }

        public abstract void DrawGui();
    }
}