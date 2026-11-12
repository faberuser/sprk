using System;
using System.Collections;
using NGame2.NAccount;
using NGame2.NDropBonusEvent;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NManager;
using NGame2.NUI.NManager.NLobby;
using NShared;
using NShared.MaxPurifyHero;
using NShared.NLocalization;
using NShared.PurifyHero;
using NShared.TranscendHero;
using NShared.UpgradeHeroStar;
using NVespa;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using NVespa.NUtil;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class PurifyHeroRoom : FrameControlWindow, IUserObserver<NGame2.NAccount.UserHeroManager.ChangeHeroInfo>, IUserObserver, IUserObserver<UserManager.UserStatInfo>, IInspectorGuiDrawer
{
	public interface Listener
	{
		void ClickBuyGold();
	}

	public const string PrefabPath = "Resources/Prefab2/Window/PurifyHero/PurifyHeroRoom";

	public UITexture Hero3DTexture;

	public UITweener BottomTween;

	public UITweener TopTween;

	public UITweener HighlightTween;

	public UILabel Label_PurifyDesc;

	public UITexture Texture_ItemIcon;

	public UIWidget[] WidgetStars;

	public UITexture Texture_Gauge;

	public UITexture Texture_GaugeBg;

	public UIButton Button_MaxPurify;

	public UIButton Button_Purify;

	public UIButton Button_Awake;

	public UILabel Label_Awake;

	public UILabel Label_Transcend;

	public UILabel Label_PurifyCost;

	public UILabel Label_PlayerGold;

	public UIGame3DWidget UiEffect_Gauge;

	public UIPlayAssetSound PurifyProgressSound;

	public UISimpleGrid AwakeMaterialItemGrid;

	private Listener _listener;

	private int _PurifyGold;

	private int _maxPurified;

	private PurifyScene _purifyScene = new PurifyScene();

	private float _lastUpdatePurifyGoldTime;

	private ItemIconComponent _AwakeMaterialItem;

	private bool _isShortCut;

	private bool _playingAwakenAnimation;

	public CachedHeroInfo HeroInfo { get; private set; }

	public CreatureData HeroData { get; private set; }

	public HeroAwakeData HeroAwakeData { get; private set; }

	public int Star { get; private set; }

	public int Level { get; private set; }

	public void Open(CachedHeroInfo heroInfo, CreatureData heroData, Listener listener, bool isShortCut)
	{
		Open(delegate
		{
			Init(heroInfo, heroData, listener, isShortCut);
		}, null);
	}

	public static int GetMaxPurified(int star, int transcend)
	{
		CreatureStarData nextData = GameRoot.Instance.CreatureStarDataContainer.GetNextData(star, transcend);
		if (nextData == null)
		{
			VespaDebug.LogError("Cannot find star data");
			return 0;
		}
		return nextData.MaxPurified;
	}

	public static bool IsAwakend(int star)
	{
		if (star == GameRoot.Instance.ConstantDataContainer.GetValueInt("HeroMaxStar"))
		{
			return false;
		}
		return true;
	}

	public void Init(CachedHeroInfo heroInfo, CreatureData heroData, Listener listener, bool isShortCut)
	{
		_maxPurified = GetMaxPurified(heroInfo.Star, heroInfo.Transcended);
		HeroData = heroData;
		HeroAwakeData = GameRoot.Instance.HeroAwakeDataContainer.GetData(heroInfo.HeroIndex);
		HeroInfo = heroInfo;
		Star = HeroInfo.Star;
		Level = HeroInfo.Level;
		_listener = listener;
		float purifyRatio = (float)HeroInfo.Purified / (float)_maxPurified;
		_purifyScene.Open(Hero3DTexture, HeroInfo, purifyRatio, IsAwakend(Star));
		SingletonUpdater<Executer>.instance.NextRun(InitPurifedFillAmount);
		SetupHeroPurifyItem();
		InitPurifyGold();
		SetupPurifyButtons();
		_purifyScene.PurifyAction(isReaction: false, blendAction: false);
		SprkResetPurifyTween(BottomTween);
		SprkResetPurifyTween(TopTween);
		SprkResetPurifyTween(HighlightTween);
		if (UiEffect_Gauge != null)
		{
			UiEffect_Gauge.Stop(immediatelyClear: true);
			UiEffect_Gauge.Play(null);
		}
		_isShortCut = isShortCut;
		_playingAwakenAnimation = false;
	}

	public override void Reset()
	{
		base.Reset();
		VespaCoroutine.StopAll(this);
		_listener = null;
		if (UiEffect_Gauge != null)
		{
			UiEffect_Gauge.Stop(immediatelyClear: true);
		}
		FactoryBase.SafeReleaseComponent(ref _AwakeMaterialItem);
		_purifyScene.Close();
	}

	public override void OnClosing()
	{
		base.OnClosing();
	}

	public override void UpdateDerived()
	{
		base.UpdateDerived();
		_purifyScene.ChainUpdate();
		if (Texture_Gauge != null && Texture_GaugeBg != null)
		{
			Rect uvRect = Texture_Gauge.uvRect;
			uvRect.x = Mathf.Repeat(uvRect.x + GlobalTimer.deltaTime * GameSetting.data.Ui.PurifyHero_GaugeScrollSpeed, 1f);
			Texture_Gauge.uvRect = uvRect;
			Texture_GaugeBg.uvRect = uvRect;
		}
		UpdatePriceByEvent();
	}

	private void InitPurifedFillAmount()
	{
		float fillAmount = (float)HeroInfo.Purified / (float)_maxPurified;
		UpdatePurifedFillAmount(fillAmount);
	}

	public void UpdatePurifedFillAmount(float fillAmount)
	{
		fillAmount = Mathf.Clamp(fillAmount, 0f, 1f);
		UIHelper.SetTextureFillAmount(Texture_Gauge, fillAmount);
		object key = ((!IsAwakend(Star)) ? HeroPurify.HERO_CUR_PURIFY_TRANSCEND_DESC : HeroPurify.HERO_CUR_PURIFY_DESC);
		int num = Convert.ToInt32(fillAmount * 100f);
		string text = LocalizationManager.GetString((Enum)key, num);
		UIHelper.SetLabelText(Label_PurifyDesc, text);
		Vector3[] worldCorners = Texture_GaugeBg.worldCorners;
		float x = worldCorners[0].x;
		float x2 = worldCorners[2].x;
		UiEffect_Gauge.transform.position = Vespa_Unity_Vector3_Extension_Method.ReplaceX(UiEffect_Gauge.transform.position, Mathf.Lerp(x, x2, fillAmount));
		if (fillAmount < 0.5f - Mathf.Epsilon)
		{
			_purifyScene.PurifyState = PurifyScene.EPurifyState.Begin;
		}
		else if (fillAmount < 1f - Mathf.Epsilon)
		{
			_purifyScene.PurifyState = PurifyScene.EPurifyState.Half;
		}
		else
		{
			_purifyScene.PurifyState = PurifyScene.EPurifyState.Full;
		}
		_purifyScene.UpdatePurifyRatio(fillAmount);
	}

	private void UpdatePriceByEvent()
	{
		if (!(Time.time - _lastUpdatePurifyGoldTime < 1f))
		{
			_lastUpdatePurifyGoldTime = Time.time;
			if (!_playingAwakenAnimation)
			{
				InitPurifyGold();
			}
		}
	}

	private void SetupPurifyButtons()
	{
		int maxPurified = _maxPurified;
		SetupPurifyButtons(HeroInfo.Purified == maxPurified);
	}

	public void SetupPurifyButtons(bool isMaxPurified)
	{
		UIHelper.EnableGameObject(Button_Purify, !isMaxPurified);
		UIHelper.EnableGameObject(Button_MaxPurify, !isMaxPurified);
		UIHelper.EnableGameObject(Button_Awake, isMaxPurified);
		bool enable = _PurifyGold <= GlobalAutoBehaviour<AccountManager>.instance.UserManager.Gold;
		UIHelper.EnableButton(Button_Purify, enable: true);
		UIHelper.EnableButton(Button_MaxPurify, enable);
		UIHelper.EnableButton(Button_Awake, enable: true);
		UIHelper.EnableGameObject(Label_Awake, isMaxPurified && IsAwakend(Star));
		UIHelper.EnableGameObject(Label_Transcend, isMaxPurified && !IsAwakend(Star));
	}

	private void SetupHeroPurifyItem()
	{
		ItemData itemData = null;
		itemData = ((!NGame2.NAccount.UserHeroManager.IsMaxStar(HeroInfo)) ? GameRoot.Instance.ItemDataContainer.GetDataByCode(HeroAwakeData.GetAwakeMaterialItemCode(HeroInfo.Star)) : GameRoot.Instance.ItemDataContainer.GetDataByCode(HeroAwakeData.GetTranscendMaterialItemCode(HeroInfo.Transcended)));
		if (itemData == null)
		{
			VespaDebug.LogError("Item Data not found");
			return;
		}
		_AwakeMaterialItem = UIHelper.AddGridItem(AwakeMaterialItemGrid, ItemIconComponent.PrefabAuto);
		_AwakeMaterialItem.Open(itemData);
	}

	private void InitPurifyGold()
	{
		CreatureStarData nextData = GameRoot.Instance.CreatureStarDataContainer.GetNextData(HeroInfo.Star, HeroInfo.Transcended);
		if (nextData == null)
		{
			VespaDebug.LogError("CreaturePurifyData not found");
			return;
		}
		int num = nextData.Gold;
		DropBonusEvent effectiveEvent = GlobalAutoBehaviour<DropBonusEventManager>.instance.GetEffectiveEvent(EventBonusType.DiscountPurify, null);
		if (effectiveEvent != null)
		{
			int num2 = num * effectiveEvent.BonusPercentage / 100;
			num -= num2;
			num = Mathf.Max(num, 0);
		}
		InitPurifyGold(num);
	}

	public void InitPurifyGold(int purifyGold)
	{
		_PurifyGold = purifyGold;
		SetupPurifyCost();
	}

	private void SetupPurifyCost()
	{
		string costGoldString = GetCostGoldString(_PurifyGold, GlobalAutoBehaviour<AccountManager>.instance.UserManager.Gold);
		UIHelper.SetLabelText(Label_PurifyCost, costGoldString);
		UIHelper.SetLabelText(Label_PlayerGold, Vespa_Long_Extension_Method.ToCommaString(GlobalAutoBehaviour<AccountManager>.instance.UserManager.Gold));
	}

	public static string GetCostGoldString(int cost, long playerGold)
	{
		string text = Vespa_Int_Extension_Method.ToCommaString(cost);
		if (cost > playerGold)
		{
			text = "[FF0000]" + text + "[-]";
		}
		return text;
	}

	private void OpenNotEnoughGoldPopup()
	{
		string text = LocalizationManager.GetString(NShared.NLocalization.Common.INFORM);
		string message = LocalizationManager.GetString(NShared.NLocalization.System.MOVE_TO_GOLD_SHOP_CONFIRM);
		MessagePopup.Open(text, message, delegate
		{
			Close();
			SceneAutoBehaviour<PayShopManagement>.instance.OpenNewPayShop(NewPayShopCategoryType.Gold.ToString());
		}, null);
	}

	public void OnClickPurify()
	{
		if (GlobalAutoBehaviour<AccountManager>.instance.UserManager.Gold < _PurifyGold)
		{
			OpenNotEnoughGoldPopup();
		}
		else
		{
			VespaCoroutine.Start(this, coPurifyHero());
		}
	}

	private IEnumerator coPurifyHero()
	{
		NShared.PurifyHero.Request request = new NShared.PurifyHero.Request
		{
			HeroIndex = HeroInfo.HeroIndex
		};
		short oldPurified = HeroInfo.Purified;
		UIHelper.EnableButton(Button_Purify, enable: false);
		UIHelper.EnableButton(Button_MaxPurify, enable: false);
		bool executeEnd = false;
		bool isSuccess = false;
		int resultPurified = oldPurified;
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.PurifyHero.Response response)
		{
			executeEnd = true;
			if (response.BaseResult != BaseResultType.Success)
			{
				MessagePopup.OpenError(response.BaseResult);
			}
			else if (response.Result != NShared.PurifyHero.ResultType.Success)
			{
				MessagePopup.OpenError(response.Result);
			}
			else
			{
				GlobalAutoBehaviour<AccountManager>.instance.HeroManager.ApplyHeroPurified(response.PurifyResult);
				GlobalAutoBehaviour<AccountManager>.instance.UserManager.Apply(response.CurrencyResult);
				isSuccess = true;
				resultPurified = response.PurifyResult.NewValue;
			}
		});
		while (!executeEnd)
		{
			yield return null;
		}
		if (!isSuccess)
		{
			SetupPurifyButtons();
			yield break;
		}
		if (PurifyProgressSound != null)
		{
			PurifyProgressSound.Play();
		}
		PlayPurifyHeroAnim(oldPurified, resultPurified, null);
	}

	public void PlayPurifyHeroAnim(float oldPurified, float resultPurified, Action onEnd)
	{
		VespaCoroutine.Start(this, coPurifyHeroAnim(oldPurified, resultPurified, onEnd));
	}

	private IEnumerator coPurifyHeroAnim(float oldPurified, float resultPurified, Action onEnd)
	{
		UIHelper.EnableButton(Button_Purify, enable: false);
		UIHelper.EnableButton(Button_MaxPurify, enable: false);
		HighlightTween.PlayForwardReset();
		bool executeEnd = false;
		_purifyScene.CreatePurifyTrail(delegate
		{
			executeEnd = true;
		});
		while (!executeEnd)
		{
			yield return null;
		}
		_purifyScene.PurifyAction(isReaction: true);
		yield return new WaitForSeconds(0.1f);
		SetupPurifyButtons();
		SetupPurifyCost();
		float elapsedTime = 0f;
		float timeLength = 0.5f;
		do
		{
			yield return null;
			elapsedTime += GlobalTimer.deltaTime;
			float t = Mathf.Clamp01(elapsedTime / timeLength);
			float num = Mathf.Lerp(oldPurified, resultPurified, t);
			UpdatePurifedFillAmount(num / (float)_maxPurified);
		}
		while (!(elapsedTime >= timeLength));
		Vespa_System_Action_Extension_Method.fire(onEnd);
	}

	public void OnClickAwake()
	{
		VespaCoroutine.Start(this, coAwakeHero());
	}

	private IEnumerator coAwakeHero()
	{
		UIHelper.EnableButton(Button_Awake, enable: false);
		bool executeEnd = false;
		bool isSuccess = false;
		ExpResultInfo expResult = null;
		GetNewHero2.EOpenMode openMode = GetNewHero2.EOpenMode.None;
		if (NGame2.NAccount.UserHeroManager.IsMaxStar(HeroInfo))
		{
			openMode = GetNewHero2.EOpenMode.Transcend;
			NShared.TranscendHero.Request request = new NShared.TranscendHero.Request
			{
				HeroIndex = HeroInfo.HeroIndex
			};
			SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.TranscendHero.Response response)
			{
				executeEnd = true;
				if (response.BaseResult != BaseResultType.Success)
				{
					MessagePopup.OpenError(response.BaseResult);
				}
				else if (response.Result != NShared.TranscendHero.ResultType.Success)
				{
					MessagePopup.OpenError(response.Result);
				}
				else
				{
					HeroInfo.Purified = 0;
					GlobalAutoBehaviour<AccountManager>.instance.HeroManager.SetTranscended(HeroInfo.HeroIndex, response.NewTranscended);
					GlobalAutoBehaviour<AccountManager>.instance.ItemManager.Apply(response.ItemResult);
					GlobalAutoBehaviour<AccountManager>.instance.UserManager.Apply(response.StaminaResult);
					expResult = response.TeamExpResult;
					isSuccess = true;
					_ = (object)null;
					_ = 21;
					_ = 1;
					_ = 0;
					(new object[1])[0] = response.NewTranscended;
				}
			});
		}
		else
		{
			openMode = GetNewHero2.EOpenMode.Awaken;
			NShared.UpgradeHeroStar.Request request2 = new NShared.UpgradeHeroStar.Request
			{
				HeroIndex = HeroInfo.HeroIndex
			};
			SingletonUpdater<WebServiceManager>.instance.Requester.Request(request2, delegate(NShared.UpgradeHeroStar.Response response)
			{
				executeEnd = true;
				if (response.BaseResult != BaseResultType.Success)
				{
					MessagePopup.OpenError(response.BaseResult);
				}
				else if (response.Result != NShared.UpgradeHeroStar.ResultType.Success)
				{
					MessagePopup.OpenError(response.Result);
				}
				else
				{
					HeroInfo.Purified = 0;
					GlobalAutoBehaviour<AccountManager>.instance.HeroManager.SetStar(HeroInfo.HeroIndex, response.NewHeroStar);
					GlobalAutoBehaviour<AccountManager>.instance.ItemManager.Apply(response.ItemResult);
					GlobalAutoBehaviour<AccountManager>.instance.UserManager.Apply(response.StaminaResult);
					expResult = response.TeamExpResult;
					isSuccess = true;
					_ = (object)null;
					_ = 20;
					_ = 1;
					_ = 0;
					(new object[1])[0] = response.NewHeroStar;
				}
			});
		}
		while (!executeEnd)
		{
			yield return null;
		}
		if (!isSuccess)
		{
			SetupPurifyButtons();
			yield break;
		}
		if (isSuccess && _isShortCut)
		{
			SceneAutoBehaviour<MyHeroManagement>.instance.CloseAllActiveWindow(immediately: true);
		}
		_playingAwakenAnimation = true;
		PlayAwakeHeroAnim(expResult, delegate
		{
			_playingAwakenAnimation = false;
			if (openMode == GetNewHero2.EOpenMode.Transcend)
			{
				SceneAutoBehaviour<LobbyPopupQueue>.instance.AddPopup(MoveToHeroAttrPopup.instance, delegate
				{
					MoveToHeroAttrPopup.Open(HeroInfo.HeroIndex, HeroInfo.Transcended, delegate
					{
						if (!_isShortCut)
						{
							GlobalAutoBehaviour<GlobalManagement>.instance.ClearAllOpenedWindowStack();
							SceneAutoBehaviour<HeroTrialDungeonManagement>.instance.CloseAllActiveWindow(immediately: true);
							SceneAutoBehaviour<EntryMenuManagement>.instance.CloseEntryMenu(delegate
							{
								SceneAutoBehaviour<MyHeroManagement>.instance.OpenMyHero(HeroInfo, Hero3DViewPanel.EToggle.Attribute);
							});
						}
						else
						{
							SceneAutoBehaviour<HeroTrialDungeonManagement>.instance.CloseAllActiveWindow(immediately: true);
							SceneAutoBehaviour<MyHeroManagement>.instance.OpenMyHero(HeroInfo, Hero3DViewPanel.EToggle.Attribute);
						}
					});
				});
			}
		}, openMode, showNewHeroPopup: true);
	}

	public void PlayAwakeHeroAnim(ExpResultInfo expResult, Action onEnd, GetNewHero2.EOpenMode openMode, bool showNewHeroPopup)
	{
		VespaCoroutine.Start(this, coAwakeHeroAnim(expResult, onEnd, openMode, showNewHeroPopup));
	}

	private IEnumerator coAwakeHeroAnim(ExpResultInfo expResult, Action onEnd, GetNewHero2.EOpenMode openMode, bool showNewHeroPopup)
	{
		UIHelper.EnableButton(Button_Awake, enable: false);
		BottomTween.PlayForwardReset();
		TopTween.PlayForwardReset();
		_purifyScene.GetPurifyCamera().PlayAwake();
		yield return null;
		if (showNewHeroPopup)
		{
			Close();
			SceneAutoBehaviour<LobbyManagement>.instance.OpenGetNewHero(GetNewHero2.Stock.Create(HeroInfo, openMode, hideInfo: false), reserveWindow: true, null, null);
			GlobalAutoBehaviour<AccountManager>.instance.UserManager.Apply(expResult);
		}
		Vespa_System_Action_Extension_Method.fire(onEnd);
	}

	public void OnClickBuyGold()
	{
		if (_listener != null)
		{
			_listener.ClickBuyGold();
		}
	}

	public void OnClickExitButton()
	{
		Close();
	}

	public void OnClickMaxPurify()
	{
		if (GlobalAutoBehaviour<AccountManager>.instance.UserManager.Gold < _PurifyGold)
		{
			OpenNotEnoughGoldPopup();
			return;
		}
		MaxPurifyPopup.Open(HeroInfo, delegate
		{
			if (base.gameObject.activeInHierarchy)
			{
				VespaCoroutine.Start(this, coMaxPurifyHero());
			}
		});
	}

	private IEnumerator coMaxPurifyHero()
	{
		NShared.MaxPurifyHero.Request request = new NShared.MaxPurifyHero.Request
		{
			HeroIndex = HeroInfo.HeroIndex
		};
		short oldPurified = HeroInfo.Purified;
		UIHelper.EnableButton(Button_Purify, enable: false);
		UIHelper.EnableButton(Button_MaxPurify, enable: false);
		bool executeEnd = false;
		bool isSuccess = false;
		int resultPurified = oldPurified;
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.MaxPurifyHero.Response response)
		{
			executeEnd = true;
			if (response.BaseResult != BaseResultType.Success)
			{
				MessagePopup.OpenError(response.BaseResult);
			}
			else if (response.Result != NShared.MaxPurifyHero.ResultType.Success)
			{
				MessagePopup.OpenError(response.Result);
			}
			else
			{
				GlobalAutoBehaviour<AccountManager>.instance.HeroManager.ApplyHeroPurified(response.PurifyResult);
				GlobalAutoBehaviour<AccountManager>.instance.UserManager.Apply(response.CurrencyResult);
				isSuccess = true;
				resultPurified = response.PurifyResult.NewValue;
			}
		});
		while (!executeEnd)
		{
			yield return null;
		}
		if (!isSuccess)
		{
			SetupPurifyButtons();
			yield break;
		}
		if (PurifyProgressSound != null)
		{
			PurifyProgressSound.Play();
		}
		PlayPurifyHeroAnim(oldPurified, resultPurified, null);
	}

	void IUserObserver<NGame2.NAccount.UserHeroManager.ChangeHeroInfo>.OnChanged(NGame2.NAccount.UserHeroManager.ChangeHeroInfo info)
	{
	}

	void IUserObserver<UserManager.UserStatInfo>.OnChanged(UserManager.UserStatInfo info)
	{
		if (info.UserStatType == UserManager.UserStatType.Gold)
		{
			SetupPurifyCost();
		}
	}

	bool IUserObserver.IsGlobal()
	{
		return false;
	}

	private static void SprkResetPurifyTween(UITweener tween)
	{
		tween.enabled = false;
		tween.ResetToBeginning();
	}
}
