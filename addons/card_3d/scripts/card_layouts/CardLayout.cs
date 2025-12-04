using Godot;
using Godot.Collections;

[GlobalClass]
public partial class CardLayout : Resource
{
	// Moves cards to where they belong in space
	public virtual void UpdateCardPositions(Array<Card3D> cards, float duration)
	{
		var positions = CalculateCardPositions(cards.Count);
		var rotations = CalculateCardRotations(cards.Count);

		for (int i = 0; i < cards.Count; i++)
		{
			var card = cards[i];
			if (card != null)
			{
				card.AnimateToPosition(positions[i], duration);
				card.DraggingRotation(rotations[i]);
			}
		}
	}

	public virtual void UpdateCardPosition(Card3D card, int numCards, int index, float duration)
	{
		var position = CalculateCardPositionByIndex(numCards, index);
		var rotation = CalculateCardRotationByIndex(numCards, index);
		card.AnimateToPosition(position, duration);
		card.DraggingRotation(rotation);
	}

	public virtual Array<Vector3> CalculateCardPositions(int numCards)
	{
		return new Array<Vector3>();
	}

	public virtual Vector3 CalculateCardPositionByIndex(int numCards, int index)
	{
		return Vector3.Zero;
	}

	public virtual Array<Vector3> CalculateCardRotations(int numCards)
	{
		var rotations = new Array<Vector3>();
		for (int i = 0; i < numCards; i++)
		{
			rotations.Add(CalculateCardRotationByIndex(numCards, i));
		}
		return rotations;
	}

	public virtual Vector3 CalculateCardRotationByIndex(int numCards, int index)
	{
		return Vector3.Zero;
	}

	/// <summary>
	/// Returns a vector representing the direction of card layout in local space.
	/// For example, if we layout the cards from left to right incrementing in the
	/// x direction we should return Vector3.Right
	/// </summary>
	public virtual Vector3 GetLayoutNormal()
	{
		return Vector3.Right;
	}
}
