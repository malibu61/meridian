using System;
using UnityEngine;
namespace Meridian
{
    public interface IMissionProgressStorage{string Read(string key);void Write(string key,string value);}
    public sealed class PlayerPrefsMissionStorage:IMissionProgressStorage
    {
        public string Read(string key)=>PlayerPrefs.GetString(key,"");
        public void Write(string key,string value){PlayerPrefs.SetString(key,value);PlayerPrefs.Save();}
    }
    /// <summary>Stable mission IDs, per-vessel namespace and monotonic completion; no voyage-position save.</summary>
    public sealed class MissionProgressStore
    {
        [Serializable] public sealed class Entry{public string id;public bool completed;public int stars,attempts;public float bestSeconds;}
        [Serializable] private sealed class SaveData{public int version=1;public string lastMissionId;public Entry[] entries;}
        private readonly IMissionProgressStorage storage;private readonly string key;private SaveData data;
        public bool SaveFailed{get;private set;}
        public VesselId Vessel{get;}
        public bool AllScenariosUnlocked{get;}
        public MissionProgressStore(IMissionProgressStorage storage,VesselId vessel=VesselId.Kumkale,bool unlockAll=false)
        {
            this.storage=storage??throw new ArgumentNullException(nameof(storage));Vessel=vessel;AllScenariosUnlocked=unlockAll;key="Meridian.Career.v1."+vessel;
            try{string json=storage.Read(key);data=string.IsNullOrEmpty(json)?null:JsonUtility.FromJson<SaveData>(json);}catch{data=null;}
            if(data==null||data.version!=1)data=new SaveData();
            var clean=new Entry[DockingMissionCatalog.Count];
            for(int i=0;i<clean.Length;i++)
            {
                string id=DockingMissionCatalog.Get(Vessel,i).Id;Entry found=data.entries==null?null:Array.Find(data.entries,e=>e!=null&&e.id==id);
                clean[i]=new Entry{id=id,completed=found?.completed??false,stars=Mathf.Clamp(found?.stars??0,0,3),attempts=Mathf.Clamp(found?.attempts??0,0,100000),bestSeconds=found!=null&&float.IsFinite(found.bestSeconds)&&found.bestSeconds>0?found.bestSeconds:0};
                // Testing permits out-of-order results. Preserve them even when production locking returns;
                // IsUnlocked checks ALL predecessors, so these results cannot bypass a missing earlier lesson.
            }
            data.entries=clean;if(DockingMissionCatalog.IndexOf(Vessel,data.lastMissionId)<0)data.lastMissionId=clean[0].id;
        }
        public int CompletedCount{get{int n=0;foreach(var e in data.entries)if(e.completed)n++;return n;}}
        public int ContinueIndex{get{int index=DockingMissionCatalog.IndexOf(Vessel,data.lastMissionId);if(index>=0&&IsUnlocked(index)&&!IsCompleted(index))return index;for(int i=0;i<data.entries.Length;i++)if(!IsCompleted(i)&&IsUnlocked(i))return i;return data.entries.Length-1;}}
        public bool IsUnlocked(int index){if(index<0||index>=data.entries.Length)return false;if(AllScenariosUnlocked)return true;for(int i=0;i<index;i++)if(!data.entries[i].completed)return false;return true;}
        public bool IsCompleted(int index)=>index>=0&&index<data.entries.Length&&data.entries[index].completed;
        public int Stars(int index)=>index>=0&&index<data.entries.Length?data.entries[index].stars:0;
        public float BestSeconds(int index)=>index>=0&&index<data.entries.Length?data.entries[index].bestSeconds:0;
        public bool Start(int index)
        {if(!IsUnlocked(index))return false;data.lastMissionId=data.entries[index].id;data.entries[index].attempts++;Save();return true;}
        public bool Complete(int index,int stars,float seconds)
        {
            if(!IsUnlocked(index)||stars<1||stars>3||!float.IsFinite(seconds)||seconds<=0)return false;
            Entry e=data.entries[index];e.completed=true;e.stars=Mathf.Max(e.stars,stars);if(e.bestSeconds<=0||seconds<e.bestSeconds)e.bestSeconds=seconds;
            data.lastMissionId=data.entries[Mathf.Min(index+1,data.entries.Length-1)].id;Save();return true;
        }
        private void Save(){try{storage.Write(key,JsonUtility.ToJson(data));SaveFailed=false;}catch{SaveFailed=true;Debug.LogWarning("Görev ilerlemesi kaydedilemedi. Cihaz depolamasını kontrol edin.");}}
    }
}
