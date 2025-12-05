# Kaderin Ağları (Web of Fate) – Genişletilmiş Oyun Tasarım Dokümanı

## Giriş

Kaderin Ağları, oyuncunun "Örgücü" (The Weaver) olarak gerçekliği bir kader ağında dokuduğu benzersiz bir roguelike/deck‐building/puzzle oyunudur. Oyun, klasik kart savaşlarından farklı olarak **hikâye kurgusu** ve **mekânsal bulmaca çözümü** üzerine kurulur.  
Amacımız, oyuncuyu hem yaratıcılığa teşvik eden hem de keşif hissi uyandıran bir deneyim sunmaktır. **Synerji** tasarımı bu tür oyunlarda keşif ve duygusal tatmin sağlar; birden fazla kartı birleştirmenin toplamından daha büyük etki yaratması oyuncunun keşfetme arzusunu canlı tutar【776939076369990†L180-L199】.  
Bu doküman, mevcut konsepti inceleyerek hataları düzeltir, mekaniği geliştirir ve Godot 4.5 üzerinden 2D bir kart oyunu olarak hayata geçirmek için ayrıntılı yönergeler sunar.  

## 1. Oyun Özeti

- **İsim:** *Kaderin Ağları* (Web of Fate)  
- **Tür:** Roguelike Deckbuilder / Puzzle Strateji  
- **Platform:** PC (Windows, macOS, Linux), Mobil (iOS/Android) ve Tablet. Oyun Godot 4.5 ile geliştirilecek; framework, kartları 2D ortamda sergileyecek şekilde yapılandırılacaktır.  
- **Tema:** Karanlık fantastik/kosmik mistik atmosfer, oyuncuyu sonsuz bir kader tezgâhında kozmik bir örümcek-tanrı konumuna yerleştirir.  
- **Hedef Kitle:** 14 yaş ve üzeri strateji, kart oyunları ve hikâye odaklı oyun severler.  

## 2. Tema ve Hikâye

### 2.1 Arka Plan

Evrenin dokusu, görünmeyen bir **kader ağı** tarafından tutulur. Bu ağın ustası **Örgücü**, her bir run’da destansı bir kahramanın kaderini şekillendirir.  

- Oyuncu, Kozmik Örümcek veya Büyücü’nün avatarı olarak **Destan Kahramanı**nın hikâyesini kurgular.  
- Bu kahraman, bilinmeyen bir dünyada yolculuk eder. Olaylar ve karakterler kader ağındaki düğümlere yerleştirilerek **kader iplikleri** üzerinde bağlanır.  
- Hikâye dark fantasy öğeleri içerir; fırtına, trajedi, mistik eserler ve umuda dair parlayan anlar.  
- Her run bir destan gibidir; sonuçta oyuncu, kaosun yönetilmesiyle kader ipliğini koparmadan mümkün olduğunca dramatik bir hikâye yazmaya çalışır.  

### 2.2 Tematik Motifler

- **Kozmik Mistik:** Lovecraft‐vari bilinmez güçler, yıldızların ötesinden gelen varlıklar, zaman döngüleri.  
- **Karanlık Orta Çağ Fantastiği:** Kale, krallık, soylular, ejderha ve efsanevi eşyalar.  
- **Dualite:** Umut ve karamsarlık, kaos ve düzen, trajedi ve kahramanlık.  

## 3. Temel Mekanikler

### 3.1 Kader Ağı (The Loom) ve Düğüm Haritası

1. **Prosedürel Düğüm Haritası:** Oyuncunun önü, her tur rastgele üretilen bir *düğüm haritası* ile dolar. Harita; düğümler (slot’lar) ve **bağlantılar** (iplikler) içerir.  
2. **Slot Sayısı:** Standart başlangıçta 5 düğüm bulunur; metaprogression ile bu sayı artırılabilir.  
3. **Bağlantı Türleri:**  
   - **İpek İplik (Beyaz):** Standart bağlantı, normal puanlar verir.  
   - **Kan İpliği (Kırmızı):** Buraya **Vahşet (Violence)** veya **Trajedi** kartları bağlanırsa ek DP (Destan Puanı) ve Kaos kazanılır.  
   - **Altın İplik (Sarı):** Bu bağlantıya bağlanan kart Kaos üretmez.  
   - **Gölge İplik (Mor – metaprogression ile açılır):** Kart etkilerini kopyalar veya tersine çevirir; risk/ödül mekaniği için kullanılır.  
