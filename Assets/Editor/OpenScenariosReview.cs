using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object=UnityEngine.Object;
namespace Meridian.Editor
{
    public static class OpenScenariosReview
    {
        private const string Folder="Logs/OpenScenariosReview";private static int checks,index=-1;private static double deadline;
        private sealed class Memory:IMissionProgressStorage
        {public Dictionary<string,string> Data=new();public string Read(string k)=>Data.TryGetValue(k,out var value)?value:"";public void Write(string k,string v)=>Data[k]=v;}
        private static void Check(bool ok,string name){checks++;if(!ok)throw new InvalidOperationException("Open scenarios: "+name);}
        public static void CheckRules()
        {
            Directory.CreateDirectory(Folder);checks=0;Check(MissionAccessPolicy.AllScenariosUnlockedForTesting,"testing policy enabled");
            foreach(var vessel in VesselCatalog.All)
            {
                var m=new Memory();var open=new MissionProgressStore(m,vessel,true);
                Check(open.CompletedCount==0&&!open.IsUnlocked(-1)&&!open.IsUnlocked(DockingMissionCatalog.Count),"valid bounds; no fake completion");
                for(int i=5;i>=0;i--)
                {
                    Check(open.IsUnlocked(i)&&open.Start(i),"fresh arbitrary start "+vessel+i);
                    Check(open.CompletedCount==0&&!open.IsCompleted(i)&&open.Stars(i)==0,"opening does not award results");
                }
                Check(open.Complete(5,3,91),"last mission before first");
                var reload=new MissionProgressStore(m,vessel,true);Check(reload.CompletedCount==1&&reload.IsCompleted(5)&&reload.Stars(5)==3&&reload.BestSeconds(5)==91&&!reload.IsCompleted(0),"out of order result survives reload");
                Check(reload.ContinueIndex==0,"continue chooses missing first, not completed count");
                reload.Complete(5,1,200);Check(new MissionProgressStore(m,vessel,true).Stars(5)==3&&reload.BestSeconds(5)==91,"best result never reduced");
                var production=new MissionProgressStore(m,vessel,false);
                Check(production.IsUnlocked(0)&&!production.IsUnlocked(1)&&!production.IsUnlocked(5),"production restores gates");
                Check(production.IsCompleted(5)&&production.Stars(5)==3&&production.BestSeconds(5)==91,"production does not erase test results");
                for(int i=0;i<5;i++)Check(production.Complete(i,1,100+i),"sequential production progression");
                Check(production.IsUnlocked(5)&&production.CompletedCount==6,"chain catches up safely");
            }
            MissionCareerReview.CheckSources();FleetCareerReview.CheckRules();
            File.WriteAllText(Folder+"/rules.txt",$"OPEN_SCENARIOS_RULES_PASS checks={checks} missions=24 out_of_order_save_preserved production_gates_retained\n");Debug.Log($"OPEN_SCENARIOS_RULES_PASS checks={checks}");
        }
        public static void RunBatch(){CheckRules();EditorSceneManager.OpenScene("Assets/Scenes/Main.unity");SessionState.SetBool("Meridian.OpenScenariosReview",true);EditorApplication.isPlaying=true;}
        [InitializeOnLoadMethod]private static void Register(){EditorApplication.update-=Tick;EditorApplication.update+=Tick;}
        private static void Tick()
        {
            if(!Application.isPlaying||EditorApplication.isCompiling)return;var session=Object.FindAnyObjectByType<GameSessionController>();if(session==null)return;
            if(SessionState.GetBool("Meridian.OpenScenariosReview",false)){SessionState.SetBool("Meridian.OpenScenariosReview",false);session.Missions.SetStorageForReview(new Memory());index=0;checks=0;deadline=EditorApplication.timeSinceStartup+120;}
            if(index<0)return;
            try
            {
                if(EditorApplication.timeSinceStartup>deadline)throw new TimeoutException("24 scenario start check");
                if(index<24)
                {
                    var vessel=VesselCatalog.All[index/6];int mission=5-index%6;
                    Check(session.Missions.GetProgress(vessel).IsUnlocked(mission),"actual controller unlock "+vessel+mission);
                    Check(session.Missions.StartMission(vessel,mission),"actual reverse-order scenario start "+vessel+mission);
                    Check(session.Missions.Index==mission&&session.ActiveVessel==vessel&&session.Missions.Run.Phase==DockingMissionPhase.Briefing,"correct briefing "+vessel+mission);
                    Check(session.Missions.Progress.CompletedCount==0,"access alone never counts completion");index++;return;
                }
                File.WriteAllText(Folder+"/runtime.txt",$"OPEN_SCENARIOS_RUNTIME_PASS checks={checks} actual_scenario_starts=24 fresh_in_memory_progress\n");Debug.Log($"OPEN_SCENARIOS_RUNTIME_PASS checks={checks}");index=-1;EditorApplication.Exit(0);
            }
            catch(Exception ex){File.WriteAllText(Folder+"/runtime.txt","FAILED\n"+ex);Debug.LogException(ex);index=-1;EditorApplication.Exit(1);}
        }
    }
}
