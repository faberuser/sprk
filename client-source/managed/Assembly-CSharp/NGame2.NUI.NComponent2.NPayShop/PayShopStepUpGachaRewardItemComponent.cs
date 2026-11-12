using System;
using System.Collections.Generic;
using NNguiExtension;
using NShared;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NComponent2.NPayShop;

public class PayShopStepUpGachaRewardItemComponent : LinkFactoryComponent, InitOnCreate, ItemIconComponent.Listener
{
	public interface IListener
	{
		void OnClickRewardItem(StepUpGachaRewardData data);
	}

	public enum ECompState
	{
		Get,
		Current,
		NotYet
	}

	public enum ESubRewardType
	{
		Emtpy,
		RewardItem1,
		RewardItem2,
		Buff
	}

	[Serializable]
	public class ArrowComponent
	{
		public GameObject GoAllow;

		public UISpriteSelector SpriteSelector;

		public PayShopEquipGachaStepUpRewardList.EAllowDirection Direction;
	}

	public const string PrefabPath = "Resources/Prefab2/Component/PayShop2/PayShopStepUpGachaRewardItemComponent";

	public static PrefabAuto<PayShopStepUpGachaRewardItemComponent> PrefabAuto = new PrefabAuto<PayShopStepUpGachaRewardItemComponent>("Resources/Prefab2/Component/PayShop2/PayShopStepUpGachaRewardItemComponent");

	public UISprite SpriteGet;

	public UISpriteSelector SpriteSelectorBg;

	public UISpriteSelector SpriteSelectorActive;

	public UISpriteSelector SpriteSelectorSale;

	public Transform MainIconRoot;

	public GameObject SubRoot;

	public GameObject SubSpriteActive;

	public GameObject SubSpriteGet;

	public Transform SubIconRoot;

	public ArrowComponent[] ArrowSprites;

	public UIGame3DWidget UI3DWidgetReset;

	public UIGame3DWidget UI3DWidgetGet;

	public UIGame3DWidget UI3DWidgetEmptyActive;

	public UIGame3DWidget UI3DWidgetParticle1;

	public UIGame3DWidget UI3DWidgetParticle2;

	public UIGame3DWidget UI3DWidgetParticle3;

	private List<Component> _iconComponent = new List<Component>();

	private IListener _listener;

	private StepUpGachaRewardData _cachedData;

	private bool _cachedAlreadyGet;

	void InitOnCreate.OnCreate()
	{
	}

	void InitOnCreate.OnRelease()
	{
		ReleaseItemIcon();
		_cachedData = null;
		_listener = null;
	}

	public void PlayAnimation(StepUpGachaRewardData data)
	{
		if (_cachedData.Round != data.Round)
		{
			_cachedData = data;
		}
		SetGet(isEnableObj: false);
		UI3DWidgetPlay(UI3DWidgetReset, isPlay: true);
	}

	public void Open(StepUpGachaRewardData data, PayShopEquipGachaStepUpRewardList.StepUpGachaRewardSlotData slotData, StepUpEquipGachaInfo info, IListener listener)
	{
		ECompState state = GetState(data, info);
		StepUpGachaRewardData nexData = GameRoot.Instance.StepUpGachaRewardDataContainer.GetNexData(data);
		SetMainReward(data, nexData, slotData, state);
		SetSubReward(data, state);
		SetEffect(data, state);
		_cachedAlreadyGet = state == ECompState.Get;
		_listener = listener;
		_cachedData = data;
	}

	public void Refresh(StepUpEquipGachaInfo info, PayShopEquipGachaStepUpRewardList.StepUpGachaRewardSlotData slotData)
	{
		ReleaseItemIcon();
		ECompState state = GetState(_cachedData, info);
		StepUpGachaRewardData nexData = GameRoot.Instance.StepUpGachaRewardDataContainer.GetNexData(_cachedData);
		SetGetEffect(_cachedData.GetMainRewardType(), state);
		SetEmptyGetEffect(_cachedData.GetMainRewardType(), state);
		_cachedAlreadyGet = state == ECompState.Get;
		SetMainReward(_cachedData, nexData, slotData, state);
		SetSubReward(_cachedData, state);
		SetEffect(_cachedData, state);
	}

