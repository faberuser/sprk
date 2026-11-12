using System;
using System.Collections;
using System.Collections.Generic;
using NGame2.N3D;
using NGame2.NAccount;
using NGame2.NMessage;
using NGame2.NMessage.NFastHandler;
using NGame2.NNodeTree;
using NGame2.NRender;
using NGame2.NRender.NRenderFacade;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NComponent2.NPayShop;
using NGame2.NUI.NRender3D;
using NGame2.NUI.NWindow.NBattle;
using NGame2.NUtil.NPayShop;
using NGameAsset;
using NNguiExtension;
using NShared;
using NShared.NLocalization;
using NVespa;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using NVespa.NUtil;
using UnityEngine;

namespace NGame2.NUI.NWindow.NPayShop;

public class PayShopEquipGachaScene : FrameControlWindow, IUserObserver<UserManager.UserStatInfo>, IUserObserver, PayShopEquipGachaHeroClassList.IListener, PayShopEquipGachaStepUpRewardList.IListener, PayShopPickupGachaHeroSelect.IListener, PayShopResourceList.IListener
{
	public interface IListener
	{
		void OnClickGacha(int gachaIndex);

		void OnClickGacha(int gachaIndex, HighGachaCategoryType type);

		void OnClickGacha(int gachaIndex, int[] heroIndices);

		void OnClickRuby();

		void OnClickGold();

		void OnClickProbability(int gachaIndex, HighGachaCategoryType gachaType);

		void OnClickHeroInfo(int heroIndex, List<CreatureData> listHeros);

		void RequestPetMileageReward(int gachaIndex, Action<int> onComplete);
	}

	public const string PrefabPath = "Resources/Prefab2/Window/PayShop/PayShopEquipGachaScene";

	public const string Command_ShowItemIcon = "EquipGachaScene/ShowItemIcon";

	public UITexture CinemaTexture;

	public UIWidget ClassRoot;

	public UILabel Label_Title;

	public UIWidget DescRoot;

	public UILabel Label_Desc;

	public UIButton Button_Probabilty;

	public OpenAnimationMasterController OpenAnimationMasterController;

	[Header("Sub List")]
	public PayShopEquipGachaItemList PayShopEquipGachaItemList;

	public PayShopEquipGachaHeroClassList PayShopEquipGachaHeroClassList;

	public PayShopEquipGachaStepUpRewardList PayShopEquipGachaStepUpRewardList;

	public PayShopEquipGachaStarterPickupProgress PayShopEquipGachaStarterPickupProgress;

	public PayShopPickupGachaHeroSelect PayShopPickupGachaHeroSelect;

	public PayShopResourceList PayShopResourceList;

	public PayShopPetGachaMileage PayShopPetGachaMileage;

	public PayShopAllInOneCeiling PayShopAllInOneCeiling;

	[Header("HighGacha HeroList")]
	public UIWidget HeroListRoot;

	public UIScrollGrid HeroIconGrid;

	[Header("StepUpGacha Button")]
	public GameObject ButtonChangeCreatureRoot;

	public GameObject Button_Right;

	public GameObject Button_Left;

	public GameObject ButtonHeroInfoRoot;

	[Header("Price")]
	public UIWidget PriceUIPosition;

	protected PayShopPriceComponent PriceComponent;

	public UILabel Label_Free;

	public UILabel Label_Ticket;

	public UIWidget MileageRoot;

	public UILabel Label_Mileage;

	public UILabel Label_RemineChance;

	public UILabel Label_NextFreeRemainTime;

	public UILabel Label_Count;

	public UIGrid FreeInfoGrid;

	[Header("ETC")]
	public float HeroXOffset;

	public float NPCChangeDelaySecond = 0.5f;

	private RenderTargetCinema _cinema;

	private RenderFacade _renderFacade;

	private int _curCreatureIndex;

	private int _curCreatureArrayIndex;

	private bool _isAnimatingItemGet;

	private bool _isPlayAnimation;

	private bool _isEnableGacha = true;

	private float _updateElapsedTime;

	private DateTime _motionStartedTime = DateTime.MinValue;

	private Holder _receiverHolder = new Holder();

	private int[] _selectedHeroIndices;

	private HighGachaCategoryType _highGachaCategoryType;

	private List<EquipGachaHeroIconComponent> _heroIconComponentList = new List<EquipGachaHeroIconComponent>();

	private IListener _listener;

	public NewEquipGachaData EquipGachaData { get; private set; }

	public NewPayShopProductInfo ProductInfo { get; private set; }

