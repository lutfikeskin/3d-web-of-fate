using Godot;

[GlobalClass]
public partial class FateCard3D : Card3D
{
	private CardData _cardData;
	
	[Export]
	public CardData CardData
	{
		get => _cardData;
		set
		{
			_cardData = value;
			UpdateCardVisuals();
		}
	}

	public override void _Ready()
	{
		base._Ready();
		UpdateCardVisuals();
	}

	private void UpdateCardVisuals()
	{
		if (_cardData == null)
		{
			return;
		}

		// Eğer kart görseli yolu varsa, material'ı yükle
		if (!string.IsNullOrEmpty(_cardData.ArtPath))
		{
			var material = GD.Load<Material>(_cardData.ArtPath);
			if (material != null)
			{
				var cardFrontMesh = GetNodeOrNull<MeshInstance3D>("CardMesh/CardFrontMesh");
				if (cardFrontMesh != null)
				{
					cardFrontMesh.SetSurfaceOverrideMaterial(0, material);
				}
			}
		}
	}

	public override string ToString()
	{
		return _cardData != null ? _cardData.CardName : "Unknown Card";
	}
}

