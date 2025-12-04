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

	private Label _dpLabel;
	private Label _chaosLabel;
	private ProgressBar _chaosBar;
	private GameState _gameState;

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
		
		// İlk değerleri göster
		UpdateDP(_gameState.DP);
		UpdateChaos(_gameState.Chaos);
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
}

