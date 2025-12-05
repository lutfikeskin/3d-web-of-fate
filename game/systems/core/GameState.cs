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

	[Signal]
	public delegate void TurnChangedEventHandler(int newTurn);

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

	private int _currentTurn = 1;
	[Export]
	public int CurrentTurn
	{
		get => _currentTurn;
		set
		{
			if (_currentTurn != value)
			{
				_currentTurn = value;
				EmitSignal(SignalName.TurnChanged, _currentTurn);
			}
		}
	}

	[Export]
	public int MaxTurns { get; set; } = 0; // 0 -> limitsiz

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
		CurrentTurn = 1;
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



