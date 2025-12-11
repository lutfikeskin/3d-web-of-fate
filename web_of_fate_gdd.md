# Kaderin Ağları (Web of Fate) – Master Tasarım Dokümanı (GDD) v3.0

Bu doküman, "Kaderin Ağları" oyununun temel mekaniklerini, veri yapısını ve teknik mimarisini tanımlar. Proje, hem 3D hem de 2D uygulamalara (Implementation) izin verecek şekilde **Veri Odaklı (Data-Driven)** ve **Mantık-Görünüm Ayrımı (Logic-View Separation)** prensipleriyle tasarlanmıştır.

---

## 1. Oyun Kimliği

*   **İsim:** Kaderin Ağları (Web of Fate)
*   **Tür:** Narrative Puzzle / Roguelike Deckbuilder
*   **Motor:** Godot 4.5+ (GDScript)
*   **Temel Vaat:** Oyuncu bir savaşçı değil, bir "Kader Örgücüsü"dür. Kartlar savaşmak için değil, hikayeyi ve kader ağını manipüle etmek için kullanılır.
*   **Hook (Kanca):** "Sticky Web" (Yapışkan Ağ) mekaniği. Yanlış hamleler masada kalır ve yer kaplar. Sadece doğru hikayeler (sinerjiler) düğümleri çözer.

---

## 2. Oynanış Döngüsü (Core Loop)

Oyun, **Tek Buton Akışı** (Weave Fate) üzerine kuruludur.

### 2.1 Fazlar
1.  **Hazırlık (Preparation):**
    *   Oyuncu desteden elini 5 karta tamamlar.
    *   Eldeki kartlar, masadaki (Loom) boş slotlara yerleştirilir.
    *   *Strateji:* Hangi kartın hangi slotta olduğu, bağlandığı iplik rengine (Thread Type) ve komşu kartlara göre belirlenir.

2.  **Örme (Weaving):**
    *   Oyuncu "WEAVE FATE" butonuna basar.
    *   Oyun duraksar ve hesaplama başlar.
    *   **Story Engine** devreye girer: Kartların kombinasyonuna göre ekrana bir hikaye metni yazılır (örn: "Kahraman kılıcı buldu ama aşka yenik düştü.").

3.  **Çözümleme (Resolution & Sticky Web):**
    *   **Sinerji Kontrolü:** Bağlı slotlar kontrol edilir. Eğer geçerli bir sinerji varsa (örn: Hero + Sword), bu kartlar puan (DP) kazandırır ve **Masadan Kaldırılır (Discard)**.
    *   **Tıkanma (Stuck):** Sinerji oluşturmayan kartlar **Masada Kalır**. Bu kartlar slotları işgal etmeye devam eder.
    *   *Ceza:* Eğer tüm slotlar dolarsa ve sinerji yoksa veya Kaos limiti aşılırsa oyun biter.

---

## 3. Sistem Mimarisi (2D/3D Agnostik)

Oyun mantığı, görünümden bağımsızdır. `GameManager` ve `LoomManager` 2D veya 3D düğümlerden haberdar değildir, sadece Veri (Data) ve ID'ler ile konuşur.

### 3.1 Singletonlar (Autoloads)
*   **`GameManager`**: Oyunun durumunu (State), Desteyi (Deck), Puanları (DP/Chaos) ve Fazları yönetir.
*   **`LoomManager`**: Slotların mantıksal haritasını tutar. Hangi Slot ID'de hangi `CardData` var, hangi Slotlar birbirine bağlı bilgisini yönetir.

### 3.2 Veri Yapıları (Custom Resources)
Tüm oyun içeriği `.tres` dosyalarıdır. Kod değiştirmeden oyun dengesi değiştirilebilir.

*   **`CardData`**:
    *   `id`: String (Unique)
    *   `display_name`: String
    *   `category`: Enum (Character, Item, Event, Location)
    *   `tags`: Array[String] (Violence, Mystic, Romance, Heroic)
    *   `base_dp`: int
    *   `base_chaos`: int
    *   `texture_path`: String (Görsel yolu)

*   **`SynergyData`**:
    *   `required_cards`: Array[CardData] (Spesifik kart gereksinimi)
    *   `required_tags`: Array[String] (Etiket gereksinimi)
    *   `result_dp_bonus`: int
    *   `result_chaos_change`: int
    *   `is_valid`: bool (Slotu temizler mi?)

