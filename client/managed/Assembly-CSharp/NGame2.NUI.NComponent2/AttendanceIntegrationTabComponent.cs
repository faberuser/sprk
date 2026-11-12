using System;
using System.Collections.Generic;
using NGame2.NContentsInfo;
using NVespa;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NComponent2;

public class AttendanceIntegrationTabComponent : LinkFactoryComponent
{
	public interface IListener
	{
		void OnClickTab(AttendanceIntegrationTabComponent caller);
	}

	public enum MenuTagType
	{
		Default,
		InProgress,
		Purchaseable
	}

	[Serializable]
	public class TypeUIPair
	{
		public MenuTagType MenuTagType;

		public Transform UiWidget;
	}

	public const string PrefabPath = "Prefab2/Component/AttendanceIntegration/AttendanceIntegrationTabComponent";

	public static PrefabAuto<AttendanceIntegrationTabComponent> PrefabAuto = new PrefabAuto<AttendanceIntegrationTabComponent>("Prefab2/Component/AttendanceIntegration/AttendanceIntegrationTabComponent");

	public List<TypeUIPair> TypeUIPairList = new List<TypeUIPair>();

	public UIToggle ToggleTabType;

	public UILabel LabelTabName;

	[Header("-- [알림 마크(받을 보상이 있는가)]")]
	public UIWidget Widget_Notify_CanReceiveReward;

	private IListener _listener;

	public string Keyword { get; private set; }

	public AttendanceKeywordGroup AttendanceGroup { get; private set; }

	public void OpenWithAttendanceData(IListener listener, string keyword, AttendanceKeywordGroup attendanceGroup)
	{
		if (listener == null)
		{
			VespaDebug.LogError("listener == null");
			return;
		}
		_listener = listener;
		Keyword = keyword;
		AttendanceGroup = attendanceGroup;
		InitUi(AttendanceGroup.GetTitleString());
	}

	public void Open(IListener listener, string keyword, string name)
	{
		if (listener == null)
		{
			VespaDebug.LogError("listener == null");
			return;
		}
		_listener = listener;
		Keyword = keyword;
		AttendanceGroup = default(AttendanceKeywordGroup);
		InitUi(name);
	}

	public void RefreshUi()
	{
		if (AttendanceGroup.IsPurchasable_v1())
		{
			SetMenuTag(MenuTagType.Purchaseable);
		}
		else
		{
			SetMenuTag(MenuTagType.Default);
		}
		UIHelper.EnableGameObject(Widget_Notify_CanReceiveReward, AttendanceFacade.CanReceiveReward_Any(AttendanceGroup));
	}

	private void InitUi(string name)
	{
		SetName(name);
		SelectToggle(select: false);
		UIHelper.EnableGameObject(Widget_Notify_CanReceiveReward, AttendanceFacade.CanReceiveReward_Any(AttendanceGroup));
	}

	private void SetName(string name)
	{
		UIHelper.SetLabelText(LabelTabName, name);
	}

	public void SetMenuTag(MenuTagType menuType)
	{
		foreach (TypeUIPair typeUIPair in TypeUIPairList)
		{
			UIHelper.EnableGameObject(typeUIPair.UiWidget, typeUIPair.MenuTagType == menuType);
		}
	}

	public void OnClickToggle()
	{
		if (!(ToggleTabType == null) && ToggleTabType.value && _listener != null)
		{
			_listener.OnClickTab(this);
		}
	}

	public void SelectToggle(bool select)
	{
		ToggleTabType.Set(select);
	}
}
