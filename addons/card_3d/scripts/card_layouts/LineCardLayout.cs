using Godot;
using Godot.Collections;

[GlobalClass]
public partial class LineCardLayout : CardLayout
{
	private float _maxWidth = 20.0f;
	[Export]
	public float MaxWidth
	{
		get => _maxWidth;
		set
		{
			_maxWidth = value;
			var halfWidth = _maxWidth / 2.0f;
			_start = new Vector3(-halfWidth, 0, 0);
			_end = new Vector3(halfWidth, 0, 0.1f);
		}
	}

	private Vector3 _start = new Vector3(-7, 0, 0);
	private Vector3 _end = new Vector3(7, 0, 0.1f);
	
	[Export]
	public float CardWidth { get; set; } = 2.5f;
	
	[Export]
	public float Padding { get; set; } = 0.5f;

	// Where the first card will be on the x axis
	private float GetHandStartX(float handWidth, float cardSize)
	{
		return (-handWidth / 2) + (cardSize / 2);
	}

	// How far apart to set each card
	private float GetCardOffset(int numCards, float cardSize)
	{
		// Calculate required space for cards with padding
		var totalCardSpace = cardSize * numCards;
		var totalPaddingSpace = (numCards - 1) * Padding;

		if (totalCardSpace + totalPaddingSpace <= MaxWidth)
		{
			// Cards fit within the available space without overlapping
			return cardSize + Padding;
		}
		else
		{
			// Cards need to overlap
			return (MaxWidth - cardSize) / (numCards - 1);
		}
	}

	public override Array<Vector3> CalculateCardPositions(int numCards)
	{
		var positions = new Array<Vector3>();
		var cardOffset = GetCardOffset(numCards, CardWidth);
		var handWidth = CardWidth + ((numCards - 1) * cardOffset);
		var startPos = GetHandStartX(handWidth, CardWidth);

		// Position each card
		for (int i = 0; i < numCards; i++)
		{
			var iPos = new Vector3(startPos + (i * cardOffset), 0, 0.001f * i);
			positions.Add(iPos);
		}

		return positions;
	}

	public override Vector3 CalculateCardPositionByIndex(int numCards, int index)
	{
		var cardOffset = GetCardOffset(numCards, CardWidth);
		var handWidth = CardWidth + ((numCards - 1) * cardOffset);
		var startPos = GetHandStartX(handWidth, CardWidth);

		return new Vector3(startPos + (index * cardOffset), 0, 0.001f * index);
	}
}

