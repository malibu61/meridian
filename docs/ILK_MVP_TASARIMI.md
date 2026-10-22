# Meridian — Geliştirme Kaydı ve Güncel Durum

> Güncelleme: 22 Ekim 2026
>
> Durum: MVP-0 hareket prototipi tamamlandı. TCG BARBAROS dış modeli, S-70B, 26 personel, PC/mobil kamera kontrolleri ve baca dumanı mevcut. Mod seçimi menüsü ve ayrı Boğaz Geçişi prototipi eklendi. Android cihaz testi, foto-gerçekçi nihai model ve yayın hazırlığı tamamlanmadı. Boğazın kıyıları/yükseltileri henüz gerçek harita verisinden üretilmedi; ilk çevre elle hazırlanmış taslaktır.

Bu belge güncel ana geliştirme kaydıdır. Eski MVP planı en sonda tarihsel kapsam olarak korunur; o plandaki hedefler yapılmış özellik anlamına gelmez. Özelliklerin kaynakları, test sınırları ve gönderim durumu aşağıda ayrı belirtilir.

## Proje kimliği ve kayıt düzeni

- Proje adı ve aktif ana klasör: **Meridian / meridian**.
- Aktif Unity projesi: `C:/Users/Muhammet Ali/source/repos/malibu61/meridian`.
- Bu sohbetin çalışma yolu olarak görünen `maritimeai2`, aktif Unity projesi değildir; geliştirme ve belgelerin asıl hedefi `meridian` klasörüdür.
- Amaç: başlangıçta TCG BARBAROS (F-244), sonrasında farklı gemilere genişleyebilen mobil gemi manevra oyunu.
- İlk onaylanan kapsam: basit gemi/liman, ileri–geri hareket, bağımsız makineler ve dümen. İlerleyen isteklerle gemi dış görünüşü, çevre, kamera, personel ve efektler ayrıntılandırıldı.
- Motor/dil: Unity **6000.6.6f1**, C#. Hedef: öncelikle yatay ekran Android; masaüstü Unity Editor geliştirme/test ortamıdır.
- Mevcut görseller ağırlıklı olarak C# ile üretilen prosedürel mesh ve malzemelerdir; nihai Blender/UV/PBR üretimi yapılmış sayılmaz.
- Yeni kapsam: mevcut liman çalışmasını koruyarak Çanakkale Boğazı'nın kuzeyinden güneye geçiş. Araştırmadan sonra mod menüsü, ayrı taslak çevre ve sıralı rota ilerlemesi uygulandı; gerçek coğrafi veri henüz indirilip işlenmedi.
- Aşağıdaki önceki test kayıtları korunur. Yeni menü/boğaz geliştirmesinin çalıştırılan testleri ayrıca [mod belgesinde](MODLAR_VE_BOGAZ_GECISI.md) kayıtlıdır.

## Güncel geliştirme özeti

### 22 Ekim 2026 — RÜZGAR, KUMKALE, dört makine ve gemi hareketi

- Menüde dört gemi vardır: BARBAROS, ANADOLU, RÜZGAR ve KUMKALE. Yeni iki küçük geminin gövde/üst yapı/numara/donanımı ayrı oluşturuldu. [Fotoğraf referansları ve kapsam](RUZGAR_VE_KUMKALE.md).
- Yalnız RÜZGAR: dış/iskele, iç/iskele, iç/sancak ve dış/sancak için dört bağımsız gaz kolu; merkezde kompakt dümen ve dört makineye ortak gaz. Eski üç geminin iki kollu düzeni korunur.
- Knot hesabındaki 4:1 model ölçeği düzeltildi. Gemiye özel sürat hedefi, dümen geçişi, gecikmeli dairesel dönüş, dönüşte dışa yatış ve yönlü rezonanslı yalpa eklendi. Analog kadranın güncel aralığı ±40° oldu; aşağıdaki ±20° kayıtları tarihsel sürümdür.
- Sert borda denizi ile baş/kıç denizi artık farklı eksen tepkisi üretir. Gerçek GM/GZ/RAO, gizli manevra kartı veya devrilme sertifikası iddiası yoktur. [Sürat/dönüş/yalpa, kaynaklar ve son testler](SURAT_DONUS_YALPA.md).
- Aktif proje `meridian` klasöründedir. Bu değişikliklerde yeni commit/push yapılmadı. Önceki test/gönderim kayıtları aşağıda korunur.
- Son tur: 39 fizik senaryosu/89 kontrol, 7949 yeni gövde geometri kontrolü, 150 canlı model/mod kontrolü, 515 dört-kol mantık ve 69 canlı kumanda kontrolü geçti. Önceki kontrol regresyonu 727/22 geçti. Son C# kaynak derlemesi başarılıdır.
- RÜZGAR 41, KUMKALE 25, BARBAROS 32, ANADOLU 21 kn ölçüldü. Sert borda denizinde yeni küçük gemiler yaklaşık 34,7°/33,1° tepe yalpası verdi. Bunlar kontrollü oyun fizik testi sonuçlarıdır; gerçek cihaz/deniz denemesi değildir.
- 18 offscreen gemi renderı üretildi. Batch modunda HUD görüntüsü oluşmadığı ve açık editörün fare kontrolü kesildiği için yeni HUD için etkileşimli görsel onay/gerçek parmak testi yapılmış sayılmaz. Açık eski Play oturumunun durdurulup yeniden başlatılması gerekir. Ayrıntı ve ham kanıtlar yukarıdaki fizik belgesindedir.

### 22 Ekim 2026 — sarkaçlı analog yalpa kadranı

- Referans eğimölçerin mantığında altta sıfır, çift yönlü derece yayı ve hareketli ibre eklendi. Ayrı sol üst panel ve başlıktaki sayı aynı fiziksel yalpa verisini kullanır.
- ±20° ölçüm aralığı küçük yatışları görünür kılan geniş grafik yaya yayılır. Ölçek dışına çıkıldığında sayı gerçek açıyı korur, ibre sınırda kalır ve uyarı gösterilir. Bu bir gemi stabilite sınırı değildir.
- Yeni panel ortak safe-area yerleşimine/kamera engeline dahil edildi; makine/dümenle çakışmaz ve komut üretmez. Test kapsamı ve görüntüler: [Analog yalpa kadranı](YALPA_KADRANI.md).
- Genişletilmiş turda 727 mantık/yerleşim ve 22 canlı kontrol geçti. İskele 10°, denge ve sancak 10° Game View görüntüleri ayrı incelendi.

