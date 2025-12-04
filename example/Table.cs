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
	private DragController _dragController;
	private Array<CardSlot> _slots;

	public override void _Ready()
	{
		_hand = GetNode<CardCollection3D>("DragController/Hand");
		_dragController = GetNode<DragController>("DragController");
		
		// Slot'ları bul ve sinyallerini bağla
		var slotsNode = GetNode<Node3D>("Slots");
		_slots = new Array<CardSlot>();
		
		foreach (Node child in slotsNode.GetChildren())
		{
			if (child is CardSlot slot)
			{
				_slots.Add(slot);
				slot.CardRemoved += OnSlotCardRemoved;
			}
		}
		
		// DragController sinyallerini bağla
		_dragController.DragStopped += OnDragStopped;
		_dragController.DragStarted += OnDragStarted;
		_dragController.CardMoved += OnCardMoved;
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
			// Layout değiştirme - sadece hand için
			if (_hand.CardLayoutStrategy is LineCardLayout)
			{
				_hand.CardLayoutStrategy = new FanCardLayout();
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

		// Kartı hand'den çıkar (artık otomatik slot'a yerleştirme yok)
		_hand.RemoveCard(randomCardIndex);
		cardToRemove.QueueFree();
	}

	// PlayCard metodu kaldırıldı - kartlar sadece drag-and-drop ile slot'a yerleştirilebilir

	// Bu metod artık kullanılmıyor - slot'tan alma RetrieveCardFromSlot ile yapılıyor

	private void ClearCards()
	{
		// Hand'deki kartları temizle
		var handCards = _hand.RemoveAll();
		foreach (var c in handCards)
		{
			c.QueueFree();
		}
		
		// Slot'lardaki kartları temizle
		foreach (var slot in _slots)
		{
			var card = slot.RemoveCard();
			if (card != null)
			{
				card.QueueFree();
			}
		}
	}

	private void OnFaceCard3DCard3DMouseUp()
	{
		AddCard();
	}

	private void OnHandCardClicked(Card3D card)
	{
		// Kart tıklama ile otomatik yerleştirme kaldırıldı
		// Kartlar sadece drag-and-drop ile slot'a yerleştirilebilir
		// Bu metod boş bırakıldı çünkü signal bağlantısı scene'de olabilir
	}

	// OnTableCardsCardClicked artık kullanılmıyor - slot'tan alma RetrieveCardFromSlot ile yapılıyor

	private void OnDragStopped(Card3D card)
	{
		// Highlight'ları temizle
		ClearSlotHighlights();
		
		if (card == null || _slots == null || !card.IsInsideTree())
		{
			return;
		}
		
		// Kart zaten bir slot'ta mı? (GetSlotWithCard, GetPlacedCard() == card kontrol eder)
		// Eğer kart geri alındıysa, _placedCard null olur ve GetSlotWithCard null döner
		var slotWithCard = GetSlotWithCard(card);
		
		// En yakın boş slot'u bul
		CardSlot nearestSlot = null;
		float nearestDistance = float.MaxValue;
		float maxDropDistance = 5.0f; // Slot'a maksimum mesafe
		
		Vector3 cardGlobalPos = card.GlobalPosition;
		
		foreach (var slot in _slots)
		{
			// Sadece boş slot'ları kontrol et
			// CanPlaceCard() hem IsOccupied (_placedCard == null) hem de card != null kontrol eder
			if (!slot.CanPlaceCard(card))
			{
				continue; // Slot dolu veya kart null, atla
			}
			
			float distance = slot.GlobalPosition.DistanceTo(cardGlobalPos);
			if (distance < nearestDistance && distance < maxDropDistance)
			{
				nearestDistance = distance;
				nearestSlot = slot;
			}
		}
		
		// Eğer yakın bir boş slot varsa, kartı oraya yerleştir
		if (nearestSlot != null)
		{
			// Kart zaten başka bir slot'taysa, önce oradan çıkar
			if (slotWithCard != null && slotWithCard != nearestSlot)
			{
				slotWithCard.RemoveCard();
			}
			
			// Hand'den çıkar (eğer hand'deyse)
			if (_hand != null && _hand.CardIndicies.ContainsKey(card))
			{
				var cardIndex = _hand.CardIndicies[card];
				_hand.RemoveCard(cardIndex);
			}
			
			// Slot'a yerleştir
			nearestSlot.PlaceCard(card);
		}
		else if (slotWithCard != null)
		{
			// Slot'a yerleştirilemedi ve kart zaten bir slot'ta
			// Kartı geri hand'e al (RetrieveCardFromSlot RemoveCard çağıracak, o da sinyal gönderecek)
			RetrieveCardFromSlot(card);
		}
		// Eğer kart hand'deyse ve slot'a yerleştirilemediyse, hiçbir şey yapma (hand'de kalsın)
	}

	private CardSlot _highlightedSlot; // Şu anda highlight edilmiş slot
	
	private void OnDragStarted(Card3D card)
	{
		// Drag başladığında tüm slot highlight'larını temizle
		ClearSlotHighlights();
	}
	
	private void OnCardMoved(Card3D card, CardCollection3D fromCollection, CardCollection3D toCollection, int fromIndex, int toIndex)
	{
		// Kart hareket ettiğinde slot kontrolü yap
	}
	
	public override void _Process(double delta)
	{
		// Drag sırasında sadece en yakın boş slot'u highlight et
		if (_dragController != null && _dragController.IsDragging())
		{
			var draggingCard = _dragController.GetDraggingCard();
			if (draggingCard != null && draggingCard.IsInsideTree() && _slots != null)
			{
				UpdateSlotHighlight(draggingCard);
			}
		}
		else
		{
			// Drag yoksa tüm highlight'ları temizle
			ClearSlotHighlights();
		}
	}
	
	private void UpdateSlotHighlight(Card3D card)
	{
		if (card == null || !card.IsInsideTree())
		{
			ClearSlotHighlights();
			return;
		}
		
		// ÖNCE: Tüm slot'ların highlight'ını kaldır
		foreach (var slot in _slots)
		{
			slot.Highlight(false);
		}
		
		// SONRA: Sadece en yakın boş slot'u bul ve highlight et
		CardSlot nearestSlot = null;
		float nearestDistance = float.MaxValue;
		float maxDropDistance = 5.0f; // Slot'a maksimum mesafe
		
		Vector3 cardGlobalPos = card.GlobalPosition;
		
		foreach (var slot in _slots)
		{
			// Sadece boş slot'ları kontrol et
			// CanPlaceCard() hem IsOccupied hem de card != null kontrol ediyor
			if (!slot.CanPlaceCard(card))
			{
				continue; // Slot dolu veya kart null, atla
			}
			
			float distance = slot.GlobalPosition.DistanceTo(cardGlobalPos);
			if (distance < nearestDistance && distance < maxDropDistance)
			{
				nearestDistance = distance;
				nearestSlot = slot;
			}
		}
		
		// Sadece en yakın boş slot'u highlight et
		if (nearestSlot != null)
		{
			nearestSlot.Highlight(true);
			_highlightedSlot = nearestSlot;
		}
		else
		{
			_highlightedSlot = null;
		}
	}
	
	private void ClearSlotHighlights()
	{
		if (_slots != null)
		{
			foreach (var slot in _slots)
			{
				slot.Highlight(false);
			}
		}
		_highlightedSlot = null;
	}

	// TryPlaceCardInSlot metodu kaldırıldı - kartlar sadece drag-and-drop ile slot'a yerleştirilebilir
	// Bu metod artık kullanılmıyor, sadece OnDragStopped içinde kart yerleştirme yapılıyor

	private void RetrieveCardFromSlot(Card3D card)
	{
		var slotWithCard = GetSlotWithCard(card);
		if (slotWithCard != null)
		{
			// RemoveCard zaten sinyal gönderecek ve OnSlotCardRemoved çağrılacak
			// Bu yüzden burada sadece RemoveCard çağırmak yeterli
			slotWithCard.RemoveCard();
		}
	}
	
	private CardSlot GetSlotWithCard(Card3D card)
	{
		foreach (var slot in _slots)
		{
			if (slot.GetPlacedCard() == card)
			{
				return slot;
			}
		}
		return null;
	}
	
	// Slot'tan kart kaldırıldığında çağrılır
	private void OnSlotCardRemoved(Card3D card)
	{
		if (_hand == null || card == null)
		{
			return;
		}
		
		// Kart zaten slot'tan çıkarılmış (RemoveCard içinde parent değişti ve _placedCard null yapıldı)
		var currentParent = card.GetParent();
		
		// Eğer kartın parent'ı hand değilse, önce çıkar
		if (currentParent != null && currentParent != _hand)
		{
			currentParent.RemoveChild(card);
		}
		
		// Kartı hand'e ekle (AppendCard içinde AddChild çağrılacak ve layout uygulanacak)
		if (card.GetParent() != _hand)
		{
			_hand.AppendCard(card);
		}
		
		// Hand'e döndükten sonra (artık tree'de) rotasyonu sıfırla
		if (card.IsInsideTree())
		{
			card.Rotation = Vector3.Zero;
			card.GlobalBasis = Basis.Identity;
		}
		
		card.RemoveHovered();
		
		// Slot'un state'i zaten RemoveCard() içinde resetlendi (_placedCard = null, Highlight(false))
		// Bu yüzden slot artık boş ve tekrar kullanılabilir durumda
	}
}
