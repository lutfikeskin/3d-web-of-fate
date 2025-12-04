using Godot;
using Godot.Collections;

[GlobalClass]
public partial class DragController : Node3D
{
	[Signal]
	public delegate void DragStartedEventHandler(Card3D card);
	
	[Signal]
	public delegate void DragStoppedEventHandler(Card3D card);
	
	[Signal]
	public delegate void CardMovedEventHandler(Card3D card, CardCollection3D fromCollection, CardCollection3D toCollection, int fromIndex, int toIndex);

	[Export]
	public int MaxDragYRotationDeg { get; set; } = 65;
	
	[Export]
	public int MaxDragXRotationDeg { get; set; } = 65;

	private Plane _cardDragPlane = new Plane(new Vector3(0, 0, 1), 1.5f); // Plane card is moved across on drag
	[Export]
	public Plane CardDragPlane
	{
		get => _cardDragPlane;
		set => _cardDragPlane = value;
	}

	/// <summary>
	/// The minimum distance the mouse needs to travel in viewport coordinates to consider the input a drag.
	/// </summary>
	private float _cardDragThreshold = 0.0f;
	private float _cardDragThresholdSquared = 0.0f;
	[Export(PropertyHint.Range, "0,100,1")]
	public float CardDragThreshold
	{
		get => _cardDragThreshold;
		set
		{
			_cardDragThresholdSquared = value * value;
			_cardDragThreshold = value;
		}
	}

	private Camera3D _camera; // Camera used for determining where mouse is on drag plane
	private Card3D _draggingCard; // Card that is being dragged
	private CardCollection3D _dragFromCollection; // Collection card being dragged from
	private bool _dragging = false;
	private Vector2 _selectionStartMousePosition;
	private Vector2 _currentMousePosition;
	private CardCollection3D _hoveredCollection; // Collection about to drop card into
	private Plane _hoveredCollectionPlane;
	private Vector3 _hoveredCollectionLayoutDirection;
	private Array<CardCollection3D> _cardCollections = new Array<CardCollection3D>();
	private Dictionary<CardCollection3D, Godot.Collections.Array<Callable>> _collectionCallables = new Dictionary<CardCollection3D, Godot.Collections.Array<Callable>>();

	public override void _Ready()
	{
		var window = GetWindow();
		_camera = window.GetCamera3D();
		
		foreach (Node child in GetChildren())
		{
			if (child is CardCollection3D collection)
			{
				AddCardCollection(collection);
			}
		}
	}

	public void AddCardCollection(CardCollection3D cardCollection)
	{
		_cardCollections.Add(cardCollection);
		
		var callables = new Godot.Collections.Array<Callable>();
		
		var cardSelectedCallable = Callable.From<Card3D>((card) => OnCollectionCardSelected(card, cardCollection));
		var mouseEnterCallable = Callable.From(() => OnCollectionMouseEnterDropZone(cardCollection));
		var mouseExitCallable = Callable.From(() => OnCollectionMouseExitDropZone(cardCollection));
		
		cardCollection.Connect(CardCollection3D.SignalName.CardSelected, cardSelectedCallable);
		cardCollection.Connect(CardCollection3D.SignalName.MouseEnterDropZone, mouseEnterCallable);
		cardCollection.Connect(CardCollection3D.SignalName.MouseExitDropZone, mouseExitCallable);
		
		callables.Add(cardSelectedCallable);
		callables.Add(mouseEnterCallable);
		callables.Add(mouseExitCallable);
		_collectionCallables[cardCollection] = callables;
	}

	public void RemoveCardCollection(CardCollection3D cardCollection)
	{
		if (_cardCollections.Contains(cardCollection))
		{
			_cardCollections.Remove(cardCollection);
			if (_collectionCallables.ContainsKey(cardCollection))
			{
				var callables = _collectionCallables[cardCollection];
				cardCollection.Disconnect(CardCollection3D.SignalName.CardSelected, callables[0]);
				cardCollection.Disconnect(CardCollection3D.SignalName.MouseEnterDropZone, callables[1]);
				cardCollection.Disconnect(CardCollection3D.SignalName.MouseExitDropZone, callables[2]);
				_collectionCallables.Remove(cardCollection);
			}
		}
	}

