# Denize adam düştü — oyun eğitimi

24 Ekim 2026 güncellemesi. BARBAROS, ANADOLU, RÜZGAR ve KUMKALE'de altıncı görev. Menü → GÖREVLER → gemi → 06. Şimdiki test sürümünde 24 görev de açıktır; production sıralı erişim ayarı ayrı korunur. Yıldız ve en iyi süre aynı kariyerde saklanır; eski kayıt/ID'ler değişmez. Toplam 24 görev ve 48 Türkçe anons vardır.

## Oynanış

1. Liman dışı sahada RÜZGAR **30 kn**, diğer üç gemi **17 kn** gerçek Rigidbody süratiyle başlar. Makineler başlangıçta seyir yükündedir; gaz kolları normal kademeleri kullanır. Brifingde zaman durur; BAŞLA ile personel bordadan suya düşer. Düşüş, geminin başlangıç ileri süratini taşır; yükseklikten türetilmiş süre, yana açılma, savrulma ve suya giriş sıçraması vardır. ALARM VER en fazla 60 oyun saniyesinde seçilir; düşüş sürerken de alarm/işaret işlemleri yapılabilir, kurtarma suya girişten önce başlayamaz.
2. KONUMU İŞARETLE, sanal MOB konum işaretini açar; uzaktan fiziksel can simidi fırlatmak değildir. Kazazedenin zaten can yeleği vardır. Düdük düğmeleri alarm sayılmaz.
3. Mesafe ve **geminin başına göre nispi kerteriz** izlenir. İskele/sancak kurtarma istasyonu görev kartında belirtilir. Oyuncu kendi dümen/makineleriyle geri döner; gemi otomatik yanaşmaz.
4. Bordadaki orta istasyona ≤6 m, boyuna hata ≤10 m, sürat <0,50 kn, bütün makine komutları STOP ve gerçek yükleri ≤%4, dönüş <0,8°/sn, yalpa <10° olmalıdır. Kazazede gövdenin dışında kalmalıdır. 4 sn korununca KAZAZEDEYİ AL açılır.
5. Oyuncu düğmeye basar. Ekip halat/sapan animasyonuyla 8 sn içinde kazazedeyi güverteye alır. Gemiyi hareket ettiren yapay kuvvet/teleport yoktur. Her koşul izlenmeye devam eder; bozulursa kurtarma sıfırlanır ve yeniden güvenli konum gerekir.
6. Sonuç kaydedilir. Gövde veya kıç/pervane tehlike alanının kazazedeyi süpürmesi, sert çarpışma, alarmın verilmemesi veya 20 dakikalık oyun süresi aşımı başarısızlıktır. Menü ve uygulama duraklamasında eğitim zamanı ilerlemez.

Bu eşikler **oyun tasarımıdır**; gerçek geminin kurtarma sertifikası değildir. Sancak: BARBAROS/RÜZGAR; iskele: ANADOLU/KUMKALE. İstasyon ve güvenlik alanı geminin boy/en ölçülerinden türetilir. Basitleştirilmiş can yelekli karakter düşüşten sonra dalgayla yükselir; suya girdikten sonraki yatay konumu bu derste sabittir (akıntı/insan sürüklenmesi modellenmez). Çember, görünürlük için ölçeklenmiş sanal seyir işaretidir. Düşüş ve dalga hareketi görsel bir oyun yaklaşımıdır; ragdoll veya insan hidrodinamiği değildir.

## Kaynak ve kapsam sınırı

Alarm/gözcü, görsel teması koruma, yavaş yaklaşma ve pervane tehlikesini önleme ilkeleri için [Transport Canada: Person Overboard](https://tc.canada.ca/en/marine-transportation/marine-safety/person-overboard) ve [MCA: Means of recovery of persons from the water](https://www.gov.uk/government/publications/mgn-544-amendment-1-mf-means-of-recovery-of-persons-from-the-water-by-ships-and-boats-plans-procedures-and-acceptance-of-recovery-equipment/mgn-544-amendment-1-mf-means-of-recovery-of-persons-from-the-water-by-ships-and-boats-plans-procedures-and-acceptance-of-recovery-equipment) incelendi. Uygulama bunların eksiksiz prosedür kopyası değildir; gemiye özgü plan ve eğitim ihtiyacını ikame etmez.

ANADOLU dahil bütün gemilerde sapanla alma, yalnız görsel bir eğitim soyutlamasıdır; gerçek matafora/kreyn ve güverte ekipmanının birebir modeli değildir. Kurtarma botu indirme, Williamson/Anderson dönüş kontrolü, gerçek akıntı/arama, sağlık tedavisi ve gerçek askerî MOB alarm prosedürü bu sürümde yoktur. Mevcut düdük düzeni korunur; MOB için evrensel bir düdük dizisi uydurulmaz.

## Doğrulama

`Meridian/MOB - Check Rescue Rules` kuralları ve eski görev regresyonlarını çalıştırır. İzole `Meridian.Editor.ManOverboardReview.RunBatch` dört gemide gerçek controller Update döngüsü, görünür kazazede, adımlar, kapalı liman araçları, duraklama/menü, kurtarma ve yerel sonuç kaydı bağlantısını **bellek içi kayıtla** test eder. Oyuncunun gerçek ilerlemesini değiştirmez.

Kanıt: `docs/testing/20261022-mob/`. Kontrollü runtime testinde gemi güvenli konuma yerleştirilip yalnız testte Rigidbody konumu/dönüşü dondurulur; gerçek görev Update, animasyon süresi ve kayıt bağlantısı çalışır. Serbest dalga sürüklenmesi altında dört geminin tamamıyla dış başlangıçtan elle dönüp kurtarma yapıldığı anlamına gelmez. İlk serbest fizik denemesinde ANADOLU'nun dalga sürüklenmesi güvenli sürat/mesafeyi bozduğu için senaryo kendiliğinden bitmedi; oyunun güvenlik koşulları test geçsin diye gevşetilmedi. Gerçek iPhone build/dokunma performansı doğrulanmadı. Önceden var olan ANADOLU `consistent edge winding` mesh kontrol hatası bu çalışmada çözülmüş sayılmaz.

Sonuç: **141 MOB kural/kayıt kontrolü + 34 kontrollü runtime kontrolü**, dört gemide dört kurtarma/sonuç/kayıt döngüsü geçti. Önceki 20 yanaşma görevinin kural testleri ve kariyer/sinyal regresyonu da geçti; raporları aynı kanıt klasöründedir. Native Unity'de KUMKALE brifingi, alarm → işaret → yaklaşma, telsiz altyazısı ve uygun olmayan konumda kilitli kurtarma düğmesi gözle kontrol edildi.

`Meridian/MOB - Open Training Preview`, yalnız geçici bellek kaydıyla KUMKALE altıncı görev brifingini açar; oyuncu sonuçlarına yazmaz. Normal test sürümünde de bütün görevler açıktır.
