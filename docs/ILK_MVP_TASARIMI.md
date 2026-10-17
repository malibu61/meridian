# Meridian — İlk MVP Tasarımı

> Güncelleme: 18 Ekim 2026
>
> Durum: MVP-0 hareket prototipi tamamlandı. TCG BARBAROS görsel MVP çalışması ve kullanıcı arayüzü çalışır durumda; Android cihaz testi ve yayın hazırlıkları henüz tamamlanmadı.

## Güncel geliştirme özeti

Proje, başlangıçtaki basit blok prototipten oynanabilir bir TCG BARBAROS (F-244) manevra demosuna dönüştürüldü. Şu anda Unity Editor içinde gemi hareket ettirilebiliyor, iki makine bağımsız kullanılabiliyor, dümen kontrol edilebiliyor ve kamera geminin çevresinde döndürülebiliyor.

### Tamamlanan altyapı

- Unity 6.6 ve C# tabanlı proje oluşturuldu.
- Ana sahne ve çalışma anında sahneyi kuran başlangıç sistemi hazırlandı.
- Proje Git deposuna dönüştürüldü ve `malibu61/meridian` GitHub deposuna ilk prototip gönderildi.
- Son görsel geliştirmeler şu anda yerel çalışma alanında bulunuyor; yeniden onaylandıktan sonra GitHub'a gönderilecek.
- Unity dışındaki geçici derleme kopyaları ve Unity Console üzerinden kod kontrolleri yapıldı.

### Tamamlanan gemi hareket sistemi

- İskele ve sancak makineleri birbirinden bağımsız çalışıyor.
- Her makine `-100%` tornistan ile `+100%` ileri yol arasında ayarlanabiliyor.
- Makine kuvvetleri geminin kıçında, gerçek yerleşime uygun ayrı noktalardan uygulanıyor.
- Sancak makine ileri verildiğinde oluşan diferansiyel kuvvet geminin başını iskeleye çevirebiliyor; tersi de uygulanıyor.
- İleri yol, stop, tornistan ve yerinde dönüşe yakın diferansiyel manevra destekleniyor.
- Makine komutları doğrudan tam kuvvete çıkmıyor; büyük gemi hissi için gecikmeli cevap veriyor.
- Boyuna direnç, yüksek yanal direnç ve açısal sönüm uygulanıyor.
- Dümen `-35°…+35°` aralığında ayarlanabiliyor ve su akışı arttıkça etkisi güçleniyor.
- Acil stop iki makine komutunu birlikte sıfırlıyor.

### TCG BARBAROS (F-244) görsel modeli

Model, kamuya açık dış görünüş fotoğrafları ve MEKO 200 TN Track II-A'nın yaklaşık `118 m × 14,8 m` oranları esas alınarak prosedürel biçimde oluşturuldu.

- Uzun ve dar, baş tarafında yükselen özel gövde mesh'i
- Eğimli ve kademeli köprü/üst yapı
- Kıç hangarı ve helikopter güvertesi
- Helikopter pisti `H` işareti
- 127 mm baş topu ve namlusu
- Ön güvertedeki VLS hücreleri
- Harpoon lançerlerini temsil eden görsel kutular
- Çift baca ve baca kapakları
- Ana mast, mast destekleri, kıç mastı ve dönen radarlar
- Radar kubbeleri ve antenler
- İskele/sancak filikaları ve CIWS silüetleri
- Borda lombozları ve güverte korkulukları
- Her iki bordada `F244` numarası; karşı bordadaki ters yazıyı gizleyen görünürlük kontrolü
- Kıç gönderinde prosedürel dokuyla oluşturulan Türk bayrağı
- Hafif yalpa ve baş-kıç hareketi veren görsel gemi hareketi

Bu model önceki blok fırkateynin yerini aldı ve Unity içinde yan, kıç-çapraz ve üst-çapraz açılardan görsel olarak kontrol edildi. Mevcut model stilize bir MVP modelidir; foto-gerçekçi ve yakın plan kullanıma uygun nihai model için Blender'da özel modelleme, UV ve PBR kaplama çalışması gereklidir.

### Deniz ve efektler

- Sabit düzlem yerine yaklaşık 2.000 köşeli dinamik deniz mesh'i kullanılıyor.
- Birbiriyle kesişen sinüs dalgalarıyla hareketli su yüzeyi oluşturuluyor.
- Prosedürel deniz dokusu kaydırılarak küçük yüzey hareketleri destekleniyor.
- İskele ve sancak pervaneleri için ayrı kıç suyu parçacıkları bulunuyor.
- Sürate göre artan baş köpüğü efekti bulunuyor.
- Kıyı ve liman duvarlarında köpük şeritleri bulunuyor.
- Deniz üzerindeki görsel hareket mobil performans düşünülerek düşük maliyetli tutuldu.

