using System;
using NGameAsset;
using NShared;
using NVespa;
using NVespa.NGlobal;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NComponent2;

public class AttendanceRewardComponent : DbRewardComponent
{
	public interface Listener
	{
		void OnClick(AttendanceRewardComponent component);
	}

	public const string PrefabPath = "Prefab2/Component/Reward/AttendanceRewardComponent";

	public const string Prefab5DaysPath = "Prefab2/Component/Reward/AttendanceReward5DaysComponent";

	public const string Prefab14DaysPath = "Prefab2/Component/Reward/AttendanceRewardSupply14DaysComponent";

	public const string PrefabMay14DaysPath = "Prefab2/Component/Reward/AttendanceRewardMay14DaysComponent";

	public const string PrefabNewMay14DaysPath = "Prefab2/Component/Reward/AttendanceRewardNewMay14DaysComponent";

	public const string PrefabSoulWeapon14DaysPath = "Prefab2/Component/Reward/AttendanceRewardSoulWeapon14DaysComponent";

	public const string PrefabReturn5DaysPath = "Prefab2/Component/Reward/AttendanceRewardReturn5DaysComponent";

	public const string PrefabNew7DaysPath = "Prefab2/Component/Reward/AttendanceRewardNew7DaysComponent";

	public const string PrefabNew7DaysSpecialPath = "Prefab2/Component/Reward/AttendanceRewardNew7DaysSpecialComponent";

	public const string Prefab9Chapter14DaysPath = "Prefab2/Component/Reward/AttendanceRewardVeronica14DaysComponent";

	public const string PrefabNew14DaysPath = "Prefab2/Component/Reward/AttendanceRewardNew14DaysComponent";

	public const string PrefabFallWelcom5DaysPath = "Prefab2/Component/Reward/AttendanceRewardFallWelcome5DaysComponent";

	public const string PrefabFallWelcom5DaysSpecialPath = "Prefab2/Component/Reward/AttendanceRewardFallWelcome5DaysSpecialComponent";

	public const string Prefab15DaysDetailPath = "Prefab2/Component/Reward/AttendanceReward15DaysDetailComponent";

	public const string PrefabAccureLoginPath = "Prefab2/Component/Reward/AccrueLoginRewardItemComponent";

	public const string PrefabNewbieRewardAttendanceDetailPath = "Prefab2/Component/NewbieReward/NewbieRewardAttendanceDetailComponent";

	public const string Prefab1000DaysAnniversary7DaysPath = "Prefab2/Component/Reward/AttendanceReward1000DaysAnniversary7DaysComponent";

	public const string Prefab1000DaysAnniversary7DaysSpecialPath = "Prefab2/Component/Reward/AttendanceReward1000DaysAnniversary7DaysSpecialComponent";

	public const string PrefabWelcome2020Path = "Prefab2/Component/Reward/AttendanceRewardWelcome2020Component";

	public const string PrefabWelcome2020SpecialPath = "Prefab2/Component/Reward/AttendanceRewardWelcome2020SpecialComponent";

	public const string Prefab2020NewSemester14DaysPath = "Prefab2/Component/Reward/AttendanceReward2020NewSemesterComponent";

	public const string PrefabGuildDailyPath = "Prefab2/Component/Reward/AttendanceRewardGuildDailyComponent";

	public const string PrefabGuildCollaborationPath = "Prefab2/Component/Reward/AttendanceRewardGuildCollaborationComponent";

	public static PrefabAuto<AttendanceRewardComponent> PrefabAuto = new PrefabAuto<AttendanceRewardComponent>("Prefab2/Component/Reward/AttendanceRewardComponent");

	public static PrefabAuto<AttendanceRewardComponent> Prefab5DaysAuto = new PrefabAuto<AttendanceRewardComponent>("Prefab2/Component/Reward/AttendanceReward5DaysComponent");

	public static PrefabAuto<AttendanceRewardComponent> Prefab14DaysAuto = new PrefabAuto<AttendanceRewardComponent>("Prefab2/Component/Reward/AttendanceRewardMay14DaysComponent");

	public static PrefabAuto<AttendanceRewardComponent> PrefabMay14DaysAuto = new PrefabAuto<AttendanceRewardComponent>("Prefab2/Component/Reward/AttendanceRewardMay14DaysComponent");

