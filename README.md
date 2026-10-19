# Meridian

TCG BARBAROS (F-244) odaklı mobil gemi manevra simülasyonu.

Projenin yapılan işleri, test kayıtları, bilinen eksikleri ve Çanakkale Boğazı araştırması için ana belge: [Geliştirme Kaydı ve Güncel Durum](docs/ILK_MVP_TASARIMI.md). Son güncelleme: **19 Ekim 2026**.

Hareket/görsel prototipi Unity Editor'de çalışır durumdadır. Foto-gerçekçilik, gerçek cihaz performansı ve yayın hazırlığı tamamlanmış değildir. Çanakkale Boğazı geçişi araştırılmıştır; coğrafya henüz oyuna uygulanmamıştır.

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
- Makine yüküne bağlı çift baca dumanı; ani gaz açmada kısa süreli koyu ve yoğun egzoz darbesi, sabit seyirde daha hafif duman
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

## Belgeler

- [Ana geliştirme kaydı, ilk MVP ve Çanakkale planı](docs/ILK_MVP_TASARIMI.md)
- [Barbaros dış modeli, dönem ve görsel inceleme](docs/BARBAROS_GORSEL_INCELEME.md)
- [S-70B, güverte ekipleri ve kırlangıç vardiyası](docs/GUVERTE_YASAMI.md)
- [PC/mobil zoom ve giriş testleri](docs/KAMERA_ZOOM.md)
- [Makine yüküne bağlı baca dumanı ve testler](docs/BACA_DUMANI.md)

## Sürüm durumu — 19 Ekim 2026

- Git uzak deposu: `https://github.com/malibu61/meridian.git`.
- Yerel `HEAD` ve son bilinen `origin/main`: `376beb7605ce68943044fbcfe2797bb222367981`. Bu kayıt gemi dış modeli, helikopter/personel ve zoom geliştirmelerini içerir. Bu belge güncellemesinde uzak sunucu yeniden sorgulanmadı.
- Baca dumanı değişiklikleri ve bu belge güncellemesi yereldedir; bu işlemde commit/push yapılmadı.
