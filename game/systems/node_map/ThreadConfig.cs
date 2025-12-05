using Godot;

[GlobalClass]
public partial class ThreadConfig : Resource
{
	[Export]
	public CardSlot.ThreadType ThreadType { get; set; } = CardSlot.ThreadType.Silk;

	[Export]
	public Color ThreadColor { get; set; } = Colors.White;

	[Export]
	public string ThreadName { get; set; } = "";

	[Export]
	public string Description { get; set; } = "";

	[Export]
	public int BonusDP { get; set; } = 0;  // Thread'e özel bonus DP

	[Export]
	public int BonusChaos { get; set; } = 0;  // Thread'e özel bonus Kaos

	[Export]
	public bool PreventsChaos { get; set; } = false;  // Altın İplik gibi Kaos'u engeller

	public ThreadConfig()
	{
	}
}