4. **Bağlantı Kuralları:** Kartlar yalnızca bağlı olduğu düğümlere etki eder. Bağlantı tipi, kart etiketleriyle etkileşerek bonus/ceza verir.  
5. **Kırılma ve Kopma:** Kaos barı doldurulursa iplikler kopar ve run sonlanır.  

### 3.2 Kartlar ve Etiketler

Kartlar **Destan**ı oluşturan yapı taşlarıdır. Dört ana **etiket/tag** vardır:

| Etiket | Renk/Kod | Tanım | Etki Eğilimi |
|-------|---------|------|--------------|
| **Vahşet (Violence)** | 🔴 | Dövüş, kan, çatışma, ölümcül risk. | Yüksek DP, yüksek Kaos |
| **Mistik (Mystic)** | 🔵 | Büyü, lanetler, kehanetler, gizem. | Sinerji odaklı, Kaos etkilerini manipüle eder |
| **Umut (Hope)** | 🟢 | İyileştirme, yardım, barış. | Kaos’u düşürür, düşük DP |
| **Trajedi (Tragedy)** | 🟣 | İhanet, kayıp, dram. | Çok yüksek DP, yüksek Kaos risk |

Kartlar **Karakterler**, **Eşyalar**, **Olaylar**, **Lokasyonlar** ve **Felaketler** olarak beş kategoriye ayrılır. Her kartın temel etkisi ve sinerji tetikleyen özel bir kombosu vardır (Bkz. Kart Listesi Bölümü).  

### 3.3 Kaynaklar

| Kaynak | Açıklama |
|-------|---------|
| **Destan Puanı (DP / Legacy)** | Skor ve para birimidir. Kart oynamak, sinerji oluşturmak ve run sonu ödülleriyle kazanılır. DP, yeni kartlar, iplik türleri ve meta yükseltmeler satın almak için kullanılır. |
| **Kaos (KP / Chaos)** | 0–100 arası çubuktur. Vahşet/Trajedi etkileri ve bazı Mistik kartlar Kaos üretir, Umut kartları ve özel sinerjiler Kaos’u azaltır. 100’ü aşarsa “Kırılma” olur ve run hemen biter. Amaç, Kaos’u kritik seviyede (örneğin 90) tutup maksimal DP elde etmektir. |
| **İplik (Thread)** | Bağlantı türlerinin seviyesi. Metaprogression ile yeni iplik çeşitleri açılır. |
| **Meta Kredi (MK)** | Run sonu meta‐progression için kullanılan kaynak. DP’nin belli bir yüzdesi MK’ye dönüştürülür. |

### 3.4 Oyun Döngüsü ve Akış (The Loop)

Oyun, **Tur (Chapter)**, **Run** ve **Meta** olmak üzere üç ana döngü üzerinde çalışır:

#### 3.4.1 Tur Döngüsü (Micro Loop)

1. **Hazırlık:** Oyuncu, desteden 5 kart çeker (metaprogression ile artabilir).  
2. **Düğüm Seçimi:** Kader ağında o turun düğüm haritası prosedürel olarak belirlenir. Bağlantılar (iplik türleri) ve boş düğümler görüntülenir.  
3. **Yerleştirme (Örgü Aşaması):** Oyuncu, elindeki kartları sırayla düğümlere yerleştirir. Mekânsal düşünmek önemlidir; kart etiketleri ve bağlantı tipleri sinerji verir.  
4. **Titreşim (Sinerji Aşaması):** Tüm kartlar açılır; bağlı olan kartlar birbiriyle etkileşir. Sinerji tespiti ve Kaos/DP hesaplaması yapılır.  
5. **Hikâye Anlatımı:** Kartların yarattığı kombinasyonlara göre mini hikâye (örn. “Prenses düğünde çok mutluydu, ancak suikastçı saldırdı — Kızıl Düğün!”). Bu, oyuna duygusal bağ ve mizah katar.  
6. **Sonuç ve Temizlik:** DP ve KP güncellenir, yok edilen kartlar mezarlığa gider, bazı kartlar desteye geri döner. Oyuncu dilerse **Bölüm Sonlandırma** düğmesini kullanarak run’ı erken bitirip DP alabilir (risk/ödül).  

