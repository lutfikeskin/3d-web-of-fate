using Godot;

/// <summary>
/// Kart üzerinde bilgileri gösteren 3D text sistem
/// CardMesh'in child'ı olarak eklenecek, CardMesh'in transform'una göre çalışacak
/// </summary>
[GlobalClass]
public partial class CardInfoDisplay : Node3D
{
	private Label3D _nameLabel;
	private Label3D _dpLabel;
	private Label3D _chaosLabel;
	private Label3D _descriptionLabel;
	private Label3D _tagLabel;

	public override void _Ready()
	{
		// Label3D node'larını oluştur veya bul
		SetupLabels();
		GD.Print("CardInfoDisplay: _Ready() called, labels should be set up");
	}

	private void SetupLabels()
	{
		// CardMesh'in transform'u: scale 5x ve döndürülmüş
		// Label3D'ler CardMesh'in local space'inde olacak
		// Kart mesh boyutu: 0.5 x 0.875 (PlaneMesh size)
		// CardMesh transform: scale 5x, rotation var
		// Label3D pozisyonları CardMesh'in local space'inde (0.5 x 0.875 boyutunda)
		
		// Kart ismi - üstte, büyük
		_nameLabel = GetNodeOrNull<Label3D>("NameLabel");
		if (_nameLabel == null)
		{
			_nameLabel = new Label3D();
			_nameLabel.Name = "NameLabel";
			AddChild(_nameLabel);
		}
		ConfigureLabel(_nameLabel, new Vector3(0, 0.4f, 0.01f), 100, 30, Colors.White);

		// DP değeri - sağ üst köşe
		_dpLabel = GetNodeOrNull<Label3D>("DPLabel");
		if (_dpLabel == null)
		{
			_dpLabel = new Label3D();
			_dpLabel.Name = "DPLabel";
			AddChild(_dpLabel);
		}
		ConfigureLabel(_dpLabel, new Vector3(0.15f, 0.35f, 0.01f), 80, 25, new Color(0.2f, 0.8f, 0.2f));

		// Kaos değeri - sol üst köşe
		_chaosLabel = GetNodeOrNull<Label3D>("ChaosLabel");
		if (_chaosLabel == null)
		{
			_chaosLabel = new Label3D();
			_chaosLabel.Name = "ChaosLabel";
			AddChild(_chaosLabel);
		}
		ConfigureLabel(_chaosLabel, new Vector3(-0.15f, 0.35f, 0.01f), 80, 25, new Color(0.8f, 0.2f, 0.2f));

		// Tag - üstte, ismin altında
		_tagLabel = GetNodeOrNull<Label3D>("TagLabel");
		if (_tagLabel == null)
		{
			_tagLabel = new Label3D();
			_tagLabel.Name = "TagLabel";
			AddChild(_tagLabel);
		}
		ConfigureLabel(_tagLabel, new Vector3(0, 0.3f, 0.01f), 60, 20, Colors.White);

		// Açıklama - altta, küçük
		_descriptionLabel = GetNodeOrNull<Label3D>("DescriptionLabel");
		if (_descriptionLabel == null)
		{
			_descriptionLabel = new Label3D();
			_descriptionLabel.Name = "DescriptionLabel";
			AddChild(_descriptionLabel);
		}
		ConfigureLabel(_descriptionLabel, new Vector3(0, -0.4f, 0.01f), 50, 15, Colors.White);
		_descriptionLabel.AutowrapMode = TextServer.AutowrapMode.WordSmart;
		_descriptionLabel.Width = 0.4f; // Kart boyutuna göre
	}

