using System.IO;
using UnityEditor;
using UnityEngine;

namespace Postprofessor
{
    public class TexturePreprocessor : AssetPostprocessor
    {
        private void OnPreprocessTexture()
        {
            
        }
        
        private void OnPostprocessTexture(Texture2D texture)
        {
            TextureProcessor tp = new MaxSizeProcessor();
            tp.Process(texture, this);
        }

        private void OnPreprocessModel()
        {
            
        }

        private void OnPostprocessModel(GameObject g)
        {
            
        }

        private void OnPreprocessAsset()
        {
            
        }

        private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets,
            string[] movedFromAssetPaths)
        {
            
        }

        private void OnPreprocessAudio()
        {
            
        }

        private void OnPostprocessAudio(AudioClip arg)
        {
            
        }

        private void OnPreprocessAnimation()
        {
             
        }

        private void OnPostprocessAnimation(GameObject root, AnimationClip clip)
        {
            
        }

        private void OnPostprocessMaterial(Material material)
        {
            
        }

        private void OnPostprocessCubemap(Cubemap texture)
        {
            
        }

        private void OnPostprocessSprites(Texture2D texture, Sprite[] sprites)
        {
            
        }
    }
}