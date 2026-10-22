# Sürat, dönüş çemberi ve yönlü yalpa

22 Ekim 2026. Bu belge son hareket güncellemesini açıklar; önceki yay-destek modelinin yerini alır.

## Değişiklik

Sürat hesabı, gemi ve boğaz coğrafyasındaki **1 Unity birimi = 4 metre** ölçeğine düzeltildi. Gösterge artık gerçek dünya karşılığı knot hesaplar. Her geminin motor itişi ve su direnci, kendi azami sürat hedefine göre kalibre edilir. Hıza anlık sınır yapıştırılmaz; itki ile doğrusal/karesel direnç dengelenir. Makine komutu yumuşak değişir; tornistan itişi ve azami tornistan sürati ayrıca daha düşüktür.

| Gemi | Tam ileri oyun hedefi | Dayanak |
|---|---:|---|
| RÜZGAR | 41 kn | [Naval Analyses tarihsel Rüzgar incelemesi](https://www.navalanalyses.com/2014/06/dogan-ruzgar-and-yildiz-class-fast.html); resmî güncel gemi deneme verisi doğrulanamadı |
| BARBAROS | 32 kn | [Marmara Üniversitesi araştırmasındaki sınıf özellikleri tablosu](https://katalog.marmara.edu.tr/veriler/yordambt/cokluortam/1F/62fdf2b6dfe4b.pdf), DzKK sınıf sayfasına atıflı |
| KUMKALE | 25 kn | [Üretici Dearsan PV57](https://www.dearsan.com/urun/pv57/) sınıf için 25+ kn yayımlar; oyun dengesi için 25 alındı |
| ANADOLU | 21 kn | [TBMM Meclis Haber, Mart–Nisan 2023, SSB kaynaklı bilgi](https://cdn.tbmm.gov.tr/TbmmWeb/Yayinlar/MeclisHaberDergisi/38219f5e-2c32-49ce-a8cb-acacc0967c87.pdf), tam yüklü azami 21 kn |

Bunlar tasarım/yayımlanmış azami değerlere dayalı oyun hedefleridir. Operasyonel güncel seyir sürati, yakıt durumu veya bütün denizlerde korunabilen sürat iddiası değildir. Yakıt tüketimi, şanzıman ve gaz türbini geçişleri simüle edilmez.

## Dönüş

Eski doğrudan `dümen × hız²` dönme momenti kaldırıldı. Yerine dümen geçiş süresi, gemiye özel yaw gecikmesi ve gemi boyuyla oranlanan hedef eğrilik kullanan basitleştirilmiş Nomoto yaklaşımı eklendi. Bütün hareket gerçek Rigidbody kuvvet/momentlerinden gelir; transform başı veya yatış açısı doğrudan döndürülmez.

Yanal su direnci gerçek hız vektörünü yavaşça yeni başa taşır; gemi yerinde araba gibi dönmek yerine iz üzerinde daireye oturur. İç ve dış makine kuvvetlerinin farklı kaldıraç kolları korunur. Düşük süratte diferansiyel makine manevrası mümkündür. ANADOLU podları bu sürümde hâlâ basitleştirilmiş azimutlu itki modelidir.

Tam dümen çap ayarları gemiye göre 3,4–4,2 boy civarında hedeflenir; ölçülen taktik çap geçiş rejimi ve makine/pod kuvvetlerinden dolayı farklı olabilir. Bunlar gerçek gemilerin pilot kartından alınmamıştır. [IMO MSC.137(76)](https://wwwcdn.imo.org/localresources/en/KnowledgeCentre/IndexofIMOResolutions/MSCResolutions/MSC.137%2876%29.pdf) dönüş/advance ölçümü için referanstır; bu gemilere mevzuat uygunluğu veya sertifikasyon iddiası değildir. Her süratte aynı metre cinsinden çap beklenmez; aynı ayarlarda tekrarlanabilir bir hareket beklenir.

Dönüş ivmesinden dışa yatış momenti üretilir: ileri yolda iskele dönüşünde sancak borda, sancak dönüşünde iskele borda alçalır. Hareket kademeli oluşur ve dümen ortalanınca sönümlenir. HUD’daki derece gerçek dümen konumunu, tutamak ise istenen komutu gösterir; küçük turkuaz işaret actual konumu izler.

## Dalga yönü ve yalpa

Yalpa, deniz yüzeyi eğimi ve gemiye özel doğal periyot/sönüm ile çalışan ikinci dereceli fiziksel tepkiye geçirildi. Bordaya yakın gelen dalga roll eksenini daha çok uyarır; baş/kıçtan gelen dalga daha çok pitch/heave üretir. Gemi döndükçe bordaya göre geliş açısı yeniden hesaplanır. Sert borda denizinde küçük gemilerde 30° üzeri tepki hedeflenmiştir; bu bir her-dalgada zorlanan sabit animasyon değildir.

Baş/kıçtan dalga **hiç yalpa olmaz** anlamına gelmez. Gerçekte parametrik yalpa gibi istisnalar vardır; bu sürüm onların tam stabilite modelini hesaplamaz. [Özgün yalpa/rezonans araştırması](https://www.mdpi.com/2077-1312/8/6/415) farklı karşılaşma koşullarının ayrımını açıklar.

Analog kadran ±40°’ye genişletildi; sayı ölçüm aralığı dışında da gerçek açıyı korur. Büyük açılarda oyun için ilerlemeli doğrultucu kuvvet bulunur. Bu katsayılar gemilerin gerçek GM/GZ/RAO veya devrilme sınırı değildir. Deniz seviyesi 1–5, oyunun beş ayarıdır; Beaufort veya WMO deniz durumu numarası değildir.

## Test yöntemi ve son durum

`Meridian → Handling - Run Speed Turn and Roll Review` opt-in testi ayrı Unity PhysicsScene’lerinde **aynı üretim kuvvet kodunu** 0,02 saniyelik adımla entegre eder. Görsel sahnedeki duvarlar ve iskeleler test çemberini kesmez. Sürat dengesi, tam tornistan, ara gazlar, iki yönde iki tam dönüş, gerçek iz çemberi, dışa yatış işareti ve dört yönden dalga ölçülür.

Son tur: **39 fizik senaryosu / 89 kontrol geçti**. C# son derleme kontrolü de `SMALLCRAFT_SOURCE_COMPILE_PASS` ile tamamlandı. Ham kayıt: [fizik sonucu](testing/20261022-smallcraft/handling-result.txt), [bütün ölçümler](testing/20261022-smallcraft/handling-measurements.txt).

| Gemi | Ölçülen tam ileri | Tam dümen taktik çapı | Kararlı dönüşte dışa yatış | Seviye 5 borda yalpa tepesi | Baş/kıç yalpa tepesi |
|---|---:|---:|---:|---:|---:|
| RÜZGAR | 41,0 kn | 207 m | 16,0° | 34,7° | 0,6° |
| KUMKALE | 25,0 kn | 215 m | 6,6° | 33,1° | 0,5° |
| BARBAROS | 32,0 kn | 469 m | 5,2° | 9,4° | 0,02° |
| ANADOLU | 21,0 kn | 980 m | 1,0° | 1,4° | <0,01° |

Dönüşler sakin deniz, tam ileri ve ±35° dümen komutuyla iki yönde ikişer tur entegre edildi. İkinci turda çemberden en büyük bağıl sapma %0,34'ün altındaydı. KUMKALE’nin [iskele iz koordinatları](testing/20261022-smallcraft/turn-Kumkale--1.csv) ve [sancak iz koordinatları](testing/20261022-smallcraft/turn-Kumkale-1.csv) metre cinsindendir. İskele dönüşünün yatışı sancak, sancak dönüşünün yatışı iskeledir.

Dalga karşılaştırması, başı/konumu sabit tutulan, düşey/yalpa/baş-kıç serbest test fikstüründe 150 saniye örneklenmiştir. Bu sayılar her süratte ve her dalga karşılaşmasında garanti edilen açılar değildir. Canlı seyirde baş, sürüklenme ve karşılaşma frekansı değişir. RÜZGAR’ın borda dalgası tepe açısı seviyeler 1→5 için yaklaşık **3,8 / 8,6 / 16,6 / 25,8 / 34,7°** oldu.

Kumanda testleri: **515** yerleşim/dokunma mantık kontrolü ve **69** canlı üretim-kod yolu kontrolü geçti. Dört şaft ayrı çalıştırıldığında dış şaftların dönüş momenti iç şaftlardan büyük çıktı; doğru iskele/sancak yönü doğrulandı. [Kayıt](testing/20261022-smallcraft/four-control-runtime.txt), [şaft ölçümü](testing/20261022-smallcraft/four-shaft-yaw.txt). Önceki iki geminin kontrol regresyonu da 727 mantık ve 22 canlı kontrol geçti.

Son model turu 7949 geometri ve 150 canlı model/mod kontrolünü geçti. İki gemi için toplam 18 kamera görüntüsü üretildi; iskele/sancak, baş/kıç, üst ve çeyrek açılar incelendi. Örnekler [model belgesinde](RUZGAR_VE_KUMKALE.md).

### Görsel ve cihaz testi sınırı

Windows Unity 6000.6.6f1 / DX11 test kopyası kullanıldı. Batch modunda offscreen 3D kamera renderları oluştu; **Game View HUD ekran görüntüleri oluşmadı**. İlk toplu sarmalayıcının son HUD görüntüsünü bekleyen adımı zaman aşımına uğradı; bundan önce yukarıdaki bireysel test grupları PASS vermişti. Sarmalayıcı artık batch modunda fizik/mantık sonuç dosyasını bekler; son kaynak derlemesi tekrar geçti. Henüz yeni HUD için etkileşimli fare/parmak görsel onayı verilmiş sayılmaz.

Açık asıl Unity oturumunun kontrolü otomatik fare hareketiyle kesildiği için yeniden başlatma yapılamadı. Yeni kaynakları görmek için **Play'i durdur, derleme bitince tekrar Play'e bas**. iPhone/iOS build, Metal, frame-time/ısınma ve gerçek çok-parmak cihaz testi ayrı iştir. Gerçekçi deniz hissi için kalibre edildi; gerçek gemi manevra/stabilite sertifikası değildir.