### 22 Ekim 2026 — anlık yalpa telemetrisi

- Üst başlığa ROTA ile MAKİNE arasında beşinci metrik olarak YALPA eklendi. Bir ondalık derece ve İSKELE/SANCAK/DENGE yönü gösterilir; anlık gövde açısı doğrudan `ShipBuoyancy.RollDegrees` üzerinden okunur.
- Safe-area içindeki beş metrik ortak `HelmLayout` hesabını kullanır. ±10° fiziksel gövde yatışı ve yalnız baş değiştirmede sıfır yalpa testleri iki gemi için eklendi. Ayrıntı: [Kumanda ve arayüz](KUMANDA_VE_ARAYUZ.md).
- Genişletilmiş kumanda regresyonu geçti: 529 mantık/yerleşim ve 22 canlı kontrol. HUD görüntüsü incelendi; anlık açı ve yön okunuyor, diğer göstergelerle çakışmıyor.

### 21–22 Ekim 2026 — borda çarpması, rota etkisi ve su/menü düzeltmeleri

- Dört gövde desteğine dalganın yatay orbital akışından kuvvet eklendi; gerçek Rigidbody sürüklenir ve dönüş momenti alır. Baş ve yer üzerindeki rota göstergelerinin farklılaşması artık yalnız makine/dümen kaynaklı değildir.
- İki bordada toplam sekiz temas örneği, yükselen su ve geliş açısına bağlı parçacık sıçraması ve yüzey köpüğü üretir. Efektler tekrar kullanılır; iki yeni sıçrama yayıcısının toplam üst sınırı 448 parçacıktır.
- Daha belirgin geometrik dalga, tepe köpüğü ve gemi çevresinde sıklaştırılmış su mesh'i eklendi. `Mathf.SmoothStep` kullanımından kaynaklanan, merkezde bile dalga genliğini %24'e düşüren hata giderildi. Uzak su düzlemine yakın yama maskesi uygulanarak dalga çukurlarının düz suyla örtülmesi de giderildi.
- Seçili ve seçili olmayan düğmelerin font/boyut/hizalama/padding ayarları eşitlendi. Geniş su parlaması kısıldı ve prosedürel gökyüzünün güneş diski kapatıldı; kullanıcının tam parlak-leke olayı tekil olarak yeniden üretilmiş sayılmaz.
- Kapsam, görsel kontrol ve test sonuçları: [Dalga çarpması ve düzeltmeler](DALGA_CARPMA_VE_DUZELTMELER.md). Cihaz/Metal performans onayı ve foto-gerçekçilik iddiası yoktur.

### 21 Ekim 2026 — yönlü dalga seçimi ve gövde fiziği

- Menüye DENİZ sekmesi, beş seviye ve sekiz dünya pusulası geliş yönü eklendi. Yeni seyir veya mevcut seyirde sıfırlamadan uygulama akışları ayrıldı.
- Eski görsel sinüs sallanması kaldırıldı. Dört su desteğinden gerçek Rigidbody'ye uygulanan yay-sönüm kuvvetleriyle yalpa, baş-kıç ve düşey hareket eklendi; Anadolu ve Barbaros farklı profiller kullanır.
- Su, mikro dalga shader'ı ve köpük aynı yön/seviye ayarlarını izler. Takip kamerasının ufku gövde yalpasından ayrıldı. Yatay itki/direnç korunur.
- Fizik ölçümleri, kullanım ve gerçekçilik sınırları: [Dalga seçimi ve gemi fiziği](DALGA_SECIMI_VE_GEMI_FIZIGI.md).

### 21 Ekim 2026 — deniz ve köpük yenilemesi

- Çizgili su görünümü yerine katmanlı mikro dalga shader'ı, açıya bağlı yansıma hissi ve güneş parlaması uygulandı. Gerçek sahne yansıması değildir.
- Kare köpükler, saydam/yumuşak kenarlı organik yüzey köpüğüne dönüştürüldü. İskele/sancak izleri gerçek makine yükünü, baş köpüğü ileri hızı izler. İki yana açılan baş köpüğü ve hareketli kıyı yıkaması eklendi.
- Deniz ve köpük ortak dalga yüksekliğini kullanır; gemi/mod değişiminde temizlenir, menüde duraklar. Kaynak yaşam döngüsü ve parçacık/köşe bütçeleri sınırlandırıldı.
- Araştırma değil, gerçek Unity derleme/seyir testleri ve görsel karşılaştırma yapıldı. Sonuçlar ve iPhone/native test sınırı [deniz belgesinde](DENIZ_VE_KOPUK.md) kayıtlıdır.

### 21 Ekim 2026 — kademeli gaz, ortak kol ve arayüz

- İskele/sancak makine komutları 10 ileri (`+10…+100%`) ve 5 tornistan (`−20…−100%`) kademesine geçirildi. Sıfır ayrı STOP konumudur; motorun gerçek yük artışı yumuşak kalır.
- Çizgili/sayısal gaz rayları, dümen ile sancak gazı arasında iki makineyi kontrol eden ortak kol eklendi. Ayrı komutlarda ortak panel AYRI gösterir; komutları kendiliğinden birleştirmez.
- Klavye için ilk basış ve basılı tutma kademe tekrarı uygulandı. Fare/touch yakalama, kol dışına sürükleme, iki ayrı parmakla bağımsız kollar ve ortak kolun geçici sahipliği ayrıldı.
- Menü, telemetri ve kumandalar ortak grafik diline taşındı; safe-area yerleşimi ve kamera engelleri aynı hesaptan üretilir. Mod ve gemi seçimi ile boğaz rota krokisi korunur.
- Unity'de son tur dahil 491 mantık/yerleşim, 16 canlı kumanda testi geçti. Ortak/bağımsız kol, dümen ve stop fareyle de denendi. Son düğme boyutu, görseller ve sonuçlar [kumanda belgesinde](KUMANDA_VE_ARAYUZ.md) kayıtlıdır. Gerçek iOS kurulumu/iki parmak donanım testi yapılmış sayılmaz.