### Liman ve çevre

- İki kıyı şeridi, kum ve bitki katmanları
- Mendirek, iskele ve liman duvarı
- Kayalıklar ve prosedürel kıyı bitkilendirmesi
- Operasyon binası, bakım hangarı ve liman ofisi
- Konteyner alanları ve vinçler
- Deniz feneri ve yanıp sönen fener ışığı
- Kırmızı/yeşil seyir şamandıraları
- Uzak şehir yapıları
- Gün ışığı, yumuşak gölgeler, atmosferik sis ve prosedürel gökyüzü

### Kamera

- Kamera geminin konumunu yumuşak biçimde takip ediyor.
- Bilgisayarda boş deniz alanında sol tıklayıp sürükleyerek geminin çevresinde dönülebiliyor.
- Fare tekerleğiyle yakınlaşma ve uzaklaşma yapılabiliyor.
- Mobilde tek parmak sürükleme yörünge kamerasını döndürüyor.
- Mobilde iki parmak hareketi yakınlaşma ve uzaklaşmayı kontrol ediyor.
- Kamera dikey açısı ve mesafesi sınırlandırılarak suyun altına veya aşırı uzağa gitmesi engellendi.
- HUD üzerinde başlayan sürüklemeler kamera hareketine aktarılmıyor.

### Kullanıcı arayüzü ve kontroller

- Mobil yatay ekran için koyu denizcilik temalı HUD hazırlandı.
- Sol tarafta iskele, sağ tarafta sancak makine kolu bulunuyor.
- Alt orta bölümde dümen sürgüsü ve acil stop düğmesi bulunuyor.
- Canlı sürat (`kn`), baş, rota ve makine durumu gösteriliyor.
- HUD başlığında `TCG BARBAROS • F-244` kimliği bulunuyor.
- Panel, kenarlık, kontrol bölgeleri ve düğmeler farklı ekran boyutlarına göre ölçekleniyor.
- Klavye desteği geliştirme ve masaüstü testi için korunuyor.

| Girdi | İşlev |
| --- | --- |
| `W / S` | İki makineyi birlikte ileri/tornistan ayarlar |
| `Q / A` | İskele makinesini ayarlar |
| `E / D` | Sancak makinesini ayarlar |
| `← / →` | Dümeni iskele/sancağa basar |
| `↓` | Dümeni ortalar |
| `Space` | Acil stop |
| Fare sürükleme | Kamerayı geminin çevresinde döndürür |
| Fare tekerleği | Kamerayı yakınlaştırır/uzaklaştırır |
| Tek parmak sürükleme | Mobil kamerayı döndürür |
| İki parmak hareketi | Mobil yakınlaştırma/uzaklaştırma |

### Test ve düzeltme döngüleri

- Unity Console'da C# derleme hataları sıfırlanana kadar proje yeniden derlendi.
- Gemi Unity Play modunda çalıştırıldı ve farklı kamera açılarından incelendi.
- İlk yan profil testinde görülen kısa-geniş gövde ve kutu biçimli üst yapı yeniden tasarlandı.
- Köprü, hangar, baca ve top kulesi eğimli özel mesh'lere dönüştürüldü.
- Karşı bordadan görünen ters `F244` yazısı için kamera tarafına göre görünürlük sistemi eklendi.
- Türk bayrağındaki gereksiz MeshCollider çalışma zamanı uyarısı giderildi.
- Kamera yakınlığı ve başlangıç açısı görsel test sonucuna göre ayarlandı.
- Git fark denetiminde boşluk veya bozuk yama hatası bulunmadı.

## Güncel aşama değerlendirmesi

| Bölüm | Durum |
| --- | --- |
| Unity proje iskeleti | Tamamlandı |
| Çift makine ve dümen fiziği | Tamamlandı |
| Masaüstü ve dokunmatik kontroller | Tamamlandı |
| Canlı seyir göstergeleri | Tamamlandı |
| Yörünge kamera sistemi | Tamamlandı |
| Stilize TCG BARBAROS modeli | MVP için tamamlandı |
| Dinamik deniz ve kıç suyu | MVP için tamamlandı |
| Liman çevresi | İlk görsel geçiş tamamlandı |
| Yanaşma hedefi ve görev sonucu | Henüz yapılmadı |
| Rıhtım çarpışma/hasar geri bildirimi | Henüz tamamlanmadı |
| Ses sistemi | Henüz yapılmadı |
| Android gerçek cihaz testi | Henüz yapılmadı |
| Performans profili ve optimizasyon | Henüz yapılmadı |
| Ayarlar, kayıt ve ana menü | Henüz yapılmadı |
| Blender nihai gemi modeli ve PBR kaplama | Sonraki üretim aşaması |

