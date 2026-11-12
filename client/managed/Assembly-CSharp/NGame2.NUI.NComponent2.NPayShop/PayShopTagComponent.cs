using NNguiExtension;
using NShared;
using NVespa.NGlobal;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NComponent2.NPayShop;

public class PayShopTagComponent : LinkFactoryComponent
{
	public interface IListener
	{
		void OnClickTag(int tag);

		void OnCloseTag();
	}

	public const string PrefabPath = "Prefab2/Component/PayShop2/PayShopTagComponent";

	public static PrefabAuto<PayShopTagComponent> PrefabAuto = new PrefabAuto<PayShopTagComponent>("Prefab2/Component/PayShop2/PayShopTagComponent");

	public const string Seleted_PrefabPath = "Prefab2/Component/PayShop2/PayShopSelectedTagComponent";

	public static PrefabAuto<PayShopTagComponent> Selected_PrefabAuto = new PrefabAuto<PayShopTagComponent>("Prefab2/Component/PayShop2/PayShopSelectedTagComponent");

	public const string Hot_PrefabPath = "Prefab2/Component/PayShop2/PayShopHotTagComponent";

	public static PrefabAuto<PayShopTagComponent> Hot_PrefabAuto = new PrefabAuto<PayShopTagComponent>("Prefab2/Component/PayShop2/PayShopHotTagComponent");

	public UILabel Label_Tag;

	public UIWidget Sprite_BG;

	public UIWidget Widget_Close;

	public UISpriteSelector UISpriteSelector;

	public Color SeletedTextColor = Color.black;

	public Color DefaultTextColor = Color.white;

	private string _tag;

	private NewPayShopTagData _tagData;

	private IListener _listener;

	public float Width { get; private set; }

	public NewPayShopTagData TagData => _tagData;

	public void Open(NewPayShopTagData tagData, IListener listener)
	{
		_listener = listener;
		_tagData = tagData;
		string stringDirect = LocalizationManager.GetStringDirect(tagData.Name);
		SetTag(stringDirect);
	}

	public void SetTag(string tag)
	{
		UIHelper.SetLabelText(Label_Tag, tag);
		CalcWidth();
	}

	public void SeletedTag(bool isOn)
	{
		if (UISpriteSelector != null)
		{
			UIHelper.SetLabelColor(Label_Tag, isOn ? SeletedTextColor : DefaultTextColor);
			UISpriteSelector.Select(isOn.ToString());
		}
	}

	public void OnClickTag()
	{
		if (_listener != null)
		{
			_listener.OnClickTag(_tagData.Index);
		}
	}

	public void OnClickClose()
	{
		if (_listener != null)
		{
			_listener.OnCloseTag();
		}
	}

	public void CalcWidth()
	{
		if (Label_Tag != null)
		{
			Label_Tag.ForceUpdateAnchors(includeChildren: true);
		}
		if (Sprite_BG != null)
		{
			Sprite_BG.ForceUpdateAnchors(includeChildren: true);
			Sprite_BG.ForceUpdateTransform();
			Sprite_BG.ResizeCollider();
		}
		if (Widget_Close != null)
		{
			Widget_Close.ForceUpdateAnchors(includeChildren: true);
		}
		Width = CalcWidth(this);
	}

	private float CalcWidth(MonoBehaviour mono)
	{
		return CalcWidth(mono.transform);
	}

	private float CalcWidth(Transform t)
	{
		return NGUIMath.CalculateRelativeWidgetBounds(t, considerInactive: true).size.x;
	}
}