Aşağıdaki önceki kayıtlar tarihsel olarak korunmuştur; sürekli gaz girişinden söz eden bölümlerin yerine artık kademeli komutlar kullanılır.

Proje, başlangıçtaki basit blok prototipten oynanabilir bir TCG BARBAROS (F-244) manevra demosuna dönüştürüldü. Şu anda Unity Editor içinde gemi hareket ettirilebiliyor, iki makine bağımsız kullanılabiliyor, dümen kontrol edilebiliyor ve kamera geminin çevresinde döndürülebiliyor.

### Tamamlanan altyapı

- Unity 6.6 ve C# tabanlı proje oluşturuldu.
- Ana sahne ve çalışma anında sahneyi kuran başlangıç sistemi hazırlandı.
- Proje Git deposuna dönüştürüldü ve `malibu61/meridian` GitHub deposuna ilk prototip gönderildi.
- Gemi dış modeli, helikopter/personel ve zoom `376beb7`; baca dumanı ve önceki belge güncellemesi `45677bb` commit'inde gönderildi. Menü ve ayrı Boğaz Geçişi prototipi daha sonraki yerel geliştirmedir. Ayrıntı: aşağıdaki sürüm geçmişi.
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
- Acil stop iki makine komutunu birlikte sıfırlıyor; geminin mevcut süratini anında sıfırlamaz.
- Bu, ayarlanabilir bir oyun fiziğidir. Gerçek hidrodinamik katsayılar, gerçek makine tesisatı ve geminin ölçülmüş manevra karakteristiğiyle doğrulanmış bir mühendislik simülasyonu değildir.

### TCG BARBAROS (F-244) görsel modeli

Model, kamuya açık dış görünüş fotoğrafları ve MEKO 200 TN Track II-A'nın yaklaşık `118 m × 14,8 m` oranları esas alınarak prosedürel biçimde oluşturuldu.

- Yetmiş en kesitle oluşturulan kapalı gövde; uzun/dar oran, başa doğru daralan ve yükselen borda, gövdeye uyan güverte
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
- Her iki bordada gövde yüzeyine oturan dolu stencil geometrili `F244` numarası; iskele/sancak sırası ayrı hesaplanır. Önceki havada duran TextMesh yaklaşımı bu modelde kullanılmaz.
- Kıç gönderinde prosedürel dokuyla oluşturulan Türk bayrağı
- Hafif yalpa ve baş-kıç hareketi veren görsel gemi hareketi

Bu model önceki blok fırkateynin yerini aldı. Modelin donanım düzeninde kamuya açık 2009–2022 fotoğrafları esas alındı; modernizasyon dönemleri rastgele karıştırılmamalıdır. Ayrıntılı dış model, sekiz genel açı ve personel/helikopter yakın planlarıyla tekrar tekrar kontrol edildi. Kapalı gövde ve üst yapı yüzeyleri düzeltildi; köprü yan kapılarındaki açıklıklar personel geçişi için bilerek bırakıldı ve iç taban/bölmeler eklendi. Mevcut model fotoğraf referanslı, stilize/prosedürel bir oyun modelidir; foto-gerçekçi nihai model için Blender'da ölçülü modelleme, UV ve PBR kaplama çalışması gereklidir.

Ayrıntılı referans ve düzeltme kaydı: [Barbaros görsel incelemesi](BARBAROS_GORSEL_INCELEME.md).

### Deniz ve efektler

- Sabit düzlem yerine yaklaşık 2.000 köşeli dinamik deniz mesh'i kullanılıyor.
- Birbiriyle kesişen sinüs dalgalarıyla hareketli su yüzeyi oluşturuluyor.
- Prosedürel deniz dokusu kaydırılarak küçük yüzey hareketleri destekleniyor.
- İskele ve sancak pervaneleri için ayrı kıç suyu parçacıkları bulunuyor.
- Sürate göre artan baş köpüğü efekti bulunuyor.
- Kıyı ve liman duvarlarında köpük şeritleri bulunuyor.
- Deniz üzerindeki görsel hareket mobil performans düşünülerek düşük maliyetli tutuldu.
- İki bacaya makine yükünü ayrı izleyen egzoz dumanı eklendi; normal/ani yük davranışları aşağıdaki özel bölümde kayıtlıdır. Temel duman sürümü `45677bb` ile gönderildi; mod değişiminde duman sıfırlama daha sonraki yerel eklemedir.

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
- PC'de `+ / -` ve sayısal klavye `+ / -` zoom yapar; `R` başlangıç kadrajına döndürür.
- Yakınlaşma sınırı önceki 17 model biriminden 6'ya indirildi; aralık 6–60, başlangıç mesafesi 34 model birimidir.
- Pinch, parmak mesafelerinin oranından hesaplanır. İki parmak da kontrol panelleri dışında başlamalıdır; panelden başlayan hareket sonradan denize taşınsa bile kamerayı ele geçirmez. Parmak değişimi, iptal ve odak kaybı ele alındı.

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
| `+ / -`, sayısal klavye `+ / -` | Kamerayı yakınlaştırır/uzaklaştırır |
| `R` | Başlangıç kamera kadrajına döner |
| Tek parmak sürükleme | Mobil kamerayı döndürür |
| İki parmak hareketi | Mobil yakınlaştırma/uzaklaştırma |

### Test ve düzeltme döngüleri

- Unity Console'da C# derleme hataları sıfırlanana kadar proje yeniden derlendi.
- Gemi Unity Play modunda çalıştırıldı ve farklı kamera açılarından incelendi.
- İlk yan profil testinde görülen kısa-geniş gövde ve kutu biçimli üst yapı yeniden tasarlandı.
- Köprü, hangar, baca ve top kulesi eğimli özel mesh'lere dönüştürüldü.
- İlk aşamada ters `F244` yazısı için kamera tarafına bağlı görünürlük sistemi eklendi. Sonraki dış model geçişinde numaralar bordaya oturan stencil geometrisine dönüştürüldü; alfa kaplama parçalanması ve yazı sırası düzeltildi.
- Türk bayrağındaki gereksiz MeshCollider çalışma zamanı uyarısı giderildi.
- Kamera yakınlığı ve başlangıç açısı görsel test sonucuna göre ayarlandı.
- Git fark denetiminde boşluk veya bozuk yama hatası bulunmadı.