#### 3.4.2 Run Döngüsü (Macro Loop)

- Her run, 8–10 bölüm sürer (metaprogression ile değişebilir).  
- Oyuncu, Kaos barı 100’e ulaşmadan tüm bölümleri tamamlamaya çalışır.  
- Bölüm sonlarında, mini boss veya destansı olaylar tetiklenir (örneğin, **Kızıl Ay** felaketi).  
- Run sonunda toplanan DP’den Meta Kredi üretilir ve **Kader Tezgâhı (Meta Shop)** üzerinden yükseltmeler satın alınır.  

#### 3.4.3 Meta Döngü (Outside Run)

Meta progression, oyuncuya her run’da keşfedilecek yeni içerikler sunarak oyunun uzun ömürlü olmasını sağlar.  

Öne çıkan meta mekaniği unsurları:

- **İlk Run Öğrenme:** Yeni kart ve iplik türleri kademeli olarak açılır; bu, oyuncunun oyunu aşamalı bir şekilde öğrenmesini sağlar. Bu yaklaşım, meta progression kullanılarak oyun içi öğeleri sırayla sunan **tutorial benzeri** bir sistemdir【856645375258190†L468-L523】.  
- **Ascension Sistemi (Yükseliş):** Oyuncu oyunu tamamladıkça (örneğin, Kaos’u yönetip destanı bitirdikçe) bir üst **Yükseliş Seviyesi** açılır. Her seviye, Kaos başlangıç değerini yükseltir, daha zorlu kartlar ekler ve DP çarpanını artırır.  
- **Kader Tezgâhı:** Oyun dışında DP/MK ile yatırımlar yapılır:  
  - Yeni kart paketleri ve kart etiketleri açmak.  
  - Başlangıç destesine özel kartlar eklemek veya çıkarmak.  
  - İplik türlerini yükseltmek (örneğin, Altın İplik’in sayısını artırmak).  
  - Ekran temasını, kozmetik eşyaları ve hikâye fragmanlarını almak.  

#### 3.4.4 Risk/Ödül ve Kaos Yönetimi

- **Kaos Barı**, oyuncuyu sürekli gergin tutar. Maksimum DP için Kaos’u yükseltmek gerekir, ancak **Kırılma** riski her zaman vardır.  
- Umut ve Mistik kartlar, Kaos’u düşürmek veya diğer kartların Kaos maliyetini sıfırlamak için kullanılır; böylece gerginlik dengelenir.  
- Oyuncu, **Uçurum Kenarı** gibi tehlikeli lokasyonlarda yüksek DP kazanmak için risk alabilir.  
- Kaos’u kasıtlı olarak 90 civarında tutarak en yüksek DP bonusunu almak stratejik bir hamledir.  

### 3.5 Sinerji ve Kombolar

Synerji, oyuncunun keşfetme duygusunu artırır ve oyunun derinliğini genişletir. **Synerji, iki veya daha fazla kartın birleştiğinde tek başına yapabileceklerinden daha büyük etki üretmesi** olarak tanımlanır【776939076369990†L195-L199】. Synerji keşfetmek oyuncuya başarı hissi verir ve oyun derinliğini artırır【776939076369990†L203-L223】.  
Aşağıdaki kart listesi sinerji odaklıdır ve oyuncunun farklı kombinasyonlar denemesini teşvik eder.

## 4. Kart Listesi ve Detaylar

### 4.1 Kart Kategorileri

Kartlar beş ana kategoridedir. Her kartın **etiketi**, **temel etkisi** ve **sinerji/kombo** özelliği vardır. Yeni kartlar, meta progression ile açılabilir. **Kırmızı** kartlar risk/ödül odaklı, **mavi** kartlar sinerji araçları, **yeşil** kartlar Kaos azaltıcıları, **mor** kartlar ise dramatik DP artışları sağlar.

#### Kategori 1: Karakterler (Aktörler)

