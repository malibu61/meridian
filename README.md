# Meridian

Türk Deniz Kuvvetleri gemilerinden esinlenen, mobil hedefli bir **gemi kumanda ve manevra oyunu**. Oyuncu bağımsız makineleri, dümeni, BARBAROS/ANADOLU liman römorkörlerini ve baş/kıç halatlarını kullanarak gemiyi yönetir.

**Durum:** oynanabilir Unity Editor prototipi. Yayına hazır ürün, foto-gerçekçi gemi modeli veya sertifikalı seyir/eğitim simülatörü değildir.

**Güncel test erişimi:** dört gemideki **24 görevin tamamı açık**; önceki aşamayı bitirmek gerekmiyor. Bu ayar Editor ve telefon build'lerinde geçerli. Görevler otomatik tamamlanmaz; gerçek yıldız/süreler saklanır ve sırasız sonuçlar yeniden yüklemede silinmez. Production sıralı erişimi için `Assets/Scripts/MissionAccessPolicy.cs` içindeki `AllScenariosUnlockedForTesting` değeri `false` yapılır; önceki sonuçlar silinmeden sıra kontrolü geri gelir. Aşağıdaki sıralı kilit açıklamaları production tasarımıdır, mevcut test sürümünde uygulanmaz.

![ANADOLU için geliştirilen liman römorkörü](docs/images/tugs/tug-1.png)

## Teknoloji ve çalıştırma

- **Unity 6.6 — 6000.6.6f1**, C# ve Unity Rigidbody fiziği.
- Kodla oluşturulan gemi/liman modelleri, özel su ve köpük shader'ları, parçacık efektleri.
- Fare/klavye ve mobil dokunma kontrolleri. Projede iOS build profili bulunur; gerçek cihaz doğrulaması ve mağaza paketleri henüz tamamlanmamıştır.
- Proje ölçeği: **1 Unity birimi = 4 metre**. Sürat göstergeleri bu ölçekten knot'a çevrilir.

1. Depoyu klonla: `git clone https://github.com/malibu61/meridian.git`.
2. Unity Hub → **Add/Open project** ile `meridian` klasörünü seç.
3. Uyumlu Editor ile aç; paketlerin ve kaynakların içe aktarılmasını bekle.
4. `Assets/Scenes/Main.unity` sahnesini açıp **Play** düğmesine bas.
5. Gemi, seyir modu ve deniz koşullarını seç; **SEYRİ BAŞLAT**.

Unity Remote görüntüsü, telefonda çalışan bağımsız bir iOS build'i değildir; gerçek cihaz kalite/performans değerlendirmesi ayrıca yapılmalıdır.

## Yeni: sahil personeli ve baş/kıç halat manevrası

- İki rıhtımda toplam **16 yürüyen personel**; beyaz hizmet üniformasında subay/astsubay esintili, basitleştirilmiş karakterler.
- Baş/kıçta dörder kişilik sahil–güverte ekipleri; el incesini atma, karşılama, ana halatı alma, çekme ve babaya bağlama animasyonları.
- Liman modunda dört gemide de rıhtıma yaklaşınca **BAŞ · HALAT AL / KIÇ · HALAT AL** paneli açılır. Bütün makineler STOP, sürat <0,60 kn ve baş 000° ±12° olmalıdır; atış mesafesi en fazla 24 m'dir. Bunlar oyun eşikleridir.
- İlk uç bağlandıktan sonra diğer uç alınır. Gerilen iki halat Rigidbody'ye ayrı noktalardan kuvvet uygular; gemi teleport edilmez. **FORA** bağlantıları bırakır.
- Dört geminin görevleri yalnız park ederek bitmez: görevin makine/römorkör çalışması tamamlanmalı, iki halat bağlanmalı, gemi sakinleşmeli ve mevcut görev koşulları korunmalıdır. Römorkör desteği halat atılırken, çekilirken ve ilk uç bağlıyken kullanılabilir; iki uçla yanaşma tamamlanınca otomatik durur. Halat panelinde de BAŞ/KIÇ/İKİSİ DUR kumandaları vardır.
- **66 kural + 108 runtime kontrolü**; dört gemi ve iki rıhtımda sekiz gerçek fizik çevrimi geçti. Hafif denizdeki bu izole test, gerçek telefon performansı veya ağır denizde her yaklaşmanın dengelendiği anlamına gelmez.