*   **`NarrativeEvent`**:
    *   `text_template`: String ("{card1} meets {card2}...")
    *   `conditions`: Array[Resource] (Hangi kartlar/etiketler yan yana gelince bu hikaye çıkar?)
    *   `priority`: int

---

## 4. Mevcut İçerik (Kartlar ve Sinerjiler)

### 4.1 Uygulanmış Kartlar (Mevcut `CardData`)
Şu anda sistemde tanımlı ve çalışan kartlar:

| Kart Adı | Kategori | Etiketler | Temel Etki |
| :--- | :--- | :--- | :--- |
| **Novice Hero** | Character | `Heroic`, `Human` | +5 DP, -2 Chaos |
| **Legendary Sword** | Item | `Weapon`, `Metal` | +10 DP, +5 Chaos |
| **Bloody Baron** | Character | `Violence`, `Villain` | +15 DP, +15 Chaos |
| **Forbidden Love** | Event | `Romance`, `Tragedy` | +20 DP, +10 Chaos |
| **Mystic Guide** | Character | `Mystic`, `Support` | +5 DP, -5 Chaos |

### 4.2 Örnek Sinerjiler
*   **Hero's Journey:** `Novice Hero` + `Legendary Sword` -> Slotlar temizlenir, yüksek puan.
*   **Tragic End:** `Forbidden Love` + `Bloody Baron` -> Yüksek Kaos, Trajik hikaye tetiklenir.

---

## 5. Proje Klasör Yapısı

Bu yapı, hem 2D hem 3D için ortaktır. Görsel dosyalar (`scenes`) ayrışır.

```text
res://
├── data/                       # TÜM OYUN VERİSİ (Logic)
│   ├── cards/                  # CardData .tres dosyaları
│   ├── synergies/              # SynergyData .tres dosyaları
│   ├── narrative/              # NarrativeEvent .tres dosyaları
│   └── threads/                # İplik tanımları
├── logic/                      # OYUN MANTIĞI (Script Only)
│   ├── game_manager.gd         # Autoload
│   ├── loom_manager.gd         # Autoload
│   ├── synergy_calculator.gd   # Helper class
│   └── story_engine.gd         # Helper class
├── resources/                  # RESOURCE SCRIPTS (Tanımlar)
│   ├── card_data.gd
│   ├── synergy_data.gd
│   ├── narrative_event.gd
│   └── ...
├── scenes/                     # GÖRÜNÜM (View)
│   ├── 3d/                     # 3D Versiyon Varlıkları
│   │   ├── table_3d.tscn
│   │   ├── card_3d.tscn
│   │   ├── slot_3d.tscn
│   │   └── thread_visualizer.gd
│   ├── 2d/                     # 2D Versiyon Varlıkları (Planlanan)
│   │   ├── table_2d.tscn
│   │   ├── card_2d.tscn
│   │   └── slot_2d.tscn
│   └── ui/                     # Ortak UI
│       └── hud.tscn
└── assets/                     # Görseller, Sesler, Materyaller
```

---

## 6. 2D ve 3D Entegrasyon Stratejisi

Bu GDD'yi 2D projede kullanırken dikkat edilecekler:

1.  **Logic Dosyaları Aynen Kalır:** `logic/` ve `resources/` klasörleri 2D projeye kopyala-yapıştır yapılabilir. Hiçbir değişiklik gerektirmez.
2.  **Sinyal Yapısı:**
    *   2D'de: `Slot2D` tıklandığında AYNI `LoomManager.card_placed` sinyalini yaymalıdır.
3.  **Görselleştirme:**
    *   2D'de `Line2D` nodu kullanılarak aynı mantık (Start Pos -> End Pos) ile iplikler çizilir.

### Mevcut Durum Notları
*   **Drag & Drop:** Şu anki `DragController` 3D Raycast kullanır. 2D versiyonu için Godot'un yerleşik `_get_drag_data` ve `_drop_data` fonksiyonları veya basit bir `Area2D` mouse takibi kullanılmalıdır.
*   **Highlight:** 2D'de `modulate` değeri veya bir `Shader` kullanılabilir.

---

## 7. Hedefler (Roadmap)

1.  **Narrative Genişlemesi:** 50+ Hikaye parçası eklemek.
2.  **Görsel Cila:**
    *   2D için: Pixel art veya vektörel UI tasarımı.
