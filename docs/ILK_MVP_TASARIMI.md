# Meridian — Geliştirme Kaydı ve Güncel Durum

> Güncelleme: 22 Ekim 2026
>
> Durum: **Dört gemili, iki oynanabilir modlu geliştirilmiş Unity Editor prototipi.** Gemi başına beş yanaşma ve bir denize adam düştü eğitimi, toplam 24 görev; gemiye özgü kayıt, telsiz/düdük, sahil/güverte personeli ve fiziksel baş/kıç halatları var. BARBAROS/ANADOLU görevlerinde gerçek kuvvetten sayılan römorkör çalışmaları, RÜZGAR'da bağımsız iç/dış makine koşulları uygulandı. Gerçek mobil build doğrulaması, bütün görevlerin elle oynanış dengesi, nihai görsel kalite ve yayın hazırlığı tamamlanmadı.

Bu belge güncel ana geliştirme kaydıdır. Eski MVP planı en sonda tarihsel kapsam olarak korunur; o plandaki hedefler yapılmış özellik anlamına gelmez. Özelliklerin kaynakları, test sınırları ve gönderim durumu aşağıda ayrı belirtilir.

## Proje kimliği ve kayıt düzeni

- Proje adı ve aktif ana klasör: **Meridian / meridian**.
- Aktif Unity projesi: `C:/Users/Muhammet Ali/source/repos/malibu61/meridian`.
- Bu sohbetin çalışma yolu olarak görünen `maritimeai2`, aktif Unity projesi değildir; geliştirme ve belgelerin asıl hedefi `meridian` klasörüdür.
- Amaç: başlangıçta TCG BARBAROS (F-244), sonrasında farklı gemilere genişleyebilen mobil gemi manevra oyunu.
- İlk onaylanan kapsam: basit gemi/liman, ileri–geri hareket, bağımsız makineler ve dümen. İlerleyen isteklerle gemi dış görünüşü, çevre, kamera, personel ve efektler ayrıntılandırıldı.
- Motor/dil: Unity **6000.6.6f1**, C#. Hedef: yatay ekran mobil oyun; masaüstü Unity Editor geliştirme/test ortamıdır. İlk planda Android vardı; iPhone 16 Pro üzerinde Unity Remote 5 ile görüntü/kumanda denemesi yapıldı ve iOS build profili eklendi. Bunlar bağımsız iOS/Android build performans doğrulaması değildir.
- Mevcut görseller ağırlıklı olarak C# ile üretilen prosedürel mesh ve malzemelerdir; nihai Blender/UV/PBR üretimi yapılmış sayılmaz.
- Yeni kapsam: mevcut liman çalışmasını koruyarak Çanakkale Boğazı'nın kuzeyinden güneye geçiş. Araştırmadan sonra mod menüsü, ayrı taslak çevre ve sıralı rota ilerlemesi uygulandı; gerçek coğrafi veri henüz indirilip işlenmedi.
- Aşağıdaki önceki test kayıtları korunur. Yeni menü/boğaz geliştirmesinin çalıştırılan testleri ayrıca [mod belgesinde](MODLAR_VE_BOGAZ_GECISI.md) kayıtlıdır.

## Şu anda hangi aşamadayız?

**Temel hareket demosunu geçtik; görev odaklı MVP'ye geçiş aşamasındayız.** Oyuncu gemi/mod/deniz koşulu seçip seyir yapabiliyor. Henüz bütün modlara ortak brifing, görev hedefleri, başarısızlık, puanlama ve kayıt sistemi yok.

| Aşama | Durum | Açıklama |
| --- | --- | --- |
| MVP-0: hareket ve kumanda | Temel hedef tamamlandı | İleri/tornistan, bağımsız makineler, dümen, kamera ve göstergeler |
| Genişletilmiş oynanabilir prototip | Mevcut aşama | Dört gemi, liman/boğaz, deniz koşulları, personel/efektler, ANADOLU römorkörleri |
| Görev tabanlı MVP | Dört gemide ilk sürüm uygulandı | 20 görev, gemiye özgü kilit/kayıt, brifing/sonuç, makine/römorkör ve halat koşulları; cihaz/denge doğrulaması sürüyor |
| Mobil kalite ve üretim | Bekliyor | Gerçek cihaz build'i, profil/optimizasyon, nihai görsel ve ses |
| Yayına hazırlık | Bekliyor | Eğitim, kayıt/ayarlar, kararlılık, mağaza paketleri ve kabul |

### Mevcut filo

| Gemi | Mevcut kumanda | Tam ileri oyun sürat hedefi |
| --- | --- | ---: |
| TCG BARBAROS — F-244 | Çift makine + dümen; S-70B ve 26 personelli güverte/köprü vardiyası | 32 kn |
| TCG ANADOLU — L-400 | İki pod için basitleştirilmiş kumanda; limanda iki römorkör | 21 kn |
| TCG RÜZGAR — P-344 | İki solda/iki sağda dört bağımsız makine; kompakt merkezî kontroller | 41 kn |
| TCG KUMKALE — P-1203 | Çift makine + dümen; farklı gövde ve üst yapı | 25 kn |

Yeni küçük gemilerde altışar hareketli personel bulunur; BARBAROS'un 26 kişilik ekibi bütün gemilere aynen kopyalanmış değildir. Süratler kontrollü oyun fizik testlerinde hedeflenmiştir; gerçek gemilerin her hava/yük durumundaki performansı iddia edilmez.

### Modlar ve oyuncu akışı

- **Gemi Yanaştırma:** örnek limanda serbest manevra; ANADOLU'da rıhtım seçimi ve römorkör desteği.
- **Boğaz Geçişi:** kuzeyden güneye ayrı çevre, 12 sıralı kontrol noktası, rota krokisi ve geçiş ilerlemesi.
- **Şamandıraya Bağlama:** menüde görünür; başlatma pasif, henüz oynanabilir değil.
- Açılışta gemi/mod seçimi; DENİZ sayfasında beş dalga seviyesi ve sekiz geliş yönü; seyirde MENÜ/Esc ile duraklat/devam.
- Yeni seyir konum, makineler, dümen ve efektleri sıfırlar. Boğaz kontrol noktaları ve ANADOLU'nun temel yanaşma bildirimi, ortak puanlı görev sisteminin tamamlandığı anlamına gelmez.

## Güncel geliştirme özeti

### 22 Ekim 2026 — römorkör refakati ve rıhtımdan çekilme

- Sabit dünya hızında geride kalan yaklaşma yerine, geminin ileri/yanal ve yaw kaynaklı istasyon hızını izleyen bağımsız refakat eklendi. İT komutu kapalıyken de takip; 5 kn sınırı yalnız itmeye uygulanır.
- Temas dışında paralel refakat; açıkta dönme, baş usturmaçasıyla yaklaşma, dur komutunda bordadan açılma. Gerçek temas/yük, makine/baş sınırları ve Rigidbody kuvvet yolu korundu.
- Örnek limanda kıyılar, görünür rıhtım kapakları, arka duvar, görevdeki bağlı gemiler ve diğer römorkör dikkate alınarak sınırlı yol bulma. Ana geminin etrafından baş/kıç rotası; süpürülmüş tekne izi ve dönüş kontrolüyle duvar geçişi önlenir.
- Daralan bordayı 12 oyun saniyesi önceden değerlendirme; itişi kesip açık-su bordasına çekilme. Yol yoksa güvenli bekleme, teleport/duvar içinden yetişme yok. Tam römorkör hidrodinamiği veya evrensel COLREG çarpışma önleyici değildir.
- Bütün 24 görevin test erişimi ve önceki kayıtlar korundu. Bu çalışma için commit/push yapılmadı.
- Doğrulama: 25 yol/geometri ve 185.342 örnek bazlı runtime kontrolü; dört refakat, dört kıyıdan çekilme ve altı önceki gerçek kuvvet deneyi geçti. Kontrollü ana gemi yollarıdır; gerçek cihaz performansı/serbest kalabalık liman senaryoları doğrulanmadı. Kanıt: `docs/testing/20261022-tug-navigation/`.

### 22 Ekim 2026 — bütün senaryolar test için açık

- `MissionAccessPolicy.AllScenariosUnlockedForTesting = true`: dört gemideki 24 görev, MOB dahil, ön koşulsuz seçilip başlatılabilir. Editor ile sınırlı değil; cihaz build'lerinde de aynı erişim var.
- Erişim açmak görevleri tamamlamaz/yıldız vermez; mevcut kayıt anahtarları ve gerçek sonuçlar korunur. Sırasız tamamlamaları yüklemede silen eski temizleme kaldırıldı. Devam et, tamamlanan görev sayısını indeks sanmak yerine uygun eksik görevi seçer.
- Production için politika `false` yapıldığında bütün önceki aşamalar denetlenir; sırasız test sonucu bu denetimi atlatamaz ve kayıt silinmez. Önceki belgelerdeki zorunlu sıralı açılma anlatımı production planıdır; şu an test sürümünde geçerli değildir.
- Menüde TEST SÜRÜMÜ / bütün görevler açık bilgisi görünür. Bu istek için commit/push yapılmadı.
- Doğrulama: 101 erişim/kayıt kontrolü ve 96 çalışma zamanı kontrolü geçti; temiz bellek kaydıyla 24 senaryo ters sırada gerçek görev başlatıcısından açıldı. Kanıt: `docs/testing/20261022-open-scenarios/`. Bu kontroller görevlerin tümünün baştan sona elle oynandığı anlamına gelmez.

### 22 Ekim 2026 — dört gemide denize adam düştü eğitimi

- Her geminin zincirine altıncı görev eklendi; beşinci başarıdan sonra açılır. İlk beş ID ve v1 kayıt anahtarı değişmedi. Beş görevli eski kayıtlar genişletilir; eski yıldız/süreler korunur, yeni görev kendiliğinden tamamlanmaz.
- Alarm, MOB konum işareti, nispi kerteriz/mesafe, gemiye özgü borda istasyonuna yaklaşma, STOP ve elle başlatılan kurtarma. Alarm için sıradan manevra düdüğü veya beş kısa uyarı kabul edilmez.
- Dalgayı takip eden can yelekli kazazede, işaret çemberi, halat/sapan ve ekip/güverteye alma animasyonu; altı yeni yerel Türkçe anons (toplam 48). Kurtarmada liman sohbeti, römorkör ve rıhtım halatları kapalı.
- Sürat, gerçek makine yükü, borda hizası, dönüş ve yalpa eşikleri; 4 sn sabit tutma + 8 sn kurtarma. Koşul bozulunca alma işlemi sıfırlanır. Gövde/kıç tehlike sahası ve aradaki süpürülmüş yol kontrol edilir; üstünden geçerek başarı kazanılamaz.
- Basitleştirilmiş oyun eğitimi; gerçek denizcilik/askerî acil durum talimatı değildir. Kurtarma botu, tıbbi işlem, sürüklenen arama sahası ve belirli MOB dönüş tipi puanlaması kapsam dışında. Ayrıntı ve test sınırları: [Denize adam düştü](DENIZE_ADAM_DUSTU.md).
- Doğrulama: 141 yeni kural/kayıt ve 34 kontrollü runtime kontrolü geçti; dört geminin kurtarma/sonuç/kayıt bağlantısı ve eski yanaşma regresyonları doğrulandı. Runtime güvenli konum test fikstüründe yapılır; dört serbest yaklaşmanın oynanış dengesi veya gerçek iPhone performansı doğrulanmış değildir.
- Yerel geliştirme; bu istek için commit/push yapılmadı.

