# Meridian

TCG BARBAROS (F-244) odaklı mobil gemi manevra simülasyonu.

Projenin tamamlanan özellikleri, test durumu ve geliştirme yol haritası için [İlk MVP Tasarımı ve Güncel Durum](docs/ILK_MVP_TASARIMI.md) belgesine bakın.

Son gemi dış modeli, fotoğraf referansları ve sekiz açılık görsel kontrol yöntemi: [Barbaros görsel incelemesi](docs/BARBAROS_GORSEL_INCELEME.md).

## İlk prototip

- Kamuya açık dış görünüş referanslarına göre oranlanmış TCG BARBAROS / MEKO 200 TN Track II-A silüeti
- Bağımsız iskele ve sancak makineleri
- İleri yol, stop ve tornistan
- Diferansiyel itkiyle dönüş
- Dümen ve takip kamerası
- Geçici mobil dokunmatik kumandalar
- Canlı sürat, baş, rota ve makine durumu göstergeleri
- F-244 borda numarası, 127 mm baş topu, VLS, mast/radar, çift baca, hangar, helikopter güvertesi ve Türk bayrağı
- Uçuş güvertesinde park edilmiş S-70B Seahawk; katlı rotor, tekerlekler, takozlar ve bağlama zincirleri
- Başta sekiz, kıçta sekiz personel: her uçta altı halatçı ve iki hareketli gözcü
- İskelede sekiz, sancakta iki kişilik köprüüstü/kırlangıç vardiyası; kapılardan gerçek yürüyüş geçişi ve dürbünlü gözetleme
- Geometrik dalgalar, hareketli deniz dokusu, baş köpüğü ve çift pervane kıç suyu
- İskele, mendirek, deniz feneri, vinçler ve liman yapıları
- Mobil ekranlara uyumlu yenilenmiş kumanda panelleri

## Editörde çalıştırma

1. Projeyi Unity Hub üzerinden aç.
2. `Assets/Scenes/Main.unity` sahnesini aç.
3. Play düğmesine bas.

Kamera: boş deniz alanında fareyle veya mobilde tek parmakla sürükleyerek geminin çevresinde dön. PC'de gemi/deniz alanının üzerinde fare tekerleğini ileri çevirerek yakınlaş, geri çevirerek uzaklaş; `+ / -` tuşları da zoom yapar. `R` başlangıç kadrajına döndürür. Mobilde iki parmağını açarak yakınlaş, birleştirerek uzaklaş. İki parmağın da kumanda panelleri dışındaki alanda başlaması gerekir. Yakınlaşma sınırı 17 yerine 6 model birimine indirildi; uzaklaşma sınırı 60 birimdir.

Klavye kontrolleri: `W/S` iki makine, `Q/A` iskele makine, `E/D` sancak makine, `←/→` dümen açısı, `↓` dümeni ortala, `Space` acil stop.

Helikopter/personel kapsamı, görsel referansları ve animasyon testi: [Güverte yaşamı](docs/GUVERTE_YASAMI.md).
