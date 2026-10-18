# TCG BARBAROS dış model incelemesi

Güncelleme: 19 Ekim 2026. Hedef, F-244'ün kamuya açık fotoğraflardaki dış görünüşüne daha yakın ve iki bordada tutarlı bir oyun modeli oluşturmaktır.

## Referanslar ve dönem

Modelin donanım yerleşimi için 2009–2022 görünümü esas alındı. Farklı gemilerin ve modernizasyon dönemlerinin ayrıntıları aynı modelde birleştirilmemelidir. Ölçek yaklaşık 1 Unity birimi = 4 metre olarak korunmaktadır. Fotoğraflardan çıkarılan küçük ayrıntılar ölçülmüş tersane çizimleri değildir.

- [2009 borda fotoğrafı, Kevin Fox / Wikimedia Commons](https://commons.wikimedia.org/wiki/File:TCG_Barbaros_(F-244)_2009_Istanbul.jpg)
- [Yakın borda ve kıç çapraz fotoğrafı / ShipSpotting](https://www.shipspotting.com/photos/1595674)
- [20 Nisan 2022 borda-kıç fotoğrafı / ShipSpotting](https://www.shipspotting.com/photos/3415387)
- [30 Mart 2022 seyir fotoğrafı / Anadolu Ajansı](https://www.aa.com.tr/tr/gundem/tcg-barbaros-firkateyni-nato-unsurlariyla-dogu-akdenizde-egitim-yapti/2550431)

Fotoğraflar görsel referans olarak incelendi; fotoğraf dosyaları oyuna kaplama olarak eklenmedi.

## Bu geçişte değişenler

- Gemi dış görünüşü `BarbarosModelBuilder.cs` içinde ayrı bir üreticiye taşındı.
- Yetmiş en kesitli kapalı gövde; başa doğru daralan ve yükselen bordalar; gövde sınırına uyan kaymaz güverte oluşturuldu.
- Kıç uçuş güvertesine açıklıklı yüksek bordalar, emniyet çubukları ve kıç emniyet ağı eklendi.
- Köprü bloğu, köprü kanatları ve ayrı ön/yan camlar; güverte evi pencereleri ve kapıları modellendi.
- Bacalar geniş oval kesitli gövdelere, siyah egzoz bandına ve üst ızgaraya dönüştürüldü.
- Ana mast, çapraz destekli kafes olarak yeniden yapıldı; üst platform, serenler ve antenler eklendi.
- Kıç radar kaidesi, dönen radar panelleri, radomlar ve SATCOM kubbeleri güncellendi.
- Top kulesi ve namlu, ayrı VLS kapakları, sekiz ayrı lançer kutusu ve CIWS dış silüetleri eklendi.
- RHIB, mataforalar, kablolar, can salı muhafazaları, can simitleri, havalandırma panjurları ve babalar eklendi.
- Hangar kapısı yatay panelli kapalı yüzey olarak modellendi.
- Uçuş güvertesine daire, H, kesikli merkez çizgisi ve taşıma rayı eklendi.
- F244 numaraları gövde üçgenlerinin yüzeyine oturan dolu stencil çizgileriyle boyalı yüzey olarak oluşturuldu. Alfa kaplamanın küçük yazılarda parçalanmasını önlemek için yazı çizgileri geometriyle üretilir. İskele/sancak yönleri ve yazı sıraları ayrı hesaplanır; eski havada duran TextMesh kullanılmaz.
- Boya shader'ı `Assets/Shaders/Resources/HullMarking.shader` içinde tutulur; Resources yolu, shader'ın oyuncu derlemesine dahil edilmesini sağlar. Yazı malzemesi gövdenin arkasından görünmez ve bağımsız gölge oluşturmaz.
- Statik ayrıntılar malzemeye göre birleştirildi; tek tek korkulukların ayrı draw call oluşturması önlendi. Dönen radarlar ayrı kalır.
- Başlangıç kamera kadrajı genişletildi ve üst yapıya göre yükseltildi.
- Deniz dokusunun aşırı tekrarından doğan sık çizgiler azaltıldı.

## Tekrarlanabilir görsel kontrol

Unity Play modunda `Meridian > Visual Review - 8 Views` komutu şu açıları render eder:

1. İskele borda
2. Sancak borda
3. Baş
4. Kıç
5. İskele baş çapraz
6. Sancak kıç çapraz
7. Üst
8. Alçak iskele borda

Çıktılar `Temp/BarbarosVisualReview` içinde saklanır ve Git'e girmez. `contact-sheet.png` sekiz görünüşü birlikte gösterir. `request.txt` içinde yeni bir test kimliği varsa editör Play modunda bu kontrolü otomatik çalıştırabilir.

Kontrol noktaları: kapalı dış yüzeyler, okunur ve bordaya oturan numaralar, doğru cam yüzeyleri, mast/baca silüeti, uçuş güvertesi bordaları ve Unity Console'da hata bulunmaması.

## Bu sürümün kontrol sonucu

- Unity 6000.6.6f1 içinde sahne çalıştırıldı ve sekiz açının tamamı birkaç düzeltme turunda render edilerek incelendi.
- İlk turda yazının alfa kaplamasında görülen parçalanma, dolu stencil çizgisi geometrisine geçilerek giderildi. Son turda iskele ve sancaktaki baş/kıç numaraları ayrı ayrı tam çözünürlükte kontrol edildi.
- Baş alt hattı raked stem biçimine düzeltildi; üstten inceleme kamerası bütün gemiyi gösterecek şekilde uzaklaştırıldı.
- Köprü, hangar ve gövde dış yüzeylerinde iç mekânı gösteren eksik yüzey görülmedi.
- Son C# derlemesi Unity günlüğünde `Tundra build success (1.49 seconds)` ile tamamlandı; son test günlüğünde C# veya shader derleme hatası görülmedi.
- `BARBAROS_VISUAL_REVIEW_COMPLETE` kaydıyla son sekizli render seti oluşturuldu.
- İnceleme sırasında Unity ILPP servisinde oluşan `pipe broken` hatası editör yeniden başlatılarak giderildi. Son geometri düzeltmesi Play modu dışında başarıyla derlendi.
- Bu kontrol masaüstü Unity Editor içindir; Android cihaz performans testi veya foto-gerçekçilik onayı değildir.

## Kalite sınırı

Bu geçiş fotoğraf referanslı, ayrıntılandırılmış prosedürel dış modeldir. Ölçülü Blender kaynak modeli, UV/PBR kaplama, aşınma/normal haritaları, donanımın ayrıntılı dönem doğrulaması ve gerçek Android performans ölçümü henüz tamamlanmış kabul edilmez. Modelin foto-gerçekçi olduğu veya yayın kabulünü geçtiği iddia edilmemelidir.
