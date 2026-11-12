using System;
using NGame2.NAccount;
using NGame2.NTutorial;
using NGame2.NUI.NManager;
using NGame2.NUI.NWindow.NBattle;
using NShared;
using NVespa;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class TeamLevelupPopup : BaseWindow
{
	[Serializable]
	public class TeamLevelEffectLabels
	{
		public UILabel Label_MaxStamina;

		public UILabel Label_StaminaRecoverTime;

		public UILabel Label_GoldBonus;

		public UILabel Label_HeroExpBonus;

		public UILabel Label_DailyStaminaBonus;

		public UILabel Label_RoyalSupportGold;
	}

	public const string PrefabPath = "Resources/Prefab2/Window/MessagePopup/TeamLevelupPopup";

	public UILabel Label_CurTeamLevel;

	public TeamLevelEffectLabels PrevTeamLevelEffectLabels;

	public TeamLevelEffectLabels CurTeamLevelEffectLabels;

	public UIGame3DWidget EffectWidget;

	public string EffectPivotName;

	public float EffectDuration;

	public Transform PivotFollower;

	public Transform LabelsRoot;

	public UILabel Label_CloseMsg;

	private static WindowPrefab<TeamLevelupPopup> _windowPrefab;

	private Transform _pivotTransform;

	private int _prevTeamLevel;

	private int _curTeamLevel;

	private float _time;

	public static TeamLevelupPopup instance
	{
		get
		{
			if (_windowPrefab == null)
			{
				_windowPrefab = new WindowPrefab<TeamLevelupPopup>("Resources/Prefab2/Window/MessagePopup/TeamLevelupPopup", GameUIRoot.ELayerType.UI);
			}
			_windowPrefab.instance.gameObject.SetActive(value: false);
			return _windowPrefab.instance;
		}
	}

	public static void Open(int PrevTeamLevel, int CurTeamLevel)
	{
		if (Helper.CheckRestriction("TeamLevelupPopupOpenCondition"))
		{
			if (_windowPrefab == null)
			{
				_windowPrefab = new WindowPrefab<TeamLevelupPopup>("Resources/Prefab2/Window/MessagePopup/TeamLevelupPopup", GameUIRoot.ELayerType.UI);
			}
			_windowPrefab.instance.OpenInternal(PrevTeamLevel, CurTeamLevel);
			GlobalAutoBehaviour<PopupManegement>.instance.Add(_windowPrefab);
		}
	}

	private void OpenInternal(int PrevTeamLevel, int CurTeamLevel)
	{
		base.Open();
		if (CurTeamLevel % 10 == 0 && CurTeamLevel <= 50)
		{
			_ = (object)null;
			_ = 15;
			_ = 1;
			_ = 0;
			(new object[1])[0] = CurTeamLevel;
		}
		else
		{
			_ = (object)null;
			_ = 14;
			_ = 1;
			_ = 0;
			(new object[1])[0] = CurTeamLevel;
		}
		_prevTeamLevel = PrevTeamLevel;
		_curTeamLevel = CurTeamLevel;
		_time = 0f;
		UIHelper.SetLabelText(Label_CurTeamLevel, CurTeamLevel);
		UIHelper.EnableGameObject(LabelsRoot, enable: false);
		UIHelper.EnableGameObject(Label_CloseMsg, enable: false);
		if (EffectWidget != null)
		{
			EffectWidget.Play(null);
			UpdatePivotTransform();
		}
	}

	public override void UpdateDerived()
	{
		base.UpdateDerived();
		UpdatePivotTransform();
		UpdateShowLabels();
	}

	public override void OnClosing()
	{
		base.OnClosing();
		EffectWidget.Stop(immediatelyClear: true);
	}

	private void UpdateShowLabels()
	{
		if (UIHelper.IsEnabledGameObjet(LabelsRoot))
		{
			return;
		}
		_time += Time.deltaTime;
		if (_time >= EffectDuration)
		{
			UIHelper.EnableGameObject(LabelsRoot, enable: true);
			UIHelper.EnableGameObject(Label_CloseMsg, enable: true);
			SetTeamLevelLabels(_prevTeamLevel, _curTeamLevel);
			if (SingletonUpdater<TutorialManager>.instance.GetRunningState().AutoType == TutorialRunningState.TutorialAutoType.All || AutoRepeatBattle.IsEnabled() || AutoNextBattle.IsEnabled())
			{
				OnClickClose();
			}
		}
	}

	private void UpdatePivotTransform()
	{
		if (!(EffectWidget == null) && !(PivotFollower == null) && EffectWidget.IsPlaying)
		{
			if (_pivotTransform == null)
			{
				_pivotTransform = Vespa_Unity_Transform_Extension_Method.FindChildRecursively(EffectWidget.transform, EffectPivotName);
			}
			if (!(_pivotTransform == null))
			{
				PivotFollower.transform.position = _pivotTransform.transform.position;
			}
		}
	}

	private void SetTeamLevelLabels(int PrevTeamLevel, int CurTeamLevel)
	{
		TeamLevelData data = GameRoot.Instance.TeamLevelDataContainer.GetData(PrevTeamLevel);
		TeamLevelData data2 = GameRoot.Instance.TeamLevelDataContainer.GetData(CurTeamLevel);
		SetTeamLevelEffectLabels(data, PrevTeamLevelEffectLabels);
		SetTeamLevelEffectLabels(data2, CurTeamLevelEffectLabels);
	}

	public static void SetTeamLevelEffectLabels(TeamLevelData teamLevelData, TeamLevelEffectLabels labels)
	{
		if (labels != null)
		{
			if (teamLevelData == null)
			{
				string text = "-";
				UIHelper.SetLabelText(labels.Label_MaxStamina, text);
				UIHelper.SetLabelText(labels.Label_StaminaRecoverTime, text);
				UIHelper.SetLabelText(labels.Label_GoldBonus, text);
				UIHelper.SetLabelText(labels.Label_HeroExpBonus, text);
				UIHelper.SetLabelText(labels.Label_DailyStaminaBonus, text);
				UIHelper.SetLabelText(labels.Label_RoyalSupportGold, text);
			}
			else
			{
				UIHelper.SetLabelText(labels.Label_MaxStamina, teamLevelData.MaxStamina);
				string staminaRecoverTime = GetStaminaRecoverTime(teamLevelData);
				UIHelper.SetLabelText(labels.Label_StaminaRecoverTime, staminaRecoverTime);
				UIHelper.SetLabelText(labels.Label_GoldBonus, teamLevelData.GoldBonusPercentage + "%");
				UIHelper.SetLabelText(labels.Label_HeroExpBonus, teamLevelData.ExpBonusPercentage + "%");
				UIHelper.SetLabelText(labels.Label_DailyStaminaBonus, teamLevelData.DailyBonusStamina);
				UILabel label_RoyalSupportGold = labels.Label_RoyalSupportGold;
				int dailyGold = teamLevelData.DailyGold;
				UIHelper.SetLabelText(label_RoyalSupportGold, Vespa_Int_Extension_Method.ToCommaString(dailyGold));
			}
		}
	}

	private static string GetStaminaRecoverTime(TeamLevelData teamLevelData)
	{
		int num = 0;
		if (GlobalAutoBehaviour<TeamLevelBuffManager>.isValidInstance)
		{
			num = GlobalAutoBehaviour<TeamLevelBuffManager>.instance.GetTeamLevelBuffBonusValue(EventBonusType.DecRegenStaminaTime);
		}
		int num2 = teamLevelData.RechargeStaminaSec - num;
		int num3 = num2 / 60;
		int num4 = num2 % 60;
		return $"{num3}m {num4}s";
	}

	public void OnClickClose()
	{
		if (!(_time <= EffectDuration))
		{
			Close();
			_windowPrefab = null;
		}
	}
}
