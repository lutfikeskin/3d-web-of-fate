using Godot;

[GlobalClass]
public partial class TurnManager : Node
{
	public enum TurnPhase
	{
		Preparation,
		Placement,
		Resolution,
		Cleanup,
		GameOver
	}

	[Signal]
	public delegate void PhaseChangedEventHandler(TurnPhase newPhase);

	[Signal]
	public delegate void TurnStartedEventHandler(int turnNumber);

	[Signal]
	public delegate void TurnEndedEventHandler(int turnNumber);

	[Export]
	public int StartingTurn { get; set; } = 1;

	private int _turnNumber = 0;
	private TurnPhase _currentPhase = TurnPhase.Preparation;

	public int TurnNumber => _turnNumber;
	public TurnPhase CurrentPhase => _currentPhase;

	public override void _Ready()
	{
		// Başlangıçta ilk turu başlat
		StartTurn();
	}

	public void StartTurn()
	{
		_turnNumber = Mathf.Max(StartingTurn, _turnNumber + 1);
		SetPhase(TurnPhase.Preparation);
		EmitSignal(SignalName.TurnStarted, _turnNumber);
	}

	public void EndTurn()
	{
		if (_currentPhase == TurnPhase.Placement)
		{
			NextPhase(); // Resolution'a geç
		}
	}

	public void NextPhase()
	{
		switch (_currentPhase)
		{
			case TurnPhase.Preparation:
				SetPhase(TurnPhase.Placement);
				break;
			case TurnPhase.Placement:
				SetPhase(TurnPhase.Resolution);
				break;
			case TurnPhase.Resolution:
				SetPhase(TurnPhase.Cleanup);
				break;
			case TurnPhase.Cleanup:
				EmitSignal(SignalName.TurnEnded, _turnNumber);
				StartTurn(); // Yeni tura geç
				break;
			case TurnPhase.GameOver:
				// Oyun bitti, faz geçişi yok
				break;
		}
	}

	public void SetGameOver()
	{
		SetPhase(TurnPhase.GameOver);
	}

	private void SetPhase(TurnPhase newPhase)
	{
		if (_currentPhase == newPhase)
		{
			return;
		}

		_currentPhase = newPhase;
		EmitSignal(SignalName.PhaseChanged, Variant.From(_currentPhase));
	}
}

