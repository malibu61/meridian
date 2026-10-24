# ANADOLU — liman römorkör desteği

## Kapsam ve kullanım

- İlk ANADOLU desteği sonradan **BARBAROS + Gemi Yanaştırma** için de genişletildi. İki büyük geminin görevleri için [Filo görevleri](FILO_GOREVLERI.md) güncel kapsamdır. Boğaz geçişinde ve küçük gemilerde römorkör oluşturulmaz.
- Manevra başında liman kontrol penceresi hedefi sorar. Doğu rıhtımı: sancak aborda; batı rıhtımı: iskele aborda. Mevcut örnek limanın gerçek iki rıhtımı kullanılır; coğrafi gerçek bir üs iddiası yoktur.
- Turkuaz çerçeve geminin hedef konumunu gösterir. Rıhtıma paralel (baş 000° ±30°) ilerlenir. Römorkörler liman yaklaşma sahasında (oyun z: −75…110) da kullanılabilir; son rıhtımın yakınında olma şartı yoktur. Geminin ileri/tornistan makineleri hâlâ oyuncudadır; römorkörler seyir otopilotu değildir.
- Baş/kıç römorkörünün üzerindeki **İT** işareti veya üstteki iki komut kartı tıklanır/dokunulur. Komut, yaklaşma → temas → kademeli itme şeklinde işler. Tekrar tıklamak bırakır; **İKİSİ DUR** iki komutu da keser.
- Oklar **yalnız gerçekten kuvvet uygulanırken** itme yönünü gösterir. Sancağa yanaşırken iskeledeki, iskeleye yanaşırken sancaktaki römorkörler iter. Baş ve kıç ayrı kuvvet noktalarıdır; tek römorkör yaw momenti de oluşturur. Panelde gerçek uygulanan itme yüzdesi ile **YAN ←/→ … kn** yanal sürat gösterilir; temasta olup bir sınır nedeniyle itemiyorsa “TEMASTA · BEKLİYOR” yazar.
- İtme 5 kn ve üstünde kesilir; 2–5 kn arasında azaltılır. Temas yokken, menü/hedef penceresi açıkken, baş uygunsuzken veya manevra alanının dışındayken kuvvet uygulanmaz. Yakın hedefte itme azaltılır; güncel tamamlanma ayrıca iki baş/kıç halatının bağlanmasına dayanır.
- Serbest seyirde **HEDEF** düğmesi rıhtımı yeniden seçtirir; görevde atanmış rıhtım değiştirilemez. Yeni sefer/mod/gemi seçimi eski filo ve çizgiyi devre dışı bırakıp kaynakları temizler.

## Görsel araştırma

Görseller tarayıcıda açılarak incelendi; fotoğraflar oyuna kopyalanmadı veya paketlenmedi.

