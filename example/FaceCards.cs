using Godot;
using Godot.Collections;

[GlobalClass]
public partial class FaceCards : Resource
{
	public enum Rank
	{
		Two = 2,
		Three = 3,
		Four = 4,
		Five = 5,
		Six = 6,
		Seven = 7,
		Eight = 8,
		Nine = 9,
		Ten = 10,
		Jack = 11,
		Queen = 12,
		King = 13,
		Ace = 14
	}

	public enum Suit
	{
		Heart,
		Diamond,
		Club,
		Spade
	}

	private Dictionary _data = GenerateAllFaceCards();

	public Dictionary Data => _data;

	public Dictionary GetCardData(Rank rank, Suit suit)
	{
		var cardId = GetCardId(rank, suit);

		if (_data.ContainsKey(cardId))
		{
			return (Dictionary)_data[cardId];
		}

		return null;
	}

	public string GetCardId(Rank rank, Suit suit)
	{
		return rank.ToString() + " of " + suit.ToString();
	}

	private static Dictionary GenerateAllFaceCards()
	{
		var allFaceCardData = new Dictionary();

		foreach (Suit suit in System.Enum.GetValues(typeof(Suit)))
		{
			foreach (Rank rank in System.Enum.GetValues(typeof(Rank)))
			{
				var frontMaterial = "res://example/materials/" + suit.ToString().ToLower() + "-" + rank.ToString() + ".tres";
				var backMaterial = "res://example/materials/card-back.tres";
				var cardData = new Dictionary
				{
					{ "rank", rank },
					{ "suit", suit },
					{ "front_material_path", frontMaterial },
					{ "back_material_path", backMaterial }
				};
				var cardId = rank.ToString() + " of " + suit.ToString();
				allFaceCardData[cardId] = cardData;
			}
		}

		return allFaceCardData;
	}
}

