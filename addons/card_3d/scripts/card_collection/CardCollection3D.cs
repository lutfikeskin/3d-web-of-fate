using Godot;
using Godot.Collections;

[GlobalClass]
public partial class CardCollection3D : Node3D
{
	[Signal]
	public delegate void MouseEnterDropZoneEventHandler();
	
	[Signal]
	public delegate void MouseExitDropZoneEventHandler();
	
	[Signal]
	public delegate void CardSelectedEventHandler(Card3D card);
	
	[Signal]
	public delegate void CardDeselectedEventHandler(Card3D card);
	
	[Signal]
	public delegate void CardClickedEventHandler(Card3D card);
	
	[Signal]
	public delegate void CardAddedEventHandler(Card3D card);
	
	[Signal]
	public delegate void CardMovedEventHandler(Card3D card, int from, int to);

	private static readonly Shape3D DefaultDropZoneShape3D = GD.Load<Shape3D>("res://addons/card_3d/shapes_3d/default_card_collection_3d_drop_zone_shape_3d.tres");
	private const float DefaultDropZoneZOffset = 1.6f;

	[Export]
	public bool HighlightOnHover { get; set; } = true;
	
	[Export]
	public float CardMoveTweenDuration { get; set; } = 0.25f;
	
	[Export]
	public float CardSwapTweenDuration { get; set; } = 0.25f;

	private CardLayout _cardLayoutStrategy = new LineCardLayout();
	[Export]
	public CardLayout CardLayoutStrategy
	{
		get => _cardLayoutStrategy;
		set
		{
			_cardLayoutStrategy = value;
			ApplyCardLayout();
		}
	}

	private DragStrategy _dragStrategy = new DragStrategy();
	[Export]
	public DragStrategy DragStrategy
	{
		get => _dragStrategy;
		set
		{
			_dragStrategy = value ?? new DragStrategy();
		}
	}

	private Shape3D _dropzoneCollisionShape = null;
	[Export]
	public Shape3D DropzoneCollisionShape
	{
		get => _dropzoneCollisionShape;
		set
		{
			_dropzoneCollisionShape = value;
			var dropZone = GetNode<CollisionShape3D>("DropZone/CollisionShape3D");
			dropZone.Shape = _dropzoneCollisionShape ?? DefaultDropZoneShape3D;
		}
	}

	private float _dropzoneZOffset = DefaultDropZoneZOffset;
	[Export]
	public float DropzoneZOffset
	{
		get => _dropzoneZOffset;
		set
		{
			_dropzoneZOffset = value;
			var dropZone = GetNode<Node3D>("DropZone");
			dropZone.Position = new Vector3(dropZone.Position.X, dropZone.Position.Y, _dropzoneZOffset);
		}
	}

	public Array<Card3D> Cards { get; private set; } = new Array<Card3D>();
	public Dictionary<Card3D, int> CardIndicies { get; private set; } = new Dictionary<Card3D, int>();
	public bool IsDraggingCard { get; set; } = false;

	public bool HoverDisabled { get; set; } = false; // Disable card hover animation (useful when dragging other cards around)
	private Card3D _hoveredCard; // Card currently hovered
	private int _previewDropIndex = -1;

	private CollisionShape3D _dropzoneCollision;
	private Dictionary<Card3D, Godot.Collections.Array<Callable>> _cardCallables = new Dictionary<Card3D, Godot.Collections.Array<Callable>>();

	public override void _Ready()
	{
		_dropzoneCollision = GetNode<CollisionShape3D>("DropZone/CollisionShape3D");
	}

	// Add a card to the hand and animate it to the correct position
	// This will add card as child of this node
	public void AppendCard(Card3D card)
	{
		InsertCard(card, Cards.Count);
	}

	public void PrependCard(Card3D card)
	{
		InsertCard(card, 0);
	}