| No | Kart Adı | Etiket | Temel Etki | Özel Sinerji / Kombo |
|---|---------|------|-----------|----------------------|
| **1** | **Acemi Kahraman** | 🟢 | Başlangıç kartıdır. +5 DP ve düşük Kaos. | **Efsanevi Kılıç** ile bağlıysa **Seçilmiş Kişi**ye dönüşür; DP +50, Kaos -10. |
| **2** | **Yasak Aşk** | 🟣 | +20 DP, +10 Kaos. | **Kıskanç Prens** ile bağlıysa “Romeo & Juliet” kombosu: Kaos x2, DP x3 (40 → 60 DP, 20 → 40 Kaos). |
| **3** | **Kanlı Baron** | 🔴 | +15 DP, +15 Kaos. | **Köylü İsyanı** ile bağlıysa Baron ölür; tüm Kaos sıfırlanır, DP bonusu +30. |
| **4** | **Gizemli Rehber** | 🔵 | +10 DP. Bağlı olduğu kartın Kaos maliyetini siler. | Yüksek Kaos üreten kartların yanına koymak için idealdir. |
| **5** | **Saray Soytarısı** | 🟣 | Rastgele etki: %50 ihtimalle Kaos’u siler, %50 ihtimalle ikiye katlar. | **Kralın Tacı** ile bağlıysa “Darbe” tetikler: Kaos barı %90’a çıkar, DP +100. |
| **6** | **Vebalı Fare** | 🔴 | +5 Kaos. | **Şehir Meydanı** ile bağlıysa “Salgın” başlatır: komşu tüm slotlara +10 Kaos yayar, DP +10. |
| **7** | **Ejderha Yavrusu** | 🔴 | +30 DP, +20 Kaos. | **Yanardağ** lokasyonuna bağlıysa **Kadim Ejderha**ya dönüşür: +100 DP, +50 Kaos. |
| **8** | **Gölge Suikastçı** | 🔴 | Bağlı olduğu kartı "öldürür" (kartın etkisi iptal olur). | İstenmeyen bir felaket veya trajedi kartını iptal etmek için kullanılır; **Lanetli Yüzük** ile kombolanırsa yüzüğü yok eder. |
| **9** | **Kıskanç Prens** (Metaprogression ile açılır) | 🟣 | +15 DP, +10 Kaos. | **Yasak Aşk** ile bağlıysa Romeo & Juliet; **Kralın Tacı** ile bağlıysa taht kavgası başlatır, DP +100, Kaos +50. |
| **10** | **Zalim Kral** | 🔴 | +25 DP, +25 Kaos. | **Zehirli Kadeh** ile bağlıysa “Taht Oyunları” bonusu: Kral ölür, DP +60, Kaos -20. |

#### Kategori 2: Eşyalar (Macguffin’ler)

| No | Kart Adı | Etiket | Temel Etki | Özel Sinerji / Kombo |
|---|---------|------|-----------|----------------------|
| **11** | **Efsanevi Kılıç** | 🔴 | +20 DP, +5 Kaos. | **Acemi Kahraman** ile bağlıysa Seçilmiş Kişi; **Kırık Kalkan** ile bağlıysa kılıç kırılır, Kaos +10; **Demirci** kartı ile bağlıysa güçlenir (DP +20, Kaos -5). |
| **12** | **Lanetli Yüzük** | 🔵 | +40 DP, her tur +5 Kaos üretir. | **Volkan** veya **Arınma Havuzu** ile bağlıysa yok edilir ve +80 DP verir. |
| **13** | **Büyükanne Kurabiyesi** | 🟢 | -20 Kaos; DP yoktur. | Gerilimli anlarda Kaos’u sıfırlamak için kullanılır; **Şehir Meydanı** ile bağlıysa +10 DP bonus verir. |
| **14** | **Harita Parçası** | 🟢 | +5 DP. | Bir diğer Harita Parçası ile bağlanırsa **Hazine Odası** etkinleştirir; gizli seviye açılır ve +50 DP. |
| **15** | **Zehirli Kadeh** | 🟣 | Bağlı olduğu karakteri öldürür; +10 DP, +10 Kaos. | **Zalim Kral** veya **Kıskanç Prens** ile bağlıysa “Taht Oyunları” kombosu (DP +60, Kaos -20). |
| **16** | **Kukla İpleri** | 🔵 | Bağlı olduğu iki kartın yerini değiştirir. | Yanlış yerleştirilmiş kartları düzeltmek veya tehlikeli kombinasyonları bozmak için kullanılır. |
| **17** | **Kırık Kalkan** | 🟣 | Savunma sembolüdür. Kartın bağlı olduğu diğer kartın DP’sini %50 azaltır, Kaos azaltır. | **Efsanevi Kılıç** ile bağlıysa kılıç kırılır; **Köylü İsyanı** ile bağlıysa isyan bastırılır (DP -10, Kaos -30). |
| **18** | **Demirci** | 🔵 | +10 DP. | **Efsanevi Kılıç** ile güçlenir; **Kırık Kalkan** onarılır. |

