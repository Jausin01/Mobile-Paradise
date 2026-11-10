using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

namespace MobileParadise.EditorTools
{
    /// <summary>
    /// Applies the project-wide Android/Player settings in one click.
    /// Rule: every game is played with the phone upright (portrait).
    /// </summary>
    public static class ProjectSettingsApplier
    {
        private const string IconPath = "Assets/Core/Art/app_icon.png";

        [MenuItem("Mobile Paradise/3. Apply Project Settings")]
        public static void Apply()
        {
            PlayerSettings.productName = "Mobile Paradise";
            PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android, "com.jausin.mobileparadise");

            // Portrait only, no auto-rotation.
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;

            // Old phones: Android 7.0+, 32-bit and 64-bit ARM.
            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel26;
            PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARMv7 | AndroidArchitecture.ARM64;

            ApplyIcon();

            AssetDatabase.SaveAssets();
            Debug.Log("Mobile Paradise: project settings applied (name, icon, portrait, Android 8+, IL2CPP, ARMv7+ARM64).");
        }

        // The picture shown on the phone's home screen. One image is used for every icon size.
        private static void ApplyIcon()
        {
            TextureImporter importer = AssetImporter.GetAtPath(IconPath) as TextureImporter;
            if (importer == null)
            {
                Debug.LogWarning("App icon not found: " + IconPath);
                return;
            }

            importer.textureType = TextureImporterType.Default;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.mipmapEnabled = false;
            importer.SaveAndReimport();

            Texture2D icon = AssetDatabase.LoadAssetAtPath<Texture2D>(IconPath);
            PlayerSettings.SetIcons(NamedBuildTarget.Unknown, new Texture2D[] { icon }, IconKind.Application);
        }
    }
}
