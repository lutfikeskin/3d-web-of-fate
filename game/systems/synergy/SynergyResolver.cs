using Godot;
using Godot.Collections;

[GlobalClass]
public partial class SynergyResolver : Node
{
	private SynergyDetector _detector;
	private Array<SynergyRule> _rules = new Array<SynergyRule>();

	[Export]
	public Array<SynergyRule> Rules { get; set; } = new Array<SynergyRule>();

	public override void _Ready()
	{
		_detector = new SynergyDetector();
		AddChild(_detector);
		_detector.Name = "SynergyDetector";
		
		LoadSynergyRules();
	}

	private void LoadSynergyRules()
	{
		_rules.Clear();

		// Önce editor'de atanan kuralları yükle
		if (Rules != null && Rules.Count > 0)
		{
			foreach (var rule in Rules)
			{
				if (rule != null)
				{
					_rules.Add(rule);
				}
			}
			GD.Print($"Loaded {_rules.Count} synergy rules from editor assignment");
		}
		else
		{
			// Editor'de atanmamışsa, otomatik olarak data/synergy klasöründen yükle
			LoadRulesFromDirectory("res://data/synergy");
		}

		if (_rules.Count == 0)
		{
			GD.PrintErr("SynergyResolver: No rules loaded! Check data/synergy directory or assign rules in inspector.");
		}
		else
		{
			GD.Print($"SynergyResolver: Total {_rules.Count} rules loaded");
		}
	}

	private void LoadRulesFromDirectory(string directoryPath)
	{
		var dir = DirAccess.Open(directoryPath);
		if (dir == null)
		{
			GD.PrintErr($"SynergyResolver: Failed to open directory {directoryPath}");
			// Fallback: Manuel dosya listesi
			LoadRulesFromManualList();
			return;
		}

		var files = dir.GetFiles();
		GD.Print($"SynergyResolver: Found {files.Length} files in {directoryPath}");

		foreach (var fileName in files)
		{
			if (fileName.EndsWith(".tres"))
			{
				var filePath = $"{directoryPath}/{fileName}";
				
				if (!ResourceLoader.Exists(filePath))
				{
					GD.PrintErr($"SynergyResolver: File does not exist: {filePath}");
					continue;
				}

				var rule = GD.Load<SynergyRule>(filePath);
				
				if (rule != null)
				{
					_rules.Add(rule);
					GD.Print($"SynergyResolver: Loaded rule '{rule.RuleName}' from {fileName}");
				}
				else
				{
					GD.PrintErr($"SynergyResolver: Failed to load rule from {fileName}");
				}
			}
		}
	}

	private void LoadRulesFromManualList()
	{
		// Fallback: Manuel dosya listesi
		var ruleFiles = new string[]
		{
			"res://data/synergy/violence_duo.tres",
			"res://data/synergy/violence_trio.tres",
			"res://data/synergy/tragedy_pair.tres",
			"res://data/synergy/hope_circle.tres",
			"res://data/synergy/violence_tragedy_mix.tres",
			"res://data/synergy/secilmis_kisi.tres",
			"res://data/synergy/romeo_juliet.tres",
			"res://data/synergy/blood_thread_violence.tres",
			"res://data/synergy/blood_thread_tragedy.tres"
		};

		foreach (var filePath in ruleFiles)
		{
			if (ResourceLoader.Exists(filePath))
			{
				var rule = GD.Load<SynergyRule>(filePath);
				if (rule != null)
				{
					_rules.Add(rule);
				}
			}
		}
	}

	/// <summary>
	/// Bir kart yerleştirildiğinde sinerji bonuslarını hesaplar ve döndürür
	/// </summary>
	public SynergyResult CalculateSynergy(CardSlot placedSlot, Array<CardSlot> allSlots, CardDatabase cardDatabase)
	{
		var result = new SynergyResult();
		
		if (placedSlot == null || !placedSlot.IsOccupied || allSlots == null)
		{
			return result;
		}
		
		var placedCard = placedSlot.GetPlacedCard();
		var cardData = GetCardData(placedCard, cardDatabase);
		
		if (cardData == null)
		{
			return result;
		}
		
		// Kart grubunu al (merkez + komşular)
		var cardGroup = _detector.GetCardGroup(placedSlot, allSlots);
		
		// Tüm sinerji kurallarını kontrol et
		foreach (var rule in _rules)
		{
			if (CheckRule(rule, placedSlot, cardGroup, cardDatabase))
			{
				ApplyRule(result, rule, placedCard, cardData);
				result.TriggeredRules.Add(rule);
			}
		}
		
		return result;
	}