	private void SetMainReward(StepUpGachaRewardData curData, StepUpGachaRewardData nextData, PayShopEquipGachaStepUpRewardList.StepUpGachaRewardSlotData slotData, ECompState compState)
	{
		EStepUpGachaRewardType mainRewardType = curData.GetMainRewardType();
		SetBg(mainRewardType, compState);
		SetSale(mainRewardType, curData.Discount);
		SetGet(mainRewardType, compState);
		SetActive(mainRewardType, compState);
		SetItemIcon(curData, mainRewardType, MainIconRoot, isMain: true, 1);
		if (nextData == null || nextData.Round != curData.Round)
		{
			HideArrow();
			return;
		}
		EStepUpGachaRewardType mainRewardType2 = nextData.GetMainRewardType();
		SetArrow(slotData, mainRewardType, mainRewardType2, compState);
	}

	private void SetSubReward(StepUpGachaRewardData curData, ECompState compState)
	{
		ESubRewardType subRewardType = GetSubRewardType(curData);
		bool flag = subRewardType != ESubRewardType.Emtpy;
		UIHelper.EnableGameObject(SubRoot, flag);
		if (flag)
		{
			bool enable = compState == ECompState.Get;
			bool enable2 = compState == ECompState.Current;
			UIHelper.EnableGameObject(SubSpriteGet, enable);
			UIHelper.EnableGameObject(SubSpriteActive, enable2);
			if (subRewardType == ESubRewardType.RewardItem1)
			{
				SetItemIcon(curData, EStepUpGachaRewardType.RewardItem, SubIconRoot, isMain: false, 1);
			}
			if (subRewardType == ESubRewardType.RewardItem2)
			{
				SetItemIcon(curData, EStepUpGachaRewardType.RewardItem, SubIconRoot, isMain: false, 2);
			}
			else
			{
				_ = 3;
			}
		}
	}

	private void SetEffect(StepUpGachaRewardData curData, ECompState compState)
	{
		EStepUpGachaRewardType mainRewardType = curData.GetMainRewardType();
		ESubRewardType subRewardType = GetSubRewardType(curData);
		bool isPlay = true;
		bool isPlay2 = true;
		bool isPlay3 = false;
		if (compState == ECompState.Get || compState == ECompState.NotYet)
		{
			isPlay = false;
			isPlay2 = false;
		}
		if (mainRewardType == EStepUpGachaRewardType.Empty)
		{
			if (compState == ECompState.Current)
			{
				isPlay3 = true;
			}
			isPlay = false;
		}
		if (subRewardType == ESubRewardType.Emtpy)
		{
			isPlay2 = false;
		}
		UI3DWidgetPlay(UI3DWidgetParticle3, isPlay3);
		UI3DWidgetPlay(UI3DWidgetParticle1, isPlay);
		UI3DWidgetPlay(UI3DWidgetParticle2, isPlay2);
	}

	private ESubRewardType GetSubRewardType(StepUpGachaRewardData curData)
	{
		EStepUpGachaRewardType allRewardType = curData.GetAllRewardType();
		if (CheckRewardType(allRewardType, EStepUpGachaRewardType.Empty))
		{
			return ESubRewardType.Emtpy;
		}
		EStepUpGachaRewardType mainRewardType = curData.GetMainRewardType();
		if (allRewardType == mainRewardType)
		{
			if (CheckRewardType(mainRewardType, EStepUpGachaRewardType.RewardItem))
			{
				RewardData data = GameRoot.Instance.RewardDataContainer.GetData(curData.RewardIndex);
				if (data == null)
				{
					return ESubRewardType.Emtpy;
				}
				if (data.Item2Code.IsNullOrEmpty())
				{
					return ESubRewardType.Emtpy;
				}
				return ESubRewardType.RewardItem2;
			}
			return ESubRewardType.Emtpy;
		}
		if (CheckRewardType(mainRewardType, EStepUpGachaRewardType.Discount))
		{
			if (CheckRewardType(allRewardType, EStepUpGachaRewardType.RewardItem))
			{
				return ESubRewardType.RewardItem1;
			}
			if (CheckRewardType(allRewardType, EStepUpGachaRewardType.Buff))
			{
				return ESubRewardType.Buff;
			}
			return ESubRewardType.Emtpy;
		}
		if (CheckRewardType(mainRewardType, EStepUpGachaRewardType.RewardItem) && CheckRewardType(allRewardType, EStepUpGachaRewardType.Buff))
		{
			return ESubRewardType.Buff;
		}
		return ESubRewardType.Emtpy;
	}

