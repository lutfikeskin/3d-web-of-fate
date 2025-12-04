using Godot;

[GlobalClass]
public partial class CardData : Resource
{
	public enum CardTag
	{
		Violence,    // 🔴 Vahşet
		Mystic,      // 🔵 Mistik
		Hope,        // 🟢 Umut
		Tragedy      // 🟣 Trajedi
	}

	public enum CardCategory
	{
		Character,   // Karakterler
		Item,        // Eşyalar
		Event,       // Olaylar
		Location,    // Lokasyonlar
		Disaster     // Felaketler
	}

	[Export]
	public string CardName { get; set; } = "";

	[Export]
	public CardTag Tag { get; set; }

	[Export]
	public CardCategory Category { get; set; }

	[Export]
	public int BaseDP { get; set; } = 0;  // Destan Puanı

	[Export]
	public int BaseChaos { get; set; } = 0;  // Kaos değeri

	[Export]
	public string Description { get; set; } = "";

	[Export]
	public string SynergyDescription { get; set; } = "";  // Sinerji açıklaması (MVP'de kullanılmayacak ama gelecek için)

	[Export]
	public string ArtPath { get; set; } = "";  // Kart görseli yolu (opsiyonel)

	public CardData()
	{
	}

	public CardData(string name, CardTag tag, CardCategory category, int dp, int chaos, string description = "")
	{
		CardName = name;
		Tag = tag;
		Category = category;
		BaseDP = dp;
		BaseChaos = chaos;
		Description = description;
	}
}