#### Kategori 3: Olaylar (Plot Twists)

| No | Kart Adı | Etiket | Temel Etki | Özel Sinerji / Kombo |
|---|---------|------|-----------|----------------------|
| **19** | **Kardeş İhaneti** | 🟣 | +50 DP, +40 Kaos (çok riskli). | **Aile Yadigârı** (açılır) varsa Kaos +30, DP +100. |
| **20** | **Ani Fırtına** | 🔵 | Tüm bağlı slotların etkilerini %50 azaltır (Kaos dahil). | **Kızıl Ay** felaketinde hayatta kalmak için sigorta. |
| **21** | **Kahramanca Fedakarlık** | 🟢 | Mevcut kahramanı öldürür; Kaos 0 olur; DP ikiye katlanır. | Run finalinde kullanmak ideal. |
| **22** | **Yanlış Anlaşılma** | 🟣 | Bağlı dost karakterleri düşmana çevirir; Kaos +10. | **Saray Soytarısı** ile mizahi kombinasyon. |
| **23** | **Kehanet** | 🔵 | Bir sonraki turun ağ yapısını gösterir; strateji planlaması sağlar. | **Kader Tezgâhı** yükseltmeleri ile etkileşerek ek bilgi sunar. |
| **24** | **Şafak Vakti** | 🟢 | -15 Kaos. | Vampir veya **Gece Yaratığı** kartları varsa onları yok eder; DP +20. |
| **25** | **Düğün Töreni** | 🟢 | +10 DP, Kaos -5. | **Yasak Aşk** ile Kızıl Düğün; **Gölge Suikastçı** ile kombinasyon dramatik bir trajediye dönüşür. |
| **26** | **Volkan Patlaması** | 🔴 | Tüm çevre slotları yok eder; DP +40, Kaos +30. | **Lanetli Yüzük** yok edilirse DP +80, Kaos -10. |

#### Kategori 4: Lokasyonlar (Bağlam)

| No | Kart Adı | Etiket | Temel Etki | Özel Sinerji / Kombo |
|---|---------|------|-----------|----------------------|
| **27** | **Karanlık Orman** | 🔴 | İçine konan her **Canavar** kartı +5 ekstra DP verir. | Canavar destesi oynayanlar için ideal. |
| **28** | **Yıkık Tapınak** | 🔵 | Mistik kartların Kaos bedelini yarıya indirir. | Mistik odaklı destelerde Kaos yönetimi sağlar. |
| **29** | **Han Köşesi** | 🟢 | Karakter kartları burada kavga etmez; Kaos -15. | Uzun runlarda Kaos’u düşürmek için mola noktası. |
| **30** | **Uçurum Kenarı** | 🟣 | Buraya bağlanan karakterin ölme riski %50’dir. Ölürse DP +25, Kaos -20; ölmezse Kaos +10. | Düşmanları itmek veya kahramanı kurban etmek stratejik. |
| **31** | **Pazar Yeri** | 🟢 | Eşya kartları burada x2 DP verir. | **Harita Parçası** ve **Kırık Kalkan** gibi eşyalarla DP artar. |
| **32** | **Yanardağ** | 🔴 | Buraya konan kartlar her tur +10 Kaos üretir; DP +10. | **Ejderha Yavrusu** burada büyür; **Lanetli Yüzük** yok olur. |
| **33** | **Arınma Havuzu** | 🟢 | Burada bulunan kartların Kaos değeri yarıya düşer; Yüzük ve Lanetleri yok eder. | Meta progression ile açılır. |
| **34** | **Zaman Kapısı** (Metaprogression) | 🔵 | Run’a özel bir ek tur sağlar; +20 DP. | **Zaman Paradoksu** felaketi ile etkileşerek Kaos nötralize eder. |

