# Dalga çarpması ve düzeltmeler

21–22 Ekim 2026. Aktif proje: `C:/Users/Muhammet Ali/source/repos/malibu61/meridian`.

## Değişiklikler

- `HullWaveImpacts`: her bordada dört su hattı örneği. Dalga yüksekliği, suyun gövdeye göre yükselme hızı ve bordaya yönelen orbital akış darbe şiddetini belirler. İki yeniden kullanılan parçacık sistemi damlacık/sis üretir; yüzey köpüğü mevcut `WakeTrail` sistemine aktarılır. Sıçrama su tepesinin üstünden ve borda genişlemesinin dışından başlar; opak suyun/gövdenin içine gömülmez.
- Damlacıklar yerçekimiyle düşer, zamanla saydamlaşır. Sıçrama için sürekli sabit emisyon yoktur. Menüde simülasyonla durur, yeni seyirde temizlenir. İki sıçrama yayıcısının toplam kapasitesi 448, önceki üç yüzey köpüğü yayıcısının kapasitesi 800'dür; iki baca yayıcısı ayrı korunur.
- `ShipBuoyancy`: dört kuvvet noktasında aynı `WaterWaves` alanının yatay orbital hızı örneklenir. Suya gömülme ve seviye ile ölçeklenen yatay kuvvet, `AddForceAtPosition` üzerinden sürüklenme ve dönüş momenti oluşturur. Baş değeri doğrudan yazılarak sahte rota sapması üretilmez. Makine/dümen ve mevcut hull direnci korunur.
- Dalga alanına ikinci harmonik eklenerek tepeler belirginleştirildi. Beş seviyenin model-birimi genlikleri `0.08 / 0.22 / 0.48 / 0.85 / 1.25`, ana dalga boyları `26 / 30 / 32 / 34 / 36` oldu. 1 model birimi yaklaşık 4 metredir; bunlar gerçek deniz sınıfı değildir.
- Su geometrisi gemi çevresinde sıklaştırıldı. Limanda 5.329, boğazda 9.409 köşe sınırı korunur. Normal haritasının küçük kabarcık etkisi azaltıldı; gerçek dalga yüksekliğiyle ilişkili tepe/çukur renk farkı ve kırılan tepe köpüğü güçlendirildi.

## Hataların nedeni

Seçili pusula düğmeleri, normal düğmenin ortalı/büyük yazı stilinden farklı bir kutu stili kullanıyordu. Artık aynı font, boyut, kalınlık, hizalama, içerik ofseti ve padding kullanılıyor; seçili durum yalnız yüzey rengini değiştiriyor.

Asıl düşük geometri yüksekliği hatası `Mathf.SmoothStep(.76f,1,edge)` kullanımıydı: HLSL `smoothstep` eşikleri gibi düşünülmüş ancak C# fonksiyonu iki uç değer arasında interpolasyon yapar. Böylece merkezde bile dalga genliği en fazla %24 kalıyordu; fizikse tam dalga alanını kullanıyordu. Artık kenar uzaklığı önce `InverseLerp` ile normalize ediliyor; merkez tam yüksekliği kullanıyor, yalnız dış kuşak sönüyor. Test gerçek mesh köşelerinin yükseklik aralığını da ölçüyor.

Boğazdaki uzak su düzlemi de yakın geometrik su yamasıyla aynı ortalama seviyede üst üste geliyordu. Dalga çukurlarını düz yüzey dolduruyordu. Uzak su shader'ı yakın yama içinde çizim yapmıyor; kenardaki geometrik dalga sönümü iki yüzeyi birleştiriyor. Aynı yaklaşım limanın uzak su devamına da uygulandı.

Ekranda güneş benzeri leke için iki olası üretici giderildi: suyun geniş/güçlü speküler parlaması küçük ve sınırlı glintlere düşürüldü, skybox'ın yapay güneş diski kapatıldı. Işıklandırma korunur. Kullanıcının tarif ettiği olayın aynı karesi elde olmadığı için tek bir kesin kök neden iddia edilmiyor; çok açılı oyun render'ları ayrıca incelendi.

## Doğrulama

Unity Editor'de `Meridian > Waves - Run Impact Review`: 5 seviye × 8 yönde ortak alanın analitik normali ve düşey orbital hızı sayısal türevle karşılaştırılır. Gerçek GPU render testi uzak suyun yakın yama içini kapatmadığını, dışında görünmeye devam ettiğini doğrular. Seçili/normal menü stil ölçüleri, skybox güneş diski ve shader derlemesi kontrol edilir.