[Halat akışı, personel, fizik ve test sınırları](docs/HALAT_VE_SAHIL_PERSONELI.md).

## Filo görevleri ve sesler

Menü → **GÖREVLER → gemini seç**. Dört geminin her birinde altı, toplam **24 görev** var: beş yanaşma + bir denize adam düştü eğitimi. Her zincir dış sahada başlar ve kendi ilerlemesini saklar.

| Gemi | Görev odağı |
| --- | --- |
| KUMKALE | Sancak/iskele aborda, dar yer, bordadan dalga, rıhtım değişimi |
| BARBAROS | Uzun gövde/atalet, iskele aborda, yalnız baş römorkörü, çift römorkör, yeniden borda/rıhtım planı |
| ANADOLU | İki römorkörle ağır gemi, ters borda, yalnız kıç römorkörü, dalgada eş güdüm, rıhtım değişimi |
| RÜZGAR | Ortak dört makine, bağımsız iç/dış makineler, dar yer, dalgalı yaklaşma, yeni rıhtım |

- Her geminin ilk görevi açık; başarıyla bitirince o geminin sıradaki görevi açılır. Yıldızlar, en iyi süre ve ilerleme bu cihazda gemi bazında ayrı saklanır. Mevcut KUMKALE görev kimlikleri/kayıt anahtarı değişmedi.
- Brifing, üç sıralı giriş kapısı, baş/kıç halatları, hedefte 5 saniye düşük sürat/paralellik, sonuç, tekrar dene ve sonraki görev akışı.
- Hafif temas yıldızı düşürür; sert temas veya zorunlu işareti zamanında vermemek başarısızlıktır. Eski tamamlamalar başarısız tekrarda silinmez.
- **48 kurgu Türkçe telsiz anonsu** (önceki 42 + kurtarma için 6 yeni), hafif parazit/squelch, altyazılar, deniz uğultusu ve sentezlenmiş gemi düdükleri. Kurtarma sırasında rutin liman sohbeti devreye girmez.
- **H / 1:** bir kısa; **2:** iki kısa; **3:** üç kısa; **L:** uzun; **5:** uyarı. Ekranda aynı düğmeler ve SES ayarı bulunur.
- Görevde görüşte eğitim gemileri vardır: sancak/iskele rota değişikliği ve **tornistan SEVK** için ilgili manevra işareti istenir. Üç kısa, sadece geriye sürüklenme demek değildir. Oyuncu sinyali kendisi seçer; dizinin bitmesi beklenir.
- 15 saniyelik görev toleransı ve tespit eşikleri oyun tasarımıdır; eksiksiz COLREG veya gerçek seyir eğitimi iddiası yoktur. Serbest seyirde ceza uygulanmaz.
- Römorkör görevlerinde baş ve kıç desteğinin **en az 3'er saniye gerçek kuvvet uygulaması** gerekir. Tek baş/kıç derslerinde önce yalnız o römorkörle 3 s çalışılır. Temassız komut veya sadece animasyon ilerleme sayılmaz; rıhtım değişince destek yeniden alınır.
- RÜZGAR'ın 2–5. görevlerinde yaklaşma alanında 5 kn altında iç/dış makine yüklerini en az 3 s farklı kullanmak gerekir. Ortak gazı oynatmak tek başına bu koşulu karşılamaz.

[Filo görevleri, özel koşullar ve test sınırları](docs/FILO_GOREVLERI.md).

[Denize adam düştü eğitimi, altıncı görev, güvenli yaklaşma ve test sınırları](docs/DENIZE_ADAM_DUSTU.md).

