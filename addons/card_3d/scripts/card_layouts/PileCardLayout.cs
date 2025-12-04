using Godot;
using Godot.Collections;

[GlobalClass]
public partial class PileCardLayout : CardLayout
{
	[Export]
	public float PileYOffset { get; set; } = 0;

	public override Array<Vector3> CalculateCardPositions(int numCards)
	{
		var positions = new Array<Vector3>();

		for (int i = 0; i < numCards; i++)
		{
			positions.Add(new Vector3(0, (numCards - i) * (-PileYOffset), 0.01f * i));
		}

		return positions;
	}

	public override Vector3 CalculateCardPositionByIndex(int numCards, int index)
	{
		return new Vector3(0, 0, 0.01f * index);
	}

	public override Array<Vector3> CalculateCardRotations(int numCards)
	{
		var rotations = new Array<Vector3>();

		for (int i = 0; i < numCards; i++)
		{
			rotations.Add(Vector3.Zero);
		}

		return rotations;
	}

	public override Vector3 CalculateCardRotationByIndex(int numCards, int index)
	{
		return Vector3.Zero;
	}
}