### 22 Ekim 2026 — kompakt seyir arayüzü

- Analog yalpa kadranı üst gösterge paneline taşındı; ayrı sol kart ve yinelenen sayısal yalpa kaldırıldı.
- Düdük/ses düğmeleri sağ üstte 2 satır × 3 sütun kare düzenine alındı. Ses ayarları yalnız açıldığında görünür; telsiz ve düdük bildirimi mevcut ince durum şeridini kullanır.
- Gaz/dümen konumları korundu. Eski yalpa/düdük alanları kamera sürüklemesi için serbest; boğaz krokisi boşalan sol alana taşındı ve yalnız görünürken kamera girdisini engeller.
- İzole Unity doğrulaması: güvenli alanlı/alan dışı, iki/dört makineli 20 yerleşimde 1300 kontrol; düdük sırası, menüde duraklatma, telsiz ve kamera bölgeleri için 10 runtime kontrolü geçti. Gerçek iPhone build/dokunma doğrulaması değildir. Kanıt: `docs/testing/20261022-compact-hud/`.
- Projede önceden bulunan ANADOLU `consistent edge winding` mesh kontrol hatası bu arayüz çalışmasının dışında kaldı; tüm proje için hatasızlık iddiası yoktur. Bu çalışma için commit/push yapılmadı.

### 22 Ekim 2026 — dört gemiye 20 görev ve fırkateyn römorkörleri

- KUMKALE'nin mevcut beş görevi korunarak BARBAROS, ANADOLU ve RÜZGAR'a beşer yeni görev eklendi. Menüde artık dört geminin de görev zinciri var.
- BARBAROS: temel sancak/iskele aborda, yalnız baş römorkörü, çift römorkörle dalga, yeni rıhtım/borda planı. ANADOLU: çift destek, ters borda, yalnız kıçla açı kontrolü, dalgalı eş güdüm, rıhtım değişimi.
- RÜZGAR: ortak dört makine, düşük süratte bağımsız iç/dış makine, dar yer, dalga ve rıhtım değişimi. Özel görevlerde en az 3 s gerçek iç/dış yük farkı gerekir.
- Römorkörler BARBAROS gövde/temas noktası/uzunluğuna uyarlandı. Özel görevlerde iki destek de 3'er saniye gerçek kuvvet vermelidir; tek römorkör derslerinde önce tek baş/kıç çalışması gerekir. Komut düğmesi, yaklaşan görsel veya açık aralık üzerinden itme puan sayılmaz.
- Yeni rıhtım emri römorkör bordasını yeniden planlar, komutları ve eski destek kanıtını sıfırlar. Görev rıhtımı HEDEF penceresinden değiştirilemez. Halatlara geçmek için görev çalışması tamamlanmalıdır; sonunda iki halat + düşük sürat/paralellik koşulu korunur.
- Her gemide ayrı kilit/yıldız/en iyi süre/son görev; eski KUMKALE v1 anahtarı ve beş sabit kimliği değişmedi. Başka geminin ilerlemesi diğerini açmaz.
- 24 yeni yerel Türkçe kurgu anons (15 özel brifing + 9 ortak filo olayı); toplam 42 WAV. Başka gemilerin telsizi artık KUMKALE diye hitap etmez. JSON metinleri ile altyazı/ses eşlemesi aynı kaynaktan alınır.
- Filo doğrulaması: 145 kural/kayıt kontrolü, 199 runtime kontrolü, 20 kontrollü sonuç/kayıt döngüsü, BARBAROS/ANADOLU × iki rıhtımda 4 gerçek kuvvet testi. İzole sakin-su itmeleri yaklaşık 10,3–11,1 m'dir; 20 görevin insan tarafından dış başlangıçtan sona kadar oynanmış olduğu iddia edilmez.
- Ayrıntı ve kanıt: [Filo görevleri](FILO_GOREVLERI.md). Yerel geliştirmedir; bu çalışma için yeni commit/push yapılmadı. Önceki yerel değişiklikler korundu.

### 22 Ekim 2026 — sahilde personel ve fiziksel baş/kıç halatları

- İki rıhtım boyunca 16 yürüyen beyaz üniformalı personel; subay/astsubay esintili basit omuz detayları, yürüyüş ve durup bakınma. Nihai karakter/rütbe doğruluğu iddiası yoktur.
- Baş ve kıçta iki sahil + iki gemi halatçısı (toplam 8 aktif ekip personeli), iş kıyafeti/can yeleği, el incesi atış yayı ve ana halatın alınması/çekilmesi/bağlanması.
- Dört gemi + liman modunda yakında açılan BAŞ/KIÇ HALAT AL ve FORA paneli. İlk uç bağlanınca ikinci uç alınır; düşük sürat/paralellik/STOP koşulları gösterilir.
- İki bağımsız gerilim hattı, tek taraflı sönümlü yay ve Rigidbody.AddForceAtPosition: çekme gerçek yanal ilerleme ve baş/kıç dönmesi üretir, gemi konumuna yazılmaz. Menüde aşama/kuvvet durur; makine verince çekme bekler, aşırı uzaklaşma/süratte ekip bırakır.
- KUMKALE'de iki halat bağlanmadan ve gemi sakinleşmeden görev geçmez. ANADOLU'da halat kullanılırken römorkör kuvveti kesilir; yanaştı sonucu artık iki uç bağlanmasına dayanır.
- 66 kural + 108 runtime kontrolü; dört gemi × iki rıhtımda 8 izole fizik çevrimi (yaklaşık 5,2–5,7 m gerçek çekme, 57–59 oyun saniyesi) geçti. 84+85 kariyer/sinyal/ses regresyon kontrolleri de geçti; kariyer testleri bellek kaydı kullanır.
- Ağır denizde bütün görevlerin insanla tamamlanması, gerçek cihaz performansı ve gerçek halat emniyeti doğrulanmış değildir. Önceden kalan ANADOLU kenar-yönü uyarısı ve su/gövde görsel sorunları kapsam dışıdır.
- Ayrıntı: [halat ve sahil personeli](HALAT_VE_SAHIL_PERSONELI.md). Bu çalışma henüz commit/push yapılmadı; önceki yerel değişiklikler korundu.

### 22 Ekim 2026 — KUMKALE kariyeri, Türkçe telsiz ve zorunlu düdükler

- Menüye GÖREVLER sekmesi eklendi. KUMKALE için beş görev: sancak aborda, iskele aborda, iki geminin arasına, bordadan dalga ve telsizle rıhtım değişikliği.
- Başlangıçlar mendireğin dışında, yaklaşık 560–640 m mesafede. Brifingde fizik durur; oyuncu onaylayınca 3 kn ilk ileri hareketle başlar.
- Üç sıralı giriş kapısı, görev başına farklı rıhtım/koşullar, beş saniye konum/baş/düşük boyuna ve yanal sürat koşulu, temas/ihlal değerlendirmesi ve sonuç ekranı eklendi.
- Bir görevi geçince sıradaki açılır; bir yıldız yeterlidir. Eski görevler tekrar oynanır, başarısız tekrar eski başarı/yıldızları silmez.
- Sabit görev kimlikleriyle gemiye özgü sürümlü yerel kayıt; 0/5…5/5 ilerleme, kilit/hazır/tamamlandı kartları, en iyi süre ve yıldızlar. Bulut ve anlık sefer konumu kaydı yoktur.
- 18 kurgu Türkçe telsiz WAV'ı yerel Tolga sesiyle üretildi; olay anonsları, periyodik kanal konuşmaları, hafif parazit, altyazılar ve ses ayarı eklendi. Gerçek askerî telsiz kayıtları kullanılmadı.
- Sentezlenmiş düdük ve deniz uğultusu eklendi. Klavye H/1, 2, 3, L, 5 ve ekrandaki düğmelerle işaret seçilir.
- Görüşteki eğitim gemileri bağlamında 1 kısa sancak / 2 kısa iskele rota değişikliği; 3 kısa tornistan sevk izlenir. Geriye sürüklenme ile tornistan sevk ayrıldı. Sinyal kendiliğinden çalmaz; oyuncunun seçtiği dizinin bitmesi beklenir.
- Sinyalin görev toleransı içinde verilmemesi başarısızlıktır. 15 s tolerans ve tespit eşikleri oyuna aittir, tam COLREG uygulaması değildir.
- Menüde ses ve fizik duraklar; yeni sefer eski ses kuyruğunu temizler. Görev denizi sabittir, serbest seyirde ayarlar kullanılabilir.
- 84 kural/kayıt/sinyal/yerleşim kontrolü ve 85 runtime kontrolü geçti; 18 sesin yüklenmesi, beş görev/kayıt döngüsü ve önceki serbest seyir/ANADOLU/boğaz akışı doğrulandı. Testler oyuncunun gerçek kariyerini değiştirmeyen bellek depolaması kullanır.
- Beş görevin sonuç testleri kontrollü konum/sürat/baş örnekleri kullanır; hepsinin insan tarafından gerçek cihazda baştan sona oynandığı veya dengelendiği iddia edilmez.
- Ayrıntı ve kaynak: [Görevler ve sesler](GOREVLER_VE_SESLER.md); kanıt: [görev testleri](testing/20261022-missions/).
- Bu görev/ses çalışması henüz yeni commit/push yapılmış değildir.

### 22 Ekim 2026 — ANADOLU römorkörleri ve yaklaşırken itme düzeltmesi

- Deniz Kuvvetleri liman römorkörleri için ÖNDER ve Y57 fotoğrafları incelendi. Özgün gri destek römorkörü modeli; yuvarlatılmış kapalı gövde, ağır baş/yan usturmaçaları, köprüüstü camları, direk/radar, çift baca, ırgat, babalar, kıç vinci ve personel eklendi. Model, belirli bir gerçek teknenin ölçülmüş birebir kopyası değildir.
- Özellik yalnız **ANADOLU + Gemi Yanaştırma** modunda çalışır; diğer gemilere ve boğaz moduna filo eklenmez.
- Liman kontrol penceresi doğu/sancak aborda veya batı/iskele aborda hedefini sorar. Turkuaz çerçeve hedef konumu gösterir.
- Baş ve kıç römorkörleri ayrı seçilebilir. Römorkör üzerindeki işaret ya da **BAŞ İT / KIÇ İT** kartı komut verir; tekrar tıklama bırakır, **İKİSİ DUR** iki komutu keser.
- Sancağa yanaşırken iskeledeki, iskeleye yanaşırken sancaktaki römorkörler iter. Temas sonrası kademeli kuvvet uygulanır; tek römorkör ayrı dönme momenti oluşturur. ANADOLU transform ile kaydırılmaz, Rigidbody kuvvetiyle hareket eder.
- İtme 5 kn altında, tam destek 2 kn ve altında; baş 000° ±30° koşulunda çalışır. Temassız, menü/hedef penceresi açık veya yaklaşma sahası dışında itme yoktur. Hedef, baş ve düşük sürat uygun olduğunda temel “YANAŞMA TAMAMLANDI” bildirimi verilir.
- İlk denemede römorkörler bordada görünmesine rağmen son rıhtımın ±48 m alanı dışında kuvvet uygulanmıyordu. Gereksiz yakınlık sınırı kaldırılıp liman yaklaşma sahasına genişletildi; ilerlerken de destek çalışır.
- **Gerçek uygulanan itme yüzdesi ve yanal sürat** panele eklendi. Kuvvet yokken yanıltıcı ok çizilmez; temasta bekleme ile itme ayrıldı.
- Borda değişiminde geminin başı/kıçı etrafından dolaşma eklendi. Römorkörün kendi intikali basitleştirilmiş kinematik harekettir; genel engelden kaçınma ve halatla çekme yoktur.
- 747 kural/gövde/yerleşim, 56 entegrasyon kontrolü ve altı ayrı fizik deneyi geçti. İki römorkörle 30 saniyede yaklaşık 6,1 m yanal hareket; yalnız baş/kıçla zıt dönüş momentleri ölçüldü.
- Ana Unity projesinde rıhtım seçimi, bağımsız it/dur, iki römorkörü durdurma ve yaklaşık 0,35 kn canlı yanal sürat PC'de kontrol edildi. Temiz ANADOLU seferi açıldı; iPhone native build testi yapılmadı.
- Ayrıntılar ve fotoğraf kaynakları: [ANADOLU römorkörleri](ANADOLU_ROMORKOR.md). Kalıcı kanıtlar: [römorkör testleri](testing/20261022-tugs/).

