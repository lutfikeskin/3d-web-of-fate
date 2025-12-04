using Godot;

[GlobalClass]
public partial class GameState : Node
{
	[Signal]
	public delegate void DPChangedEventHandler(int newDP);

	[Signal]
	public delegate void ChaosChangedEventHandler(int newChaos);

	[Signal]
	public delegate void ChaosMaxedEventHandler();  // Kaos 100'e ulaştığında

	private int _dp = 0;
	[Export]
	public int DP
	{
		get => _dp;
		set
		{
			if (_dp != value)
			{
				_dp = value;
				EmitSignal(SignalName.DPChanged, _dp);
			}
		}
	}

	private int _chaos = 0;
	[Export]
	public int Chaos
	{
		get => _chaos;
		set
		{
			var oldChaos = _chaos;
			_chaos = Mathf.Clamp(value, 0, 100);
			
			if (oldChaos != _chaos)
			{
				EmitSignal(SignalName.ChaosChanged, _chaos);
				
				// Kaos 100'e ulaştığında sinyal gönder (Kırılma)
				if (_chaos >= 100)
				{
					EmitSignal(SignalName.ChaosMaxed);
				}
			}
		}
	}

	[Export]
	public int StartingDP { get; set; } = 0;

	[Export]
	public int StartingChaos { get; set; } = 0;

	public override void _Ready()
	{
		Reset();
	}

	public void Reset()
	{
		DP = StartingDP;
		Chaos = StartingChaos;
	}

	public void AddDP(int amount)
	{
		DP += amount;
	}

	public void AddChaos(int amount)
	{
		Chaos += amount;
	}

	public void RemoveDP(int amount)
	{
		DP = Mathf.Max(0, DP - amount);
	}

	public void RemoveChaos(int amount)
	{
		Chaos -= amount;  // Clamp zaten Chaos setter'ında yapılıyor
	}

	public bool IsChaosMaxed()
	{
		return Chaos >= 100;
	}
}

