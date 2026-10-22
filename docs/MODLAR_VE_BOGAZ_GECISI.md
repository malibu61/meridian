# Modlar ve Boğaz Geçişi

Güncelleme: 19 Ekim 2026. Bu özellikler `45677bb` sonrasındaki yerel çalışmadır; bu istek kapsamında commit/push yapılmadı.

## Menü ve kapsam

Uygulama açılınca mod seçimi görünür. Kart seçimi ve başlatma ayrı adımlardır; kartı incelemek mevcut seyrini sıfırlamaz.

| Kart | Mevcut davranış |
| --- | --- |
| Gemi Yanaştırma | Önceki limanda serbest manevra; puanlı yanaşma görevi eklenmiş sayılmaz |
| Boğaz Geçişi | Ayrı çevre ve kuzey başlangıcı; Marmara yaklaşımı → Ege, 12 sıralı kontrol noktası |
| Şamandıraya Bağlama | Yakında; açıklaması seçilebilir, başlatma düğmesi pasif |

- Seyirde sol üst MENÜ veya Esc mod seçimini açar. Simülasyon zamanı, fizik, personel ve parçacıklar duraklar; makine/dümen klavye girdisi ve kamera sürükleme/zoom engellenir.
- MEVCUT SEYRE DEVAM konum, makine komutları ve rota ilerlemesini korur. Tamamlanmış rota doğrudan devam ettirilemez; yeniden başlatılır.
- MODU BAŞLAT, aynı mod seçilse bile yeni seyir başlatır. Makineler, gerçeklenen yükler, dümen, doğrusal/açısal hız, duman/iz parçacıkları ve dumanın geçici yük darbesi sıfırlanır.
- Önceki çevrenin collider'ları hemen devre dışı bırakılır; nesneler ve sahip olunan mesh/malzemeler kaldırılır. Oyuncu gemisi ve kamera yeniden üretilmez.
- Menü paneli yatay 1280×720 referans yerleşimden ekranın güvenli alanına ölçeklenir. Oyun içi rota krokisi küçük yükseklikte küçültülerek sancak kumandasının üzerine binmesi engellenir.
- Mobil işletim sistemi uygulamayı arka plana alınca menü açılarak seyir duraklatılır. Bu davranışın fiziksel mobil cihaz testi henüz yapılmadı.

## Boğaz Geçişi'nin bu sürümü

Yaklaşık enlem/boylam merkez hattı Marmara, Gelibolu/Çardak, köprü, Nara, Eceabat, Çanakkale/Kilitbahir, Kepez, Erenköy ve güney çıkışı etiketleriyle elle hazırlanmıştır. Yerel düzlem dönüşümünde yaklaşık 1 model birimi = 4 m korunur. Bu yaklaşım hassas bir GIS projeksiyonu değildir.

- İki kıyıda parçalar hâlinde, collider'lı prosedürel arazi; basit sahil/taş/bitki malzeme bölgeleri.
- 1915 Köprüsü'nü çağrıştıran tabliye, kırmızı kuleler, ana kablolar ve askılar. Köprü çalışması ilk silüet düzeyindedir; ölçülmüş nihai yapı modeli değildir.
- Kuzey/kent yerleşimi, kale ve güney anıtı silüetleri. Bina yerleri/zemin ilişkisi ayrıntılı coğrafi doğrulamadan geçmedi.
- Geniş uzak su yüzeyi ve gemiyi takip eden 64×64 hücreli yakın dalga yaması. Mod değişiminde dalga mesh/malzemeleri temizlenir.
- Başlangıç başı ilk rota parçasına göre güneye yönelir; boğaz modunda daha alçak ve arkadan başlangıç kamera açısı kullanılır. Fare/mobil orbit ve zoom korunur.
- Üst görev satırında sonraki noktanın adı, geçilen/total noktalar, ilerleme çubuğu ve küçük rota krokisi bulunur.
- Turkuaz çember yalnızca sıradaki oyun kontrol noktasıdır. Öncekini atlayıp ilerideki bir çemberden geçmek sayılmaz; ters yönde geçiş reddedilir. Hızlı karelerde çemberi atlamamak için önceki/yeni konum arasındaki hareket parçası kontrol edilir.
- Son noktada geçiş tamamlanır ve menü açılır. Bu görev başarısı gerçek seyir yeterliliği değildir.

