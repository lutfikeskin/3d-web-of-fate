# Editor Setup - CardDatabase ve SynergyResolver

## CardDatabase'e Kart Atama

1. **Scene'de CardDatabase Node'unu Bul**
   - `table.tscn` dosyasını aç
   - Scene tree'de `CardDatabase` node'unu bul (veya `Table` altında olmalı)

2. **Inspector'da Cards Array'ini Doldur**
   - `CardDatabase` node'unu seç
   - Inspector panelinde `Cards` property'sini bul
   - Array size'ı 8 yap (veya istediğin kadar)
   - Her slot için kart `.tres` dosyasını sürükle-bırak:
     - `res://data/cards/acemi_kahraman.tres`
     - `res://data/cards/yasak_ask.tres`
     - `res://data/cards/kanli_baron.tres`
     - `res://data/cards/gizemli_rehber.tres`
     - `res://data/cards/efsanevi_kilic.tres`
     - `res://data/cards/buyukanne_kurabiyesi.tres`
     - `res://data/cards/karanlik_orman.tres`
     - `res://data/cards/kizil_ay.tres`

## SynergyResolver'a Kurallar Atama

1. **Scene'de SynergyResolver Node'unu Bul**
   - `table.tscn` dosyasını aç
   - Scene tree'de `SynergyResolver` node'unu bul (veya `Table` altında olmalı)

2. **Inspector'da Rules Array'ini Doldur**
   - `SynergyResolver` node'unu seç
   - Inspector panelinde `Rules` property'sini bul
   - Array size'ı 9 yap
   - Her slot için sinerji kuralı `.tres` dosyasını sürükle-bırak:
     - `res://data/synergy/violence_duo.tres`
     - `res://data/synergy/violence_trio.tres`
     - `res://data/synergy/tragedy_pair.tres`
     - `res://data/synergy/hope_circle.tres`
     - `res://data/synergy/violence_tragedy_mix.tres`
     - `res://data/synergy/secilmis_kisi.tres`
     - `res://data/synergy/romeo_juliet.tres`
     - `res://data/synergy/blood_thread_violence.tres`
     - `res://data/synergy/blood_thread_tragedy.tres`

## Alternatif: Otomatik Yükleme

Eğer editor'de manuel atama yapmazsan, sistem otomatik olarak `data/cards` ve `data/synergy` klasörlerindeki `.tres` dosyalarını yüklemeye çalışır. Ancak bu bazen çalışmayabilir, bu yüzden editor'de manuel atama önerilir.

