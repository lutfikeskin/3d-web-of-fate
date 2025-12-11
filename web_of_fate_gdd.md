# Kaderin Ağları (Web of Fate) – Genişletilmiş Oyun Tasarım Dokümanı

## Giriş

Kaderin Ağları, oyuncunun "Örgücü" (The Weaver) olarak gerçekliği bir kader ağında dokuduğu benzersiz bir roguelike/deck‐building/puzzle oyunudur. Oyun, klasik kart savaşlarından farklı olarak **hikâye kurgusu** ve **mekânsal bulmaca çözümü** üzerine kurulur.  
Amacımız, oyuncuyu hem yaratıcılığa teşvik eden hem de keşif hissi uyandıran bir deneyim sunmaktır. **Sinerji** tasarımı bu tür oyunlarda keşif ve duygusal tatmin sağlar; birden fazla kartı birleştirmenin toplamından daha büyük etki yaratması oyuncunun keşfetme arzusunu canlı tutar.

Bu doküman, mevcut konsepti inceleyerek hataları düzeltir, mekaniği geliştirir ve Godot 4.5 üzerinden bir kart oyunu olarak hayata geçirmek için ayrıntılı yönergeler sunar.

## 1. Oyun Özeti

- **İsim:** *Kaderin Ağları* (Web of Fate)  
- **Tür:** Roguelike Deckbuilder / Puzzle Strateji  
- **Platform:** PC (Windows, macOS, Linux), Mobil (iOS/Android) ve Tablet. Oyun Godot 4.5 ile geliştirilecek; framework, kartları ortamda sergileyecek şekilde yapılandırılacaktır.  
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
2. **Slot Sayısı:** Standart başlangıçta 5 düğüm bulunur.  
3. **Bağlantı Türleri:**  
   - **İpek İplik (Beyaz):** Standart bağlantı, normal puanlar verir.  
   - **Kan İpliği (Kırmızı):** Buraya **Vahşet (Violence)** veya **Trajedi** kartları bağlanırsa ek DP (Destan Puanı) ve Kaos kazanılır.  
   - **Altın İplik (Sarı):** Bu bağlantıya bağlanan kart Kaos üretmez.  
   - **Gölge İplik (Mor – metaprogression ile açılır):** Kart etkilerini kopyalar veya tersine çevirir; risk/ödül mekaniği için kullanılır.  
4. **Tıkanan Ağ (Sticky Web):** Kartlar oynandıktan sonra, eğer bir **sinerji** oluşturmamışlarsa masada kalırlar. Bu, slotları tıkar. Sadece başarılı sinerjiye giren kartlar masadan temizlenir. 
5. **Kırılma ve Kopma:** Kaos barı 100'e ulaşırsa veya masadaki tüm slotlar dolup hamle yapılamaz hale gelirse iplikler kopar ve run sonlanır (Game Over).  

### 3.2 Kartlar ve Etiketler

Kartlar **Destan**ı oluşturan yapı taşlarıdır. Dört ana **etiket/tag** vardır:

| Etiket | Renk/Kod | Tanım | Etki Eğilimi |
|-------|---------|------|--------------|
| **Vahşet (Violence)** | 🔴 | Dövüş, kan, çatışma, ölümcül risk. | Yüksek DP, yüksek Kaos |
| **Mistik (Mystic)** | 🔵 | Büyü, lanetler, kehanetler, gizem. | Sinerji odaklı, Kaos etkilerini manipüle eder |
| **Umut (Hope)** | 🟢 | İyileştirme, yardım, barış. | Kaos’u düşürür, düşük DP |
| **Trajedi (Tragedy)** | 🟣 | İhanet, kayıp, dram. | Çok yüksek DP, yüksek Kaos risk |

Kartlar **Karakterler**, **Eşyalar**, **Olaylar**, **Lokasyonlar** ve **Felaketler** olarak beş kategoriye ayrılır. Her kartın temel etkisi ve sinerji tetikleyen özel bir kombosu vardır.

### 3.3 Kaynaklar

| Kaynak | Açıklama |
|-------|---------|
| **Destan Puanı (DP / Legacy)** | Skor ve para birimidir. Kart oynamak, sinerji oluşturmak ve run sonu ödülleriyle kazanılır. DP, yeni kartlar, iplik türleri ve meta yükseltmeler satın almak için kullanılır. |
| **Kaos (KP / Chaos)** | 0–100 arası çubuktur. Vahşet/Trajedi etkileri Kaos üretir. 100’ü aşarsa “Kırılma” olur ve run hemen biter. Amaç, Kaos’u kritik seviyede yönetmektir. |
| **El Limiti** | Oyuncunun eli her tur başında 5 karta tamamlanır. Eldeki gereksiz kartları oynamak veya temizlemek stratejik önem taşır. |