	public void Open(NewEquipGachaData gachaData, IListener listener)
	{
		Open();
		EquipGachaData = gachaData;
		ProductInfo = GlobalAutoBehaviour<AccountManager>.instance.NewPayShopProductManager.Get(gachaData.PurchaseRestrictProductIndex);
		_listener = listener;
		_receiverHolder.Add<CommandMessage>(new ObjVerifier(this), OnMessageFast);
		PayShopResourceList.Open(this);
		UIHelper.EnableGameObject(Button_Probabilty, gachaData.ShowProbability);
		UIHelper.SetLabelText(Label_Title, "");
		if (gachaData.Type == EquipGachaType.High)
		{
			UIHelper.EnableGameObject(PayShopEquipGachaHeroClassList, enable: true);
			UIHelper.EnableGameObject(HeroListRoot, enable: true);
			PayShopEquipGachaHeroClassList.Open(this);
			PayShopEquipGachaHeroClassList.Select(HighGachaCategoryType.NewHero);
			_highGachaCategoryType = HighGachaCategoryType.NewHero;
			CreateHeroList(HighGachaCategoryType.NewHero);
		}
		else if (gachaData.Type == EquipGachaType.StepUp)
		{
			UIHelper.EnableGameObject(PayShopEquipGachaStepUpRewardList, enable: true);
			StepUpEquipGachaInfo info = GlobalAutoBehaviour<AccountManager>.instance.StepUpEquipGachamanager.Get(gachaData.Index);
			PayShopEquipGachaStepUpRewardList.Setup(gachaData, info, this);
			bool enable = __Extension_Method_ICollection__.COUNT(gachaData.StepUpHeroIndices) > 1;
			UIHelper.EnableGameObject(ButtonChangeCreatureRoot, enable);
			UIHelper.EnableGameObject(ButtonHeroInfoRoot, enable: true);
			SetNextPrevButton(gachaData);
		}
		else if (gachaData.Type == EquipGachaType.Pickup)
		{
			UIHelper.EnableGameObject(PayShopPickupGachaHeroSelect, enable: true);
			PayShopPickupGachaHeroSelect.Open(gachaData, this);
			if (gachaData.StarterMileage > 0)
			{
				UIHelper.EnableGameObject(PayShopEquipGachaStarterPickupProgress, enable: true);
				PayShopEquipGachaStarterPickupProgress.Open(gachaData);
			}
		}
		else if (gachaData.Type == EquipGachaType.Pet)
		{
			if (__Extension_Method_ICollection__.COUNT(gachaData.RewardIndices) <= 0)
			{
				UIHelper.EnableGameObject(PayShopPetGachaMileage, enable: false);
			}
			else
			{
				UIHelper.EnableGameObject(PayShopPetGachaMileage, enable: true);
				int mileage = GlobalAutoBehaviour<AccountManager>.instance.PetGachaCountManager.GetMileage(gachaData.Index);
				PayShopPetGachaMileage.Setup(gachaData, mileage, null);
			}
		}
		else if (gachaData.Type == EquipGachaType.AllInOne)
		{
			if (__Extension_Method_ICollection__.COUNT(gachaData.RewardIndices) <= 0)
			{
				PayShopAllInOneCeiling.ShowUi(show: false);
			}
			else
			{
				PayShopAllInOneCeiling.ShowUi(show: true);
				int gachaCount = GlobalAutoBehaviour<AccountManager>.instance.EquipGachaCeilingManager.GetGachaCount(gachaData.Index);
				PayShopAllInOneCeiling.Setup(gachaData, gachaCount, null);
			}
		}
		OpenAnimationMasterController.Ready();
		UpdateData(gachaData);
		SetDesc(gachaData);
		VespaCoroutine.Start(this, coCreateBackground(gachaData, OnEndCreateBackground));
	}

	public override void UpdateDerived()
	{
		_updateElapsedTime += Time.deltaTime;
		if (_updateElapsedTime >= 0.5f)
		{
			UpdateData(EquipGachaData);
			_updateElapsedTime = 0f;
		}
	}

	private void UpdateData(NewEquipGachaData gachaData)
	{
		if (gachaData != null)
		{
			if (ProductInfo != null)
			{
				SetPeriodicalGacha(gachaData, ProductInfo);
				return;
			}
			SetPrice(gachaData);
			SetFreeInfo(gachaData);
		}
	}

	private IEnumerator coCreateBackground(NewEquipGachaData gachaData, System.Action callback)
	{
		string text = "EquipGachaBackGround_" + gachaData.GetGachaBackGround();
		_cinema = CreateCienma("Common/EquipGacha", text);
		CinemaTexture.mainTexture = _cinema.RenderTarget;
		CinemaTexture.alpha = 1f;
		while (!_cinema.IsCompleteLoad())
		{
			yield return null;
		}
		_cinema.StartNodeTree(callback);
	}

	private RenderTargetCinema CreateCienma(string packcode, string name)
	{
		ReleaseCinema();
		RenderTargetCinema.StockData stock = default(RenderTargetCinema.StockData);
		SingletonAuto<ResolutionInfo>.instance.GetScreenRenderTargetSize(out stock.RenderTargetWidth, out stock.RenderTargetHeight);
		stock.NodeTreePackCode = packcode;
		stock.NodeTreeName = name;
		return RenderTargetCinema.CreateInstance(stock);
	}

	private void ReleaseCinema()
	{
		if (_cinema != null)
		{
			RenderTargetCinema.ReleaseInstance(_cinema);
			_cinema = null;
		}
	}

	private void OnEndCreateBackground()
	{
		if (EquipGachaData != null && __Extension_Method_ICollection__.COUNT(EquipGachaData.StepUpHeroIndices) > 0)
		{
			PlayCreateHero(EquipGachaData);
		}
		else if (EquipGachaData == null || EquipGachaData.Type != EquipGachaType.Pickup)
		{
			PlayCreateAppearNodeTree();
		}
	}

	private void PlayCreateHero(NewEquipGachaData gachaData)
	{
		VespaCoroutine.Start(this, coCreateHero(EquipGachaData, OnFinishOpen));
	}

	private IEnumerator coCreateHero(NewEquipGachaData gachaData, System.Action callback)
	{
		CreateRenderFacadeHero(0);
		if (_renderFacade != null)
		{
			float time = 0f;
			float playTime = 0.5f;
			_renderFacade.ExtraAlphaController.SetAlpha(ExtraAlphaController.EAlphaType.NodeTree, 0f);
			while (time <= playTime)
			{
				yield return null;
				time += Time.deltaTime;
				float alpha = Mathf.Lerp(0f, 1f, time / playTime);
				_renderFacade.ExtraAlphaController.SetAlpha(ExtraAlphaController.EAlphaType.NodeTree, alpha);
			}
		}
		yield return new WaitForSeconds(0.5f);
		Vespa_System_Action_Extension_Method.fire(callback);
	}

