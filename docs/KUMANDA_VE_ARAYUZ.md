# Kademeli gaz, ortak kol ve arayüz

21 Ekim 2026. Aktif Unity projesi: `C:/Users/Muhammet Ali/source/repos/malibu61/meridian`.

## 22 Ekim 2026 — yalpa göstergesi

Sonraki görsel iyileştirme: sayı korunarak ayrı bir dereceli sarkaç kadranı eklendi. [Ölçek, davranış ve görsel testler](YALPA_KADRANI.md).

Üst telemetri sırası SÜRAT / BAŞ / ROTA / YALPA / MAKİNE oldu. YALPA, geminin anlık fiziksel gövde yatışını bir ondalık dereceyle ve altında İSKELE veya SANCAK etiketiyle gösterir. Mutlak değer kullanılır; yön etikette belirtilir. Gösterim hassasiyeti altında, `|açı| < 0,05°` olduğunda DENGE yazar. Bu maksimum açı, dalga seviyesi veya baş-kıç açısı değil, anlık roll değeridir. Başın değişmesi tek başına yalpa değildir.

Okuma doğrudan kök `ShipBuoyancy.RollDegrees` değeridir; yapay bir gösterge animasyonu veya ayrı sinüs hesabı yoktur. `SetShip` bileşen referansını önbelleğe alır. Beş metrik `HelmLayout.HeaderMetric` üzerinden yerleşir. Mevcut seyir, dümen ve gaz kumandaları korunur.

Son tur `HELM_LOGIC_CHECK_PASS checks=529` ve `HELM_RUNTIME_CHECK_PASS checks=22` geçti (`Logs/play-session-restart.log`). Dört ekran/safe-area düzeninde beş başlık hücresinin sığması/çakışmaması, gösterim hassasiyeti ve yön etiketleri; iki gemide gerçek gövde ±10° döndürülerek HUD okuması ve başı 80° değiştirirken yalpanın sıfır kalması kontrol edildi. Önceki gaz/ortak kol/dümen/stop/menü/sıfırlama testleri de aynı canlı turda geçti. [Güncel HUD görseli](yalpa-gostergesi.png), Windows Unity Game View çıktısıdır.

## Kumanda davranışı

İleri: `0, +10, +20, +30, +40, +50, +60, +70, +80, +90, +100%`.

Tornistan: `0, −20, −40, −60, −80, −100%`.

Sıfır, ileri/geri kademe sayısına dahil edilmeyen ayrı STOP konumudur. İleri 10 ve tornistan 5 kademe, ray üzerinde eşit fiziksel aralıklarla yerleşir; bu yüzden STOP rayın alt üçte bir sınırındadır. Her yüzde 10 için çizgi çizilir. İleri çizgileri 10'ar; tornistan seçilebilir kademeleri 20'şer sayısal etiket taşır. Tornistan ara 10'luk çizgileri daha sönüktür ve ayrı seçilebilir kademe değildir.

`ThrottleDetents` komutun tek kaynağıdır: dokunma, fare, klavye ve doğrudan `ShipController` setter'ları aynı kademeye oturur. Aralık dışı değerler sınırlandırılır; geçersiz sayısal komut STOP olur. Bu değişiklik makine komutunu kademelendirir; gerçek yük/itki `FixedUpdate` içinde geminin mevcut gecikmesiyle artar.

## Ortak gaz

Yeni ortak kol, dümen ile sancak kolu arasındadır. Dokunup sürüklemek iskele ve sancağı aynı komuta getirir. Kalıcı bir bağlantı anahtarı değildir: bağımsız iskele/sancak kolunu değiştirmek diğer makineyi etkilemez.

Komutlar farklıysa ortak panel **AYRI**, ray üzerinde iki ayrı konum çizgisi ve **DOKUN → EŞİTLE** gösterir. Ortalama değer gerçek bir ortak komutmuş gibi yüzdeyle gösterilmez. Yalnızca ortak kola yeniden dokunulduğunda iki komut eşitlenir.

İki ayrı parmak bağımsız makine kollarını kullanabilir. Ortak kol tutulduğunda iki makinenin pointer sahipliğini geçici olarak devralır; dümen ayrı parmakla kontrol edilebilir. Bırakıldığında bağımsız kollar yeniden alınabilir. Menü/duraklama, odak kaybı ve iptal kalan pointer yakalamalarını bırakır. Kamera, ortak kol dahil gerçek panel sınırlarından başlayan sürüklemeyi/zoom'u almaz.

Kol başlığına veya yüzde metnine dokunmak gaz vermez; rayın dokunma bölgesi kullanılır. Ray dışına sürüklemede yakalanmış kol sınıra kadar çalışır. Mobilde gerçek touch verisi okunur; simüle mouse ile ikinci kez uygulanmaz.

## Klavye ve stop

