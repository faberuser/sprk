using System;
using System.Collections;
using System.Collections.Generic;
using NGame2.N3D;
using NGame2.NAccount;
using NGame2.NNodeTree;
using NGame2.NNodeTree.NUtil;
using NGame2.NPayShopItemEvent;
using NGame2.NRender;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NComponent2.NPayShop;
using NGame2.NUI.NHelper.NPayShop;
using NGame2.NUI.NManager;
using NGame2.NUI.NRender3D;
using NGame2.NUI.NRender3D.NRenderTargetCinema;
using NGameAsset;
using NGameAsset.NCreature;
using NNguiExtension;
using NShared;
using NShared.NLocalization;
using NVespa;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow.NPayShop;

public class PayShopHeroInfoPopup : BaseWindow, PayShopResourceList.IListener, IUserObserver<UserManager.UserStatInfo>, IUserObserver, IUserObserver<PayShopItemEventManager.EventChangedInfo>
{
	public enum HeroGetType
	{
		HeroBuy,
		HeroSelect
	}

	public enum PageDirection
	{
		Prev,
		Next
	}

	public interface IListener
	{
		void OnClickBuy(CreatureData creatureData);

		void OnClickViewHeroDetail(CachedHeroInfo heroInfo);

		void OnTryBattle(HeroInfo heroInfo);

		void OnChangedHero(int heroIndex);
	}

	public const string PrefabPath = "Resources/Prefab2/Window/PayShop/PayShopHeroInfoPopup";

	public PayShopResourceList PayShopResourceList;

	public UIBundleLoader BackgroundLoader;

	public UILabel Label_HeroName;

	public HeroInUIComponent HeroComponent;

	public UISpriteSelector StarSelector;

	public UISpriteSelector TranscendStarSelector;

	public UIScrollGrid SkillListGrid;

	public UISimpleGrid AttributeListGrid;

	public Transform PricePosition;

	public UILabel ObtainLabel;

	public Transform BonusStaminaRoot;

	public UILabel BonusStaminaLabel;

	public Transform BonusMileageRoot;

	public UILabel BonusMileageLabel;

	public GameObject ButtonRight;

	public GameObject ButtonLeft;

	public GameObject ButtonHeroBattle;

	public GameObject ButtonHeroView;

	public GameObject WidgetWarning;

	public GameObject ButtonYes;

	public List<UITweener> TweenList = new List<UITweener>();

	public float NPCActionDelaySecond = 5f;

	public float NPCChangeDelaySecond = 0.5f;

	public int RefreshGCCallCount = 5;

	public const int HeroVisualStar = 5;

	private List<CreatureData> _listHeroes = new List<CreatureData>();

	private List<PayShopSkillIconComponent> _listSkillIcons = new List<PayShopSkillIconComponent>();

	private List<AttributeIconComponent> _listAttributeIcons = new List<AttributeIconComponent>();

	private PayShopPriceComponent _priceComponent;

	private IListener _listener;

	private HeroGetType _heroGetType;

	private CachedHeroInfo _cachedHeroInfo;

	private int _heroChangedCount;

	private DateTime _motionStartedTime = DateTime.MinValue;

	private string _appearMotionKey = "";

	private HeroRenderCinema _cinema;

	private HeroCameraController _cameraController;

	private int _heroUid = 1;

	private string _nodeTreeName = "GetNewHero";

	public CreatureData CreatureData { get; private set; }

	public int HeroStar { get; private set; }

	public int HeroTranscended { get; private set; }

	public int HeroLevel { get; private set; }

	public int CurrentIndex { get; private set; }

	void PayShopResourceList.IListener.OnClickStamina()
	{
		CloseActiveWindow();
		SceneAutoBehaviour<PayShopManagement>.instance.OpenNewPayShop(NewPayShopCategoryType.Stamina.ToString());
	}

	void PayShopResourceList.IListener.OnClickRuby()
	{
		CloseActiveWindow();
		SceneAutoBehaviour<PayShopManagement>.instance.OpenNewPayShop(NewPayShopCategoryType.Ruby.ToString());
	}

	void PayShopResourceList.IListener.OnClickGold()
	{
		CloseActiveWindow();
		SceneAutoBehaviour<PayShopManagement>.instance.OpenNewPayShop(NewPayShopCategoryType.Gold.ToString());
	}