## Güncel aşama değerlendirmesi

| Bölüm | Durum |
| --- | --- |
| Unity proje iskeleti | Tamamlandı |
| Çift makine ve dümen fiziği | Oynanabilir prototip; gerçek gemi kalibrasyonu yapılmadı |
| Masaüstü ve dokunmatik kontroller | Uygulandı; fiziksel mobil cihaz doğrulaması bekliyor |
| Canlı seyir göstergeleri | Mevcut; metre/model birimi tutarlılığı düzeltilmeli |
| Yörünge kamera sistemi | Tamamlandı |
| Stilize TCG BARBAROS modeli | MVP için tamamlandı |
| Dinamik deniz ve kıç suyu | MVP için tamamlandı |
| Liman çevresi | İlk görsel geçiş tamamlandı |
| S-70B Seahawk | Park edilmiş prosedürel dış model mevcut; uçuş yok |
| Güverte ve kırlangıç personeli | 26 kişi; halat, gözcü ve yürüme animasyonları mevcut |
| Fare/klavye zoom ve mobil pinch | Uygulandı; 23 mantık kontrolü ve PC canlı test kaydı mevcut |
| Makine yüküne bağlı baca dumanı | `45677bb` ile gönderildi; 17 tepki kontrolü ve canlı bağlantı test kaydı mevcut |
| Menü ve mod seçimi | Açılış, MENÜ/Esc, duraklat/devam ve yeniden başlat akışı eklendi |
| Çanakkale kuzey–güney geçişi | Ayrı oynanabilir taslak; 12 sıralı kontrol noktası, kıyı ve köprü silüetleri |
| Şamandıraya bağlama | Yakında kartı; başlatma pasif, görev henüz yok |
| Gerçek OSM/DEM coğrafyası, akıntı ve streaming/floating origin | Henüz uygulanmadı; elle hazırlanmış yaklaşık koordinatlı taslak bunların yerine geçmez |
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
| `OrbitZoomInput.cs` | Zoom/pinch giriş durumu, aralıklar ve panel hareketi ayrımı |
| `PrototypeHud.cs` | Göstergeler ve mobil kumandalar |
| `OceanSurface.cs` | Dinamik deniz mesh'i ve dalgalar |
| `WakeTrail.cs` | Pervane izi ve baş köpüğü |
| `ShipVisualMotion.cs` | Görsel yalpa ve salınım |
| `RotatingRadar.cs` | Radar dönüş animasyonu |
| `BeaconPulse.cs` | Deniz feneri ışığı |
| `BarbarosModelBuilder.cs` | Kapalı gövde, donanım, stencil borda numaraları ve gemi dış modelinin üretimi |
| `SeahawkModelBuilder.cs` | Park edilmiş S-70B dış modeli |
| `NavalDeckLife.cs` | 26 personel, halat/gözcü/yürüyüş pozları ve köprü kapısı rotaları |
| `EngineExhaustResponse.cs` | Makine yükü ve ani komut değişiminden duman tepkisi hesaplama |
| `EngineExhaustSmoke.cs` | İki bacanın parçacıkları ve prosedürel bulut dokusu |
| `HullNumberVisibility.cs` | Önceki borda yazısı yaklaşımından kalan görünürlük bileşeni |
| `Assets/Shaders/Resources/HullMarking.shader` | Gövdeye oturan borda numarası malzemesi |
| `Assets/Shaders/Resources/FunnelSmoke.shader` | Baca dumanı malzemesi |
| `Assets/Editor/ProjectInitializer.cs` | Proje/sahne başlangıç kurulumu |
| `Assets/Editor/BarbarosVisualReview.cs` | Çok açılı render ve personel/rota/runtime incelemesi |
| `Assets/Editor/CameraZoomReview.cs` | 23 zoom giriş kontrolü ve kamera inceleme araçları |
| `Assets/Editor/EngineExhaustReview.cs` | 17 duman tepki kontrolü ve dört durumlu görsel çalışma |
| `GameSessionController.cs` | Aktif mod, menü/duraklat/devam, görev ilerlemesi ve bitiş |
| `ModeSelectionMenu.cs` | Mod kartları, açıklamalar, başlat/devam, görev HUD'ı ve rota krokisi |
| `StraitRoute.cs` | Elle hazırlanmış yaklaşık coğrafi merkez hattı, genişlikler ve etiketler |
| `StraitEnvironment.cs` | Ayrı taslak kıyı/tepe, köprü, yerleşim ve kontrol çemberi |
| `VoyageProgress.cs` | Sıralı ve hareket yönünü kontrol eden oyun geçiş noktaları |
| `EnvironmentResources.cs` | Mod değişiminde üretilmiş mesh/malzemelerin temizliği |
| `Assets/Editor/ModeSelectionReview.cs` | Rota, mod değişimi ve canlı hareket/duraklatma testleri |

## Sıradaki önerilen geliştirme sırası

Bu sıra bir öneridir; bu belgeyi güncellemek uygulama veya yeni bir push başlatmaz.

1. Çanakkale kapsamını ve gerçek ölçek standardını kesinleştirme; mevcut metre/model birimi uyumsuzluğunu kontrollü düzeltme.
2. Coğrafi verileri indirip kıyı, yükseklik ve kapsama hatalarını karşılaştırma; bütün boğazın sade coğrafi iskeletini çıkarma.
3. Nara–Çanakkale–Kilitbahir bölümünü görsel kalite örneği yapma; yakın/uzak arazi yükleme ve koordinat altyapısını kurma.
4. Köprü ve diğer önemli yapılarla tam güzergâhı ayrıntılandırma; akıntı, görev kontrol noktaları ve geçiş kaydını ekleme.
5. Liman eğitimini koruma; yanaşma hedefi, başarı/başarısızlık ve güvenilir çarpışma geri bildirimini tamamlama.
6. Gerçek Android cihazda dokunmatik, CPU/GPU, bellek ve uzun süreli seyir testi; kalite seviyeleri.
7. Makine/deniz/rüzgâr/düdük sesleri, ana menü, ayarlar ve kısa eğitim.
8. Ölçülü nihai gemi modeli, UV/PBR ve dönem doğrulaması; yayın kabul çalışması.
9. Yerel duman ve belge değişiklikleri için ancak ayrıca istendiğinde commit/push.

