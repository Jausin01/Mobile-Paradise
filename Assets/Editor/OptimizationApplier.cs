using UnityEditor;
using UnityEditor.Build;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace MobileParadise.EditorTools
{
    /// <summary>
    /// One click that applies the build-size and speed settings. Safe to run many times.
    /// Visible quality stays the same: music keeps its stereo/44.1 kHz format, only the
    /// bitrate is lowered to a level that is not audible for background music.
    /// </summary>
    public static class OptimizationApplier
    {
        private const string MusicFolder = "Assets/Core/Resources/Audio/Music";
        private const string UrpAssetPath = "Assets/Settings/UniversalRP.asset";
        private const float MusicQuality = 0.55f;   // Vorbis quality 0..1 (about 160 kbps)

        [MenuItem("Mobile Paradise/5. Apply Optimizations")]
        public static void Apply()
        {
            OptimizeMusic();
            OptimizePlayer();
            OptimizeRenderer();

            AssetDatabase.SaveAssets();
            Debug.Log("Mobile Paradise: optimizations applied (music streaming + Vorbis, no splash, stripping Medium, HDR off).");
        }

        // Music is the biggest part of the APK and of the RAM: stream it instead of
        // unpacking every track in memory, and store it at a sensible bitrate.
        private static void OptimizeMusic()
        {
            string[] guids = AssetDatabase.FindAssets("t:AudioClip", new string[] { MusicFolder });
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                AudioImporter importer = AssetImporter.GetAtPath(path) as AudioImporter;
                if (importer == null) continue;

                AudioImporterSampleSettings settings = importer.defaultSampleSettings;
                settings.loadType = AudioClipLoadType.Streaming;
                settings.compressionFormat = AudioCompressionFormat.Vorbis;
                settings.quality = MusicQuality;
                settings.sampleRateSetting = AudioSampleRateSetting.PreserveSampleRate;
                settings.preloadAudioData = false;

                importer.defaultSampleSettings = settings;
                importer.forceToMono = false;
                importer.SaveAndReimport();
            }
        }

        private static void OptimizePlayer()
        {
            PlayerSettings.companyName = "Jausin";
            PlayerSettings.SplashScreen.show = false;
            PlayerSettings.SetManagedStrippingLevel(NamedBuildTarget.Android, ManagedStrippingLevel.Medium);
        }

        // The games are flat 2D UI: the high-precision (HDR) color buffer only costs GPU time.
        private static void OptimizeRenderer()
        {
            UniversalRenderPipelineAsset asset = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(UrpAssetPath);
            if (asset == null)
            {
                Debug.LogWarning("URP asset not found: " + UrpAssetPath);
                return;
            }

            asset.supportsHDR = false;
            EditorUtility.SetDirty(asset);
        }
    }
}