	public static PrefabAuto<AttendanceRewardComponent> PrefabNewMay14DaysAuto = new PrefabAuto<AttendanceRewardComponent>("Prefab2/Component/Reward/AttendanceRewardNewMay14DaysComponent");

	public static PrefabAuto<AttendanceRewardComponent> PrefabSoulWeapon14DaysAuto = new PrefabAuto<AttendanceRewardComponent>("Prefab2/Component/Reward/AttendanceRewardSoulWeapon14DaysComponent");

	public static PrefabAuto<AttendanceRewardComponent> PrefabNew14DaysAuto = new PrefabAuto<AttendanceRewardComponent>("Prefab2/Component/Reward/AttendanceRewardNew14DaysComponent");

	public static PrefabAuto<AttendanceRewardComponent> PrefabReturn5DaysAuto = new PrefabAuto<AttendanceRewardComponent>("Prefab2/Component/Reward/AttendanceRewardReturn5DaysComponent");

	public static PrefabAuto<AttendanceRewardComponent> PrefabNew7DaysAuto = new PrefabAuto<AttendanceRewardComponent>("Prefab2/Component/Reward/AttendanceRewardNew7DaysComponent");

	public static PrefabAuto<AttendanceRewardComponent> PrefabNew7DaysSpecialAuto = new PrefabAuto<AttendanceRewardComponent>("Prefab2/Component/Reward/AttendanceRewardNew7DaysSpecialComponent");

	public static PrefabAuto<AttendanceRewardComponent> Prefab15DaysDetail = new PrefabAuto<AttendanceRewardComponent>("Prefab2/Component/Reward/AttendanceReward15DaysDetailComponent");

	public static PrefabAuto<AttendanceRewardComponent> PrefabAccureLogin = new PrefabAuto<AttendanceRewardComponent>("Prefab2/Component/Reward/AccrueLoginRewardItemComponent");

	public static PrefabAuto<AttendanceRewardComponent> PrefabFallWelcomeAuto = new PrefabAuto<AttendanceRewardComponent>("Prefab2/Component/Reward/AttendanceRewardFallWelcome5DaysComponent");

	public static PrefabAuto<AttendanceRewardComponent> PrefabFallWelcomeSpecailAuto = new PrefabAuto<AttendanceRewardComponent>("Prefab2/Component/Reward/AttendanceRewardFallWelcome5DaysSpecialComponent");

	public static PrefabAuto<AttendanceRewardComponent> Prefab9Chatper14DaysAuto = new PrefabAuto<AttendanceRewardComponent>("Prefab2/Component/Reward/AttendanceRewardVeronica14DaysComponent");

	public static PrefabAuto<AttendanceRewardComponent> PrefabNewbieRewardAttendanceDetailAuto = new PrefabAuto<AttendanceRewardComponent>("Prefab2/Component/NewbieReward/NewbieRewardAttendanceDetailComponent");

	public static PrefabAuto<AttendanceRewardComponent> Prefab1000DaysAnniversary7DaysAuto = new PrefabAuto<AttendanceRewardComponent>("Prefab2/Component/Reward/AttendanceReward1000DaysAnniversary7DaysComponent");

	public static PrefabAuto<AttendanceRewardComponent> Prefab1000DaysAnniversary7DaysSpecialAuto = new PrefabAuto<AttendanceRewardComponent>("Prefab2/Component/Reward/AttendanceReward1000DaysAnniversary7DaysSpecialComponent");

	public static PrefabAuto<AttendanceRewardComponent> PrefabWelcome2020Auto = new PrefabAuto<AttendanceRewardComponent>("Prefab2/Component/Reward/AttendanceRewardNew7DaysComponent");

	public static PrefabAuto<AttendanceRewardComponent> PrefabWelcome2020SpecialAuto = new PrefabAuto<AttendanceRewardComponent>("Prefab2/Component/Reward/AttendanceRewardNew7DaysSpecialComponent");

	public static PrefabAuto<AttendanceRewardComponent> Prefab2020NewSemesterAuto = new PrefabAuto<AttendanceRewardComponent>("Prefab2/Component/Reward/AttendanceReward2020NewSemesterComponent");

	public static PrefabAuto<AttendanceRewardComponent> PrefabGuildDailyAuto = new PrefabAuto<AttendanceRewardComponent>("Prefab2/Component/Reward/AttendanceRewardGuildDailyComponent");

	public static PrefabAuto<AttendanceRewardComponent> PrefabGuildCollaborationAuto = new PrefabAuto<AttendanceRewardComponent>("Prefab2/Component/Reward/AttendanceRewardGuildCollaborationComponent");