### 3.4 Oyun Döngüsü ve Akış (The Loop)

Oyun, **Tek Buton Akışı** (Weave Fate) ile basitleştirilmiş stratejik bir döngüye sahiptir.

#### 3.4.1 Tur Döngüsü (Micro Loop)

1. **Hazırlık (Preparation):** 
   - Oyuncunun eli 5 karta tamamlanır.
   - Oyuncu elindeki kartları boş slotlara yerleştirir. 
   - "Click-to-Place" veya "Drag & Drop" ile kartlar oynanır.
   - Boş slotlar ve uygun hedefler görsel olarak vurgulanır (Highlight).

2. **Kaderi Dokuma (Weave Fate):**
   - Oyuncu "WEAVE FATE" butonuna basar.
   - **Titreşim:** Ağ üzerindeki kartlar ve iplikler görsel/işitsel olarak tepki verir.
   - **Hesaplama:** Kartların etkileri, iplik bonusları ve sinerjiler hesaplanır.
   - **Hikâye:** Kart etkileşimlerine dayalı prosedürel bir hikâye parçası oluşturulur ve günlüğe yazılır.
   - **Çözümleme (Resolution):** Sinerji oluşturan kartlar puan verip masadan kalkar (Discard). Sinerji oluşturmayanlar masada kalarak slotu tıkamaya devam eder.

3. **Sonuç ve Kontrol:**
   - Kaos 100 oldu mu? -> Game Over.
   - 5 Slot da dolu ve hamle yok mu? -> Game Over.
   - Değilse -> Bir sonraki tura geçilir (Tur sayısı artar, el yenilenir).

#### 3.4.2 Run Döngüsü (Macro Loop)

- Her run, oyuncu hayatta kalabildiği sürece devam eder (Sonsuz veya Bölüm Bazlı).
- Run sonunda toplanan DP’den Meta Kredi üretilir ve **Kader Tezgâhı (Meta Shop)** üzerinden yükseltmeler satın alınır.  

#### 3.4.3 Meta Döngü (Outside Run)

- **Ascension Sistemi (Yükseliş):** Oyuncu oyunu tamamladıkça bir üst **Yükseliş Seviyesi** açılır.
- **Kader Tezgâhı:** Oyun dışında DP/MK ile yatırımlar yapılır:  
  - Yeni kart paketleri ve kart etiketleri açmak.  
  - İplik türlerini yükseltmek.  

### 3.5 Prosedürel Hikaye Sistemi

Oyun, kartların etkileşimine göre dinamik metinler üretir.
- **Sistem:** `StoryEngine`, masadaki kartları, etiketlerini ve iplik türlerini analiz eder.
- **Örnek:**
  - *Novice Hero* oynandı: "A novice hero begins their journey."
  - *Bloody Baron* yanına kondu (Kırmızı İplik): "The Bloody Baron intercepts the hero on a path of blood!"
  - *Sinerji Yok:* "The threads are tangled, fate is unclear."

## 4. Teknik Uygulama Notları (Godot 4.5)

### 4.1 Veri Yapıları (Custom Resources)
Oyun tamamen veri odaklı (Data-Driven) tasarlanmıştır.
- **CardData (.tres):** Kartın adı, görseli, etkileri, etiketleri.
- **ThreadDefinition (.tres):** İplik rengi, kalınlığı, shader parametreleri.
- **SynergyData (.tres):** Hangi kartların/etiketlerin birleşince ne yapacağı.
- **NarrativeEvent (.tres):** Hikaye şablonları ve tetiklenme koşulları.

### 4.2 Görsellik ve Shaderlar
- **İplikler:** `ShaderMaterial` kullanan dinamik silindirler. `thread_pulse.gdshader` ile üzerinde enerji akışı ve parlama (emission) efekti vardır.
- **Kartlar:** Mistik ortamda fiziksel varlığı olan nesneler.
- **Slotlar:** Doluluk ve etkileşim durumuna göre renk değiştiren (Yeşil/Kırmızı/Beyaz) highlight mesh'leri.

### 4.3 Kontrol
- **Hibrit Kontrol:** Hem sürükle-bırak (Drag&Drop) hem de Tıkla-Yerleştir (Click-to-Place) desteklenir.

## 5. Gelecek Planları

- **Meta Shop:** DP harcayarak yeni kartların kilidini açma arayüzü.
- **Ses Tasarımı:** Kart hareketleri, iplik titreşimleri ve atmosferik müzik.
- **Daha Fazla İçerik:** 100+ Kart ve 50+ Sinerji kombinasyonu.

---
*Doküman Sürümü: 2.0 - Stratejik Revizyon Sonrası*
