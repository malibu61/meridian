# Kamera zoom kontrolü — 19 Ekim 2026

- PC: oyun görüntüsünde gemi/deniz alanının üzerinde fare tekerleği ileri = yakınlaşma, geri = uzaklaşma. `+ / -` ve sayısal klavye `+ / -` aynı işi yapar; `R` başlangıç kadrajına döndürür.
- Mobil: iki parmağı açma = yakınlaşma, birleştirme = uzaklaşma. Tek parmakla yörünge hareketi korunur.
- Kamera mesafesi 6–60 model birimidir; başlangıç 34'tür. Önceki en yakın mesafe 17 idi. Geçişler yumuşatılır.
- Pinch, piksel farkı yerine iki parmak aralığının oranıyla hesaplanır. Farklı çözünürlüklerde aynı oransal hareket aynı zoom'u üretir.
- İki parmağın da kumanda panelleri dışında başlaması gerekir; kumanda üzerinde başlayan aynı iki-parmak hareketi, sonradan denize taşınsa bile kamera kontrolünü alamaz. Parmak yer değiştirmesi/sırası, ekleme-kaldırma, iptal ve uygulama odak kaybı ele alınır. Tek parmağa dönüşte yörünge hareketi yeniden baz alınır.
- `OrbitZoomInput` giriş durumunu kameradan ayırır. `Meridian > Camera Zoom - Check Input` 23 otomatik kontrol çalıştırır; editör derlemesinden sonra da çalışır.

## Doğrulama

- Unity 6000.6.6f1: `Tundra build success (2.51 seconds)`; `CAMERA_ZOOM_CHECK_PASS checks=23 range=6..60`.
- Gerçek fare tekerleğiyle Play modunda kamera mesafesi 34,000 → 30,155 → 34,000 olarak ölçüldü; yakın/uzak görüntüler render edilerek kaydedildi.
- Sayısal klavye artı tuşuyla 34,000 → 33,032 mesafe değişimi gözlendi. `R` sıfırlama ayrıca Play modunda denendi.
- Test çıktıları `Temp/CameraZoomReview` içindedir; Git'e alınmaz. `request.txt` içindeki yeni bir kimlik, mevcut oyun kamerasını değiştirmeden konumunu ölçüp render alır.
- Mobil pinch mantığı sentetik iki-parmak girdileriyle test edildi; fiziksel Android/iOS cihaz testi yapıldığı iddia edilmez.
