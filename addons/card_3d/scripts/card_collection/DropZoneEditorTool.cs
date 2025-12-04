using Godot;

[Tool]
public partial class DropZoneEditorTool : Node
{
	private static readonly Shape3D DefaultDropZoneShape3D = GD.Load<Shape3D>(
		"res://addons/card_3d/shapes_3d/default_card_collection_3d_drop_zone_shape_3d.tres"
	);

	[Export]
	public CardCollection3D CardCollection3D { get; set; }
	
	[Export]
	public CollisionShape3D DropZoneCollisionShape3D { get; set; }
	
	private Shape3D _currentShape3D;
	private float _currentZOffset;

	public override void _Ready()
	{
		if (Engine.IsEditorHint())
		{
			SetProcess(true);
			UpdateShape3D();
			UpdateZOffset();
		}
		else
		{
			SetProcess(false);
		}
	}

	public override void _Process(double delta)
	{
		if (CardCollection3D == null || DropZoneCollisionShape3D == null)
		{
			return;
		}
		
		if (_currentShape3D != CardCollection3D.DropzoneCollisionShape)
		{
			UpdateShape3D();
		}
		
		if (_currentZOffset != CardCollection3D.DropzoneZOffset)
		{
			UpdateZOffset();
		}
	}

	private void UpdateShape3D()
	{
		if (CardCollection3D.DropzoneCollisionShape != null)
		{
			_currentShape3D = CardCollection3D.DropzoneCollisionShape;
		}
		else
		{
			_currentShape3D = DefaultDropZoneShape3D;
		}
		DropZoneCollisionShape3D.Shape = _currentShape3D;
	}

	private void UpdateZOffset()
	{
		_currentZOffset = CardCollection3D.DropzoneZOffset;
		DropZoneCollisionShape3D.Position = new Vector3(
			DropZoneCollisionShape3D.Position.X,
			DropZoneCollisionShape3D.Position.Y,
			_currentZOffset
		);
	}
}