Çalışan sahnede Barbaros için dingin dalga, sert dalgada yatay kuvvet kapalı kontrolü, sert dalgada kuvvet açık test; Anadolu için sert dalga/boğaz testi yapılır. Her senaryoda gaz sıfırdır; gerçek gövde başı/konumu, kuvvet ve dönüş momenti, borda darbe sayıları ölçülür. Yeni seyir sıfırlaması ve duraklatma da kontrol edilir.

Son dalga test turu: `WAVE_IMPACT_LOGIC_PASS checks=963`, `WAVE_IMPACT_PASS checks=39`. Geometrinin ölçümü eklenmeden önce görülen düz su sorunu bu yeni regresyonla yakalandı; kenar sönümü düzeltildikten sonra geçti. Test kayıtları `Temp/WaveImpactReview/logic-result.txt` ve `result.txt`, ayrıntılar `Logs/play-session-restart.log` içindedir.

13 saniyelik, gaz sıfır test düzeni (farklı fazlardaki kısa oyun ölçümleri, kalibre gerçek gemi verisi değil):

| Koşul | En büyük baş farkı | En büyük yatay yer değiştirme | Borda darbesi |
|---|---:|---:|---:|
| Barbaros, dingin | 0,022° | 0,0026 model birimi | 0 |
| Barbaros, sert, yatay kuvvet kapalı kontrolü | 1,453° | 0,0151 | 40 |
| Barbaros, sert, yatay kuvvet açık | 3,421° | 0,2133 | 36 |
| Anadolu, sert, boğaz | 0,278° | 0,5849 | 39 |

Görünen mesh tepe–çukur farkı dinginde `0,146`, sert dalgada `2,287–2,289` model birimidir. Shader'a aktarılan genlik ve yön/şiddet değerleri de test kaydına yazılır. Yatay kuvvet açıkken gerçek gövdede sıfırdan farklı yatay kuvvet ve Y ekseni dönüş momenti ölçüldü. Kontrol testindeki küçük baş değişimi dikey kuvvetler ve tam serbest gövde dinamiğinden gelebilir; yatay kuvvetin etkisi ayrı aç/kapat testiyle karşılaştırıldı.

Seçili/normal yazı ölçüleri, gerçek GPU uzak-su maskesi, shader derlemesi, skybox diskinin kapalı olması, tekrar Configure'da yedi yayıcı/tek darbe bileşeni korunması, sıfırlamada tüm parçacıkların temizlenmesi ve duraklamada gövdenin sabit kalması geçti.

Önceki su/köpük regresyonu iki gemi × liman/boğaz kombinasyonuyla yeniden çalıştırıldı: `WATER_LOGIC_CHECK_PASS checks=1019`, `WATER_RUNTIME_CHECK_PASS checks=108`. İleri gaz altında gemi hareketi, pervane köpüğünün gerçekten üretilmesi, bütçe/yayıcı sayısı, duraklatmada fizik ve parçacık sayısının donması, yeni seyirde temizlenmesi kontrol edildi.

Takip isteğinde üst bara anlık YALPA açısı ve İSKELE/SANCAK yönü eklendi. Genişletilmiş kumanda turu da `HELM_LOGIC_CHECK_PASS checks=529`, `HELM_RUNTIME_CHECK_PASS checks=22` ile geçti. Ayrıntılar: [Yalpa telemetrisi ve kumandalar](KUMANDA_VE_ARAYUZ.md).

Unity render kontrolü: [Barbaros / sert dalga](dalga-barbaros-borda.png), [Anadolu / sert dalga](dalga-anadolu-borda.png). Yan/çeyrek/diğer borda açılarında geometrik tepeler ve çukurlar incelendi; bu render'larda ekran ortasında güneş diski gözlenmedi. Sıçrama kısa ömürlü ve dalga fazına bağlıdır; bir sabit kare tüm animasyonu göstermez.

## Sınırlar

Bu fizik oyun için ayarlanmış yaklaşım; CFD, doğrulanmış gemi RAO/GM veya seyir/stabilite aracı değildir. Dalga kaynaklı salınımlı sürüklenme vardır; gerçek akıntı, rüzgâr, kırılan dalga hacmi, güverteye su alma ve hasar modeli yoktur. Windows DX11 Editor kontrolü iPhone native/Metal performansı ve Unity Remote görüntü kalitesi garantisi değildir. Önceden bulunan Anadolu gövde uç kapağı geometri kontrol hatası bu kapsamda giderilmiş sayılmaz. Bu iş kapsamında commit/push yapılmadı.
