using System.Collections;
using NGame2.NAccount;
using NGame2.NCutScene;
using NGame2.NUI.NManager;
using NGame2.NUI.NWindow.NBattle;
using NGame2.NUtil.NCostume;
using NShared;
using NShared.GiveRewardMaxClosenessHero;
using NVespa;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NSingleton;
using NVespa.NTask.NRuntime;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class HeroInnView : HeroRecruitView, IUserObserver<UserManager.UserStatInfo>, IUserObserver, HeroInnViewButtonGroup.Listener
{
	public interface Listener
	{
		void OnGreeting();

		void OnConversation();

		void OnSendGift();

		void OnClosenessGreeting();

		void OnClosenessConversation();

		void OnClosenessGift();

		void OnRecruit();

		void OnStartRecruit();

		void OnChangeRecruit();

		void OnChangeAndStartRecruit();

		void OnShowDetail();

		void RequestRefresh();

		void OnChangeHero(int heroIndex);

		bool CheckAreadySelectHero();
	}

	private enum EGroupButtonType
	{
		None,
		Friendly,
		Closeness,
		Connect
	}

	public const string PrefabPath = "Resources/Prefab2/Window/FullScene/HeroInnView";

	public UIWidget FriendlyWidget;

	public UIWidget DoFriendlyWidget;

	public UIWidget WidgetSelect;

	public UIButton CloseButton;

	public UIButton ChangeRecruitButton;

	public UIButton StartRecruitButton;

	public UIButton RecruitButton;

	public UILabel FriendshipValueLabel;

	public UILabel GoldValueLabel;

	public UILabel HeroNameLabel;

	public UILabel HeroSelectLabel;

	public UILabel HeroLeaveLabel;

	public UILabel Label_FriendlyProgress;

	public UIClipSprite Sprite_FriendlyGauge;

	public UISprite Sprite_FriendlyGaugeBack;

	public UILabel Label_ClosessnessProgress;

	public UIClipSprite Sprite_ClosenessGauge;

	public UISprite Sprite_ClosenessGaugeBack;

	public Animator Anim_Closeness;

	public HeroInnPortrait[] HeroInnPortraits;

	public HeroInnViewButtonGroup FriendlyBtnGroup;

	public HeroInnViewButtonGroup ClosenessBtnGroup;

	public HeroInnViewButtonGroup ConnectBtnGroup;

	public GameObject RightButtonRoot;

	public GameObject FriendlyTitleRoot;

	public GameObject ClosenessTitleRoot;

	public GameObject ConnectTitleRoot;

	private int _switchingButtonClickCount;

	private Listener _listener;

	private float _lastLabelUpdateTime;

	private EGroupButtonType prevButtonType = EGroupButtonType.Closeness;

	public void Open(HeroInfo heroInfo, Listener listener, bool isSelectHero)
	{
		_listener = listener;
		InitAction = InitUIBtnGroup;
		new DelegateTask(OpenAsync(heroInfo, isSelectHero)).Commit();
	}

	public void Reload(HeroInfo heroInfo)
	{
		InitAction = InitUIBtnGroup;
		new DelegateTask(ReloadAsync(heroInfo)).Commit();
	}

	public void ReservedMaxClosenessReward(NShared.GiveRewardMaxClosenessHero.Response res)
	{
		if (res == null || (__Extension_Method_ICollection__.IsNullOrEmpty(res.CurrencyResults) && __Extension_Method_ICollection__.IsNullOrEmpty(res.itemResults) && __Extension_Method_ICollection__.IsNullOrEmpty(res.equipItemInfos) && __Extension_Method_ICollection__.IsNullOrEmpty(res.staminaResult) && __Extension_Method_ICollection__.IsNullOrEmpty(res.FriendshipPointResult)))
		{
			return;
		}
		SceneAutoBehaviour<CutSceneManager>.instance.Open("CutScene/other/HM1_1B", canSkip: false, delegate
		{
			int num = __Extension_Method_ICollection__.COUNT(res.CurrencyResults);
			num += __Extension_Method_ICollection__.COUNT(res.itemResults);
			num += __Extension_Method_ICollection__.COUNT(res.equipItemInfos);
			num += __Extension_Method_ICollection__.COUNT(res.staminaResult);
			num += __Extension_Method_ICollection__.COUNT(res.FriendshipPointResult);
			int num2 = GlobalAutoBehaviour<AccountManager>.instance.HeroFriendlyManager.MaxClosenessHeroCount();
			if (num != num2)
			{
				VespaDebug.LogError($"[Invalid Match] Response Reward Count : {num} Client Max Closeness Count : {num2}");
			}
			string stringDirect = LocalizationManager.GetStringDirect("HEROINN_CLOSENESS_REWARD", num2);
			BaseRewardPopup.PopupOpenData popupOpenData = new BaseRewardPopup.PopupOpenData().SetTitle(stringDirect).AddCurrencyResultInfos3(res.CurrencyResults).AddItemResultInfos(res.itemResults)
				.AddEquipResultInfos(res.equipItemInfos)
				.AddStaminaResultInfos(res.staminaResult);
			if (!__Extension_Method_ICollection__.IsNullOrEmpty(res.FriendshipPointResult))
			{
				int num3 = res.FriendshipPointResult.Length;
				int valueInt = GameRoot.Instance.ConstantDataContainer.GetValueInt("MaxRewardFriendshipPoint");
				long addValue = valueInt;
				FriendshipPointResultInfo[] array = new FriendshipPointResultInfo[num3];
				for (int i = 0; i < array.Length; i++)
				{
					array[i] = new FriendshipPointResultInfo
					{
						AddValue = addValue
					};
				}
				if (array.Length != 0)
				{
					popupOpenData.SetFriendshipPointResults(array);
				}
			}
			HeroinnRewardPopup.OpenPopup(popupOpenData, isGlobal: false);
		});
	}

	protected override void OnShowRoot(bool isShow)
	{
		base.OnShowRoot(isShow);
	}

	private void InitHeroPortraits()
	{
		if (__Extension_Method_ICollection__.IsNullOrEmpty(HeroInnPortraits))
		{
			return;
		}
		int[] heroSelectedIndices = GetHeroSelectedIndices();
		for (int i = 0; i < HeroInnPortraits.Length && i < heroSelectedIndices.Length && i < heroSelectedIndices.Length; i++)
		{
			HeroInnPortraitListener listener = new HeroInnPortraitListener(delegate(int heroIndex)
			{
				if (heroIndex != base.HeroIndex)
				{
					_listener.OnChangeHero(heroIndex);
				}
			});
			HeroInnPortraits[i].Open(heroSelectedIndices[i], listener);
		}
	}

	public override void Reset()
	{
		base.Reset();
		_switchingButtonClickCount = 0;
		ClosenessBtnGroup.Reset();
		ConnectBtnGroup.Reset();
		FriendlyBtnGroup.Reset();
	}

	protected override int CheckReplaceBasicCostume(int costumeIndex)
	{
		return HeroCostume.CheckReplaceBasicCostume(costumeIndex, base.HeroInfo.HeroIndex, base.HeroInfo.Star);
	}

	private bool IsSelectedHero()
	{
		return GlobalAutoBehaviour<AccountManager>.instance.HeroFriendlyManager.IsSelectHero(base.HeroIndex);
	}

	public bool IsHaveHero()
	{
		return GlobalAutoBehaviour<AccountManager>.instance.HeroFriendlyManager.IsHaveHero(base.HeroIndex);
	}

	private int[] GetHeroSelectedIndices()
	{
		return GlobalAutoBehaviour<AccountManager>.instance.HeroFriendlyManager.GetSelectedHeroIndices();
	}

	private bool GetSelectCoolTime()
	{
		PlayerHeroFriendlyInfo[] values = GlobalAutoBehaviour<AccountManager>.instance.HeroFriendlyManager.GetValues();
		PlayerHeroFriendlyInfo playerHeroFriendlyInfo = (__Extension_Method_ICollection__.IsNullOrEmpty(values) ? null : values[0]);
		if (playerHeroFriendlyInfo == null)
		{
			return false;
		}
		return SceneAutoBehaviour<HeroInnManagement>.instance.ResetHourElapsed(playerHeroFriendlyInfo.SelectedTime);
	}

	private bool GetCoolTime(FriendlyActionType actionType)
	{
		PlayerHeroFriendlyInfo[] values = GlobalAutoBehaviour<AccountManager>.instance.HeroFriendlyManager.GetValues();
		PlayerHeroFriendlyInfo playerHeroFriendlyInfo = (__Extension_Method_ICollection__.IsNullOrEmpty(values) ? null : values[0]);
		if (playerHeroFriendlyInfo == null)
		{
			return true;
		}
		switch (actionType)
		{
		case FriendlyActionType.Greeting:
			return SceneAutoBehaviour<HeroInnManagement>.instance.ResetHourElapsed(playerHeroFriendlyInfo.LastGreetingTime);
		case FriendlyActionType.Conversation:
			return SceneAutoBehaviour<HeroInnManagement>.instance.ResetHourElapsed(playerHeroFriendlyInfo.LastConversationTime);
		case FriendlyActionType.Gift:
			return SceneAutoBehaviour<HeroInnManagement>.instance.ResetHourElapsed(playerHeroFriendlyInfo.LastGiftTime);
		case FriendlyActionType.Closeness_Greeting:
		{
			string closenessGreetingTime = GlobalAutoBehaviour<AccountManager>.instance.HeroFriendlyManager.GetClosenessGreetingTime(base.HeroIndex);
			return SceneAutoBehaviour<HeroInnManagement>.instance.ResetHourElapsed(closenessGreetingTime);
		}
		case FriendlyActionType.Closeness_Conversation:
		{
			string closenessConversationTime = GlobalAutoBehaviour<AccountManager>.instance.HeroFriendlyManager.GetClosenessConversationTime(base.HeroIndex);
			return SceneAutoBehaviour<HeroInnManagement>.instance.ResetHourElapsed(closenessConversationTime);
		}
		case FriendlyActionType.Closeness_Gift:
		{
			string closenessGiftTime = GlobalAutoBehaviour<AccountManager>.instance.HeroFriendlyManager.GetClosenessGiftTime(base.HeroIndex);
			return SceneAutoBehaviour<HeroInnManagement>.instance.ResetHourElapsed(closenessGiftTime);
		}
		default:
			return true;
		}
	}

	private bool GetCost(FriendlyActionType actionType, out ShopCostType costType, out int cost)
	{
		cost = 0;
		costType = ShopCostType.None;
		HeroFriendlyData data = GameRoot.Instance.HeroFriendlyDataContainer.GetData(base.HeroIndex);
		if (data == null)
		{
			return false;
		}
		HeroFriendlySubData dataByFriendlyActionType = data.GetDataByFriendlyActionType(actionType);
		cost = dataByFriendlyActionType.ActionCost;
		costType = dataByFriendlyActionType.CostType;
		if (costType == ShopCostType.None)
		{
			return true;
		}
		long num = 0L;
		switch (costType)
		{
		case ShopCostType.Gold:
			num = GlobalAutoBehaviour<AccountManager>.instance.UserManager.Gold;
			break;
		case ShopCostType.FriendshipPoint:
			num = GlobalAutoBehaviour<AccountManager>.instance.UserManager.UserInfo.FriendshipPoint;
			break;
		}
		return num >= cost;
	}

	private bool CheckClosenessPoint()
	{
		int closenessPoint = GlobalAutoBehaviour<AccountManager>.instance.HeroFriendlyManager.GetClosenessPoint(base.HeroIndex);
		int needFriendlyPoint = GetNeedFriendlyPoint();
		return closenessPoint >= needFriendlyPoint;
	}

	private int GetConnectTalkPoint(FriendlyActionType actionType)
	{
		return actionType switch
		{
			FriendlyActionType.Connect_Talk_1 => GameRoot.Instance.ConstantDataContainer.GetValueInt("ConnectTalk1Point"), 
			FriendlyActionType.Connect_Talk_2 => GameRoot.Instance.ConstantDataContainer.GetValueInt("ConnectTalk2Point"), 
			FriendlyActionType.Connect_Talk_3 => GameRoot.Instance.ConstantDataContainer.GetValueInt("ConnectTalk3Point"), 
			_ => 0, 
		};
	}

	private bool CheckConnectTalkCost(FriendlyActionType actionType)
	{
		int connectTalkPoint = GetConnectTalkPoint(actionType);
		if (connectTalkPoint == 0)
		{
			return false;
		}
		int closenessPoint = GlobalAutoBehaviour<AccountManager>.instance.HeroFriendlyManager.GetClosenessPoint(base.HeroIndex);
		return connectTalkPoint <= closenessPoint;
	}

	private int GetNeedFriendlyPoint()
	{
		return GameRoot.Instance.HeroFriendlyDataContainer.GetData(base.HeroIndex)?.NeedFriendlyPoint ?? 0;
	}

	private bool SetFriendlyGauge()
	{
		int friendlyPoint = GlobalAutoBehaviour<AccountManager>.instance.HeroFriendlyManager.GetFriendlyPoint(base.HeroIndex);
		return SetGauge(friendlyPoint, Label_FriendlyProgress, Sprite_FriendlyGauge, Sprite_FriendlyGaugeBack);
	}

	private bool SetClosenessGauge()
	{
		int closenessPoint = GlobalAutoBehaviour<AccountManager>.instance.HeroFriendlyManager.GetClosenessPoint(base.HeroIndex);
		bool num = SetGauge(closenessPoint, Label_ClosessnessProgress, Sprite_ClosenessGauge, Sprite_ClosenessGaugeBack);
		if (num)
		{
			UIHelper.SetLabelText(Label_ClosessnessProgress, LocalizationManager.GetStringDirect("COMMON_MAX"));
		}
		return num;
	}

	private bool SetGauge(int gaugeValue, UILabel progressLabel, UIClipSprite gaugeFrontSprite, UISprite gaugeBackSprite)
	{
		if (progressLabel == null || gaugeFrontSprite == null || gaugeBackSprite == null)
		{
			return false;
		}
		HeroFriendlyData data = GameRoot.Instance.HeroFriendlyDataContainer.GetData(base.HeroIndex);
		if (data == null)
		{
			return false;
		}
		int needFriendlyPoint = data.NeedFriendlyPoint;
		UIHelper.SetProgressValue(gaugeFrontSprite, null, gaugeValue, needFriendlyPoint, showMaxValue: false);
		UIHelper.SetLabelText(progressLabel, $"{gaugeValue} / {needFriendlyPoint}");
		return gaugeValue >= needFriendlyPoint;
	}

	public void OnClickConnect_Talk1()
	{
		UpdateResponse(FriendlyActionType.Connect_Talk_1);
	}

	public void OnClickConnect_Talk2()
	{
		UpdateResponse(FriendlyActionType.Connect_Talk_2);
	}

	public void OnClickConnect_Talk3()
	{
		UpdateResponse(FriendlyActionType.Connect_Talk_3);
	}

	public void OnClickSpecial_Greeting()
	{
		UpdateResponse(FriendlyActionType.Greeting_Max);
	}

	public void OnClickGreeting()
	{
		_listener.OnGreeting();
	}

	public void OnClickConversation()
	{
		_listener.OnConversation();
	}

	public void OnClickGift()
	{
		_listener.OnSendGift();
	}

	public void OnClosenessGreeting()
	{
		_listener.OnClosenessGreeting();
	}

	public void OnClosenessConversation()
	{
		_listener.OnClosenessConversation();
	}

	public void OnClosenessGift()
	{
		_listener.OnClosenessGift();
	}

	public void OnClickShowDetail()
	{
		_listener.OnShowDetail();
	}

	public void OnClickChangeRecruit()
	{
		_listener.OnChangeRecruit();
	}

	public void OnClickRecruit()
	{
		_listener.OnRecruit();
	}

	public void OnClickStartRecruit()
	{
		if (_listener.CheckAreadySelectHero())
		{
			string stringDirect = LocalizationManager.GetStringDirect("HEROINN_START_SCOUT");
			string stringDirect2 = LocalizationManager.GetStringDirect("HEROINN_POPUP_RESET_2");
			MessagePopup.Open(stringDirect, stringDirect2, delegate
			{
				_listener.OnChangeAndStartRecruit();
			}, delegate
			{
			});
		}
		else
		{
			_listener.OnStartRecruit();
		}
	}

	public void OnClickClosenessAndConnectTalk()
	{
		if (!Anim_Closeness.GetBool("ClickCloseness") && !Anim_Closeness.GetBool("ClickConnectTalk"))
		{
			_switchingButtonClickCount++;
			if (_switchingButtonClickCount % 2 == 0)
			{
				SetButtonGroup(EGroupButtonType.Closeness, isInit: false);
			}
			else
			{
				SetButtonGroup(EGroupButtonType.Connect, isInit: false);
			}
		}
	}

	public void OnClickMiniGame()
	{
		Roulette.Create();
	}

	public void InitUIBtnGroup()
	{
		EGroupButtonType gType = (IsHaveHero() ? EGroupButtonType.Closeness : (IsSelectedHero() ? EGroupButtonType.Friendly : EGroupButtonType.None));
		SetButtonGroup(gType);
		FriendlyBtnGroup.SetListener(this);
		ClosenessBtnGroup.SetListener(this);
		ConnectBtnGroup.SetListener(this);
	}

	private void UpdateBtnGroupLabel()
	{
		FriendlyBtnGroup.SetButtonLabel();
		ClosenessBtnGroup.SetButtonLabel();
		ConnectBtnGroup.SetButtonLabel();
	}

	private void SetButtonGroup(EGroupButtonType gType, bool isInit = true)
	{
		if (isInit)
		{
			SetButtonGroup(FriendlyBtnGroup, gType, EGroupButtonType.Friendly);
			SetButtonGroup(ClosenessBtnGroup, gType, EGroupButtonType.Closeness);
			SetButtonGroup(ConnectBtnGroup, gType, EGroupButtonType.Connect);
		}
		if (gType == EGroupButtonType.Closeness && prevButtonType != EGroupButtonType.Closeness)
		{
			SetAnim("ClickCloseness", isInit, gType);
			prevButtonType = EGroupButtonType.Closeness;
		}
		if (gType == EGroupButtonType.Connect && prevButtonType != EGroupButtonType.Connect)
		{
			SetAnim("ClickConnectTalk", isInit, gType);
			prevButtonType = EGroupButtonType.Connect;
		}
	}

	private void SetButtonGroup(HeroInnViewButtonGroup group, EGroupButtonType type, EGroupButtonType compareType)
	{
		if (!(group == null))
		{
			UIHelper.EnableGameObject(group, type == compareType);
		}
	}

	private void SetButnGroupTitle(bool isHave)
	{
		UIHelper.EnableGameObject(FriendlyTitleRoot, !isHave && IsSelectedHero());
		UIHelper.EnableGameObject(ClosenessTitleRoot, isHave);
		UIHelper.EnableGameObject(ConnectTitleRoot, isHave);
	}

	private void SetAnim(string triggerName, bool isInit, EGroupButtonType gType)
	{
		ResetAnim();
		Anim_Closeness.SetBool(triggerName, value: true);
		VespaCoroutine.Start(WaitAnimator(isInit, gType));
	}

	private IEnumerator WaitAnimator(bool isInit, EGroupButtonType gType)
	{
		yield return new WaitForSeconds(0.5f);
		ResetAnim();
		SetButtonGroup(FriendlyBtnGroup, gType, EGroupButtonType.Friendly);
		SetButtonGroup(ClosenessBtnGroup, gType, EGroupButtonType.Closeness);
		SetButtonGroup(ConnectBtnGroup, gType, EGroupButtonType.Connect);
	}

	private void ResetAnim()
	{
		Anim_Closeness.SetBool("ClickCloseness", value: false);
		Anim_Closeness.SetBool("ClickConnectTalk", value: false);
	}

	public override void UpdateDerived()
	{
		base.UpdateDerived();
		if (Time.time - _lastLabelUpdateTime > 0.1f)
		{
			_lastLabelUpdateTime = Time.time;
			UpdateLabel();
			UpdateRefresh();
		}
	}

	private void UpdateHeroConversationLabel()
	{
		HeroFriendlyActionData data = GameRoot.Instance.HeroFriendlyActionDataContainer.GetData(base.HeroIndex);
		if (data != null)
		{
			int num = Random.Range(0, __Extension_Method_ICollection__.COUNT(data.BaseSpeak));
			UIHelper.SetLabelText(HeroChatLabel, LocalizationManager.GetStringDirect(data.BaseSpeak[num]));
		}
	}

	private void UpdateUIWidget(bool baseSpeakAction = true)
	{
		bool flag = SetFriendlyGauge();
		bool flag2 = IsHaveHero();
		bool flag3 = IsSelectedHero();
		bool flag4 = !flag2 && !flag3;
		UIHelper.EnableGameObject(WidgetSelect, flag4);
		UIHelper.EnableGameObject(FriendlyWidget, !flag4);
		if (flag4)
		{
			if (_listener.CheckAreadySelectHero())
			{
				int userSelectHeroIndex = GlobalAutoBehaviour<AccountManager>.instance.HeroFriendlyManager.GetUserSelectHeroIndex();
				CreatureData data = GameRoot.Instance.CreatureDataContainer.GetData(userSelectHeroIndex);
				if (data == null)
				{
					UIHelper.EnableGameObject(HeroSelectLabel, enable: false);
					return;
				}
				UIHelper.SetLabelText(HeroSelectLabel, LocalizationManager.GetStringDirect("HEROINN_ANOTHER_VISIT", data.Name));
				UIHelper.EnableGameObject(HeroSelectLabel, enable: true);
				UIHelper.EnableGameObject(HeroLeaveLabel, enable: false);
			}
			else
			{
				string stringDirect = LocalizationManager.GetStringDirect("HEROINN_VISIT", base.CreatureData.Name);
				string stringDirect2 = LocalizationManager.GetStringDirect("HEROINN_VISIT_LEAVE");
				string text = $"{stringDirect}\n{stringDirect2}";
				UIHelper.SetLabelText(HeroSelectLabel, text);
				UIHelper.EnableGameObject(HeroSelectLabel, enable: true);
				UIHelper.EnableGameObject(HeroLeaveLabel, enable: false);
			}
		}
		else
		{
			if (flag2)
			{
				SetClosenessGauge();
			}
			if (flag3 && flag)
			{
				UIHelper.EnableGameObject(RightButtonRoot, enable: false);
			}
			else
			{
				UIHelper.EnableGameObject(RightButtonRoot, enable: true);
			}
			UIHelper.SetLabelText(HeroNameLabel, base.CreatureData.Name);
			int valueInt = GameRoot.Instance.ConstantDataContainer.GetValueInt("MaxFriendshipPoint");
			UIHelper.SetLabelText(GoldValueLabel, Vespa_Long_Extension_Method.ToCommaString(GlobalAutoBehaviour<AccountManager>.instance.UserManager.Gold));
			UIHelper.SetLabelText(FriendshipValueLabel, $"{GlobalAutoBehaviour<AccountManager>.instance.UserManager.FriendshipPoint}/{valueInt}");
			if (baseSpeakAction)
			{
				UpdateHeroConversationLabel();
			}
		}
		UIHelper.EnableGameObject(ChangeRecruitButton, flag3);
		UIHelper.EnableGameObject(RecruitButton, flag3 && flag);
		UIHelper.EnableButton(ChangeRecruitButton, flag3 && !flag);
	}

	public override void UpdateUI(bool bSelectHero, bool BaseSpeakAction = true)
	{
		if (base.HeroInfo != null && base.CreatureData != null)
		{
			InitHeroPortraits();
			SetButnGroupTitle(IsHaveHero());
			UpdateUIWidget(BaseSpeakAction);
		}
	}

	private void UpdateRefresh()
	{
		if (GetSelectCoolTime())
		{
			_listener.RequestRefresh();
		}
	}

	private void UpdateLabel()
	{
		if (IsHaveHero())
		{
			ClosenessBtnGroup.UpdateButtons();
			ConnectBtnGroup.UpdateButtons();
		}
		else if (IsSelectedHero())
		{
			FriendlyBtnGroup.UpdateButtons();
		}
	}

	bool HeroInnViewButtonGroup.Listener.CheckCoolTime(FriendlyActionType friendlyAction)
	{
		switch (friendlyAction)
		{
		case FriendlyActionType.Greeting:
		case FriendlyActionType.Conversation:
		case FriendlyActionType.Gift:
			return GetCoolTime(friendlyAction);
		case FriendlyActionType.Closeness_Greeting:
		case FriendlyActionType.Closeness_Conversation:
		case FriendlyActionType.Closeness_Gift:
			if (GetCoolTime(friendlyAction))
			{
				return !CheckClosenessPoint();
			}
			return false;
		default:
			return true;
		}
	}

	bool HeroInnViewButtonGroup.Listener.CheckCost(FriendlyActionType friendlyAction)
	{
		switch (friendlyAction)
		{
		case FriendlyActionType.Greeting:
		case FriendlyActionType.Conversation:
		case FriendlyActionType.Gift:
		case FriendlyActionType.Closeness_Greeting:
		case FriendlyActionType.Closeness_Conversation:
		case FriendlyActionType.Closeness_Gift:
		{
			ShopCostType costType;
			int cost;
			return GetCost(friendlyAction, out costType, out cost);
		}
		case FriendlyActionType.Greeting_Max:
			return CheckClosenessPoint();
		case FriendlyActionType.Connect_Talk_1:
		case FriendlyActionType.Connect_Talk_2:
		case FriendlyActionType.Connect_Talk_3:
			return CheckConnectTalkCost(friendlyAction);
		default:
			return true;
		}
	}

	bool HeroInnViewButtonGroup.Listener.CheckClosenessPoint()
	{
		return CheckClosenessPoint();
	}

	int HeroInnViewButtonGroup.Listener.GetCostValue(FriendlyActionType friendlyAction)
	{
		switch (friendlyAction)
		{
		case FriendlyActionType.Greeting:
		case FriendlyActionType.Conversation:
		case FriendlyActionType.Gift:
		case FriendlyActionType.Closeness_Greeting:
		case FriendlyActionType.Closeness_Conversation:
		case FriendlyActionType.Closeness_Gift:
		{
			GetCost(friendlyAction, out var _, out var cost);
			return cost;
		}
		case FriendlyActionType.Greeting_Max:
			return GetNeedFriendlyPoint();
		case FriendlyActionType.Connect_Talk_1:
		case FriendlyActionType.Connect_Talk_2:
		case FriendlyActionType.Connect_Talk_3:
			return GetConnectTalkPoint(friendlyAction);
		default:
			return 0;
		}
	}

	string HeroInnViewButtonGroup.Listener.GetCostKey(FriendlyActionType friendlyAction)
	{
		switch (friendlyAction)
		{
		case FriendlyActionType.Greeting:
		case FriendlyActionType.Closeness_Greeting:
			return "COMMON_FREE_BUY";
		case FriendlyActionType.Gift:
		case FriendlyActionType.Closeness_Gift:
			return "HEROINN_GOLD_COUNT";
		case FriendlyActionType.Conversation:
		case FriendlyActionType.Closeness_Conversation:
			return "HEROINN_FRIENDSHIP_COUNT";
		case FriendlyActionType.Connect_Talk_1:
		case FriendlyActionType.Connect_Talk_2:
		case FriendlyActionType.Connect_Talk_3:
		case FriendlyActionType.Greeting_Max:
			return "CLOSENESS_TALK_UNLOCK";
		default:
			return "COMMON_FREE_BUY";
		}
	}

	bool IUserObserver.IsGlobal()
	{
		return false;
	}

	public void OnChanged(UserManager.UserStatInfo userStatInfo)
	{
		switch (userStatInfo.UserStatType)
		{
		case UserManager.UserStatType.FriendshipPoint:
		{
			int valueInt = GameRoot.Instance.ConstantDataContainer.GetValueInt("MaxFriendshipPoint");
			UIHelper.SetLabelText(FriendshipValueLabel, $"{GlobalAutoBehaviour<AccountManager>.instance.UserManager.FriendshipPoint}/{valueInt}");
			break;
		}
		case UserManager.UserStatType.Gold:
			UIHelper.SetLabelText(GoldValueLabel, Vespa_Long_Extension_Method.ToCommaString(GlobalAutoBehaviour<AccountManager>.instance.UserManager.Gold));
			break;
		}
	}
}