## Açık sınırlar

**Bu sürüm gerçek coğrafyanın tamamlandığı anlamına gelmez.** Menü ve seyir HUD'ında taslak uyarısı bulunur.

- OSM kıyısı, Copernicus DEM, gerçek bina izleri ve batimetri henüz içeri alınmadı. Kıyılar merkez hattının iki tarafına genişlik verilerek üretilir; ölçülmüş kıyı çizgisi değildir.
- Akıntı, yerel rüzgâr, gerçek trafik şeritleri, feribot/AI trafiği, sığlık/karaya oturma, hasar, resmî seyir yardımcıları yoktur.
- Harita streaming ve floating origin yoktur; sade kıyı mesh'leri bir defada kurulur. İlk prototipteki basit geometri nihai mobil performans onayı sayılmaz.
- Genel gemi sürat göstergesindeki metre/model-birimi tutarsızlığı bu menü çalışmasında düzeltilmedi. HUD sürati gerçek gemi seyir verisi gibi kullanılmamalıdır.
- Kalıcı kayıt, gerçek zamana göre tam geçiş doğrulaması, zaman hızlandırma ve nihai görsel üretim kapsam dışıdır.
- Şamandıraya bağlama hâlâ planlanan moddur. Mevcut personel halat hareketleri bağlama fiziği değildir.

## Test ve tekrar çalıştırma

- `Meridian > Modes - Check Route Logic`: 45 otomatik kontrol; mod erişimi, gelecekteki/geçersiz modun engellenmesi, sıralı noktalar, ters yön, ıskalama, hareket parçasıyla geçiş, bitiş ve rota meta verileri. Sonuç: `MODE_ROUTE_CHECK_PASS checks=45`.
- `Meridian > Modes - Check Runtime Switching`: Play modunda 35 kontrol; kuzey başlangıcı/baş, duraklat-devam, mod değişiminde komut/sürat sıfırlama, tek gemi/kamera, tekrarlı geçişler ve bütün noktaların mantıksal tamamlanması. Sonuç: `MODE_RUNTIME_CHECK_PASS checks=35`.
- Bu tam güzergâh kontrolü, kontrol noktalarının mantıksal örneklemesidir; bütün boğazın oyuncu kumandasıyla baştan sona seyredildiği iddia edilmez.
- Gerçek menü kartı ve başlatma düğmesiyle Boğaz Geçişi açıldı. Görsel incelemede rota çizgisinin ölçekli menü dışına taşması düzeltildi; küçük ekranda krokinin sancak panelini örtmesi için yerleşim güncellendi.
- Mevcut zoom ve duman kontrolleri derleme sonrasında sırasıyla `CAMERA_ZOOM_CHECK_PASS checks=23` ve `ENGINE_EXHAUST_CHECK_PASS checks=17` verdi.
- `Temp/ModeSelectionReview/request.txt` için benzersiz `eylem:kimlik` istekleri: `checks`, `harbor`, `strait`, `menu`, `resume`, `bridge`, `cruise`, `motion`. `motion` gerçek FixedUpdate hareketi ve menü açıkken konumun sabit kaldığını sınar. `capture:dosya-adi` oyun görüntüsünü PNG olarak aynı geçici klasöre kaydeder.
- Render ve geçici test istekleri Git'e alınmaz. Gerçek Android cihazında dokunmatik ve performans doğrulaması henüz yapılmadı.
- Canlı fizik testi geçti: `MODE_MOTION_PASS distance=0,445 speed=0,81`; 3 saniyelik testte gemi gerçek FixedUpdate ile hareket etti. Ardından menü 0,7 saniye açıkken konum değişmedi: `MODE_PAUSE_PASS`. Mesafe model birimi, sürat mevcut prototip göstergesidir; ölçek sınırı yukarıda belirtilmiştir.

## Sıradaki coğrafya işi

Gerçek kıyı/yükseklik verisinin yerel kalite kontrolü → ölçek standardı → Nara/Çanakkale/Kilitbahir kalite örneği → tüm güzergâhın ayrıntılandırılması. İlk araştırma ve kaynaklar [ana belgede](ILK_MVP_TASARIMI.md) korunur.
