# Meridian — Filo görevleri

22 Ekim 2026. Dört gemide beşer görev, toplam 20 görev. Menü → GÖREVLER → gemiyi seç. Bu yerel çalışma için commit/push yapılmadı.

## Beşer görev

| No | BARBAROS | ANADOLU | RÜZGAR | KUMKALE (korunan) |
| --- | --- | --- | --- | --- |
| 1 | Fırkateynle sancak aborda | Ağır gemi, iki römorkör | Ortak dört makineyle sancak aborda | Sancak aborda |
| 2 | İskele aborda, uzun gövde | Batı rıhtımı, ters bordadan iki destek | İç/dış makine farkı, iskele aborda | İskele aborda |
| 3 | Önce yalnız baş, sonra çift destek | Önce yalnız kıç, sonra çift destek | Dar yer, bağımsız dört makine | İki geminin arasına |
| 4 | İki destekle dalgalı batı rıhtımı | Dalgada eş güdümlü römorkör/pod | Dalgada iç/dış makine, batı rıhtımı | Bordadan dalga |
| 5 | Yeni rıhtıma borda/destek planı | Büyük gemiyle yeniden planlama | Yeni rıhtımda bağımsız makine | Telsizle rıhtım değişimi |

Hepsi mendirek dışında başlar. ANADOLU yaklaşık 720–800 m, BARBAROS 640–720 m, küçük gemiler 560–640 m dış yaklaşma başlangıcı kullanır. Brifing onayına kadar fizik durur. İlk sürat ANADOLU 2 kn, BARBAROS/KUMKALE 3 kn, RÜZGAR 4 kn; makine komutu STOP olduğundan oyuncu sevk vermezse sürat azalır.

Üç giriş kapısı sıralı geçilir. Liman içi oyun sürat sınırları ANADOLU 5, BARBAROS 6, küçük gemiler 8 kn. Dalga seviyesi/yönü göreve atanır; deniz sayfasından atlanamaz. Beşinci görevlerde ikinci giriş kapısında batı rıhtımı emri verilir. Bunlar kurgusal eğitim limanı kurallarıdır, gerçek liman/askerî işletme limitleri değildir.

## Römorkör görevi gerçekten neyi sayıyor?

- BARBAROS 3–5 ve ANADOLU 1–5 römorkör gerektirir. BARBAROS 1–2 temel makine/dümen görevidir. Küçük gemilerde römorkör yoktur; serbest limanda iki büyük gemi de destek alabilir.
- Her destekli görevde baş ve kıç römorkörünün ayrı ayrı en az 3 s gerçek kuvvet uygulaması gerekir. Düğmeye basmak, açık mesafede yaklaşmak veya temas hâlinde komutsuz beklemek sayılmaz.
- Tek baş/kıç dersinde önce yalnız ilgili römorkör en az 3 s iter; sonra çift destek aşaması sayılır. İki komuta birlikte basarak tek destek dersi atlanamaz.
- Sayım production `TugAssistController.SimulateStep` içinde, `AddForceAtPosition` ile gerçekten uygulanan kuvvetin normalize edilmiş yükü üzerinden yapılır. Sinyal ve temas görselleriyle sınırlı değildir.
- Rıhtım değişince komutlar ve eski destek süreleri temizlenir. Römorkörler baş/kıç etrafından borda değiştirir; yeni tarafta yeniden İT komutu gerekir. Görev HEDEF düğmesi pasiftir; yalnız senaryonun atadığı rıhtım kullanılabilir.
- BARBAROS için boyuna temas istasyonu, borda yarı genişliği ve hedef çerçevesi fırkateyn modelinden alınır. ANADOLU'nun uzun temas istasyonları aynen fırkateyne kopyalanmadı.
- İtme oyun limanı alanında, baş 000° ±30° ve 5 kn altında mümkündür. Genel engelden kaçınma, gerçek bollard-pull veya römorkör yedekleme halatı hâlâ yoktur.

Römorkör kartı görev adı/kapılar, gerçek destek süreleri, atanmış rıhtım, sürat/yanal hareket ve zorunlu düdük süresini gösterir. Ayrı görev kartıyla üst üste çizilmez. Halat aşaması hazır olduğunda aynı üst alana halat kartı geçer.

## RÜZGAR'ın bağımsız makine koşulu

2–5. görevlerde aynı bordadaki iç ve dış makine çiftlerinden en az birinde gerçekleşen yük farkı ≥0,15 olmalıdır. Yaklaşma alanında, en az bir giriş kapısı geçilmişken ve sürat <5 kn iken 3 s biriktirilir. Dört koldan verilen hedef değil, gerçekleşen makine yükleri okunur. Ortak gazdaki eşit yükler sayılmaz. Rıhtım değişince yeni yaklaşmada tekrar yapılır. Eşik ve süre oyun tasarımıdır.

