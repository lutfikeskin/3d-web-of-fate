using Godot;

[GlobalClass]
public partial class CardSlot : Node3D
{
	[Signal]
	public delegate void CardPlacedEventHandler(Card3D card);
	
	[Signal]
	public delegate void CardRemovedEventHandler(Card3D card);

	public bool IsOccupied => _placedCard != null;

	private Card3D _placedCard;
	private MeshInstance3D _slotVisual;
	private Area3D _dropArea;
	private StandardMaterial3D _slotMaterial; // Slot'un kendi material'ı (diğer slotlardan bağımsız)

	public override void _Ready()
	{
		// Scene'deki mesh'i al veya oluştur
		_slotVisual = GetNodeOrNull<MeshInstance3D>("SlotVisual");
		if (_slotVisual == null)
		{
			CreateSlotVisual();
		}
		
		// ÖNEMLİ: Material'ı kopyala, böylece diğer slotlar etkilenmez
		// Scene'deki slotlar aynı material'ı paylaşıyor olabilir
		var existingMaterial = _slotVisual.MaterialOverride as StandardMaterial3D;
		if (existingMaterial != null)
		{
			_slotMaterial = new StandardMaterial3D();
			_slotMaterial.Transparency = existingMaterial.Transparency;
			_slotMaterial.ShadingMode = existingMaterial.ShadingMode;
			_slotMaterial.EmissionEnabled = existingMaterial.EmissionEnabled;
			_slotMaterial.CullMode = existingMaterial.CullMode;
			_slotMaterial.AlbedoColor = existingMaterial.AlbedoColor;
			_slotMaterial.Emission = existingMaterial.Emission;
		}
		else
		{
			// Material yoksa yeni bir tane oluştur
			_slotMaterial = new StandardMaterial3D();
			_slotMaterial.Transparency = BaseMaterial3D.TransparencyEnum.Alpha;
			_slotMaterial.ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded;
			_slotMaterial.EmissionEnabled = true;
			_slotMaterial.CullMode = BaseMaterial3D.CullModeEnum.Disabled;
			_slotMaterial.AlbedoColor = new Color(0.3f, 0.5f, 0.8f, 0.6f);
			_slotMaterial.Emission = new Color(0.1f, 0.3f, 0.6f, 1.0f);
		}
		
		_slotVisual.MaterialOverride = _slotMaterial;
		
		// Scene'deki drop area'yı al veya oluştur
		_dropArea = GetNodeOrNull<Area3D>("DropArea");
		if (_dropArea == null)
		{
			CreateDropArea();
		}
		else
		{
			// Scene'deki area'nın sinyallerini bağla
			_dropArea.InputEvent += OnAreaInputEvent;
			// MouseEntered ve MouseExited sinyallerini BAĞLAMA - Table kontrol edecek
			// _dropArea.MouseEntered += OnMouseEntered;
			// _dropArea.MouseExited += OnMouseExited;
		}
	}

	private void CreateSlotVisual()
	{
		// Debug amaçlı box mesh oluştur
		var boxMesh = new BoxMesh();
		boxMesh.Size = new Vector3(3.2f, 0.1f, 4.7f); // Kart boyutundan biraz büyük, düşük yükseklik
		
		_slotVisual = new MeshInstance3D();
		_slotVisual.Mesh = boxMesh;
		
		// Slot material - şeffaf, kenarlıklı, daha belirgin
		var material = new StandardMaterial3D();
		material.AlbedoColor = new Color(0.3f, 0.5f, 0.8f, 0.6f); // Mavi ton
		material.Transparency = BaseMaterial3D.TransparencyEnum.Alpha;
		material.ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded;
		material.EmissionEnabled = true;
		material.Emission = new Color(0.1f, 0.3f, 0.6f, 1.0f); // Parlayan mavi
		material.CullMode = BaseMaterial3D.CullModeEnum.Disabled; // Çift taraflı görünür
		_slotVisual.MaterialOverride = material;
		
		// Slot'u yatay düzleme yerleştir
		_slotVisual.Position = new Vector3(0, 0.05f, 0);
		
		AddChild(_slotVisual);
		
		// Debug amaçlı kenar çizgileri için wireframe görünüm (opsiyonel)
		// Alternatif olarak daha belirgin bir görünüm için outline eklenebilir
	}

	private void CreateDropArea()
	{
		// Drop area için Area3D kullan
		var area = new Area3D();
		
		// Collision shape - kartları yakalayabilmek için
		var collisionShape = new CollisionShape3D();
		var boxShape = new BoxShape3D();
		boxShape.Size = new Vector3(3.5f, 0.1f, 4.8f);
		collisionShape.Shape = boxShape;
		collisionShape.Position = new Vector3(0, 0.05f, 0);
		
		area.AddChild(collisionShape);
		
		// Input event için mouse detection
		area.InputEvent += OnAreaInputEvent;
		// MouseEntered ve MouseExited sinyallerini BAĞLAMA - Table kontrol edecek
		// area.MouseEntered += OnMouseEntered;
		// area.MouseExited += OnMouseExited;
		
		AddChild(area);
		_dropArea = area;
	}
	