#### Kategori 5: Felaketler (Kaos Arttırıcılar)

Bu kartlar genellikle oyuncunun eline **zorla** gelir veya “lanet” olarak desteye girer. Sonuçları ölümcül olabilir.

| No | Kart Adı | Etiket | Temel Etki | Özel Sinerji / Kombo |
|---|---------|------|-----------|----------------------|
| **35** | **Kızıl Ay** | 🔴 | Tüm Vahşet kartlarının Kaos üretimini 2 katına çıkarır. | **Kurtadam** (metaprogression) ile bağlıysa kontrol edilemez güç yaratır; DP +150, Kaos +100. |
| **36** | **Unutkanlık** | 🔵 | Bağlı olduğu kartın etkisini siler (boş slot gibi davranır). | İyi kartı silmek risk; kötü bir felaketi de engelleyebilir. |
| **37** | **Zaman Paradoksu** | 🔵 | +50 Kaos. | **Zaman Kapısı** veya **Büyücü** kartıyla bağlıysa Kaos nötrlenir, DP +100. |
| **38** | **Kitlesel Histeri** | 🟣 | Sahadaki her karakter için +5 Kaos artar. | Kalabalık sahnelerde tehlikeli; tek karakter olduğunda avantaj sağlar. |
| **39** | **Tanrının Gazabı** | 🔴 | Masadaki her şeyi yok eder; run biter. | Sadece **Nihai Son** (Final Boss) slotuna saklanmalı; DP +300 ve nadir ödül. |
| **40** | **Kıyamet Saati** (Metaprogression) | 🔵 | Tur sayısını bir azaltır; Kaos’u %50 artırır. | Zaman sınırlı modlarda kullanılır; **Kehanet** ile kombolanırsa Kaos azalır. |

### 4.2 Sinerji Mekanikleri

- **Etiket Temelli Sinerji:** Aynı etikete sahip kartlar birbirine bağlandığında ek DP veya Kaos bonusu verir.  
- **Bağlantı Temelli Sinerji:** Kartın etiketine uygun iplik tipine yerleştirilmesiyle bonus oluşur (örneğin Kanlı Baron + Kırmızı iplik = DP +10).  
- **Mekânsal Sinerji:** Bazı lokasyonlar (Han, Uçurum) bağlı tüm slotları etkileyerek kartların etkisini değiştirir.  
- **Lentiküler Tasarım:** Sinerji karmaşık görünmemelidir; kartlar başlangıçta basit etkiler sunar, ancak deneyimli oyuncular sinerjiyi keşfederek daha derin strateji geliştirir【776939076369990†L262-L276】.  

## 5. Progression ve Meta Progression

### 5.1 Oyun İçi Progression

- **Bölüm Bazlı:** Her bölümde düğüm haritasının zorluğu artar; daha karmaşık bağlantı yapıları ve kaos üretme potansiyeli çıkar.  
- **Kart Havuzu Genişlemesi:** Oyuncu, run ilerledikçe yeni kartlar kazanır veya elindeki kartları yükseltme fırsatı elde eder. Bu, deck‐building öğesini derinleştirir.  
- **Mini Boss ve Felaketler:** İlerleyen bölümlerde kart destesine **Kızıl Ay**, **Zaman Paradoksu** gibi zorunlu felaketler girer; oyuncu bunlara hazırlıklı olmalıdır.  

### 5.2 Meta Progression (Kader Tezgâhı)

**Meta progression**, oyuncunun run’lar arasında kalıcı gelişme kaydetmesini sağlar. Bu mekanizma aynı zamanda oyuna kademeli öğretim katar; yeni kartlar ve özellikler yavaşça açılır【856645375258190†L468-L523】.

