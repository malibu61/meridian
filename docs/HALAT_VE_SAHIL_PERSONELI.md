# Meridian — Sahil personeli ve halat manevrası

22 Ekim 2026. Aktif proje `meridian`; değişiklikler yereldir, bu çalışma için commit/push yapılmadı.

Güncel filo genişlemesinde halat zorunluluğu dört geminin toplam 20 görevine uygulanır. BARBAROS/ANADOLU römorkör çalışması veya RÜZGAR iç/dış makine koşulu tamamlanmadan halat aşamasına geçilemez. [Filo görevleri](FILO_GOREVLERI.md).

## Oyuncu akışı

Liman modunda dört gemi de bu sistemi kullanır. Görevde atanmış rıhtım, ANADOLU'da seçilen rıhtım; diğer serbest seyirlerde yakın rıhtım kullanılır. Açık halat varken otomatik olarak karşı rıhtıma atlanmaz.

1. Rıhtıma paralel, düşük süratle yaklaş. Yakın mesafede HALAT MANEVRASI paneli açılır.
2. Bütün makineleri STOP'a getir; gerçek makine yükünün düşmesini bekle. Sürat <0,60 kn, yanal <0,35 kn, baş 000° ±12°, bağlantı noktası mesafesi en fazla 24 m olmalıdır. Bunlar oyun eşikleridir, gerçek operasyon limitleri değildir.
3. BAŞ HALAT AL veya KIÇ HALAT AL seç. Sahil ekibi önce hafif el incesini atar (1,4 s), gemi ekibi karşılar ve ana halatı alır (2 s).
4. Ekip ve basitleştirilmiş ırgat boşunu alır (14 s). Halat kısaldıkça gemiye kuvvet uygulanır. Eller çalışma ucuna bağlıdır; kalın ana hat baba ile gemi bitası arasındadır.
5. İlk uç bağlanınca diğer ucu al. İki halat gemiyi birbirinden ayrı baş/kıç noktalarından çekip yönünü toparlar.
6. İki uç bağlı, konum/baş/sürat uygunken 3 s sakinleşme ile halat manevrası tamamlanır. KUMKALE görevleri ayrıca mevcut konum/baş/sürat ve düdük koşullarını 5 s korur; sonraki görev ancak başarılı sonuçla açılır.

FORA iki halatı bırakır, bekleme ilerlemesini temizler. Menü açıkken kuvvet ve aşamalar ilerlemez. Makinelere komut verilir veya yük >%8 kalırsa çekme bekler. Bağlantı noktası aralığı 36 m'yi veya sürat 2,5 kn'yi aşarsa ekip bağlantıyı bırakır; tekrar yaklaşılabilir. Bunlar oyunun koruma davranışlarıdır, gerçek halat kopma hesabı değildir.

## Görsel yaşam

- Doğu ve batı rıhtımlarında sekizer, toplam 16 yürüyen personel. Rotalar rıhtım kapağının üzerinde, halat çalışma şeridinin gerisindedir. Durma/bakınma ve karşı yöne dönme döngüleri vardır.
- Beyaz hizmet kıyafeti, siyah ayakkabı/siperlik ve basitleştirilmiş altın omuz detayları. Subay ve astsubay temsili görsel esinlenmedir; gerçek birlik/rütbe/üniforma sertifikasyonu değildir. Bütün astsubayların mavi giydiği varsayılmadı.
- İki baş + iki kıç sahil halatçısı, gemide de ikişer karşılayıcı/çekici (8 aktif ekip personeli). İş mavisi kıyafet, görünür turuncu koruyucu donanım ve el hareketleri.
- Geminin boy/en/güverte yüksekliği profiline göre baş/kıç bağlantı noktaları; ANADOLU güvertesi ve küçük gemiler aynı yükseklikte değildir. Mevcut dekoratif güverte personeli korunur.
- Bir karakter: 576 köşeli dinamik düşük poligon mesh, beş ortak malzeme; ek collider/NavMesh yok. Mobil performans henüz profillenmedi.
- İnce atış hattı ve yumuşak ağırlık yay çizer; ana halat alınırken görünür şekilde ilerler, boşlukta sarkar, gerilince doğrulur. Baba, gemi bitası ve sahilde istiflenmiş çalışma halatı vardır.

## Fizik ve kapsam

