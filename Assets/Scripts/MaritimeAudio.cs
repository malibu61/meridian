using System;
using System.Collections.Generic;
using UnityEngine;
using static Meridian.MaritimeUiTheme;
namespace Meridian
{
    /// <summary>Original synthesized whistle/sea, baked Turkish TTS, lightweight radio filtering; all platforms use AudioClips.</summary>
    public sealed class MaritimeAudio:MonoBehaviour
    {
        private GameSessionController session;private AudioSource horn,radio,sea;
        private readonly Dictionary<string,AudioClip> clips=new();private readonly Dictionary<WhistleSignal,AudioClip> whistles=new();
        private readonly List<AudioClip> owned=new();private readonly Queue<string> radioQueue=new();
        private EnvironmentResources resources;private MaritimeUiTheme ui;private float clock,hornEnds,radioEnds,nextChatter,subtitleEnds;
        private int chatter;private bool paused,appPaused,settingsOpen;private WhistleSignal currentHorn;
        private float master=.65f;private bool muted;private string subtitle="";private bool missingWarned;
        public event Action<WhistleSignal> HornCompleted;
        public bool HornBusy=>currentHorn!=WhistleSignal.None;
        public bool RadioBusy=>radio!=null&&radio.clip!=null&&clock<radioEnds;
        public string Subtitle=>clock<subtitleEnds?subtitle:"";
        public float MasterVolume=>master;public bool Muted=>muted;
        public bool SettingsOpen=>settingsOpen;
        public string StatusFeedback
        {
            get
            {
                var run=session?.Missions?.Run;
                if(run!=null&&session.Missions.Running&&run.Signals.Expected!=WhistleSignal.None)return $"ZORUNLU DÜDÜK · {ManeuverSignalMonitor.Meaning(run.Signals.Expected)} · {run.Signals.SecondsLeft(run.Elapsed):0} sn";
                if(HornBusy)return "DÜDÜK ÇALIYOR · "+(currentHorn==WhistleSignal.Prolonged?"UZUN":currentHorn==WhistleSignal.Attention?"5 KISA UYARI":$"{(int)currentHorn} KISA")+" · Dizinin bitmesini bekle";
                return Subtitle.Length>0?"TELSİZ · "+Subtitle:"";
            }
        }
        public void Initialize(GameSessionController owner)
        {
            session=owner;resources=gameObject.GetComponent<EnvironmentResources>()??gameObject.AddComponent<EnvironmentResources>();
            horn=Source("Ship whistle",.72f);radio=Source("Radio communications",.60f);sea=Source("Sea ambience",.14f);
            master=Mathf.Clamp01(PlayerPrefs.GetFloat("Meridian.Audio.Volume.v1",.65f));muted=PlayerPrefs.GetInt("Meridian.Audio.Mute.v1",0)==1;
            sea.clip=Own(BuildSea());sea.loop=true;ResetForVoyage();
        }
        private AudioSource Source(string name,float gain){var go=new GameObject(name);go.transform.SetParent(transform,false);var source=go.AddComponent<AudioSource>();source.playOnAwake=false;source.spatialBlend=0;source.volume=gain;source.priority=name=="Ship whistle"?32:64;return source;}
        public static float SignalDuration(WhistleSignal signal)=>signal switch{WhistleSignal.Starboard=>1,WhistleSignal.Port=>3,WhistleSignal.Astern=>5,WhistleSignal.Prolonged=>5,WhistleSignal.Attention=>6.2f,_=>0};
        public bool PlayWhistle(WhistleSignal signal)
        {
            if(session==null||!session.CanControl||HornBusy||SignalDuration(signal)<=0)return false;
            if(!whistles.TryGetValue(signal,out AudioClip clip)){clip=Own(BuildWhistle(signal,session.ActiveVessel));whistles[signal]=clip;}
            currentHorn=signal;hornEnds=clock+clip.length;horn.clip=clip;horn.Play();return true;
        }
        public void ResetForVoyage()
        {
            currentHorn=WhistleSignal.None;horn?.Stop();radio?.Stop();radioQueue.Clear();subtitle="";radioEnds=0;nextChatter=clock+26;chatter=0;settingsOpen=false;
            foreach(var clip in whistles.Values){owned.Remove(clip);if(clip!=null)Destroy(clip);}whistles.Clear();
            if(sea!=null&&!sea.isPlaying)sea.Play();
        }
        public void Say(string id,bool urgent=false)
        {
            if(string.IsNullOrEmpty(id))return;
            if(urgent){radio?.Stop();radioEnds=0;radioQueue.Clear();}
            if(radioQueue.Count<3&&!radioQueue.Contains(id))radioQueue.Enqueue(id);
        }
        private void Update()
        {
            if(session==null)return;if(session.MenuOpen)settingsOpen=false;bool shouldPause=appPaused||session.MenuOpen||!session.HasStarted;
            if(shouldPause!=paused){paused=shouldPause;foreach(var s in new[]{horn,radio,sea}){if(paused)s.Pause();else s.UnPause();}}
            float volume=muted?0:master;horn.volume=volume*.72f;radio.volume=volume*.60f;sea.volume=volume*.14f*(HornBusy?.4f:1);
            if(paused)return;clock+=Time.unscaledDeltaTime;
            if(HornBusy&&clock>=hornEnds){var finished=currentHorn;currentHorn=WhistleSignal.None;HornCompleted?.Invoke(finished);}
            if(!RadioBusy&&radioQueue.Count>0)PlayRadio(radioQueue.Dequeue());
            if(clock>=nextChatter&&session.CanControl&&(session.Missions==null||!session.Missions.Terminal)&&(session.Missions?.Run?.Rescue==null))
            {Say("chatter_"+(chatter%3+1));chatter++;nextChatter=clock+35+(chatter%3)*8;}
            if(session.CanControl)
            {
                if(Input.GetKeyDown(KeyCode.H)||Input.GetKeyDown(KeyCode.Alpha1))PlayWhistle(WhistleSignal.Starboard);
                if(Input.GetKeyDown(KeyCode.Alpha2))PlayWhistle(WhistleSignal.Port);
                if(Input.GetKeyDown(KeyCode.Alpha3))PlayWhistle(WhistleSignal.Astern);
                if(Input.GetKeyDown(KeyCode.L))PlayWhistle(WhistleSignal.Prolonged);
                if(Input.GetKeyDown(KeyCode.Alpha5))PlayWhistle(WhistleSignal.Attention);
            }
        }
        private void PlayRadio(string id)
        {
            AudioClip clip=LoadRadio(id);subtitle=RadioText(id);subtitleEnds=clock+(clip!=null?clip.length:6);
            if(clip!=null){radio.clip=clip;radioEnds=clock+clip.length;radio.Play();}else radioEnds=clock+1;
        }
        public AudioClip LoadRadio(string id)
        {
            if(clips.TryGetValue(id,out AudioClip ready))return ready;
            AudioClip raw=Resources.Load<AudioClip>("Audio/Radio/"+id);
            if(raw==null){if(!missingWarned){Debug.LogWarning("Türkçe telsiz ses dosyası eksik; altyazı etkin.");missingWarned=true;}return null;}
            raw.LoadAudioData();float[] input=new float[raw.samples*raw.channels];
            if(!raw.GetData(input,0))return raw;
            float[] output=new float[raw.samples+raw.frequency/5];float hp=0,last=0,lp=0;uint noise=1234567;
            for(int i=0;i<output.Length;i++)
            {
                float x=0;int sample=i-raw.frequency/10;if(sample>=0&&sample<raw.samples)for(int c=0;c<raw.channels;c++)x+=input[sample*raw.channels+c]/raw.channels;
                float high=.92f*(hp+x-last);last=x;hp=high;lp+=.48f*(high-lp);noise=noise*1664525+1013904223;
                float n=((noise>>8)/(float)0xffffff*2-1);float squelch=sample<0||sample>=raw.samples?.045f:.012f;
                output[i]=Mathf.Clamp(lp*1.7f+n*squelch,-.85f,.85f);
            }
            var processed=Own(AudioClip.Create("Radio filtered "+id,output.Length,1,raw.frequency,false));processed.SetData(output,0);clips[id]=processed;return processed;
        }
        [Serializable]private sealed class RadioLine{public string id,text;}
        [Serializable]private sealed class RadioLines{public RadioLine[] lines;}
        private static Dictionary<string,string> radioTexts;
        private static bool FindRadioText(string id,out string value)
        {
            if(radioTexts==null)
            {
                radioTexts=new Dictionary<string,string>();
                foreach(string name in new[]{"lines","mob_lines"})
                {var asset=Resources.Load<TextAsset>("Audio/Radio/"+name);if(asset!=null){var rows=JsonUtility.FromJson<RadioLines>("{\"lines\":"+asset.text+"}");if(rows?.lines!=null)foreach(var row in rows.lines)if(row!=null&&!string.IsNullOrEmpty(row.id))radioTexts[row.id]=row.text;}}
            }
            return radioTexts.TryGetValue(id,out value);
        }
        public static string RadioText(string id)=>FindRadioText(id,out string value)?value:id switch
        {
            "brief_1"=>"Liman kontrol → KUMKALE: Doğu rıhtımı, sancak aborda. Giriş kapılarını sırayla takip edin.",
            "brief_2"=>"Liman kontrol → KUMKALE: Batı rıhtımı, iskele aborda. Girişte süratinizi azaltın.",
            "brief_3"=>"Liman kontrol: Doğu rıhtımındaki iki eğitim gemisi arasına yanaşın. Baş/kıç mesafelerine dikkat.",
            "brief_4"=>"Liman kontrol: Doğudan dalga alıyorsunuz. Batı rıhtımına iskele aborda yanaşın.",
            "brief_5"=>"Liman kontrol: İlk plan doğu rıhtımı. Yeni talimatlar için kanalda kalın.",
            "entry"=>"Giriş hattınız uygun. Kontrollü ilerleyin; manevra alanında eğitim gemileri var.",
            "approach"=>"Rıhtıma yaklaşıyorsunuz. İleri ve yanal sürati azaltın; baş/kıç mesafelerini kontrol edin.",
            "reassign"=>"YENİ EMİR: Doğu rıhtımı kullanılamıyor. Batı rıhtımına İSKELE ABORDA yanaşın.",
            "signal_1"=>"Sancağa rota değişikliği: BİR KISA düdük işaretini verin.","signal_2"=>"İskeleye rota değişikliği: İKİ KISA düdük işaretini verin.","signal_3"=>"Tornistan sevk kullanılıyor: ÜÇ KISA düdük işaretini verin.",
            "success"=>"Manevra tamamlandı. Bir sonraki görev açıldı.","career_complete"=>"KUMKALE'nin beş eğitim manevrasını tamamladınız. Tebrikler.","failure"=>"Görev tamamlanamadı. Brifingi kontrol ederek yeniden deneyin.",
            "chatter_1"=>"Eğitim botu: Mendirek dışında beklemedeyiz, talimat bekliyoruz.","chatter_2"=>"Liman kontrol: Bekleme mevkiinizi koruyun; rıhtım manevrası devam ediyor.","chatter_3"=>"Tüm istasyonlar: Düşük süratle seyredin, emniyet mesafelerini koruyun.",_=>"Liman kontrol: Eğitim sahasına hoş geldiniz. Dikkatli seyredin."
        };
        private AudioClip Own(AudioClip clip){owned.Add(clip);return clip;}
        public static AudioClip BuildWhistle(WhistleSignal signal,VesselId vessel)
        {
            const int rate=22050;float duration=SignalDuration(signal);int count=signal==WhistleSignal.Port?2:signal==WhistleSignal.Astern?3:signal==WhistleSignal.Attention?5:1;
            float on=signal==WhistleSignal.Prolonged?5:1,gap=signal==WhistleSignal.Attention?.3f:1;
            float hz=vessel==VesselId.Anadolu?110:vessel==VesselId.Barbaros?160:330;float[] data=new float[Mathf.RoundToInt(duration*rate)];
            for(int i=0;i<data.Length;i++)
            {
                float t=i/(float)rate;int blast=Mathf.FloorToInt(t/(on+gap));float local=t-blast*(on+gap);if(blast>=count||local>=on)continue;
                float envelope=Mathf.Min(1,local/.035f)*Mathf.Min(1,(on-local)/.07f);float phase=2*Mathf.PI*hz*t;
                data[i]=envelope*(Mathf.Sin(phase)*.38f+Mathf.Sin(phase*2.005f)*.16f+Mathf.Sin(phase*3)*.06f);
            }
            AudioClip clip=AudioClip.Create("Whistle "+signal,data.Length,1,rate,false);clip.SetData(data,0);return clip;
        }
        private static AudioClip BuildSea()
        {
            const int rate=22050,length=rate*8;float[] data=new float[length];uint noise=5511;float low=0;
            for(int i=0;i<length;i++){noise=noise*1664525+1013904223;float n=(noise>>8)/(float)0xffffff*2-1;low+=.06f*(n-low);float swell=.5f+.25f*Mathf.Sin(i/(float)length*Mathf.PI*4);data[i]=low*swell;}
            // A short seam blend avoids a click at each loop boundary.
            for(int i=0;i<512;i++)data[length-512+i]=Mathf.Lerp(data[length-512+i],data[i],i/511f);
            AudioClip clip=AudioClip.Create("Original procedural sea",length,1,rate,false);clip.SetData(data,0);return clip;
        }
        public void SetVolume(float value){master=Mathf.Clamp01(value);PlayerPrefs.SetFloat("Meridian.Audio.Volume.v1",master);PlayerPrefs.Save();}
        public void SetMuted(bool value){muted=value;PlayerPrefs.SetInt("Meridian.Audio.Mute.v1",muted?1:0);PlayerPrefs.Save();}
        private void OnGUI()
        {
            if(session==null||!session.HasStarted||session.MenuOpen||session.Missions!=null&&session.Missions.BlocksControl)return;
            ui??=new MaritimeUiTheme();GUI.depth=-26;var layout=HelmLayout.Current;var previous=GUI.matrix;GUI.matrix=Matrix4x4.TRS(layout.Offset,Quaternion.identity,Vector3.one*layout.Scale);
            try
            {
                ui.Surface(layout.Signals);
                for(int i=0;i<6;i++)
                {
                    Rect cell=layout.SignalButton(i);bool playing=i<5&&currentHorn==(WhistleSignal)(i+1);bool canPress=i==5||session.CanControl&&!HornBusy;
                    bool previousEnabled=GUI.enabled;GUI.enabled=canPress;
                    if(GUI.Button(cell,GUIContent.none,playing||i==5&&settingsOpen?ui.Selected:ui.Button)){if(i==5)settingsOpen=!settingsOpen;else{settingsOpen=false;PlayWhistle((WhistleSignal)(i+1));}}
                    GUI.enabled=previousEnabled;
                    string top=i<3?(i+1).ToString():i==3?"—":i==4?"5":"SES";
                    string bottom=i<3?"KISA":i==3?"UZUN":i==4?"UYARI":muted?"KAPALI":"AYAR";
                    Color ink=playing?Amber:canPress?Ink:MaritimeUiTheme.Muted;
                    ui.Text(new Rect(cell.x+3,cell.y+4,cell.width-6,cell.height-25),top,i==5?(cell.width>60?15:13):cell.width>60?25:21,ink,TextAnchor.MiddleCenter,true);
                    ui.Text(new Rect(cell.x+3,cell.yMax-21,cell.width-6,14),bottom,cell.width>60?11:9,i==5&&muted?Amber:MaritimeUiTheme.Muted,TextAnchor.MiddleCenter,true);
                    if(playing){float fraction=Mathf.Clamp01(1-(hornEnds-clock)/SignalDuration(currentHorn));Fill(new Rect(cell.x+4,cell.yMax-4,(cell.width-8)*fraction,2),Amber);}
                }
                if(settingsOpen)
                {
                    Rect settings=layout.SoundSettings;ui.Surface(settings);ui.Text(new Rect(settings.x+12,settings.y+8,settings.width-94,24),"SES AYARLARI",12,Teal,bold:true);
                    if(GUI.Button(new Rect(settings.xMax-76,settings.y+8,64,26),"KAPAT",ui.Button))settingsOpen=false;
                    bool mute=GUI.Toggle(new Rect(settings.x+12,settings.y+44,155,26),muted,"Sessiz");if(mute!=muted)SetMuted(mute);
                    float volume=GUI.HorizontalSlider(new Rect(settings.x+14,settings.y+91,settings.width-96,22),master,0,1);if(Mathf.Abs(volume-master)>.01f)SetVolume(volume);
                    ui.Text(new Rect(settings.xMax-72,settings.y+80,58,26),$"%{master*100:0}",16,Ink);
                }
            }
            finally{GUI.matrix=previous;}
        }
        public bool BlocksCamera(Vector2 screen)
        {var l=HelmLayout.Current;var p=l.FromScreen(screen);return l.Signals.Contains(p)||settingsOpen&&l.SoundSettings.Contains(p);}
        private void OnApplicationPause(bool value)=>appPaused=value;
        private void OnDestroy(){ui?.Dispose();foreach(var clip in owned)if(clip!=null)Destroy(clip);}
    }
}