	private void SetBg(EStepUpGachaRewardType rewardType, ECompState compState)
	{
		int spriteSelectoBgIndex = GetSpriteSelectoBgIndex(rewardType, compState);
		SetSpriteSelectByIndex(SpriteSelectorBg, spriteSelectoBgIndex);
	}

	private void SetSale(EStepUpGachaRewardType rewardType, int discount)
	{
		bool flag = rewardType == EStepUpGachaRewardType.Discount;
		UIHelper.EnableGameObject(SpriteSelectorSale, flag);
		if (flag)
		{
			string text = discount.ToString();
			if (SpriteSelectorSale != null)
			{
				SpriteSelectorSale.Select(text);
			}
		}
	}

	private bool CheckRewardType(EStepUpGachaRewardType rewardType, EStepUpGachaRewardType checkType)
	{
		return (checkType & rewardType) != 0;
	}

	private void SetGet(EStepUpGachaRewardType rewardType, ECompState compState)
	{
		bool get = !CheckRewardType(rewardType, EStepUpGachaRewardType.Empty) && compState == ECompState.Get;
		SetGet(get);
	}

	private void SetGet(bool isEnableObj)
	{
		UIHelper.EnableGameObject(SpriteGet, isEnableObj);
	}

	private void SetEmptyGetEffect(EStepUpGachaRewardType rewardType, ECompState compState)
	{
		bool isPlay = CheckRewardType(rewardType, EStepUpGachaRewardType.Empty) && compState == ECompState.Get && !_cachedAlreadyGet;
		UI3DWidgetPlay(UI3DWidgetEmptyActive, isPlay);
	}

	private void SetGetEffect(EStepUpGachaRewardType rewardType, ECompState compState)
	{
		bool isPlay = !CheckRewardType(rewardType, EStepUpGachaRewardType.Empty) && compState == ECompState.Get && !_cachedAlreadyGet;
		UI3DWidgetPlay(UI3DWidgetGet, isPlay);
	}

	private void SetActive(EStepUpGachaRewardType rewardType, ECompState compState)
	{
		bool flag = compState == ECompState.Current;
		UIHelper.EnableGameObject(SpriteSelectorActive, flag);
		if (flag)
		{
			int spriteSelectorActivateIndex = GetSpriteSelectorActivateIndex(rewardType);
			SetSpriteSelectByIndex(SpriteSelectorActive, spriteSelectorActivateIndex);
		}
	}

	private void SetItemIcon(StepUpGachaRewardData curData, EStepUpGachaRewardType rewardType, Transform iconRoot, bool isMain, int itemNo)
	{
		bool flag = CheckRewardType(rewardType, EStepUpGachaRewardType.RewardItem);
		UIHelper.EnableGameObject(iconRoot, flag);
		if (!flag)
		{
			return;
		}
		RewardElementData rewardElementData = GetRewardElementData(curData.RewardIndex, itemNo);
		if (rewardElementData != null)
		{
			if (rewardElementData.GetCurrencyType() != CurrencyType.None)
			{
				CreateCurrencyItemIcon(rewardElementData, iconRoot);
			}
			else
			{
				CreateItemIcon(rewardElementData, iconRoot, isMain);
			}
		}
	}

	private void UI3DWidgetPlay(UIGame3DWidget widget, bool isPlay)
	{
		if (widget == null)
		{
			return;
		}
		if (isPlay)
		{
			if (widget.IsPlaying)
			{
				widget.Stop(immediatelyClear: false);
			}
			widget.SetDirty();
			widget.Play(null);
		}
		else
		{
			widget.Stop(immediatelyClear: false);
		}
	}