## S-70B ve hareketli personel — tamamlananlar

### Helikopter

- Uçuş güvertesine park edilmiş S-70B Seahawk eklendi: kapalı gövde, kokpit/kabin camları, kapı, motor muhafazaları, burun altı radom/sensör, tekerlekli iniş takımı ve kuyruk yüzeyleri.
- Ana rotorun dört pali geriye katlanmış; dört palli kuyruk rotoru, tekerlek takozları ve bağlama zincirleri bulunur.
- Camların gövde tarafından örtülmesi yüzeye uyan geometriyle düzeltildi. Statik parçalar sekiz malzeme grubunda birleştirildi.
- Helikopter uçmaz; motor/rotor çalıştırma, iniş/kalkış ve oyuncu kontrolü eklenmedi. Gerçek seri numarası uydurulmadı.

### Personel dağılımı ve davranış

| Yer | Sayı | Davranış |
| --- | ---: | --- |
| Baş güverte | 8 | 6 halatçı + 2 baş/gövde çeviren ve dürbün kullanan gözcü |
| Kıç güverte | 8 | 6 halatçı + 2 hareketli gözcü |
| İskele köprüüstü/kırlangıç | 8 | 4 dış gözcü + iç/dış rota izleyen 4 yürüyen personel |
| Sancak köprüüstü/kırlangıç | 2 | 1 dış gözcü + 1 yürüyen personel |
| Toplam | 26 | Ekip sayıları içeride bekleyenleri de kapsar |

- Halatçılar yaklaşık 3,6 saniyelik faz farkları olan döngülerde kol/dirsek/el ve bacak pozlarını değiştirir; eller halat hattını izler.
- Baş/kıçta baba, eğitim halatı ve yedek halat istifi bulunur. Halat bir görsel çalışma döngüsüdür; kıyıya bağlı fiziksel palamar veya makine emniyet kilidi değildir.
- Beş yürüyen personel köprü içinden yan kapıya, kırlangıç yoluna ve dış gözetleme noktasına gider; sonra geri döner. İskele ekibi aynı kapıda yığılmamak için sırayla hareket eder.
- Köprü yan duvarlarında gerçek açıklıklar, söve/eşik, içe açık kapı kanadı, iç taban ve koyu bölmeler oluşturuldu. İçerideki personel ışınlanarak değil, dış yapıyla doğal biçimde örtülür.
- Yürüyüşte adım, ayak kaldırma, diz ve karşı kol salınımı; gözcülerde baş çevirme ve dürbün hareketi bulunur. Animasyon geminin yerel koordinatlarını izler; poz tamponları tekrar kullanılır.
- Kamuya açık fotoğraflardan lacivert iş kıyafeti, siyah bot, turuncu can yeleği, reflektif ayrıntılar, eldiven ve koruyucu başlık incelendi. Köprü ekibinde lacivert kıyafet/başlık kullanıldı.
- Bunlar stilize prosedürel karakterlerdir; birebir üniforma/rütbe/arma replikası veya foto-gerçekçi insan modeli değildir. Fotoğraflar oyuna kaplama olarak indirilmedi.

Kaynak fotoğraflar, ayrıntılar ve test ölçümleri: [Güverte yaşamı](GUVERTE_YASAMI.md).

## Makine yüküne bağlı baca dumanı — yerel geliştirme

- Her bacada ayrı parçacık sistemi var. Çıkışlar gemiyle hareket eder; çıkan duman dünya koordinatında kalır ve gemiye yapışmaz.
- Gerçeklenen iskele/sancak makine yükleri ayrı okunur. Hafif rölanti, normal seyir ve ani yüksek yol emri birbirinden ayrılır.
- Yavaş gaz açmak siyah ani yük darbesi oluşturmaz. Sert yüklenmede duman daha koyu, yoğun ve hızlı çıkar; koni daralır, kısa ek parçacık darbesi oluşur.
- Ani etki yaklaşık 2,4 saniyede azalır. Tam yol sabit tutulunca normal tam yük seviyesine oturur; her kare yeniden tetiklenmez.
- Tornistan mutlak yükle değerlendirilir; sert yön değişimi ek darbe oluşturabilir. Gaz kısmak yeni darbe başlatmaz.
- `ShipController` yükleri salt okunur açar; duman çalışmasında itki/dümen davranışı değiştirilmedi.
- Baca başına en fazla 240 parçacık, ortak 64×64 prosedürel bulut dokusu ve özel shader kullanılır. Gerçek egzoz tesisatı/emisyon davranışının doğrulanmış modeli değildir.
- Bu temel duman dosyaları `45677bb` ile gönderildi. Sonraki menü çalışmasında mod değişiminde parçacıkların ve geçici egzoz yük tepkisinin sıfırlanması eklendi.

Ayrıntılar: [Baca dumanı](BACA_DUMANI.md).

## Toplu test ve doğrulama kaydı

Aşağıdakiler önceki geliştirme döngülerinden kayıtlı sonuçlardır. 19 Ekim belge güncellemesinde test araçları ve çıktı dosyalarının varlığı incelendi; tüm testler yeniden çalıştırılmadı. Bunlar yayın sertifikasyonu veya fiziksel Android cihaz testi yerine geçmez.

