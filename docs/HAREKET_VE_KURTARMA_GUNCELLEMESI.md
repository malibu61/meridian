# Sürat yönü, halatla römorkör desteği ve hareketli MOB

24 Ekim 2026. Mevcut oyuncu kayıtları ve görev kimlikleri korunur. Test erişimindeki 24 görev açık kalır.

## Sürat yönü

`ShipController.SignedSpeedKnots` gerçek yatay hızın geminin ileri eksenindeki bileşenidir. HUD'da yukarı ok ileri, aşağı ok geri hareketi gösterir. Gösterilen kn değeri toplam yatay sürattir; yön oku gaz komutundan bağımsızdır. ±0,05 kn boyuna ölü bölgede nötr çizgi kullanılır. Yalnız yanal sürüklenen gemiye ileri/geri yönü atfedilmez. Ok font glifi değil çizgi geometrisidir.

## Halat verildikten sonra römorkör

Halatın `Engaged` olması itme iznini artık kesmez. BARBAROS ve ANADOLU'da mevcut görev/mod kapsamı değişmeden, halat atılırken/çekilirken veya ilk uç bağlıyken BAŞ ve KIÇ römorkörleri bağımsız kullanılabilir. Halat kartındaki ilave bir satır iki kumanda ve İKİSİ DUR sağlar; dünya üstü etiketler de çalışır. Bu satır kamera sürüklemesini engeller, ekranın geri kalanı serbesttir.

Gerçek temas, 5 kn sürat sınırı, rıhtıma paralellik, doğru borda, kıyıdan güvenli intikal, hedefin ötesine itmeme ve menü engelleri korunur. İki halatla yanaşma tamamlandığında destek otomatik kesilir. Halatın mevcut mesafe/sürat emniyet bırakma koşulları gevşetilmedi. Halat çekme sırasında ana makineler STOP gerekliliği devam eder.

## Seyir sırasında denize düşüş

MOB görevlerinde RÜZGAR 30 kn, BARBAROS/ANADOLU/KUMKALE 17 kn ile başlar. Başlangıç gerçek Rigidbody hızı ve seyir yüküne getirilmiş makine durumudur; sonra normal makine tepki ve hidrodinamik kuvvet modeli çalışır. Sabit hız kilidi veya otomatik durdurma yoktur.

Can yelekli karakter brifingde bordada görünür, görev başlayınca düşer. Süre güverte yüksekliği ve 1 birim = 4 m ölçeğine göre hesaplanır. Yatay hareket başlangıç gemi süratini taşır; suya giriş noktasına kesintisiz ilerler ve yumuşak parçacıklardan tek sıçrama verir. Sonrasında dalga yüzeyinde kalır. Düşen karaktere suya girmeden gövde/pervane çarpışma cezası uygulanmaz; suya girdikten sonra eski süpürülmüş gövde/pervane güvenlik kontrolü etkinleşir. Düşüş süresi menüde ilerlemez. Alarm/işaret sırası, STOP/sürat/dönüş/yalpa koşulları ve 8 saniyelik kurtarma korunur.

Kapsam: basitleştirilmiş oyun animasyonu; ragdoll, gerçek insan hidrodinamiği, akıntıda sürüklenme veya sertifikalı kurtarma eğitimi değil.

## Doğrulama yöntemi

`Meridian.Editor.MotionRescueReview.RunBatch`, ayrı test projesinde bellek içi kariyer kaydı kullanır. İki büyük gemi × iki rıhtımda halatla birlikte gerçek römorkör kuvvetini ve iki halatın sonlanmasını; dört gemide gerçek başlangıç süratini, düşüş/sıçrama görselini, duraklamayı ve gerçek hızdan türetilen yönü denetler. Kontrollü yatay fizik sahneleri ve yakın plan renderlar kullanır. Mevcut MOB, kariyer, HUD, halat ve römorkör kural testleri de tekrar çalıştırılır. Gerçek telefonda baştan sona oynanış/performance testi yerine geçmez.

Sonuç: **386 yeni kural/örnek kontrolü ve 80 kontrollü runtime kontrolü geçti.** BARBAROS/ANADOLU × doğu/batı rıhtımı dört fizik döngüsünde hem halat çekilirken hem ilk uç bağlandıktan sonra gerçek itme ölçüldü; ardından iki halatla tamamlanma ve otomatik römorkör duruşu geçti. Dört gemide 17/30 kn başlangıç, düşüş ve tek sıçrama doğrulandı. Mevcut 141 MOB kural/kayıt kontrolü ve 1300 kompakt HUD yerleşim kontrolü dahil çağrılan regresyonlar geçti. Önceden var olan ANADOLU `Vessel check: consistent edge winding` geometri denetimi hatası bu güncellemenin konusu değildir ve çözülmüş sayılmamıştır.

Kanıt: `docs/testing/20261024-motion-rescue/`.

Son ek doğrulama: yeni başlangıç/düşüşle mevcut `ManOverboardReview.RunBatch` **34 runtime kontrolünü** yeniden geçti; dört gemide alarm/işaret, güvenli duruş, 8 saniyelik güverteye alma ve bellek içi sonuç kaydı tamamlandı. Ana Unity Editor'de RÜZGAR görev brifingi ve seyirde 30,4 kn yanında yukarı yön oku gözle kontrol edildi. Başlangıç 30,0 kn'dır; daha sonra normal gaz/hidrodinamik modeli nedeniyle sürat değişir. Native telefon build'inde doğrulama yapılmadı.