`MooringRules.PullAcceleration`: çekmeye çalışan tek taraflı yay, radyal sönüm, kütleye ölçeklenen sınırlı kuvvet. Gevşek halat itmez. İki kuvvet Rigidbody.AddForceAtPosition ile CG yüksekliğinde ayrı boyuna noktalardan uygulanır. Oyun gemisinin transform/konumuna hareket için yazılmaz; yalpa kuvveti iki kez sayılmaz. Esas model yatay düzlemdedir; halatın düşey bileşeni, gerçek ırgat torku ve mukavemet/snap-back hesapları yoktur.

24 Ekim güncellemesi: BARBAROS/ANADOLU römorkörleri halat alınırken ve ilk uç bağlıyken kullanılabilir. Halat kartına BAŞ/KIÇ römorkör kumandaları ve İKİSİ DUR eklendi. Gerçek temas/sürat/kıyı güvenliği koşulları korunur; iki uçla yanaşma tamamlanınca itme kesilir. Eski sadece konum/sürat ile “yanaştı” kararı yerine iki uç tamamlanması izlenir. Bu, römorkörle yedekleme halatı değildir. Boğaz modunda ekip/halat servisi devre dışıdır, şamandıra modu hâlâ yer tutucudur.

## Doğrulama

- `MooringReview.CheckRules`: 66 kontrol. İki rıhtım/dört gemi geometri ve atış sınırları, kuvvet yönü/sınırı, gevşek hat, halatsız görevin geçmemesi, dört makineli dahil panel çakışmaları.
- `MooringReview.RunBatch`: 108 kontrol, 8 fizik çevrimi. Üretim ShipController ve MooringController kuvvet yolları izole Unity PhysicsScene içinde adım adım çalıştırılır; her adımda gemi konumu örneklenir, konuma elle taşıma yapılmaz. Başlangıç yerleştirmesi test kurulumudur.
- Dört gemi × iki rıhtım: baş atılır, ilk uç bağlanınca kıç alınır, iki hat çekerek gemiyi yaklaşık 5,24–5,72 m ilerletir; tamamlanma yaklaşık 57–59 oyun saniyesidir. Ayrıntı `testing/20261022-mooring/physics-detail.txt`.
- STOP kilidi, tek seferde bir uç, menüde kuvvet/aşama durması, FORA, 16 kişinin hareketi/mesh/ayak yüksekliği, mod/gemi değişiminde temizlik test edildi.
- 84 kural + 85 görev/kariyer/ses regresyon kontrolü yeniden geçti. Bu durum makinesi testleri halat tamamlanma girdisini kontrollü verir; gerçek çekme ayrı 8 fizik testinde doğrulanır. Oyuncunun kariyer kaydı açılıp tamamlanmaz.
- Ana Unity Editor'de KUMKALE serbest liman denemesinde ekrandan önce baş, sonra kıç halatı alındı; iki uç BAĞLI ve YANAŞMA TAMAMLANDI bildirimi, yaklaşık 001° baş ve 0,1 kn süratle görüldü. Üstteki halat kartı görev/römorkör kartının yerine geçer; geminin gövdesini kapatan ek orta panel kaldırıldı. Bu manuel deneme bir KUMKALE kariyer görevi tamamlaması değildir.
- Fizik testleri sakin denizde, dikey/yalpa eksenleri kilitli izole sahnede yapılır. Ağır denizde beş görevin tamamının insan tarafından bitirilmesi, gerçek iPhone performansı ve çoklu dokunma bu testlerin kapsamı değildir.
- Mevcut ANADOLU gövde kenar-yönü uyarısı, su–gövde kesişmeleri ve nihai karakter kalitesi bu geliştirmeyle çözülmüş sayılmaz.

Referans sınırı: kamuya açık [MSB deniz personeli/liman haberleri](https://www.msb.gov.tr/SlaytHaber/9c27101cfbac4b9cb99d6e62ee40d0ce), [Sahil Güvenlik kıyafet metni](https://www.sg.gov.tr/kurumlar/sg.gov.tr/mevzuatlar/yonetmelikler/KIYAFET-YONETMELIK.pdf) ve [MEB rıhtım/şamandıra modülü](https://megep.meb.gov.tr/mte_program_modul/moduller_pdf/R%C4%B1ht%C4%B1m%20Ve%20%C5%9Eamand%C4%B1ra%20Manevras%C4%B1%20Yapma.pdf) araştırmada bulundu. MEB PDF'i bu oturumda tam indirilemedi; ayrıntılı manevra tarifi veya kesin askerî üniforma doğruluğu kaynağı olarak kullanılmış değildir. Oyunun iki hatlı akışı gerçek bir liman operasyonunun tamamını temsil etmez.
