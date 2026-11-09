#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace FinalCoat.EditorTools{
    public class ArtImporter : AssetPostprocessor{
        const string ArtFolder = "Assets/FinalCoat/Art/";
        static readonly Dictionary<string, Vector2> Pivots = new Dictionary<string, Vector2>{
            { "bot1", new Vector2(0.586f, 0.33f) },
            { "bot1_key", new Vector2(0.676f, 0.365f) },
            { "bot2", new Vector2(0.469f, 0.52f) },
            { "bot2_key", new Vector2(0.709f, 0.559f) },
            { "heli_tail_rotor", new Vector2(0.924f, 0.533f) },
            { "paint_drop", new Vector2(0.414f, 0.521f) }
        };
        void OnPreprocessTexture(){
            if (!assetPath.StartsWith(ArtFolder)) return;
            var importer = (TextureImporter)assetImporter;
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = 256;
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true;
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            var settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);
            Vector2 pivot;
            if (Pivots.TryGetValue(Path.GetFileNameWithoutExtension(assetPath), out pivot)){
                settings.spriteAlignment = (int)SpriteAlignment.Custom;
                settings.spritePivot = pivot;
            }
            else settings.spriteAlignment = (int)SpriteAlignment.Center;
            importer.SetTextureSettings(settings);
        }
    }
}
#endif
