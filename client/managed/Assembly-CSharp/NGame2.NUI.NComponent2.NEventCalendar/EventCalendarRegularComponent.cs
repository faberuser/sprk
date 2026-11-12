using System;
using System.Collections.Generic;
using NShared;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NComponent2.NEventCalendar;

public class EventCalendarRegularComponent : MonoBehaviour, InitOnCreate
{
	public const string PrefabPath = "Resources/Prefab2/Component/EventCalendar/EventCalendarRegularComponent";

	public static PrefabAuto<EventCalendarRegularComponent> PrefabAuto = new PrefabAuto<EventCalendarRegularComponent>("Resources/Prefab2/Component/EventCalendar/EventCalendarRegularComponent");

	public UIWidget SpriteRoot;

	public UILabel LabelTitle;

	public UILabel LabelDescription;

	public UIBaseGrid EventIconGrid;

	private EventCalendarData _data;

	private Action<EventCalendarRegularComponent> _onPress;

	private Action<EventCalendarRegularComponent> _onRelease;

	private List<RegularEventIconComponent> _regularIconComponentList = new List<RegularEventIconComponent>();

	public int EventId { get; set; }

	public void Open(EventCalendarData data, Action<EventCalendarRegularComponent> onPress, Action<EventCalendarRegularComponent> onRelease)
	{
		_data = data;
		_onPress = onPress;
		_onRelease = onRelease;
		Color color = NGUIText.ParseColor((data.Color != null) ? data.Color.Replace("#", "") : "DBDBB4", 0);
		EventCalendarLocalInfo localInfo = data.LocalInfo;
		if (localInfo != null)
		{
			UIHelper.SetLabelText(LabelTitle, localInfo.Title);
			UIHelper.SetLabelText(LabelDescription, localInfo.SimpleDescription);
			UIHelper.SetLabelColor(LabelTitle, color);
			UIHelper.EnableGameObject(LabelTitle, enable: true);
			UIHelper.EnableGameObject(LabelDescription, enable: true);
		}
		else
		{
			UIHelper.EnableGameObject(LabelTitle, enable: false);
			UIHelper.EnableGameObject(LabelDescription, enable: false);
		}
		SetupEventList();
		TooltipPrintInfo componentInParent = SpriteRoot.GetComponentInParent<TooltipPrintInfo>();
		if (componentInParent != null)
		{
			componentInParent.Mode = UITooltipTrigger.ETooltipTriggerMode.ClickAndTooltip;
		}
	}

	void InitOnCreate.OnCreate()
	{
	}

	void InitOnCreate.OnRelease()
	{
		_onPress = null;
		_onRelease = null;
		ClearIconComponent();
	}

	private void ClearIconComponent()
	{
		FactoryBase.ReleaseAndClearList(_regularIconComponentList);
	}

	public void OnPressComponent()
	{
		if (_onPress != null)
		{
			EventId = _data.Id;
			Vespa_System_Action_Extension_Method.fire(_onPress, this);
		}
	}

	public void OnReleaseComponent()
	{
		if (_onRelease != null)
		{
			EventId = 0;
			Vespa_System_Action_Extension_Method.fire(_onRelease, this);
		}
	}

	private void UpdateRegularIconListPosition()
	{
		EventIconGrid.RepositionGrid();
	}

	private void SetupEventList()
	{
		ClearIconComponent();
		if (_data.Regular != 0)
		{
			AddRegularEventIcon(_data);
		}
		UpdateRegularIconListPosition();
	}

	private void AddRegularEventIcon(EventCalendarData data)
	{
		if (!(EventIconGrid == null))
		{
			RegularEventIconComponent regularEventIconComponent = EventIconGrid.AddGridItem(RegularEventIconComponent.PrefabAuto);
			if (!(regularEventIconComponent == null))
			{
				regularEventIconComponent.Open(data.BonusType);
				_regularIconComponentList.Add(regularEventIconComponent);
			}
		}
	}
}
