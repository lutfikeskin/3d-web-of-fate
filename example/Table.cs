using Godot;
using Godot.Collections;
using System;

public partial class Table : Node3D
{
	private FaceCards _cardDatabase = new FaceCards();
	private FaceCards.Suit[] _suits = {
		FaceCards.Suit.Club,
		FaceCards.Suit.Spade,
		FaceCards.Suit.Diamond,
		FaceCards.Suit.Heart,
	};
	private int[] _ranks = { 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14 };

	private int _suitIndex = 0;
	private int _rankIndex = 0;

	private CardCollection3D _hand;
	private CardCollection3D _pile;

	public override void _Ready()
	{
		_hand = GetNode<CardCollection3D>("DragController/Hand");
		_pile = GetNode<CardCollection3D>("DragController/TableCards");
	}

	public override void _Input(InputEvent @event)
	{
		if (@event.IsActionPressed("ui_down"))
		{
			AddCard();
		}
		else if (@event.IsActionPressed("ui_up"))
		{
			RemoveCard();
		}
		else if (@event.IsActionPressed("ui_left"))
		{
			ClearCards();
		}
		else if (@event.IsActionPressed("ui_right"))
		{
			if (_pile.CardLayoutStrategy is PileCardLayout && _hand.CardLayoutStrategy is LineCardLayout)
			{
				var layout = new LineCardLayout();
				_pile.CardLayoutStrategy = layout;
			}
			else if (_hand.CardLayoutStrategy is LineCardLayout)
			{
				_hand.CardLayoutStrategy = new FanCardLayout();
			}
			else if (_pile.CardLayoutStrategy is LineCardLayout)
			{
				_pile.CardLayoutStrategy = new PileCardLayout();
			}
			else if (_hand.CardLayoutStrategy is FanCardLayout)
			{
				_hand.CardLayoutStrategy = new LineCardLayout();
			}
		}
	}

	private FaceCard3D InstantiateFaceCard(FaceCards.Rank rank, FaceCards.Suit suit)
	{
		var scene = GD.Load<PackedScene>("res://example/face_card_3d.tscn");
		var faceCard3D = scene.Instantiate<FaceCard3D>();
		var cardData = _cardDatabase.GetCardData(rank, suit);
		faceCard3D.Rank = (FaceCards.Rank)(int)cardData["rank"].AsInt32();
		faceCard3D.Suit = (FaceCards.Suit)(int)cardData["suit"].AsInt32();
		faceCard3D.FrontMaterialPath = (string)cardData["front_material_path"];
		faceCard3D.BackMaterialPath = (string)cardData["back_material_path"];

		return faceCard3D;
	}

	private void AddCard()
	{
		var data = NextCard();
		var card = InstantiateFaceCard((FaceCards.Rank)(int)data["rank"].AsInt32(), (FaceCards.Suit)(int)data["suit"].AsInt32());
		_hand.AppendCard(card);
		var deck = GetNode<Node3D>("../Deck");
		card.GlobalPosition = deck.GlobalPosition;
	}

	private Dictionary NextCard()
	{
		var suit = _suits[_suitIndex];
		var rank = _ranks[_rankIndex];

		_rankIndex += 1;

		if (_rankIndex == _ranks.Length)
		{
			_rankIndex = 0;
			_suitIndex += 1;
		}

		if (_suitIndex == _suits.Length)
		{
			_suitIndex = 0;
		}

		return new Dictionary
		{
			{ "suit", (int)suit },
			{ "rank", rank }
		};
	}

	private void RemoveCard()
	{
		if (_hand.Cards.Count == 0)
		{
			return;
		}

		var random = new Random();
		var randomCardIndex = random.Next(_hand.Cards.Count);
		var cardToRemove = _hand.Cards[randomCardIndex];

		PlayCard(cardToRemove);
	}

	private void PlayCard(Card3D card)
	{
		var cardIndex = _hand.CardIndicies[card];
		var cardGlobalPosition = _hand.Cards[cardIndex].GlobalPosition;
		var c = _hand.RemoveCard(cardIndex);

		_pile.AppendCard(c);
		c.RemoveHovered();
		c.GlobalPosition = cardGlobalPosition;
	}

	private void RetrieveCardFromTable(Card3D card)
	{
		var cardIndex = _pile.CardIndicies[card];
		var cardGlobalPosition = _pile.Cards[cardIndex].GlobalPosition;
		var c = _pile.RemoveCard(cardIndex);

		_hand.AppendCard(c);
		c.RemoveHovered();
		c.GlobalPosition = cardGlobalPosition;
	}

	private void ClearCards()
	{
		var handCards = _hand.RemoveAll();
		var pileCards = _pile.RemoveAll();

		foreach (var c in handCards)
		{
			c.QueueFree();
		}

		foreach (var c in pileCards)
		{
			c.QueueFree();
		}
	}

	private void OnFaceCard3DCard3DMouseUp()
	{
		AddCard();
	}

	private void OnHandCardClicked(Card3D card)
	{
		PlayCard(card);
	}

	private void OnTableCardsCardClicked(Card3D card)
	{
		RetrieveCardFromTable(card);
	}
}