| Alan | Kayıtlı doğrulama | Sınır |
| --- | --- | --- |
| Dış model | Sekiz genel açı; kapalı yüzey, cam, borda yazısı ve donanım kontrolü; `BARBAROS_VISUAL_REVIEW_COMPLETE` | Masaüstü Unity Editor; foto-gerçekçilik onayı değil |
| Güverte | `DECK_LIFE_CHECK_PASS`: 26 kişi, yaklaşık 0,095 birim el hareketi, 0,0000 halat sapması | Örneklenmiş animasyon geometrisi |
| Köprü vardiyası | `BRIDGE_WATCH_CHECK_PASS`: 5 rota, 8+8+8+2 dağılım, 0,770 birim yürüme ve 49,2° baş dönüşü | Rota sınırları, kapı ve taban kontrolü |
| Canlı personel | `DECK_LIFE_RUNTIME_PASS`: 98 gerçek güncelleme karesi, 0,106 el/0,708 yürüme birimi, 37,8° baş dönüşü | Play-mode animasyon; mobil performans ölçümü değil |
| Zoom | `CAMERA_ZOOM_CHECK_PASS checks=23 range=6..60` | Pinch sentetik girdilerle test edildi |
| Gerçek PC girdisi | Fareyle 34,000 → 30,155 → 34,000; sayısal klavye artı 34,000 → 33,032; `R` ayrıca denendi | Fiziksel telefon pinch testi yapılmadı |
| Duman tepkisi | `ENGINE_EXHAUST_CHECK_PASS checks=17` | Yük/ani darbe mantığı; gerçek emisyon doğrulaması değil |
| Duman karşılaştırması | Baca başına rölanti 0,80; yarım yol 7,80; ani tam yoldan 0,7 sn sonra 51,13; sabit tam yol 14,80 parçacık/sn | Görsel çalışma örnekleri; FPS ölçümü değil |
| Canlı makine–duman | İskele 0 / sancak +1: üretim 0,80 / 14,80; 52 canlı parçacık gözlendi | Bağımsız baca bağlantısı doğrulandı |
| Derleme | Önceki son kontrollerde başarılı C# derleme kayıtları; ilgili son test günlüklerinde C#/shader hatası görülmedi | Bu belge işlemi yeni derleme çalıştırmadı |

Tekrarlama araçları:

- `Meridian > Visual Review - 8 Views`: sekiz genel görünüş ve helikopter/personel için on yakın plan. Çıktı: `Temp/BarbarosVisualReview`.
- `Meridian > Camera Zoom - Check Input`: 23 giriş kontrolü. Kamera görselleri: `Temp/CameraZoomReview`.
- `Meridian > Engine Exhaust - Check Response`: 17 tepki kontrolü.
- `Meridian > Engine Exhaust - Capture Study`: ayrı geçici gemide rölanti, seyir, ani tam yol ve oturmuş tam yol. Çıktı: `Temp/EngineExhaustReview`.
- İlgili `request.txt` dosyalarındaki yeni kimlikler Play modunda inceleme isteklerini tetikleyebilir. Geçici renderlar ve test klasörleri Git'e alınmaz; kalıcı kanıt özeti ilgili Markdown belgelerindedir.
- Düzeltilen örnek sorunlar: eksik/yanlış dış yüzeyler, borda yazısı konumu/ayna sırası/alfa parçalanması, bayrak MeshCollider uyarısı, helikopter cam örtülmesi, personel kol uzanması ve boyun boşluğu. Bir inceleme sırasında Unity ILPP `pipe broken` sorunu editör yeniden başlatılarak giderildi.

## Çanakkale Boğazı — araştırma ve önerilen uygulama

**Araştırma kaydı ve güncel sınır:** Aşağıdaki araştırma kullanıcıya raporlandı. Sonrasında mod menüsü ve ayrı oynanabilir taslak çevre eklendi; aşağıdaki gerçek veri/GIS üretim planı henüz gerçekleştirilmedi. Veri dosyaları indirilip üst üste doğrulanmadı; çözünürlükler yerel doğruluk garantisi değildir. Mevcut yaklaşık merkez hattı ve prosedürel kıyılar gerçek seyir talimatı veya ölçülmüş coğrafya değildir. Ayrıntı: [uygulanan ilk mod](MODLAR_VE_BOGAZ_GECISI.md).

### Coğrafya ve güzergâh

Çanakkale Boğazı Marmara ile Ege'yi bağlar. Araştırmada yaklaşık 61 km uzunluk ve 1,2–7 km genişlik verilir; oyundaki toplam rota, seçilecek başlangıç/bitiş sınırına ve izlenen hatta bağlıdır. Geçiş düz bir güney hattı değildir; kıyılar, burunlar ve Nara çevresindeki dönüşler baş değişimini gerektirir. [Bilimsel coğrafya/akıntı kaynağı](https://agupubs.onlinelibrary.wiley.com/doi/10.1029/2012JC008348)

| Kuzeyden güneye bölüm | Önemli çevre ve oyun karşılığı |
| --- | --- |
| Marmara yaklaşımı, Gelibolu–Çardak | Açık denizden boğaza giriş, kıyıların belirginleşmesi |
| Lapseki–Sütlüce, 1915 Köprüsü | Uzaktan tanınan büyük yapı, ölçek hissi ve altından geçiş |
| Köprü güneyi–Nara yaklaşımı | Burun/koy dağılımı ve dönüşten önce görüşü kapatan kıyı |
| Nara Geçidi | Dönüş/daralma; dümen, sürat ve akıntı ilişkisi |
| Eceabat–Çanakkale–Kilitbahir | Yerleşimler, karşılıklı kıyılar, kaleler ve iskeleler |
| Kepez–Dardanos–Erenköy çevresi | Güney kıyıları, değişen yerleşim ve arazi karakteri |
| Seddülbahir–Kumkale, Ege çıkışı | Ufkun açılması, açık denize çıkış ve görev bitişi |

İlk görsel öncelikler: kıyı çizgisi, genişlik, tepe profili, burunların görüşü kapatması ve doğru yapı konumları. Özel model adayları: 1915 Köprüsü, Kilitbahir/Çimenlik kaleleri, Çanakkale kıyı kütlesi, Dur Yolcu düzenlemesi, Şehitler Abidesi ve görünür iskeleler. Köprünün ana açıklığı 2.023 m'dir; oranlar resmî ölçülerden alınmalı. [KGM proje bilgileri](https://www.kgm.gov.tr/Sayfalar/KGM/SiteTr/Projeler/ProjelerDetay.aspx?q=148), [yerel görsel referanslar](https://canakkale.ktb.gov.tr/TR-181870/slider.html)

