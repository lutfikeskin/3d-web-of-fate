using Godot;

public partial class GameHUD : Control
{
	[Export]
	public NodePath GameStatePath { get; set; } = new NodePath("../Table/GameState");
	
	[Export]
	public NodePath DPLabelPath { get; set; } = new NodePath("VBoxContainer/DPContainer/DPLabel");
	
	[Export]
	public NodePath ChaosLabelPath { get; set; } = new NodePath("VBoxContainer/ChaosContainer/ChaosTitleContainer/ChaosLabel");
	
	[Export]
	public NodePath ChaosBarPath { get; set; } = new NodePath("VBoxContainer/ChaosContainer/ChaosBar");

	[Export]
	public NodePath TurnLabelPath { get; set; } = new NodePath("VBoxContainer/TurnLabel");

	[Export]
	public NodePath PhaseLabelPath { get; set; } = new NodePath("VBoxContainer/PhaseLabel");

	[Export]
	public NodePath EndTurnButtonPath { get; set; } = new NodePath("VBoxContainer/EndTurnButton");

	[Export]
	public NodePath DeckLabelPath { get; set; } = new NodePath("VBoxContainer/DeckLabel");

	[Export]
	public NodePath HandLabelPath { get; set; } = new NodePath("VBoxContainer/HandLabel");

	private Label _dpLabel;
	private Label _chaosLabel;
	private ProgressBar _chaosBar;
	private GameState _gameState;
	private Label _turnLabel;
	private Label _phaseLabel;
	private Button _endTurnButton;
	private Label _deckLabel;
	private Label _handLabel;

	private TurnManager _turnManager;
	private CardCollection3D _hand;
	private Deck _deck;
	private Table _table;

	public override void _Ready()
	{
		// Node referanslarını al
		_dpLabel = GetNodeOrNull<Label>(DPLabelPath);
		_chaosLabel = GetNodeOrNull<Label>(ChaosLabelPath);
		_chaosBar = GetNodeOrNull<ProgressBar>(ChaosBarPath);
		
		if (_dpLabel == null || _chaosLabel == null || _chaosBar == null)
		{
			GD.PrintErr("HUD node'ları bulunamadı! Scene'de doğru node yapısını oluşturduğundan emin ol.");
			return;
		}

		// Opsiyonel node'ları al veya oluştur
		_turnLabel = GetNodeOrNull<Label>(TurnLabelPath);
		_phaseLabel = GetNodeOrNull<Label>(PhaseLabelPath);
		_endTurnButton = GetNodeOrNull<Button>(EndTurnButtonPath);
		_deckLabel = GetNodeOrNull<Label>(DeckLabelPath);
		_handLabel = GetNodeOrNull<Label>(HandLabelPath);

		var vbox = GetNodeOrNull<VBoxContainer>("VBoxContainer");
		if (vbox != null)
		{
			if (_turnLabel == null)
			{
				_turnLabel = new Label { Name = "TurnLabel", Text = "Tur: 1" };
				vbox.AddChild(_turnLabel);
			}
			if (_phaseLabel == null)
			{
				_phaseLabel = new Label { Name = "PhaseLabel", Text = "Faz: Preparation" };
				vbox.AddChild(_phaseLabel);
			}
			if (_endTurnButton == null)
			{
				_endTurnButton = new Button { Name = "EndTurnButton", Text = "Turu Bitir" };
				vbox.AddChild(_endTurnButton);
			}
			if (_deckLabel == null)
			{
				_deckLabel = new Label { Name = "DeckLabel", Text = "Deste: ?" };
				vbox.AddChild(_deckLabel);
			}
			if (_handLabel == null)
			{
				_handLabel = new Label { Name = "HandLabel", Text = "El: ?" };
				vbox.AddChild(_handLabel);
			}
		}
		
		// GameState'i bul
		if (!GameStatePath.IsEmpty)
		{
			_gameState = GetNodeOrNull<GameState>(GameStatePath);
		}
		
		// Alternatif: Table'dan bul
		if (_gameState == null)
		{
			var table = GetNodeOrNull<Table>("../Table");
			if (table != null)
			{
				_gameState = table.GetNodeOrNull<GameState>("GameState");
			}
		}
		
		// Son çare: Scene tree'de ara
		if (_gameState == null)
		{
			_gameState = GetTree().GetFirstNodeInGroup("game_state") as GameState;
		}
		
		if (_gameState == null)
		{
			GD.PrintErr("GameState not found for HUD");
			return;
		}
		
		// GameState sinyallerini bağla
		_gameState.DPChanged += OnDPChanged;
		_gameState.ChaosChanged += OnChaosChanged;
		_gameState.ChaosMaxed += OnChaosMaxed;
		_gameState.TurnChanged += OnTurnChanged;

		// Table, TurnManager, Deck, Hand referanslarını bul
		_table = GetNodeOrNull<Table>("../Table");
		if (_table == null)
		{
			_table = GetParent()?.GetNodeOrNull<Table>("Table");
		}

		if (_table != null)
		{
			_turnManager = _table.GetNodeOrNull<TurnManager>("TurnManager");
			_deck = _table.GetNodeOrNull<Deck>("DeckSystem");
			_hand = _table.GetNodeOrNull<CardCollection3D>("DragController/Hand");
		}
		else
		{
			_turnManager = GetTree().GetFirstNodeInGroup("turn_manager") as TurnManager;
		}

		// TurnManager sinyalleri
		if (_turnManager != null)
		{
			_turnManager.PhaseChanged += OnPhaseChanged;
			_turnManager.TurnStarted += OnTurnStarted;
		}

		// End Turn butonu
		if (_endTurnButton != null)
		{
			_endTurnButton.Pressed += OnEndTurnPressed;
			_endTurnButton.Disabled = true; // Placement dışı kapalı
		}
		
		// İlk değerleri göster
		UpdateDP(_gameState.DP);
		UpdateChaos(_gameState.Chaos);
		UpdateTurn(_gameState.CurrentTurn);
		UpdatePhase(_turnManager != null ? _turnManager.CurrentPhase : TurnManager.TurnPhase.Preparation);
		UpdateDeckHandInfo();
	}

