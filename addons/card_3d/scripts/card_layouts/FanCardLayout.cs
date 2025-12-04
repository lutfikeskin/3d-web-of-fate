using Godot;
using Godot.Collections;

[GlobalClass]
public partial class FanCardLayout : CardLayout
{
	public enum FanDirection
	{
		Normal,
		Reverse
	}

	[Export]
	public FanDirection Direction { get; set; } = FanDirection.Normal;

	private float _arcAngleDeg = 90.0f;
	[Export]
	public float ArcAngleDeg
	{
		get => _arcAngleDeg;
		set
		{
			_arcAngleDeg = value;
			_startAngle = Mathf.Pi / 2 + (Mathf.DegToRad(_arcAngleDeg) / 2);
		}
	}

	[Export]
	public float ArcRadius { get; set; } = 7.0f;

	private float _startAngle = Mathf.Pi / 2 + (Mathf.DegToRad(90.0f) / 2);

	public override Array<Vector3> CalculateCardPositions(int numCards)
	{
		var angleStep = Mathf.DegToRad(ArcAngleDeg) / (numCards + 1);
		var positions = new Array<Vector3>();

		for (int i = 1; i <= numCards; i++)
		{
			var angle = _startAngle - (i * angleStep);
			var x = ArcRadius * Mathf.Cos(angle);
			var y = (ArcRadius * Mathf.Sin(angle)) - ArcRadius;
			var position = GetArcPosition(x, y, i);
			positions.Add(position);
		}

		return positions;
	}

	public override Vector3 CalculateCardPositionByIndex(int numCards, int index)
	{
		var angleStep = Mathf.DegToRad(ArcAngleDeg) / (numCards + 1);
		var angle = _startAngle - ((index + 1) * angleStep);
		var x = ArcRadius * Mathf.Cos(angle);
		var y = (ArcRadius * Mathf.Sin(angle)) - ArcRadius;
		var position = GetArcPosition(x, y, index);
		return position;
	}

	public override Array<Vector3> CalculateCardRotations(int numCards)
	{
		var rotations = new Array<Vector3>();
		var angleStep = Mathf.DegToRad(ArcAngleDeg) / (numCards + 1);

		for (int i = 1; i <= numCards; i++)
		{
			var angle = _startAngle - (i * angleStep);
			var rotationQuat = GetRotationQuat(angle);
			rotations.Add(rotationQuat.GetEuler());
		}

		return rotations;
	}

	public override Vector3 CalculateCardRotationByIndex(int numCards, int index)
	{
		var angleStep = Mathf.DegToRad(ArcAngleDeg) / (numCards + 1);
		var angle = _startAngle - ((index + 1) * angleStep);
		var rotationQuat = GetRotationQuat(angle);
		return rotationQuat.GetEuler();
	}

	private Vector3 GetArcPosition(float x, float y, int i)
	{
		if (Direction == FanDirection.Normal)
		{
			return new Vector3(x, y, 0.001f * i);
		}

		return new Vector3(x, -y, 0.001f * i);
	}

	private Quaternion GetRotationQuat(float angle)
	{
		if (Direction == FanDirection.Normal)
		{
			return new Quaternion(new Vector3(0, 0, 1), angle - Mathf.Pi / 2);
		}

		return new Quaternion(new Vector3(0, 0, 1), -angle + Mathf.Pi / 2);
	}
}

