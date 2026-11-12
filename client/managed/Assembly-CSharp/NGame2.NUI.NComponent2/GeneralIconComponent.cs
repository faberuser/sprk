using System;
using NGame2.NAccount;
using NGame2.NGuild;
using NGame2.NUI.NWindow;
using NGame2.NUtil.NStringProxy;
using NGameAsset;
using NShared;
using NShared.NLocalization;
using NVespa.NGlobal;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NComponent2;

public class GeneralIconComponent : MonoBehaviour, NVespa.NPrefab2.InitOnCreate
{
	public enum DecoType
	{
		None,
		EntryMenuBuff
	}

	[Serializable]
	public class DecoInfo
	{
		public DecoType Type;

		public Transform Root;
	}

	public const string PrefabPath = "Prefab2/Component/Icon/GeneralIconComponent";

	public static PrefabAuto<GeneralIconComponent> PrefabAuto = new PrefabAuto<GeneralIconComponent>("Prefab2/Component/Icon/GeneralIconComponent");

	public UITexture IconTexture;

	public DecoInfo[] Deco;

	public UILabel Label_RemainTime;

	private TooltipTextInfo _TooltipTextInfo;

	private HeroBonusData _heroBonusData;

	private DateTime _EndTime = DateTime.MinValue;

	private float _lastUpdateTime;

	private GuildPenaltyType _GuildPenaltyType;

	public void Open(string path, HeroBonusData heroBonusData, DecoType type = DecoType.None)
	{
		AssetTexture plainTexture = UIHelper.GetPlainTexture(path);
		UIHelper.ResetPlaneAssetTextureAsync(this, IconTexture, plainTexture);
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(Deco); i++)
		{
			UIHelper.EnableGameObject(Deco[i].Root, Deco[i].Type == type);
		}
		_heroBonusData = heroBonusData;
		SetupTooltipText();
	}

	public void OpenGuildContentsRestrict(string path, DateTime withdrawTime, GuildPenaltyType resultType)
	{
		AssetTexture plainTexture = UIHelper.GetPlainTexture(path);
		UIHelper.ResetPlaneAssetTextureAsync(this, IconTexture, plainTexture);
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(Deco); i++)
		{
			UIHelper.EnableGameObject(Deco[i].Root, Deco[i].Type == DecoType.EntryMenuBuff);
		}
		_heroBonusData = null;
		_EndTime = withdrawTime;
		UIHelper.EnableGameObject(Label_RemainTime, _EndTime.CompareTo(DateTime.MinValue) != 0);
		_GuildPenaltyType = resultType;
		SetupTooltipText();
		UpdateRemainTimeLabel();
	}

	void NVespa.NPrefab2.InitOnCreate.OnCreate()
	{
		UIHelper.EnableGameObject(Label_RemainTime, enable: false);
	}

	void NVespa.NPrefab2.InitOnCreate.OnRelease()
	{
		UIHelper.ClearPlaneAssetTexture(IconTexture);
		_EndTime = DateTime.MinValue;
	}

	private void Update()
	{
		if (!(Time.time - _lastUpdateTime < 1f))
		{
			_lastUpdateTime = Time.time;
			UpdateTooltipText();
			UpdateRemainTimeLabel();
		}
	}

	public void OnShowTooltip()
	{
		HeroBonusTooltip.Open(IconTexture, _TooltipTextInfo);
	}

	public void OnHideTooltip()
	{
		HeroBonusTooltip.Hide(IconTexture);
	}

	public void UpdateTooltipText()
	{
		if (HeroBonusTooltip.IsVisible(IconTexture))
		{
			SetupTooltipText();
			HeroBonusTooltip.Refresh(IconTexture, _TooltipTextInfo);
		}
	}

	private void SetupTooltipText()
	{
		if (_TooltipTextInfo == null)
		{
			_TooltipTextInfo = new TooltipTextInfo();
		}
		if (_heroBonusData != null)
		{
			_TooltipTextInfo.Name.Value = LocalizationManager.GetStringDirect(_heroBonusData.BonusNameKey);
			_TooltipTextInfo.Desc.Value = LocalizationManager.GetStringDirect(_heroBonusData.BonusDescKey);
		}
		_TooltipTextInfo.TypeText.Reset();
		if (_EndTime.CompareTo(DateTime.MinValue) != 0)
		{
			int guildWithdrawCount = GlobalAutoBehaviour<AccountManager>.instance.UserManager.MiscInfo.GuildWithdrawCount;
			int reJoinHour = GameRoot.Instance.GuildPenaltyDataContainer.GetReJoinHour(guildWithdrawCount);
			int restrictHour = GameRoot.Instance.GuildPenaltyDataContainer.GetRestrictHour(guildWithdrawCount);
			if (_GuildPenaltyType == GuildPenaltyType.WithdrawPenalty)
			{
				_TooltipTextInfo.Name.Value = LocalizationManager.GetString(Guild.GUILD_WITHDRAW_PENALTY_NAME);
				_TooltipTextInfo.Desc.Value = LocalizationManager.GetString(Guild.GUILD_WITHDRAW_PENALTY_DESC, reJoinHour, restrictHour);
			}
			if (_GuildPenaltyType == GuildPenaltyType.ContentsRestricted)
			{
				_TooltipTextInfo.Name.Value = LocalizationManager.GetString(Guild.GUILD_ACTIVITY_RESTRICT_NAME);
				_TooltipTextInfo.Desc.Value = LocalizationManager.GetString(Guild.GUILD_ACTIVITY_RESTRICT_DESC);
			}
			DateTime currentServerUTCTime = GlobalAutoBehaviour<AccountManager>.instance.GetCurrentServerUTCTime();
			int num = (int)(_EndTime - currentServerUTCTime).TotalSeconds;
			if (num > 0)
			{
				_TooltipTextInfo.TypeText.Assign(new BoosterTimeRemainedBuilder(num, appendLine: false));
			}
			else
			{
				_TooltipTextInfo.TypeText.Reset();
			}
		}
	}

	private void UpdateRemainTimeLabel()
	{
		if (_EndTime.CompareTo(DateTime.MinValue) != 0)
		{
			int remainSeconds = GetRemainSeconds();
			if (remainSeconds == 0)
			{
				UIHelper.EnableGameObject(Label_RemainTime, enable: false);
			}
			else
			{
				UIHelper.SetLabelText(Label_RemainTime, UIHelper.GetRemainTimeTextSingle(remainSeconds));
			}
		}
	}

	private int GetRemainSeconds()
	{
		DateTime currentServerUTCTime = GlobalAutoBehaviour<AccountManager>.instance.GetCurrentServerUTCTime();
		return Math.Max((int)(_EndTime - currentServerUTCTime).TotalSeconds, 0);
	}

	public bool IsEffective()
	{
		if (_heroBonusData != null)
		{
			return true;
		}
		return GetRemainSeconds() > 0;
	}
}