	public Transform VisibleRoot;

	public UIWidget GrayscaleRoot;

	public UILabel DayTextLabel;

	public UILabel DayLabel;

	public UIGame3DWidget EffectWidget;

	public GameObject AlreadyRewardedObject;

	public GameObject TodayRewardObject;

	public GameObject TodayReceivableObject;

	public UITexture CustomImageTexture;

	public UILabel DescriptionTitleLable;

	public UILabel DescriptionLabel;

	private Listener _listener;

	private bool _customTextureRequested;

	public GameObject GoSpecial;

	public GameObject GoReceiveButton;

	public void SetListener(Listener listener)
	{
		_listener = listener;
	}

	protected override void DestroyComponent()
	{
		base.DestroyComponent();
		UIHelper.ClearPlaneAssetTexture(CustomImageTexture);
		_customTextureRequested = false;
	}

	public void OnClickComponent()
	{
		if (_listener != null)
		{
			_listener.OnClick(this);
		}
	}

	public void Show(bool show)
	{
		UIHelper.EnableGameObject(VisibleRoot, show);
	}

	public void EnableDay(bool enable)
	{
		UIHelper.EnableGameObject(DayTextLabel, enable);
		UIHelper.EnableGameObject(DayLabel, enable);
	}

	public void SetDay(int day)
	{
		UIHelper.SetLabelText(DayLabel, day);
	}

	public void SetDayLabel(string dayText)
	{
		UIHelper.SetLabelText(DayLabel, dayText);
	}

	public void SetAlreadyRewarded(bool rewarded)
	{
		UIHelper.EnableGameObject(AlreadyRewardedObject, rewarded);
		UIHelper.SetWidgetGrayScale(GrayscaleRoot, rewarded);
		UIHelper.EnableGameObject(TodayReceivableObject, !rewarded);
	}

	public void SetGrayScale(bool isGrayscale)
	{
		UIHelper.SetWidgetGrayScale(GrayscaleRoot, isGrayscale);
	}

	public void SetTodayReward(bool today)
	{
		UIHelper.EnableGameObject(TodayRewardObject, today);
	}

	public void StartReceiveEffect(Action onEnd = null)
	{
		if ((bool)EffectWidget)
		{
			EffectWidget.Play(onEnd);
		}
		else
		{
			Vespa_System_Action_Extension_Method.fire(onEnd);
		}
	}

	public void SetRewardInfo(AttendanceRewardInfoData rewardInfo)
	{
		if (rewardInfo != null)
		{
			AttendanceRewardUrlData data = GameRoot.Instance.AttendanceRewardUrlDataContainer.GetData(rewardInfo.Index, rewardInfo.Day, Vespa_String_Extension_Method.GetEnumString(SingletonAuto<LocalizationManager>.instance.Language));
			if (data != null && !String_Extension_Method.IsNullOrEmpty(data.Url))
			{
				_customTextureRequested = true;
				AssetTexture assetTexture = new AssetTexture();
				assetTexture.OpenUrl(data.Url);
				UIHelper.ResetPlaneAssetTextureAsync(this, CustomImageTexture, assetTexture, UIHelper.EAssetTextureAssign.None, delegate
				{
				});
			}
			if (!String_Extension_Method.IsNullOrEmpty(rewardInfo.RewardTitleKey))
			{
				UIHelper.SetLabelText(DescriptionLabel, LocalizationManager.GetStringDirect(rewardInfo.RewardTitleKey), unwrapNewline: true);
			}
			if (!String_Extension_Method.IsNullOrEmpty(rewardInfo.RewardDescKey))
			{
				UIHelper.SetLabelText(DescriptionTitleLable, LocalizationManager.GetStringDirect(rewardInfo.RewardDescKey), unwrapNewline: true);
			}
		}
	}

	public bool CustomImageLoaded()
	{
		if (CustomImageTexture == null)
		{
			return true;
		}
		if (CustomImageTexture.mainTexture != null)
		{
			return true;
		}
		if (!_customTextureRequested)
		{
			return true;
		}
		return false;
	}

	public void SetSpecial(bool isOn)
	{
		UIHelper.EnableGameObject(GoSpecial, isOn);
	}

	public void SetRecieveButton(bool isOn)
	{
		UIHelper.EnableGameObject(GoReceiveButton, isOn);
	}
}