	private void CloseActiveWindow()
	{
		if (SceneAutoBehaviour<PayShopManagement>.isValidInstance)
		{
			SceneAutoBehaviour<PayShopManagement>.instance.CloseAllActiveWindow();
		}
		if (SceneAutoBehaviour<CostumeManagement>.isValidInstance)
		{
			SceneAutoBehaviour<CostumeManagement>.instance.CloseAllActiveWindow();
		}
		if (SceneAutoBehaviour<InventoryManagement>.isValidInstance)
		{
			SceneAutoBehaviour<InventoryManagement>.instance.CloseAllActiveWindow();
		}
	}

	bool IUserObserver.IsGlobal()
	{
		return false;
	}

	void IUserObserver<PayShopItemEventManager.EventChangedInfo>.OnChanged(PayShopItemEventManager.EventChangedInfo changedInfo)
	{
		if (changedInfo.PayShopCategory == PayShopCategory.Hero)
		{
			SetProductPrice(CreatureData);
		}
	}

	void IUserObserver<UserManager.UserStatInfo>.OnChanged(UserManager.UserStatInfo changedInfo)
	{
		PayShopResourceList.OnChanged(changedInfo);
	}

	public override void Reset()
	{
		base.Reset();
		_listener = null;
		PayShopResourceList.ClearAll();
		ResetHero();
		GCCollect();
		SetEtcButton(isEnable: true);
	}

	private void ResetHero()
	{
		CreatureData = null;
		_cachedHeroInfo = null;
		_motionStartedTime = DateTime.MinValue;
		_appearMotionKey = "";
		ClearHeroUI();
	}

	private void ClearHeroUI()
	{
		FactoryBase.ReleaseAndClearList(_listSkillIcons);
		FactoryBase.ReleaseAndClearList(_listAttributeIcons);
		if (HeroComponent != null)
		{
			HeroComponent.Clear();
		}
	}

	private void GCCollect()
	{
		_heroChangedCount = 0;
		SingletonUpdater<GCManager>.instance.Collect(GCManager.ECollectType.Lite);
	}

	public void Open(CreatureData creatureData, List<CreatureData> listHeroes, IListener listener)
	{
		Open(delegate
		{
			_heroGetType = HeroGetType.HeroBuy;
			CreatureData = creatureData;
			_listHeroes = listHeroes;
			_listener = listener;
			_heroChangedCount = 0;
			HeroLevel = 1;
			HeroStar = creatureData.StartHeroStar;
			HeroTranscended = 0;
			CurrentIndex = _listHeroes.FindIndex((CreatureData e) => e.Index == CreatureData.Index);
			Init(creatureData);
		}, null);
	}

	public void OpenHeroSelect(CreatureData creatureData, List<CreatureData> listHeroes, IListener listener, int heroLevel = 0, int heroStar = 0, int heroTranscended = 0)
	{
		base.Open(delegate
		{
			_heroGetType = HeroGetType.HeroSelect;
			CreatureData = creatureData;
			_listHeroes = listHeroes;
			_listener = listener;
			_heroChangedCount = 0;
			HeroLevel = ((_heroGetType == HeroGetType.HeroBuy) ? 1 : heroLevel);
			HeroStar = ((_heroGetType == HeroGetType.HeroBuy) ? creatureData.StartHeroStar : heroStar);
			HeroTranscended = ((_heroGetType != HeroGetType.HeroBuy) ? heroTranscended : 0);
			CurrentIndex = _listHeroes.FindIndex((CreatureData e) => e.Index == CreatureData.Index);
			Init(creatureData);
		});
	}

	public void Init(CreatureData creatureData)
	{
		PayShopResourceList.Open(this);
		SetHeroProduct(creatureData);
	}

	private void SetHeroProduct(CreatureData creatureData)
	{
		ResetToBegineTweens();
		PlayTweens(isForward: true);
		SetProduct(creatureData);
		SetHero(creatureData);
	}

	public void SetBackGround()
	{
		string assetPath = "BackGround/PayShop/PayShop";
		UIHelper.SetBundleTexture(BackgroundLoader, assetPath);
	}

