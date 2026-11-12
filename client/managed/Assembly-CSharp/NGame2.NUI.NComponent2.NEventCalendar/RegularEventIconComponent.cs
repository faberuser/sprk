using System.Collections.Generic;
using NNguiExtension;
using NShared;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NComponent2.NEventCalendar;

public class RegularEventIconComponent : MonoBehaviour, InitOnCreate
{
	public const string PrefabPath = "Resources/Prefab2/Component/EventCalendar/RegularEventIconComponent";

	public static PrefabAuto<RegularEventIconComponent> PrefabAuto = new PrefabAuto<RegularEventIconComponent>("Resources/Prefab2/Component/EventCalendar/RegularEventIconComponent");

	public UISpriteSelector IconSelector;

	public UILabel LabelCount;

	public UIWidget Root;

	private int _eventId;

	private bool _isBundle;

	private Dictionary<int, EventBonusType> _eventBundleDictionary;

	public void Open(EventBonusType bonusType, int eventId = 0, int count = 0)
	{
		_eventId = eventId;
		_isBundle = false;
		if (bonusType == EventBonusType.None)
		{
			_eventId = 0;
			_isBundle = true;
			_eventBundleDictionary = new Dictionary<int, EventBonusType>();
			count = ((count > 9) ? 9 : count);
			string text = "+" + count;
			UIHelper.SetLabelText(LabelCount, text);
			UIHelper.EnableGameObject(IconSelector, enable: false);
			UIHelper.EnableGameObject(LabelCount, enable: true);
		}
		else
		{
			IconSelector.SelectEnum(bonusType);
			UIHelper.EnableGameObject(IconSelector, enable: true);
			UIHelper.EnableGameObject(LabelCount, enable: false);
		}
	}

	public void AddEvent(EventBonusType bonusType, int eventId = 0)
	{
		if (_eventBundleDictionary != null)
		{
			_eventBundleDictionary.Add(eventId, bonusType);
		}
	}

	public void SetHighlight(int eventId)
	{
		if (eventId == 0)
		{
			UIHelper.SetWidgetAlpha(Root, 1f);
		}
		else if (_eventId == eventId)
		{
			UIHelper.SetWidgetAlpha(Root, 1f);
		}
		else
		{
			UIHelper.SetWidgetAlpha(Root, 0.3f);
		}
		if (!_isBundle)
		{
			return;
		}
		EventBonusType value = EventBonusType.None;
		if (_eventBundleDictionary != null && _eventBundleDictionary.TryGetValue(eventId, out value))
		{
			if (value != EventBonusType.None)
			{
				UIHelper.SetWidgetAlpha(Root, 1f);
				IconSelector.SelectEnum(value);
				UIHelper.EnableGameObject(IconSelector, enable: true);
				UIHelper.EnableGameObject(LabelCount, enable: false);
			}
		}
		else
		{
			UIHelper.EnableGameObject(IconSelector, enable: false);
			UIHelper.EnableGameObject(LabelCount, enable: true);
		}
	}

	void InitOnCreate.OnCreate()
	{
	}

	void InitOnCreate.OnRelease()
	{
	}
}
