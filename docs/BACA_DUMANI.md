# Makine yüküne bağlı baca dumanı

- İki bacada ayrı, dünya koordinatlarında çalışan parçacık sistemleri vardır. Çıkışlar baca gruplarına bağlı olduğundan geminin yalpa ve hareketini takip eder; çıkan bulutlar sonradan gemiye yapışmaz.
- Hafif rölanti dumanı, gerçeklenen makine yüküyle artan normal seyir dumanı ve ani gaz açmaya özgü geçici yoğun/koyu duman ayrıdır.
- Yavaş gaz açmada hız eşiği aşılmadığı için siyah darbe oluşmaz. Ani yüksek yol emrinde yoğunluk, koyuluk ve çıkış hızı artar; koni daralır ve kısa bir ek parçacık darbesi oluşur. Etki yaklaşık 2,4 saniyede azalır; gaz sabit tutuldukça tekrar tetiklenmez.
- İleri/tornistan için mutlak yük kullanılır; sert yön değiştirme ek yük darbesi oluşturabilir. Gaz kısmak yeni darbe tetiklemez. Makine yükü sıfıra inerken duman hafif rölanti seviyesine döner.
- `ShipController` yalnızca gerçeklenen iskele/sancak yükünü salt okunur olarak açar; itki ve dümen fiziği değiştirilmez. `EngineExhaustResponse` görsel davranışı hesaplar, `EngineExhaustSmoke` parçacıklara uygular.
- Her bacada en fazla 240 parçacık vardır. Ortak 64×64 prosedürel bulut dokusu ve `Resources/FunnelSmoke.shader` kullanılır; dışarıdan fotoğraf/kaplama indirilmedi. Duman kendi gölgesini üretmez. Bu bir görsel oyun yaklaşımıdır; TCG Barbaros'un gerçek makine-egzoz tesisatı veya emisyon karakteristiğinin doğrulanmış simülasyonu değildir.

## Test

`Meridian > Engine Exhaust - Check Response` 17 otomatik kontrol çalıştırır: yumuşak gaz, ani tam yol, fazla/koyu/hızlı duman, bağımsız makineler, tekrar tetiklememe, normal seviyeye dönüş, gaz kesme, rölanti, tornistan, sert yön değiştirme, ilk bağlama, duraklatma ve geçersiz/sınırsız girdiler.

`Meridian > Engine Exhaust - Capture Study`, canlı kullanıcı gemisini kumanda etmeden ayrı bir geçici gemi üzerinde dört durumu karşılaştırır: `01-idle`, `02-cruise`, `03-sudden-full`, `04-settled-full`. Çıktılar `Temp/EngineExhaustReview` içinde tutulur ve Git'e girmez. `request.txt` içindeki yeni bir kimlik Play modunda aynı incelemeyi tetikler.

Mobil cihaz performans testi henüz yapılmamıştır.

## Son kontrol sonucu

- `ENGINE_EXHAUST_CHECK_PASS checks=17`: tepki testleri geçti.
- Karşılaştırmada baca başına rölanti 0,80, yarım yol 7,80, ani tam yola geçişten 0,7 saniye sonra 51,13, sabit tam yolda 14,80 parçacık/saniye üretim hesaplandı. Ani yük etkisi sabit tam yolda sıfıra döndü.
- Görsel simülasyonun devamlı çalışması kontrol edildi: ani tam yol görüntüsünde 124, sabit tam yol görüntüsünde 107 canlı parçacık vardı. Bunlar iki bacanın toplamıdır; normal ve ani dumanın opaklığı/koyuluğu ayrıca farklıdır.
- Gerçek Play-mode kumandasında iskele 0, sancak +1 iken iskele 0,80, sancak 14,80 üretim değeri ve 52 canlı parçacık gözlendi. Böylece canlı makine bağlantısı ve iki bacanın bağımsız davranışı doğrulandı.
- Normal seyir dumanının kamera mesafesinde seçilmesi için opaklık ayarı artırıldı; ani yük darbesi belirgin biçimde daha yoğun kalır. Dört görünüş son ayarla yeniden render edilip incelendi; testteki geçici gemi deniz üzerinde, kullanıcı gemisinden ayrı konumlandırıldı.
- Son duman değişiklikleri yereldedir; bu istek kapsamında yeni commit/push yapılmadı.
