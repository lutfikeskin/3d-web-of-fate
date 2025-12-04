using Godot;
using Godot.Collections;

[GlobalClass]
public partial class SlotManager : Node3D
{
	[Export]
	public int SlotCount { get; set; } = 5;
	
	[Export]
	public float SlotSpacing { get; set; } = 4.0f; // Slotlar arası mesafe

	private Array<CardSlot> _slots = new Array<CardSlot>();
	private Card3D _draggedCard;
	private CardSlot _hoveredSlot;

	public override void _Ready()
	{
		CreateSlots();
	}

	private void CreateSlots()
	{
		// Slotları yatay bir çizgide yerleştir
		float totalWidth = (SlotCount - 1) * SlotSpacing;
		float startX = -totalWidth / 2.0f;

		for (int i = 0; i < SlotCount; i++)
		{
			var slot = new CardSlot();
			slot.Name = $"Slot_{i}";
			slot.Position = new Vector3(startX + (i * SlotSpacing), 0, 0);
			
			// Slot sinyallerini bağla
			slot.CardPlaced += OnCardPlaced;
			slot.CardRemoved += OnCardRemoved;
			
			AddChild(slot);
			_slots.Add(slot);
		}
	}

	public CardSlot GetSlot(int index)
	{
		if (index >= 0 && index < _slots.Count)
		{
			return _slots[index];
		}
		return null;
	}

	public CardSlot GetNearestSlot(Vector3 position)
	{
		CardSlot nearest = null;
		float nearestDistance = float.MaxValue;

		foreach (var slot in _slots)
		{
			float distance = slot.GlobalPosition.DistanceTo(position);
			if (distance < nearestDistance)
			{
				nearestDistance = distance;
				nearest = slot;
			}
		}

		return nearest;
	}

	public CardSlot GetEmptySlot()
	{
		foreach (var slot in _slots)
		{
			if (!slot.IsOccupied)
			{
				return slot;
			}
		}
		return null;
	}

	public CardSlot GetSlotWithCard(Card3D card)
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

	private void OnCardPlaced(Card3D card)
	{
		GD.Print($"Card placed in slot: {card.Name}");
	}

	private void OnCardRemoved(Card3D card)
	{
		GD.Print($"Card removed from slot: {card.Name}");
	}

	public Array<CardSlot> GetAllSlots()
	{
		return _slots;
	}
}