- **İplik Yükseltmeleri:** Altın ve Gölge İplik sayısını artırma, yeni iplik türleri (Mor Gölge, Mavi Kehanet) açma.  
- **Kart Paketi Açma:** Yeni kartlar, karakterler, lokasyonlar ve felaketler meta mağazadan satın alınabilir.  
- **Başlangıç Destesi Özelleştirme:** Oyuncu, favori kartlarını başlangıç destesine ekleyebilir veya gereksiz kartları çıkarabilir (deck thinning).  
- **Relic Sistemi:** Kalıcı pasif bonuslar (örneğin her run’a +10 Kaos azaltıcı, ekstra kart çekimi, gizli sinerji açma) sunan gizemli nesneler.  
- **Yükseliş (Ascension) Seviyeleri:** Run’ı bitirdikçe yeni zorluk kademeleri açılır; Kaos başlangıcı yüksek, felaket kartları daha erken gelir, ancak DP çarpanı artar.  

### 5.3 Ödül ve Ceza Mekanikleri

- **Bütünsel Yüksek DP:** Kaos’u kritik seviyede tutarak bölüm sonlandırma ve run’i bitirmeden önce *Maksimum Gerilim* ödülü almak risklidir ancak çok kârlıdır.  
- **Kaos Patlaması:** Kaos barı dolunca iplikler kopar; run sonlanır ve DP’nin yalnızca %50’si meta krediye döner.  
- **Kahramanca Fedakarlık** kartı ile run finalinde Kaos sıfırlanarak DP ikiye katlanabilir, ancak kahraman ölür ve kart desteden çıkar.  

## 6. Oyun Modları

- **Klasik Run:** 8–10 bölüm, standart kart havuzu, meta progression açık.  
- **Zaman Yarışı:** Zaman Kapısı kartı kullanılarak run süresi sınırlıdır; oyuncu hızlı karar vermek zorundadır.  
- **Günlük Meydan Okuma:** Günlük olarak özel kart düzeni ve iplik yapılarını içeren leaderboard modudur.  
- **Özel Desteler:** Oyuncu, meta progression’de açtığı kartlarla özel bir başlangıç destesi oluşturabilir ve arkadaşıyla veya AI ile karşılaşabilir (planlanabilir).  

## 7. UI/UX ve Tasarım

Godot 4.5 ve kart oyun framework’ü kullanarak 2D bir masa alanı tasarlanacaktır.

### 7.1 Kart ve Masa Tasarımı

- **Kartlar:**  
  - Kartlar **2D dikdörtgen** olarak modellenir; ön yüzünde sanat eseri, renk kodu, etiket ve etkiler yazılıdır.  
  - Arka yüz tek tip “kader” teması taşır.  
  - Kartlar **drag & drop** ile düğümlere yerleştirilebilir. Godot’un kart framework’ü; kartlar arasında drag&drop, hedefleme okları ve gruplama gibi özellikleri destekler【618850826405569†L55-L73】.  
- **Düğüm Haritası:**  
  - Her düğüm, heks/penta şeklinde (örümcek ağı motifinde) temsil edilir.  
  - Bağlantılar farklı renkte iplikler olarak görselleştirilir.  
  - Oyuncu, bağlantı üzerindeki rengi görerek hangi kartların uygun olduğunu anlayabilir.  
- **Hand UI:** Kart elini 2D ortamda hafif kıvrılmış bir düzlemde gösterir; kart seçilince büyütülür (focus-in).  
- **Harita Önizleme:** Kehanet ve benzeri kartlar kullanıldığında, bir sonraki tur düğüm haritasının holografik önizlemesi ekranda gösterilir.  

### 7.2 Kullanıcı Deneyimi

- **Geri Bildirim:**  
  - Kartlar yerleştirildiğinde highlight, sinerji oluştuğunda parlayan iplikler, Kaos yükseldiğinde ekran titremesi gibi geri bildirimler verilir.  
  - Oyuncu kararsız kaldığında ipucu sistemi devreye girerek sinerji önerileri sunabilir (isteğe bağlı).  