	private void PlayCreateAppearNodeTree()
	{
		VespaCoroutine.Start(this, coCreateAppearNodeTree(EquipGachaData, OnEndCreateAppearNodeTree));
	}

	private PositionInfo GetPositionInfo()
	{
		PositionInfo positionInfo = new PositionInfo();
		if (__Extension_Method_ICollection__.COUNT(EquipGachaData.StepUpAccessoryOffset) == 3)
		{
			positionInfo.PositionX = EquipGachaData.StepUpAccessoryOffset[0];
			positionInfo.PositionY = EquipGachaData.StepUpAccessoryOffset[1];
			positionInfo.PositionZ = EquipGachaData.StepUpAccessoryOffset[2];
		}
		else
		{
			VespaDebug.LogError("StepUpAccessoryOffset must have 3 values (x, y, z). The offset will be replaced with default value (0, 0, 0).");
		}
		if (__Extension_Method_ICollection__.COUNT(EquipGachaData.StepUpAccessoryOffset) == 3)
		{
			positionInfo.RotationX = EquipGachaData.StepUpAccessoryRotation[0];
			positionInfo.RotationY = EquipGachaData.StepUpAccessoryRotation[1];
			positionInfo.RotationZ = EquipGachaData.StepUpAccessoryRotation[2];
		}
		else
		{
			VespaDebug.LogError("StepUpAccessoryRotation must have 3 values (x, y, z). The rotation will be replaced with default value (0, 0, 0).");
		}
		positionInfo.Scale = EquipGachaData.StepUpAccessoryScale;
		return positionInfo;
	}

	private void CreateRenderFacadeHero(int index)
	{
		if (_cinema == null)
		{
			return;
		}
		DestroyRenderFacadeHero();
		int stepUpHeroIndex = EquipGachaData.GetStepUpHeroIndex(index);
		if (stepUpHeroIndex == 0)
		{
			return;
		}
		_curCreatureArrayIndex = index;
		_curCreatureIndex = stepUpHeroIndex;
		_motionStartedTime = DateTime.UtcNow;
		int stepUpHeroCostumeIndex = EquipGachaData.GetStepUpHeroCostumeIndex(index);
		int stepUpAccessoryIndex = EquipGachaData.StepUpAccessoryIndex;
		CreatureData data = GameRoot.Instance.CreatureDataContainer.GetData(stepUpHeroIndex);
		RenderFacadeBuilder renderFacadeBuilder = new RenderFacadeBuilder(null, TeamType.Left, new CreatureStock
		{
			Code = (data.ModelCode ?? data.CodeName),
			Star = data.StartHeroStar
		});
		renderFacadeBuilder.LoadLevel = CreatureStock.ELoadLevel.IncludeFacial;
		if (stepUpHeroCostumeIndex != 0)
		{
			renderFacadeBuilder.SetCostume(stepUpHeroCostumeIndex);
		}
		if (stepUpAccessoryIndex != 0)
		{
			PositionInfo positionInfo = GetPositionInfo();
			renderFacadeBuilder.SetAccessory(stepUpAccessoryIndex, stepUpHeroIndex, positionInfo);
		}
		_cinema.SpawnCreature(VariableKeyword.Myself, 1, renderFacadeBuilder);
		_renderFacade = _cinema.GETRENDERFACADE(1);
		if (!(_renderFacade != null))
		{
			return;
		}
		_renderFacade.Action.SetDirection(new VectorXZ(0f, 1f), blendMotion: false);
		ClientCreatureData data2 = GameRoot.Instance.ClientCreatureDataContainer.GetData(data.CodeName, stepUpHeroCostumeIndex);
		if (data2 != null)
		{
			if (data2.StepUpScale != 0f)
			{
				_renderFacade.transform.localScale = Vector3.one * data2.StepUpScale;
			}
			Vector3 localPosition = Vector3.left * HeroXOffset;
			if (__Extension_Method_ICollection__.COUNT(data2.StepUpGachaOffset) >= 2)
			{
				Vector3 vector = new Vector3(data2.StepUpGachaOffset[0], data2.StepUpGachaOffset[1], 0f);
				localPosition += vector;
			}
			_renderFacade.transform.localPosition = localPosition;
		}
	}

	private void DestroyRenderFacadeHero()
	{
		if (!(_cinema == null) && _renderFacade != null)
		{
			_cinema.DestroyCreature(1);
			_renderFacade = null;
		}
	}

	private IEnumerator coCreateAppearNodeTree(NewEquipGachaData gachaData, System.Action callback)
	{
		_cinema.LoadNodeTree("Common/EquipGacha", gachaData.AppearNodeTree);
		_cinema.AddKeyword(VariableKeyword.WeaponPrefix, gachaData.GetLuaWeaponPrefix());
		while (!_cinema.IsCompleteLoad())
		{
			yield return null;
		}
		_cinema.StartNodeTree(callback);
	}

	private void OnEndCreateAppearNodeTree()
	{
		OnFinishOpen();
	}

	private void OnFinishOpen()
	{
		SetTitle(EquipGachaData);
		OpenAnimationMasterController.Play();
		CheckPetMileageReward(EquipGachaData);
	}

	private void SetTitle(NewEquipGachaData data)
	{
		string gachaCategorySelectTitle = data.GachaCategorySelectTitle;
		if (String_Extension_Method.IsNullOrEmpty(gachaCategorySelectTitle))
		{
			gachaCategorySelectTitle = data.Name;
		}
		string stringDirect = LocalizationManager.GetStringDirect(gachaCategorySelectTitle);
		UIHelper.SetLabelText(Label_Title, stringDirect);
	}