	private void EffectPlay(UIScreenEffectPlayer effectPlayer, bool isPlay)
	{
		if (effectPlayer == null)
		{
			return;
		}
		if (isPlay)
		{
			if (effectPlayer.IsPlaying)
			{
				effectPlayer.Stop(immediatelyClear: false);
			}
			effectPlayer.Play(null);
		}
		else
		{
			effectPlayer.Stop(immediatelyClear: false);
		}
	}

	public void ReleaseItemIcon()
	{
		FactoryBase.ReleaseAndClearList(_iconComponent);
	}

	private void CreateCurrencyItemIcon(RewardElementData rewardElement, Transform root)
	{
		CurrencyIconComponent currencyIconComponent = CurrencyIconComponent.PrefabAuto.Factory.Create(root);
		if (!(currencyIconComponent == null))
		{
			CurrencyType currencyType = rewardElement.GetCurrencyType();
			int itemMin = rewardElement.ItemMin;
			CurrencyIconComponent.Listener listener = null;
			int num = itemMin;
			currencyIconComponent.Open(currencyType, num, listener);
			_iconComponent.Add(currencyIconComponent);
		}
	}

	private ItemData GetItemData(RewardElementData rewardElement, int roopCount = 1)
	{
		if (rewardElement == null || rewardElement.ItemCode.IsNullOrEmpty())
		{
			return null;
		}
		if (roopCount > 3)
		{
			return null;
		}
		ItemData dataByCode = GameRoot.Instance.ItemDataContainer.GetDataByCode(rewardElement.ItemCode);
		if (dataByCode != null)
		{
			return dataByCode;
		}
		PackageItemData data = GameRoot.Instance.PackageItemDataContainer.GetData(rewardElement.ItemCode);
		if (data == null)
		{
			return null;
		}
		int rewardIndex = data.RewardIndex1;
		RewardElementData rewardElementData = GetRewardElementData(rewardIndex, 1);
		return GetItemData(rewardElementData, roopCount + 1);
	}

	private void CreateItemIcon(RewardElementData rewardElement, Transform root, bool isMain)
	{
		ItemData itemData = GetItemData(rewardElement);
		if (itemData == null)
		{
			return;
		}
		ItemIconComponent itemIconComponent = ItemIconComponent.PrefabAuto.Factory.Create(root);
		if (!(itemIconComponent == null))
		{
			Vespa_Unity_Transform_Extension_Method.ResetLocalTM(itemIconComponent.transform);
			if (isMain)
			{
				itemIconComponent.Open(itemData, this);
			}
			else
			{
				itemIconComponent.Open(itemData);
			}
			_iconComponent.Add(itemIconComponent);
			int num = (isMain ? 1 : 2);
			itemIconComponent.ChangeWidgetDepth(100 * num);
		}
	}

	private RewardElementData GetRewardElementData(int rewardIndex, int itemNo)
	{
		RewardData data = GameRoot.Instance.RewardDataContainer.GetData(rewardIndex);
		if (data == null || data.Item1Code.IsNullOrEmpty())
		{
			return null;
		}
		RewardElementData rewardElementData = null;
		if (itemNo == 1)
		{
			return data.GetElementByNo(1);
		}
		return data.GetElementByNo(2);
	}