## Tamamlama ve kayıt

1. Üç giriş kapısı.
2. Atanmış rıhtım; varsa özel makine/römorkör çalışması.
3. Bütün makineler STOP ve düşük sürat: baş/kıç halatlarını sırayla al, ekip çeksin, iki uç bağlansın.
4. Konum/baş/boyuna ve yanal sürat, düdük ve 5 s sabitlik koşulları korununca sonuç/yıldız/sonraki görev.

Sert temas veya eksik zorunlu düdük başarısızlıktır. Tekrar, önceki en iyi sonuçları silmez. Dört geminin kilidi, yıldızları, en iyi süresi ve son görev seçimi ayrıdır. Her geminin ilk görevi başlangıçta açıktır. KUMKALE `Meridian.Career.v1.Kumkale` anahtarı ve beş mevcut görev kimliği değişmedi; eski kayıt korunur. Yeni gemiler kendi sabit kimliklerini kullanır. Bulut ve anlık sefer konumu kaydı yoktur.

## Ses

Önceki 18'e 24 yerel kurgu Türkçe WAV eklendi: 15 gemiye özel brifing ve 9 ortak filo olayı. Toplam 42. KUMKALE eski konuşmalarını kullanır; diğerleri yanlışlıkla KUMKALE diye çağrılmaz. `Tools/RadioLines.json`, paketlenen `Assets/Resources/Audio/Radio/lines.json` ile aynı metindir. Altyazı eşlemesi JSON'dan okunur. `GenerateRadio.ps1 -FleetOnly` kurulu Tolga sesiyle yalnız 24 yeni kaydı üretir; ağ/kurulum/kayıt defteri değişikliği yapmaz. Gerçek askerî haberleşme kaydı kullanılmaz.

## Doğrulama ve sınırlar

- Filo: **145 kural** ve **199 runtime** kontrolü. 20 görev/kayıt döngüsü, dört ayrı namespace, kilit atlama, eski KUMKALE JSON'undan devam, yanlış rıhtım desteğinin sayılmaması, solo dersin çift komutla atlanmaması ve yeniden planlamada kanıt temizliği test edildi.
- BARBAROS/ANADOLU × doğu/batı: **4 ayrı PhysicsScene kuvvet denemesi**, gerçek üretim kontrolcüleriyle yaklaşık **10,3–11,1 m** shoreward hareket, baş/kıç 3 s destek ve fırkateyn solo ders koşulu doğrulandı. Başlangıçta oyuncu ve römorkörlerin test konumlarına yerleştirilmesi kurulumdur; adımlarda gemi konumuna yazılmaz.
- Görev sonuç döngüleri kontrollü konum/baş/sürat ve çalışma kanıtı girdileri kullanır. 20 görevin tamamı insan tarafından dış yaklaşımdan sona kadar oynanmış/dengelenmiş sayılmaz. Gerçek itme ayrıca dört fizik denemesinde kontrol edilir.
- Test kariyeri bellek depolamasıdır; oyuncunun gerçek görevleri otomatik açılmaz/tamamlanmaz. Sesler dosyadan yüklenir; gerçek iPhone ses/dokunma/performans ve ağır deniz dengesi ayrıca test edilmelidir.
- Eski KUMKALE kariyer/sinyal/ses testleri 84+85; dört geminin halat kuralları/gerçek fizik çevrimleri 66+108 kontrolle yeniden geçti. Bunlar yeni filonun bütün yolculuklarının elle oynandığı anlamına gelmez.
- Ana Unity Editor'de dört geminin beş kartı, ayrı 0/5 ilerlemesi ve sıralı kilitleri görsel olarak kontrol edildi. ANADOLU'nun ilk römorkör görevine menüden girilip brifingin gemi adı, özel çalışma ve düdük/halat koşullarıyla doğru açıldığı görüldü. Gerçek kariyerde görev tamamlanıp kilit açılmadı; yalnız manuel başlatma normal deneme sayısını artırabilir.
- ANADOLU'nun mevcut gövde kenar-yönü kontrol uyarısı ve su/gövde görsel sorunları bu görev çalışmasında çözülmüş değildir. Tam sertifikalı eğitim veya yayın kalitesi iddiası yoktur.

Kanıtlar `docs/testing/20261022-fleet/` altında; eski görev/ses ve halat regresyonları ayrıca kaydedilir. Ortak boğaz kariyeri ve şamandıra senaryoları sonraki kapsamdır.