	public void InsertCard(Card3D card, int index)
	{
		var callables = new Godot.Collections.Array<Callable>();
		
		var mouseDownCallable = Callable.From(() => OnCardPressed(card));
		var mouseUpCallable = Callable.From(() => OnCardDeselected(card));
		var mouseOverCallable = Callable.From(() => OnCardHover(card));
		var mouseExitCallable = Callable.From(() => OnCardExit(card));
		
		card.Connect(Card3D.SignalName.Card3DMouseDown, mouseDownCallable);
		card.Connect(Card3D.SignalName.Card3DMouseUp, mouseUpCallable);
		card.Connect(Card3D.SignalName.Card3DMouseOver, mouseOverCallable);
		card.Connect(Card3D.SignalName.Card3DMouseExit, mouseExitCallable);
		
		callables.Add(mouseDownCallable);
		callables.Add(mouseUpCallable);
		callables.Add(mouseOverCallable);
		callables.Add(mouseExitCallable);
		_cardCallables[card] = callables;

		Cards.Insert(index, card);
		AddChild(card);

		for (int i = index; i < Cards.Count; i++)
		{
			CardIndicies[Cards[i]] = i;
		}

		ApplyCardLayout();
		EmitSignal(SignalName.CardAdded, card);
	}

	// Remove and return card from the end of the list
	public Card3D PopCard()
	{
		return RemoveCard(Cards.Count - 1);
	}

	// Remove and return card from the beginning of the list
	public Card3D ShiftCard()
	{
		return RemoveCard(0);
	}

	// Remove card from this hand and return it.
	// The caller is responsible for adding card elsewhere
	// and/or calling QueueFree on it
	public Card3D RemoveCard(int index)
	{
		var removedCard = Cards[index];
		Cards.RemoveAt(index);
		CardIndicies.Remove(removedCard);

		for (int i = index; i < Cards.Count; i++)
		{
			CardIndicies[Cards[i]] = i;
		}

		RemoveChild(removedCard);
		ApplyCardLayout();

		if (_cardCallables.ContainsKey(removedCard))
		{
			var callables = _cardCallables[removedCard];
			removedCard.Disconnect(Card3D.SignalName.Card3DMouseDown, callables[0]);
			removedCard.Disconnect(Card3D.SignalName.Card3DMouseUp, callables[1]);
			removedCard.Disconnect(Card3D.SignalName.Card3DMouseOver, callables[2]);
			removedCard.Disconnect(Card3D.SignalName.Card3DMouseExit, callables[3]);
			_cardCallables.Remove(removedCard);
		}

		return removedCard;
	}

	// Remove and return all cards
	public Array<Card3D> RemoveAll()
	{
		var cardsToReturn = new Array<Card3D>(Cards);
		Cards.Clear();
		CardIndicies.Clear();

		foreach (var c in cardsToReturn)
		{
			RemoveChild(c);
			if (_cardCallables.ContainsKey(c))
			{
				var callables = _cardCallables[c];
				c.Disconnect(Card3D.SignalName.Card3DMouseDown, callables[0]);
				c.Disconnect(Card3D.SignalName.Card3DMouseUp, callables[1]);
				c.Disconnect(Card3D.SignalName.Card3DMouseOver, callables[2]);
				c.Disconnect(Card3D.SignalName.Card3DMouseExit, callables[3]);
				_cardCallables.Remove(c);
			}
		}

		ApplyCardLayout();

		return cardsToReturn;
	}

	public void MoveCard(Card3D cardToMove, int newIndex)
	{
		var currentIndex = CardIndicies[cardToMove];

		Cards.RemoveAt(currentIndex);
		Cards.Insert(newIndex, cardToMove);

		var from = Mathf.Min(currentIndex, newIndex);
		var to = Mathf.Max(currentIndex, newIndex) + 1;
		for (int i = from; i < to; i++)
		{
			CardIndicies[Cards[i]] = i;
		}

		ApplyCardLayout();
		EmitSignal(SignalName.CardMoved, cardToMove, currentIndex, newIndex);
	}

	public void ApplyCardLayout()
	{
		_cardLayoutStrategy.UpdateCardPositions(Cards, CardMoveTweenDuration);
	}

	public void PreviewCardRemove(Card3D draggingCard)
	{
		if (CardIndicies.ContainsKey(draggingCard))
		{
			var previewCards = new Array<Card3D>();
			var cardIndex = CardIndicies[draggingCard];
			
			// Add cards before the dragged card
			for (int i = 0; i < cardIndex; i++)
			{
				previewCards.Add(Cards[i]);
			}
			
			// Add cards after the dragged card
			for (int i = cardIndex + 1; i < Cards.Count; i++)
			{
				previewCards.Add(Cards[i]);
			}

			_cardLayoutStrategy.UpdateCardPositions(previewCards, CardSwapTweenDuration);
		}
	}