	public override void _UnhandledInput(InputEvent @event)
	{
		if (@event is InputEventMouseButton mouseButton)
		{
			if (mouseButton.IsReleased() && mouseButton.ButtonIndex == MouseButton.Left)
			{
				OnCollectionCardDeselected();
			}
		}
		
		if (@event is InputEventMouseMotion)
		{
			_currentMousePosition = GetViewport().GetMousePosition();
			if (_dragging)
			{
				HandleDragEvent();
			}
			else if (_draggingCard != null)
			{
				if (_selectionStartMousePosition.DistanceSquaredTo(_currentMousePosition) > _cardDragThresholdSquared)
				{
					DragCardStart();
				}
			}
		}
	}

	private void OnCollectionCardSelected(Card3D card, CardCollection3D collection)
	{
		_selectionStartMousePosition = GetViewport().GetMousePosition();
		_dragFromCollection = collection;
		_draggingCard = card;

		foreach (var cardCollection in _cardCollections)
		{
			cardCollection.HoverDisabled = true;
		}
	}

	private void OnCollectionCardDeselected()
	{
		if (_dragging)
		{
			StopDrag();
		}
		else
		{
			_draggingCard = null;
			_dragFromCollection = null;
			foreach (var cardCollection in _cardCollections)
			{
				cardCollection.HoverDisabled = false;
			}
		}
	}

	private void OnCollectionMouseEnterDropZone(CardCollection3D collection)
	{
		SetHoveredCollection(collection);
	}

	private void OnCollectionMouseExitDropZone(CardCollection3D collection)
	{
		if (_hoveredCollection != _dragFromCollection)
		{
			_hoveredCollection.ApplyCardLayout();
		}
		else
		{
			_hoveredCollection.PreviewCardRemove(_draggingCard);
		}

		_hoveredCollection = null;
	}

	/// <summary>
	/// Sets the currently hovered card collection and updates its layout direction and interaction plane.
	/// </summary>
	private void SetHoveredCollection(CardCollection3D collection)
	{
		_hoveredCollection = collection;
		var layoutNormalLocal = collection.CardLayoutStrategy.GetLayoutNormal();
		_hoveredCollectionLayoutDirection = (collection.GlobalTransform.Basis * layoutNormalLocal).Normalized();
		_hoveredCollectionPlane = new Plane(collection.GlobalTransform.Basis.Z, collection.GlobalPosition);
	}

	private void ReturnCardToCollection(Vector2 mousePosition)
	{
		_dragFromCollection.IsDraggingCard = true;
		if (_dragFromCollection.CanReorderCard(_draggingCard))
		{
			SetHoveredCollection(_dragFromCollection);
			var currentIndex = _dragFromCollection.CardIndicies[_draggingCard];
			var newIndex = GetHoveredCollectionIndexAtMousePos(mousePosition);
			newIndex = Mathf.Clamp(newIndex, 0, _dragFromCollection.Cards.Count - 1);

			if (currentIndex != newIndex)
			{
				_dragFromCollection.MoveCard(_draggingCard, newIndex);
				EmitSignal(SignalName.CardMoved, _draggingCard, _dragFromCollection, _dragFromCollection, currentIndex, newIndex);
			}
		}

		_dragFromCollection.ApplyCardLayout();
	}

	private void DropCardToAnotherCollection(Vector2 mousePosition)
	{
		if (!_hoveredCollection.CanInsertCard(_draggingCard, _dragFromCollection))
		{
			return;
		}

		var cardIndex = _dragFromCollection.CardIndicies[_draggingCard];
		var cardGlobalPosition = _dragFromCollection.Cards[cardIndex].GlobalPosition;
		var c = _dragFromCollection.RemoveCard(cardIndex);

		_hoveredCollection.IsDraggingCard = true;
		if (_hoveredCollection.CanReorderCard(c))
		{
			var index = GetHoveredCollectionIndexAtMousePos(mousePosition);
			_hoveredCollection.InsertCard(c, index);
			EmitSignal(SignalName.CardMoved, _draggingCard, _dragFromCollection, _hoveredCollection, cardIndex, index);
		}
		else
		{
			_hoveredCollection.AppendCard(c);
			EmitSignal(SignalName.CardMoved, _draggingCard, _dragFromCollection, _hoveredCollection, cardIndex, _hoveredCollection.Cards.Count - 1);
		}
		
		c.RemoveHovered();
		c.GlobalPosition = cardGlobalPosition;
	}