	public override void Reset()
	{
		base.Reset();
		if (CinemaTexture != null)
		{
			CinemaTexture.mainTexture = null;
		}
		DestroyRenderFacadeHero();
		ReleaseCinema();
		_receiverHolder.Clear();
		_highGachaCategoryType = HighGachaCategoryType.None;
		_selectedHeroIndices = null;
		_curCreatureArrayIndex = 0;
		_curCreatureIndex = 0;
		_motionStartedTime = DateTime.MinValue;
		PayShopEquipGachaItemList.Clear();
		PayShopEquipGachaHeroClassList.Clear();
		PayShopEquipGachaStepUpRewardList.Close();
		PayShopEquipGachaStarterPickupProgress.Clear();
		PayShopPickupGachaHeroSelect.Clear();
		PayShopResourceList.ClearAll();
		PayShopPetGachaMileage.Close();
		UIHelper.EnableGameObject(PayShopEquipGachaHeroClassList, enable: false);
		UIHelper.EnableGameObject(HeroListRoot, enable: false);
		UIHelper.EnableGameObject(PayShopEquipGachaStepUpRewardList, enable: false);
		UIHelper.EnableGameObject(PayShopEquipGachaStarterPickupProgress, enable: false);
		UIHelper.EnableGameObject(PayShopPickupGachaHeroSelect, enable: false);
		UIHelper.EnableGameObject(ButtonChangeCreatureRoot, enable: false);
		UIHelper.EnableGameObject(ButtonHeroInfoRoot, enable: false);
		UIHelper.EnableGameObject(PayShopPetGachaMileage, enable: false);
		UIHelper.EnableGameObject(PayShopAllInOneCeiling, enable: false);
		_isPlayAnimation = false;
		_isAnimatingItemGet = false;
		_isEnableGacha = true;
		FactoryBase.ReleaseAndClearList(_heroIconComponentList);
	}

	private void ReleaseHeroComponentList()
	{
		FactoryBase.ReleaseAndClearList(_heroIconComponentList);
	}

	public void OnClickGacha()
	{
		if (_isPlayAnimation || !_isEnableGacha)
		{
			return;
		}
		if (_listener != null)
		{
			if (_highGachaCategoryType != HighGachaCategoryType.None)
			{
				_listener.OnClickGacha(EquipGachaData.Index, _highGachaCategoryType);
			}
			else if (!__Extension_Method_ICollection__.IsNullOrEmpty(_selectedHeroIndices))
			{
				_listener.OnClickGacha(EquipGachaData.Index, _selectedHeroIndices);
			}
			else
			{
				_listener.OnClickGacha(EquipGachaData.Index);
			}
		}
		_isEnableGacha = false;
		SingletonUpdater<Executer>.instance.DelayRun(1f, this, delegate
		{
			_isEnableGacha = true;
		});
	}

	private void SetPeriodicalGacha(NewEquipGachaData data, NewPayShopProductInfo productInfo)
	{
		UIHelper.EnableGameObject(Label_Free, enable: false);
		UIHelper.EnableGameObject(Label_Ticket, enable: false);
		UIHelper.EnableGameObject(MileageRoot, enable: false);
		UIHelper.EnableGameObject(Label_NextFreeRemainTime, enable: false);
		string resetDesc = PayShopUIHelper.GetResetDesc(productInfo);
		UIHelper.EnableGameObject(Label_RemineChance, resetDesc != null);
		UIHelper.SetLabelText(Label_RemineChance, resetDesc);
		PlayerProductPurchaseInfo playerProductPurchaseInfo = GlobalAutoBehaviour<AccountManager>.instance.PlayerProductPurchaseManager.Get(productInfo.Index);
		int purchasableCount = productInfo.PurchasableCount;
		int num = purchasableCount - (playerProductPurchaseInfo?.PurchasedCount ?? 0);
		UIHelper.EnableGameObject(Label_Count, enable: true);
		UIHelper.SetLabelText(Label_Count, $"{num}/{purchasableCount}");
		ReleasePriceComponent();
		int freeGachaItemCount = HelperItem.GetFreeGachaItemCount(data.Index);
		bool num2 = freeGachaItemCount > 0;
		bool flag = num > 0;
		if (num2)
		{
			UIHelper.EnableGameObject(Label_Free, enable: false);
			UIHelper.EnableGameObject(Label_Ticket, enable: true);
			string text = LocalizationManager.GetString(Gacha.FREE_GACHA_ITEM_COUNT, freeGachaItemCount);
			UIHelper.SetLabelText(Label_Ticket, text);
		}
		else
		{
			UIHelper.EnableGameObject(Label_Free, enable: false);
			UIHelper.EnableGameObject(Label_Ticket, enable: false);
			if (PriceComponent == null)
			{
				PriceComponent = PayShopPriceComponent.PrefabAuto.Factory.Create(PriceUIPosition.transform);
				Vespa_Unity_Transform_Extension_Method.ResetLocalTM(PriceComponent.transform);
				UIDepthHelper.RearrangeWidgetDepth(PriceComponent.gameObject, PriceUIPosition.depth);
			}
			if (!flag)
			{
				PriceComponent.NotBuyableOpen();
			}
			else
			{
				bool num3 = Checker.IsDiscount(data);
				bool num4 = data.OpenGem > 0;
				CurrencyType currency = ((!num4) ? CurrencyType.Gold : CurrencyType.Gem);
				int price = data.OpenGold;
				if (num4)
				{
					price = Calculator.GetGemApplyValidDiscounts(data);
					price = Calculator.GetStepUpDiscount(data, price);
				}
				if (num3)
				{
					PriceComponent.SetGamePrice(currency, price, data.OpenGem);
				}
				else
				{
					PriceComponent.SetGamePrice(currency, price);
				}
			}
		}
		UIHelper.ResetGridPosition(FreeInfoGrid);
	}