### Önerilen veriler ve araçlar

| Katman | Kaynak / araç | Sınır ve yapılacak kontrol |
| --- | --- | --- |
| Kıyı, yollar, bina izleri, yerleşim | OpenStreetMap | Yerel kapsam/güncellik denetimi; ODbL atıf ve dağıtım koşulları |
| Arazi yükseltisi | Copernicus DEM GLO-30 | Yaklaşık 30 m örneklemeli yüzey modeli; yapı/bitki etkisi ve kıyı hataları temizlenmeli |
| Deniz tabanı | EMODnet, yerel kapsama göre | Genel ızgara yaklaşık 115 m; hassas liman/karaya oturma doğruluğu varsayılamaz |
| Belirgin yapılar | Ölçü + kamuya açık fotoğraf referansları, Blender | Fotoğraf tek başına ölçülü model veya yeniden kullanım izni değildir |
| Veri hazırlama | QGIS | Kesme, aynı koordinat sistemine getirme, su/kara maskesi ve parçalara ayırma |
| Çalışma zamanı | Unity + C# | Bu kapsam için Node.js/React Native ana oyun altyapısı olmayacak |

Copernicus DEM çıplak toprak modeli değildir; yüzeydeki yapı/bitki etkilerini içerebilir. 30 m veriyi sıklaştırmak gerçek yeni ayrıntı üretmez. Rıhtım, yakın kıyı ve keskin yapı kenarları ayrıca modellenmelidir. Su seviyesi ve yükseklik referansları birlikte kontrol edilmeli; lisans/atıf kayıtları veri paketine eklenmelidir. GLO-30 görüntüleme hizmetinde 2026 itibarıyla kayıt/lisans erişim koşulları da kontrol edilmelidir.