	public void PreviewCardDrop(Card3D draggingCard, int index)
	{
		if (index == _previewDropIndex)
		{
			return;
		}

		_previewDropIndex = index;
		var previewCards = new Array<Card3D>();

		if (CardIndicies.ContainsKey(draggingCard))
		{
			// Dragging card in the current collection
			index = Mathf.Clamp(index, 0, Cards.Count - 1);
			var currentIndex = CardIndicies[draggingCard];
			
			// Add cards before the dragged card
			for (int i = 0; i < currentIndex; i++)
			{
				previewCards.Add(Cards[i]);
			}
			
			// Add cards after the dragged card
			for (int i = currentIndex + 1; i < Cards.Count; i++)
			{
				previewCards.Add(Cards[i]);
			}
			
			previewCards.Insert(index, null);
		}
		else
		{
			// Dragging new card in from another collection
			for (int i = 0; i < index; i++)
			{
				previewCards.Add(Cards[i]);
			}
			previewCards.Add(null);
			for (int i = index; i < Cards.Count; i++)
			{
				previewCards.Add(Cards[i]);
			}
		}

		_cardLayoutStrategy.UpdateCardPositions(previewCards, CardSwapTweenDuration);
	}

	public void EnableDropZone()
	{
		_previewDropIndex = -1;
		_dropzoneCollision.Disabled = false;
	}

	public void DisableDropZone()
	{
		_previewDropIndex = -1;
		_dropzoneCollision.Disabled = true;
	}

	/// <summary>
	/// Returns the index at which a card should be inserted based on a projection along the layout direction.
	/// </summary>
	/// <param name="globalDirection">The normalized direction vector in global space.</param>
	/// <param name="distanceAlongLayout">The projected distance along the layout direction.</param>
	public int GetClosestCardIndexAlongVector(Vector3 globalDirection, float distanceAlongLayout)
	{
		int index = Cards.Count;
		for (int i = 0; i < Cards.Count; i++)
		{
			var cardPositionLocal = _cardLayoutStrategy.CalculateCardPositionByIndex(Cards.Count, i);
			var cardPositionGlobal = ToGlobal(cardPositionLocal);
			var cardProjection = cardPositionGlobal.Dot(globalDirection);
			if (distanceAlongLayout < cardProjection)
			{
				index = i;
				break;
			}
		}
		return index;
	}

	// When a mouse enters card collision
	// Set hover state, if applicable
	private void OnCardHover(Card3D card)
	{
		if (!HoverDisabled && CanSelectCard(card))
		{
			_hoveredCard = card;

			if (HighlightOnHover)
			{
				card.SetHovered();
			}
		}
	}

	private void OnCardExit(Card3D card)
	{
		if (!HoverDisabled && _hoveredCard == card)
		{
			card.RemoveHovered();
			_hoveredCard = null;
		}
	}

	private void OnCardPressed(Card3D card)
	{
		if (CanSelectCard(card))
		{
			IsDraggingCard = false;
			EmitSignal(SignalName.CardSelected, card);
		}
	}

	private void OnCardDeselected(Card3D card)
	{
		if (!IsDraggingCard)
		{
			EmitSignal(SignalName.CardClicked, card);
		}
		IsDraggingCard = false;
		EmitSignal(SignalName.CardDeselected, card);
	}

	private void OnDropZoneMouseEntered()
	{
		EmitSignal(SignalName.MouseEnterDropZone);
	}

	private void OnDropZoneMouseExited()
	{
		_previewDropIndex = -1;
		EmitSignal(SignalName.MouseExitDropZone);
	}

	public bool CanSelectCard(Card3D card)
	{
		return _dragStrategy.CanSelectCard(card, this);
	}

	public bool CanRemoveCard(Card3D card)
	{
		return _dragStrategy.CanRemoveCard(card, this);
	}

	public bool CanReorderCard(Card3D card)
	{
		return _dragStrategy.CanReorderCard(card, this);
	}

	public bool CanInsertCard(Card3D card, CardCollection3D fromCollection)
	{
		return _dragStrategy.CanInsertCard(card, this, fromCollection);
	}
}