	private void OnDPChanged(int newDP)
	{
		UpdateDP(newDP);
	}

	private void OnChaosChanged(int newChaos)
	{
		UpdateChaos(newChaos);
	}

	private void OnChaosMaxed()
	{
		GD.Print("KIRILMA! Kaos 100'e ulaştı!");
		// İleride run sonlandırma mantığı buraya eklenecek
	}

	private void OnTurnChanged(int newTurn)
	{
		UpdateTurn(newTurn);
	}

	private void OnPhaseChanged(TurnManager.TurnPhase newPhase)
	{
		UpdatePhase(newPhase);
		UpdateDeckHandInfo();
		if (_endTurnButton != null)
		{
			_endTurnButton.Disabled = newPhase != TurnManager.TurnPhase.Placement;
		}
	}

	private void OnTurnStarted(int turnNumber)
	{
		UpdateTurn(turnNumber);
		UpdateDeckHandInfo();
	}

	private void OnEndTurnPressed()
	{
		_turnManager?.EndTurn();
	}

	private void UpdateDP(int dp)
	{
		if (_dpLabel != null)
		{
			_dpLabel.Text = dp.ToString();
		}
	}

	private void UpdateChaos(int chaos)
	{
		if (_chaosLabel != null)
		{
			_chaosLabel.Text = $"{chaos}/100";
		}
		
		if (_chaosBar != null)
		{
			_chaosBar.Value = chaos;
			
			// Kaos seviyesine göre renk değiştir
			var styleBox = _chaosBar.GetThemeStylebox("fill") as StyleBoxFlat;
			if (styleBox != null)
			{
				if (chaos < 50)
				{
					styleBox.BgColor = new Color(0.2f, 0.8f, 0.2f); // Yeşil
				}
				else if (chaos < 80)
				{
					styleBox.BgColor = new Color(0.8f, 0.8f, 0.2f); // Sarı
				}
				else
				{
					styleBox.BgColor = new Color(0.8f, 0.2f, 0.2f); // Kırmızı
				}
			}
		}
	}

	private void UpdateTurn(int turn)
	{
		if (_turnLabel != null)
		{
			_turnLabel.Text = $"Tur: {turn}";
		}
	}

	private void UpdatePhase(TurnManager.TurnPhase phase)
	{
		if (_phaseLabel != null)
		{
			_phaseLabel.Text = $"Faz: {phase}";
		}
	}

	private void UpdateDeckHandInfo()
	{
		if (_deck != null && _deckLabel != null)
		{
			_deckLabel.Text = $"Deste: {_deck.GetRemainingCount()}";
		}

		if (_hand != null && _handLabel != null)
		{
			_handLabel.Text = $"El: {_hand.Cards.Count}";
		}
	}
}

