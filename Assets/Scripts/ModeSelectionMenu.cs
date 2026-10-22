using UnityEngine;
using static Meridian.MaritimeUiTheme;
namespace Meridian
{
    public sealed class ModeSelectionMenu:MonoBehaviour
    {
        private GameSessionController session;
        private SimulationMode selected;
        private VesselId selectedVessel=VesselId.Barbaros;
        private MaritimeUiTheme ui;
        private bool menuWasOpen;
        private bool seaPage;
        private SeaStateSettings selectedSea=SeaStateSettings.Default;
        public SeaStateSettings SelectedSea=>selectedSea;
        public bool StableSelectionLayout=>ui!=null&&ui.SelectionLayoutMatches;
        public void ShowSeaOptions()=>seaPage=true;
        public bool SelectSea(SeaStateSettings sea){if(!sea.IsValid)return false;selectedSea=sea;return true;}
        public SimulationMode SelectedMode=>selected;
        public VesselId SelectedVessel=>selectedVessel;
        public bool SelectVessel(VesselId vessel)
        {if(!VesselCatalog.IsAvailable(vessel))return false;selectedVessel=vessel;return true;}
        public void SelectMode(SimulationMode mode){if(mode==SimulationMode.Harbor||mode==SimulationMode.StraitPassage||mode==SimulationMode.BuoyMooring)selected=mode;}
        public void Configure(GameSessionController controller){session=controller;selected=SimulationMode.Harbor;selectedVessel=controller.ActiveVessel;selectedSea=controller.ActiveSea;}
        private void OnGUI()
        {
            if(session==null)return;
            if(session.MenuOpen&&!menuWasOpen)GUIUtility.hotControl=0;
            menuWasOpen=session.MenuOpen;ui??=new MaritimeUiTheme();GUI.depth=-30;
            HelmLayout layout=HelmLayout.Current;
            Matrix4x4 previous=GUI.matrix;
            if(!session.MenuOpen)
            {
                GUI.matrix=Matrix4x4.TRS(layout.Offset,Quaternion.identity,Vector3.one*layout.Scale);
                try{DrawStatus(layout.Status);if(session.ActiveMode==SimulationMode.StraitPassage)DrawRouteMap(layout.RouteMap);}finally{GUI.matrix=previous;}return;
            }
            Fill(new Rect(0,0,Screen.width,Screen.height),new Color(.018f,.029f,.045f,.98f));
            // The menu keeps a centered 16:9 design inside the display safe area.
            Vector2 offset=layout.Offset+new Vector2((layout.Width-1280)*layout.Scale*.5f,(layout.Height-720)*layout.Scale*.5f);
            GUI.matrix=Matrix4x4.TRS(offset,Quaternion.identity,Vector3.one*layout.Scale);
            try{DrawMenu();}finally{GUI.matrix=previous;}
        }
        private void DrawMenu()
        {
            for(int x=40;x<1280;x+=60)Fill(new Rect(x,82,1,560),new Color(.12f,.19f,.25f,.13f));
            for(int y=82;y<650;y+=60)Fill(new Rect(40,y,1200,1),new Color(.12f,.19f,.25f,.13f));
            Fill(new Rect(40,30,4,26),Teal);ui.Text(new Rect(58,28,350,30),"M E R I D I A N",22,Ink,bold:true);
            if(GUI.Button(new Rect(818,24,198,40),"01  /  SEYİR",seaPage?ui.Button:ui.Selected))seaPage=false;
            if(GUI.Button(new Rect(1030,24,210,40),"02  /  DENİZ",seaPage?ui.Selected:ui.Button))seaPage=true;
            Fill(new Rect(40,77,1200,1),new Color(.17f,.24f,.30f));
            ui.Text(new Rect(40,99,800,62),seaPage?"Deniz senin seçimin.":session.Completed?"Geçiş tamamlandı.":"Bir sonraki seyrin.",48,Ink,bold:true);
            ui.Text(new Rect(43,167,1000,28),seaPage?"Dalga seviyesini ve dalganın GELDİĞİ pusula yönünü seç.":"Platformunu seç. Görevini belirle. Kumanda sende.",18,Muted);
            if(seaPage){DrawSeaOptions();return;}
            ui.Text(new Rect(40,220,480,26),"01   PLATFORM",13,Teal,bold:true);
            ui.Text(new Rect(628,220,612,26),"02   SEYİR MODU",13,Teal,bold:true);
            DrawVesselCard(VesselId.Barbaros,new Rect(40,263,268,104));
            DrawVesselCard(VesselId.Anadolu,new Rect(320,263,268,104));
            DrawVesselCard(VesselId.Ruzgar,new Rect(40,381,268,104));
            DrawVesselCard(VesselId.Kumkale,new Rect(320,381,268,104));
            VesselDefinition chosen=VesselCatalog.Get(selectedVessel);
            GUI.Box(new Rect(40,505,548,99),GUIContent.none,ui.Inset);
            ui.Text(new Rect(58,516,510,20),chosen.ClassLabel+" / "+chosen.Pennant,12,Muted,bold:true);
            ui.Text(new Rect(58,545,510,45),chosen.Description,16,Ink);
            DrawMode(SimulationMode.Harbor,new Rect(628,263,612,97),"01","Gemi Yanaştırma","Liman içinde serbest manevra","SERBEST SEYİR");
            DrawMode(SimulationMode.StraitPassage,new Rect(628,373,612,97),"02","Boğaz Geçişi","Çanakkale · kuzeyden güneye","TASLAK COĞRAFYA");
            DrawMode(SimulationMode.BuoyMooring,new Rect(628,483,612,97),"03","Şamandıraya Bağlama","Yaklaşma ve bağlama alıştırması","YAKINDA");
            string detail=selected==SimulationMode.Harbor?"Serbest manevra alanı · Puanlı yanaşma görevi henüz yok.":selected==SimulationMode.StraitPassage?"12 sıralı kontrol noktası · Gerçek harita ve akıntı henüz yok.":"Bu görev henüz geliştirilmedi; başlatılamaz.";
            ui.Text(new Rect(632,587,608,26),detail,12,Muted);
            bool available=GameSessionController.IsAvailable(selected);GUI.enabled=available;
            if(GUI.Button(new Rect(926,638,314,54),available?"SEYRİ BAŞLAT   →":"YAKINDA",ui.Primary))
            {session.StartMode(selected,selectedVessel,selectedSea);GUIUtility.ExitGUI();}
            GUI.enabled=true;
            if(session.HasStarted&&!session.Completed&&GUI.Button(new Rect(628,638,280,54),"MEVCUT SEYRE DEVAM",ui.Button))
            {session.Resume();GUIUtility.ExitGUI();}
            ui.Text(new Rect(40,636,548,24),$"DALGA {selectedSea.Level} / 5   ·   {selectedSea.DirectionName.ToUpperInvariant()} YÖNÜNDEN",12,Teal,bold:true);
            ui.Text(new Rect(40,666,548,22),"Yeni seyir sıfırlar. Devam, mevcut gemi ve konumu korur.",12,Muted);
        }
        private void DrawSeaOptions()
        {
            ui.Text(new Rect(40,220,548,26),"01   DALGA SEVİYESİ",13,Teal,bold:true);
            ui.Text(new Rect(628,220,612,26),"02   GELİŞ YÖNÜ / DÜNYA PUSULASI",13,Teal,bold:true);
            for(int level=1;level<=5;level++)
            {
                SeaStateSettings option=new(level,selectedSea.FromDegrees);Rect r=new(40,263+(level-1)*59,548,49);
                if(GUI.Button(r,GUIContent.none,level==selectedSea.Level?ui.Selected:ui.Button))selectedSea=option;
                ui.Text(new Rect(r.x+17,r.y+7,40,35),level.ToString(),23,level==selectedSea.Level?Teal:Muted,bold:true);
                ui.Text(new Rect(r.x+74,r.y+7,330,35),option.LevelName,19,Ink,bold:true);
                for(int i=0;i<5;i++)Fill(new Rect(r.xMax-116+i*17,r.y+27-i*3,10,8+i*3),i<level?Teal:new Color(.15f,.22f,.26f));
            }
            GUI.Box(new Rect(628,263,612,286),GUIContent.none,ui.Inset);
            int[] columns={1,2,2,2,1,0,0,0},rows={0,0,1,2,2,2,1,0};
            for(int direction=0;direction<8;direction++)
            {
                float degrees=direction*45;Rect r=new(710+columns[direction]*146,286+rows[direction]*74,132,58);
                if(GUI.Button(r,$"{SeaStateSettings.DirectionShort[direction]}  ·  {degrees:000}°",Mathf.Abs(Mathf.DeltaAngle(selectedSea.FromDegrees,degrees))<1?ui.Selected:ui.Button))selectedSea=new SeaStateSettings(selectedSea.Level,degrees);
            }
            Vector2 center=new(922,389);Vector2 travel=selectedSea.TravelDirection;Vector2 arrow=new(travel.x,-travel.y);
            Line(center-arrow*17,center+arrow*17,3,Teal);Vector2 tip=center+arrow*17,perp=new(-arrow.y,arrow.x);
            Line(tip,tip-arrow*9+perp*6,3,Teal);Line(tip,tip-arrow*9-perp*6,3,Teal);
            ui.Text(new Rect(650,513,568,24),$"{selectedSea.DirectionName} yönünden geliyor → karşı yöne ilerliyor",14,Ink,TextAnchor.MiddleCenter);
            GUI.Box(new Rect(40,577,548,45),GUIContent.none,ui.Inset);
            for(int i=0;i<80;i++)
            {
                float x=58+i*6.4f,next=x+6.4f;
                float h=WaterWaves.SampleHeight(selectedSea,i*.5f,0,Time.unscaledTime);
                float hn=WaterWaves.SampleHeight(selectedSea,(i+1)*.5f,0,Time.unscaledTime);
                Line(new Vector2(x,600-h*17),new Vector2(next,600-hn*17),2,Teal);
            }
            ui.Text(new Rect(632,564,608,46),"Yandan dalga: yalpa. Baştan/kıçtan: baş-kıç hareketi. Seviye arttıkça fiziksel sallanma artar.",16,Muted);
            bool canApply=session.HasStarted&&!session.Completed;
            if(GUI.Button(new Rect(926,638,314,54),canApply?"UYGULA VE DEVAM  →":"SEYİR SEÇİMİNE DÖN  →",ui.Primary))
            {
                if(canApply){session.ApplySeaState(selectedSea);session.Resume();GUIUtility.ExitGUI();}else seaPage=false;
            }
            if(GUI.Button(new Rect(628,638,280,54),"SEYİR SEÇİMİ",ui.Button))seaPage=false;
            ui.Text(new Rect(40,636,548,25),"5 OYUN SEVİYESİ · RESMÎ DENİZ DURUMU ÖLÇEĞİ DEĞİLDİR",11,Muted,bold:true);
            ui.Text(new Rect(40,666,548,23),"Uygula mevcut seyri korur; deniz 4 saniyede geçiş yapar.",12,Muted);
        }
        private void DrawVesselCard(VesselId id,Rect r)
        {
            bool active=selectedVessel==id;
            if(GUI.Button(r,GUIContent.none,active?ui.Selected:ui.Button))SelectVessel(id);
            if(active)Fill(new Rect(r.x,r.y+18,3,r.height-36),Teal);
            DrawSilhouette(new Rect(r.x+171,r.y+48,77,41),id,active?Teal:Muted);
            VesselDefinition v=VesselCatalog.Get(id);
            ui.Text(new Rect(r.x+18,r.y+12,224,26),v.Name,19,Ink,bold:true);
            ui.Text(new Rect(r.x+18,r.y+42,145,19),v.Pennant,14,Muted);
            ui.Text(new Rect(r.x+18,r.y+64,149,19),$"{v.LengthMetres:0.#} × {v.BeamMetres:0.#} m",12,Muted);
            ui.Text(new Rect(r.x+18,r.y+87,232,14),v.EngineCaption,9,active?Teal:Muted);
            ui.Text(new Rect(r.xMax-28,r.y+10,16,20),active?"●":"○",12,active?Teal:Muted,TextAnchor.MiddleCenter);
        }
        private static void DrawSilhouette(Rect r,VesselId id,Color c)
        {
            Matrix4x4 previous=GUI.matrix;
            GUI.matrix=previous*Matrix4x4.TRS(r.position,Quaternion.identity,new Vector3(r.width/125f,r.height/47f,1));
            r=new Rect(0,0,125,47);
            try
            {
            float y=r.y+31;
            Fill(new Rect(r.x,y,r.width-12,8),c);Line(new Vector2(r.x+r.width-12,y+8),new Vector2(r.x+r.width,y-1),2,c);
            if(id==VesselId.Anadolu)
            {
                Fill(new Rect(r.x,y-3,r.width,3),c);Fill(new Rect(r.x+30,y-18,62,15),c);
                Fill(new Rect(r.x+38,y-31,2,13),c);Fill(new Rect(r.x+80,y-31,2,13),c);
                Line(new Vector2(r.x+r.width-15,y-3),new Vector2(r.x+r.width,y-9),3,c);
            }
            else if(id==VesselId.Barbaros)
            {
                Fill(new Rect(r.x+12,y-13,33,13),c);Fill(new Rect(r.x+55,y-19,28,19),c);
                Fill(new Rect(r.x+68,y-36,2,18),c);Fill(new Rect(r.x+45,y-22,6,9),c);
                Line(new Vector2(r.x+94,y-7),new Vector2(r.x+113,y-7),2,c);Fill(new Rect(r.x+93,y-8,8,8),c);
            }
            else
            {
                float s=r.width/125f;
                Fill(new Rect(r.x+r.width*.30f,y-13,r.width*.35f,13),c);
                Fill(new Rect(r.x+r.width*.48f,y-31,2,19),c);
                if(id==VesselId.Ruzgar)
                {
                    Fill(new Rect(r.x+r.width*.44f,y-33,9*s,9*s),c);
                    Line(new Vector2(r.x+r.width*.16f,y-7),new Vector2(r.x+r.width*.27f,y-13),3,c);
                    Fill(new Rect(r.x+r.width*.83f,y-5,8*s,5),c);
                }
                else Line(new Vector2(r.x+r.width*.64f,y-13),new Vector2(r.x+r.width*.84f,y),3,c);
                Line(new Vector2(r.x+r.width*.80f,y-5),new Vector2(r.x+r.width*.96f,y-5),2,c);
            }
            }
            finally{GUI.matrix=previous;}
        }
        private void DrawMode(SimulationMode mode,Rect r,string number,string name,string description,string badge)
        {
            bool active=selected==mode,available=GameSessionController.IsAvailable(mode);
            if(GUI.Button(r,GUIContent.none,active?ui.Selected:ui.Button))selected=mode;
            if(active)Fill(new Rect(r.x,r.y+15,3,r.height-30),Teal);
            ui.Text(new Rect(r.x+20,r.y+20,42,42),number,26,active?Teal:Muted,bold:true);
            ui.Text(new Rect(r.x+78,r.y+12,405,30),name,22,available?Ink:Muted,bold:true);
            ui.Text(new Rect(r.x+78,r.y+46,484,22),description,15,Muted);
            ui.Text(new Rect(r.x+78,r.y+74,490,15),badge,10,available?Teal:Muted);
            ui.Text(new Rect(r.xMax-43,r.y+26,26,26),available?"→":"—",20,active?Teal:Muted,TextAnchor.MiddleCenter);
        }
        private void DrawStatus(Rect r)
        {
            GUI.Box(r,GUIContent.none,ui.Inset);
            if(session.ActiveMode==SimulationMode.StraitPassage&&session.Voyage!=null)
            {
                VoyageProgress voyage=session.Voyage;
                ui.Text(new Rect(r.x+14,r.y+5,r.width-28,25),$"BOĞAZ GEÇİŞİ  /  {voyage.PassedCount}/{voyage.GateCount}   →   {StraitRoute.Names[Mathf.Min(voyage.NextIndex,StraitRoute.Count-1)]}   ·   DALGA {session.ActiveSea.Level}/5 / {session.ActiveSea.DirectionName}   ·   TASLAK COĞRAFYA",12,Muted);
                Fill(new Rect(r.x+2,r.yMax-2,(r.width-4)*voyage.Fraction,2),Teal);
            }
            else ui.Text(new Rect(r.x+14,r.y+5,r.width-28,25),$"GEMİ YANAŞTIRMA / SERBEST MANEVRA       DALGA {session.ActiveSea.Level}/5 · {session.ActiveSea.DirectionName} yönünden       ESC · MENÜ",12,Muted);
        }
        private void DrawRouteMap(Rect r)
        {
            GUI.Box(r,GUIContent.none,ui.Inset);
            Vector3 first=StraitRoute.Point(0),last=StraitRoute.Point(StraitRoute.Count-1);
            Vector2 Map(Vector3 p)=>new(r.x+12+Mathf.InverseLerp(last.x-900,first.x+900,p.x)*(r.width-24),r.y+10+Mathf.InverseLerp(first.z+500,last.z-500,p.z)*(r.height-20));
            for(int i=0;i<StraitRoute.Count-1;i++)Line(Map(StraitRoute.Point(i)),Map(StraitRoute.Point(i+1)),5,new Color(.12f,.32f,.36f));
            for(int i=0;i<StraitRoute.Count;i++)
            {Vector2 p=Map(StraitRoute.Point(i));Fill(new Rect(p.x-2,p.y-2,4,4),session.Voyage!=null&&i<session.Voyage.NextIndex?Teal:Muted);}
            if(session.PlayerShip!=null){Vector2 p=Map(session.PlayerShip.transform.position);Fill(new Rect(p.x-3,p.y-3,6,6),Amber);}
        }
        private void OnDestroy()=>ui?.Dispose();
    }
}
