using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Meridian.Editor
{
    public static class ProjectInitializer
    {
        private const string ScenePath = "Assets/Scenes/Main.unity";

        [MenuItem("Meridian/Prototype Sahnesini Kur")]
        public static void SetupPrototype()
        {
            Directory.CreateDirectory("Assets/Scenes");
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            new GameObject("Meridian Prototype").AddComponent<PrototypeBootstrap>();
            EditorSceneManager.SaveScene(scene, ScenePath);
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };

            PlayerSettings.companyName = "Meridian Studio";
            PlayerSettings.productName = "Meridian";
            PlayerSettings.defaultScreenWidth = 1920;
            PlayerSettings.defaultScreenHeight = 1080;
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.LandscapeLeft;

            AssetDatabase.SaveAssets();
            Debug.Log("Meridian prototip sahnesi hazır: " + ScenePath);
        }
    }
}
