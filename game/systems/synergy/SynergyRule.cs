using Godot;
using Godot.Collections;

[GlobalClass]
public partial class SynergyRule : Resource
{
	public enum RuleType
	{
		TagBased,      // Etiket bazlı (örn: 2 Violence kartı yan yana)
		Combo,         // Özel kombo (örn: Acemi Kahraman + Efsanevi Kılıç)
		ThreadBased,   // Thread tipi bazlı (örn: Violence + Blood Thread)
		LocationBased  // Lokasyon bazlı (örn: Karanlık Orman + Canavar)
	}

	[Export]
	public RuleType Type { get; set; } = RuleType.TagBased;

	[Export]
	public string RuleName { get; set; } = "";

	[Export]
	public Array<string> RequiredCards { get; set; } = new Array<string>();  // Kombo için gerekli kart isimleri

	[Export]
	public CardData.CardTag RequiredTag { get; set; }  // Etiket bazlı sinerji için

	[Export]
	public int RequiredTagCount { get; set; } = 2;  // Kaç tane aynı etiketli kart gerekli

	[Export]
	public CardSlot.ThreadType RequiredThread { get; set; }  // Thread bazlı sinerji için

	[Export]
	public int BonusDP { get; set; } = 0;  // Sinerji bonus DP

	[Export]
	public int BonusChaos { get; set; } = 0;  // Sinerji bonus Kaos

	[Export]
	public float DPMultiplier { get; set; } = 1.0f;  // DP çarpanı (örn: x2, x3)

	[Export]
	public float ChaosMultiplier { get; set; } = 1.0f;  // Kaos çarpanı

	[Export]
	public string Description { get; set; } = "";  // Sinerji açıklaması

	public SynergyRule()
	{
	}
}