	private void DragCardStart()
	{
		_dragging = true;
		_draggingCard.DisableCollision();
		_draggingCard.RemoveHovered();
		_dragFromCollection.EnableDropZone();

		foreach (var collection in _cardCollections)
		{
			if (collection.CanInsertCard(_draggingCard, _dragFromCollection))
			{
				collection.EnableDropZone();
			}

			collection.HoverDisabled = true;
		}

		EmitSignal(SignalName.DragStarted, _draggingCard);
	}

	private void StopDrag()
	{
		bool canInsert = true;

		if (_hoveredCollection != null)
		{
			canInsert = _hoveredCollection.CanInsertCard(_draggingCard, _dragFromCollection) && _dragFromCollection.CanRemoveCard(_draggingCard);
		}

		if (!canInsert)
		{
			ReturnCardToCollection(_currentMousePosition);
		}
		else if (_hoveredCollection == null || _hoveredCollection == _dragFromCollection)
		{
			ReturnCardToCollection(_currentMousePosition);
		}
		else if (_hoveredCollection != null && _hoveredCollection != _dragFromCollection)
		{
			DropCardToAnotherCollection(_currentMousePosition);
		}

		_dragFromCollection.DisableDropZone();
		_draggingCard.EnableCollision();

		var card = _draggingCard;
		var fromCollection = _dragFromCollection;
		var toCollection = _hoveredCollection;

		_dragging = false;
		_draggingCard = null;
		_dragFromCollection = null;

		foreach (var collection in _cardCollections)
		{
			collection.DisableDropZone();
			collection.HoverDisabled = false;
		}

		EmitSignal(SignalName.DragStopped, card);
	}

	private void HandleDragEvent()
	{
		var position3D = _cardDragPlane.IntersectsRay(
			_camera.ProjectRayOrigin(_currentMousePosition),
			_camera.ProjectRayNormal(_currentMousePosition)
		);
		var cardPosition = _draggingCard.GlobalPosition;

		var xDistance = position3D.X - cardPosition.X;
		var yDistance = position3D.Y - cardPosition.Y;

		// Add rotation to make dragging cards pretty
		// Rotate around y axis for horizontal rotation
		var yDegrees = xDistance * 25;
		yDegrees = Mathf.Clamp(yDegrees, -MaxDragYRotationDeg, MaxDragYRotationDeg);

		// Rotate around x axis for vertical rotation
		var xDegrees = -yDistance * 25;
		xDegrees = Mathf.Clamp(xDegrees, -MaxDragXRotationDeg, MaxDragXRotationDeg);
		var zDegrees = 0.0f;

		// Put degrees in Vector3
		var targetRotation = new Vector3(
			Mathf.DegToRad(xDegrees),
			Mathf.DegToRad(yDegrees),
			Mathf.DegToRad(zDegrees)
		);

		// Set rotation
		_draggingCard.DraggingRotation(targetRotation);

		// Set card position to under mouse
		_draggingCard.GlobalPosition = new Vector3(position3D.X, position3D.Y, position3D.Z);

		if (_hoveredCollection != null && position3D != Vector3.Zero && _hoveredCollection.CanReorderCard(_draggingCard))
		{
			var index = GetHoveredCollectionIndexAtMousePos(_currentMousePosition);
			_hoveredCollection.PreviewCardDrop(_draggingCard, index);
		}
	}

	/// <summary>
	/// Returns the index in the hovered collection where a card would be inserted based on the mouse position.
	/// </summary>
	private int GetHoveredCollectionIndexAtMousePos(Vector2 mousePos)
	{
		var collectionPlaneIntersection = _hoveredCollectionPlane.IntersectsRay(
			_camera.ProjectRayOrigin(mousePos),
			_camera.ProjectRayNormal(mousePos)
		);

		if (collectionPlaneIntersection == Vector3.Zero)
		{
			return _hoveredCollection.Cards.Count;
		}

		var offset = collectionPlaneIntersection - _hoveredCollection.GlobalPosition;
		var distanceAlongLayout = offset.Dot(_hoveredCollectionLayoutDirection);
		return _hoveredCollection.GetClosestCardIndexAlongVector(_hoveredCollectionLayoutDirection, distanceAlongLayout);
	}

	private Vector2 GetDragScreenPoint(Vector3 worldPosition)
	{
		if (worldPosition != Vector3.Zero)
		{
			return _camera.UnprojectPosition(worldPosition);
		}
		else
		{
			return Vector2.Zero;
		}
	}
}

