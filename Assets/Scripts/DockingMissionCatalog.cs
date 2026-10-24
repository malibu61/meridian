using System;
using UnityEngine;
namespace Meridian
{
    public enum TugTraining{None,Both,BowSolo,SternSolo}
    public sealed class DockingMissionDefinition
    {
        public string Id{get;} public string Title{get;} public string Briefing{get;} public int Number{get;}
        public HarborBerth InitialBerth{get;} public SeaStateSettings Sea{get;} public bool TightBerth{get;} public bool Reassign{get;}
        public VesselId Vessel{get;}public TugTraining TugExercise{get;}public bool IndependentEngines{get;}
        public bool RequiresTugs=>TugExercise!=TugTraining.None;
        public bool IsRescue{get;}
        public int RescueSide=>Vessel==VesselId.Barbaros||Vessel==VesselId.Ruzgar?1:-1;
        public string RadioId=>IsRescue?"mob_brief":Vessel==VesselId.Kumkale?"brief_"+Number:Vessel.ToString().ToLowerInvariant()+"_brief_"+Number;
        public Vector3 Spawn=>IsRescue?new Vector3(0,.1f,-320):new(Number==2?-5:Number==5?5:0,.1f,-(Vessel==VesselId.Anadolu?180:Vessel==VesselId.Barbaros?160:140)-(Number-1)*5);
        public float InitialKnots=>IsRescue?(Vessel==VesselId.Ruzgar?30:17):Vessel==VesselId.Anadolu?2:Vessel==VesselId.Ruzgar?4:3;
        public float HarborSpeedLimit=>Vessel==VesselId.Anadolu?5:Vessel==VesselId.Barbaros?6:8;
        public string ExerciseBrief=>IsRescue?"ALARM → KONUMU İŞARETLE → BORDAYA YAKLAŞ → STOP → KURTAR. Kazazedeyi görüşte tut; üstünden veya kıç/pervane sahasından geçme. Bu, basitleştirilmiş oyun eğitimidir; gerçek acil durum prosedürü değildir.":TugExercise switch
        {TugTraining.Both=>"Baş ve kıç römorkörü gerçekten temas ederek en az 3'er saniye itsin. Sonra STOP ve iki halat.",TugTraining.BowSolo=>"Önce yalnız BAŞ römorkörü en az 3 s itsin. Ardından kıç da en az 3 s destek versin. Son aşama iki halat.",TugTraining.SternSolo=>"Önce yalnız KIÇ römorkörü en az 3 s itsin. Ardından baş da en az 3 s destek versin. Son aşama iki halat.",_=>IndependentEngines?"İç ve dış makine yüklerinden en az bir çifti 3 s farklı kullan. Liman yaklaşmasında 5 kn altında yap; sonra iki halat.":"Makine/dümenle yaklaş; STOP, düşük sürat, baş ve kıç halatlarını al."};
        public DockingMissionDefinition(int number,string id,string title,string briefing,HarborBerth berth,int waves=1,float direction=315,bool tight=false,bool reassign=false,VesselId vessel=VesselId.Kumkale,TugTraining tugs=TugTraining.None,bool independent=false,bool rescue=false)
        {Number=number;Id=id;Title=title;Briefing=briefing;InitialBerth=berth;Sea=new SeaStateSettings(waves,direction);TightBerth=tight;Reassign=reassign;Vessel=vessel;TugExercise=tugs;IndependentEngines=independent;IsRescue=rescue;}
        public Vector3 Target(HarborBerth berth)=>Vessel==VesselId.Kumkale?new(berth==HarborBerth.East?30.6f:-28.6f,.1f,berth==HarborBerth.East?70:54):MooringRules.Destination(VesselCatalog.Get(Vessel),berth);
    }
    public static class DockingMissionCatalog
    {
        private static readonly DockingMissionDefinition[] missions={
            new(1,"kumkale.starboard.v1","İlk Yanaşma — Sancak Aborda","Mendireğin dışından başla. Üç giriş kapısını sırayla geç, doğu rıhtımına sancak aborda yanaş. Sürati erken azalt. Görüşteki eğitim gemileri nedeniyle manevra düdüklerini sen vermelisin.",HarborBerth.East),
            new(2,"kumkale.port.v1","Diğer Borda — İskele Aborda","Farklı yaklaşma konumundan limana gir. Batı rıhtımına iskele aborda yanaş. Makine ve dümenle baş/kıç hareketini yönet; yapılan rota değişikliği ve tornistan sevk için doğru düdüğü ver.",HarborBerth.West),
            new(3,"kumkale.tight.v1","Dar Yer — İki Geminin Arasına","Doğu rıhtımında iki bağlı eğitim gemisi arasındaki boşluğa yerleş. Baş ve kıç mesafelerini koru. Sert temas görevi bitirir; hafif temas yıldızını azaltır.",HarborBerth.East,tight:true),
            new(4,"kumkale.beam-sea.v1","Koşullar Değişti — Bordadan Dalga","Doğudan gelen üçüncü seviye oyun dalgaları altında batı rıhtımına yanaş. Baş, rota ve yanal sürati birlikte izle. Görev deniz koşulları sabittir; bu görev rüzgâr simülasyonu değildir.",HarborBerth.West,3,90),
            new(5,"kumkale.reassignment.v1","Beklenmeyen Emir — Rıhtım Değişikliği","Doğu rıhtımına yaklaşmak üzere limana gir. Liman kontrolünden gelecek yeni talimatı takip et. Rıhtım değişikliği güvenli yeniden planlama mesafesinde bildirilecek.",HarborBerth.East,2,315,reassign:true)
        };
        private static readonly DockingMissionDefinition[] barbaros={
            new(1,"barbaros.starboard.v1","Fırkateynle İlk Yanaşma","Mendireğin dışından doğu rıhtımına sancak aborda yaklaş. Uzun gövdenin ataletini ve erken tornistan frenini yönet. Baş/kıç halatlarıyla tamamla.",HarborBerth.East,vessel:VesselId.Barbaros),
            new(2,"barbaros.port.v1","İskele Aborda — Uzun Gövde","Batı rıhtımına iskele aborda yanaş. İki makineyi bağımsız kullan, baş/kıç salınımını erken durdur. İki halatı almadan görev bitmez.",HarborBerth.West,2,315,vessel:VesselId.Barbaros),
            new(3,"barbaros.bow-tug.v1","Baş Römorkörü — Başını Toparla","Doğu rıhtımına yaklaş. Önce yalnız baş römorkörüyle baş hareketini gör, ardından kıç desteğiyle paralelleş. Gerçek itme sürelerini panelden izle.",HarborBerth.East,vessel:VesselId.Barbaros,tugs:TugTraining.BowSolo),
            new(4,"barbaros.twin-tug.v1","İki Römorkör — Dalgalı Yaklaşma","Doğudan dalga altında batı rıhtımına yaklaş. Sancaktaki iki römorkörün temasa geçip iskeleye itmesini yönet. İtme sonrası baş ve kıç halatlarını al.",HarborBerth.West,2,90,vessel:VesselId.Barbaros,tugs:TugTraining.Both),
            new(5,"barbaros.reassignment.v1","Yeni Rıhtım — Desteği Yeniden Kur","İkinci giriş kapısında yeni rıhtım emri gelecek. Römorkörler karşı bordaya baş/kıç etrafından intikal eder; yeniden komut ver. Yeni rıhtımda iki desteği de kullan.",HarborBerth.East,2,315,reassign:true,vessel:VesselId.Barbaros,tugs:TugTraining.Both)
        };
        private static readonly DockingMissionDefinition[] anadolu={
            new(1,"anadolu.twin-tug.v1","Ağır Gemi — İlk Römorkör Manevrası","Dış yaklaşmadan doğu rıhtımına ilerle. Podlarla sürati azalt; iki römorkörü temasa alıp sancak yönünde ittir. Her iki gerçek destekten sonra halatları al.",HarborBerth.East,vessel:VesselId.Anadolu,tugs:TugTraining.Both),
            new(2,"anadolu.port.v1","Batı Rıhtımı — İskeleye İtme","Batı rıhtımına iskele aborda yanaş. Römorkörler sancak bordasından iter. Büyük gövdenin süratini girişten önce düşür, baş ve kıç hareketini birlikte izle.",HarborBerth.West,vessel:VesselId.Anadolu,tugs:TugTraining.Both),
            new(3,"anadolu.stern-tug.v1","Kıç Römorkörü — Açıyı Düzelt","Doğu rıhtımına yaklaş. Önce kıç römorkörünü tek başına çalıştırarak açıyı düzelt; sonra baş desteğini de devreye al. Sadece temas değil gerçek itme sayılır.",HarborBerth.East,vessel:VesselId.Anadolu,tugs:TugTraining.SternSolo),
            new(4,"anadolu.beam-sea.v1","Bordadan Dalga — Eş Güdümlü İtme","Doğudan gelen ikinci seviye oyun dalgalarında batı rıhtımına yanaş. İki römorkörü ve pod komutlarını koordine et. Halat alırken makineleri STOP'a getir.",HarborBerth.West,2,90,vessel:VesselId.Anadolu,tugs:TugTraining.Both),
            new(5,"anadolu.reassignment.v1","Rıhtım Değişti — Büyük Gemi Planı","Başlangıç hedefi doğu. İkinci giriş kapısında batıya yönlendirileceksin. Römorkörleri yeni bordaya geçir, yeniden gerçek destek al ve iki halatla tamamla.",HarborBerth.East,2,315,reassign:true,vessel:VesselId.Anadolu,tugs:TugTraining.Both)
        };
        private static readonly DockingMissionDefinition[] ruzgar={
            new(1,"ruzgar.starboard.v1","Dört Makine — İlk Yanaşma","Dış yaklaşmadan doğu rıhtımına sancak aborda gel. Hücumbotun süratini erken azalt. Dört makineyi ortak kolla yönetip iki halatla tamamla.",HarborBerth.East,vessel:VesselId.Ruzgar),
            new(2,"ruzgar.port.v1","İskele Aborda — Diferansiyel Makine","Batı rıhtımına yaklaş. İç/dış makine kollarını bağımsız kullan: yaklaşma sahasında 5 kn altında en az 3 s farklı yük uygula. Sonra durdurup iki halatı al.",HarborBerth.West,vessel:VesselId.Ruzgar,independent:true),
            new(3,"ruzgar.tight.v1","Dar Yanaşma — Dört Bağımsız Makine","İki bağlı eğitim gemisi arasındaki yere yerleş. İç ve dış makine yüklerini farklı kullanarak düşük süratte baş/kıç kontrolü yap; mesafeleri koru ve iki halatı al.",HarborBerth.East,tight:true,vessel:VesselId.Ruzgar,independent:true),
            new(4,"ruzgar.beam-sea.v1","Hücumbot — Bordadan Dalga","Doğudan ikinci seviye dalga altında batı rıhtımına yanaş. Küçük gövdenin yalpasını ve yanal sürati izle. Farklı iç/dış makine yükü çalışmasını düşük süratte tamamla.",HarborBerth.West,2,90,vessel:VesselId.Ruzgar,independent:true),
            new(5,"ruzgar.reassignment.v1","Hızlı Bot — Yeni Rıhtım Emri","İkinci kapıda doğu yerine batı rıhtımı verilecek. Yeni planla limana gir; iç/dış makineleri düşük süratte bağımsız kullan. Baş ve kıç halatlarıyla tamamla.",HarborBerth.East,2,315,reassign:true,vessel:VesselId.Ruzgar,independent:true)
        };
        private static System.Collections.Generic.IReadOnlyList<DockingMissionDefinition> WithRescue(DockingMissionDefinition[] docking,VesselId vessel)
        {
            var result=new DockingMissionDefinition[docking.Length+1];Array.Copy(docking,result,docking.Length);
            result[^1]=new DockingMissionDefinition(6,vessel.ToString().ToLowerInvariant()+".mob.v1","Denize Adam Düştü — Kurtarma Eğitimi",$"Liman dışında {(vessel==VesselId.Ruzgar?30:17)} kn ile seyirdesin. Görev başlayınca bordadaki personel denize düşer. Alarmı ver, MOB konumunu işaretle ve geri dön. Kurtarma istasyonunu kazazedeye yaklaştır; düşük sürat, STOP ve sakin durumda kurtarmayı başlat. Nispi kerterizi ve mesafeyi izle.",HarborBerth.East,vessel:vessel,rescue:true);
            return Array.AsReadOnly(result);
        }
        public static System.Collections.Generic.IReadOnlyList<DockingMissionDefinition> All{get;}=WithRescue(missions,VesselId.Kumkale);
        private static readonly System.Collections.Generic.IReadOnlyList<DockingMissionDefinition> Barbaros=WithRescue(barbaros,VesselId.Barbaros),Anadolu=WithRescue(anadolu,VesselId.Anadolu),Ruzgar=WithRescue(ruzgar,VesselId.Ruzgar);
        public static int Count=>All.Count;
        public static int TotalCount=>Count*VesselCatalog.All.Count;
        public static System.Collections.Generic.IReadOnlyList<DockingMissionDefinition> For(VesselId vessel)=>vessel switch{VesselId.Barbaros=>Barbaros,VesselId.Anadolu=>Anadolu,VesselId.Ruzgar=>Ruzgar,_=>All};
        public static DockingMissionDefinition Get(VesselId vessel,int index)=>index>=0&&index<Count?For(vessel)[index]:null;
        public static int IndexOf(VesselId vessel,string id){for(int i=0;i<Count;i++)if(Get(vessel,i).Id==id)return i;return -1;}
        public static DockingMissionDefinition Get(int index)=>Get(VesselId.Kumkale,index);
        public static int IndexOf(string id)=>IndexOf(VesselId.Kumkale,id);
        public static Vector3 Gate(int index)=>new(0,.1f,index==0?-80:index==1?-20:12);
        public static float GateRadius(int index)=>index==0?8:7;
    }
}
