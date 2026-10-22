# Analog yalpa kadranı

22 Ekim 2026. Kullanıcının verdiği sarkaçlı deniz tipi eğimölçer fotoğrafının çalışma mantığı temel alındı; fotoğraf oyuna doku olarak eklenmedi.

## Görünüm ve okuma

Sol üstte durum çubuğuyla iskele makine paneli arasına ayrı bir kadran eklendi. Üst bardaki sayısal YALPA değeri korunur. Kadranda altta 0, iki yana artan 5 / 10 / 15 / 20 derece etiketleri, birer derecelik ara çizgiler, sabit sıfır çizgisi, metal pivot ve hareketli sarkaç ibresi vardır. Etkin yay bölümü ve İSKELE/SANCAK yazısı yönü vurgular.

Küçük yatışın telefonda görünür olması için ±20° ölçüm aralığı grafik üzerinde ±60° yay süpürmesine yayılır. Etiketler ve sayısal değer gerçek gemi yatış derecesidir; ibrenin ekrandaki açısı gerçek gövde açısının 1:1 çizimi değildir. 20° aşılırsa ibre ölçek sonunda kalır, gerçek sayı değişmeye devam eder ve ÖLÇEK DIŞI uyarısı görünür. Bu bir stabilite sınırı veya tehlike/alabora eşiği değildir.

Pozitif fiziksel `ShipBuoyancy.RollDegrees` iskeleye yatıştır: ibre sola gider. Negatifte sancağa/sağa gider. Dengede ibre sıfırdadır. Ayrı bir sinüs, sahte sallanma veya fizik hesabı eklenmedi; üst sayı ve yeni kadran aynı `PrototypeHud.RollDegrees` değerini okur.

`RollGauge` mevcut IMGUI temasıyla koddan çizilir. Yeni raster varlık, sahne objesi veya her kare oluşturulan doku yoktur. `HelmLayout.RollInstrument` safe-area ölçeklemesine dahildir; kamera sürükleme/zoom başlangıcını engeller, fakat makine/dümen komutu üretmez.

## Test

`Meridian > Controls - Run Live Review` komutu, önceki kumanda/yön testlerine ek olarak kadranın sıfır noktası, iskele-sol/sancak-sağ yönü, sabit ibre boyu, ölçek sınırı ve geçersiz sayı güvenliğini kontrol eder. Dört ekran/safe-area düzeninde yeni panelin üst bar, durum satırı, harita, makine kolları ve dümenle çakışmadığı; kamera engeline dahil olduğu ve komut üretmediği kontrol edilir.

Canlı testte iki geminin fiziksel gövdesi ±10° yatırılır ve yalnızca baş değiştirilince yalpanın sıfır kaldığı doğrulanır. Ayrı görsel test düzeninde gemi iskele 10°, denge, sancak 10° konumlarına alınır; fizik kısa süre durdurularak gerçek Game View çıktıları kaydedilir. Bu üç görüntü doğal dalgada yakalanmış anlık ölçüm değil, ibre-yön doğrulama düzenidir. Test bitiminde gövde ve zaman ölçeği geri yüklenir, menü açılır.

Son tur geçti: `HELM_LOGIC_CHECK_PASS checks=727`, `HELM_RUNTIME_CHECK_PASS checks=22`, `HELM_VISUAL_REVIEW_READY`. Kayıt: `Logs/play-session-restart.log`, test görselleri `Temp/HelmControlReview/roll-gauge-*.png`. 727 mantık/yerleşim kontrolü yeni ibre geometrisi ve panel kontrollerini; 22 canlı kontrol iki gemide fiziksel açı okuması ile önceki kumanda regresyonunu kapsar. Üç farklı yatış konumunda aşağıdaki çıktılar görsel olarak incelendi; ibre ve sayı aynı yönü gösteriyor, panel diğer göstergelerin üzerine taşmıyor.

- [İskele 10°](yalpa-kadrani-iskele.png)
- [Denge 0°](yalpa-kadrani-denge.png)
- [Sancak 10°](yalpa-kadrani-sancak.png)

Windows Unity Editor doğrulamasıdır; native iPhone/Metal performansı veya cihaz üzerinde ekran okunurluğu onayı anlamına gelmez. Önceden bulunan Anadolu kapak geometri kontrol hatası bu arayüz işi kapsamında değişmedi. Commit/push yapılmadı.
# 22 Ekim son güncellemesi

Sert denizde küçük gemilerin yüksek yalpasını göstermek için güncel kadran **±40°** aralığındadır; ana çizgiler 10°'de, ara çizgiler 2°'dedir. Aşağıdaki ±20° açıklaması ve önceki görseller ilk kadran sürümünün tarihsel kaydıdır. Yeni hareket ve test kaydı: [Sürat, dönüş, yalpa](SURAT_DONUS_YALPA.md).