	public void SetProduct(CreatureData creatureData)
	{
		if (GlobalAutoBehaviour<AccountManager>.instance.GetHeroInfo(creatureData.Index) != null)
		{
			UIHelper.EnableGameObject(ButtonYes, enable: false);
			UIHelper.EnableGameObject(BonusMileageRoot, enable: false);
			UIHelper.EnableGameObject(BonusStaminaRoot, enable: false);
		}
		else
		{
			SetBuyButton(creatureData);
			SetBonusProduct(creatureData);
			SetProductPrice(creatureData);
		}
		SetNextPrevButton();
	}

	public void SetHero(CreatureData creatureData)
	{
		_cachedHeroInfo = CreateCachedHeroInfo(creatureData.Index);
		UIHelper.SetLabelText(Label_HeroName, creatureData.Title);
		SetStar(creatureData);
		SetSkill(creatureData);
		SetHeroAttribute(creatureData);
		SetHeroComponent(creatureData);
	}

	public void SetBuyButton(CreatureData creatureData)
	{
		bool num = creatureData.OpenType == CreatureOpenType.Opened;
		bool flag = _heroGetType == HeroGetType.HeroSelect;
		bool enable = num || flag;
		UIHelper.EnableGameObject(ButtonYes, enable);
	}

	private void SetBonusProduct(CreatureData creatureData)
	{
		bool flag = _heroGetType != HeroGetType.HeroSelect;
		bool flag2 = creatureData.OpenType == CreatureOpenType.Opened;
		UIHelper.EnableGameObject(BonusMileageRoot, flag && flag2);
		if (flag && flag2)
		{
			int milieage = PayShopHelper.GetMilieage(creatureData);
			string text = LocalizationManager.GetString(NShared.NLocalization.Common.MILEAGE_COUNT, milieage);
			UIHelper.SetLabelText(BonusMileageLabel, text);
		}
		int addStamina = creatureData.AddStamina;
		string stringDirect = LocalizationManager.GetStringDirect("NEW_PAYSHOP_STAMINA_INCREASE", addStamina);
		UIHelper.EnableGameObject(BonusStaminaRoot, enable: true);
		UIHelper.SetLabelText(BonusStaminaLabel, stringDirect);
	}

	public void SetProductPrice(CreatureData creatureData)
	{
		bool flag = _heroGetType != HeroGetType.HeroSelect;
		if (flag)
		{
			int price = PayShopHelper.GetPrice(creatureData);
			if (_priceComponent == null)
			{
				_priceComponent = PayShopPriceComponent.PrefabAuto.Factory.Create(PricePosition);
			}
			_priceComponent.SetGamePrice(CurrencyType.Gem, price);
		}
		UIHelper.EnableGameObject(PricePosition, flag);
		UIHelper.EnableGameObject(ObtainLabel, !flag);
	}

	private void SetNextPrevButton()
	{
		int num = __Extension_Method_ICollection__.COUNT(_listHeroes) - 1;
		bool flag = _listHeroes.Count > 1;
		UIHelper.EnableGameObject(ButtonRight, flag && CurrentIndex != num);
		UIHelper.EnableGameObject(ButtonLeft, flag && CurrentIndex != 0);
	}

	public void SetEtcButton(bool isEnable)
	{
		UIHelper.EnableGameObject(ButtonHeroBattle, isEnable);
		UIHelper.EnableGameObject(ButtonHeroView, isEnable);
		UIHelper.EnableGameObject(WidgetWarning, isEnable);
	}

	private void SetStar(CreatureData creatureData)
	{
		UIHelper.EnableGameObject(StarSelector, HeroTranscended == 0);
		UIHelper.EnableGameObject(TranscendStarSelector, HeroTranscended > 0);
		if (HeroTranscended == 0 && StarSelector != null)
		{
			StarSelector.SelectInt(HeroStar);
		}
		if (HeroTranscended > 0 && TranscendStarSelector != null)
		{
			TranscendStarSelector.SelectInt(HeroTranscended);
		}
	}