1. [Ottomar — TCG ÖNDER TUG BOAT / MSB Alaybey Tersanesi](https://www.ottomar.com.tr/tr/hizmet/tcg-onder-tug-boat): geniş siyah baş itme usturmaçası, gri gövde/üst yapı, yüksek camlı köprüüstü, direk ve bacalar. [Referans fotoğraf](https://www.ottomar.com.tr/uploads/tcg-onder.jpg).
2. [TRT/AA — İstanbul Tersanesi Komutanlığı fotoğraf dizisi](https://www.trthaber.com/foto-galeri/mavi-vatanin-kalesi-istanbul-tersanesi-komutanligi/49733.html): Y57 liman römorkörünün üç çeyrek görünüşü; açık kıç çalışma güvertesi, ırgat, baba, vinç, borda usturmaçaları, korkuluklar ve emniyet ekipmanları. [İncelenen fotoğraf](https://trthaberstatic.cdn.wp.trt.com.tr/resimler/1902000/1903028.jpg).

Ortaya çıkan iki model **özgün, bu referanslardan esinlenen liman destek römorkörleridir**. ÖNDER'in birebir ölçülmüş kopyası veya bu iki belirli teknenin ANADOLU'ya fiilen tahsis edildiği iddia edilmez. Oyun modeli boyutları yaklaşık 30 × 9,8 m; proje ölçeği 1 Unity birimi = 4 m. Köprüüstü camları, kapalı yuvarlatılmış gövde, ağır baş usturmaçası, yan lastikler, çift baca, radar/direk, çeki ırgatı, babalar, kıç vinci ve iki güverte personeli vardır. Statik parçalar malzeme başına birleştirilir; kaynaklar filo köküne aittir.

## Fizik ve sınırlamalar

ANADOLU'nun hareketi Rigidbody.AddForceAtPosition ile oluşur: transform ile gemi kaydırılmaz, tıklama anında uzaktan kuvvet verilmez. Kuvvetler mevcut indirgenmiş düzlemsel gemi fiziğiyle aynı ağırlık merkezi yüksekliğinde uygulanır; baş/kıç kolları gerçek dönme momenti üretir. İki eşit simetrik kuvvet yaklaşık saf yanal harekete katkı verir.

Römorkör intikali artık geminin ileri, yanal ve dönmeden kaynaklanan istasyon hızını izleyen kinematik refakattir; gemiye parent edilmez ve yetişmek için teleport edilmez. İtme yokken paralel refakat eder; uygun sürat/baş ve İT komutunda açıkta dönüp baş usturmaçasıyla temasa girer. **5 kn sınırı refakate değil, kuvvet uygulamaya aittir.**

Örnek limana özel sınırlı görünürlük-grafı yol bulma; kıyıları, iki rıhtımın görünür kapaklarını, arka duvarı, görevdeki bağlı eğitim gemilerini, ana geminin etrafını ve diğer römorkörü dikkate alır. Gövde/usturmaçayı her yaw açısında içine alan muhafazakâr dairesel iz, yer değiştirme boyunca ve dönüşte korunur. Temas yaklaşmasında ayrı su hattı gövde kontrolü çalışır. Borda değişimi baş veya kıç çevresinden yapılır; düz çizgide ana geminin içinden geçilmez.

Rıhtım tarafı daralması, geminin hızıyla 12 oyun saniyesi ileri bakılarak değerlendirilir. Arada kalacak römorkör itişi bırakıp açık-su bordasına intikal eder; uygun rota yoksa panelde **GÜVENLİ YOL BEKLİYOR** yazar, kuvvet vermez ve duvardan geçerek yetişmeye çalışmaz. Bu kestirim/katsayılar oyun ayarıdır; gerçek seyir emniyet planı değildir. Oyuncu gemiyi kasıtlı olarak hızlıca bir römorköre/engelle kapalı bölgeye sürerse bütün sıkışmaların fiziksel olarak çözüleceği garanti edilmez.

Bağımsız tam römorkör sevk/hidrodinamik ve çarpışma kuvvetleri simüle edilmez. Genel liman/başka harita/serbest hareketli gemi trafiği için evrensel çarpışma önleyici değildir. Halatla çekme, halat esnekliği/kopması ve sertifikalı bollard-pull katsayıları kapsam dışıdır. İtme katsayısı **oyun ayarıdır**, gerçek ÖNDER çeki kuvveti değildir. Rıhtıma yerleşme yardımının gerçek seyir eğitimi/emniyet sistemi olduğu iddia edilmez.

## Doğrulama

### Refakat ve rıhtımdan çekilme regresyonu

`Meridian.Editor.TugNavigationReview.RunBatch`: 25 yol/geometri kontrolü; 185.342 örnek bazlı runtime kontrolü geçti. ANADOLU/BARBAROS × ileri/yanal = dört refakat, iki gemi × iki rıhtım = dört otomatik çekilme ve önceki altı bağımsız gerçek Rigidbody kuvvet deneyi çalıştı. 16,5 kn ileri refakat boyunca itme kuvveti sıfır kaldı. İki gemide iki rıhtıma bilerek yanlış bordadan yanal yaklaşma sırasında römorkörler açık-su bordasına geçti; her örnekte görünür rıhtım, süpürülmüş hareket ve ana geminin su hattı kontrol edildi.

Yeni deneyde 30 sn iki römorkörle yaklaşık ±5,985 m yanal yer değiştirme; yalnız baş/kıç yaklaşık ±14,029° baş değişimi görüldü. Bunlar kontrollü oyun deneyi değerleridir; gerçek gemi performansı değildir. Refakat/çekilme testinde ana gemi yolu bilerek belirlenir; bütün görevlerin başlangıçtan sona elle oynandığı veya telefonda performans doğrulandığı anlamına gelmez.

Kanıt: `docs/testing/20261022-tug-navigation/`. Aşağıdaki ilk sürüm test sayıları tarihsel kayıt olarak korunur; yeni sonuçla karıştırılmamalıdır.

`Meridian/Tugs - Check Rules and Hull`: mod/gemi kapsamı, hedef/itme yönü, sürat ve baş sınırı, kapalı-dışa bakan gövde geometrisi ve farklı yatay ekranlarda panel yerleşimi.

`Meridian/Tugs - Run Integration Review`: gerçek oturumda hedef penceresi, bağımsız komutlar, menü engeli, yeniden planlama ve mod/gemi geçişlerinde filonun kaldırılması. Altı ayrı Unity PhysicsScene deneyinde üretim kuvvet yolu kullanılır: iki römorkörle sancağa/iskeleye kayma, yalnız başla ve yalnız kıçla zıt dönüş momentleri, final hedeften uzaktayken ve girişte ileri yolla birlikte gerçek yanal kuvvet; temas öncesi/sürat üstü/menü açıkken kuvvet yokluğu. Testler kullanıcının açık oyununa kendiliğinden müdahale etmez; ayrı doğrulama projesinde çalıştırılır.

### İlk denemeden gelen düzeltme

Kullanıcı ekranında römorkörler bordada olmasına rağmen “YAKLAŞIYOR / işaretli manevra alanına ilerle” yazıyordu. Neden, kuvvet izninin son rıhtımın ±12 birimine (±48 m) gereksiz yere bağlanmasıydı. Bu şart liman yaklaşma sahasına genişletildi; kuvvet olmayan anda çizilen yanıltıcı oklar kaldırıldı. İleri yol sürerken ve hedefe yaklaşık 210 m uzaktayken itme için regresyon deneyi eklendi. Geminin konumu yapay olarak kaydırılmadı veya kuvveti abartılarak sorun gizlenmedi.

Gerçek iPhone donanımında dokunma, performans ve Metal görüntüsü ayrıca doğrulanmalıdır.

### 22 Ekim 2026 sonuçları

- 747 kural/gövde/yerleşim kontrolü ve 56 entegrasyon kontrolü geçti; altı bağımsız fizik deneyi çalıştı.
- 30 saniye (yaklaşma ve kuvvetin yükselmesi dahil): iki römorkörle sancağa **6,115 m**, iskeleye **6,115 m** kayma; iki durumda baş değişimi yaklaşık **0°**.
- Yalnız baş römorkörü: **+14,510°**; yalnız kıç: **−14,510°** baş değişimi. Her ikisinde yaklaşık **3,072 m** yanal hareket.
- Hedeften uzakta z=18 ve girişte z=−52 konumlarında ileri makineyle ilerlerken **6,102 m** gerçek yanal yer değiştirme. Yukarıdaki değerler sakin-deniz, indirgenmiş oyun fiziği deneyleridir; gerçek ANADOLU manevra verisi değildir.
- Üç açıdan model render'ı incelendi. Kullanıcı ekranı, önceki yaklaşma engelini ve tıklama komutlarını doğrulamak için kullanıldı. Son arayüzün gerçek cihazdaki dokunma/performance testi tamamlandı diye değerlendirilmemelidir.
- Projedeki daha önceden mevcut `VesselSelectionReview` ANADOLU gövde kapaklarının kenar yönü uyarısı devam ediyor; bu çalışmada gizlenmedi veya değiştirilmedi. Römorkör gövdesinin kapalı ve tutarlı yönlenmiş geometri kontrolü ayrı olarak geçti.

Kanıt: `docs/testing/20261022-tugs/{rules,runtime,physics}.txt`; özgün model görüntüleri: `docs/images/tugs/tug-0.png`, `tug-1.png`, `tug-2.png`.