- `W/S`: iki makineyi birer kademe artır/azalt. Önceden farklı değerlerdeyse farklarını hemen yok etmez.
- `Q/A`: iskele; `E/D`: sancak. İlk basış bir kademe, basılı tutma yaklaşık 0,16 saniyede bir tekrar.
- Sol/sağ ok: sürekli dümen/pod yönü. Aşağı ok veya **ORTALA · 0°**: sıfırla.
- `Space` veya **ACİL STOP**: iki makine komutunu STOP yapar. Dümeni veya geminin gerçek süratini anında sıfırlamaz.

Anadolu'da mevcut sınırlı ortak pod yönü, Barbaros'ta dümen korunur. Bu güncelleme gerçek azimut pod sistemi veya baş iticisi eklemez.

## Grafik düzeni

Koyu grafit yüzeyler, sade çerçeve ve küçük köşe yarıçapları; turkuaz ortak kumanda/seçim, amber ileri, kırmızı tornistan/stop. Metalik kol tutamakları, okunur hedef yüzdesi, ayrı sessiz ölçek ve düşük yoğunluklu telemetri kullanılır. Menüde platform ve mod iki ayrı sütundur; seçili, hover, basılı ve kullanılamayan durumlar görsel olarak ayrılır. Boğaz ilerlemesi/krokisi, iki gemi ve henüz geliştirilmeyen şamandıra durumu korunur.

`HelmLayout` ekranın güvenli alanından yerleşimi üretir; HUD çizimi, pointer dönüşümü ve kamera engeli aynı koordinatları kullanır. `MaritimeUiTheme` üretilen küçük UI dokularını ve stilleri ortak yönetir; component yok edildiğinde dokuları bırakır. Bu, mevcut Built-in/IMGUI altyapısında yapılan arayüz yenilemesidir; UI Toolkit/uGUI göçü değildir.

## Doğrulama

Editör komutları: `Meridian > Controls - Check Detents and Layout` ve Play Mode'da `Controls - Run Live Review`.

- Önceki 21 Ekim turu: `HELM_LOGIC_CHECK_PASS checks=491`. Tüm kademeler, sınırlar, nötr geçiş, yüzde/kol konumu gidiş-dönüşü, dört ekran boyutu ve safe-area, panel çakışmaları, kamera engeli ve pointer sahipliği kontrol edildi.
- Önceki 21 Ekim turu: `HELM_RUNTIME_CHECK_PASS checks=16`. Barbaros ve Anadolu'da gerçek controller üzerinde kademeye oturma, ortak ileri/geri, bağımsızlığın geri alınması, stop, menü duraklaması/devamı ve yeni boğaz seyrinde sıfırlanma denendi.
- Son tur günlüğü: `Logs/play-session-restart.log`, 2547/2561/2575 numaralı satırlarda mantık/canlı/görsel kontrol işaretleri. Son görseller 21 Ekim 2026 22:24'te üretildi. Derleyici `error CS` kaydı yok.
- Gerçek fareyle ortak kol sürüklenerek iki makinede `+80%` görüldü. İskele `−40%` yapılınca sancak `+80%` kaldı, ortak panel AYRI gösterdi. Ortak kola tekrar dokununca iki makine `−60%` oldu. Dümen tutamağı `+23°` konumuna sürüklendi; ACİL STOP iki gazı sıfırladı ve dümen `+23°` kaldı.
- Son görsel incelemede kumanda panelleri ve tick etiketleri birbirine girmiyor. İlk inceleme sonrası dümen ortalama ve acil stop düğmeleri 44 tasarım birimi yüksekliğe büyütüldü, testler yeniden geçti.
- Görsel kayıtlar: [Kumanda ekranı](images/kumanda-20261021.png), [Gemi/mod menüsü](images/menu-20261021.png). Bunlar Unity Game View görüntüleridir; iPhone native ekran görüntüsü değildir.

Gerçek iPhone native build performansı ve donanım üzerinde eşzamanlı iki parmak testi bu otomatik kontrollerin kapsamı değildir. Unity Remote görüntüsü sıkıştırılabilir; bu arayüz değişikliği Remote aktarım kalitesini veya mevcut köpük/model ayrıntı düzeyini düzeltmez. Önceden kalan Anadolu gövde testi hatası, kumanda değişikliğinin başarılı testleriyle giderilmiş sayılmaz.

Bu istek kapsamında commit/push yapılmadı.
# 22 Ekim ek güncellemesi: RÜZGAR dört makine

Bu belgedeki iki-makine düzenine ek olarak yalnız TCG RÜZGAR’da dört bağımsız iç/dış kol bulunur. Ortak gaz bütün aktif makineleri eşitler; AYRI durumu dört komutu da denetler. Merkezî dümen/ortak kol küçültülür. `Z/X` iç iskele, `C/V` iç sancak ek tuşlarıdır. Tam kapsam: [RÜZGAR ve KUMKALE](RUZGAR_VE_KUMKALE.md). Yalpa kadranı artık ±40°; sürat ve manevra değişiklikleri [yeni fizik kaydındadır](SURAT_DONUS_YALPA.md).
