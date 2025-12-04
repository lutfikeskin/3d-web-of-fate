using Godot;
using Godot.Collections;

[GlobalClass]
public partial class CardDatabase : Node
{
	private Godot.Collections.Dictionary<string, CardData> _cards = new Godot.Collections.Dictionary<string, CardData>();
	private Array<CardData> _allCards = new Array<CardData>();

	[Export]
	public string CardsJsonPath { get; set; } = "res://data/cards/cards.json";

	public override void _Ready()
	{
		LoadCardsFromJson();
	}

	public void LoadCardsFromJson()
	{
		_cards.Clear();
		_allCards.Clear();

		if (!FileAccess.FileExists(CardsJsonPath))
		{
			GD.PrintErr($"Card database JSON file not found: {CardsJsonPath}");
			return;
		}

		var file = FileAccess.Open(CardsJsonPath, FileAccess.ModeFlags.Read);
		if (file == null)
		{
			GD.PrintErr($"Failed to open card database file: {CardsJsonPath}");
			return;
		}

		var jsonString = file.GetAsText();
		file.Close();

		var json = new Json();
		var parseResult = json.Parse(jsonString);

		if (parseResult != Error.Ok)
		{
			GD.PrintErr($"Failed to parse JSON: {parseResult}");
			return;
		}

		var jsonData = json.Data.AsGodotDictionary();
		
		if (!jsonData.ContainsKey("cards"))
		{
			GD.PrintErr("JSON file missing 'cards' key");
			return;
		}

		var cardsArray = jsonData["cards"].AsGodotArray();
		
		foreach (var cardObj in cardsArray)
		{
			var cardDict = cardObj.AsGodotDictionary();
			var cardData = ParseCardFromDictionary(cardDict);
			
			if (cardData != null)
			{
				_cards[cardData.CardName] = cardData;
				_allCards.Add(cardData);
			}
		}

		GD.Print($"Loaded {_cards.Count} cards from database");
	}

	private CardData ParseCardFromDictionary(Dictionary cardDict)
	{
		try
		{
			var cardData = new CardData();
			
			if (cardDict.ContainsKey("name"))
			{
				cardData.CardName = cardDict["name"].AsString();
			}
			
			if (cardDict.ContainsKey("tag"))
			{
				var tagStr = cardDict["tag"].AsString().ToLower();
				cardData.Tag = tagStr switch
				{
					"violence" => CardData.CardTag.Violence,
					"mystic" => CardData.CardTag.Mystic,
					"hope" => CardData.CardTag.Hope,
					"tragedy" => CardData.CardTag.Tragedy,
					_ => CardData.CardTag.Violence
				};
			}
			
			if (cardDict.ContainsKey("category"))
			{
				var categoryStr = cardDict["category"].AsString().ToLower();
				cardData.Category = categoryStr switch
				{
					"character" => CardData.CardCategory.Character,
					"item" => CardData.CardCategory.Item,
					"event" => CardData.CardCategory.Event,
					"location" => CardData.CardCategory.Location,
					"disaster" => CardData.CardCategory.Disaster,
					_ => CardData.CardCategory.Character
				};
			}
			
			if (cardDict.ContainsKey("dp"))
			{
				cardData.BaseDP = cardDict["dp"].AsInt32();
			}
			
			if (cardDict.ContainsKey("chaos"))
			{
				cardData.BaseChaos = cardDict["chaos"].AsInt32();
			}
			
			if (cardDict.ContainsKey("description"))
			{
				cardData.Description = cardDict["description"].AsString();
			}
			
			if (cardDict.ContainsKey("synergy"))
			{
				cardData.SynergyDescription = cardDict["synergy"].AsString();
			}
			
			if (cardDict.ContainsKey("art_path"))
			{
				cardData.ArtPath = cardDict["art_path"].AsString();
			}
			
			return cardData;
		}
		catch (System.Exception e)
		{
			GD.PrintErr($"Error parsing card: {e.Message}");
			return null;
		}
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

