# Dalga seçimi ve fiziksel gemi tepkisi

21 Ekim 2026. Beş oyun seviyesi; resmî deniz durumu/Beaufort ölçeği veya doğrulanmış gemi stabilite hesabı değildir.

Sonraki borda sıçraması, yatay kuvvet/rota etkisi, daha dik dalga ve menü düzeltmeleri: [Dalga çarpması ve düzeltmeler](DALGA_CARPMA_VE_DUZELTMELER.md). Aşağıdaki RMS tablosu önceki dalga boylarıyla alınan tarihsel ölçümdür; son ayarların ölçümü olarak kullanılmamalıdır.

## Menü

Menünün üstündeki **DENİZ** sekmesinde 1–5 dalga seviyesi ve dalganın geldiği pusula yönü seçilir. Yön dünya referanslıdır, gemiye göre bağıl değildir: K (000°), KD (045°), D (090°), GD (135°), G (180°), GB (225°), B (270°), KB (315°). Kuzeyden gelen dalga güneye ilerler. Ortadaki ok yayılma yönünü gösterir.

1. Dingin
2. Hafif
3. Orta
4. Dalgalı
5. Sert

Seçim tek başına aktif denizi değiştirmez. **SEYRİ BAŞLAT** seçili gemi/mod/denizle yeni seyir kurar. Aktif seyir varsa **UYGULA VE DEVAM**, geminin yerini, makinelerini veya rota ilerlemesini sıfırlamadan yeni denizi uygular. Dalga alanı dört simülasyon saniyesinde yeni duruma geçer. Normal **MEVCUT SEYRE DEVAM** bekleyen menü seçimlerini uygulamaz; aktif ayarları korur. Seyir durum çubuğunda aktif dalga seviyesi/yönü görünür.

Menüde küçük hareketli önizleme vardır; bu önizleme duraklatılmış gemiyi hareket ettirmez. Oyun alanı ve gerçek fizik menüde durur.

## Fiziksel hareket

Eski `ShipVisualMotion` içindeki bağımsız sinüs sallanması kaldırıldı. Gövdenin gerçek Rigidbody'sinde Y konumu ile X/Z dönüşleri artık kilitli değildir. `ShipBuoyancy`, iskele/sancak ve baş/kıç yönündeki dört noktada ortak su yüzeyini örnekler. Yükseklik farkı ve noktanın dikey hızından yay-sönüm kuvveti hesaplanır, `AddForceAtPosition` ile gerçek fizik gövdesine uygulanır.

- Yandan dalga daha çok yalpa (roll), baş/kıç doğrultusundaki dalga daha çok baş-kıç hareketi (pitch) oluşturur.
- Gövde ayrıca düşey hareket eder (heave). Sabit bir sağ-sol animasyonu değildir; geminin başı değişince dalgaya bağıl açısı ve cevabı da değişir.
- Anadolu'nun boyutu, destek aralıkları ve daha ağır sönüm profili Barbaros'tan farklıdır.
- Gerçek çarpışma kutusu, güverte ekibi ve üst yapı gövdeyle birlikte hareket eder.
- İtki ve boyuna/yanal direnç yatay düzlemde kalır; makine vermek yanlışlıkla kaldırma kuvvetine dönüşmez. Dümen/pod ve iki makine kumandaları korunur.
- Takip kamerası geminin başını izler, yalpayı kopyalamaz; ufuk kamerayla yan yatmaz.

Suyun geometrisi, mikro dalga yönü/şiddeti, baş/kıç köpükleri ve kıyı yıkaması aynı dalga ayarına bağlıdır. `OceanSurface` bounds yüksekliği 5. seviyeyi içerecek şekilde genişletildi. Önceki su testinin sabit yükseklik sınırı yeni seçilen profilin sınırını kullanır.

## Teknik kapsam ve sınırlar

`SeaStateSettings` seçim/doğrulama, `WaterWaves` yönlü bileşik dalga alanı ve geçiş, `ShipBuoyancy` dört noktalı kuvvet modeli, `GameSessionController` aktif ayar/yeni seyir/uygulama akışı içindir. Yön katsayıları ayar değişiminde önceden hesaplanır; her su köşesinde yeniden pusula trigonometri hesabı yapılmaz.

Görsel model 1:4 ölçeklidir. Seviyeler oyun için ayarlanmış genlik/dalga boyudur; gerçek anlamlı dalga yüksekliği, rüzgâr hızı, gerçek gemi GM/RAO değerleri veya sertifikalı stabilite hesabı olarak sunulmaz. Gerçekçi his amaçlayan yay-sönüm yaklaşımıdır; tam altı serbestlik dereceli deniz mühendisliği simülasyonu değildir. Batma/alabora/hasar, su alma, rüzgâr ve akıntı bu değişiklikte uygulanmadı.

