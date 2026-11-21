#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace FinalCoat.EditorTools{
    public class ArtImporter : AssetPostprocessor{
        const string ArtFolder = "Assets/FinalCoat/Art/";
        const string FurnitureFolder = ArtFolder + "Furniture/";
        const string BackgroundFolder = ArtFolder + "Background/";
        static readonly Dictionary<string, Vector2> Pivots = new Dictionary<string, Vector2>{
            { "bot1", new Vector2(0.586f, 0.33f) },
            { "bot1_key", new Vector2(0.676f, 0.365f) },
            { "bot2", new Vector2(0.469f, 0.52f) },
            { "bot2_key", new Vector2(0.709f, 0.559f) },
            { "heli_tail_rotor", new Vector2(0.924f, 0.533f) },
            { "paint_drop", new Vector2(0.414f, 0.521f) }
        };
        static readonly Dictionary<string, float> FurniturePixelsPerUnit = new Dictionary<string, float>{
            { "seesaw_plank", 8f },
            { "seesaw_plank_cream", 8f },
            { "table_leg_bead", 8f },
            { "ladder", 8f }
        };
        static readonly Dictionary<string, Vector4> FurnitureBorders = new Dictionary<string, Vector4>{
            { "tabletop", new Vector4(3f, 0f, 3f, 0f) },
            { "seesaw_plank", new Vector4(4f, 0f, 4f, 0f) },
            { "seesaw_plank_cream", new Vector4(4f, 0f, 4f, 0f) }
        };
        void OnPreprocessTexture(){
            if (!assetPath.StartsWith(ArtFolder)) return;
            var importer = (TextureImporter)assetImporter;
            string fileName = Path.GetFileNameWithoutExtension(assetPath);
            bool furniture = assetPath.StartsWith(FurnitureFolder);
            bool background = assetPath.StartsWith(BackgroundFolder);
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = 256;
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true;
            importer.filterMode = background || fileName.StartsWith("paintbrush") || fileName.StartsWith("logo") ? FilterMode.Bilinear : FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            if (furniture){
                float ppu;
                importer.spritePixelsPerUnit = FurniturePixelsPerUnit.TryGetValue(fileName, out ppu) ? ppu : 2f;
                importer.wrapMode = TextureWrapMode.Clamp;
            }
            if (background) importer.spritePixelsPerUnit = 100f;
            var settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);
            Vector2 pivot;
            if (Pivots.TryGetValue(fileName, out pivot)){
                settings.spriteAlignment = (int)SpriteAlignment.Custom;
                settings.spritePivot = pivot;
            }
            else settings.spriteAlignment = (int)SpriteAlignment.Center;
            if (furniture) settings.spriteMeshType = SpriteMeshType.FullRect;
            importer.SetTextureSettings(settings);
            Vector4 border;
            if (furniture && FurnitureBorders.TryGetValue(fileName, out border)) importer.spriteBorder = border;
        }
    }
}
#endif
