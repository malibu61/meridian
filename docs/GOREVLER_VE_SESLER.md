# KUMKALE kariyeri, telsiz ve düdükler

22 Ekim 2026. Bu sürüm beş görev, sıralı açılma, yerel kayıt ve sesli manevra işaretlerini ekler. Gerçek seyir eğitimi veya COLREG uygunluk sertifikası değildir.

**Güncel genişleme:** KUMKALE'nin bu ilk zinciri korunarak diğer üç gemiye de beşer görev eklendi; toplam 20 görev ve 42 telsiz WAV'ı var. Gemiye özgü kayıt, BARBAROS/ANADOLU römorkör koşulları ve RÜZGAR iç/dış makine çalışması için [Filo görevleri](FILO_GOREVLERI.md) güncel kapsamı anlatır. Aşağıdaki 18 ses ve KUMKALE test kayıtları ilk sürümün tarihsel ayrıntılarıdır.

## Beş farklı görev

| Sıra | Görev | Farkı |
| --- | --- | --- |
| 1 | İlk Yanaşma — Sancak Aborda | Sakin denizde doğu rıhtımı, geniş hedef |
| 2 | Diğer Borda — İskele Aborda | Farklı dış başlangıç konumu, batı rıhtımı |
| 3 | Dar Yer — İki Geminin Arasına | Doğu rıhtımında iki çarpıştırıcılı sabit eğitim gemisi arasında dar hedef |
| 4 | Koşullar Değişti — Bordadan Dalga | Doğudan üçüncü seviye oyun dalgası, batı rıhtımı; rüzgâr sistemi iddiası yok |
| 5 | Beklenmeyen Emir — Rıhtım Değişikliği | Başlangıçta doğu; ikinci giriş kapısında telsizle batıya yeniden planlama |

Görevler mendirek dışında, oyun z=−140…−160 konumlarında (girişten yaklaşık 560–640 m uzakta), 3 kn ilk ileri hareketle başlar. Brifing boyunca fizik durur; ANLAŞILDI · BAŞLA ile devam eder. Başlangıç komutu STOP'tur: oyuncu gaz vermezse mevcut ileri hareket su direnciyle yavaşlar.

Üç giriş kapısı sıralı ve kuzey yönünde geçilir. Rıhtım hedefi, baş ve hızlar HUD'da takip edilir. Yeni [baş/kıç halat aşamasında](HALAT_VE_SAHIL_PERSONELI.md) iki uç da alınmalı, çekilmeli, bağlanmalı ve gemi sakinleşmelidir. Halatsız hedefte park etmek yeterli değildir. Hedefte yanal konum hatası <3 m, boyuna hata normal görevlerde <8 m/dar görevde <4,8 m, baş hatası <6°, boyuna sürat <0,30 kn ve yanal sürat <0,15 kn koşulları 5 saniye korunur. Bekleyen zorunlu düdük varken görev tamamlanmaz.

Sert yatay temas (>0,65 m/s oyun eşiği) görevi bitirir. Hafif temas, liman içinde 8 kn görev sınırını 3 saniye aşma veya yanlış düdük seçimi yıldızı düşürebilir. Sinyal eksikliği ve görev sahasından çıkma başarısızlık nedenidir. İlk görevlerde süre başarısızlık şartı değildir. Bu sayısal eşikler oyun tasarımıdır; gerçek bir limanın emniyet limitleri değildir.

## Menü, kilitler ve kayıt

- Menüde SEYİR / GÖREVLER / DENİZ sekmeleri vardır.
- İlk görev açık; bir görevin başarıyla tamamlanması sıradakini açar. Bir yıldız yeterlidir, kusursuz sonuç zorunlu değildir.
- Tamamlanan görevler tekrar oynanabilir. Başarısız tekrar eski tamamlanma, yıldız ve en iyi süreyi silmez.
- KUMKALE ilerlemesi 0/5…5/5, tamamlandı/hazır/kilitli kartları ve en iyi sonuçlar gösterilir.
- Diğer gemiler de görev sayfasından seçilebilir; her birinin beş görevi ve gemiye özgü ilerlemesi vardır.
- Sonuç ekranı TEKRAR DENE / GÖREVLER / SONRAKİ GÖREV seçeneklerini sunar.
- PlayerPrefs üzerinde sürümlü, sabit görev kimlikli JSON kayıt kullanılır. Oyun kapanınca kariyer ilerlemesi korunur; geminin anlık sefer konumu kaydedilmez. “Kaldığın göreve dön” yeni oturumda görevi dış başlangıçtan açar. Açık seferde “Mevcut göreve devam” aynı oturumu sürdürür.
- Görev denizi sabittir; menüden değiştirerek koşullar atlanamaz. Serbest seyirde deniz ayarı eskisi gibi değiştirilebilir.
- İlerleme yereldir; bulut eşitleme ve cihazlar arası aktarım yoktur. Depolama hatasında uyarı gösterilir. Bozuk/sürümü tanınmayan kayıtta güvenli ilk-görev durumuna dönülür.

## Düdükler ve kural ayrımı

Uluslararası COLREG Kural 32/34(a), görüşte başka gemiler varken belirtilen manevra bağlamında kullanılır:

| Oyuncunun seçimi | Anlam / uygulama |
| --- | --- |
| 1 KISA — H veya 1 | Sancağa rota değişikliği |
| 2 KISA — 2 | İskeleye rota değişikliği |
| 3 KISA — 3 | Tornistan sevk kullanılması |
| UZUN — L | 5 saniyelik uzun düdük, isteğe bağlı |
| 5 UYARI — 5 | Şüphe/uyarı için beş kısa işaret, isteğe bağlı |