### Denize adam düştü — dört gemide altıncı aşama

Production zincirinde beşinci görevden sonra kurtarma eğitimi açılır; mevcut test sürümünde bütün görevler baştan açıktır. Eski görev kimlikleri ve kayıtları korunur. RÜZGAR 30 kn, BARBAROS/ANADOLU/KUMKALE 17 kn seyir ve yüklenmiş makinelerle başlar. Görev başlayınca can yelekli personel bordadan düşer; düşerken ileri hareketi korunur, suya girişte tek sıçrama oluşur. Alarm ver → MOB konumunu işaretle → geminin borda istasyonunu yaklaştır → bütün makineler STOP → kazazedeyi al. Kazazede dalgayla hareket eder; ekip sapan/halatla güverteye alma animasyonu oynatır. İstasyona mesafe ve nispi kerteriz görev kartındadır. Römorkör/rıhtım halatları bu görevde kapalıdır.

**Sürat yönü:** sürat değerinin yanındaki yukarı ok gerçek ileri, aşağı ok gerçek geri hareketi gösterir. Gaz kolunu tornistana almak, gemi hâlâ ileri akarken oku ters çevirmez. Boyuna sürat ±0,05 kn içinde olduğunda nötr çizgi gösterilir; yanal sürüklenme ileri/geri sayılmaz.

[24 Ekim hareket/römorkör/MOB değişiklikleri ve doğrulama kapsamı](docs/HAREKET_VE_KURTARMA_GUNCELLEMESI.md).

Bu görev **basitleştirilmiş oyun eğitimi**: gerçek askerî MOB talimatı, sertifikalı kurtarma simülasyonu veya sağlık eğitimi değildir. Kurtarma botu indirme, akıntı sürüklenmesi ve Williamson/Anderson dönüş değerlendirmesi bu sürümde yoktur.

[Görevler, yerel kayıt, düdük kuralları, ses üretimi ve test sınırları](docs/GOREVLER_VE_SESLER.md).

## Gemiler

| Platform | Kumanda | Oyundaki tam ileri sürat hedefi |
| --- | --- | --- |
| TCG BARBAROS — F-244 | İki bağımsız makine + dümen + liman römorkörleri | 32 kn |
| TCG ANADOLU — L-400 | İki pod için basitleştirilmiş kumanda + römorkör desteği | 21 kn |
| TCG RÜZGAR — P-344 | Dört bağımsız makine + kompakt dümen/ortak gaz | 41 kn |
| TCG KUMKALE — P-1203 | İki bağımsız makine + dümen | 25 kn |

Bunlar oyunun sürat hedefleridir; gerçek gemilerin her deniz/yük koşulundaki performansı iddia edilmez. Dönüş çapı, hızlanma, yalpa ve römorkör kuvvetleri indirgenmiş oyun modelleriyle ayarlanmıştır.

BARBAROS'ta S-70B helikopteri, hareketli baş/kıç güverte ekipleri ve köprüüstü/kırlangıç vardiyası bulunur. ANADOLU'nun geniş uçuş güvertesi, ada üst yapısı ve park edilmiş helikopteri; küçük gemilerin ise farklı gövde, üst yapı ve ekipman düzenleri vardır. Modeller kamuya açık fotoğraflara dayanan özgün görsel çalışmalardır, tersane CAD modelleri değildir.

## Oynanabilir modlar

- **Gemi Yanaştırma:** örnek limanda serbest manevra. BARBAROS ve ANADOLU'da hedef rıhtım seçimi ve iki bağımsız römorkör vardır. Ayrı GÖREVLER sekmesinde gemi başına beş sıralı yanaşma görevi bulunur.
- **Boğaz Geçişi:** Çanakkale Boğazı'nın kuzeyinden güneyine ilerleyen, 12 sıralı kontrol noktalı ayrı prototip; rota krokisi, köprü ve kıyı silüetleri.
- **Şamandıraya Bağlama:** menüde yer tutucu; henüz oynanabilir değildir.

