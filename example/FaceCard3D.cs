using Godot;
using Godot.Collections;

[GlobalClass]
public partial class FaceCard3D : Card3D
{
	private Dictionary _data;
	[Export]
	public Dictionary Data
	{
		get => _data;
		set
		{
			_data = value;
			if (_data != null)
			{
				if (_data.ContainsKey("rank"))
				{
					Rank = (FaceCards.Rank)_data["rank"];
				}

				if (_data.ContainsKey("suit"))
				{
					Suit = (FaceCards.Suit)_data["suit"];
				}

				if (_data.ContainsKey("front_material_path"))
				{
					FrontMaterialPath = (string)_data["front_material_path"];
				}

				if (_data.ContainsKey("back_material_path"))
				{
					BackMaterialPath = (string)_data["back_material_path"];
				}
			}
		}
	}

	[Export]
	public FaceCards.Rank Rank { get; set; } = FaceCards.Rank.Two;
	
	[Export]
	public FaceCards.Suit Suit { get; set; } = FaceCards.Suit.Diamond;

	private string _frontMaterialPath = "";
	[Export]
	public string FrontMaterialPath
	{
		get => _frontMaterialPath;
		set
		{
			_frontMaterialPath = value;
			if (!string.IsNullOrEmpty(_frontMaterialPath))
			{
				var material = GD.Load<Material>(_frontMaterialPath);

				if (material != null)
				{
					var cardFrontMesh = GetNode<MeshInstance3D>("CardMesh/CardFrontMesh");
					cardFrontMesh.SetSurfaceOverrideMaterial(0, material);
				}
			}
		}
	}

	private string _backMaterialPath = "";
	[Export]
	public string BackMaterialPath
	{
		get => _backMaterialPath;
		set
		{
			_backMaterialPath = value;
			if (!string.IsNullOrEmpty(_backMaterialPath))
			{
				var material = GD.Load<Material>(_backMaterialPath);

				if (material != null)
				{
					var cardBackMesh = GetNode<MeshInstance3D>("CardMesh/CardBackMesh");
					cardBackMesh.SetSurfaceOverrideMaterial(0, material);
				}
			}
		}
	}

	public override string ToString()
	{
		return Rank.ToString() + " of " + Suit.ToString();
	}
}