	private void SetSkill(CreatureData creatureData)
	{
		List<int> skillIndexList = creatureData.GetSkillIndexList(includeExtra: false);
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(skillIndexList); i++)
		{
			int skillSlotIndex = i + 1;
			if (skillIndexList[i] != 0)
			{
				PayShopSkillIconComponent payShopSkillIconComponent = UIHelper.AddGridItem(SkillListGrid, PayShopSkillIconComponent.PrefabAuto);
				payShopSkillIconComponent.Open(_cachedHeroInfo, skillSlotIndex);
				_listSkillIcons.Add(payShopSkillIconComponent);
			}
		}
		UIHelper.RepositionGrid(SkillListGrid);
	}

	private void SetHeroAttribute(CreatureData creatureData)
	{
		AttributeIconComponent attributeIconComponent = UIHelper.AddGridItem(AttributeListGrid, AttributeIconComponent.PrefabAuto);
		attributeIconComponent.Open(LocalizationManager.GetString(CreatureData.TagType), creatureData.TagType);
		_listAttributeIcons.Add(attributeIconComponent);
		AttributeIconComponent attributeIconComponent2 = UIHelper.AddGridItem(AttributeListGrid, AttributeIconComponent.PrefabAuto);
		SkillAttrType skillAttrType = ((creatureData.AttrType != null) ? creatureData.AttrType[0] : SkillAttrType.None);
		attributeIconComponent2.Open(LocalizationManager.GetString(skillAttrType), skillAttrType);
		_listAttributeIcons.Add(attributeIconComponent2);
		foreach (Tuple<string, string> heroPosition in creatureData.GetHeroPositionList())
		{
			string item = heroPosition.Item1;
			string stringDirect = LocalizationManager.GetStringDirect(heroPosition.Item2);
			if (String_Extension_Method.IsNullOrEmpty(item) || String_Extension_Method.IsNullOrEmpty(stringDirect))
			{
				break;
			}
			AttributeIconComponent attributeIconComponent3 = UIHelper.AddGridItem(AttributeListGrid, AttributeIconComponent.PrefabAuto);
			attributeIconComponent3.Open(stringDirect, item);
			_listAttributeIcons.Add(attributeIconComponent3);
		}
		UIHelper.RepositionGrid(AttributeListGrid);
	}

	private void SetHeroComponent(CreatureData creatureData)
	{
		if (HeroComponent == null)
		{
			return;
		}
		EnableCreatureWidget(enable: true);
		CreatureInUIVisualData visualData = new CreatureInUIVisualData(new AnimationKeyData(DefineCreature.EMotion.Idle, string.Empty), CreatureStock.ELoadLevel.Normal);
		HeroComponent.Open(_cachedHeroInfo, isPlaySound: true, visualData, delegate
		{
			HeroComponent.RenderFacade.AssetCreature.SetShaderTransform(new ShaderTransformData(-5f, depthShift: false), null);
			ClientCreatureData data = GameRoot.Instance.ClientCreatureDataContainer.GetData(creatureData.CodeName);
			if (data != null)
			{
				if (__Extension_Method_ICollection__.COUNT(data.PayShopOffset) >= 2)
				{
					Vector3 localPosition = new Vector3(data.PayShopOffset[0], data.PayShopOffset[1]);
					if (localPosition.magnitude > 1f)
					{
						HeroComponent.RenderPivot.localPosition = localPosition;
					}
				}
				if (data.PayShopScale > 1f)
				{
					HeroComponent.RenderPivot.localScale = Vector3.one * data.PayShopScale;
				}
				_appearMotionKey = data.PayShopAnimationKey;
			}
			SetAppearMotion();
		});
	}

	public void SetCreatureData(CreatureData creatureData)
	{
		ResetHero();
		PeriodicGCCollect();
		CreatureData = creatureData;
		HeroStar = ((_heroGetType == HeroGetType.HeroBuy) ? creatureData.StartHeroStar : HeroStar);
		SetHeroProduct(creatureData);
	}

	private void PeriodicGCCollect()
	{
		_heroChangedCount++;
		if (_heroChangedCount >= RefreshGCCallCount)
		{
			GCCollect();
		}
	}

	public void EnableCreatureWidget(bool enable)
	{
		UIHelper.EnableGameObject(HeroComponent, enable);
	}

	public override void Close()
	{
		EnableCreatureWidget(enable: false);
		base.Close();
	}

	private CachedHeroInfo CreateCachedHeroInfo(int creatureIndex)
	{
		CachedHeroInfo cachedHeroInfo = new CachedHeroInfo();
		cachedHeroInfo.HeroIndex = creatureIndex;
		cachedHeroInfo.Level = (short)HeroLevel;
		cachedHeroInfo.Star = 5;
		cachedHeroInfo.Transcended = (sbyte)HeroTranscended;
		for (int i = 1; i <= 5; i++)
		{
			cachedHeroInfo.SetSkillLevel(i, 1);
			cachedHeroInfo.SetSkillExtend(i, 0);
		}
		cachedHeroInfo.Validate();
		return cachedHeroInfo;
	}

	private void SetVictoryMotion()
	{
		SetNamedMotion("Victory");
	}

	private void SetAppearMotion()
	{
		if (String_Extension_Method.IsNullOrEmpty(_appearMotionKey))
		{
			SetVictoryMotion();
		}
		else
		{
			SetNamedMotion(_appearMotionKey);
		}
	}

	private void SetNamedMotion(string animationKey)
	{
		_motionStartedTime = DateTime.UtcNow;
		HeroComponent.SetNamedMotion(animationKey, isLoop: false);
	}

	private void PlayTweens(bool isForward)
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(TweenList); i++)
		{
			UITweener uITweener = TweenList[i];
			if (uITweener != null)
			{
				UITweener.Play(uITweener, isForward);
			}
		}
	}

	private void ResetToBegineTweens()
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(TweenList); i++)
		{
			UITweener uITweener = TweenList[i];
			if (uITweener != null)
			{
				UITweener.ResetToBeginning(uITweener);
			}
		}
	}

	public void OnClickNPC()
	{
		if (!((DateTime.UtcNow - _motionStartedTime).TotalSeconds < (double)NPCActionDelaySecond))
		{
			SetAppearMotion();
		}
	}

	public void OnDragStart()
	{
		if (HeroComponent != null)
		{
			HeroComponent.OnDragStart();
		}
	}

	public void OnDragEnd()
	{
		if (HeroComponent != null)
		{
			HeroComponent.OnDragEnd();
		}
	}

	public void OnClickYes()
	{
		if (_listener == null || CreatureData == null)
		{
			Close();
		}
		else
		{
			_listener.OnClickBuy(CreatureData);
		}
	}

	public void OnClickHeroBattle()
	{
		if (_listener != null && CreatureData != null)
		{
			_listener.OnTryBattle(_cachedHeroInfo);
		}
	}

	public void OnClickViewHeroDetail()
	{
		EnableCreatureWidget(enable: false);
		if (_listener != null && CreatureData != null)
		{
			_listener.OnClickViewHeroDetail(_cachedHeroInfo);
		}
	}

	public void OnClickLeft()
	{
		if (!((DateTime.UtcNow - _motionStartedTime).TotalSeconds < (double)NPCChangeDelaySecond))
		{
			Tuple<int, CreatureData> nextCreatureData = GetNextCreatureData(PageDirection.Next);
			if (_listener != null && nextCreatureData != null)
			{
				CurrentIndex = nextCreatureData.Item1;
				SetCreatureData(nextCreatureData.Item2);
				int index = nextCreatureData.Item2.Index;
				_listener.OnChangedHero(index);
			}
		}
	}

	public void OnClickRight()
	{
		if (!((DateTime.UtcNow - _motionStartedTime).TotalSeconds < (double)NPCChangeDelaySecond))
		{
			Tuple<int, CreatureData> nextCreatureData = GetNextCreatureData(PageDirection.Prev);
			if (_listener != null && nextCreatureData != null)
			{
				CurrentIndex = nextCreatureData.Item1;
				SetCreatureData(nextCreatureData.Item2);
				int index = nextCreatureData.Item2.Index;
				_listener.OnChangedHero(index);
			}
		}
	}

	public Tuple<int, CreatureData> GetNextCreatureData(PageDirection direction)
	{
		if (__Extension_Method_ICollection__.IsNullOrEmpty(_listHeroes))
		{
			return null;
		}
		int currentIndex = CurrentIndex;
		int num = __Extension_Method_ICollection__.COUNT(_listHeroes) - 1;
		currentIndex = ((direction != PageDirection.Next) ? ((currentIndex >= num) ? num : (++currentIndex)) : ((currentIndex > 0) ? (--currentIndex) : 0));
		if (CurrentIndex == currentIndex)
		{
			return null;
		}
		CreatureData i_item = (Vespa_Generic_IList_Extension_Method.IsValidIndex(_listHeroes, currentIndex) ? _listHeroes[currentIndex] : null);
		return new Tuple<int, CreatureData>(currentIndex, i_item);
	}

	private void InitCameraController()
	{
		string pathNameFromCode = PackHelper.GetPathNameFromCode(CreatureData.CodeName);
		string nodeTreeName = _nodeTreeName;
		if (_cinema == null)
		{
			_cinema = HeroRenderCinema.CreateInstance(new HeroRenderCinema.StockData
			{
				NodeTreePackCode = pathNameFromCode,
				NodeTreeName = nodeTreeName
			});
		}
		SpawnCreature();
		SetCameraController();
	}

	private void SpawnCreature()
	{
		RenderFacadeBuilder renderFacadeBuilder = new RenderFacadeBuilder(null, TeamType.Left, _cachedHeroInfo);
		renderFacadeBuilder.LoadLevel = CreatureStock.ELoadLevel.IncludeFacial;
		_heroUid = _cinema.SpawnCreature(VariableKeyword.Myself, _heroUid, renderFacadeBuilder);
		RenderFacade rENDERFACADE = _cinema.GETRENDERFACADE(_heroUid);
		if (!(rENDERFACADE != null))
		{
			return;
		}
		rENDERFACADE.AssetCreature.SetShaderTransform(new ShaderTransformData(GameSetting.data.Ui.Hero3dView.CameraAngle, depthShift: true), null);
		rENDERFACADE.Action.SetDirection(new VectorXZ(0f, -1f), blendMotion: false);
		ClientCreatureData data = GameRoot.Instance.ClientCreatureDataContainer.GetData(CreatureData.CodeName);
		if (data == null)
		{
			return;
		}
		Vector3 zero = Vector3.zero;
		float num = 1f;
		float x = GameUIRoot.instance.transform.localScale.x;
		if (__Extension_Method_ICollection__.COUNT(data.PayShopOffset) >= 2)
		{
			zero = new Vector3(data.PayShopOffset[0], data.PayShopOffset[1], 0f);
			if (zero.magnitude > 1f)
			{
				Vector3 position = GameUIRoot.instance.transform.position;
				position = zero * x;
				rENDERFACADE.SetRuntimeOffset("PayShop", position);
			}
		}
		if (data.PayShopScale > 1f)
		{
			num = data.PayShopScale * x;
			rENDERFACADE.SetRuntimeScale("PayShop", num);
		}
	}

	private void SetCameraController()
	{
		RenderFacade rENDERFACADE = _cinema.GETRENDERFACADE(_heroUid);
		_cameraController = new HeroCameraController(null, _heroUid, _cinema.CinemaCamera, rENDERFACADE);
		_cameraController.SetCameraInfo(CreatureData.Index, 0);
		_cameraController.ZoomOutRate = 0.5f;
	}

	private void CloseCamerController()
	{
		if (_cinema != null)
		{
			HeroRenderCinema.ReleaseInstance(_cinema);
			_cinema.DestroyCreature(_heroUid);
			_cinema = null;
		}
		if (_cameraController != null)
		{
			_cameraController = null;
		}
	}

	public void OnClickCinema()
	{
		if (!((DateTime.UtcNow - _motionStartedTime).TotalSeconds < (double)NPCActionDelaySecond))
		{
			VespaCoroutine.Start(coPlay(null));
		}
	}

	private IEnumerator coPlay(Action onEnd)
	{
		do
		{
			yield return null;
		}
		while (!_cinema.IsCompleteLoad());
		_motionStartedTime = DateTime.UtcNow;
		_cinema.StartNodeTree(delegate
		{
			Vespa_System_Action_Extension_Method.fire(onEnd);
		});
	}

	public void OnCinemaDragStart()
	{
		if (_cinema != null)
		{
			_cinema.OnDragStart();
		}
	}

	public void OnCinemaDragEnd()
	{
		if (_cinema != null)
		{
			_cinema.OnDragEnd();
		}
	}
}