	private bool CheckRule(SynergyRule rule, CardSlot placedSlot, Array<Card3D> cardGroup, CardDatabase cardDatabase)
	{
		switch (rule.Type)
		{
			case SynergyRule.RuleType.TagBased:
				return CheckTagBasedRule(rule, cardGroup, cardDatabase);
			
			case SynergyRule.RuleType.Combo:
				return CheckComboRule(rule, cardGroup, cardDatabase);
			
			case SynergyRule.RuleType.ThreadBased:
				return CheckThreadBasedRule(rule, placedSlot, cardGroup, cardDatabase);
			
			case SynergyRule.RuleType.LocationBased:
				return CheckLocationBasedRule(rule, cardGroup, cardDatabase);
			
			default:
				return false;
		}
	}

	private bool CheckTagBasedRule(SynergyRule rule, Array<Card3D> cardGroup, CardDatabase cardDatabase)
	{
		int tagCount = 0;
		
		foreach (var card in cardGroup)
		{
			var cardData = GetCardData(card, cardDatabase);
			if (cardData != null && cardData.Tag == rule.RequiredTag)
			{
				tagCount++;
			}
		}
		
		return tagCount >= rule.RequiredTagCount;
	}

	private bool CheckComboRule(SynergyRule rule, Array<Card3D> cardGroup, CardDatabase cardDatabase)
	{
		if (rule.RequiredCards.Count == 0)
		{
			return false;
		}
		
		var foundCards = new Array<string>();
		
		foreach (var card in cardGroup)
		{
			var cardData = GetCardData(card, cardDatabase);
			if (cardData != null && rule.RequiredCards.Contains(cardData.CardName))
			{
				if (!foundCards.Contains(cardData.CardName))
				{
					foundCards.Add(cardData.CardName);
				}
			}
		}
		
		// Tüm gerekli kartlar bulundu mu?
		return foundCards.Count == rule.RequiredCards.Count;
	}

	private bool CheckThreadBasedRule(SynergyRule rule, CardSlot placedSlot, Array<Card3D> cardGroup, CardDatabase cardDatabase)
	{
		// Thread tipi kontrolü
		if (placedSlot.Thread != rule.RequiredThread)
		{
			return false;
		}
		
		// Etiket kontrolü (eğer varsa)
		if (rule.RequiredTagCount > 0)
		{
			var placedCard = placedSlot.GetPlacedCard();
			var cardData = GetCardData(placedCard, cardDatabase);
			if (cardData != null && cardData.Tag == rule.RequiredTag)
			{
				return true;
			}
		}
		else
		{
			return true;  // Sadece thread tipi yeterli
		}
		
		return false;
	}

	private bool CheckLocationBasedRule(SynergyRule rule, Array<Card3D> cardGroup, CardDatabase cardDatabase)
	{
		// Lokasyon bazlı sinerji (ileride implement edilecek)
		return false;
	}

	private void ApplyRule(SynergyResult result, SynergyRule rule, Card3D card, CardData cardData)
	{
		// Bonus DP/Kaos ekle
		result.BonusDP += rule.BonusDP;
		result.BonusChaos += rule.BonusChaos;
		
		// Çarpanları uygula
		if (rule.DPMultiplier != 1.0f)
		{
			result.DPMultiplier *= rule.DPMultiplier;
		}
		
		if (rule.ChaosMultiplier != 1.0f)
		{
			result.ChaosMultiplier *= rule.ChaosMultiplier;
		}
	}

	private CardData GetCardData(Card3D card, CardDatabase cardDatabase)
	{
		if (card == null || cardDatabase == null)
		{
			return null;
		}
		
		// Meta'dan CardData'yı al
		if (card.HasMeta("card_data"))
		{
			var variant = card.GetMeta("card_data");
			return variant.AsGodotObject() as CardData;
		}
		
		return null;
	}
}

/// <summary>
/// Sinerji hesaplama sonucu
/// </summary>
public class SynergyResult
{
	public int BonusDP { get; set; } = 0;
	public int BonusChaos { get; set; } = 0;
	public float DPMultiplier { get; set; } = 1.0f;
	public float ChaosMultiplier { get; set; } = 1.0f;
	public Array<SynergyRule> TriggeredRules { get; set; } = new Array<SynergyRule>();
}