	private void SetPrice(NewEquipGachaData data)
	{
		bool flag = Checker.IsDiscount(data);
		bool num = GlobalAutoBehaviour<AccountManager>.instance.FreeEquipGachaManager.Free(data.Index);
		int freeGachaItemCount = HelperItem.GetFreeGachaItemCount(data.Index);
		UIHelper.EnableGameObject(Label_Count, enable: false);
		UIHelper.EnableGameObject(MileageRoot, enable: false);
		ReleasePriceComponent();
		if (num)
		{
			UIHelper.EnableGameObject(Label_Free, enable: true);
			UIHelper.EnableGameObject(Label_Ticket, enable: false);
		}
		else if (freeGachaItemCount > 0 || data.TicketOnly)
		{
			UIHelper.EnableGameObject(Label_Free, enable: false);
			UIHelper.EnableGameObject(Label_Ticket, enable: true);
			string text = LocalizationManager.GetString(Gacha.FREE_GACHA_ITEM_COUNT, freeGachaItemCount);
			UIHelper.SetLabelText(Label_Ticket, text);
		}
		else if (data.OpenGem > 0 || data.OpenGold > 0)
		{
			if (PriceComponent == null)
			{
				PriceComponent = PayShopPriceComponent.PrefabAuto.Factory.Create(PriceUIPosition.transform);
				Vespa_Unity_Transform_Extension_Method.ResetLocalTM(PriceComponent.transform);
				UIDepthHelper.RearrangeWidgetDepth(PriceComponent.gameObject, PriceUIPosition.depth);
			}
			UIHelper.EnableGameObject(Label_Free, enable: false);
			UIHelper.EnableGameObject(Label_Ticket, enable: false);
			bool num2 = data.OpenGem > 0;
			CurrencyType currency = ((!num2) ? CurrencyType.Gold : CurrencyType.Gem);
			int price;
			if (!num2)
			{
				price = data.OpenGold;
			}
			else
			{
				price = Calculator.GetGemApplyValidDiscounts(data);
				price = Calculator.GetStepUpDiscount(data, price);
			}
			if (flag)
			{
				PriceComponent.SetGamePrice(currency, price, data.OpenGem);
			}
			else
			{
				PriceComponent.SetGamePrice(currency, price);
			}
			if (num2)
			{
				UIHelper.EnableGameObject(MileageRoot, enable: true);
				int mileageApplyValidMileage = Calculator.GetMileageApplyValidMileage(data);
				long mileage = mileageApplyValidMileage;
				mileage = Calculator.GetStepUpMileage(data, mileage);
				UIHelper.SetLabelText(Label_Mileage, "+ " + Vespa_Int_Extension_Method.ToCommaString(mileage));
			}
		}
		else
		{
			UIHelper.EnableGameObject(Label_Free, enable: true);
			UIHelper.EnableGameObject(Label_Ticket, enable: false);
		}
	}

	private void SetFreeInfo(NewEquipGachaData data)
	{
		if (data.DailyMaxFree > 0)
		{
			int remainGachaChance = GlobalAutoBehaviour<AccountManager>.instance.FreeEquipGachaManager.GetRemainGachaChance(EquipGachaData.Index);
			string text = LocalizationManager.GetString(Gacha.REMAINDAY_GACHA_CHANCE, remainGachaChance, EquipGachaData.DailyMaxFree);
			UIHelper.EnableGameObject(Label_RemineChance, enable: true);
			UIHelper.SetLabelText(Label_RemineChance, text);
			bool flag = false;
			bool flag2 = GlobalAutoBehaviour<AccountManager>.instance.FreeEquipGachaManager.Free(EquipGachaData.Index);
			if (data.DailyMaxFree > 1)
			{
				flag = !flag2 && remainGachaChance > 0;
			}
			else if (data.DailyMaxFree == 1)
			{
				flag = !flag2;
			}
			UIHelper.EnableGameObject(Label_NextFreeRemainTime, flag);
			if (flag)
			{
				TimeSpan? remainFreeGachaCoolTimeSpan = GlobalAutoBehaviour<AccountManager>.instance.FreeEquipGachaManager.GetRemainFreeGachaCoolTimeSpan(EquipGachaData.Index);
				if (!remainFreeGachaCoolTimeSpan.HasValue)
				{
					UIHelper.EnableGameObject(Label_NextFreeRemainTime, enable: false);
				}
				else
				{
					string empty = string.Empty;
					UIHelper.SetLabelText(text: (remainFreeGachaCoolTimeSpan.Value.Hours <= 0) ? LocalizationManager.GetString(Gacha.REMAINDAY_GACHA_COOLTIME, remainFreeGachaCoolTimeSpan.Value.Minutes, remainFreeGachaCoolTimeSpan.Value.Seconds) : LocalizationManager.GetString(Gacha.REMAINDAY_GACHA_COOLTIME_HOUR, remainFreeGachaCoolTimeSpan.Value.Hours, remainFreeGachaCoolTimeSpan.Value.Minutes), label: Label_NextFreeRemainTime);
				}
			}
		}
		else
		{
			UIHelper.EnableGameObject(Label_NextFreeRemainTime, enable: false);
			UIHelper.EnableGameObject(Label_RemineChance, enable: false);
		}
		UIHelper.ResetGridPosition(FreeInfoGrid);
	}

	private void ReleasePriceComponent()
	{
		FactoryBase.SafeReleaseComponent(ref PriceComponent);
	}

