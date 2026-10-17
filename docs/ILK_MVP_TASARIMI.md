# Meridian — İlk MVP Tasarımı

> Durum: İlk teknik prototip onaylandı ve geliştirme başladı.

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

