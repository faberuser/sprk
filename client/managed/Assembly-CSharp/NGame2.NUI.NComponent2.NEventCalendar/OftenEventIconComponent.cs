using NNguiExtension;
using NShared;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NComponent2.NEventCalendar;

public class OftenEventIconComponent : MonoBehaviour, InitOnCreate
{
	public const string PrefabPath = "Resources/Prefab2/Component/EventCalendar/OftenEventIconComponent";

	public static PrefabAuto<OftenEventIconComponent> PrefabAuto = new PrefabAuto<OftenEventIconComponent>("Resources/Prefab2/Component/EventCalendar/OftenEventIconComponent");

	public UISpriteSelector IconSelector;

	public UISpriteSelector IconSelectorBg;

	public UIWidget Root;

	public UIWidget Sprite_Icon;

	private int _eventId;

	public void Open(EventBonusType bonusType, string color, int eventId = 0)
	{
		Color color2 = NGUIText.ParseColor((color != null) ? color.Replace("#", "") : "FFFFFF", 0);
		IconSelector.SelectEnum(bonusType);
		IconSelectorBg.SelectEnum(bonusType);
		Sprite_Icon.color = color2;
		_eventId = eventId;
	}

	public void SetHighlight(int eventId)
	{
		if (eventId == 0)
		{
			SetGrayScale();
		}
		else
		{
			SetGrayScale(_eventId != eventId);
		}
	}

	private void SetGrayScale(bool isGray = false)
	{
		if (isGray)
		{
			UIHelper.SetWidgetAlpha(Root, 0.3f);
		}
		else
		{
			UIHelper.SetWidgetAlpha(Root, 1f);
		}
	}

	void InitOnCreate.OnCreate()
	{
	}

	void InitOnCreate.OnRelease()
	{
	}
}