	public void PlayGachaAnimation(GachaItemResultInfo[] gachaItemResults, RewardResultInfo cachedStepUpRewardItemResult, RewardResultInfo cachedStarterRewardItemResult, RewardResultInfo cachedPetRewardItemResult, RewardResultInfo cachedAllInOneRewardResult)
	{
		VespaCoroutine.Start(coPlayGachaAnimation(EquipGachaData, gachaItemResults, cachedStepUpRewardItemResult, cachedStarterRewardItemResult, cachedPetRewardItemResult, cachedAllInOneRewardResult));
	}

	private IEnumerator coPlayGachaAnimation(NewEquipGachaData gachaData, GachaItemResultInfo[] gachaItemResults, RewardResultInfo cachedStepUpRewardItemResult, RewardResultInfo cachedStarterRewardItemResult, RewardResultInfo cachedPetRewardItemResult, RewardResultInfo cachedAllInOneRewardResult)
	{
		_isPlayAnimation = true;
		PayShopEquipGachaItemList.Clear();
		OpenAnimationMasterController.Play(isForward: false);
		if (_renderFacade != null)
		{
			DestroyRenderFacadeHero();
			bool isCreateLua = false;
			yield return VespaCoroutine.Start(coCreateAppearNodeTree(gachaData, delegate
			{
				isCreateLua = true;
			}));
			while (!isCreateLua)
			{
				yield return null;
			}
		}
		_cinema.LoadNodeTree("Common/EquipGacha", gachaData.ReactionNodeTree);
		yield return null;
		while (!_cinema.IsCompleteLoad())
		{
			yield return null;
		}
		PayShopEquipGachaItemList.SetItem(gachaItemResults);
		_isAnimatingItemGet = false;
		bool endNodeTree = false;
		_cinema.StartNodeTree(delegate
		{
			endNodeTree = true;
		});
		while (!endNodeTree)
		{
			yield return null;
		}
		while (!_isAnimatingItemGet)
		{
			yield return null;
		}
		yield return new WaitForSeconds(0.3f);
		if (cachedStepUpRewardItemResult != null)
		{
			string stringDirect = LocalizationManager.GetStringDirect("STEPUP_GACHA_BONUS_REWARD_TEXT");
			yield return VespaCoroutine.Start(coRewardPopup(cachedStepUpRewardItemResult, stringDirect));
		}
		if (cachedStarterRewardItemResult != null)
		{
			string stringDirect2 = LocalizationManager.GetStringDirect("STARTER_PICKUP_ALLCLEAR_REWARD_TEXT");
			yield return VespaCoroutine.Start(coRewardPopup(cachedStarterRewardItemResult, stringDirect2));
		}
		if (cachedPetRewardItemResult != null)
		{
			string stringDirect3 = LocalizationManager.GetStringDirect("PET_MILEAGE_REWARD_TITLE");
			yield return VespaCoroutine.Start(coRewardPopup(cachedPetRewardItemResult, stringDirect3));
		}
		if (cachedAllInOneRewardResult != null)
		{
			string stringDirect4 = LocalizationManager.GetStringDirect("ALLINONE_GACHA_BONUS_REWARD_TEXT");
			yield return VespaCoroutine.Start(coRewardPopup(cachedAllInOneRewardResult, stringDirect4));
		}
		OpenAnimationMasterController.Play();
		if (PayShopEquipGachaItemList.IsOpenItem)
		{
			SetContinueGachaUI();
		}
		yield return new WaitForSeconds(0.3f);
		if (EquipGachaData.Type == EquipGachaType.StepUp)
		{
			StepUpEquipGachaInfo newInfo = GlobalAutoBehaviour<AccountManager>.instance.StepUpEquipGachamanager.Get(EquipGachaData.Index);
			if (PayShopEquipGachaStepUpRewardList.IsPlayAnimation(EquipGachaData, newInfo))
			{
				yield return StartCoroutine(PayShopEquipGachaStepUpRewardList.CoPlayAnimation(EquipGachaData, newInfo));
			}
		}
		if (EquipGachaData.StarterMileage > 0)
		{
			bool isEndAnimation = false;
			PayShopEquipGachaStarterPickupProgress.IncreaseAnimation(delegate
			{
				isEndAnimation = true;
			});
			while (!isEndAnimation)
			{
				yield return null;
			}
		}
		if (EquipGachaData.Type == EquipGachaType.Pet)
		{
			int mileage = GlobalAutoBehaviour<AccountManager>.instance.PetGachaCountManager.GetMileage(EquipGachaData.Index);
			PayShopPetGachaMileage.RefreshMileageProgress(mileage, EquipGachaData.MileageAmountForReward);
		}
		if (EquipGachaData.Index == 11)
		{
			int currentCeilingCount = GlobalAutoBehaviour<AccountManager>.instance.EquipGachaCeilingManager.GetGachaCount(EquipGachaData.Index);
			int maxCeilingCount = EquipGachaData.MileageAmountForReward * EquipGachaData.RewardIndices.Length;
			if (GlobalAutoBehaviour<AccountManager>.instance.EquipGachaCeilingManager.GetGachaCount(EquipGachaData.Index) == 0 && cachedAllInOneRewardResult != null)
			{
				PayShopAllInOneCeiling.RefreshRewardProgress(maxCeilingCount, maxCeilingCount);
				yield return new WaitForSeconds(1.3f);
				yield return PayShopAllInOneCeiling.ResetRewardItemComponents();
			}
			PayShopAllInOneCeiling.RefreshRewardProgress(currentCeilingCount, maxCeilingCount);
		}
		UpdateData(gachaData);
		_isPlayAnimation = false;
	}

	private void OnMessageFast(CommandMessage message)
	{
		if (message.IsValidMessage() && message.IsValidScene(_cinema) && message.Command == "EquipGachaScene/ShowItemIcon")
		{
			VespaCoroutine.Start(this, coGetItem());
		}
	}

