# Güverte yaşamı — 19 Ekim 2026

## Eklenenler

- `BarbarosModelBuilder` gemiyi oluşturduktan sonra `NavalDeckLife` ile toplam 26 personelin etkinliğini kurar: başta 8, kıçta 8; iskele köprüüstü/kırlangıç ekibinde 8, sancakta 2.
- Personel yaklaşık 1,75–1,80 metre ölçeğindedir. Lacivert iş kıyafeti, siyah bot, turuncu can yeleği, reflektif parçalar, eldiven ve mavi/kahverengi koruyucu başlık kullanır.
- Omuz, dirsek, el, kalça ve diz konumları 3,6 saniyelik çekme/geri uzanma döngüsüyle hareket eder. Ayaklar güvertede kalır; eller aynı halat hattını takip eder. Ekipler küçük faz farklarıyla çalışır.
- Baş ve kıçta güverte içi eğitim hattı, baba ve yedek halat istifi bulunur. Bu dekoratif bir çalışma döngüsüdür; gemiyi kıyıya bağlayan veya makine kumandalarını engelleyen bir palamar fiziği değildir. Gerçek operasyon prosedürü olarak kullanılmamalıdır.
- Uçuş güvertesinde bir S-70B Seahawk dış modeli vardır: kapalı gövde, kokpit ve kabin camları, sürgülü kapı ayrıntıları, çift motor muhafazası, burun altı radom, sensör, tekerlekli iniş takımı, kuyruk yüzeyleri, dört palli kuyruk rotoru ve geriye katlanmış dört ana rotor pali.
- Helikopter park hâlindedir; tekerlek takozları ve sabitleme zincirleri bulunur. Motor/rotor dönmesi, kalkış veya uçuş kontrolü bu sürüme dahil değildir. Gerçek bir hava aracına ait seri numarası uydurulmadı.
- Helikopterin statik parçaları sekiz malzeme grubunda birleştirilir. Personel ortak düşük poligonlu mesh ve ortak malzemeler kullanır. Animasyon, gemi-local koordinatlarda çalışarak gemi hareketini/sallanmasını takip eder.

## Kırlangıç vardiyası ve artırılan güverte ekipleri

- Baş ve kıçta iki bordaya dağılan altışar halatçı, ikişer gözcü vardır. Halatçılar mevcut çekme hareketini sürdürür. Gözcüler baş ve gövdelerini farklı zamanlamalarla çevirir, ara sıra dürbünlerini gözlerine kaldırıp indirir.
- İskele ekibi dört dış gözcü ve dört yürüyen personelden; sancak ekibi bir dış gözcü ve bir yürüyen personelden oluşur. Ekip sayıları köprüüstünün içindeki personeli de kapsar; sekiz kişinin tamamı aynı anda dar kırlangıca sıkıştırılmaz.
- Beş yürüyen personel iç bekleme noktası, kapı, kırlangıç yolu ve dış gözetleme noktası arasında gidip gelir. İskele kapısı için 10 saniyelik sıra aralığı kullanılır; dört kişi aynı kapıya aynı anda yığılmaz. Kişiler gizlenip ışınlanmaz: kapalı köprüüstü onları doğal olarak örter.
- Köprü yan yüzeylerinde yaklaşık 1,6 metre genişliğinde ve 2,16 metre yüksekliğinde açıklıklar kesildi; eşik, söve, içe açık kapı kanadı, iç yürüyüş tabanı ve koyu iç bölmeler eklendi. Dış gövde yalnızca bu kapılarda açılır.
- Köprü ekibi can yeleği yerine mevcut lacivert iş kıyafeti ve lacivert başlık kullanır. Güverte çalışma ekipleri koruyucu donanımını korur.
- Yürüyüşte adım, ayak kaldırma, diz ve ters kol salınımı bulunur. Poz tamponları yeniden kullanılır; animasyon her karede yeni el/ayak dizileri oluşturmaz.

## İncelenen fotoğraflar

