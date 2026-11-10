using UnityEditor;
using UnityEngine;

namespace MobileParadise.EditorTools
{
    /// <summary>
    /// Any new texture inside an "Art" folder is imported as crisp pixel art:
    /// Sprite, 32 pixels per unit, Point filter, no compression, no mipmaps.
    /// Files named "ui_*.png" are 9-sliced frames (6 px borders).
    /// The automatic pass runs only on first import, so manual changes in the Inspector are kept.
    /// </summary>
    public class PixelArtImporter : AssetPostprocessor
    {
        public static void ApplySettings(TextureImporter importer)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = 32;
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true;

            if (System.IO.Path.GetFileName(importer.assetPath).StartsWith("ui_"))
            {
                importer.spriteBorder = new Vector4(6, 6, 6, 6);
            }
        }

        private void OnPreprocessTexture()
        {
            if (!assetPath.Contains("/Art/")) return;

            TextureImporter importer = (TextureImporter)assetImporter;
            if (!importer.importSettingsMissing) return;

            ApplySettings(importer);
        }
    }
}
