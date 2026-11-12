using System;
using System.Collections.Generic;
using NShared;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NComponent2.NEventCalendar;

public class EventCalendarOftenComponent : MonoBehaviour, InitOnCreate
{
	public const string PrefabPath = "Resources/Prefab2/Component/EventCalendar/EventCalendarOftenComponent";

	public static PrefabAuto<EventCalendarOftenComponent> PrefabAuto = new PrefabAuto<EventCalendarOftenComponent>("Resources/Prefab2/Component/EventCalendar/EventCalendarOftenComponent");

	public UIWidget Root;

	public UIWidget SpriteRoot;

	public UILabel LabelTitle;

	public UILabel LabelDescription;

	public UILabel LabelDetail;

	public UILabel LabelSimple;

	public UIBaseGrid EventIconGrid;

	public EventCalendarData eventData;

	private Action<EventCalendarOftenComponent> _onPress;

	private Action<EventCalendarOftenComponent> _onRelease;

	private Action<string, EventCalendarOftenComponent> _onClickDetail;

	private List<OftenEventIconComponent> _oftenIconComponentList = new List<OftenEventIconComponent>();

	private bool _isSimpleDesc;

	public int EventId { get; set; }

	public void Open(EventCalendarData data, Action<EventCalendarOftenComponent> onPress, Action<EventCalendarOftenComponent> onRelease, Action<string, EventCalendarOftenComponent> onClickDetail)
	{
		eventData = data;
		_onPress = onPress;
		_onRelease = onRelease;
		_onClickDetail = onClickDetail;
		_isSimpleDesc = true;
		EventCalendarLocalInfo localInfo = data.LocalInfo;
		if (localInfo != null)
		{
			UIHelper.SetLabelText(LabelTitle, localInfo.Title);
			UIHelper.SetLabelText(LabelDescription, localInfo.SimpleDescription);
			Color color = NGUIText.ParseColor((data.Color != null) ? data.Color.Replace("#", "") : "DBDBB4", 0);
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
		_onClickDetail = null;
		ClearIconComponent();
	}

	private void ClearIconComponent()
	{
		FactoryBase.ReleaseAndClearList(_oftenIconComponentList);
	}

	public void OnPressComponent()
	{
		if (_onPress != null)
		{
			EventId = eventData.Id;
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

	public void OnClickDetailComponent()
	{
		if (_onClickDetail != null)
		{
			string text = "";
			if (_isSimpleDesc)
			{
				text = ((eventData.LocalInfo != null) ? eventData.LocalInfo.Description : "");
				_isSimpleDesc = false;
			}
			else
			{
				text = ((eventData.LocalInfo != null) ? eventData.LocalInfo.SimpleDescription : "");
				_isSimpleDesc = true;
			}
			UIHelper.EnableGameObject(LabelDetail, _isSimpleDesc);
			UIHelper.EnableGameObject(LabelSimple, !_isSimpleDesc);
			Vespa_System_Action_Extension_Method.fire(_onClickDetail, text, this);
		}
	}

	private void UpdateEventIconListPosition()
	{
		EventIconGrid.RepositionGrid();
	}

	private void SetupEventList()
	{
		ClearIconComponent();
		AddOftenEventIcon(eventData);
		UpdateEventIconListPosition();
	}

	private void AddOftenEventIcon(EventCalendarData data)
	{
		if (!(EventIconGrid == null))
		{
			OftenEventIconComponent oftenEventIconComponent = EventIconGrid.AddGridItem(OftenEventIconComponent.PrefabAuto);
			if (!(oftenEventIconComponent == null))
			{
				oftenEventIconComponent.Open(data.BonusType, data.Color);
				_oftenIconComponentList.Add(oftenEventIconComponent);
			}
		}
	}

	public int GetHeight()
	{
		if (SpriteRoot == null)
		{
			return 0;
		}
		if (Root != null)
		{
			Root.height = SpriteRoot.height;
			Root.Update();
		}
		return SpriteRoot.height;
	}

	public void SetDesc(string desc)
	{
		UIHelper.SetLabelText(LabelDescription, desc);
		UIHelper.EnableGameObject(Root, enable: false);
		UIHelper.EnableGameObject(Root, enable: true);
	}
}