	private IEnumerator coGetItem()
	{
		PayShopEquipGachaItemList.Play();
		while (PayShopEquipGachaItemList.IsAnimatingItemGet)
		{
			yield return null;
		}
		_isAnimatingItemGet = true;
	}

	private IEnumerator coRewardPopup(RewardResultInfo info, string title)
	{
		BaseRewardPopup.PopupOpenData popupOpenData = new BaseRewardPopup.PopupOpenData().SetTitle(title);
		popupOpenData.AddItemResultInfos(info.ItemResults).AddEquipResultInfos(info.EquipItemResults).AddCurrencyResultInfos3(info.CurrencyResults)
			.AddStaminaResultInfos(info.StaminaResultInfos);
		bool closeRewardPopup = false;
		BaseRewardPopup.OpenPopup(popupOpenData, isGlobal: false, delegate
		{
			closeRewardPopup = true;
		});
		while (!closeRewardPopup)
		{
			yield return null;
		}
	}

	public override void OnSystemBackButton()
	{
		OnClickClose();
	}

	public void OnClickClose()
	{
		if (!_isPlayAnimation)
		{
			Close();
		}
	}

	void PayShopEquipGachaHeroClassList.IListener.OnClickTab(HighGachaCategoryType type)
	{
		_highGachaCategoryType = type;
		CreateHeroList(type);
		SetDesc(EquipGachaData);
		PayShopEquipGachaItemList.Clear();
	}

	private void CreateHeroList(HighGachaCategoryType type)
	{
		EquipGachaClientData data = GameRoot.Instance.EquipGachaClientDataContainer.GetData(EquipGachaData.GachaCategoryGroupIndex, type);
		if (data != null)
		{
			CreateHeroList(data.UniqueWeaponHeroIndex);
		}
	}

