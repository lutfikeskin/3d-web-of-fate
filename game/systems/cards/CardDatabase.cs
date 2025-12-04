using Godot;
using Godot.Collections;

[GlobalClass]
public partial class CardDatabase : Node
{
	private Godot.Collections.Dictionary<string, CardData> _cards = new Godot.Collections.Dictionary<string, CardData>();
	private Array<CardData> _allCards = new Array<CardData>();

	[Export]
	public Array<CardData> Cards { get; set; } = new Array<CardData>();

	public override void _Ready()
	{
		LoadCardsFromResources();
	}

	private void LoadCardsFromResources()
	{
		_cards.Clear();
		_allCards.Clear();

		GD.Print("CardDatabase: Starting to load cards...");

		// Önce editor'de atanan kartları yükle
		if (Cards != null && Cards.Count > 0)
		{
			GD.Print($"CardDatabase: Found {Cards.Count} cards in editor assignment");
			foreach (var cardData in Cards)
			{
				if (cardData != null && !string.IsNullOrEmpty(cardData.CardName))
				{
					_cards[cardData.CardName] = cardData;
					_allCards.Add(cardData);
				}
			}
			GD.Print($"CardDatabase: Loaded {_cards.Count} cards from editor assignment");
		}
		else
		{
			GD.Print("CardDatabase: No cards in editor assignment, loading from manual list...");
		}

		// Her zaman manuel listeden de yükle (editor'de atanmış olsa bile, eksik kartlar için)
		LoadCardsFromManualList();
		
		// Eğer hala kart yoksa, dizinden yüklemeyi dene
		if (_cards.Count == 0)
		{
			GD.Print("CardDatabase: No cards loaded from manual list, trying directory scan...");
			LoadCardsFromDirectory("res://data/cards");
		}

		if (_cards.Count == 0)
		{
			GD.PrintErr("CardDatabase: No cards loaded! Check data/cards directory or assign cards in inspector.");
		}
		else
		{
			GD.Print($"CardDatabase: Total {_cards.Count} cards loaded");
		}
	}

	private void LoadCardsFromDirectory(string directoryPath)
	{
		var dir = DirAccess.Open(directoryPath);
		if (dir == null)
		{
			GD.PrintErr($"CardDatabase: Failed to open directory {directoryPath}");
			// Fallback: Manuel dosya listesi
			LoadCardsFromManualList();
			return;
		}

		var files = dir.GetFiles();
		GD.Print($"CardDatabase: Found {files.Length} files in {directoryPath}");

		foreach (var fileName in files)
		{
			if (fileName.EndsWith(".tres"))
			{
				var filePath = $"{directoryPath}/{fileName}";
				GD.Print($"CardDatabase: Loading {filePath}");
				
				if (!ResourceLoader.Exists(filePath))
				{
					GD.PrintErr($"CardDatabase: File does not exist: {filePath}");
					continue;
				}

				// ResourceLoader.Load() kullan (GD.Load yerine)
				var resource = ResourceLoader.Load(filePath);
				if (resource == null)
				{
					GD.PrintErr($"CardDatabase: ResourceLoader.Load returned null for {filePath}");
					continue;
				}

				var cardData = resource as CardData;
				if (cardData == null)
				{
					GD.PrintErr($"CardDatabase: Resource is not CardData type for {filePath}, got {resource.GetType().Name}");
					continue;
				}
				
				if (!string.IsNullOrEmpty(cardData.CardName))
				{
					if (!_cards.ContainsKey(cardData.CardName))
					{
						_cards[cardData.CardName] = cardData;
						_allCards.Add(cardData);
						GD.Print($"CardDatabase: Loaded card '{cardData.CardName}' from {fileName}");
					}
					else
					{
						GD.Print($"CardDatabase: Duplicate card name '{cardData.CardName}' found in {fileName}");
					}
				}
				else
				{
					GD.PrintErr($"CardDatabase: CardData has no name from {fileName}");
				}
			}
		}
	}

	private void LoadCardsFromManualList()
	{
		// Manuel dosya listesi - en güvenilir yöntem
		var cardFiles = new string[]
		{
			"res://data/cards/acemi_kahraman.tres",
			"res://data/cards/yasak_ask.tres",
			"res://data/cards/kanli_baron.tres",
			"res://data/cards/gizemli_rehber.tres",
			"res://data/cards/efsanevi_kilic.tres",
			"res://data/cards/buyukanne_kurabiyesi.tres",
			"res://data/cards/karanlik_orman.tres",
			"res://data/cards/kizil_ay.tres"
		};

		GD.Print($"CardDatabase: Loading cards from manual list...");
		int loadedCount = 0;

		foreach (var filePath in cardFiles)
		{
			if (ResourceLoader.Exists(filePath))
			{
				// ResourceLoader.Load() kullan (GD.Load yerine)
				var resource = ResourceLoader.Load(filePath);
				if (resource == null)
				{
					GD.PrintErr($"CardDatabase: ResourceLoader.Load returned null for {filePath}");
					continue;
				}

				var cardData = resource as CardData;
				if (cardData == null)
				{
					GD.PrintErr($"CardDatabase: Resource is not CardData type for {filePath}, got {resource.GetType().Name}");
					continue;
				}

				if (!string.IsNullOrEmpty(cardData.CardName))
				{
					if (!_cards.ContainsKey(cardData.CardName))
					{
						_cards[cardData.CardName] = cardData;
						_allCards.Add(cardData);
						loadedCount++;
						GD.Print($"CardDatabase: Loaded '{cardData.CardName}' from {filePath}");
					}
					else
					{
						GD.Print($"CardDatabase: Duplicate card '{cardData.CardName}' skipped");
					}
				}
				else
				{
					GD.PrintErr($"CardDatabase: CardData has no name from {filePath}");
				}
			}
			else
			{
				GD.PrintErr($"CardDatabase: File does not exist: {filePath}");
			}
		}

		GD.Print($"CardDatabase: Loaded {loadedCount} cards from manual list");
	}

	public CardData GetCard(string cardName)
	{
		return _cards.ContainsKey(cardName) ? _cards[cardName] : null;
	}

	public Array<CardData> GetAllCards()
	{
		return _allCards;
	}

	public Array<CardData> GetCardsByTag(CardData.CardTag tag)
	{
		var result = new Array<CardData>();
		foreach (var card in _allCards)
		{
			if (card.Tag == tag)
			{
				result.Add(card);
			}
		}
		return result;
	}

	public Array<CardData> GetCardsByCategory(CardData.CardCategory category)
	{
		var result = new Array<CardData>();
		foreach (var card in _allCards)
		{
			if (card.Category == category)
			{
				result.Add(card);
			}
		}
		return result;
	}
}