**MENÜ / Esc** simülasyonu duraklatır. **MEVCUT SEYRE DEVAM** mevcut seferi sürdürür; yeni seyir geminin konumunu, makineleri, dümeni ve efektleri sıfırlar.

## BARBAROS / ANADOLU: römorkörlü yanaşma

Bu özellik **BARBAROS veya ANADOLU + Gemi Yanaştırma** seçiliyken açılır. Görevde rıhtım brifingden atanır; serbest seyirde liman kontrol penceresi sorar. BARBAROS'un ilk iki görevi römorkörsüz temel manevradır.

1. Liman kontrol penceresinde **doğu rıhtımı / sancak aborda** veya **batı rıhtımı / iskele aborda** seçilir.
2. Turkuaz çerçeve hedefi gösterir. İleri/tornistan ve dümen hâlâ oyuncunun kontrolündedir.
3. Römorkör üzerindeki işarete veya **BAŞ İT / KIÇ İT** kartına tıklanır/dokunulur.
4. Römorkör temasa yaklaşır ve kuvveti kademeli artırır. Tekrar tıklamak ilgili komutu, **İKİSİ DUR** iki komutu da keser.

Sancağa itmek için iskeledeki, iskeleye itmek için sancaktaki römorkörler kullanılır. İki eşit kuvvet yanal harekete; yalnız baş veya kıç römorkörü ise dönüşe de katkı verir. Geminin hareketi gerçek Rigidbody kuvvetiyle oluşur, transform ile gemi kaydırılmaz.

- Destek limana yaklaşırken de çalışır; son rıhtıma yakınlık şartı yoktur.
- Römorkörler İT kapalıyken ve 5 kn üzerinde de geminin ileri/yanal hareketine refakat eder. Temas dışında paralel gider; itmek için açıkta dönüp bordaya yaklaşır.
- Rıhtımın daralan tarafında kalırsa itişi bırakıp baş/kıç çevresinden açık-su bordasına çıkar. Görünür rıhtım kapakları, kıyı/duvar ve gemi gövdesi yol/süpürme kontrolündedir; güvenli rota yoksa bekler, duvarın içinden geçmez.
- İtme için baş **000° ±30°**, sürat **5 kn altında** olmalıdır; tam destek **2 kn ve altında** verilir.
- Temas yokken veya menü/hedef penceresi açıkken kuvvet uygulanmaz.
- Panelde gerçekten uygulanan itme yüzdesi ve **YAN ←/→ … kn** yanal sürat gösterilir. Oklar yalnız kuvvet uygulanırken çizilir.
- Römorkörlerin kendi intikali basitleştirilmiştir; örnek limana özel sınırlı yol bulma vardır. **Römorkörle yedekleme halatı**, tam bağımsız tekne hidrodinamiği ve başka haritalar/hareketli genel trafik için evrensel kaçınma henüz yoktur. Kıyıya bağlı baş/kıç halatları ayrı sistemdir.

[Araştırma, kullanım, fizik ve test sonuçları](docs/ANADOLU_ROMORKOR.md).

## Kumanda ve arayüz

- İleri **10 kademe**: +10…+100%; tornistan **5 kademe**: −20…−100%; ayrı STOP konumu.
- Yüzdelik çizgiler, sayısal etiketler ve tüm makineleri birlikte ayarlayan **ORTAK GAZ**.
- Yalnız RÜZGAR'da iki solda, iki sağda dört bağımsız kol; daha küçük merkezî kumandalar.
- Anlık sürat, baş, rota ve makine durumu; üst panelde tek **±40° analog yalpa göstergesi**. Ayrı sol yalpa kartı ve yinelenen sayısal yalpa kaldırıldı.
- Sağ üstte **2 satır × 3 sütun kare düğmeler**: 1/2/3 kısa, uzun, uyarı ve ses ayarı. Telsiz/düdük geri bildirimi mevcut ince durum şeridinde; gaz/dümen konumları korunarak orta görüş alanı açıldı.
- Ekran güvenli alanına ölçeklenen ortak koyu arayüz.

