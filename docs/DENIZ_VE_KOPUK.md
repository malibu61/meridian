# Deniz, dalga ve köpük

21 Ekim 2026. Unity Built-in renderer üzerinde görsel iyileştirme; akıntı veya yeni gemi fiziği değildir.

## Değişiklikler

- Eski renkli/çizgili su dokusu yerine iki ölçekte kayan, yönleri farklı mikro dalga eğimleri ve geniş ölçekli desen kırılması kullanan su shader'ı. Dikişsiz gürültüden üretilen eğim dokusu kullanılır.
- Bakış açısına bağlı Fresnel yansıma hissi, gökyüzü renk yaklaşımı ve güneş parlaması. Gerçek zamanlı sahne yansıması/SSR, kırılma veya su altı render'ı yoktur.
- Üç bileşenli ortak dalga alanı. Deniz mesh'i, yüzey köpüğü, baş köpüğü şeritleri ve kıyı yıkaması aynı zaman/yükseklik hesabını kullanır.
- Karelerin nedeni olan dokusuz opak parçacık malzemesi kaldırıldı. Açık alpha blending, saydam kenarlı organik köpük dokusu, mipmap ve yumuşak yaşam boyu solma kullanılır.
- Pervane köpüğü kameraya dik kartlar yerine yatay yüzey parçacıklarıdır. Dünyada kalan izler geminin dönüşünü takip eder; dalgaların üzerinde yükselip alçalır. İskele ve sancak gerçek motor yükleri ayrı izlenir.
- Tornistan/ileri işareti, pervane akışının yönünü değiştirir. STOP sonrası makine yükü hemen yok olmadığı için köpük de yapay olarak anında kaybolmaz. Yeni seyirde efektler temizlenir.
- Baş köpüğü ileri yönlü gerçek hızla artar; geri giderken baş köpüğü üretilmez. Hızlandıkça iki yana açılan yüzey köpüğü şeritleri görünür.
- Limandaki üç düz beyaz köpük kutusu, kıyı boyunca dalgayla genişleyip solan saydam yıkama şeritlerine dönüştürüldü.
- Deniz shader'ını yanlışlıkla kullanan mavi konteynerlere ayrı standart malzeme verildi.

## Yaşam döngüsü ve performans kapsamı

`WaterWaves` ortak yüzey hesabı, `WaterVisuals` malzeme/doku üretimi, `OceanSurface` dalga mesh'i, `WakeTrail` üç parçacık sistemi, `BowWaveSheet` baş izi ve `ShorelineWash` liman yıkaması içindir.

Köpük üst sınırı iki pervane için 320'şer, baş için 160 olmak üzere 800 parçacıktır. Baca dumanı ayrı kalır. Tekrarlı Configure/gemi/mod değişiminde parçacık sistemi sayısı çoğalmaz. Köpük parçacığı tamponu tekrar kullanılır; her kare yeni parçacık dizisi üretilmez.

Liman dalga alanı 73×73 = 5.329; gemiyi izleyen boğaz alanı 97×97 = 9.409 köşedir. Normal yönleri analitik türevden hesaplanır; her kare tüm üçgenler üzerinden normal/bounds hesabı yapılmaz. Boğaz alanı dünya ızgarasına oturtulur; doku dünya koordinatından okunur. Uzak suyla aynı ortalama seviyeye inen kenar geçişi vardır.

Shader'lar `Assets/Resources` altındadır; yalnızca Shader.Find'a güvenilip build'de budanmaz. Üretilen kalıcı su/köpük malzeme ve dokuları bootstrap kaynak sahibine, yerel mesh/malzemeler kendi component yaşam döngüsüne bağlı temizlenir.

Bu sınırlar mobil bütçeyi gözeten uygulama tercihleridir; gerçek iPhone GPU süresi/FPS ölçümü yerine geçmez. iOS Metal üzerinde native build henüz denenmedi. Ekran görüntüleri Windows Unity Editor / DX11 çıktısıdır. Unity Remote aktarımının sıkıştırması bu değişiklikten bağımsızdır.

## Doğrulama

`Meridian > Water - Check Waves and Materials`: dalga sınırları/analitik normal doğruluğu, alpha kenarları/mipmap, yük-hız etkisi ve shader kontrolü.

`Meridian > Water - Run Live Review`: Barbaros/Anadolu × liman/boğaz kombinasyonlarında gerçek hareket, köpük üretimi, parça bütçesi, yatay alpha köpük malzemesi, tekrar Configure, sıfırlama ve menüde birden fazla kare boyunca duraklama kontrolü.

İlk tur: `WATER_LOGIC_CHECK_PASS checks=1019`, `WATER_RUNTIME_CHECK_PASS checks=92`. Görsel incelemede görülen fazla düzenli mikro dalga tekrarı gürültü tabanlı eğim dokusu ve farklı açılı örnekleme ile düzeltildi.

Son tur: `WATER_LOGIC_CHECK_PASS checks=1019`, `WATER_RUNTIME_CHECK_PASS checks=100`. Son sürümde menü açıldıktan 0,6 saniye sonra zamanın, gemi konumunun ve parçacık sayılarının gerçekten sabit kaldığı ayrıca kontrol edildi. İki gemi × iki mod için 5'er saniyelik gerçek makine yüklenmesi/hareket denemeleri yapıldı. Derleyici/shader hatası ve Water runtime hatası yok. Günlük: `Logs/play-session-restart.log`, son tur işaretleri 3514/3526/3538 satırlarında. Makine gecikmesi ve hareket korunur; test görüntüsü almak için fizik hızına doğrudan müdahale edilmedi.

Son dört render görsel olarak incelendi; kare parçacık köşeleri kaldırılmış, köpük su üzerinde kalmış ve su desenindeki düzenli tekrar azaltılmıştır. Native iPhone üzerinde performans veya Metal shader doğrulaması bu incelemenin yerine geçmez.

- [Barbaros / liman](images/deniz-barbaros-liman-20261021.png)
- [Barbaros / boğaz](images/deniz-barbaros-bogaz-20261021.png)
- [Anadolu / liman](images/deniz-anadolu-liman-20261021.png)
- [Anadolu / boğaz](images/deniz-anadolu-bogaz-20261021.png)

Önceden kalan Anadolu kapak/gövde geometri testi hatası bu su değişikliğiyle giderilmiş sayılmaz. Eski kumanda, gemi ve çevre değişiklikleri korunmuştur. Commit/push yapılmadı.
