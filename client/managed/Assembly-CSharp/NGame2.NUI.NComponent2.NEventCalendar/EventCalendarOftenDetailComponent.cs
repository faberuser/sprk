using System.Collections.Generic;
using NShared;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NComponent2.NEventCalendar;

public class EventCalendarOftenDetailComponent : MonoBehaviour, InitOnCreate
{
	public const string PrefabPath = "Resources/Prefab2/Component/EventCalendar/EventCalendarOftenDetailComponent";

	public static PrefabAuto<EventCalendarOftenDetailComponent> PrefabAuto = new PrefabAuto<EventCalendarOftenDetailComponent>("Resources/Prefab2/Component/EventCalendar/EventCalendarOftenDetailComponent");

	public Transform Root;

	public UILabel LabelTitle;

	public UILabel LabelDescription;

	public UIPanel PanelSimple;

	public UIPanel PanelDetail;

	private EventCalendarData _data;

	private List<RegularEventIconComponent> _regularIconComponentList = new List<RegularEventIconComponent>();

	public void Open(EventCalendarData data)
	{
		_data = data;
		EventCalendarLocalInfo localInfo = data.LocalInfo;
		if (localInfo != null)
		{
			UIHelper.SetLabelText(LabelTitle, localInfo.Title);
			UIHelper.SetLabelText(LabelDescription, localInfo.Description);
			Color color = NGUIText.ParseColor((data.Color != null) ? data.Color.Replace("#", "") : "DBDBB4", 0);
			UIHelper.SetLabelColor(LabelTitle, color);
		}
		else
		{
			UIHelper.EnableGameObject(LabelTitle, enable: false);
			UIHelper.EnableGameObject(LabelDescription, enable: false);
		}
	}

	void InitOnCreate.OnCreate()
	{
	}

	void InitOnCreate.OnRelease()
	{
	}

	public void OnClickClose()
	{
		UIHelper.EnableGameObject(Root, enable: false);
	}

	public void OnClickUrl()
	{
	}
}