| PC girişi | İşlev |
| --- | --- |
| W / S | Tüm makineleri kademe kademe ileri / tornistan ayarla |
| Q / A | İskele makine; RÜZGAR'da dış iskele |
| E / D | Sancak makine; RÜZGAR'da dış sancak |
| Z / X, C / V | Yalnız RÜZGAR: iç iskele, iç sancak |
| ← / →, ↓ | Dümen, dümeni ortala |
| Space | Makine komutlarını sıfırla; gemi anında durmaz |
| Fareyle sürükle | Kumandalar dışındaki alanda kamerayı geminin etrafında döndür |
| Tekerlek, + / −, R | Yakınlaş/uzaklaş, başlangıç kadrajına dön |

Mobilde tek parmak kamera dönüşü, iki parmak açma/birleştirme zoom ve dokunmatik kumandalar bulunur. Kamera hareketi kumanda panelleri üzerinde başlamaz.

## Deniz ve hareket

- Menüde **5 oyun dalga seviyesi** ve 8 pusula geliş yönü; mevcut seyirde koşulları değiştirme.
- Geometrik dalga yüzeyi, hareketli küçük su detayları, baş köpüğü, pervane izi, kıyı yıkaması ve borda sıçramaları.
- Dalgalardan yalpa, baş-kıç, düşey hareket ve yatay kuvvet/rota etkisi.
- Dümen geçiş süresi, gecikmeli dönüş çemberi, dönüşte dışa yatış ve gemiye göre farklı sürat/tepki.
- Makine yüküyle artan egzoz; ani gaz açmada kısa süreli koyulaşan duman.

**Dalga seviyeleri standart bir deniz durumu/Beaufort ölçeği değildir.** Su-gövde etkileşimi tam akışkan ve deplasman hesabı içermez; dalganın güverteyi kesmesi gibi görsel/fiziksel eksikler devam eder.

## Doğrulama ve bilinen sınırlar

22 Ekim 2026 tarihli kayıtlar:

- Küçük gemi geometrisi **7.949**, model çalışma zamanı **150** kontrol.
- Dört makine kumandası **515** mantık ve **69** çalışma zamanı kontrolü.
- Sürat/dönüş/yalpa: **39 fizik senaryosu, 89 kontrol**.
- Römorkörler: **747 kural/gövde/yerleşim**, **56 entegrasyon kontrolü ve 6 fizik senaryosu**.
- KUMKALE kariyer/sinyal/ses: **84 kural kontrolü**, **85 çalışma zamanı kontrolü**; 18 Türkçe ses ve beş görev/kayıt döngüsü. Sonuç testleri kontrollü durum örnekleri kullanır; beş görevin tümünün elle baştan sona oynandığı iddia edilmez.
- Filo genişlemesi: **145 kural + 199 runtime kontrolü**, 20 kontrollü görev/kayıt döngüsü ve iki büyük gemi × iki rıhtımda dört gerçek römorkör kuvvet testi. Eski KUMKALE 84+85 ve halat 66+108 kontrolleri yeniden geçti. [Filo test kanıtları](docs/testing/20261022-fleet/).
- Ayrı Unity PhysicsScene deneylerinde üretimdeki kuvvet yolu kullanıldı. Römorkörlerle 30 saniyede yaklaşık **6,1 m** yanal hareket ve baş/kıç için zıt dönüş momentleri ölçüldü.
- PC'de rıhtım seçimi, it/dur komutları ve canlı yanal sürat kontrol edildi. Bu kayıtlar gerçek iPhone/Metal/multitouch performans testi yerine geçmez.

