using System;
using System.Collections.Generic;
using NGame2.NContentsDefine;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NManager;
using NShared;
using NShared.NLocalization;
using NVespa.NGlobal;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class TierChangePopup : BaseWindow
{
	[Serializable]
	public class LabelInfo
	{
		public UIWidget IconWigetRoot1;

		public UIWidget IconWigetRoot2;

		public IconProxy IconProxy1;

		public IconProxy IconProxy2;

		public UILabel Label_Reward1;

		public UILabel Label_Reward2;
	}

	public const string PrefabPath = "Resources/Prefab2/Window/MessagePopup/TierChangePopup";

	public UILabel Label_RewardType1;

	public UILabel Label_RewardType2;

	public LabelInfo OldLabels;

	public LabelInfo NewLabels;

	public UIGame3DWidget EffectWidget;

	public string EffectPivotName;

	public float EffectDuration;

	public Transform PivotFollower;

	public Transform LabelsRoot;

	private Transform _pivotTransform;

	private int _oldTierIndex;

	private int _newTierIndex;

	private float _time;

	private VisualTierRank _newVisualTierRank;

	private MatchArenaType _arenaType;

	private MatchTierIconComponent _tierIconComponent;

	public static TierChangePopup Open(int oldTierIndex, int newTierIndex, VisualTierRank newVisualTierRank, MatchArenaType arenaType)
	{
		WindowPrefab<TierChangePopup> windowPrefab = new WindowPrefab<TierChangePopup>("Resources/Prefab2/Window/MessagePopup/TierChangePopup", GameUIRoot.ELayerType.UI);
		windowPrefab.instance.OpenInternal(oldTierIndex, newTierIndex, newVisualTierRank, arenaType);
		GlobalAutoBehaviour<PopupManegement>.instance.Add(windowPrefab);
		return windowPrefab.instance;
	}

	public void OpenInternal(int oldTierIndex, int newTierIndex, VisualTierRank newVisualTierRank, MatchArenaType arenaType)
	{
		Open(delegate
		{
			Init(oldTierIndex, newTierIndex, newVisualTierRank, arenaType);
		}, null);
	}

	private void Init(int oldTierIndex, int newTierIndex, VisualTierRank newVisualTierRank, MatchArenaType arenaType)
	{
		_oldTierIndex = oldTierIndex;
		_newTierIndex = newTierIndex;
		_time = 0f;
		_newVisualTierRank = newVisualTierRank;
		_arenaType = arenaType;
		UIHelper.EnableGameObject(LabelsRoot, enable: false);
		if (EffectWidget != null)
		{
			EffectWidget.Play(null);
			UpdatePivotTransform();
		}
		InitRewardTypeLabel();
		FactoryBase.SafeReleaseComponent(ref _tierIconComponent);
		_tierIconComponent = MatchTierIconComponent.PrefabAuto.Factory.Create(PivotFollower);
		_tierIconComponent.Open(newTierIndex, newVisualTierRank, arenaType);
		_tierIconComponent.HideName();
		Vespa_Unity_Transform_Extension_Method.ResetLocalTM(_tierIconComponent.transform);
	}

	private void InitRewardTypeLabel()
	{
		string text = "";
		string text2 = "";
		switch (_arenaType)
		{
		case MatchArenaType.Normal:
			text = LocalizationManager.GetString(Match.PVP_REWARDINFO_DAILY);
			text2 = LocalizationManager.GetString(Match.PVP_REWARDINFO_SEASON);
			break;
		case MatchArenaType.BanPick:
			text = LocalizationManager.GetString(Match.PVP2_TIERREWORD);
			text2 = LocalizationManager.GetString(Match.PVP2_WEEKREWARD);
			break;
		}
		UIHelper.SetLabelText(Label_RewardType1, text);
		UIHelper.SetLabelText(Label_RewardType2, text2);
	}

	public override void UpdateDerived()
	{
		base.UpdateDerived();
		_time += Time.deltaTime;
		UpdatePivotTransform();
		UpdateShowLabels();
	}

	public override void OnClosing()
	{
		base.OnClosing();
		EffectWidget.Stop(immediatelyClear: true);
		FactoryBase.SafeReleaseComponent(ref _tierIconComponent);
		ReleaseIconProxy(ref OldLabels);
		ReleaseIconProxy(ref NewLabels);
	}

	private void ReleaseIconProxy(ref LabelInfo labelInfo)
	{
		if (labelInfo != null)
		{
			if (labelInfo.IconProxy1 != null)
			{
				FactoryBase.SafeReleaseComponent(ref labelInfo.IconProxy1);
			}
			if (labelInfo.IconProxy2 != null)
			{
				FactoryBase.SafeReleaseComponent(ref labelInfo.IconProxy2);
			}
			labelInfo.IconProxy1 = null;
			labelInfo.IconProxy2 = null;
		}
	}

	private void UpdateShowLabels()
	{
		if (!UIHelper.IsEnabledGameObjet(LabelsRoot) && _time >= EffectDuration)
		{
			UIHelper.EnableGameObject(LabelsRoot, enable: true);
			SetTierRewardIcon();
			SetTierLevelLabels(_oldTierIndex, _newTierIndex);
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

	private void SetTierRewardIcon()
	{
		switch (_arenaType)
		{
		case MatchArenaType.Normal:
			SetTierRewardIconProxy(OldLabels, CurrencyType.PvpCoin, CurrencyType.Gem);
			SetTierRewardIconProxy(NewLabels, CurrencyType.PvpCoin, CurrencyType.Gem);
			break;
		case MatchArenaType.BanPick:
			SetTierRewardIconProxy(OldLabels, CurrencyType.GloryPoint, CurrencyType.RankingPoint);
			SetTierRewardIconProxy(NewLabels, CurrencyType.GloryPoint, CurrencyType.RankingPoint);
			break;
		}
	}

	private void SetTierRewardIconProxy(LabelInfo labels, CurrencyType rewardType1, CurrencyType rewardType2)
	{
		if (labels.IconProxy1 == null)
		{
			labels.IconProxy1 = IconProxy.Create(labels.IconWigetRoot1);
		}
		if (labels.IconProxy2 == null)
		{
			labels.IconProxy2 = IconProxy.Create(labels.IconWigetRoot2);
		}
		labels.IconProxy1.SetupCurrencyNoBg(rewardType1);
		labels.IconProxy2.SetupCurrencyNoBg(rewardType2);
	}

	private void SetTierLevelLabels(int oldTierIndex, int newTierIndex)
	{
		switch (_arenaType)
		{
		case MatchArenaType.Normal:
		{
			List<MatchRewardData> rewardDataListOfTierIndex3 = GameRoot.Instance.MatchRewardDataContainer.GetRewardDataListOfTierIndex(oldTierIndex);
			List<MatchRewardData> rewardDataListOfTierIndex4 = GameRoot.Instance.MatchRewardDataContainer.GetRewardDataListOfTierIndex(newTierIndex);
			SetTeamLevelEffectLabels(rewardDataListOfTierIndex3, _arenaType, _newVisualTierRank, OldLabels);
			SetTeamLevelEffectLabels(rewardDataListOfTierIndex4, _arenaType, _newVisualTierRank, NewLabels);
			break;
		}
		case MatchArenaType.BanPick:
		{
			List<MatchBanPickTierRewardData> rewardDataListOfTierIndex = GameRoot.Instance.GlobalBanPickTierRewardDataContainer.GetRewardDataListOfTierIndex(oldTierIndex);
			List<MatchBanPickTierRewardData> rewardDataListOfTierIndex2 = GameRoot.Instance.GlobalBanPickTierRewardDataContainer.GetRewardDataListOfTierIndex(newTierIndex);
			SetTeamLevelEffectLabels(rewardDataListOfTierIndex, _arenaType, _newVisualTierRank, OldLabels);
			SetTeamLevelEffectLabels(rewardDataListOfTierIndex2, _arenaType, _newVisualTierRank, NewLabels);
			break;
		}
		}
	}

	private static T FindRewardData<T>(List<T> datas, MatchArenaType arenaType, VisualTierRank visualTierRank) where T : MatchRewardData
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(datas); i++)
		{
			T val = datas[i];
			VisualTierRank visualTierRank2 = VisualTierRank.CreateFromTableMinRank(val, arenaType);
			VisualTierRank visualTierRank3 = VisualTierRank.CreateFromTableMaxRank(val, arenaType);
			if (visualTierRank2.Value > 0 && visualTierRank3.Value > 0 && visualTierRank.Value >= visualTierRank2.Value && visualTierRank.Value <= visualTierRank3.Value)
			{
				return val;
			}
		}
		return null;
	}

	public static void SetTeamLevelEffectLabels(List<MatchRewardData> datas, MatchArenaType arenaType, VisualTierRank visualTierRank, LabelInfo labels)
	{
		if (labels == null)
		{
			return;
		}
		if (__Extension_Method_ICollection__.COUNT(datas) == 0)
		{
			string text = "-";
			UIHelper.SetLabelText(labels.Label_Reward1, text);
			UIHelper.SetLabelText(labels.Label_Reward2, text);
			return;
		}
		datas.Sort((MatchRewardData l, MatchRewardData r) => l.MinRank.CompareTo(r.MinRank));
		MatchRewardData matchRewardData = FindRewardData(datas, arenaType, visualTierRank);
		if (matchRewardData == null)
		{
			matchRewardData = datas[datas.Count - 1];
		}
		UIHelper.SetLabelText(labels.Label_Reward1, matchRewardData.DailyReward);
		UIHelper.SetLabelText(labels.Label_Reward2, matchRewardData.SeasonReward);
	}

	public static void SetTeamLevelEffectLabels(List<MatchBanPickTierRewardData> datas, MatchArenaType arenaType, VisualTierRank visualTierRank, LabelInfo labels)
	{
		if (labels == null)
		{
			return;
		}
		if (__Extension_Method_ICollection__.COUNT(datas) == 0)
		{
			string text = "-";
			UIHelper.SetLabelText(labels.Label_Reward1, text);
			UIHelper.SetLabelText(labels.Label_Reward2, text);
			return;
		}
		datas.Sort((MatchBanPickTierRewardData l, MatchBanPickTierRewardData r) => l.MinRank.CompareTo(r.MinRank));
		MatchBanPickTierRewardData matchBanPickTierRewardData = FindRewardData(datas, arenaType, visualTierRank);
		if (matchBanPickTierRewardData == null)
		{
			matchBanPickTierRewardData = datas[datas.Count - 1];
		}
		UIHelper.SetLabelText(labels.Label_Reward1, matchBanPickTierRewardData.AttendanceReward);
		UIHelper.SetLabelText(labels.Label_Reward2, matchBanPickTierRewardData.WeeklyReward);
	}

	public void OnClickClose()
	{
		Close();
	}
}