Kaynaklar: [Copernicus DEM](https://dataspace.copernicus.eu/explore-data/data-collections/copernicus-contributing-missions/collections-description/COP-DEM), [2026 görüntüleme erişim duyurusu](https://dataspace.copernicus.eu/news/2026-7-17-copernicus-dem-30m-view-service-license-acceptance), [EMODnet](https://emodnet.ec.europa.eu/en/bathymetry), [OSM lisansı](https://www.openstreetmap.org/copyright/en), [QGIS](https://www.qgis.org/en/).

### Büyük dünya ve ölçek önerisi

- Mevcut `BarbarosModelBuilder` açıklaması yaklaşık **1 model birimi = 4 m** kabul eder. `ShipController.SpeedKnots`, birim/sn değerini doğrudan `1.94384` ile çarparak metre/sn gibi işler. Bu tutarsızlık gerçek mesafe/sürat eşleşmesinden önce düzeltilmeli; mevcut gösterge gerçek gemi sürati olarak kabul edilmemelidir.
- Öneri: **1 Unity birimi = 1 metre** standardına kontrollü geçiş. Model, kamera, itki/direnç, fizik ofsetleri ve efektlerin birlikte yeniden kalibrasyonu gerekir; yalnızca Transform ölçeğini değiştirmek yetmez. Bu henüz uygulanmadı.
- Başlangıç önerisi: yaklaşık **2×2 km** arazi parçaları; yakında ayrıntılı, uzakta sade arazi/tepe silüeti. Boyut ve görünürlük mesafeleri gerçek mobil ölçümle kesinleşmeli.
- Veriler önceden hazırlanıp yerelden yüklenmeli; ilk sürüm için sürekli internet veya canlı harita akışı zorunlu tutulmamalı.
- Unity Terrain komşulukları ve asenkron/eklemeli sahne yükleme kullanılabilir. Addressables henüz projeye kurulmuş kabul edilmez.
- Gerçek konumu yüksek hassasiyette ayrı saklama ve yerel dünyayı gemiye yakın tutan floating origin önerilir. Gemi, kamera, kıyı, dünya koordinatlı duman ve kıç suyu birlikte ele alınmalıdır.
- Mevcut deniz `300×360` model birimi, kamera uzak kesme düzlemi `420` birimdir; bu küçük liman düzenini yalnızca büyütmek bütün boğazı çözmez. Görünür su, uzak kıyılar, yükleme ve LOD ayrı tasarlanmalıdır.

Teknik kaynaklar: [Unity komşu arazi parçaları](https://docs.unity3d.com/6000.0/Documentation/Manual/terrain-CreateNeighborTerrains.html), [Addressables sahne yükleme](https://docs.unity3d.com/Packages/com.unity.addressables@2.7/manual/LoadingScenes.html).

### Su, akıntı ve seyir süresi

- Genel su alışverişi üst tabakada Marmara→Ege, altta ters yöndedir. Kısa dönem değişimleri ve tersine dönüşler gözlenmiştir; bütün boğaza sabit bir güney vektörü uygulanması doğrulanmış akıntı modeli sayılmaz. [Gözlemsel çalışma](https://agupubs.onlinelibrary.wiley.com/doi/10.1029/2012JC008348)
- İlk oyun önerisi: kontrollü hava altında konuma bağlı, yumuşak değişen basit yüzey akıntısı. Gerçek zamanlı bilimsel akış/CFD çözümü ilk MVP kapsamında değil.
- Suya göre sürat dümen/direnç hesabında, yere göre sürat harita ilerlemesinde ayrı kullanılmalı. Motor stopta akıntıyla sürüklenme desteklenmeli. Mevcut kodda bu akıntı katmanı yok.
- Görsel rüzgâr dalgası, akıntı hissi, kıyı köpüğü ve gemi izi ayrı katmanlar olarak ele alınmalı.
- Örnek 65 km rota, akıntı/manevra yok sayılırsa 15 kn ile yaklaşık 2 sa 20 dk, 20 kn ile 1 sa 45 dk sürer. Bu, son rota ölçümü değil tasarım hesabıdır.
- Öneri: coğrafi ölçeği koruyup tam geçişe ek kısa bölümler, kontrol noktasından devam ve uygun kesimlerde isteğe bağlı zaman hızlandırma sunmak. Dar/geçiş hassasiyeti yüksek yerlerde hızlandırma sınırlandırılmalı.

### Önerilen coğrafya üretim sırası ve kabul

1. Bütün boğazın gerçek veriye dayalı sade kıyı/tepe iskeleti ve kuzey–güney geçişi.
2. Nara–Çanakkale–Kilitbahir kesimini ayrıntılı kalite örneği yapma; diğer bölümler için referans standardı.
3. Köprü, yerleşimler ve güney çıkışını aynı görsel standarda taşıma.
4. Akıntı, görev kontrol noktaları, harita, kayıt/devam ve cihaz performansı. Feribot/diğer gemi trafiği sonraki kapsam.
5. Liman çalışması korunur. Kurgusal eğitim limanı ayrı kalabilir; gerçek boğaza bağlanacaksa gerçek liman konumu/geometrisi ayrıca seçilmelidir.

Kabul: kesintisiz tam geçiş; gemi seviyesinden doğru kıyı/tepe silüeti; tutarlı konum/mesafe/sürat; arazi birleşimlerinde boşluk olmaması; görünen kıyı ile çarpışmanın uyuşması; orbit/zoom'da eksik yüzey görülmemesi; uzun seyirde koordinat/efekt sıçraması ve bellek birikimi olmaması. Belirlenecek hedef telefonda en az 30 FPS bir önerilen hedeftir, ölçülmüş sonuç değildir.

**Açık kararlar:** kesin başlangıç/bitiş, güncel coğrafya ile geminin 2009–2022 görünümünün zaman kurgusu, hedef telefon, ilk ayrıntılı liman, tam geçiş/kısa görev dengesi ve verilerin yerel kalite sınırı. Araştırma yapılmış olması bu kararların onaylandığı veya coğrafyanın tamamlandığı anlamına gelmez.

## Sürüm geçmişi ve gönderim durumu

19 Ekim 2026 yerel denetiminde `origin` adresi `https://github.com/malibu61/meridian.git` olarak okundu. Yerel `HEAD` ve son bilinen `origin/main`, `376beb7605ce68943044fbcfe2797bb222367981` değerindeydi. Bu denetimde fetch/ls-remote yapılmadı; uzak sunucunun anlık durumuna ilişkin yeni doğrulama iddiası yoktur.

| Tarih | Commit | Kapsam |
| --- | --- | --- |
| 18 Ekim 2026 | `dbfecce` | İlk Meridian MVP-0 gemi manevra prototipi |
| 18 Ekim 2026 | `e9ba14f` | TCG BARBAROS görsel MVP ve çevre iyileştirmesi |
| 19 Ekim 2026 | `376beb7` | Dış model ayrıntıları, helikopter/personel animasyonları ve kamera zoom |
| 19 Ekim 2026, yerel | Henüz commit yok | Makine yüküne bağlı baca dumanı, ilgili test araçları/belge ve ana belge güncellemesi |
| 19 Ekim 2026, sonraki gönderim | `45677bb` | Bir önceki satırdaki duman ve belge çalışması commit/push edildi; uzak `main` ile doğrulandı |
| 19 Ekim 2026, son yerel çalışma | Henüz commit yok | Mod seçimi menüsü, ayrı Boğaz Geçişi taslağı, sıralı rota ve testler |

İlk belge güncellemesinde yerel olan duman farkları `45677bb` içinde gönderilmiştir. En son menü/mod geliştirmesi yereldedir; bu özellik isteğinde ayrıca commit/push istenmediği için yeni gönderim yapılmadı.

## Bilinen sınırlar ve yayın öncesi eksikler

- Gerçek gemi manevra verileriyle doğrulama ve metre/knot ölçek tutarlılığı tamamlanmadı.
- Su dalgaları ve geminin yalpası görseldir; tam kaldırma kuvveti/denizde altı serbestlik dereceli fizik yoktur.
- Yanaşma hedefi, görev başarı/başarısızlık ve hasar/çarpışma geri bildirimi tamamlanmadı; limanın görünmesi tam yanaşma görevi anlamına gelmez.
- Üniforma ve gemi dış modeli stilizedir. Yakın plan foto-gerçekçilik, UV/PBR üretimi ve ölçülü nihai Blender modeli yoktur.
- Park helikopteri uçmaz; personel/halatlar görsel animasyondur, operasyon prosedürü simülasyonu değildir.
- Gerçek mobil cihaz testi, Android yayın paketi doğrulaması, CPU/GPU/bellek profili ve mağaza yayını tamamlanmadı. Sentetik pinch testleri bunların yerine geçmez.
- Ses, ayarlar, kalıcı kayıt, tam eğitim, trafik/yapay zekâ, çok oyunculu ve savaş sistemleri uygulanmış sayılmaz. Açılış/mod seçimi menüsü artık mevcuttur; liman hâlâ serbest manevradır.
- Çanakkale kıyı/DEM/batimetri verisi doğrulaması, hassas coğrafi koordinat altyapısı, akıntı ve büyük dünya yükleme henüz yoktur. Güncel Boğaz Geçişi, yaklaşık merkez hattına göre üretilen kıyılarla oynanabilir bir taslaktır.

## Ayrıntılı belge dizini

- [Barbaros görsel incelemesi](BARBAROS_GORSEL_INCELEME.md): fotoğraf kaynakları, donanım dönemi, geometri düzeltmeleri ve sekiz açılı kontrol.
- [Güverte yaşamı](GUVERTE_YASAMI.md): S-70B, kıyafet referansları, 26 personel ve canlı animasyon testleri.
- [Kamera zoom](KAMERA_ZOOM.md): fare/klavye/mobil girdi davranışı ve 23 kontrol.
- [Baca dumanı](BACA_DUMANI.md): yük tepkisi, 17 kontrol, dört görsel durum ve yerel gönderim sınırı.
- [README](../README.md): projeyi açma, kontroller ve kısa özellik özeti.
- [Modlar ve Boğaz Geçişi](MODLAR_VE_BOGAZ_GECISI.md): uygulanan menü, mod yaşam döngüsü, taslak coğrafya ve yeni testler.

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

