using System;
using System.Collections.Generic;
using System.Text;
using NGame2.NContentsInfo;
using NGame2.NUI.NManager.Mission;
using NShared;
using NVespa;
using NVespa.NGlobal;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NComponent2;

public class ButtonSpecialEventComponent : LinkFactoryComponent, InitOnCreate
{
	public interface Listner
	{
		void OnClick(MissionCategoryData Data);
	}

	public const string PrefabAutoPath = "Resources/Prefab2/Component/MissionCategory/";

	public const string DefaultPrefabName = "ButtonSpecialEventComponent";

	private static Dictionary<string, PrefabAuto<ButtonSpecialEventComponent>> _dicPrefabAuto = new Dictionary<string, PrefabAuto<ButtonSpecialEventComponent>>();

	[Header("[ButtonSpecialEventComponent]")]
	public UILabel SpecialMissionLabel;

	public UILabel SpecialMissionRemainTimeLabel;

	public GameObject NewMarkObject;

	public GameObject RewardMarkObject;

	public Animator Animator;

	public string AnimatorLoopState;

	public string AnimatorRewardState;

	private Listner _listener;

	private float _lastUpdateTick = -9999f;

	public MissionCategoryData MissionCategoryData { get; set; }

	private static PrefabAuto<ButtonSpecialEventComponent> GetPrefabAuto(string prefabName)
	{
		if (String_Extension_Method.IsNullOrEmpty(prefabName))
		{
			prefabName = "ButtonSpecialEventComponent";
		}
		if (_dicPrefabAuto.TryGetValue(prefabName, out var value))
		{
			return value;
		}
		value = new PrefabAuto<ButtonSpecialEventComponent>(new StringBuilder("Resources/Prefab2/Component/MissionCategory/").Append(prefabName).ToString());
		_dicPrefabAuto.Add(prefabName, value);
		return value;
	}

	public static ButtonSpecialEventComponent CreateComponent(string prefabName, Transform parent)
	{
		if (parent == null)
		{
			return null;
		}
		PrefabAuto<ButtonSpecialEventComponent> prefabAuto = GetPrefabAuto(prefabName);
		if (prefabAuto.Factory == null)
		{
			prefabAuto = GetPrefabAuto("");
		}
		return prefabAuto.Factory.Create(parent);
	}

	void InitOnCreate.OnCreate()
	{
		Clear();
	}

	void InitOnCreate.OnRelease()
	{
		Clear();
	}

	private void Clear()
	{
		UIHelper.EnableGameObject(RewardMarkObject, enable: false);
		UIHelper.EnableGameObject(NewMarkObject, enable: false);
		UIHelper.EnableGameObject(SpecialMissionRemainTimeLabel, enable: false);
	}

	public void Open(MissionCategoryData Data, Listner listner)
	{
		MissionCategoryData = Data;
		_listener = listner;
		Init();
	}

	public bool IsTimeout()
	{
		if (MissionCategoryData == null)
		{
			return false;
		}
		if (!SingletonAuto<MissionStatus>.instance.TryGetRemainTime(MissionCategoryData, out var remainTimeSpan))
		{
			return false;
		}
		return remainTimeSpan <= TimeSpan.Zero;
	}

	public void Init()
	{
		SetupSpecialMissionButton();
	}

	public void Refresh()
	{
		SetupSpecialMissionButton();
	}

	public void Update()
	{
		if (Time.time - _lastUpdateTick > 1f)
		{
			_lastUpdateTick = Time.time;
			SetupRemainTime();
		}
	}

	private void SetupSpecialMissionButton()
	{
		if (MissionCategoryData != null)
		{
			string stringDirect = LocalizationManager.GetStringDirect(MissionCategoryData.IconLocalKey);
			UIHelper.SetLabelText(SpecialMissionLabel, stringDirect);
			if (SpecialMissionLabel != null)
			{
				Vector3 localPosition = SpecialMissionLabel.transform.localPosition;
				SpecialMissionLabel.pivot = UIWidget.Pivot.Bottom;
				SpecialMissionLabel.transform.localPosition = localPosition;
			}
			SetupRemainTime();
			SetIsNew();
		}
	}

	private void SetupRemainTime()
	{
		if (MissionCategoryData != null)
		{
			string remainTimeString_Short = SingletonAuto<MissionStatus>.instance.GetRemainTimeString_Short(MissionCategoryData);
			if (String_Extension_Method.IsNullOrEmpty(remainTimeString_Short))
			{
				UIHelper.EnableGameObject(SpecialMissionRemainTimeLabel, enable: false);
				return;
			}
			UIHelper.EnableGameObject(SpecialMissionRemainTimeLabel, enable: true);
			UIHelper.SetLabelText(SpecialMissionRemainTimeLabel, remainTimeString_Short);
		}
	}

	private void SetIsNew()
	{
		if (MissionCategoryData != null)
		{
			if (MissionManagement.IsNewSubCategory(MissionCategoryData.MainCategoryIndex, MissionCategoryData.SubCategoryIndex))
			{
				UIHelper.EnableGameObject(NewMarkObject, enable: true);
				UIHelper.EnableGameObject(RewardMarkObject, enable: false);
				return;
			}
			bool flag = false;
			flag |= MissionManagement.IsMissionCategoryRewardCheck(MissionCategoryData.MainCategoryIndex);
			UIHelper.EnableGameObject(RewardMarkObject, flag);
			UIHelper.EnableGameObject(NewMarkObject, enable: false);
			SetupAnimator(flag);
		}
	}

	private void SetupAnimator(bool Reward)
	{
		if (!(Animator == null) && Reward)
		{
			Animator.Play(AnimatorRewardState);
		}
	}

	public void OnClick()
	{
		if (_listener != null)
		{
			_listener.OnClick(MissionCategoryData);
		}
	}
}
