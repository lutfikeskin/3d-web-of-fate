using Godot;
using Godot.Collections;
using System;

public partial class Table : Node3D
{
	private CardDatabase _cardDatabase;
	private GameState _gameState;
	private SynergyResolver _synergyResolver;
	
	private CardCollection3D _hand;
	private DragController _dragController;
	private Array<CardSlot> _slots;

	public override void _Ready()
	{
		_hand = GetNode<CardCollection3D>("DragController/Hand");
		_dragController = GetNode<DragController>("DragController");
		
		// CardDatabase ve GameState'i bul veya oluştur
		_cardDatabase = GetNodeOrNull<CardDatabase>("CardDatabase");
		if (_cardDatabase == null)
		{
			_cardDatabase = new CardDatabase();
			AddChild(_cardDatabase);
			_cardDatabase.Name = "CardDatabase";
			// Runtime'da oluşturulan node için _Ready() manuel çağrılmalı
			// Ama Godot otomatik çağırır, bu yüzden sadece bekle
			CallDeferred(MethodName.EnsureCardDatabaseReady);
		}
		
		_gameState = GetNodeOrNull<GameState>("GameState");
		if (_gameState == null)
		{
			_gameState = new GameState();
			AddChild(_gameState);
			_gameState.Name = "GameState";
		}
		
		// GameState'i grup olarak ekle (HUD için)
		_gameState.AddToGroup("game_state");
		
		// SynergyResolver'ı oluştur
		_synergyResolver = GetNodeOrNull<SynergyResolver>("SynergyResolver");
		if (_synergyResolver == null)
		{
			_synergyResolver = new SynergyResolver();
			AddChild(_synergyResolver);
			_synergyResolver.Name = "SynergyResolver";
		}
		
		// Slot'ları bul ve sinyallerini bağla
		var slotsNode = GetNode<Node3D>("Slots");
		_slots = new Array<CardSlot>();
		
		foreach (Node child in slotsNode.GetChildren())
		{
			if (child is CardSlot slot)
			{
				_slots.Add(slot);
				slot.CardRemoved += OnSlotCardRemoved;
				slot.CardPlaced += OnSlotCardPlaced;
			}
		}
		
		// DragController sinyallerini bağla
		_dragController.DragStopped += OnDragStopped;
		_dragController.DragStarted += OnDragStarted;
		_dragController.CardMoved += OnCardMoved;
		
		// CardDatabase'in yüklenmesini bekle
		CallDeferred(MethodName.EnsureCardDatabaseReady);
	}
	