- [Millî Savunma Bakanlığı, halatla çalışan güverte personeli](https://x.com/tcsavunma/status/1106878603072077824): lacivert iş kıyafeti, turuncu can yeleği, renkli baret, eldiven ve bot silüeti incelendi. [Doğrudan fotoğraf](https://pbs.twimg.com/media/D1xsLsPXQAAvrOh.jpg).
- [TCG Gelibolu üzerindeki Seahawk fotoğrafı / Anadolu Ajansı fotoğraflı haber](https://www.turkiyetoday.com/nation/turkish-navys-tcg-gelibolu-floating-fortress-prepares-for-nato-missions-3204088): burun altı radomu, dört palli rotor, iniş takımı ve kuyruk oranları incelendi.
- [Güvertede park edilmiş helikopter / TRT Haber](https://www.trthaber.com/foto-galeri/efes-2026da-tcg-anadoludan-govde-gosterisi/76915.html): kokpit, motor muhafazaları ve güverte ölçeği için yardımcı görsel; farklı hava aracı konfigürasyonu olabileceğinden S-70B tip doğrulaması yerine kullanılmadı.

Fotoğraflar oyuna kaplama olarak indirilmedi. Kıyafet ve helikopter fotoğraf referanslı, stilize prosedürel modellerdir; birebir üniforma/rütbe/arma replikası veya fotogerçekçi karakter modeli değildir.

## Test

`Meridian > Visual Review - 8 Views`, sekiz genel görünüşe ek olarak `09`–`18` yakın planlarını oluşturur. `10` ve `12` aynı kameradan farklı halat animasyonu zamanlarıdır; `14` ve `16` aynı iskele kırlangıcının iki farklı yürüyüş zamanıdır. `15` sancak, `17` ve `18` baş/kıç ekiplerinin geniş görünümleridir. Dosyalar `Temp/BarbarosVisualReview` içindedir.

- İlk görsel turda gereğinden uzun kol uzanması fark edildi; hareket mesafesi ve el yerleşimi düzeltildi.
- Kıç yakın planı korkuluk arkasından değil, güverte içinden alınacak şekilde düzeltildi.
- `DECK_LIFE_CHECK_PASS`: 26 personel, örneklenen iki poz arasında yaklaşık 0,095 model birimi el hareketi; halat hattından sapma 0,006 birim toleransının altında aranır.
- `BRIDGE_WATCH_CHECK_PASS`: baş/kıç 8+8, iskele/sancak 8+2 ve dört güverte gözcüsü kontrol edilir. Beş yürüyüş rotasının tüm döngüsü 0,125 saniye aralıkla örneklenerek içeri/dışarı ulaşma, kapı açıklığından geçiş, kırlangıç sınırı ve taban yüksekliği denetlenir. Yürüme ve baş çevirme pozları ayrıca karşılaştırılır.
- `DECK_LIFE_RUNTIME_PASS`: gerçek Play-mode `Update` çağrıları boyunca en az 20 kare, 0,04 birim el hareketi, 0,15 birim yürüyüş hareketi ve 5 derece gözcü baş dönüşü aranır; sadece elle örneklenmiş poz testiyle yetinilmez.
- İlk helikopter geçişinde ön camların gövde tarafından örtülmesi yüzeye uyan cam geometrisiyle giderildi. Artırılan ekip geçişinin son doğrulama kaydı aşağıya eklenir.
- Bu masaüstü Unity Editor görsel/animasyon kontrolüdür; Android cihaz performansı ve yayın kabulü henüz doğrulanmamıştır.

### 26 kişilik düzenin son kontrolü

- C# derlemesi `Tundra build success (1.14 seconds)` ile tamamlandı.
- `BRIDGE_WATCH_CHECK_PASS`: 5 rota; baş 8, kıç 8, iskele 8, sancak 2, güverte gözcüsü 4. Örneklenen yürüyüş mesafesi 0,770 birim, baş dönüşü 49,2 derece.
- `DECK_LIFE_CHECK_PASS`: 26 personel, el hareketi 0,095 birim, halat sapması 0,0000 birim.
- `DECK_LIFE_RUNTIME_PASS`: 98 gerçek güncelleme karesi; el hareketi 0,106 birim, yürüyüş hareketi 0,708 birim, gözcü baş dönüşü 37,8 derece.
- Sekiz genel görünüş ve on yakın plan incelendi. Başın üstten görünüşünde sekiz personelin tamamı, kıçta helikopterin yanlarında sekiz personel; iki kırlangıçta kapı ve farklı yürüyüş zamanları kontrol edildi. Yakın planda görülen boyun birleşimi boşluğu kapatıldı.
- Son test günlüğünde C# / shader hatası veya çalışma zamanı istisnası görülmedi.