	private void CreateHeroList(int[] heroIndices)
	{
		ReleaseHeroComponentList();
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(heroIndices); i++)
		{
			CreatureData data = GameRoot.Instance.CreatureDataContainer.GetData(heroIndices[i]);
			if (data != null)
			{
				EquipGachaHeroIconComponent equipGachaHeroIconComponent = UIHelper.AddGridItem(HeroIconGrid, EquipGachaHeroIconComponent.PrefabAuto);
				if (!(equipGachaHeroIconComponent == null))
				{
					equipGachaHeroIconComponent.Open(data);
					_heroIconComponentList.Add(equipGachaHeroIconComponent);
				}
			}
		}
		UIHelper.RepositionGrid(HeroIconGrid);
	}

	void PayShopEquipGachaStepUpRewardList.IListener.OnClickRewardItem(StepUpGachaRewardData data)
	{
		PayShopStepUpGachaRewardPopup.Open(data);
	}

	private void SetContinueGachaUI()
	{
		UIHelper.EnableGameObject(Button_Right, enable: false);
		UIHelper.EnableGameObject(Button_Left, enable: false);
		UIHelper.EnableGameObject(ButtonHeroInfoRoot, enable: false);
	}

	public void OnClickLeft()
	{
		if (!((DateTime.UtcNow - _motionStartedTime).TotalSeconds < (double)NPCChangeDelaySecond))
		{
			int num = _curCreatureArrayIndex - 1;
			if (num >= 0)
			{
				CreateRenderFacadeHero(num);
				SetNextPrevButton(EquipGachaData);
			}
		}
	}

	public void OnClickRight()
	{
		if (!((DateTime.UtcNow - _motionStartedTime).TotalSeconds < (double)NPCChangeDelaySecond))
		{
			int[] stepUpHeroIndices = EquipGachaData.StepUpHeroIndices;
			int num = _curCreatureArrayIndex + 1;
			int num2 = ((__Extension_Method_ICollection__.COUNT(stepUpHeroIndices) > 1) ? (__Extension_Method_ICollection__.COUNT(stepUpHeroIndices) - 1) : 0);
			if (num <= num2)
			{
				CreateRenderFacadeHero(num);
				SetNextPrevButton(EquipGachaData);
			}
		}
	}

	public void OnClickHeroInfo()
	{
		GameRoot.Instance.CreatureDataContainer.GetData(_curCreatureIndex);
		List<CreatureData> list = new List<CreatureData>();
		for (int i = 0; i < EquipGachaData.StepUpHeroIndices.Length; i++)
		{
			int key = EquipGachaData.StepUpHeroIndices[i];
			CreatureData data = GameRoot.Instance.CreatureDataContainer.GetData(key);
			list.Add(data);
		}
		if (_listener != null)
		{
			_listener.OnClickHeroInfo(_curCreatureIndex, list);
		}
	}

	public void SelectHero(int heroIndex)
	{
		if (EquipGachaData == null || EquipGachaData.Type != EquipGachaType.StepUp)
		{
			return;
		}
		int[] stepUpHeroIndices = EquipGachaData.StepUpHeroIndices;
		if (__Extension_Method_ICollection__.COUNT(stepUpHeroIndices) > 1)
		{
			int num = Array.IndexOf(stepUpHeroIndices, heroIndex);
			if (num >= 0)
			{
				CreateRenderFacadeHero(num);
				SetNextPrevButton(EquipGachaData);
			}
		}
	}

	private void SetDesc(NewEquipGachaData gachaData)
	{
		UIHelper.EnableGameObject(DescRoot, enable: true);
		if (gachaData.Type == EquipGachaType.High)
		{
			SetDesc_High(gachaData, _highGachaCategoryType);
			return;
		}
		string text = "";
		if (!String_Extension_Method.IsNullOrEmpty(gachaData.GachaSceneDesc))
		{
			text = gachaData.GachaSceneDesc;
		}
		else if (gachaData.Type == EquipGachaType.Pickup)
		{
			text = "PICKUP_GACHA_DESC_DETAIL";
		}
		if (!String_Extension_Method.IsNullOrEmpty(text))
		{
			string stringDirect = LocalizationManager.GetStringDirect(text);
			UIHelper.SetLabelText(Label_Desc, stringDirect);
		}
		else
		{
			UIHelper.EnableGameObject(DescRoot, enable: false);
		}
	}

	private void SetDesc_High(NewEquipGachaData gachaData, HighGachaCategoryType category)
	{
		if (category == HighGachaCategoryType.NewHero)
		{
			string newHeroNames = GetNewHeroNames(gachaData);
			int num = 1;
			if (gachaData != null)
			{
				num = gachaData.Bid;
			}
			string key = ((num != 10) ? "HIGH_ITEM_GACHA_DESC_NEWHERO_1_DETAIL" : "HIGH_ITEM_GACHA_DESC_NEWHERO_10_DETAIL");
			string stringDirect = LocalizationManager.GetStringDirect(key, newHeroNames);
			UIHelper.SetLabelText(Label_Desc, stringDirect);
			return;
		}
		string key2 = "";
		switch (category)
		{
		case HighGachaCategoryType.Knight:
			key2 = "HIGH_EQUIP_GACHA_DESC_KNIGHT";
			break;
		case HighGachaCategoryType.Warrior:
			key2 = "HIGH_EQUIP_GACHA_DESC_WARRIOR";
			break;
		case HighGachaCategoryType.Assassin:
			key2 = "HIGH_EQUIP_GACHA_DESC_ASSASSIN";
			break;
		case HighGachaCategoryType.Archer:
			key2 = "HIGH_EQUIP_GACHA_DESC_ARCHER";
			break;
		case HighGachaCategoryType.Mechanic:
			key2 = "HIGH_EQUIP_GACHA_DESC_MECHANIC";
			break;
		case HighGachaCategoryType.Wizard:
			key2 = "HIGH_EQUIP_GACHA_DESC_WIZARD";
			break;
		case HighGachaCategoryType.Priest:
			key2 = "HIGH_EQUIP_GACHA_DESC_PRIEST";
			break;
		}
		string stringDirect2 = LocalizationManager.GetStringDirect(key2);
		UIHelper.SetLabelText(Label_Desc, stringDirect2);
	}

	private string GetNewHeroNames(NewEquipGachaData equipGachaData)
	{
		EquipGachaClientData data = GameRoot.Instance.EquipGachaClientDataContainer.GetData(equipGachaData.GachaCategoryGroupIndex, HighGachaCategoryType.NewHero);
		if (data == null)
		{
			return "";
		}
		string text = string.Empty;
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(data.UniqueWeaponHeroIndex); i++)
		{
			if (i > 0)
			{
				text += ", ";
			}
			int key = data.UniqueWeaponHeroIndex[i];
			CreatureData data2 = GameRoot.Instance.CreatureDataContainer.GetData(key);
			if (data2 != null)
			{
				text += data2.Name;
			}
		}
		return Vespa_String_Extension_Method.NguiColor(text, Color.yellow);
	}

	private void SetNextPrevButton(NewEquipGachaData gachaData)
	{
		int[] stepUpHeroIndices = gachaData.StepUpHeroIndices;
		int num = __Extension_Method_ICollection__.COUNT(stepUpHeroIndices) - 1;
		bool flag = __Extension_Method_ICollection__.COUNT(stepUpHeroIndices) > 1;
		UIHelper.EnableGameObject(Button_Right, flag && _curCreatureArrayIndex != num);
		UIHelper.EnableGameObject(Button_Left, flag && _curCreatureArrayIndex != 0);
	}

	private void CheckPetMileageReward(NewEquipGachaData gachaData)
	{
		if (EquipGachaData.Type != EquipGachaType.Pet)
		{
			return;
		}
		if (__Extension_Method_ICollection__.COUNT(gachaData.RewardIndices) <= 0)
		{
			UIHelper.EnableGameObject(PayShopPetGachaMileage, enable: false);
		}
		else if (GlobalAutoBehaviour<AccountManager>.instance.PetGachaCountManager.GetMileage(gachaData.Index) >= gachaData.MileageAmountForReward && _listener != null)
		{
			_listener.RequestPetMileageReward(gachaData.Index, delegate(int changedMileage)
			{
				PayShopPetGachaMileage.Setup(gachaData, changedMileage, null);
			});
		}
	}

	void PayShopPickupGachaHeroSelect.IListener.OnFinishPickUp(int[] heroIndices)
	{
		UIHelper.EnableGameObject(PayShopPickupGachaHeroSelect, enable: false);
		_selectedHeroIndices = heroIndices;
		UIHelper.EnableGameObject(HeroListRoot, enable: true);
		CreateHeroList(heroIndices);
		PlayCreateAppearNodeTree();
	}

	void PayShopResourceList.IListener.OnClickStamina()
	{
	}

	void PayShopResourceList.IListener.OnClickRuby()
	{
		Close();
		if (_listener != null)
		{
			_listener.OnClickRuby();
		}
	}

	void PayShopResourceList.IListener.OnClickGold()
	{
		Close();
		if (_listener != null)
		{
			_listener.OnClickGold();
		}
	}

	void IUserObserver<UserManager.UserStatInfo>.OnChanged(UserManager.UserStatInfo changedInfo)
	{
		PayShopResourceList.OnChanged(changedInfo);
	}

	bool IUserObserver.IsGlobal()
	{
		return false;
	}

	public void OnClickProbability()
	{
		if (_listener != null)
		{
			_listener.OnClickProbability(EquipGachaData.Index, _highGachaCategoryType);
		}
	}
}