## Önemli kaynak dosyalar

| Dosya | Sorumluluk |
| --- | --- |
| `PrototypeBootstrap.cs` | Gemi, liman, ışık, malzeme ve sahnenin çalışma anında kurulması |
| `ShipController.cs` | İki makine, dümen, direnç ve gemi hareketi |
| `FollowCamera.cs` | Fare ve dokunmatik yörünge kamerası |
| `PrototypeHud.cs` | Göstergeler ve mobil kumandalar |
| `OceanSurface.cs` | Dinamik deniz mesh'i ve dalgalar |
| `WakeTrail.cs` | Pervane izi ve baş köpüğü |
| `ShipVisualMotion.cs` | Görsel yalpa ve salınım |
| `RotatingRadar.cs` | Radar dönüş animasyonu |
| `BeaconPulse.cs` | Deniz feneri ışığı |
| `HullNumberVisibility.cs` | Doğru bordadaki F244 yazısının gösterilmesi |

## Sıradaki önerilen geliştirme sırası

1. Son görsel sürümü GitHub'a gönderme
2. Yanaşma hedef alanı, başarı/başarısızlık şartları ve görev ekranı
3. Rıhtım, mendirek ve şamandıralar için güvenilir çarpışma geri bildirimi
4. Makine, deniz, rüzgâr ve düdük sesleri
5. Android build ayarları ve gerçek telefonda dokunmatik test
6. Profiler ile CPU/GPU ve bellek optimizasyonu
7. Ana menü, ayarlar, kalite seçenekleri ve kısa eğitim
8. Blender'da ayrıntılı TCG BARBAROS modeli, UV ve PBR kaplamalar

---

## Başlangıçta hazırlanan MVP-0 planı

Aşağıdaki bölümler projenin geliştirme öncesindeki ilk kapsam kararlarını korumaktadır.

## İlk teknik prototip: MVP-0

Bu ilk adım özellikle küçük tutulacaktır:

- Basit ve ayrıntısız gemi silüeti
- Sakin deniz ve basit liman blokları
- Bağımsız iskele ve sancak makineleri
- İleri yol, stop ve tornistan
- İki makine arasındaki kuvvet farkından doğan basit dönüş
- Geçici mobil kontrol arayüzü ve klavye kontrolleri

Yanaşma görevi, ayrıntılı model, gelişmiş su fiziği ve görsel iyileştirmeler bu temel hareket sistemi onaylandıktan sonra eklenecektir.

## 1. Proje fikri

Oyuncu, Türk Deniz Kuvvetleri gemilerinden esinlenen bir fırkateyni liman içinde düşük süratte kumanda eder. İlk sürümün amacı savaş veya görev sistemi değil; geminin çift makine, dümen, ileri yol ve tornistan davranışını anlaşılır ve keyifli biçimde hissettirmektir.

İlk prototipte gemi, Barbaros sınıfının genel oranlarını çağrıştıran basit bir silüet olacaktır. Kusursuz ve ayrıntılı TCG BARBAROS modeli, hareket sistemini doğruladıktan sonraki ayrı bir üretim aşamasıdır.

## 2. İlk MVP'nin tek cümlelik hedefi

Oyuncu, basit bir limanda sancak ve iskele makinelerini ayrı ayrı kullanarak gemiyi ileri–geri götürebilmeli, döndürebilmeli ve işaretli yanaşma bölgesine getirebilmelidir.

## 3. Platform ve teknoloji

- Oyun motoru: Unity 6, C#
- Hedef cihaz: İlk olarak Android telefon
- Ekran yönü: Yatay
- 3B üretim: Blender
- Görsel yaklaşım: Temiz, okunaklı, düşük ayrıntılı prototip
- Kamera: Gemiyi arkadan ve hafif yukarıdan izleyen takip kamerası

## 4. Oynanış döngüsü