	private void OnAreaInputEvent(Node camera, InputEvent @event, Vector3 eventPosition, Vector3 normal, long shapeIdx)
	{
		if (@event is InputEventMouseButton mouseEvent && mouseEvent.Pressed && mouseEvent.ButtonIndex == MouseButton.Left)
		{
			// Slot'ta kart varsa, önce düzgünce RemoveCard() çağır
			// RemoveCard hem parent'ı değiştirir hem de sinyal yollar
			if (_placedCard != null)
			{
				RemoveCard();
			}
		}
	}
	
	private void OnMouseEntered()
	{
		// Mouse highlight'ı devre dışı - sadece Table'dan kontrol edilecek
		// Drag sırasında çakışmayı önlemek için
		// Highlight(false);
	}
	
	private void OnMouseExited()
	{
		// Mouse highlight'ı devre dışı - sadece Table'dan kontrol edilecek
		// Highlight(false);
	}

	public bool CanPlaceCard(Card3D card)
	{
		return !IsOccupied && card != null;
	}

	public bool PlaceCard(Card3D card)
	{
		if (!CanPlaceCard(card))
		{
			return false;
		}

		_placedCard = card;
		
		// Kartın mevcut parent'ını al
		var currentParent = card.GetParent();
		
		// ÖNEMLİ: Kartın mevcut global pozisyonunu parent değiştirmeden ÖNCE al
		Vector3 cardCurrentGlobalPos = card.IsInsideTree() ? card.GlobalPosition : Vector3.Zero;
		Vector3 cardScale = card.IsInsideTree() ? card.Scale : Vector3.One;
		
		// Slot'un hedef global position'ı (tam yüzeyde)
		var targetGlobalPosition = GlobalPosition;
		
		// Kartı slot'un child'ı yap
		if (currentParent != this)
		{
			// Önce parent'tan çıkar
			if (currentParent != null)
			{
				currentParent.RemoveChild(card);
			}
			
			// Slot'a ekle
			AddChild(card);
			card.Visible = true;
			card.Scale = cardScale;
			
			// Kartı slot/table düzlemine paralel, yüzü yukarı bakacak şekilde düz yatır
			// Slot'un global basis'ini alıp X ekseninde -90° döndürerek kullan
			var flatBasis = GlobalBasis * new Basis(Vector3.Right, -Mathf.Pi * 0.5f);
			card.GlobalBasis = flatBasis;
			
			// Kartın mevcut pozisyonunu koru (animasyon elinden başlasın)
			if (cardCurrentGlobalPos != Vector3.Zero)
			{
				card.Position = ToLocal(cardCurrentGlobalPos);
			}
		}
		
		// Hedef pozisyonu local'a çevir
		var targetLocalPosition = ToLocal(targetGlobalPosition);
		
		// Kartı slot'a animasyonlu olarak yerleştir (daha sakin, zıplamasız)
		var tween = card.CreateTween();
		tween.SetEase(Tween.EaseType.Out);
		tween.SetTrans(Tween.TransitionType.Cubic);
		tween.TweenProperty(card, "position", targetLocalPosition, 0.25f);
		
		// Kart state: bulunduğu slot bilgisini kart'a yaz
		card.SetMeta("slot", this);
		card.SetMeta("in_slot", true);
		
		EmitSignal(SignalName.CardPlaced, card);
		return true;
	}

	public Card3D RemoveCard()
	{
		if (_placedCard == null)
		{
			return null;
		}

		var card = _placedCard;
		_placedCard = null;
		
		// Kartı slot'tan çıkar (parent'ını değiştir)
		// ÖNEMLİ: Bu işlem sinyal göndermeden ÖNCE yapılmalı
		// Çünkü sinyal handler'ı kartı başka bir parent'a ekleyecek
		if (card != null && card.GetParent() == this)
		{
			RemoveChild(card);
		}
		
		// Kart state: slot bilgisini temizle
		if (card != null)
		{
			if (card.HasMeta("slot"))
			{
				card.RemoveMeta("slot");
			}
			if (card.HasMeta("in_slot"))
			{
				card.RemoveMeta("in_slot");
			}
		}
		
		// Slot'u resetle: highlight'ı kaldır ve state'i temizle
		Highlight(false);
		
		// Sinyali gönder (kart artık slot'un child'ı değil)
		EmitSignal(SignalName.CardRemoved, card);
		return card;
	}

	public Card3D GetPlacedCard()
	{
		return _placedCard;
	}

	public void Highlight(bool highlight)
	{
		if (_slotMaterial == null)
		{
			return;
		}
		
		if (highlight)
		{
			_slotMaterial.AlbedoColor = new Color(0.2f, 0.8f, 0.2f, 0.7f); // Yeşil highlight - daha belirgin
			_slotMaterial.Emission = new Color(0.1f, 0.6f, 0.1f, 1.0f); // Parlayan yeşil
		}
		else
		{
			_slotMaterial.AlbedoColor = new Color(0.3f, 0.5f, 0.8f, 0.6f); // Normal mavi renk
			_slotMaterial.Emission = new Color(0.1f, 0.3f, 0.6f, 1.0f); // Normal parlayan mavi
		}
	}

	public bool IsPointInSlot(Vector3 globalPosition)
	{
		// Slot'un global pozisyonuna göre mesafe kontrolü
		float distance = GlobalPosition.DistanceTo(globalPosition);
		return distance < 2.5f; // Slot yarıçapı
	}
}