	private void EnsureCardDatabaseReady()
	{
		if (_cardDatabase != null)
		{
			// CardDatabase'in kartları yüklediğinden emin ol
			var allCards = _cardDatabase.GetAllCards();
			if (allCards.Count == 0)
			{
				GD.PrintErr("Table: CardDatabase has no cards! Check CardDatabase initialization.");
			}
			else
			{
				GD.Print($"Table: CardDatabase ready with {allCards.Count} cards");
			}
		}
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

	private Card3D InstantiateFateCard(CardData cardData)
	{
		if (cardData == null)
		{
			GD.PrintErr("CardData is null");
			return null;
		}
		
		// Card3D scene'ini yükle
		var cardScene = GD.Load<PackedScene>("res://addons/card_3d/scenes/card_3d.tscn");
		var card3D = cardScene.Instantiate<Card3D>();
		
		if (card3D == null)
		{
			GD.PrintErr($"Failed to instantiate Card3D for {cardData.CardName}");
			return null;
		}
		
		// CardData'yı meta olarak ekle (FateCard3D yerine Card3D kullanıyoruz)
		// Resource'ları meta olarak eklerken Variant.From kullan
		card3D.SetMeta("card_data", Variant.From(cardData));
		card3D.Name = cardData.CardName;
		
		// Kart bilgilerini göster
		SetupCardInfoDisplay(card3D, cardData);
		
		return card3D;
	}

	private void SetupCardInfoDisplay(Card3D card3D, CardData cardData)
	{
		if (cardData == null)
		{
			GD.PrintErr("Table: cardData is null in SetupCardInfoDisplay");
			return;
		}

		GD.Print($"Table: Setting up card info display for {cardData.CardName}");

		// CardMesh'i bul
		var cardMesh = card3D.GetNodeOrNull<Node3D>("CardMesh");
		if (cardMesh == null)
		{
			GD.PrintErr("Table: CardMesh not found in Card3D");
			return;
		}

		// CardInfoDisplay'i oluştur veya bul
		var infoDisplay = cardMesh.GetNodeOrNull<CardInfoDisplay>("CardInfoDisplay");
		if (infoDisplay == null)
		{
			GD.Print("Table: Creating new CardInfoDisplay");
			infoDisplay = new CardInfoDisplay();
			infoDisplay.Name = "CardInfoDisplay";
			cardMesh.AddChild(infoDisplay);
			// _Ready() otomatik çağrılacak
		}
		else
		{
			GD.Print("Table: Found existing CardInfoDisplay");
		}

		// Bir frame bekle ki _Ready() çağrılsın, sonra bilgileri güncelle
		GetTree().CreateTimer(0.1f).Timeout += () =>
		{
			if (infoDisplay != null && cardData != null && infoDisplay.IsInsideTree())
			{
				GD.Print($"Table: Updating card info deferred for {cardData.CardName}");
				infoDisplay.UpdateCardInfo(cardData);
			}
		};
	}
	
	// Card3D'den CardData'yı almak için helper method
	private CardData GetCardData(Card3D card)
	{
		if (card == null || !card.HasMeta("card_data"))
		{
			return null;
		}
		var variant = card.GetMeta("card_data");
		return variant.AsGodotObject() as CardData;
	}

	private void AddCard()
	{
		if (_cardDatabase == null)
		{
			GD.PrintErr("CardDatabase not initialized");
			return;
		}
		
		// Rastgele bir kart seç
		var allCards = _cardDatabase.GetAllCards();
		if (allCards.Count == 0)
		{
			GD.PrintErr("No cards in database");
			return;
		}
		
		var random = new Random();
		var randomIndex = random.Next(allCards.Count);
		var cardData = allCards[randomIndex];
		
		var card = InstantiateFateCard(cardData);
		if (card != null)
		{
			_hand.AppendCard(card);
			var deck = GetNodeOrNull<Node3D>("../Deck");
			if (deck != null)
			{
				card.GlobalPosition = deck.GlobalPosition;
			}
		}
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
			
			// Hand'den çıkar (eğer hand'deyse) - ÖNCE hand'den çıkar
			// Böylece hand'in layout animasyonu başlamadan kart slot'a gider
			if (_hand != null && _hand.CardIndicies.ContainsKey(card))
			{
				var cardIndex = _hand.CardIndicies[card];
				_hand.RemoveCard(cardIndex);
			}
			
			// Slot'a yerleştir - hand'den çıktıktan SONRA
			// DP/Kaos güncellemesi OnSlotCardPlaced'de yapılacak
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
	
	// Slot'a kart yerleştirildiğinde çağrılır
	private void OnSlotCardPlaced(Card3D card)
	{
		if (_gameState == null || card == null)
		{
			return;
		}
		
		// CardData'yı meta'dan al
		var cardData = GetCardData(card);
		if (cardData == null)
		{
			return;
		}
		
		var slot = GetSlotWithCard(card);
		if (slot == null)
		{
			return;
		}
		
		// Thread tipine göre Kaos hesaplama (MVP için sadece Gold thread Kaos'u sıfırlar)
		int chaosValue = cardData.BaseChaos;
		if (slot.Thread == CardSlot.ThreadType.Gold)
		{
			chaosValue = 0;  // Altın İplik Kaos üretmez
		}
		
		// Temel DP/Kaos ekle
		int baseDP = cardData.BaseDP;
		_gameState.AddDP(baseDP);
		_gameState.AddChaos(chaosValue);
		
		// Sinerji hesapla
		if (_synergyResolver != null && _cardDatabase != null)
		{
			var synergyResult = _synergyResolver.CalculateSynergy(slot, _slots, _cardDatabase);
			
			if (synergyResult.TriggeredRules.Count > 0)
			{
				// Sinerji bonuslarını uygula
				int synergyDP = synergyResult.BonusDP;
				int synergyChaos = synergyResult.BonusChaos;
				
				// Çarpanları uygula (base değerlere)
				int finalDP = Mathf.RoundToInt(baseDP * synergyResult.DPMultiplier) + synergyDP;
				int finalChaos = Mathf.RoundToInt(chaosValue * synergyResult.ChaosMultiplier) + synergyChaos;
				
				// Farkı ekle (zaten base değerler eklendi, sadece bonus/çarpan farkını ekle)
				int dpDifference = finalDP - baseDP;
				int chaosDifference = finalChaos - chaosValue;
				
				if (dpDifference != 0)
				{
					_gameState.AddDP(dpDifference);
				}
				
				if (chaosDifference != 0)
				{
					_gameState.AddChaos(chaosDifference);
				}
				
				// Debug: Tetiklenen sinerjileri göster
				GD.Print($"Synergy triggered for {cardData.CardName}:");
				foreach (var rule in synergyResult.TriggeredRules)
				{
					GD.Print($"  - {rule.RuleName}: {rule.Description}");
				}
				GD.Print($"  Bonus: DP +{dpDifference}, Chaos +{chaosDifference}");
			}
		}
		
		GD.Print($"Card placed: {cardData.CardName} - DP: +{baseDP}, Chaos: +{chaosValue}");
	}
	
	// Slot'tan kart kaldırıldığında çağrılır
	private void OnSlotCardRemoved(Card3D card)
	{
		if (_hand == null || card == null)
		{
			return;
		}
		
		// DP/Kaos'u geri al (kart slot'tan çıkarıldığında)
		// Not: Slot bilgisi metadata'dan silindiği için, MVP'de sadece base değerleri kullanıyoruz
		// İleride slot referansını signal'a ekleyebiliriz
		if (_gameState != null)
		{
			var cardData = GetCardData(card);
			if (cardData != null)
			{
			
			// MVP için basit: sadece base değerleri geri al
			// Thread tipi kontrolü eklenebilir ama şimdilik base değerler yeterli
				_gameState.RemoveDP(cardData.BaseDP);
				_gameState.RemoveChaos(cardData.BaseChaos);
				
				GD.Print($"Card removed: {cardData.CardName} - DP: -{cardData.BaseDP}, Chaos: -{cardData.BaseChaos}");
			}
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
		
		// Hand'e döndükten sonra dik dursun (tree'ye girdiyse)
		if (card.IsInsideTree())
		{
			card.GlobalBasis = Basis.Identity;
		}
		else
		{
			// İçinde değilse deferred ayarla
			card.CallDeferred("set", "global_basis", Basis.Identity);
		}
		
		card.RemoveHovered();
		
		// Slot'un state'i zaten RemoveCard() içinde resetlendi (_placedCard = null, Highlight(false))
		// Bu yüzden slot artık boş ve tekrar kullanılabilir durumda
	}
}