1. Oyuncu limanda hareketsiz başlar.
2. İskele ve sancak makine kollarını bağımsız olarak ayarlar.
3. Dümeni sancak veya iskeleye basar.
4. Geminin süratini, baş yönünü ve kıç suyunu takip eder.
5. Gemiyi yeşil hedef alanına düşük süratle getirir.
6. Konum, yön ve sürat uygunsa görev tamamlanır.

## 5. MVP kontrolleri

| Kontrol | Aralık | Etki |
| --- | ---: | --- |
| İskele makine | -100…+100 | Tornistan, stop ve ileri yol |
| Sancak makine | -100…+100 | Tornistan, stop ve ileri yol |
| Dümen | -35°…+35° | İskele veya sancak dönüşü |
| Kamera | Tek düğme | Takip ve üstten görünüm arasında geçiş |
| Acil stop | Tek düğme | İki makineyi sıfırlar |

Örnek davranış: Sancak makine ileri, iskele makine stop durumunda geminin başı iskeleye yönelir. İki makine eşit ileri çalışırsa gemi düz ilerler. Biri ileri, diğeri tornistan çalışırsa gemi düşük süratte kendi boyuna yakın bir dönüş yapar.

## 6. Basitleştirilmiş fizik modeli

İlk MVP tam hidrodinamik simülasyon olmayacak; oyuncuya doğru hissi veren, ayarlanabilir bir model kullanılacaktır.

- Her makine gemiye ayrı bir ileri/geri kuvvet uygular.
- Kuvvetler geminin kıçında ve sancak/iskele tarafında uygulanır.
- Dümenin etkisi ileri veya geri su akışı arttıkça güçlenir.
- Su direnci ileri hareketi ve yana kaymayı yavaşlatır.
- Büyük gemi hissi için komutlara gecikmeli tepki ve yüksek atalet uygulanır.
- Çarpışmalar basit gövde çarpanı ile hesaplanır.

## 7. İlk sahne

- Sakin deniz yüzeyi
- Basit rıhtım ve mendirek silüeti
- Bir şamandıra hattı
- Başlangıç noktası
- Yeşil yanaşma/hedef alanı
- Basit fırkateyn silüeti
- Kıç suyu ve pervane çalışmasını anlatan temel parçacık efekti
- Gün ışığı ve hafif pus

## 8. Ekranda gösterilecek bilgiler

- Sürat: knot
- Baş açısı: derece
- İskele ve sancak makine yüzdeleri
- Dümen açısı
- Hedefe kalan mesafe
- Kısa durum mesajları: `STOP`, `İLERİ`, `TORNİSTAN`, `HEDEFTE`

## 9. MVP dışında kalanlar

- Silah sistemleri ve muharebe
- Çok oyunculu mod
- Açık dünya
- Dalga, fırtına ve gelişmiş hava sistemi
- Gerçek köprüüstü ekipmanı
- Hasar modeli
- Ayrıntılı liman trafiği ve yapay zekâ
- Kusursuz gemi modeli ve foto-gerçekçi kaplamalar
- iOS derlemesi

## 10. Kabul ölçütleri

MVP tamamlanmış sayılacaksa:

- Android telefonda akıcı çalışmalı.
- İki makine birbirinden bağımsız kumanda edilebilmeli.
- İleri, tornistan, diferansiyel dönüş ve dümen davranışları gözle anlaşılır olmalı.
- Gemi komutlara aniden değil, ağır bir platform gibi tepki vermeli.
- Rıhtıma çarpma algılanmalı.
- Hedef alana doğru yön ve düşük süratle girildiğinde görev tamamlanmalı.
- Kontroller ilk kez oynayan biri tarafından kısa açıklamayla anlaşılmalı.

## 11. Onaydan sonra geliştirme sırası

1. Unity proje iskeleti ve Android ayarları
2. Deniz, liman blokları ve geçici gemi gövdesi
3. Çift makine kuvvet modeli
4. Dümen ve su direnci
5. Mobil kumanda arayüzü
6. Kamera, göstergeler ve kıç suyu
7. Yanaşma hedefi ve görev sonucu
8. Telefonda performans testi

## 12. Birlikte kararlaştıracağımız noktalar

- Ana görünüm: köprüüstüne yakın takip mi, üstten taktik görünüm mü?
- Kontrol biçimi: dikey makine kolları mı, büyük dokunmatik düğmeler mi?
- İlk görev: düz rotada duruş mu, liman içinde yanaşma mı?
- Görsel ton: gerçekçi gri/deniz tonu mu, daha sade eğitim simülatörü mü?
- Geminin ilk silüeti: çok basit blok model mi, Barbaros oranlarına daha yakın orta ayrıntı mı?

