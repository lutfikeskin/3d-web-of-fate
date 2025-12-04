using Godot;

[GlobalClass]
public partial class Card3D : Node3D
{
	[Signal]
	public delegate void Card3DMouseDownEventHandler();
	
	[Signal]
	public delegate void Card3DMouseUpEventHandler();
	
	[Signal]
	public delegate void Card3DMouseOverEventHandler();
	
	[Signal]
	public delegate void Card3DMouseExitEventHandler();

	[Export]
	public float HoverScaleFactor { get; set; } = 1.15f;
	
	[Export]
	public Vector3 HoverPosMove { get; set; } = new Vector3(0, 0.7f, 0);
	
	[Export]
	public float MoveTweenDuration { get; set; } = 0.08f;
	
	[Export]
	public float RotateTweenDuration { get; set; } = 0.15f;

	private bool _faceDown = false;
	[Export]
	public bool FaceDown
	{
		get => _faceDown;
		set
		{
			_faceDown = value;
			var cardMesh = GetNode<Node3D>("CardMesh");
			if (_faceDown)
			{
				cardMesh.Rotation = new Vector3(cardMesh.Rotation.X, Mathf.Pi, cardMesh.Rotation.Z);
			}
			else
			{
				cardMesh.Rotation = new Vector3(cardMesh.Rotation.X, 0, cardMesh.Rotation.Z);
			}
		}
	}

	private Tween _positionTween;
	private Tween _rotateTween;
	private Tween _hoverTween;

	public void DisableCollision()
	{
		var collisionShape = GetNode<CollisionShape3D>("StaticBody3D/CollisionShape3D");
		collisionShape.Disabled = true;
	}

	public void EnableCollision()
	{
		var collisionShape = GetNode<CollisionShape3D>("StaticBody3D/CollisionShape3D");
		collisionShape.Disabled = false;
	}

	public void SetHovered()
	{
		if (_hoverTween != null && _hoverTween.IsValid())
		{
			_hoverTween.Kill();
		}

		_hoverTween = CreateTween();
		_hoverTween.SetParallel(true);
		_hoverTween.SetEase(Tween.EaseType.EaseIn);
		TweenCardScale(HoverScaleFactor);
		TweenMeshPosition(HoverPosMove, MoveTweenDuration);
	}

	public void RemoveHovered()
	{
		if (_hoverTween != null && _hoverTween.IsValid())
		{
			_hoverTween.Kill();
		}

		_hoverTween = CreateTween();
		_hoverTween.SetParallel(true);
		_hoverTween.SetEase(Tween.EaseType.EaseIn);
		TweenCardScale(1.0f);
		TweenMeshPosition(Vector3.Zero, MoveTweenDuration);
	}

	public void DraggingRotation(Vector3 dragRotation)
	{
		if (_rotateTween != null && _rotateTween.IsValid())
		{
			_rotateTween.Kill();
		}

		_rotateTween = CreateTween();
		TweenCardRotation(dragRotation, RotateTweenDuration);
	}

	public Tween AnimateToPosition(Vector3 newPosition, float duration = -1)
	{
		if (duration < 0)
		{
			duration = MoveTweenDuration;
		}

		if (_positionTween != null && _positionTween.IsValid())
		{
			_positionTween.Kill();
		}

		// Set z to prevent transition spring from making card go below another card
		Position = new Vector3(Position.X, Position.Y, newPosition.Z);
		_positionTween = CreateTween();
		_positionTween.SetEase(Tween.EaseType.EaseOut);
		_positionTween.SetTrans(Tween.TransitionType.Spring);
		TweenCardPosition(newPosition, duration);
		return _positionTween;
	}

	private void TweenCardScale(float scaleFactor)
	{
		var targetScale = new Vector3(scaleFactor, scaleFactor, 1);
		_hoverTween.TweenProperty(this, "scale", targetScale, MoveTweenDuration);
	}

	private void TweenMeshPosition(Vector3 pos, float duration)
	{
		var cardMesh = GetNode<Node3D>("CardMesh");
		_hoverTween.TweenProperty(cardMesh, "position", pos, duration);
	}

	private void TweenCardPosition(Vector3 pos, float duration)
	{
		_positionTween.TweenProperty(this, "position", pos, duration);
	}

	private void TweenCardRotation(Vector3 targetRotation, float duration)
	{
		_rotateTween.SetEase(Tween.EaseType.EaseIn);
		_rotateTween.TweenProperty(this, "rotation", targetRotation, duration);
	}

	private void OnStaticBody3DMouseEntered()
	{
		EmitSignal(SignalName.Card3DMouseOver);
	}

	private void OnStaticBody3DMouseExited()
	{
		EmitSignal(SignalName.Card3DMouseExit);
	}

	private void OnStaticBody3DInputEvent(Node camera, InputEvent @event, Vector3 eventPosition, Vector3 normal, long shapeIdx)
	{
		if (@event is InputEventMouseButton mouseEvent)
		{
			var button = mouseEvent.ButtonIndex;
			var pressed = mouseEvent.Pressed;
			if (button == MouseButton.Left && pressed == true)
			{
				EmitSignal(SignalName.Card3DMouseDown);
			}
			else if (button == MouseButton.Left && pressed == false)
			{
				EmitSignal(SignalName.Card3DMouseUp);
			}
		}
	}
}