- **Hikâye Metni:** Kartlar açıldığında sahnenin alt kısmında mini hikâye cümleleri akıcı olarak gösterilir; bu, oyuna anlatı ve mizah katar.  
- **Erişilebilirlik:** Renk körü modu (farklı ikonlar), ayarlanabilir font boyutu ve buton yerleşimi.  

### 7.3 Teknik Uygulama

- **Motor ve Framework:** Godot 4.5'in Card Game Framework’ü; kart hareketleri, highlight’lar, drag & drop ve token sistemi için hazır fonksiyonlar sunar【618850826405569†L55-L73】.  
- **Kod Yapısı:**  
  - **Card** sınıfı: veri (isim, etiket, etkiler, sinerji fonksiyonu) ve görselleştirme (Sprite3D, Label) içerir.  
  - **NodeMap** sınıfı: düğüm yapısını ve bağlantıları (Graph) tutar; procedural generation fonksiyonları.  
  - **GameManager:** Tur döngüsünü, Kaos/DP sayaçlarını ve run akışını yönetir.  
  - **MetaManager:** Meta progression, mağaza ve save sistemi.  
- **Veri Tanımları:** Kartlar JSON formatında tanımlanır; böylece balancing işlemleri kolaylaşır (Godot framework’ün JSON kart tanımlama özelliği vardır【618850826405569†L67-L69】).  

## 8. Sanat ve Ses Tasarımı

- **Görsel Stil:** Karanlık ve kozmik bir palet. İpler, pulsar ışıkları gibi parlayan; düğümler örümcek ağı motifinde. Kart illüstrasyonları mistik ve gotik sanat yönelimleriyle uyumlu olmalıdır.  
- **Animasyonlar:** Kartların düğüme yerleştirildiğinde ipliklerin titreşmesi; sinerji sırasında kıvılcımlar; Kaos patladığında ekranın çatlaması.  
- **Ses:**  
  - Arka planda düşük tempolu, ambient dark fantasy müzik.  
  - Kart oynamada kağıt sürtme ve büyülü tonlar; sinerji oluşumunda crescendo efekti; felaketlerde yoğun gong veya bağırtılar.  
  - Özel kartlarda (Kızıl Ay, Kahramanca Fedakarlık) ses manevraları ile dramatik vurgu.  

## 9. Pazarlama ve Hit Olma Stratejisi

- **Erken Erişim ve Topluluk:** Oyunu early access olarak Steam’de yayınlayarak topluluktan geri bildirim almak. Yaratıcı kullanıcıların kart önerileri ile kart havuzunu genişletmek.  
- **Twitch ve YouTube Yayıncıları:** Sinerji keşiflerine dayalı “bir run’da en yüksek destan puanı” meydan okumaları, viral klipler oluşturur. Synerji keşifleri oyuncuyu “bulmaca çözücü” gibi hissettirir; bu duygusal tatmin, oyuncuların oyuna bağlanmasını sağlar【776939076369990†L225-L251】.  
- **Mobil ve Tablet:** Mobil uyumlu tasarım sayesinde geniş kitleye ulaşılabilir.  
- **Mod Desteği:** Kart veri yapısı açık olduğundan, oyuncular kendi kart ve modlarını ekleyebilir; community desteği oyun ömrünü uzatır.  

## 10. Sonuç

*Kaderin Ağları*, kart oyunlarının sinerji odaklı keyfini, roguelike'ın tekrar oynanabilirliğini ve puzzle stratejisinin zihinsel tatminini bir araya getirerek benzersiz bir deneyim sunar. Bu dokümanda anlatılan meta progression sistemleri sayesinde oyun, oyuncuyu adım adım yeni içeriklerle tanıştırır ve uzun vadeli hedefler sunar【856645375258190†L468-L523】. Godot 4.5'in kart oyun framework'ü ile 2D ortamda zengin bir kullanıcı deneyimi tasarlamak mümkündür【618850826405569†L55-L73】.  
Oyuncuların kendi destanlarını dokudukları bu oyunda, her run yeni bir hikâye, yeni sinerjiler ve yeni risklerle dolu olacak; böylece **Kaderin Ağları** oyun dünyasında güçlü bir yer edinmeye adaydır.

---

## 11. Geliştirme Notları ve İlerleme Raporu

### 11.1 Veri Yönetimi ve Resource Sistemi