[Doğrulama kayıtları](docs/testing/) ve Editor'deki `Meridian/…Review` araçları ayrıntıları içerir. İnceleme araçları istek üzerine çalıştırılır; bazı eski kaynak kontrolleri Editor açılışında da rapor üretir.

Bilinen eksikler:

- Mevcut `VesselSelectionReview`, ANADOLU gövde kapaklarında kenar yönü tutarsızlığı raporluyor; bu nedenle bütün proje kontrolleri yeşil değildir.
- Oyun açıkken kaynak yeniden derlenmesi gemi tanımı/görsel eşleşmesini bozabilir. Şüpheli durumda Play'i durdurup yeni sefer başlat.
- Çanakkale coğrafyası taslak; gerçek OSM/DEM verisi ve akıntı sistemi uygulanmadı.
- Foto-gerçekçilik, tam gemi hidrodinamiği, gerçek cihaz optimizasyonu ve mağaza hazırlığı tamamlanmadı.
- Dört gemide beş yanaşma ve bir kurtarma görevi mevcut; şamandıra bağlama ve ortak boğaz kariyeri henüz yoktur. İki uçlu kıyı halatı indirgenmiş yay modeliyle çalışır; gerçek halat mukavemeti/snap-back veya sertifikalı emniyet eğitimi değildir. Kariyer yereldir; bulut ve anlık sefer konumu kaydı yoktur.

## Görev sisteminin sonraki genişlemesi — yol haritası

Dört geminin beşer yanaşma görevine birer kurtarma eğitimi eklendi. Aşağıdakiler sonraki genişleme önerileridir:

1. **Gerçek cihaz ve denge:** 20 liman görevini baştan sona oynayarak eşik/zorluk ve mobil performansı ayarla.
2. **Yeni liman düzenleri:** farklı rıhtım geometrileri, yaklaşma açıları ve kontrollü trafik koşulları.
3. **Boğaz geçişi:** kontrol noktalarını tamamla; rota dışına çıkma ve sürat ihlallerini değerlendir.
4. **İleri aşama:** sert hava, kısıtlı görüş, tek makine arızası ve şamandıra bağlama.

Brifing → hedef/uyarılar → özel makine/römorkör koşulu → iki halat → başarı/başarısızlık → değerlendirme akışı dört gemide vardır. Ortak boğaz görev değerlendirmesi henüz bu kariyere bağlanmadı.

## Belgeler

- [Ana geliştirme kaydı ve ilk MVP](docs/ILK_MVP_TASARIMI.md)
- [BARBAROS görsel incelemesi](docs/BARBAROS_GORSEL_INCELEME.md)
- [Güverte yaşamı ve S-70B](docs/GUVERTE_YASAMI.md)
- [PC/mobil kamera ve zoom](docs/KAMERA_ZOOM.md)
- [Makine yüküne bağlı duman](docs/BACA_DUMANI.md)
- [Modlar ve Boğaz Geçişi](docs/MODLAR_VE_BOGAZ_GECISI.md)
- [Kumanda ve arayüz](docs/KUMANDA_VE_ARAYUZ.md)
- [Deniz ve köpük](docs/DENIZ_VE_KOPUK.md)
- [Dalga seçimi ve gemi fiziği](docs/DALGA_SECIMI_VE_GEMI_FIZIGI.md)
- [Borda çarpması ve dalga düzeltmeleri](docs/DALGA_CARPMA_VE_DUZELTMELER.md)
- [Analog yalpa kadranı](docs/YALPA_KADRANI.md)
- [RÜZGAR ve KUMKALE](docs/RUZGAR_VE_KUMKALE.md)
- [Sürat, dönüş ve yalpa fiziği](docs/SURAT_DONUS_YALPA.md)
- [ANADOLU römorkörleri](docs/ANADOLU_ROMORKOR.md)
- [KUMKALE görevleri, kariyer, telsiz ve düdükler](docs/GOREVLER_VE_SESLER.md)
- [Dört gemi, 20 görev ve römorkör koşulları](docs/FILO_GOREVLERI.md)
