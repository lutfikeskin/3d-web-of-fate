using Godot;
using Godot.Collections;

[GlobalClass]
public partial class SynergyDetector : Node
{
	// Komşuluk mesafesi (slot'lar arası maksimum mesafe)
	[Export]
	public float NeighborDistance { get; set; } = 6.0f;

	/// <summary>
	/// Belirli bir slot'un komşu slot'larını bulur
	/// </summary>
	public Array<CardSlot> GetNeighborSlots(CardSlot slot, Array<CardSlot> allSlots)
	{
		var neighbors = new Array<CardSlot>();
		
		if (slot == null || allSlots == null)
		{
			return neighbors;
		}
		
		Vector3 slotPosition = slot.GlobalPosition;
		
		foreach (var otherSlot in allSlots)
		{
			if (otherSlot == slot || otherSlot == null)
			{
				continue;
			}
			
			float distance = slotPosition.DistanceTo(otherSlot.GlobalPosition);
			if (distance <= NeighborDistance)
			{
				neighbors.Add(otherSlot);
			}
		}
		
		return neighbors;
	}

	/// <summary>
	/// Belirli bir slot'taki kartın komşu kartlarını bulur
	/// </summary>
	public Array<Card3D> GetNeighborCards(CardSlot slot, Array<CardSlot> allSlots)
	{
		var neighborCards = new Array<Card3D>();
		var neighborSlots = GetNeighborSlots(slot, allSlots);
		
		foreach (var neighborSlot in neighborSlots)
		{
			var card = neighborSlot.GetPlacedCard();
			if (card != null)
			{
				neighborCards.Add(card);
			}
		}
		
		return neighborCards;
	}

	/// <summary>
	/// Tüm dolu slot'ları ve kartlarını döndürür
	/// </summary>
	public Dictionary<CardSlot, Card3D> GetAllPlacedCards(Array<CardSlot> allSlots)
	{
		var placedCards = new Dictionary<CardSlot, Card3D>();
		
		if (allSlots == null)
		{
			return placedCards;
		}
		
		foreach (var slot in allSlots)
		{
			if (slot != null && slot.IsOccupied)
			{
				var card = slot.GetPlacedCard();
				if (card != null)
				{
					placedCards[slot] = card;
				}
			}
		}
		
		return placedCards;
	}

	/// <summary>
	/// Belirli bir slot ve komşularındaki kart kombinasyonunu analiz eder
	/// </summary>
	public Array<Card3D> GetCardGroup(CardSlot centerSlot, Array<CardSlot> allSlots)
	{
		var cardGroup = new Array<Card3D>();
		
		// Merkez kart
		if (centerSlot != null && centerSlot.IsOccupied)
		{
			var centerCard = centerSlot.GetPlacedCard();
			if (centerCard != null)
			{
				cardGroup.Add(centerCard);
			}
		}
		
		// Komşu kartlar
		var neighborCards = GetNeighborCards(centerSlot, allSlots);
		foreach (var neighborCard in neighborCards)
		{
			if (!cardGroup.Contains(neighborCard))
			{
				cardGroup.Add(neighborCard);
			}
		}
		
		return cardGroup;
	}
}

