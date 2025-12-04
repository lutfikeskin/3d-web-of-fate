using Godot;

[GlobalClass]
public partial class DragStrategy : Resource
{
	[ExportGroup("Default Behavior")]
	[Export]
	public bool CanSelect { get; set; } = true;
	
	[Export]
	public bool CanRemove { get; set; } = true;
	
	[Export]
	public bool CanReorder { get; set; } = true;
	
	[Export]
	public bool CanInsert { get; set; } = true;

	public virtual bool CanSelectCard(Card3D card, CardCollection3D toCollection)
	{
		return CanSelect;
	}

	public virtual bool CanRemoveCard(Card3D card, CardCollection3D toCollection)
	{
		return CanRemove;
	}

	public virtual bool CanReorderCard(Card3D card, CardCollection3D toCollection)
	{
		return CanReorder;
	}

	public virtual bool CanInsertCard(Card3D card, CardCollection3D toCollection, CardCollection3D fromCollection)
	{
		return CanInsert;
	}
}