Native iPhone/Metal ve gerçek cihaz performans testi ayrıca gereklidir. Windows Unity Editor testi, Unity Remote aktarım kalitesi anlamına gelmez. Önceden kalan Anadolu kapak/gövde geometri testi hatası bu değişiklikle giderilmiş sayılmaz.

## Doğrulama

`Meridian > Sea - Check Levels and Directions`: beş seviyede artan genlik, sekiz yönde dalganın geldiği yönün tersine ilerlemesi, sınırlar ve geçersiz girdiler. İlk sonuç: `SEA_LOGIC_CHECK_PASS checks=1287`.

`Meridian > Sea - Run Physical Review`: Barbaros'ta beş seviye yandan dalga, ayrıca baştan dalga; Anadolu'da boğazda güçlü yandan dalga. Gövde kök Rigidbody açılarının RMS değerleri ölçülür; görsel alt objenin açısı ölçülerek fizik varmış gibi raporlanmaz. Duraklama, kamera ufku ve mevcut seyirde ayar uygularken kumanda/rota korunması kontrol edilir.

Son turda `SEA_LOGIC_CHECK_PASS checks=1289` ve `SEA_RUNTIME_CHECK_PASS checks=63` alındı. Mantık kontrollerine geçiş devam ederken tekrar ayar değiştirmede yükseklik/normal sürekliliği de dahil edildi. Son fizik turu `Logs/play-session-restart.log` içinde 4443/4674 satırlarında kayıtlıdır. Her fizik senaryosu yaklaşık 9 saniye örneklenmiş, menüde 0,5 saniye gerçek zaman beklenerek gövdenin durduğu doğrulanmıştır.

Yeni fizik açıkken kumanda regresyonu da yeniden geçti: `HELM_LOGIC_CHECK_PASS checks=491` ve `HELM_RUNTIME_CHECK_PASS checks=16` (4686/4698). Gerçek menü tıklamalarıyla DENİZ → 5 → D 090° → UYGULA VE DEVAM denendi; durum çubuğu 5/5 ve Doğu gösterdi, iki gaz komutu +60% olarak korundu ve seyir devam etti.

Son tur RMS ölçümleri (oyun gövdesi, derece):

| Gemi / koşul | Yalpa RMS | Baş-kıç RMS |
|---|---:|---:|
| Barbaros, 1, doğudan | 0,413° | 0,058° |
| Barbaros, 2, doğudan | 0,690° | 0,131° |
| Barbaros, 3, doğudan | 1,216° | 0,320° |
| Barbaros, 4, doğudan | 2,244° | 0,361° |
| Barbaros, 5, doğudan | 3,092° | 0,253° |
| Barbaros, 5, kuzeyden | 1,367° | 1,361° |
| Anadolu, 5, doğudan, boğaz | 2,015° | 0,310° |

Bunlar 9 saniyelik aynı test düzenindeki oyun ölçümleridir; gerçek gemi denizcilik/konfor sınıfı veya tüm fazlarda sabit sonuç değildir. Doğudan dalgada 5. seviyenin yalpası 1. seviyeden belirgin yüksek; baştan dalgada baş-kıç hareketi artmış; Anadolu'nun sönümlü profili daha düşük yalpa üretmiştir. Kamera ufku ve mevcut seyirde ayar uygularken gaz/konum/rota korunması kontrolleri geçti.

Görsel kayıtlar: [Deniz menüsü](images/dalga-menu-20261021.png), [Barbaros / seviye 5](images/dalga-barbaros-20261021.png), [Anadolu / seviye 5](images/dalga-anadolu-20261021.png). Windows Unity Editor çıktılarıdır.

Commit/push yapılmadı.
# 22 Ekim hareket modeli güncellemesi

Önceki dört yay desteğinin roll/pitch momentleri yerine gemi periyodu/sönümü ve dalga eğimiyle çalışan fiziksel tepki eklendi. Düşey destek ve yatay dalga sürüklenmesi korunur. Küçük gemilerin bordadan sert denizde yüksek yalpası, dışa dönüş yatışı ve ±40° kadran için güncel kayıt: [Sürat, dönüş ve yalpa](SURAT_DONUS_YALPA.md). Aşağıdaki eski ölçümler önceki modeli belgelendirir.
