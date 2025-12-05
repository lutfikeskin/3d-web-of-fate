using Godot;
using Godot.Collections;

[GlobalClass]
public partial class Deck : Node
{
	/// <summary>
	/// Başlangıç destesini inspector'den ayarlamak için.
	/// </summary>
	[Export]
	public Array<CardData> StartingDeck { get; set; } = new Array<CardData>();

	private Array<CardData> _drawPile = new Array<CardData>();
	private RandomNumberGenerator _rng = new RandomNumberGenerator();

	public override void _Ready()
	{
		_rng.Randomize();

		// Editor'den atanmış başlangıç destesini kullan
		if (StartingDeck != null && StartingDeck.Count > 0)
		{
			Reset(StartingDeck);
		}
	}

	/// <summary>
	/// Desteyi yeni kart listesiyle sıfırlar ve karıştırır.
	/// </summary>
	public void Reset(Array<CardData> startingDeck = null)
	{
		_drawPile.Clear();

		if (startingDeck != null && startingDeck.Count > 0)
		{
			foreach (var card in startingDeck)
			{
				if (card != null)
				{
					_drawPile.Add(card);
				}
			}
		}
		else if (StartingDeck != null && StartingDeck.Count > 0)
		{
			foreach (var card in StartingDeck)
			{
				if (card != null)
				{
					_drawPile.Add(card);
				}
			}
		}

		Shuffle();
	}

	/// <summary>
	/// Fisher-Yates ile desteyi karıştırır.
	/// </summary>
	public void Shuffle()
	{
		for (int i = _drawPile.Count - 1; i > 0; i--)
		{
			int j = _rng.RandiRange(0, i);
			(_drawPile[i], _drawPile[j]) = (_drawPile[j], _drawPile[i]);
		}
	}

	/// <summary>
	/// Tek kart çeker; destede kart yoksa null döner.
	/// </summary>
	public CardData DrawCard()
	{
		if (_drawPile.Count == 0)
		{
			return null;
		}

		var top = _drawPile[0];
		_drawPile.RemoveAt(0);
		return top;
	}

	/// <summary>
	/// Belirli sayıda kart çeker ve döner.
	/// </summary>
	public Array<CardData> DrawCards(int count)
	{
		var result = new Array<CardData>();

		for (int i = 0; i < count; i++)
		{
			var card = DrawCard();
			if (card == null)
			{
				break;
			}
			result.Add(card);
		}

		return result;
	}

	/// <summary>
	/// Kartı destenin altına ekler.
	/// </summary>
	public void AddCard(CardData card)
	{
		if (card == null)
		{
			return;
		}
		_drawPile.Add(card);
	}

	public int GetRemainingCount()
	{
		return _drawPile.Count;
	}
}