	private void ConfigureLabel(Label3D label, Vector3 position, int fontSize, int outlineSize, Color modulate)
	{
		label.Position = position;
		label.Billboard = BaseMaterial3D.BillboardModeEnum.Enabled;
		label.NoDepthTest = true; // Depth test kapalı - her zaman görünsün
		label.FontSize = fontSize;
		label.OutlineSize = outlineSize;
		label.Modulate = modulate;
		label.OutlineModulate = Colors.Black;
		label.PixelSize = 0.005f; // Daha büyük pixel size - görünürlük için
		label.RenderPriority = 100;
		label.Visible = true;
		label.Text = label.Text ?? "TEST"; // Test için varsayılan text
		GD.Print($"CardInfoDisplay: Configured label '{label.Name}' at position {position}, text: '{label.Text}', visible: {label.Visible}, pixelSize: {label.PixelSize}");
	}

	public void UpdateCardInfo(CardData cardData)
	{
		if (cardData == null)
		{
			GD.PrintErr("CardInfoDisplay: cardData is null!");
			HideAllLabels();
			return;
		}

		GD.Print($"CardInfoDisplay: Updating card info for {cardData.CardName}");

		// İsim
		if (_nameLabel != null)
		{
			_nameLabel.Text = cardData.CardName;
			_nameLabel.Visible = true;
			GD.Print($"CardInfoDisplay: Set name label text to '{_nameLabel.Text}', visible: {_nameLabel.Visible}");
		}
		else
		{
			GD.PrintErr("CardInfoDisplay: _nameLabel is null!");
		}

		// DP
		if (_dpLabel != null)
		{
			_dpLabel.Text = $"DP: {cardData.BaseDP}";
			_dpLabel.Visible = true;
			GD.Print($"CardInfoDisplay: Set DP label text to '{_dpLabel.Text}'");
		}

		// Kaos
		if (_chaosLabel != null)
		{
			var chaosText = cardData.BaseChaos >= 0 ? $"+{cardData.BaseChaos}" : $"{cardData.BaseChaos}";
			_chaosLabel.Text = $"Kaos: {chaosText}";
			_chaosLabel.Visible = true;
			GD.Print($"CardInfoDisplay: Set Chaos label text to '{_chaosLabel.Text}'");
		}

		// Tag
		if (_tagLabel != null)
		{
			_tagLabel.Text = GetTagName(cardData.Tag);
			_tagLabel.Modulate = GetTagColor(cardData.Tag);
			_tagLabel.Visible = true;
			GD.Print($"CardInfoDisplay: Set Tag label text to '{_tagLabel.Text}'");
		}

		// Açıklama
		if (_descriptionLabel != null)
		{
			_descriptionLabel.Text = cardData.Description;
			_descriptionLabel.Visible = true;
			GD.Print($"CardInfoDisplay: Set Description label text to '{_descriptionLabel.Text}'");
		}
	}

	private string GetTagName(CardData.CardTag tag)
	{
		return tag switch
		{
			CardData.CardTag.Violence => "🔴 Vahşet",
			CardData.CardTag.Mystic => "🔵 Mistik",
			CardData.CardTag.Hope => "🟢 Umut",
			CardData.CardTag.Tragedy => "🟣 Trajedi",
			_ => tag.ToString()
		};
	}

	private Color GetTagColor(CardData.CardTag tag)
	{
		return tag switch
		{
			CardData.CardTag.Violence => new Color(0.8f, 0.2f, 0.2f), // Kırmızı
			CardData.CardTag.Mystic => new Color(0.2f, 0.4f, 0.8f), // Mavi
			CardData.CardTag.Hope => new Color(0.2f, 0.8f, 0.2f), // Yeşil
			CardData.CardTag.Tragedy => new Color(0.6f, 0.2f, 0.8f), // Mor
			_ => Colors.White
		};
	}

	private void HideAllLabels()
	{
		if (_nameLabel != null) _nameLabel.Visible = false;
		if (_dpLabel != null) _dpLabel.Visible = false;
		if (_chaosLabel != null) _chaosLabel.Visible = false;
		if (_tagLabel != null) _tagLabel.Visible = false;
		if (_descriptionLabel != null) _descriptionLabel.Visible = false;
	}
}