	private void SetArrow(PayShopEquipGachaStepUpRewardList.StepUpGachaRewardSlotData slotData, EStepUpGachaRewardType curRewardType, EStepUpGachaRewardType nextRewardType, ECompState compState)
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(ArrowSprites); i++)
		{
			ArrowComponent arrowComponent = ArrowSprites[i];
			if (arrowComponent != null)
			{
				bool flag = arrowComponent.Direction == slotData.AllowDirection;
				UIHelper.EnableGameObject(arrowComponent.GoAllow, flag);
				if (flag)
				{
					int spriteSelectorArrowIndex = GetSpriteSelectorArrowIndex(curRewardType, nextRewardType, compState);
					SetSpriteSelectByIndex(arrowComponent.SpriteSelector, spriteSelectorArrowIndex);
				}
			}
		}
	}

	private void HideArrow()
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(ArrowSprites); i++)
		{
			ArrowComponent arrowComponent = ArrowSprites[i];
			if (arrowComponent != null)
			{
				UIHelper.EnableGameObject(arrowComponent.GoAllow, enable: false);
			}
		}
	}

	private void SetSpriteSelectByIndex(UISpriteSelector selector, int index)
	{
		if (!(selector == null))
		{
			selector.SelectByIndex(index);
		}
	}

	private bool IsLastReward(StepUpGachaRewardData data)
	{
		if (data == null)
		{
			return false;
		}
		StepUpGachaRewardData nexData = GameRoot.Instance.StepUpGachaRewardDataContainer.GetNexData(data);
		if (nexData == null)
		{
			return false;
		}
		if (data.Round != nexData.Round)
		{
			return true;
		}
		if (data.Step > nexData.Step)
		{
			return true;
		}
		return false;
	}

	private ECompState GetState(StepUpGachaRewardData data, StepUpEquipGachaInfo info)
	{
		StepUpGachaRewardData data2 = GameRoot.Instance.StepUpGachaRewardDataContainer.GetData(info.GachaIndex, info.CompletedStep);
		if (IsLastReward(data2))
		{
			if (GameRoot.Instance.StepUpGachaRewardDataContainer.GetNexData(data2).Step == data.Step)
			{
				return ECompState.Current;
			}
			return ECompState.NotYet;
		}
		if (data.Step <= info.CompletedStep)
		{
			return ECompState.Get;
		}
		if (data.Step == info.CompletedStep + 1)
		{
			return ECompState.Current;
		}
		return ECompState.NotYet;
	}

	private int GetSpriteSelectoBgIndex(EStepUpGachaRewardType rewardType, ECompState compState)
	{
		if (CheckRewardType(rewardType, EStepUpGachaRewardType.Empty))
		{
			if (compState == ECompState.NotYet || compState == ECompState.Current)
			{
				return 1;
			}
			return 2;
		}
		return 0;
	}

	private int GetSpriteSelectorActivateIndex(EStepUpGachaRewardType rewardType)
	{
		if (CheckRewardType(rewardType, EStepUpGachaRewardType.Empty))
		{
			return 1;
		}
		return 0;
	}

	private int GetSpriteSelectorArrowIndex(EStepUpGachaRewardType curRewardType, EStepUpGachaRewardType nextRewardType, ECompState compState)
	{
		if (!CheckRewardType(curRewardType, EStepUpGachaRewardType.Empty))
		{
			curRewardType = EStepUpGachaRewardType.RewardItem;
		}
		if (!CheckRewardType(nextRewardType, EStepUpGachaRewardType.Empty))
		{
			nextRewardType = EStepUpGachaRewardType.RewardItem;
		}
		int num = 0;
		if (curRewardType == EStepUpGachaRewardType.RewardItem && nextRewardType == EStepUpGachaRewardType.Empty)
		{
			num = 1;
		}
		else if (curRewardType == EStepUpGachaRewardType.Empty && nextRewardType == EStepUpGachaRewardType.RewardItem)
		{
			num = 2;
		}
		else if (curRewardType == EStepUpGachaRewardType.Empty && nextRewardType == EStepUpGachaRewardType.Empty)
		{
			num = 3;
		}
		if (compState == ECompState.Get)
		{
			num += 4;
		}
		return num;
	}

	public void OnClick()
	{
		if (_cachedData != null && _cachedData.GetMainRewardType() != EStepUpGachaRewardType.RewardItem && _cachedData.GetMainRewardType() != EStepUpGachaRewardType.Empty && _listener != null)
		{
			_listener.OnClickRewardItem(_cachedData);
		}
	}

	void ItemIconComponent.Listener.OnClick(ItemIconComponent itemIconComponent)
	{
		if (_cachedData != null && _cachedData.GetMainRewardType() == EStepUpGachaRewardType.RewardItem && _listener != null)
		{
			_listener.OnClickRewardItem(_cachedData);
		}
	}
}