Önemli: **3 kısa, geriye doğru hız ölçülmesi demek değildir.** Gemi ileri giderken tornistan sevkle fren yapabilir; o durumda da tornistan sevk izlenir. Sevk kullanmadan geriye sürüklenmek aynı tetikleyici değildir. Görevlerde gerçeklenen makine yükleri okunur; en az bir makinenin tornistan yüküne geçmesi takip edilir. Rota değişikliği dümen/diferansiyel makine niyeti ve gerçek baş değişimi üzerinden örneklenir. Küçük salınımlar için eşik/histerezis vardır; her dümen tıkı ayrı düdük isteği değildir.

Kısa düdük yaklaşık 1 s; iki/üç kısa diziler arasında yaklaşık 1 s boşluk vardır. Uzun düdük 5 s olarak üretilir (Kural 32 aralığı 4–6 s). Oyuncu butondan veya klavyeden diziyi seçer; oyun makine komutunda otomatik düdük çalmaz. Tamamlanma, butona basışta değil dizinin gerçek çalma süresi bitince sayılır; aynı anda iki dizi başlatılamaz.

Görüşte sabit eğitim gemileri olan bu görevlerde manevra işaretleri zorunludur. İstek 15 oyun saniyesi içinde tamamlanmazsa görev başarısız olur; yanlış seçim doğru yükümlülüğü kaldırmaz. Uygun kısa bir önceki bildirim penceresi ve aynı tornistan kullanımında tekrar tekrar istememek için bölüm/histerezis kontrolü vardır. **15 s, baş değişimi ve yük eşikleri oyunun eğitim koşullarıdır; COLREG'de yazan bir tolerans veya eksiksiz mevzuat uygulaması değildir.** Serbest seyirde düdükler kullanılabilir fakat görev cezası uygulanmaz. Sis/dar kanal/sollama kurallarının bütünü simüle edilmez; ABD iç su kuralları uluslararası kurallarla karıştırılmadı.

Kaynaklar: [USCG Navigation Rules Handbook — International Rules 32/34](https://www.navcen.uscg.gov/sites/default/files/pdf/navRules/Handbook/CG_NAV_Rules_29APR2020.pdf), [Hindistan Denizcilik İdaresi — resmî COLREG metni](https://dgshipping.gov.in/Content/PreventionofCollisionsatSeaRule1975.aspx).

## Ses ve telsiz

- 18 özgün kurgu Türkçe anons; brifingler, giriş, yaklaşma, rıhtım değişikliği, sinyal hatırlatmaları, sonuç ve periyodik kanal konuşmaları.
- Kurulu yerel Microsoft Tolga Türkçe sesiyle çevrimdışı üretilmiş WAV dosyaları paketlenir. Oyunun çalışması Windows TTS veya internet gerektirmez; iOS/Android de normal Unity AudioClip kullanır.
- Konuşmaya band-sınırlama ve hafif parazit uygulanır; baş/son squelch hissi vardır. Anonslar gerçek askerî telsiz kaydı veya belirli bir kişinin taklidi değildir.
- Arka plan kanal konuşmaları yaklaşık 35–51 s aralıklarla, ilk konuşma yaklaşık 26 s sonra gelir. Olay anonsları önceliklidir; kuyruk sınırlıdır. Türkçe altyazılar da gösterilir.
- Özgün harmonik düdük ve deniz uğultusu kodla sentezlenir. Düdük gemiye göre farklı temel frekanstadır; gerçek gemi ses kaydı iddiası yoktur.
- SES paneli ses düzeyi/sessiz seçimi sunar; tercih yerelde saklanır. Sessiz oynarken de işaret butonunu doğru seçmek gerekir; erişilebilirlik nedeniyle işaretin mantıksal tamamlanması korunur.
- Menü/uygulama duraklamasında sesler duraklar; yeni seferde eski düdük/anons kuyruğu temizlenir.

Sesleri yeniden üretmek için Windows'ta kurulu Türkçe Tolga gerekir: `Tools/GenerateRadio.ps1`. Metinler `Tools/RadioLines.json`; paketlenmiş sesler `Assets/Resources/Audio/Radio/` içindedir. Araç kayıt defterini değiştirmez ve ağ isteği yapmaz.

## Doğrulama

- `MissionCareerReview.CheckSources`: 84 kural/kariyer/kayıt/sinyal/görev/gövde yerleşimi/ses dizisi kontrolü geçti.
- `MissionCareerReview.RunBatch`: 85 çalışma zamanı kontrolü, beş görev tamamlama/kayıt döngüsü ve 18 Türkçe sesin yüklenmesi/PCM enerjisi doğrulandı.
- Kilit atlama, kayıt yeniden yükleme, başarısız tekrarda korunma, bozuk kayıt, geriye sürüklenme–tornistan ayrımı, doğru/yanlış/eksik sinyal, menüde duraklama, yalnız dizi bitiminde sayma, rıhtım değişimi ve önceki ANADOLU/boğaz akışının korunması test edildi.
- Testler kariyer kaydı için **bellek depolaması** kullanır; oyuncunun gerçek kariyerini açıp kapatmaz veya bütün görevleri tamamlanmış yapmaz.
- Görev sonuç testleri kontrollü konum/baş/sürat örneklerini durum makinesine besler. Beş görevin tamamının insan tarafından gerçek cihazda baştan sona oynanıp dengelendiği iddia edilmez.
- Yeni mantık/ses testleri mevcut ANADOLU gövde kenar-yönü uyarısını, su-gövde kesişmesi sorununu veya native iPhone performans testini çözmüş sayılmaz.

Kanıtlar: `docs/testing/20261022-missions/rules.txt`, `runtime.txt`. Gerçek cihaz ses/dokunma/performans ve uzun süreli oynanabilirlik ayrıca doğrulanmalıdır.
