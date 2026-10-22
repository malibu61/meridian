using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
namespace Meridian.Editor
{
    /// <summary>Opt-in command-line orchestrator. Never auto-starts in the user's editor.</summary>
    public static class SmallCraftBatchReview
    {
        private static int phase{get=>SessionState.GetInt("Meridian.BatchPhase",0);set=>SessionState.SetInt("Meridian.BatchPhase",value);}
        private static double deadline{get=>SessionState.GetFloat("Meridian.BatchDeadline",0);set=>SessionState.SetFloat("Meridian.BatchDeadline",(float)value);}
        [InitializeOnLoadMethod] private static void Register(){if(SessionState.GetBool("Meridian.BatchActive",false)){EditorApplication.update-=Tick;EditorApplication.update+=Tick;}}
        public static void CheckSources(){HelmControlReview.RunChecks();SmallCraftControlReview.RunChecks();SmallCraftReview.RunChecks();Debug.Log("SMALLCRAFT_SOURCE_COMPILE_PASS");}
        public static void Run()
        {
            try
            {
                if(!Application.isBatchMode)throw new InvalidOperationException("Command-line validation only; use individual review menus in the interactive editor.");
                SessionState.SetBool("Meridian.BatchActive",true);SessionState.SetString("Meridian.BatchEpoch",DateTime.UtcNow.Ticks.ToString());
                foreach(string folder in new[]{"SmallCraftReview","SmallCraftControls","ShipHandlingReview"})Directory.CreateDirectory("Temp/"+folder);
                File.WriteAllText("Temp/SmallCraftReview/runtime-result.txt","RUNNING\n");File.WriteAllText("Temp/SmallCraftControls/runtime-result.txt","RUNNING\n");File.WriteAllText("Temp/ShipHandlingReview/result.txt","RUNNING\n");
                EditorSceneManager.OpenScene("Assets/Scenes/Main.unity");
                HelmControlReview.RunChecks();SmallCraftControlReview.RunChecks();SmallCraftReview.RunChecks();
                phase=0;deadline=EditorApplication.timeSinceStartup+600;ShipHandlingReview.StartReview();
                EditorApplication.update-=Tick;EditorApplication.update+=Tick;
            }
            catch(Exception e){Debug.LogException(e);ArchiveOutputs();EditorApplication.Exit(1);}
        }
        private static void ArchiveOutputs()
        {
            foreach(string folder in new[]{"SmallCraftReview","SmallCraftControls","ShipHandlingReview","HelmControlReview"})
            {
                string source="Temp/"+folder,destination="Logs/SmallCraftValidation/"+folder;
                if(!Directory.Exists(source))continue;Directory.CreateDirectory(destination);
                foreach(string file in Directory.GetFiles(source))File.Copy(file,Path.Combine(destination,Path.GetFileName(file)),true);
            }
        }
        private static bool Pass(string file,string marker)=>File.Exists(file)&&File.ReadAllText(file).Contains(marker);
        private static void Tick()
        {
            try
            {
                if(EditorApplication.timeSinceStartup>deadline)throw new TimeoutException("Batch phase "+phase);
                if(phase==1&&Pass("Temp/SmallCraftReview/runtime-result.txt","FAILED"))throw new InvalidOperationException(File.ReadAllText("Temp/SmallCraftReview/runtime-result.txt"));
                if(phase==2&&Pass("Temp/SmallCraftControls/runtime-result.txt","FAILED"))throw new InvalidOperationException(File.ReadAllText("Temp/SmallCraftControls/runtime-result.txt"));
                if(phase==0)
                {
                    string path="Temp/ShipHandlingReview/result.txt";
                    if(Pass(path,"FAILED"))throw new InvalidOperationException(File.ReadAllText(path));
                    if(Pass(path,"SHIP_HANDLING_PASS")){phase=1;SmallCraftReview.StartReview();}
                }
                else if(phase==1&&Pass("Temp/SmallCraftReview/runtime-result.txt","SMALLCRAFT_RUNTIME_PASS")){phase=2;SmallCraftControlReview.StartReview();}
                else if(phase==2&&Pass("Temp/SmallCraftControls/runtime-result.txt","FOUR_CONTROL_RUNTIME_PASS")){phase=3;HelmControlReview.StartReview();deadline=EditorApplication.timeSinceStartup+90;}
                else if(phase==3&&Pass("Temp/HelmControlReview/runtime-result.txt","HELM_RUNTIME_CHECK_PASS"))
                {phase=4;deadline=EditorApplication.timeSinceStartup+3;}
                else if(phase==4&&EditorApplication.timeSinceStartup>deadline-1)
                {
                    ArchiveOutputs();File.WriteAllText("Logs/SmallCraftValidation/batch-result.txt","SMALLCRAFT_BATCH_PASS logic_physics_models_only\n");Debug.Log("SMALLCRAFT_BATCH_PASS logic_physics_models_only; UI screenshots require an interactive Game View.");EditorApplication.update-=Tick;EditorApplication.Exit(0);
                }
            }
            catch(Exception e){Debug.LogException(e);ArchiveOutputs();EditorApplication.update-=Tick;EditorApplication.Exit(1);}
        }
    }
}