### 22 Ekim 2026 — toplu commit/push ve README

- Menü/modlar, dört gemi, dört makine kumandası, dalgalar, fizik/yalpa, römorkörler, test kayıtları ve belgeler **150 dosyalık** güncelleme olarak commit edildi.
- Commit: [2ecae49](https://github.com/malibu61/meridian/commit/2ecae49f5bd2c728a3beb28451713cebf2ca0eca); hedef **malibu61/meridian → main**.
- Push sonrasında GitHub main SHA'sı yerel HEAD ile karşılaştırıldı; çalışma ağacı temizdi.
- README; kurulum, gemiler, modlar, kumandalar, römorkörler, deniz/fizik, testler, bilinen eksikler ve görev önerileriyle yeniden düzenlendi.
- **Bu ana durum belgesinin şimdiki güncellemesi, yukarıdaki commit'ten sonradır; bu belge güncellemesinde ayrıca commit/push yapılmadı.**

### 22 Ekim 2026 — RÜZGAR, KUMKALE, dört makine ve gemi hareketi

- Menüde dört gemi vardır: BARBAROS, ANADOLU, RÜZGAR ve KUMKALE. Yeni iki küçük geminin gövde/üst yapı/numara/donanımı ayrı oluşturuldu. [Fotoğraf referansları ve kapsam](RUZGAR_VE_KUMKALE.md).
- Yalnız RÜZGAR: dış/iskele, iç/iskele, iç/sancak ve dış/sancak için dört bağımsız gaz kolu; merkezde kompakt dümen ve dört makineye ortak gaz. Eski üç geminin iki kollu düzeni korunur.
- Knot hesabındaki 4:1 model ölçeği düzeltildi. Gemiye özel sürat hedefi, dümen geçişi, gecikmeli dairesel dönüş, dönüşte dışa yatış ve yönlü rezonanslı yalpa eklendi. Analog kadranın güncel aralığı ±40° oldu; aşağıdaki ±20° kayıtları tarihsel sürümdür.
- Sert borda denizi ile baş/kıç denizi artık farklı eksen tepkisi üretir. Gerçek GM/GZ/RAO, gizli manevra kartı veya devrilme sertifikası iddiası yoktur. [Sürat/dönüş/yalpa, kaynaklar ve son testler](SURAT_DONUS_YALPA.md).
- Aktif proje `meridian` klasöründedir. Bu geliştirmeler sonradan 22 Ekim 2026 tarihli `2ecae49` commit'iyle push edildi.
- Son tur: 39 fizik senaryosu/89 kontrol, 7949 yeni gövde geometri kontrolü, 150 canlı model/mod kontrolü, 515 dört-kol mantık ve 69 canlı kumanda kontrolü geçti. Önceki kontrol regresyonu 727/22 geçti. Son C# kaynak derlemesi başarılıdır.
- RÜZGAR 41, KUMKALE 25, BARBAROS 32, ANADOLU 21 kn ölçüldü. Sert borda denizinde yeni küçük gemiler yaklaşık 34,7°/33,1° tepe yalpası verdi. Bunlar kontrollü oyun fizik testi sonuçlarıdır; gerçek cihaz/deniz denemesi değildir.
- 18 offscreen gemi renderı üretildi. Batch modunda HUD görüntüsü oluşmadığı ve açık editörün fare kontrolü kesildiği için yeni HUD için etkileşimli görsel onay/gerçek parmak testi yapılmış sayılmaz. Açık eski Play oturumunun durdurulup yeniden başlatılması gerekir. Ayrıntı ve ham kanıtlar yukarıdaki fizik belgesindedir.

### 22 Ekim 2026 — sarkaçlı analog yalpa kadranı

- Referans eğimölçerin mantığında altta sıfır, çift yönlü derece yayı ve hareketli ibre eklendi. Ayrı sol üst panel ve başlıktaki sayı aynı fiziksel yalpa verisini kullanır.
- İlk sürümde ±20° olan ölçüm aralığı, küçük gemi fiziği güncellemesiyle **±40°** oldu. Dereceler geniş bir grafik yaya yayılır. Ölçek dışına çıkıldığında sayı gerçek açıyı korur, ibre sınırda kalır ve uyarı gösterilir. Bu bir gemi stabilite sınırı değildir.
- Yeni panel ortak safe-area yerleşimine/kamera engeline dahil edildi; makine/dümenle çakışmaz ve komut üretmez. Test kapsamı ve görüntüler: [Analog yalpa kadranı](YALPA_KADRANI.md).
- Genişletilmiş turda 727 mantık/yerleşim ve 22 canlı kontrol geçti. İskele 10°, denge ve sancak 10° Game View görüntüleri ayrı incelendi.

### 22 Ekim 2026 — anlık yalpa telemetrisi

- Üst başlığa ROTA ile MAKİNE arasında beşinci metrik olarak YALPA eklendi. Bir ondalık derece ve İSKELE/SANCAK/DENGE yönü gösterilir; anlık gövde açısı doğrudan `ShipBuoyancy.RollDegrees` üzerinden okunur.
- Safe-area içindeki beş metrik ortak `HelmLayout` hesabını kullanır. ±10° fiziksel gövde yatışı ve yalnız baş değiştirmede sıfır yalpa testleri iki gemi için eklendi. Ayrıntı: [Kumanda ve arayüz](KUMANDA_VE_ARAYUZ.md).
- Genişletilmiş kumanda regresyonu geçti: 529 mantık/yerleşim ve 22 canlı kontrol. HUD görüntüsü incelendi; anlık açı ve yön okunuyor, diğer göstergelerle çakışmıyor.

### 21–22 Ekim 2026 — borda çarpması, rota etkisi ve su/menü düzeltmeleri

- Dört gövde desteğine dalganın yatay orbital akışından kuvvet eklendi; gerçek Rigidbody sürüklenir ve dönüş momenti alır. Baş ve yer üzerindeki rota göstergelerinin farklılaşması artık yalnız makine/dümen kaynaklı değildir.
- İki bordada toplam sekiz temas örneği, yükselen su ve geliş açısına bağlı parçacık sıçraması ve yüzey köpüğü üretir. Efektler tekrar kullanılır; iki yeni sıçrama yayıcısının toplam üst sınırı 448 parçacıktır.
- Daha belirgin geometrik dalga, tepe köpüğü ve gemi çevresinde sıklaştırılmış su mesh'i eklendi. `Mathf.SmoothStep` kullanımından kaynaklanan, merkezde bile dalga genliğini %24'e düşüren hata giderildi. Uzak su düzlemine yakın yama maskesi uygulanarak dalga çukurlarının düz suyla örtülmesi de giderildi.
- Seçili ve seçili olmayan düğmelerin font/boyut/hizalama/padding ayarları eşitlendi. Geniş su parlaması kısıldı ve prosedürel gökyüzünün güneş diski kapatıldı; kullanıcının tam parlak-leke olayı tekil olarak yeniden üretilmiş sayılmaz.
- Kapsam, görsel kontrol ve test sonuçları: [Dalga çarpması ve düzeltmeler](DALGA_CARPMA_VE_DUZELTMELER.md). Cihaz/Metal performans onayı ve foto-gerçekçilik iddiası yoktur.

### 21 Ekim 2026 — yönlü dalga seçimi ve gövde fiziği

- Menüye DENİZ sekmesi, beş seviye ve sekiz dünya pusulası geliş yönü eklendi. Yeni seyir veya mevcut seyirde sıfırlamadan uygulama akışları ayrıldı.
- Eski görsel sinüs sallanması kaldırıldı. Dört su desteğinden gerçek Rigidbody'ye uygulanan yay-sönüm kuvvetleriyle yalpa, baş-kıç ve düşey hareket eklendi; Anadolu ve Barbaros farklı profiller kullanır.
- Su, mikro dalga shader'ı ve köpük aynı yön/seviye ayarlarını izler. Takip kamerasının ufku gövde yalpasından ayrıldı. Yatay itki/direnç korunur.
- Fizik ölçümleri, kullanım ve gerçekçilik sınırları: [Dalga seçimi ve gemi fiziği](DALGA_SECIMI_VE_GEMI_FIZIGI.md).

### 21 Ekim 2026 — deniz ve köpük yenilemesi

- Çizgili su görünümü yerine katmanlı mikro dalga shader'ı, açıya bağlı yansıma hissi ve güneş parlaması uygulandı. Gerçek sahne yansıması değildir.
- Kare köpükler, saydam/yumuşak kenarlı organik yüzey köpüğüne dönüştürüldü. İskele/sancak izleri gerçek makine yükünü, baş köpüğü ileri hızı izler. İki yana açılan baş köpüğü ve hareketli kıyı yıkaması eklendi.
- Deniz ve köpük ortak dalga yüksekliğini kullanır; gemi/mod değişiminde temizlenir, menüde duraklar. Kaynak yaşam döngüsü ve parçacık/köşe bütçeleri sınırlandırıldı.
- Araştırma değil, gerçek Unity derleme/seyir testleri ve görsel karşılaştırma yapıldı. Sonuçlar ve iPhone/native test sınırı [deniz belgesinde](DENIZ_VE_KOPUK.md) kayıtlıdır.

### 21 Ekim 2026 — kademeli gaz, ortak kol ve arayüz

- İskele/sancak makine komutları 10 ileri (`+10…+100%`) ve 5 tornistan (`−20…−100%`) kademesine geçirildi. Sıfır ayrı STOP konumudur; motorun gerçek yük artışı yumuşak kalır.
- Çizgili/sayısal gaz rayları, dümen ile sancak gazı arasında iki makineyi kontrol eden ortak kol eklendi. Ayrı komutlarda ortak panel AYRI gösterir; komutları kendiliğinden birleştirmez.
- Klavye için ilk basış ve basılı tutma kademe tekrarı uygulandı. Fare/touch yakalama, kol dışına sürükleme, iki ayrı parmakla bağımsız kollar ve ortak kolun geçici sahipliği ayrıldı.
- Menü, telemetri ve kumandalar ortak grafik diline taşındı; safe-area yerleşimi ve kamera engelleri aynı hesaptan üretilir. Mod ve gemi seçimi ile boğaz rota krokisi korunur.
- Unity'de son tur dahil 491 mantık/yerleşim, 16 canlı kumanda testi geçti. Ortak/bağımsız kol, dümen ve stop fareyle de denendi. Son düğme boyutu, görseller ve sonuçlar [kumanda belgesinde](KUMANDA_VE_ARAYUZ.md) kayıtlıdır. Gerçek iOS kurulumu/iki parmak donanım testi yapılmış sayılmaz.

Aşağıdaki önceki kayıtlar tarihsel olarak korunmuştur; sürekli gaz girişinden söz eden bölümlerin yerine artık kademeli komutlar kullanılır.

Proje, başlangıçtaki basit blok prototipten oynanabilir bir TCG BARBAROS (F-244) manevra demosuna dönüştürüldü. Şu anda Unity Editor içinde gemi hareket ettirilebiliyor, iki makine bağımsız kullanılabiliyor, dümen kontrol edilebiliyor ve kamera geminin çevresinde döndürülebiliyor.

### Tamamlanan altyapı

- Unity 6.6 ve C# tabanlı proje oluşturuldu.
- Ana sahne ve çalışma anında sahneyi kuran başlangıç sistemi hazırlandı.
- Proje Git deposuna dönüştürüldü ve `malibu61/meridian` GitHub deposuna ilk prototip gönderildi.
- Gemi dış modeli, helikopter/personel ve zoom `376beb7`; baca dumanı ve önceki belge güncellemesi `45677bb` commit'inde gönderildi. Menü ve ayrı Boğaz Geçişi dahil sonraki geliştirmeler `2ecae49` içinde gönderildi. Ayrıntı: aşağıdaki sürüm geçmişi.
- Unity dışındaki geçici derleme kopyaları ve Unity Console üzerinden kod kontrolleri yapıldı.

### Tamamlanan gemi hareket sistemi

- İskele ve sancak makineleri birbirinden bağımsız çalışıyor.
- Her makine `-100%` tornistan ile `+100%` ileri yol arasında ayarlanabiliyor.
- Makine kuvvetleri geminin kıçında, gerçek yerleşime uygun ayrı noktalardan uygulanıyor.
- Sancak makine ileri verildiğinde oluşan diferansiyel kuvvet geminin başını iskeleye çevirebiliyor; tersi de uygulanıyor.
- İleri yol, stop, tornistan ve yerinde dönüşe yakın diferansiyel manevra destekleniyor.
- Makine komutları doğrudan tam kuvvete çıkmıyor; büyük gemi hissi için gecikmeli cevap veriyor.
- Boyuna direnç, yüksek yanal direnç ve açısal sönüm uygulanıyor.
- Dümen `-35°…+35°` aralığında ayarlanabiliyor ve su akışı arttıkça etkisi güçleniyor.
- Acil stop aktif geminin bütün makine komutlarını sıfırlar; geminin mevcut süratini anında sıfırlamaz. Römorkörler için ayrıca İKİSİ DUR komutu vardır.
- Bu, ayarlanabilir bir oyun fiziğidir. Gerçek hidrodinamik katsayılar, gerçek makine tesisatı ve geminin ölçülmüş manevra karakteristiğiyle doğrulanmış bir mühendislik simülasyonu değildir.

### TCG BARBAROS (F-244) görsel modeli

Model, kamuya açık dış görünüş fotoğrafları ve MEKO 200 TN Track II-A'nın yaklaşık `118 m × 14,8 m` oranları esas alınarak prosedürel biçimde oluşturuldu.

- Yetmiş en kesitle oluşturulan kapalı gövde; uzun/dar oran, başa doğru daralan ve yükselen borda, gövdeye uyan güverte
- Eğimli ve kademeli köprü/üst yapı
- Kıç hangarı ve helikopter güvertesi
- Helikopter pisti `H` işareti
- 127 mm baş topu ve namlusu
- Ön güvertedeki VLS hücreleri
- Harpoon lançerlerini temsil eden görsel kutular
- Çift baca ve baca kapakları
- Ana mast, mast destekleri, kıç mastı ve dönen radarlar
- Radar kubbeleri ve antenler
- İskele/sancak filikaları ve CIWS silüetleri
- Borda lombozları ve güverte korkulukları
- Her iki bordada gövde yüzeyine oturan dolu stencil geometrili `F244` numarası; iskele/sancak sırası ayrı hesaplanır. Önceki havada duran TextMesh yaklaşımı bu modelde kullanılmaz.
- Kıç gönderinde prosedürel dokuyla oluşturulan Türk bayrağı
- İlk görsel salınımın yerini alan, ortak dalga alanından gerçek Rigidbody'ye uygulanan yalpa/baş-kıç/düşey hareket

Bu model önceki blok fırkateynin yerini aldı. Modelin donanım düzeninde kamuya açık 2009–2022 fotoğrafları esas alındı; modernizasyon dönemleri rastgele karıştırılmamalıdır. Ayrıntılı dış model, sekiz genel açı ve personel/helikopter yakın planlarıyla tekrar tekrar kontrol edildi. Kapalı gövde ve üst yapı yüzeyleri düzeltildi; köprü yan kapılarındaki açıklıklar personel geçişi için bilerek bırakıldı ve iç taban/bölmeler eklendi. Mevcut model fotoğraf referanslı, stilize/prosedürel bir oyun modelidir; foto-gerçekçi nihai model için Blender'da ölçülü modelleme, UV ve PBR kaplama çalışması gereklidir.

Ayrıntılı referans ve düzeltme kaydı: [Barbaros görsel incelemesi](BARBAROS_GORSEL_INCELEME.md).

### Deniz ve efektler

- Sabit düzlem yerine, gemi çevresinde sıklaştırılmış ve ortak dalga alanına göre güncellenen dinamik deniz mesh'i kullanılıyor; ilk sürümdeki yaklaşık 2.000 köşe bilgisi güncel bütçe değildir.
- Birbiriyle kesişen sinüs dalgalarıyla hareketli su yüzeyi oluşturuluyor.
- Prosedürel deniz dokusu kaydırılarak küçük yüzey hareketleri destekleniyor.
- İskele ve sancak pervaneleri için ayrı kıç suyu parçacıkları bulunuyor.
- Sürate göre artan baş köpüğü efekti bulunuyor.
- Kıyı ve liman duvarlarında köpük şeritleri bulunuyor.
- Deniz üzerindeki görsel hareket mobil performans düşünülerek düşük maliyetli tutuldu.
- İki bacaya makine yükünü ayrı izleyen egzoz dumanı eklendi; normal/ani yük davranışları aşağıdaki özel bölümde kayıtlıdır. Temel duman sürümü `45677bb`, mod değişimi sıfırlama ve sonraki uyarlamalar `2ecae49` ile gönderildi.

### Liman ve çevre

- İki kıyı şeridi, kum ve bitki katmanları
- Mendirek, iskele ve liman duvarı
- Kayalıklar ve prosedürel kıyı bitkilendirmesi
- Operasyon binası, bakım hangarı ve liman ofisi
- Konteyner alanları ve vinçler
- Deniz feneri ve yanıp sönen fener ışığı
- Kırmızı/yeşil seyir şamandıraları
- Uzak şehir yapıları
- Gün ışığı, yumuşak gölgeler, atmosferik sis ve prosedürel gökyüzü

### Kamera

- Kamera geminin konumunu yumuşak biçimde takip ediyor.
- Bilgisayarda boş deniz alanında sol tıklayıp sürükleyerek geminin çevresinde dönülebiliyor.
- Fare tekerleğiyle yakınlaşma ve uzaklaşma yapılabiliyor.
- Mobilde tek parmak sürükleme yörünge kamerasını döndürüyor.
- Mobilde iki parmak hareketi yakınlaşma ve uzaklaşmayı kontrol ediyor.
- Kamera dikey açısı ve mesafesi sınırlandırılarak suyun altına veya aşırı uzağa gitmesi engellendi.
- HUD üzerinde başlayan sürüklemeler kamera hareketine aktarılmıyor.
- PC'de `+ / -` ve sayısal klavye `+ / -` zoom yapar; `R` başlangıç kadrajına döndürür.
- Yakınlaşma sınırı önceki 17 model biriminden 6'ya indirildi; aralık 6–60, başlangıç mesafesi 34 model birimidir.
- Pinch, parmak mesafelerinin oranından hesaplanır. İki parmak da kontrol panelleri dışında başlamalıdır; panelden başlayan hareket sonradan denize taşınsa bile kamerayı ele geçirmez. Parmak değişimi, iptal ve odak kaybı ele alındı.

### Kullanıcı arayüzü ve kontroller

- Mobil yatay ekran için koyu denizcilik temalı HUD hazırlandı.
- Sol tarafta iskele, sağ tarafta sancak makine kolu bulunuyor.
- Alt orta bölümde dümen sürgüsü ve acil stop düğmesi bulunuyor.
- Canlı sürat (`kn`), baş, rota ve makine durumu gösteriliyor.
- HUD başlığında seçili geminin adı/borda numarası; sürat, baş, rota, yalpa ve makine bilgileri bulunur. Römorkör panelinde ek olarak yanal sürat gösterilir.
- Panel, kenarlık, kontrol bölgeleri ve düğmeler farklı ekran boyutlarına göre ölçekleniyor.
- Klavye desteği geliştirme ve masaüstü testi için korunuyor.

| Girdi | İşlev |
| --- | --- |
| `W / S` | Aktif gemide bütün makineleri ileri/tornistan kademeler |
| `Z / X`, `C / V` | Yalnız RÜZGAR: iç iskele, iç sancak makinelerini kademeler |
| `Q / A` | İskele makinesini ayarlar |
| `E / D` | Sancak makinesini ayarlar |
| `← / →` | Dümeni iskele/sancağa basar |
| `↓` | Dümeni ortalar |
| `Space` | Acil stop |
| Fare sürükleme | Kamerayı geminin çevresinde döndürür |
| Fare tekerleği | Kamerayı yakınlaştırır/uzaklaştırır |
| `+ / -`, sayısal klavye `+ / -` | Kamerayı yakınlaştırır/uzaklaştırır |
| `R` | Başlangıç kamera kadrajına döner |
| Tek parmak sürükleme | Mobil kamerayı döndürür |
| İki parmak hareketi | Mobil yakınlaştırma/uzaklaştırma |

### Test ve düzeltme döngüleri

- Unity Console'da C# derleme hataları sıfırlanana kadar proje yeniden derlendi.
- Gemi Unity Play modunda çalıştırıldı ve farklı kamera açılarından incelendi.
- İlk yan profil testinde görülen kısa-geniş gövde ve kutu biçimli üst yapı yeniden tasarlandı.
- Köprü, hangar, baca ve top kulesi eğimli özel mesh'lere dönüştürüldü.
- İlk aşamada ters `F244` yazısı için kamera tarafına bağlı görünürlük sistemi eklendi. Sonraki dış model geçişinde numaralar bordaya oturan stencil geometrisine dönüştürüldü; alfa kaplama parçalanması ve yazı sırası düzeltildi.
- Türk bayrağındaki gereksiz MeshCollider çalışma zamanı uyarısı giderildi.
- Kamera yakınlığı ve başlangıç açısı görsel test sonucuna göre ayarlandı.
- Git fark denetiminde boşluk veya bozuk yama hatası bulunmadı.

## Güncel aşama değerlendirmesi

| Bölüm | Güncel durum |
| --- | --- |
| Unity proje iskeleti / Git | Hazır; aktif proje meridian, son toplu geliştirme main'e gönderildi |
| Filo | BARBAROS, ANADOLU, RÜZGAR, KUMKALE seçilebilir |
| Makine/dümen | İki veya dört bağımsız makine; kademeli komut, ortak gaz, sınırlı dümen geçişi |
| Sürat ölçeği | 1 birim = 4 m hesabı düzeltildi; gemiye özel oyun hedefleri doğrulandı |
| Dönüş ve yatış | Gecikmeli dönüş çemberi ve dışa yatış uygulandı; gerçek gemi deneme verisiyle doğrulanmadı |
| Dalga ve yalpa | Beş oyun seviyesi, yön seçimi, fiziksel gövde tepkisi, ±40° kadran |
| Deniz/köpük/duman | Uygulandı; su-gövde etkileşimi ve nihai görsel kalite eksikleri sürüyor |
| Kamera / kumanda | Fare, klavye, dokunma, orbit ve pinch mevcut; native mobil doğrulama bekliyor |
| Mod menüsü | Açılış, gemi/mod/deniz seçimi, duraklat/devam mevcut |
| Liman | Taslak kıyı, rıhtım, mendirek, yapılar, fener ve şamandıralar |
| ANADOLU römorkörleri | İki bağımsız itme komutu, hedef popup'ı, gerçek yanal kuvvet ve göstergeler |
| ANADOLU yanaşma bildirimi | Temel konum/baş/sürat kontrolü var; ortak puanlı görev sistemi değil |
| Boğaz geçişi | Ayrı oynanabilir taslak; 12 sıralı kontrol noktası |
| Gerçek coğrafya / akıntı | OSM/DEM, batimetri, akıntı, streaming/floating origin uygulanmadı |
| Helikopter / personel | Park S-70B ve hareketli stilize ekipler var; uçuş ve gerçek operasyon prosedürü yok |
| Şamandıra / fiziksel halat | Kıyı baş/kıç halatları indirgenmiş kuvvet modeliyle var; şamandıra görevi ve römorkör yedekleme halatı yok |
| Görev/kariyer sistemi | KUMKALE için beş görev, brifing/sonuç, yıldız ve yerel kayıt var; diğer gemiler bekliyor |
| Çarpışma / hasar | Temel çarpıştırıcılar var; güvenilir görev ihlali/hasar değerlendirmesi tamamlanmadı |
| Mobil test / yayın | iOS profili ve Remote denemesi var; native cihaz ölçümleri/yayın paketi onayı yok |
| Ses / ayarlar / kayıt | Telsiz, düdük, deniz sesi ve ses ayarı; yerel kariyer kaydı var. Diğer ayarlar/bulut/sefer kaydı yok |
| Nihai model / PBR | Prosedürel görsel prototip var; ölçülü Blender/UV/PBR üretimi tamamlanmadı |

**Sonuç:** “Gemi kullanma ve çevreyle etkileşim prototipi” hazır; “görevlerle oynanıp cihazda doğrulanmış yayın MVP'si” henüz hazır değil.

## Önemli kaynak dosyalar

| Dosya | Sorumluluk |
| --- | --- |
| `PrototypeBootstrap.cs` | Gemi, liman, ışık, malzeme ve sahnenin çalışma anında kurulması |
| `ShipController.cs` | Gemiye göre iki/dört makine, dümen geçişi, direnç, dönüş ve sürat |
| `VesselCatalog.cs` | Dört geminin kimliği, boyutları, sürat ve oyun hareket profilleri |
| `ShipBuoyancy.cs` | Dalga örnekleme, düşey/yalpa/baş-kıç ve yatay dalga kuvveti |
| `SeaStateSettings.cs`, `WaterWaves.cs` | Deniz seviyesi/yön seçimi ve ortak dalga alanı |
| `HelmLayout.cs`, `HelmPointerRouter.cs`, `ThrottleDetents.cs` | Güvenli alan, bağımsız giriş sahipliği ve kademeli gaz |
| `MaritimeUiTheme.cs`, `RollGauge.cs` | Ortak UI teması ve analog yalpa kadranı |
| `AnadoluModelBuilder.cs` | ANADOLU gövde, uçuş güvertesi ve ada üst yapısı |
| `SmallCraftHull.cs`, `SmallCraftModelBuilder.cs` | RÜZGAR/KUMKALE gövde ve farklı donanımları |
| `SmallCraftMotion.cs`, `SmallCraftDeckLife.cs` | Küçük gemi hareketli parçaları ve altışar personel |
| `TugAssistRules.cs`, `TugAssistController.cs` | ANADOLU rıhtım hedefi, komutlar, temas, kuvvet ve yanal sürat |
| `HarborTugModel.cs` | Özgün donanma tipi liman römorkörü görseli |
| `HullWaveImpacts.cs`, `BowWaveSheet.cs`, `ShorelineWash.cs` | Borda sıçraması, baş köpüğü ve kıyı yıkaması |
| `FollowCamera.cs` | Fare ve dokunmatik yörünge kamerası |
| `OrbitZoomInput.cs` | Zoom/pinch giriş durumu, aralıklar ve panel hareketi ayrımı |
| `PrototypeHud.cs` | Göstergeler ve mobil kumandalar |
| `OceanSurface.cs` | Dinamik deniz mesh'i ve dalgalar |
| `WakeTrail.cs` | Pervane izi ve baş köpüğü |
| `ShipVisualMotion.cs` | Eski model kökü işaretçisi; güncel gövde salınımını fizik üretir |
| `RotatingRadar.cs` | Radar dönüş animasyonu |
| `BeaconPulse.cs` | Deniz feneri ışığı |
| `BarbarosModelBuilder.cs` | Kapalı gövde, donanım, stencil borda numaraları ve gemi dış modelinin üretimi |
| `SeahawkModelBuilder.cs` | Park edilmiş S-70B dış modeli |
| `NavalDeckLife.cs` | 26 personel, halat/gözcü/yürüyüş pozları ve köprü kapısı rotaları |
| `EngineExhaustResponse.cs` | Makine yükü ve ani komut değişiminden duman tepkisi hesaplama |
| `EngineExhaustSmoke.cs` | İki bacanın parçacıkları ve prosedürel bulut dokusu |
| `HullNumberVisibility.cs` | Önceki borda yazısı yaklaşımından kalan görünürlük bileşeni |
| `Assets/Shaders/Resources/HullMarking.shader` | Gövdeye oturan borda numarası malzemesi |
| `Assets/Shaders/Resources/FunnelSmoke.shader` | Baca dumanı malzemesi |
| `Assets/Editor/ProjectInitializer.cs` | Proje/sahne başlangıç kurulumu |
| `Assets/Editor/BarbarosVisualReview.cs` | Çok açılı render ve personel/rota/runtime incelemesi |
| `Assets/Editor/CameraZoomReview.cs` | 23 zoom giriş kontrolü ve kamera inceleme araçları |
| `Assets/Editor/EngineExhaustReview.cs` | 17 duman tepki kontrolü ve dört durumlu görsel çalışma |
| `GameSessionController.cs` | Aktif mod, menü/duraklat/devam, görev ilerlemesi ve bitiş |
| `ModeSelectionMenu.cs` | Mod kartları, açıklamalar, başlat/devam, görev HUD'ı ve rota krokisi |
| `StraitRoute.cs` | Elle hazırlanmış yaklaşık coğrafi merkez hattı, genişlikler ve etiketler |
| `StraitEnvironment.cs` | Ayrı taslak kıyı/tepe, köprü, yerleşim ve kontrol çemberi |
| `VoyageProgress.cs` | Sıralı ve hareket yönünü kontrol eden oyun geçiş noktaları |
| `EnvironmentResources.cs` | Mod değişiminde üretilmiş mesh/malzemelerin temizliği |
| `Assets/Editor/ModeSelectionReview.cs` | Rota, mod değişimi ve canlı hareket/duraklatma testleri |
| `Assets/Editor/ShipHandlingReview.cs` | Sürat, dönüş, dışa yatış ve dalga fiziği deneyleri |
| `Assets/Editor/SmallCraftReview.cs`, `SmallCraftControlReview.cs` | Küçük gemi modeli ve dört makine kontrolleri |
| `Assets/Editor/TugAssistReview.cs` | Römorkör gövdesi, kapsam, yaşam döngüsü ve altı fizik deneyi |

## Görev senaryoları — ilk KUMKALE zinciri uygulandı

KUMKALE'de sancak aborda, iskele aborda, dar yere yanaşma, bordadan dalga ve rıhtım değişikliği görevleri oynanabilir ilk sürüm olarak eklendi. Her biri dış başlangıç → brifing → sıralı giriş → yanaşma → sonuç akışını kullanır. Başarı yeni görevi açar; yıldızlar ve ilerleme yerelde saklanır.

Diğer gemilerin görevleri, ANADOLU'nun puanlı römorkör görevi, boğaz görev kısıtları, arıza/görüş senaryoları ve şamandıra görevi hâlâ sonraki kapsamdır. İlk beş görev için gerçek cihaz oynanış dengesi, eğitim netliği ve farklı oyuncuların tamamlayabilirliği ölçülmelidir.

## Sıradaki önerilen geliştirme sırası

1. Mevcut prototipte su-gövde kesişmesi, ANADOLU geometri uyarısı ve yeniden derlemede kimlik eşleşmesi riskini ele alma.
2. Uygulanan KUMKALE görevlerini oynanış/cihaz testleriyle dengeleme; ortak yapıya ANADOLU'nun römorkörlü görevini ekleme.
3. Liman girişini ve boğaz kontrol noktalarını aynı görev yapısına bağlama; çarpışma/sürat/rota değerlendirmesini ekleme.
4. Native iPhone/Android build'i üzerinde gerçek parmak, safe-area, kare zamanı, ısınma, bellek ve uzun oturum testi; hedef cihaz önceliğini kesinleştirme.
5. Çanakkale için gerçek kıyı/yükseklik verisi ve koordinat/ölçek standardını koruyan arazi üretimi; kalite örneği ve akıntı.
6. Gemi/sahil/su görselleri, Blender/UV/PBR, LOD ve cihaz kalite ayarlarını geliştirme.
7. Mevcut telsiz/düdük/deniz sesi ve kariyer kaydını geliştirme; kısa eğitim, diğer ayarlar ve sefer kaydını tamamlama.
8. Yayın öncesi hata/performans kabulü ve mağaza paketleri.

Bu sıra bir öneridir; belge güncellemesi bu işleri otomatik uygulamaz ve yeni commit/push başlatmaz.

## S-70B ve hareketli personel — tamamlananlar

### Helikopter

- Uçuş güvertesine park edilmiş S-70B Seahawk eklendi: kapalı gövde, kokpit/kabin camları, kapı, motor muhafazaları, burun altı radom/sensör, tekerlekli iniş takımı ve kuyruk yüzeyleri.
- Ana rotorun dört pali geriye katlanmış; dört palli kuyruk rotoru, tekerlek takozları ve bağlama zincirleri bulunur.
- Camların gövde tarafından örtülmesi yüzeye uyan geometriyle düzeltildi. Statik parçalar sekiz malzeme grubunda birleştirildi.
- Helikopter uçmaz; motor/rotor çalıştırma, iniş/kalkış ve oyuncu kontrolü eklenmedi. Gerçek seri numarası uydurulmadı.

### Personel dağılımı ve davranış

| Yer | Sayı | Davranış |
| --- | ---: | --- |
| Baş güverte | 8 | 6 halatçı + 2 baş/gövde çeviren ve dürbün kullanan gözcü |
| Kıç güverte | 8 | 6 halatçı + 2 hareketli gözcü |
| İskele köprüüstü/kırlangıç | 8 | 4 dış gözcü + iç/dış rota izleyen 4 yürüyen personel |
| Sancak köprüüstü/kırlangıç | 2 | 1 dış gözcü + 1 yürüyen personel |
| Toplam | 26 | Ekip sayıları içeride bekleyenleri de kapsar |

- Halatçılar yaklaşık 3,6 saniyelik faz farkları olan döngülerde kol/dirsek/el ve bacak pozlarını değiştirir; eller halat hattını izler.
- Eski NavalDeckLife baş/kıç baba, eğitim halatı ve yedek halat istifi dekoratif döngüdür. Yeni MooringController liman modunda buna ek, ayrı sahil bağlantılı fiziksel halat ekipleri oluşturur; makine komutu varken çekmeyi bekletir.
- Beş yürüyen personel köprü içinden yan kapıya, kırlangıç yoluna ve dış gözetleme noktasına gider; sonra geri döner. İskele ekibi aynı kapıda yığılmamak için sırayla hareket eder.
- Köprü yan duvarlarında gerçek açıklıklar, söve/eşik, içe açık kapı kanadı, iç taban ve koyu bölmeler oluşturuldu. İçerideki personel ışınlanarak değil, dış yapıyla doğal biçimde örtülür.
- Yürüyüşte adım, ayak kaldırma, diz ve karşı kol salınımı; gözcülerde baş çevirme ve dürbün hareketi bulunur. Animasyon geminin yerel koordinatlarını izler; poz tamponları tekrar kullanılır.
- Kamuya açık fotoğraflardan lacivert iş kıyafeti, siyah bot, turuncu can yeleği, reflektif ayrıntılar, eldiven ve koruyucu başlık incelendi. Köprü ekibinde lacivert kıyafet/başlık kullanıldı.
- Bunlar stilize prosedürel karakterlerdir; birebir üniforma/rütbe/arma replikası veya foto-gerçekçi insan modeli değildir. Fotoğraflar oyuna kaplama olarak indirilmedi.

Kaynak fotoğraflar, ayrıntılar ve test ölçümleri: [Güverte yaşamı](GUVERTE_YASAMI.md).

## Makine yüküne bağlı baca dumanı — tamamlanan ve gönderilen geliştirme

- Her bacada ayrı parçacık sistemi var. Çıkışlar gemiyle hareket eder; çıkan duman dünya koordinatında kalır ve gemiye yapışmaz.
- Gerçeklenen iskele/sancak makine yükleri ayrı okunur. Hafif rölanti, normal seyir ve ani yüksek yol emri birbirinden ayrılır.
- Yavaş gaz açmak siyah ani yük darbesi oluşturmaz. Sert yüklenmede duman daha koyu, yoğun ve hızlı çıkar; koni daralır, kısa ek parçacık darbesi oluşur.
- Ani etki yaklaşık 2,4 saniyede azalır. Tam yol sabit tutulunca normal tam yük seviyesine oturur; her kare yeniden tetiklenmez.
- Tornistan mutlak yükle değerlendirilir; sert yön değişimi ek darbe oluşturabilir. Gaz kısmak yeni darbe başlatmaz.
- `ShipController` yükleri salt okunur açar; duman çalışmasında itki/dümen davranışı değiştirilmedi.
- Baca başına en fazla 240 parçacık, ortak 64×64 prosedürel bulut dokusu ve özel shader kullanılır. Gerçek egzoz tesisatı/emisyon davranışının doğrulanmış modeli değildir.
- Bu temel duman dosyaları `45677bb` ile gönderildi. Sonraki menü çalışmasında mod değişiminde parçacıkların ve geçici egzoz yük tepkisinin sıfırlanması eklendi.

Ayrıntılar: [Baca dumanı](BACA_DUMANI.md).

## Toplu test ve doğrulama kaydı

Bu bölüm geliştirme sırasında kaydedilmiş sonuçları özetler. **Bu son belge güncellemesi yeni test/derleme çalıştırmadı**; kaynaklar, mevcut test kayıtları ve Git durumu kontrol edildi. Kontrol sayıları farklı test grupları/sürümlerine aittir; bağımsız benzersiz testlerin tek toplamı gibi birleştirilmemelidir.

### Son doğrulanmış kayıtlar — 22 Ekim 2026

| Alan | Sonuç | Kanıt / sınır |
| --- | --- | --- |
| Yeni küçük gemi gövdesi | 7.949 kontrol PASS | [Geometri](testing/20261022-smallcraft/model-geometry.txt) |
| Model/mod çalışma zamanı | 150 kontrol PASS | [Runtime](testing/20261022-smallcraft/model-runtime.txt) |
| Dört makine kumandası | 515 mantık + 69 runtime kontrol PASS | [Mantık](testing/20261022-smallcraft/four-control-logic.txt), [runtime](testing/20261022-smallcraft/four-control-runtime.txt) |
| Sürat/dönüş/yalpa | 39 fizik senaryosu / 89 kontrol PASS | [Fizik](testing/20261022-smallcraft/handling-result.txt) |
| Römorkör kuralları/gövde/yerleşim | 747 kontrol PASS | [Kurallar](testing/20261022-tugs/rules.txt) |
| Römorkör entegrasyonu | 56 kontrol / 6 fizik senaryosu PASS | [Runtime](testing/20261022-tugs/runtime.txt), [ölçüm](testing/20261022-tugs/physics.txt) |
| Kaynak derleme | Önceki test turlarında başarılı | Geometri kontrolündeki mevcut ANADOLU uyarısı ayrı konudur |
| PC etkileşimi | Rıhtım seçimi, bağımsız it/dur ve yanal sürat gözlendi | Native mobil cihaz testi yerine geçmez |

Kontrollü fizik deneylerinde tam ileri hedefleri RÜZGAR 41 / BARBAROS 32 / KUMKALE 25 / ANADOLU 21 kn oldu. KUMKALE tam dümen taktik çapı yaklaşık 215 m; sert borda denizinde RÜZGAR/KUMKALE tepe yalpası yaklaşık 34,7°/33,1° ölçüldü. Bunlar gerçek gemi seyir/stabilite kartı değildir. Dalga karşılaştırmasının sabit baş/konum test koşulları [hareket belgesinde](SURAT_DONUS_YALPA.md) açıklanır.

Römorkör deneyleri: iki eşit komutla 30 saniyede ±6,115 m yana hareket ve yaklaşık sıfır baş değişimi; yalnız baş/kıçla ±14,510° baş değişimi. İleri yol ve rıhtımdan uzak yaklaşma deneylerinde yaklaşık 6,102 m yanal hareket doğrulandı.

Model kontrollerinde küçük gemiler için 18 ve römorkör için üç açı render'ı üretildi. [Küçük gemiler](images/smallcraft/) ve [römorkörler](images/tugs/) kalıcı örnekleri içerir. Batch modunda küçük gemi HUD görüntüsü alınamaması ve gerçek telefon testinin yapılmaması önceki test sınırları olarak geçerlidir; sonraki ANADOLU römorkör paneli PC'de ayrıca kontrol edildi.

### Önceki sürümlerin korunan test kayıtları

Aşağıdakiler eski geliştirme döngülerinin sonuçlarıdır; yeni bütünleşik sürümün yeniden eksiksiz test edildiği anlamına gelmez.

| Alan | Kayıtlı doğrulama | Sınır |
| --- | --- | --- |
| Dış model | Sekiz genel açı; kapalı yüzey, cam, borda yazısı ve donanım kontrolü; `BARBAROS_VISUAL_REVIEW_COMPLETE` | Masaüstü Unity Editor; foto-gerçekçilik onayı değil |
| Güverte | `DECK_LIFE_CHECK_PASS`: 26 kişi, yaklaşık 0,095 birim el hareketi, 0,0000 halat sapması | Örneklenmiş animasyon geometrisi |
| Köprü vardiyası | `BRIDGE_WATCH_CHECK_PASS`: 5 rota, 8+8+8+2 dağılım, 0,770 birim yürüme ve 49,2° baş dönüşü | Rota sınırları, kapı ve taban kontrolü |
| Canlı personel | `DECK_LIFE_RUNTIME_PASS`: 98 gerçek güncelleme karesi, 0,106 el/0,708 yürüme birimi, 37,8° baş dönüşü | Play-mode animasyon; mobil performans ölçümü değil |
| Zoom | `CAMERA_ZOOM_CHECK_PASS checks=23 range=6..60` | Pinch sentetik girdilerle test edildi |
| Gerçek PC girdisi | Fareyle 34,000 → 30,155 → 34,000; sayısal klavye artı 34,000 → 33,032; `R` ayrıca denendi | Fiziksel telefon pinch testi yapılmadı |
| Duman tepkisi | `ENGINE_EXHAUST_CHECK_PASS checks=17` | Yük/ani darbe mantığı; gerçek emisyon doğrulaması değil |
| Duman karşılaştırması | Baca başına rölanti 0,80; yarım yol 7,80; ani tam yoldan 0,7 sn sonra 51,13; sabit tam yol 14,80 parçacık/sn | Görsel çalışma örnekleri; FPS ölçümü değil |
| Canlı makine–duman | İskele 0 / sancak +1: üretim 0,80 / 14,80; 52 canlı parçacık gözlendi | Bağımsız baca bağlantısı doğrulandı |
| Derleme | Önceki son kontrollerde başarılı C# derleme kayıtları; ilgili son test günlüklerinde C#/shader hatası görülmedi | Bu belge işlemi yeni derleme çalıştırmadı |

Tekrarlama araçları:

- `Meridian > Visual Review - 8 Views`: sekiz genel görünüş ve helikopter/personel için on yakın plan. Çıktı: `Temp/BarbarosVisualReview`.
- `Meridian > Camera Zoom - Check Input`: 23 giriş kontrolü. Kamera görselleri: `Temp/CameraZoomReview`.
- `Meridian > Engine Exhaust - Check Response`: 17 tepki kontrolü.
- `Meridian > Engine Exhaust - Capture Study`: ayrı geçici gemide rölanti, seyir, ani tam yol ve oturmuş tam yol. Çıktı: `Temp/EngineExhaustReview`.
- İlgili `request.txt` dosyalarındaki yeni kimlikler Play modunda inceleme isteklerini tetikleyebilir. Geçici renderlar ve test klasörleri Git'e alınmaz; kalıcı kanıt özeti ilgili Markdown belgelerindedir.
- Düzeltilen örnek sorunlar: eksik/yanlış dış yüzeyler, borda yazısı konumu/ayna sırası/alfa parçalanması, bayrak MeshCollider uyarısı, helikopter cam örtülmesi, personel kol uzanması ve boyun boşluğu. Bir inceleme sırasında Unity ILPP `pipe broken` sorunu editör yeniden başlatılarak giderildi.

## Çanakkale Boğazı — araştırma ve önerilen uygulama

**Araştırma kaydı ve güncel sınır:** Aşağıdaki araştırma kullanıcıya raporlandı. Sonrasında mod menüsü ve ayrı oynanabilir taslak çevre eklendi; aşağıdaki gerçek veri/GIS üretim planı henüz gerçekleştirilmedi. Veri dosyaları indirilip üst üste doğrulanmadı; çözünürlükler yerel doğruluk garantisi değildir. Mevcut yaklaşık merkez hattı ve prosedürel kıyılar gerçek seyir talimatı veya ölçülmüş coğrafya değildir. Ayrıntı: [uygulanan ilk mod](MODLAR_VE_BOGAZ_GECISI.md).

### Coğrafya ve güzergâh

Çanakkale Boğazı Marmara ile Ege'yi bağlar. Araştırmada yaklaşık 61 km uzunluk ve 1,2–7 km genişlik verilir; oyundaki toplam rota, seçilecek başlangıç/bitiş sınırına ve izlenen hatta bağlıdır. Geçiş düz bir güney hattı değildir; kıyılar, burunlar ve Nara çevresindeki dönüşler baş değişimini gerektirir. [Bilimsel coğrafya/akıntı kaynağı](https://agupubs.onlinelibrary.wiley.com/doi/10.1029/2012JC008348)

| Kuzeyden güneye bölüm | Önemli çevre ve oyun karşılığı |
| --- | --- |
| Marmara yaklaşımı, Gelibolu–Çardak | Açık denizden boğaza giriş, kıyıların belirginleşmesi |
| Lapseki–Sütlüce, 1915 Köprüsü | Uzaktan tanınan büyük yapı, ölçek hissi ve altından geçiş |
| Köprü güneyi–Nara yaklaşımı | Burun/koy dağılımı ve dönüşten önce görüşü kapatan kıyı |
| Nara Geçidi | Dönüş/daralma; dümen, sürat ve akıntı ilişkisi |
| Eceabat–Çanakkale–Kilitbahir | Yerleşimler, karşılıklı kıyılar, kaleler ve iskeleler |
| Kepez–Dardanos–Erenköy çevresi | Güney kıyıları, değişen yerleşim ve arazi karakteri |
| Seddülbahir–Kumkale, Ege çıkışı | Ufkun açılması, açık denize çıkış ve görev bitişi |

İlk görsel öncelikler: kıyı çizgisi, genişlik, tepe profili, burunların görüşü kapatması ve doğru yapı konumları. Özel model adayları: 1915 Köprüsü, Kilitbahir/Çimenlik kaleleri, Çanakkale kıyı kütlesi, Dur Yolcu düzenlemesi, Şehitler Abidesi ve görünür iskeleler. Köprünün ana açıklığı 2.023 m'dir; oranlar resmî ölçülerden alınmalı. [KGM proje bilgileri](https://www.kgm.gov.tr/Sayfalar/KGM/SiteTr/Projeler/ProjelerDetay.aspx?q=148), [yerel görsel referanslar](https://canakkale.ktb.gov.tr/TR-181870/slider.html)

### Önerilen veriler ve araçlar

| Katman | Kaynak / araç | Sınır ve yapılacak kontrol |
| --- | --- | --- |
| Kıyı, yollar, bina izleri, yerleşim | OpenStreetMap | Yerel kapsam/güncellik denetimi; ODbL atıf ve dağıtım koşulları |
| Arazi yükseltisi | Copernicus DEM GLO-30 | Yaklaşık 30 m örneklemeli yüzey modeli; yapı/bitki etkisi ve kıyı hataları temizlenmeli |
| Deniz tabanı | EMODnet, yerel kapsama göre | Genel ızgara yaklaşık 115 m; hassas liman/karaya oturma doğruluğu varsayılamaz |
| Belirgin yapılar | Ölçü + kamuya açık fotoğraf referansları, Blender | Fotoğraf tek başına ölçülü model veya yeniden kullanım izni değildir |
| Veri hazırlama | QGIS | Kesme, aynı koordinat sistemine getirme, su/kara maskesi ve parçalara ayırma |
| Çalışma zamanı | Unity + C# | Bu kapsam için Node.js/React Native ana oyun altyapısı olmayacak |

Copernicus DEM çıplak toprak modeli değildir; yüzeydeki yapı/bitki etkilerini içerebilir. 30 m veriyi sıklaştırmak gerçek yeni ayrıntı üretmez. Rıhtım, yakın kıyı ve keskin yapı kenarları ayrıca modellenmelidir. Su seviyesi ve yükseklik referansları birlikte kontrol edilmeli; lisans/atıf kayıtları veri paketine eklenmelidir. GLO-30 görüntüleme hizmetinde 2026 itibarıyla kayıt/lisans erişim koşulları da kontrol edilmelidir.

Kaynaklar: [Copernicus DEM](https://dataspace.copernicus.eu/explore-data/data-collections/copernicus-contributing-missions/collections-description/COP-DEM), [2026 görüntüleme erişim duyurusu](https://dataspace.copernicus.eu/news/2026-7-17-copernicus-dem-30m-view-service-license-acceptance), [EMODnet](https://emodnet.ec.europa.eu/en/bathymetry), [OSM lisansı](https://www.openstreetmap.org/copyright/en), [QGIS](https://www.qgis.org/en/).

### Büyük dünya ve ölçek önerisi

- **Güncel durum:** proje 1 Unity birimi = 4 m ölçeğini kullanır. 22 Ekim hareket güncellemesinde knot hesabındaki eksik ölçek çarpanı düzeltildi; eski birim/sn → metre/sn tutarsızlığı artık bekleyen iş değildir. Bunun düzeltilmesi gerçek gemi hidrodinamik doğrulaması anlamına gelmez.
- Önceki araştırmada 1 Unity birimi = 1 metreye geçiş bir alternatif olarak önerilmişti. **Onaylanmış/uygulanmış karar değildir; mevcut standart 4 m/birimdir.** Gelecekte değiştirilirse model, coğrafya, kamera, kuvvetler ve efektler birlikte yeniden kalibre edilmelidir.
- Başlangıç önerisi: yaklaşık **2×2 km** arazi parçaları; yakında ayrıntılı, uzakta sade arazi/tepe silüeti. Boyut ve görünürlük mesafeleri gerçek mobil ölçümle kesinleşmeli.
- Veriler önceden hazırlanıp yerelden yüklenmeli; ilk sürüm için sürekli internet veya canlı harita akışı zorunlu tutulmamalı.
- Unity Terrain komşulukları ve asenkron/eklemeli sahne yükleme kullanılabilir. Addressables henüz projeye kurulmuş kabul edilmez.
- Gerçek konumu yüksek hassasiyette ayrı saklama ve yerel dünyayı gemiye yakın tutan floating origin önerilir. Gemi, kamera, kıyı, dünya koordinatlı duman ve kıç suyu birlikte ele alınmalıdır.
- Liman deniz yaması `300×360` model birimi, liman kamerası uzak kesme düzlemi `420` birimdir; boğaz ayrı çevre/kamera mesafesi kullanır. Küçük liman düzenini yalnızca büyütmek bütün boğazı çözmez. Görünür su, uzak kıyılar, yükleme ve LOD ayrı tasarlanmalıdır.

Teknik kaynaklar: [Unity komşu arazi parçaları](https://docs.unity3d.com/6000.0/Documentation/Manual/terrain-CreateNeighborTerrains.html), [Addressables sahne yükleme](https://docs.unity3d.com/Packages/com.unity.addressables@2.7/manual/LoadingScenes.html).

### Su, akıntı ve seyir süresi

- Genel su alışverişi üst tabakada Marmara→Ege, altta ters yöndedir. Kısa dönem değişimleri ve tersine dönüşler gözlenmiştir; bütün boğaza sabit bir güney vektörü uygulanması doğrulanmış akıntı modeli sayılmaz. [Gözlemsel çalışma](https://agupubs.onlinelibrary.wiley.com/doi/10.1029/2012JC008348)
- İlk oyun önerisi: kontrollü hava altında konuma bağlı, yumuşak değişen basit yüzey akıntısı. Gerçek zamanlı bilimsel akış/CFD çözümü ilk MVP kapsamında değil.
- Suya göre sürat dümen/direnç hesabında, yere göre sürat harita ilerlemesinde ayrı kullanılmalı. Motor stopta akıntıyla sürüklenme desteklenmeli. Mevcut kodda bu akıntı katmanı yok.
- Görsel rüzgâr dalgası, akıntı hissi, kıyı köpüğü ve gemi izi ayrı katmanlar olarak ele alınmalı.
- Örnek 65 km rota, akıntı/manevra yok sayılırsa 15 kn ile yaklaşık 2 sa 20 dk, 20 kn ile 1 sa 45 dk sürer. Bu, son rota ölçümü değil tasarım hesabıdır.
- Öneri: coğrafi ölçeği koruyup tam geçişe ek kısa bölümler, kontrol noktasından devam ve uygun kesimlerde isteğe bağlı zaman hızlandırma sunmak. Dar/geçiş hassasiyeti yüksek yerlerde hızlandırma sınırlandırılmalı.

### Önerilen coğrafya üretim sırası ve kabul

1. Bütün boğazın gerçek veriye dayalı sade kıyı/tepe iskeleti ve kuzey–güney geçişi.
2. Nara–Çanakkale–Kilitbahir kesimini ayrıntılı kalite örneği yapma; diğer bölümler için referans standardı.
3. Köprü, yerleşimler ve güney çıkışını aynı görsel standarda taşıma.
4. Akıntı, görev kontrol noktaları, harita, kayıt/devam ve cihaz performansı. Feribot/diğer gemi trafiği sonraki kapsam.
5. Liman çalışması korunur. Kurgusal eğitim limanı ayrı kalabilir; gerçek boğaza bağlanacaksa gerçek liman konumu/geometrisi ayrıca seçilmelidir.

Kabul: kesintisiz tam geçiş; gemi seviyesinden doğru kıyı/tepe silüeti; tutarlı konum/mesafe/sürat; arazi birleşimlerinde boşluk olmaması; görünen kıyı ile çarpışmanın uyuşması; orbit/zoom'da eksik yüzey görülmemesi; uzun seyirde koordinat/efekt sıçraması ve bellek birikimi olmaması. Belirlenecek hedef telefonda en az 30 FPS bir önerilen hedeftir, ölçülmüş sonuç değildir.

**Açık kararlar:** kesin başlangıç/bitiş, güncel coğrafya ile geminin 2009–2022 görünümünün zaman kurgusu, hedef telefon, ilk ayrıntılı liman, tam geçiş/kısa görev dengesi ve verilerin yerel kalite sınırı. Araştırma yapılmış olması bu kararların onaylandığı veya coğrafyanın tamamlandığı anlamına gelmez.

## Sürüm geçmişi ve gönderim durumu

Uzak depo: [malibu61/meridian](https://github.com/malibu61/meridian), dal: **main**.

| Tarih | Commit | Kapsam |
| --- | --- | --- |
| 18 Ekim 2026 | `dbfecce` | İlk Meridian MVP-0 gemi manevra prototipi |
| 18 Ekim 2026 | `e9ba14f` | BARBAROS görsel MVP ve çevre iyileştirmesi |
| 19 Ekim 2026 | `376beb7` | Dış model, helikopter/personel ve kamera zoom |
| 19 Ekim 2026 | `45677bb` | Makine yüküne bağlı baca dumanı ve belgeler |
| 22 Ekim 2026 | `2ecae49` | Filo/modlar, dört makine, deniz/fizik/yalpa, ANADOLU römorkörleri, test kanıtları ve README |

Son gönderilmiş tam SHA: `2ecae49f5bd2c728a3beb28451713cebf2ca0eca`. Önceki push işlemi sonrasında `git ls-remote` ile uzak main eşleşmesi ve temiz çalışma ağacı doğrulandı.

**2ecae49 sonrasında ana belge, KUMKALE görevleri ve ses sistemi yerel olarak geliştirildi. Bu son görev/ses çalışması için yeni commit/push yapılmadı.** Eski bölümlerdeki “yerel” ifadeleri o geliştirme anını anlatır; yukarıdaki gönderilmiş kapsam esas alınmalıdır.

## Bilinen sınırlar ve yayın öncesi eksikler

1. **Su-gövde/güverte kesişmesi:** deniz yüzeyi gövdeye göre dışlanmıyor; gerçek güverteyi basan suyun akışı/tahliyesi hesaplanmıyor. Gemi ve personel dalganın altında görünebiliyor. Gerçek dalga aşması mümkün olsa da her kesişme “gerçekçi” kabul edilmedi; çözüm henüz uygulanmadı.
2. **ANADOLU geometri kontrolü:** mevcut `VesselSelectionReview`, gövde kapaklarında kenar yönü tutarsızlığı raporluyor. Römorkör/yeni küçük gemi kontrolleri geçse de bütün proje testleri yeşil değildir.
3. **Play sırasında yeniden derleme:** gemi tanımı ile görselin farklı kalması riski/gözlemi var. Kaynak yeniden derlendikten sonra şüpheli durumda Play durdurulup yeni sefer başlatılmalı; kalıcı yaşam döngüsü düzeltmesi doğrulanmadı.
4. **İndirgenmiş fizik:** metre/knot ölçeği düzeltildi ve Rigidbody tepkileri mevcut; gerçek GM/GZ/RAO, deplasman, tam akışkan veya ölçülmüş gemi manevra modeli değildir. Dalga 1–5 seviyesi standart Beaufort/WMO ölçeği değildir.
5. **Görev sistemi:** KUMKALE için beş görev ve kariyerin ilk sürümü var; diğer gemilerin görevleri, kapsamlı dengeleme ve cihaz doğrulaması yok. Anlık sefer konumu/bulut kayıt edilmez.
6. **Römorkör sınırı:** kuvvet oyunda fizikseldir; römorkörün kendi seyri kinematiktir. Halatla çekme, gerçek bollard-pull kalibrasyonu ve genel engelden kaçınma yoktur.
7. **Gerçek cihaz:** iPhone 16 Pro / Unity Remote 5 denemesi, bağımsız telefonda çalışan uygulama testi değildir. Native iOS/Android, Metal, çoklu dokunma, FPS/kare zamanı, ısınma ve bellek profili doğrulanmadı.
8. **Görsel üretim:** modeller/üniformalar stilize ve prosedüreldir. Yakın plan foto-gerçekçilik, ölçülü Blender modeli, UV/PBR ve bütün açılarda nihai kalite tamamlanmadı.
9. **Personel/helikopter:** sahil/güverte personeli prosedürel animasyon, kıyı baş/kıç halatları basitleştirilmiş yay fiziğidir; gerçek personel emniyeti, halat mukavemeti/snap-back simüle edilmez. Helikopter park modelidir; uçuş/kalkış/iniş yoktur.
10. **Coğrafya:** Çanakkale taslak; OSM/DEM/batimetri doğrulaması, akıntı, büyük dünya koordinat/yükleme altyapısı ve trafik tamamlanmadı.
11. **Ürünleşme:** telsiz/düdük/deniz sesi, ses ayarı ve yerel kariyer kaydı var; kapsamlı ayarlar, sefer kaydı, tam eğitim, hasar, yapay zekâ, çok oyunculu ve savaş sistemleri tamamlanmadı. Şamandıra bağlama henüz oynanabilir değildir.
12. **Yayın:** cihaz kalite bütçeleri, uzun oturum regresyonu, yayın kabul kriterleri ve mağaza paketleri tamamlanmadı.

## Ayrıntılı belge dizini

- [Barbaros görsel incelemesi](BARBAROS_GORSEL_INCELEME.md): fotoğraf kaynakları, donanım dönemi, geometri düzeltmeleri ve sekiz açılı kontrol.
- [Güverte yaşamı](GUVERTE_YASAMI.md): S-70B, kıyafet referansları, 26 personel ve canlı animasyon testleri.
- [Kamera zoom](KAMERA_ZOOM.md): fare/klavye/mobil girdi davranışı ve 23 kontrol.
- [Baca dumanı](BACA_DUMANI.md): yük tepkisi, 17 kontrol, dört görsel durum ve yerel gönderim sınırı.
- [README](../README.md): projeyi açma, kontroller ve güncel genel özet.
- [RÜZGAR ve KUMKALE](RUZGAR_VE_KUMKALE.md): model referansları, dört makine ve küçük gemiler.
- [Sürat, dönüş ve yalpa](SURAT_DONUS_YALPA.md): ölçek düzeltmesi, fizik modeli ve ölçümler.
- [ANADOLU römorkörleri](ANADOLU_ROMORKOR.md): araştırma, hedef/komut, itme düzeltmesi ve testler.
- [Kumanda ve arayüz](KUMANDA_VE_ARAYUZ.md), [analog kadran](YALPA_KADRANI.md).
- [Deniz/köpük](DENIZ_VE_KOPUK.md), [dalga seçimi](DALGA_SECIMI_VE_GEMI_FIZIGI.md), [borda çarpması](DALGA_CARPMA_VE_DUZELTMELER.md).
- [Görevler ve sesler](GOREVLER_VE_SESLER.md): beş KUMKALE görevi, kariyer, telsiz/düdük ve kaynaklar.
- [Kalıcı test kayıtları](testing/): küçük gemi/fizik, römorkör ve görev/ses sonuçları.
- [Modlar ve Boğaz Geçişi](MODLAR_VE_BOGAZ_GECISI.md): uygulanan menü, mod yaşam döngüsü, taslak coğrafya ve yeni testler.

---

## Başlangıçta hazırlanan MVP-0 planı

Aşağıdaki bölümler projenin geliştirme öncesindeki ilk kapsam kararlarını korumaktadır.

## İlk teknik prototip: MVP-0

Bu ilk adım özellikle küçük tutulacaktır:

- Basit ve ayrıntısız gemi silüeti
- Sakin deniz ve basit liman blokları
- Bağımsız iskele ve sancak makineleri
- İleri yol, stop ve tornistan
- İki makine arasındaki kuvvet farkından doğan basit dönüş
- Geçici mobil kontrol arayüzü ve klavye kontrolleri

Yanaşma görevi, ayrıntılı model, gelişmiş su fiziği ve görsel iyileştirmeler bu temel hareket sistemi onaylandıktan sonra eklenecektir.

## 1. Proje fikri

Oyuncu, Türk Deniz Kuvvetleri gemilerinden esinlenen bir fırkateyni liman içinde düşük süratte kumanda eder. İlk sürümün amacı savaş veya görev sistemi değil; geminin çift makine, dümen, ileri yol ve tornistan davranışını anlaşılır ve keyifli biçimde hissettirmektir.

İlk prototipte gemi, Barbaros sınıfının genel oranlarını çağrıştıran basit bir silüet olacaktır. Kusursuz ve ayrıntılı TCG BARBAROS modeli, hareket sistemini doğruladıktan sonraki ayrı bir üretim aşamasıdır.

## 2. İlk MVP'nin tek cümlelik hedefi

Oyuncu, basit bir limanda sancak ve iskele makinelerini ayrı ayrı kullanarak gemiyi ileri–geri götürebilmeli, döndürebilmeli ve işaretli yanaşma bölgesine getirebilmelidir.

## 3. Platform ve teknoloji

- Oyun motoru: Unity 6, C#
- Hedef cihaz: İlk olarak Android telefon
- Ekran yönü: Yatay
- 3B üretim: Blender
- Görsel yaklaşım: Temiz, okunaklı, düşük ayrıntılı prototip
- Kamera: Gemiyi arkadan ve hafif yukarıdan izleyen takip kamerası

## 4. Oynanış döngüsü

1. Oyuncu limanda hareketsiz başlar.
2. İskele ve sancak makine kollarını bağımsız olarak ayarlar.
3. Dümeni sancak veya iskeleye basar.
4. Geminin süratini, baş yönünü ve kıç suyunu takip eder.
5. Gemiyi yeşil hedef alanına düşük süratle getirir.
6. Konum, yön ve sürat uygunsa görev tamamlanır.

## 5. MVP kontrolleri

| Kontrol | Aralık | Etki |
| --- | ---: | --- |
| İskele makine | -100…+100 | Tornistan, stop ve ileri yol |
| Sancak makine | -100…+100 | Tornistan, stop ve ileri yol |
| Dümen | -35°…+35° | İskele veya sancak dönüşü |
| Kamera | Tek düğme | Takip ve üstten görünüm arasında geçiş |
| Acil stop | Tek düğme | İki makineyi sıfırlar |

Örnek davranış: Sancak makine ileri, iskele makine stop durumunda geminin başı iskeleye yönelir. İki makine eşit ileri çalışırsa gemi düz ilerler. Biri ileri, diğeri tornistan çalışırsa gemi düşük süratte kendi boyuna yakın bir dönüş yapar.

## 6. Basitleştirilmiş fizik modeli

İlk MVP tam hidrodinamik simülasyon olmayacak; oyuncuya doğru hissi veren, ayarlanabilir bir model kullanılacaktır.

- Her makine gemiye ayrı bir ileri/geri kuvvet uygular.
- Kuvvetler geminin kıçında ve sancak/iskele tarafında uygulanır.
- Dümenin etkisi ileri veya geri su akışı arttıkça güçlenir.
- Su direnci ileri hareketi ve yana kaymayı yavaşlatır.
- Büyük gemi hissi için komutlara gecikmeli tepki ve yüksek atalet uygulanır.
- Çarpışmalar basit gövde çarpanı ile hesaplanır.

## 7. İlk sahne

- Sakin deniz yüzeyi
- Basit rıhtım ve mendirek silüeti
- Bir şamandıra hattı
- Başlangıç noktası
- Yeşil yanaşma/hedef alanı
- Basit fırkateyn silüeti
- Kıç suyu ve pervane çalışmasını anlatan temel parçacık efekti
- Gün ışığı ve hafif pus

## 8. Ekranda gösterilecek bilgiler

- Sürat: knot
- Baş açısı: derece
- İskele ve sancak makine yüzdeleri
- Dümen açısı
- Hedefe kalan mesafe
- Kısa durum mesajları: `STOP`, `İLERİ`, `TORNİSTAN`, `HEDEFTE`

## 9. MVP dışında kalanlar

- Silah sistemleri ve muharebe
- Çok oyunculu mod
- Açık dünya
- Dalga, fırtına ve gelişmiş hava sistemi
- Gerçek köprüüstü ekipmanı
- Hasar modeli
- Ayrıntılı liman trafiği ve yapay zekâ
- Kusursuz gemi modeli ve foto-gerçekçi kaplamalar
- iOS derlemesi

## 10. Kabul ölçütleri

MVP tamamlanmış sayılacaksa:

- Android telefonda akıcı çalışmalı.
- İki makine birbirinden bağımsız kumanda edilebilmeli.
- İleri, tornistan, diferansiyel dönüş ve dümen davranışları gözle anlaşılır olmalı.
- Gemi komutlara aniden değil, ağır bir platform gibi tepki vermeli.
- Rıhtıma çarpma algılanmalı.
- Hedef alana doğru yön ve düşük süratle girildiğinde görev tamamlanmalı.
- Kontroller ilk kez oynayan biri tarafından kısa açıklamayla anlaşılmalı.

## 11. Onaydan sonra geliştirme sırası

1. Unity proje iskeleti ve Android ayarları
2. Deniz, liman blokları ve geçici gemi gövdesi
3. Çift makine kuvvet modeli
4. Dümen ve su direnci
5. Mobil kumanda arayüzü
6. Kamera, göstergeler ve kıç suyu
7. Yanaşma hedefi ve görev sonucu
8. Telefonda performans testi

## 12. Birlikte kararlaştıracağımız noktalar

- Ana görünüm: köprüüstüne yakın takip mi, üstten taktik görünüm mü?
- Kontrol biçimi: dikey makine kolları mı, büyük dokunmatik düğmeler mi?
- İlk görev: düz rotada duruş mu, liman içinde yanaşma mı?
- Görsel ton: gerçekçi gri/deniz tonu mu, daha sade eğitim simülatörü mü?
- Geminin ilk silüeti: çok basit blok model mi, Barbaros oranlarına daha yakın orta ayrıntı mı?

