using System;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object=UnityEngine.Object;
namespace Meridian.Editor
{
    public static class TugNavigationReview
    {
        private const string Folder="Logs/TugNavigationReview";private static int checks,stage=-1;private static double deadline;
        private static void Check(bool ok,string label){checks++;if(!ok)throw new InvalidOperationException("Tug navigation: "+label);}
        [InitializeOnLoadMethod]private static void Register(){EditorApplication.update-=Tick;EditorApplication.update+=Tick;}
        public static void RunBatch(){Directory.CreateDirectory(Folder);EditorSceneManager.OpenScene("Assets/Scenes/Main.unity");SessionState.SetBool("Meridian.TugNavigationReview",true);EditorApplication.isPlaying=true;}
        private static void Rules()
        {
            var nav=new HarborTugNavigation();var v=VesselCatalog.Get(VesselId.Anadolu);Vector3 ship=new(0,.1f,54);Vector2 other=new(10000,10000);
            Check(nav.WaterClear(new Vector2(0,50))&&!nav.WaterClear(new Vector2(-31,50))&&!nav.WaterClear(new Vector2(33,70)),"visible quays blocked");
            Check(!nav.WaterSegmentClear(new Vector2(0,50),new Vector2(-60,50)),"sweep cannot cross west wall");
            Check(!nav.WaterSegmentClear(new Vector2(20,115),new Vector2(45,115)),"rotating tug radius protected at east pier corner");
            Check(nav.NextWaypoint(new Vector2(-10,70),new Vector2(10,70),ship,0,v,other,out var waypoint),"route across bordas exists");
            Check(Mathf.Abs(waypoint.y-54)>v.LengthMetres/8,"around ship end, not through hull");
            Check(!nav.NextWaypoint(new Vector2(0,50),new Vector2(40,70),ship,0,v,other,out _),"land goal rejected");
            Check(!HarborTugNavigation.HullPoseClear(new Vector2(0,54),0,ship,0,v),"own ship hull exclusion");
            foreach(var id in new[]{VesselId.Anadolu,VesselId.Barbaros})foreach(int sign in new[]{-1,1})for(int i=0;i<2;i++)
            {
                var d=VesselCatalog.Get(id);Vector3 p=ship+TugAssistRules.LocalStation(i,sign,d);
                Check(HarborTugNavigation.HullPoseClear(HarborTugNavigation.Flat(p),sign*90,ship,0,d),"contact fender outside sampled hull "+id+sign+i);
            }
            // Validate that the planner's harbour boundaries still match the actual scene geometry.
            string[] names={"West coast","East coast","West breakwater cap","East pier cap","Harbor wall"};
            for(int i=0;i<names.Length;i++)
            {
                var go=GameObject.Find(names[i]);Check(go!=null,"scene geometry "+names[i]);var r=HarborTugNavigation.Terrain[i];
                Check(Mathf.Abs(go.transform.position.x-r.center.x)<.01f&&Mathf.Abs(go.transform.position.z-r.center.y)<.01f&&Mathf.Abs(go.transform.localScale.x-r.width)<.01f&&Mathf.Abs(go.transform.localScale.z-r.height)<.01f,"navigation geometry sync "+names[i]);
            }
            TugAssistReview.CheckSources();File.WriteAllText(Folder+"/rules.txt",$"TUG_NAV_RULES_PASS checks={checks} terrain_caps_sweeps own_hull_end_route contact_geometry\n");
        }
        private sealed class Fixture:IDisposable
        {
            public Scene Scene;public ShipController Ship;public Rigidbody Body;public GameSessionController Session;public TugAssistController Tugs;
            public Fixture(VesselId id,HarborBerth berth,Vector3 position)
            {
                Scene=SceneManager.CreateScene("Tug navigation controlled fixture",new CreateSceneParameters(LocalPhysicsMode.Physics3D));
                var go=new GameObject("Navigation test ship");SceneManager.MoveGameObjectToScene(go,Scene);var v=VesselCatalog.Get(id);var box=go.AddComponent<BoxCollider>();box.size=v.ColliderSize;box.center=v.ColliderCenter;
                Ship=go.AddComponent<ShipController>();Ship.ConfigureVessel(v);Ship.enabled=false;Body=go.GetComponent<Rigidbody>();Body.interpolation=RigidbodyInterpolation.None;Ship.ResetSimulation(position,Quaternion.identity);
                var host=new GameObject("Navigation test session");SceneManager.MoveGameObjectToScene(host,Scene);Session=host.AddComponent<GameSessionController>();Session.enabled=false;typeof(GameSessionController).GetProperty("HasStarted").SetValue(Session,true);
                Tugs=host.AddComponent<TugAssistController>();Tugs.enabled=false;Tugs.Initialize(Session,Ship,null);Tugs.BeginMode(SimulationMode.Harbor,id);Tugs.SelectBerth(berth);WaterWaves.Reset(new SeaStateSettings(1,0),0);
            }
            public void SetPose(Vector3 p,float heading,Vector3 velocity){Ship.ResetSimulation(p,Quaternion.Euler(0,heading,0));Body.linearVelocity=velocity;}
            public void Dispose()=>SceneManager.UnloadSceneAsync(Scene);
        }
        private static void EscortCase(VesselId id,bool lateral)
        {
            using var f=new Fixture(id,HarborBerth.East,new Vector3(0,.1f,lateral?54:-60));
            if(!lateral){f.Tugs.Toggle(0);f.Tugs.Toggle(1);}Vector3 origin=f.Body.position,a=f.Tugs.TugPosition(0),b=f.Tugs.TugPosition(1);
            var nav=new HarborTugNavigation();Vector3 velocity=lateral?new Vector3(-.13f,0,0):new Vector3(.04f,0,16.5f/(4*1.9438445f));
            for(int step=0;step<1500;step++)
            {
                Vector3 p=origin+velocity*(step*.02f);f.SetPose(p,0,velocity);f.Tugs.SimulateStep(.02f,step*.02f);
                for(int i=0;i<2;i++)
                {
                    Vector3 tug=f.Tugs.TugPosition(i);Check(nav.WaterClear(HarborTugNavigation.Flat(tug)),"escort quay clearance "+id+lateral+step+i);
                    Check(HarborTugNavigation.HullPoseClear(HarborTugNavigation.Flat(tug),f.Tugs.TugRotation(i).eulerAngles.y,p,0,f.Ship.Definition),"escort outside hull");
                    if(!lateral)Check(f.Tugs.Delivered(i)==0,"escort above five knots is NOT pushing");
                }
            }
            Vector3 delta=f.Body.position-origin;Vector3 ta=f.Tugs.TugPosition(0)-a,tb=f.Tugs.TugPosition(1)-b;
            for(int i=0;i<2;i++)
            {Vector3 expected=f.Body.position+TugAssistRules.LocalStation(i,1,f.Ship.Definition)-Vector3.right*HarborTugNavigation.StandOff;Check(Vector2.Distance(HarborTugNavigation.Flat(expected),HarborTugNavigation.Flat(f.Tugs.TugPosition(i)))<.4f,"moving formation slot reached "+id+lateral+i);}
            Check(lateral||Mathf.Abs(ta.z-delta.z)<1&&Mathf.Abs(tb.z-delta.z)<1,"both tugs inherit surge without parent "+id);
            for(int i=0;i<2;i++)Check(!f.Tugs.transform.IsChildOf(f.Ship.transform),"independent fleet transform");
            Vector3 before=f.Tugs.TugPosition(0);typeof(GameSessionController).GetProperty("MenuOpen").SetValue(f.Session,true);f.Tugs.SimulateStep(.2f,32);Check(f.Tugs.TugPosition(0)==before&&f.Tugs.Delivered(0)==0,"menu freezes escort and forces");
            File.AppendAllText(Folder+"/cases.txt",$"escort {id} lateral={lateral} ship_delta={delta} bow_delta={ta} stern_delta={tb}\n");
        }
        private static void ShoreCase(VesselId id,bool west)
        {
            // Deliberately steer toward the OPPOSITE quay from the stated berth; tugs must yield automatically.
            using var f=new Fixture(id,west?HarborBerth.East:HarborBerth.West,new Vector3(0,.1f,west?54:70));
            f.Tugs.Toggle(0);f.Tugs.Toggle(1);var nav=new HarborTugNavigation();int sign=west?-1:1;Vector3 origin=f.Body.position;bool escaping=false;
            for(int n=0;n<6000;n++)
            {
                float x=sign*Mathf.Min(22,n*.02f*.18f);Vector3 p=origin+Vector3.right*x;Vector3 velocity=Mathf.Abs(x)<22?Vector3.right*(sign*.18f):Vector3.zero;
                f.SetPose(p,0,velocity);Vector3 pa=f.Tugs.TugPosition(0),pb=f.Tugs.TugPosition(1);f.Tugs.SimulateStep(.02f,n*.02f);
                for(int i=0;i<2;i++)
                {
                    Vector3 point=f.Tugs.TugPosition(i);Check(nav.WaterClear(HarborTugNavigation.Flat(point)),"never enters visible quay "+id+west+n+i);
                    Check(nav.WaterSegmentClear(HarborTugNavigation.Flat(i==0?pa:pb),HarborTugNavigation.Flat(point)),"never tunnels through quay "+id+west+n+i);
                    Check(HarborTugNavigation.HullPoseClear(HarborTugNavigation.Flat(point),f.Tugs.TugRotation(i).eulerAngles.y,p,0,f.Ship.Definition),"withdrawal remains outside moving ship "+id+west+n+i);
                    if(f.Tugs.ClearingShore(i)){escaping=true;Check(f.Tugs.Delivered(i)==0,"withdrawal never pushes across hull");}
                }
                if(n%1000==0)File.AppendAllText(Folder+"/cases.txt",$"withdraw live {id} west={west} n={n} ship={f.Body.position} bow={f.Tugs.TugPosition(0)} stern={f.Tugs.TugPosition(1)} escape={f.Tugs.ClearingShore(0)}/{f.Tugs.ClearingShore(1)} waiting={f.Tugs.WaitingForRoute(0)}/{f.Tugs.WaitingForRoute(1)}\n");
            }
            Check(escaping,"anticipatory shore withdrawal observed "+id+west);
            for(int i=0;i<2;i++)Check((f.Tugs.TugPosition(i).x-f.Body.position.x)*-sign>f.Ship.Definition.BeamMetres/8,"both escape to seaward borda "+id+west+i);
            File.AppendAllText(Folder+"/cases.txt",$"withdraw {id} west={west} ship={f.Body.position} bow={f.Tugs.TugPosition(0)} stern={f.Tugs.TugPosition(1)}\n");
        }
        private static void Tick()
        {
            if(!Application.isPlaying||EditorApplication.isCompiling)return;var session=Object.FindAnyObjectByType<GameSessionController>();if(session==null)return;
            if(SessionState.GetBool("Meridian.TugNavigationReview",false)){SessionState.SetBool("Meridian.TugNavigationReview",false);stage=0;checks=0;deadline=EditorApplication.timeSinceStartup+240;File.WriteAllText(Folder+"/cases.txt","");}
            if(stage<0)return;
            try
            {
                if(EditorApplication.timeSinceStartup>deadline)throw new TimeoutException("Navigation stage "+stage);
                if(stage==0){Rules();stage++;return;}
                if(stage<=4){int n=stage-1;EscortCase(n<2?VesselId.Anadolu:VesselId.Barbaros,n%2==1);stage++;return;}
                if(stage<=8){int n=stage-5;ShoreCase(n<2?VesselId.Anadolu:VesselId.Barbaros,n%2==0);stage++;return;}
                if(stage<=14)
                {typeof(TugAssistReview).GetMethod("PhysicsCase",BindingFlags.NonPublic|BindingFlags.Static).Invoke(null,new object[]{stage-9});stage++;return;}
                File.WriteAllText(Folder+"/runtime.txt",$"TUG_NAV_RUNTIME_PASS checks={checks} escort_cases=4 withdrawal_cases=4 original_force_cases=6\n");Debug.Log($"TUG_NAV_RUNTIME_PASS checks={checks}");stage=-1;EditorApplication.Exit(0);
            }
            catch(Exception ex){File.WriteAllText(Folder+"/runtime.txt","FAILED stage="+stage+"\n"+ex);Debug.LogException(ex);stage=-1;EditorApplication.Exit(1);}
        }
    }
}
