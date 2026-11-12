using System;
using System.Collections.Generic;
using System.Linq;
using NGame2.NAccount;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NManager;
using NShared;
using NShared.NLocalization;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class TrialChallengeHeroSelect : BaseWindow, HeroPortraitComponent.Listener, IUserObserver<ItemInfo>, IUserObserver
{
	public interface Listener
	{
		void ChallengeEnterHeroDungeon(CachedHeroInfo heroInfo);
	}

	public const string PrefabPath = "Resources/Prefab2/Window/HeroTrialDungeon/TrialChallengeHeroSelect";

	public UISimpleGrid ItemListGrid;

	public UIScrollGrid HeroListGrid;

	public UISimpleGrid NeedAwakeChallengeItemGrid;

	public UISimpleGrid NeedTranscendChallengeItemGrid;

	public UIWidget WidgetOnSelectHeroAwake;

	public UIWidget WidgetOnSelectHeroTranscend;

	public UIToggle Toggle_Awake;

	public UIToggle Toggle_Transcend;

	public Transform ToggleGrid;

	public Transform AwakeRoot;

	public Transform TranscendRoot;

	public UIWidget AwakeWidget;

	public UIWidget TranscentWidget;

	public UIWidget WidgetNoHeroSelectedAwake;

	public UIWidget WidgetNoHeroSelectedTranscend;

	private Listener _listener;

	private HeroAwakeType _toggle;

	private HeroPortraitComponent _SelectedPortrait;

	private List<ItemIconComponent> _ListFragmentItems = new List<ItemIconComponent>();

	private List<HeroPortraitComponent> _ListHeroPortrait = new List<HeroPortraitComponent>();

	private List<AwakeChallengeItemComponent> _ListAwakeChallengeItems = new List<AwakeChallengeItemComponent>();

	private List<AwakeChallengeItemComponent> _ListTranscendChallengeItems = new List<AwakeChallengeItemComponent>();

	private IList<CachedHeroInfo> _InitHeroInfos;

	public override void Reset()
	{
		_SelectedPortrait = null;
		FactoryBase.ReleaseAndClearList(_ListHeroPortrait);
		FactoryBase.ReleaseAndClearList(_ListFragmentItems);
		FactoryBase.ReleaseAndClearList(_ListAwakeChallengeItems);
		FactoryBase.ReleaseAndClearList(_ListTranscendChallengeItems);
	}

	public void Open(Listener listener, HeroAwakeType heroAwakeType, IList<CachedHeroInfo> heroInfos = null, Action openCompleteCallback = null)
	{
		Open(delegate
		{
			Init(listener, heroAwakeType, heroInfos);
		}, openCompleteCallback);
	}

	public void Init(Listener listener, HeroAwakeType heroAwakeType, IList<CachedHeroInfo> heroInfos = null)
	{
		_listener = listener;
		_toggle = heroAwakeType;
		SetDefaultToggle();
		_InitHeroInfos = heroInfos;
		SetupToggledWidget();
		SetupChallengeTotalReqItemList();
		SetupHeroList();
		SetupSelectHeroDescWidget();
		UIHelper.EnableGameObject(ToggleGrid, enable: true);
		Toggle_Awake.Set(_toggle == HeroAwakeType.Awaken);
		Toggle_Transcend.Set(_toggle == HeroAwakeType.Transcend);
	}

	private void SetupChallengeTotalReqItemList()
	{
		IList<ItemData> itemDataListByType = GameRoot.Instance.ItemDataContainer.GetItemDataListByType(ItemType.AwakeChallenge);
		if (itemDataListByType == null)
		{
			return;
		}
		List<ItemData> list = itemDataListByType.ToList();
		list.Sort((ItemData l, ItemData r) => l.Index.CompareTo(r.Index));
		for (int num = 0; num < list.Count; num++)
		{
			ItemData data = GameRoot.Instance.ItemDataContainer.GetData(list[num].Index);
			if (data == null)
			{
				break;
			}
			if ((data.Grade != 4 || _toggle != HeroAwakeType.Transcend) && (data.Grade != 6 || _toggle != HeroAwakeType.Awaken))
			{
				ItemIconComponent itemIconComponent = UIHelper.AddGridItem(ItemListGrid, ItemIconComponent.PrefabAuto);
				int itemCount = Helper.GetItemCount(data.Index);
				itemIconComponent.Open(data);
				itemIconComponent.SetIconGrayScale(itemCount == 0);
				itemIconComponent.SetItemCount_notzero(itemCount);
				_ListFragmentItems.Add(itemIconComponent);
			}
		}
		UIHelper.RepositionGrid(ItemListGrid);
	}

	private void SetupHeroList()
	{
		FactoryBase.ReleaseAndClearList(_ListHeroPortrait);
		IList<CachedHeroInfo> list = null;
		if (_InitHeroInfos == null)
		{
			if (_toggle == HeroAwakeType.Awaken)
			{
				List<CachedHeroInfo> awakeChallengeableHeroes = GlobalAutoBehaviour<AccountManager>.instance.HeroManager.GetAwakeChallengeableHeroes(CreatureTagType.None);
				awakeChallengeableHeroes.Sort(delegate(CachedHeroInfo l, CachedHeroInfo r)
				{
					int num3 = -l.Star.CompareTo(r.Star);
					return (num3 != 0) ? num3 : l.HeroIndex.CompareTo(r.HeroIndex);
				});
				list = awakeChallengeableHeroes;
			}
			else
			{
				List<CachedHeroInfo> transcendChallengeableHeroes = GlobalAutoBehaviour<AccountManager>.instance.HeroManager.GetTranscendChallengeableHeroes(CreatureTagType.None);
				transcendChallengeableHeroes.Sort(delegate(CachedHeroInfo l, CachedHeroInfo r)
				{
					int num3 = -l.Transcended.CompareTo(r.Transcended);
					return (num3 != 0) ? num3 : l.HeroIndex.CompareTo(r.HeroIndex);
				});
				list = transcendChallengeableHeroes;
			}
			for (int num = 0; num < __Extension_Method_ICollection__.COUNT(list); num++)
			{
				CreateTrialHeroComponent(list[num]);
			}
		}
		else
		{
			for (int num2 = 0; num2 < __Extension_Method_ICollection__.COUNT(_InitHeroInfos); num2++)
			{
				CreateTrialHeroComponent(_InitHeroInfos[num2]);
			}
		}
		UIHelper.RepositionGrid(HeroListGrid);
		_SelectedPortrait = null;
	}

	public void UpdateHeroPortrait(CachedHeroInfo heroInfo)
	{
		HeroPortraitComponent heroPortraitComponent = _ListHeroPortrait.Find((HeroPortraitComponent e) => e.HeroInfo.HeroIndex == heroInfo.HeroIndex);
		if (!(heroPortraitComponent == null))
		{
			heroPortraitComponent.SetGrayScale(!CanHeroTrialChallenge(heroInfo));
			heroPortraitComponent.ShowShortOfFragment(!CanHeroTrialChallenge(heroInfo), _toggle);
		}
	}

	private void CreateTrialHeroComponent(CachedHeroInfo heroInfo)
	{
		if (heroInfo != null)
		{
			HeroPortraitComponent heroPortraitComponent = UIHelper.AddGridItem(HeroListGrid, HeroPortraitComponent.PrefabAuto);
			heroPortraitComponent.Open(heroInfo, this);
			heroPortraitComponent.ShowCreatureName();
			heroPortraitComponent.SetGrayScale(!CanHeroTrialChallenge(heroInfo));
			heroPortraitComponent.ShowShortOfFragment(!CanHeroTrialChallenge(heroInfo), _toggle);
			_ListHeroPortrait.Add(heroPortraitComponent);
		}
	}

	void HeroPortraitComponent.Listener.OnClick(HeroPortraitComponent comp)
	{
		if (CanHeroTrialChallenge(comp.HeroInfo))
		{
			if (_listener != null)
			{
				_listener.ChallengeEnterHeroDungeon(comp.HeroInfo);
			}
			return;
		}
		if (_SelectedPortrait != null)
		{
			_SelectedPortrait.SetTrialHeroSelect(select: false);
		}
		if (Vespa_Common_Class_Extension_Method.EQUALS(_SelectedPortrait, comp))
		{
			_SelectedPortrait = null;
		}
		else
		{
			_SelectedPortrait = comp;
			_SelectedPortrait.SetTrialHeroSelect(select: true);
		}
		SetupSelectHeroDescWidget();
	}

	void HeroPortraitComponent.Listener.OnViewInfo(HeroPortraitComponent comp, bool isMine)
	{
	}

	void HeroPortraitComponent.Listener.OnClickHeroNotify(HeroNotifyComponent.ENotifyInfo notifyInfo, CachedHeroInfo cachedHeroInfo)
	{
	}

	void HeroPortraitComponent.Listener.OnLongClick(HeroPortraitComponent heroPortrait, CachedHeroInfo heroInfo)
	{
	}

	private void SetupSelectHeroDescWidget(bool initAwakeItemDesc = true)
	{
		UIHelper.EnableGameObject(WidgetNoHeroSelectedAwake, _SelectedPortrait == null);
		UIHelper.EnableGameObject(WidgetNoHeroSelectedTranscend, _SelectedPortrait == null);
		if (_SelectedPortrait == null)
		{
			UIHelper.EnableGameObject(WidgetOnSelectHeroAwake, enable: false);
			UIHelper.EnableGameObject(WidgetOnSelectHeroTranscend, enable: false);
			return;
		}
		UIHelper.EnableGameObject(WidgetOnSelectHeroAwake, enable: true);
		UIHelper.EnableGameObject(WidgetOnSelectHeroTranscend, enable: true);
		if (initAwakeItemDesc && !CanHeroTrialChallenge(_SelectedPortrait.HeroInfo))
		{
			SetupAwakeChallengeItemDesc(_SelectedPortrait.HeroInfo);
		}
	}

	private void SetupAwakeChallengeItemDesc(CachedHeroInfo heroInfo)
	{
		//IL_015e: Expected I8, but got I4
		//IL_00ac: Expected I8, but got I4
		//IL_01c2: Expected I8, but got I4
		if (heroInfo == null)
		{
			return;
		}
		CreatureData data = GameRoot.Instance.CreatureDataContainer.GetData(heroInfo.HeroIndex);
		if (data == null)
		{
			return;
		}
		CreatureAwakeChallengeItemData data2 = GameRoot.Instance.CreatureAwakeChallengeItemDataContainer.GetData(data.TagType);
		if (data2 == null)
		{
			return;
		}
		FactoryBase.ReleaseAndClearList(_ListAwakeChallengeItems);
		FactoryBase.ReleaseAndClearList(_ListTranscendChallengeItems);
		if (_toggle == HeroAwakeType.Awaken)
		{
			int awakeHeroItemCount = HeroTrialDungeonManagement.GetAwakeHeroItemCount(heroInfo.Star);
			for (int i = 1; i <= 3; i++)
			{
				ItemData dataByCode = GameRoot.Instance.ItemDataContainer.GetDataByCode(data2.GetItemCode(i));
				if (dataByCode != null)
				{
					AwakeChallengeItemComponent awakeChallengeItemComponent = UIHelper.AddGridItem(NeedAwakeChallengeItemGrid, AwakeChallengeItemComponent.PrefabAuto);
					SprkHeroMaterialTickets.OpenHall(awakeChallengeItemComponent, dataByCode, awakeHeroItemCount, Helper.GetItemCount(dataByCode.Index), null);
					_ListAwakeChallengeItems.Add(awakeChallengeItemComponent);
				}
			}
			UIHelper.RepositionGrid(NeedAwakeChallengeItemGrid);
		}
		else
		{
			if (_toggle != HeroAwakeType.Transcend)
			{
				return;
			}
			CreatureStarData nextData = GameRoot.Instance.CreatureStarDataContainer.GetNextData(heroInfo.Star, heroInfo.Transcended);
			if (nextData == null)
			{
				return;
			}
			int heroAwakeItemCount = nextData.HeroAwakeItemCount;
			int heroTranscendItemIndex = nextData.HeroTranscendItemIndex;
			ItemData data3 = GameRoot.Instance.ItemDataContainer.GetData(heroTranscendItemIndex);
			if (data3 != null)
			{
				AwakeChallengeItemComponent awakeChallengeItemComponent2 = UIHelper.AddGridItem(NeedTranscendChallengeItemGrid, AwakeChallengeItemComponent.PrefabAuto);
				SprkHeroMaterialTickets.OpenHall(awakeChallengeItemComponent2, data3, nextData.HeroTranscendItemCount, Helper.GetItemCount(data3.Index), OnClickCraftItem);
				_ListTranscendChallengeItems.Add(awakeChallengeItemComponent2);
			}
			for (int j = 1; j <= 3; j++)
			{
				data3 = GameRoot.Instance.ItemDataContainer.GetDataByCode(data2.GetTranscendItemCode(j));
				if (data3 != null)
				{
					AwakeChallengeItemComponent awakeChallengeItemComponent3 = UIHelper.AddGridItem(NeedTranscendChallengeItemGrid, AwakeChallengeItemComponent.PrefabAuto);
					SprkHeroMaterialTickets.OpenHall(awakeChallengeItemComponent3, data3, heroAwakeItemCount, Helper.GetItemCount(data3.Index), OnClickCraftItem);
					_ListTranscendChallengeItems.Add(awakeChallengeItemComponent3);
				}
			}
			UIHelper.RepositionGrid(NeedTranscendChallengeItemGrid);
		}
	}

	private void SetupToggledWidget()
	{
		UIHelper.EnableGameObject(AwakeRoot, _toggle == HeroAwakeType.Awaken);
		UIHelper.EnableGameObject(TranscendRoot, _toggle == HeroAwakeType.Transcend);
		UIHelper.EnableGameObject(AwakeWidget, _toggle == HeroAwakeType.Awaken);
		UIHelper.EnableGameObject(TranscentWidget, _toggle == HeroAwakeType.Transcend);
	}

	public static bool CanHeroTrialChallenge(CachedHeroInfo heroInfo)
	{
		if (heroInfo == null)
		{
			return false;
		}
		if (heroInfo.EnterDungeon == 1)
		{
			return true;
		}
		if (NGame2.NAccount.UserHeroManager.IsMaxStar(heroInfo))
		{
			int valueInt = GameRoot.Instance.ConstantDataContainer.GetValueInt("HeroMaxTranscend");
			if (heroInfo.Transcended >= valueInt)
			{
				return false;
			}
			return FragmentSatisfied(heroInfo, HeroAwakeType.Transcend);
		}
		return FragmentSatisfied(heroInfo, HeroAwakeType.Awaken);
	}

	public static bool FragmentSatisfied(CachedHeroInfo heroInfo, HeroAwakeType toggle)
	{
		if (heroInfo == null)
		{
			return false;
		}
		CreatureData data = GameRoot.Instance.CreatureDataContainer.GetData(heroInfo.HeroIndex);
		if (data == null)
		{
			return false;
		}
		CreatureAwakeChallengeItemData data2 = GameRoot.Instance.CreatureAwakeChallengeItemDataContainer.GetData(data.TagType);
		if (data2 == null)
		{
			return false;
		}
		if (toggle == HeroAwakeType.Awaken)
		{
			int awakeHeroItemCount = HeroTrialDungeonManagement.GetAwakeHeroItemCount(heroInfo.Star);
			if (awakeHeroItemCount == 0)
			{
				return false;
			}
			for (int i = 1; i <= 3; i++)
			{
				if (Helper.GetAccountItemCount(data2.GetItemCode(i)) < awakeHeroItemCount)
				{
					return false;
				}
			}
			return true;
		}
		CreatureStarData nextData = GameRoot.Instance.CreatureStarDataContainer.GetNextData(heroInfo.Star, heroInfo.Transcended);
		if (nextData == null)
		{
			return false;
		}
		int heroAwakeItemCount = nextData.HeroAwakeItemCount;
		if (heroAwakeItemCount == 0)
		{
			return false;
		}
		for (int j = 1; j <= 3; j++)
		{
			if (Helper.GetAccountItemCount(data2.GetTranscendItemCode(j)) < heroAwakeItemCount)
			{
				return false;
			}
		}
		int heroTranscendItemIndex = nextData.HeroTranscendItemIndex;
		int heroTranscendItemCount = nextData.HeroTranscendItemCount;
		if (Helper.GetItemCount(heroTranscendItemIndex) < heroTranscendItemCount)
		{
			return false;
		}
		return true;
	}

	public void RefreshAwakeChallengeItemList()
	{
		FactoryBase.ReleaseAndClearList(_ListFragmentItems);
		SetupChallengeTotalReqItemList();
	}

	public void OnClickEnterDungeon()
	{
		if (_listener != null && _SelectedPortrait != null)
		{
			_listener.ChallengeEnterHeroDungeon(_SelectedPortrait.HeroInfo);
		}
	}

	public void OnChangeToggle(UIToggle toggle)
	{
		if (toggle.value)
		{
			if (Toggle_Transcend.value)
			{
				_toggle = HeroAwakeType.Transcend;
			}
			else
			{
				_toggle = HeroAwakeType.Awaken;
			}
			SetupToggledWidget();
			RefreshAwakeChallengeItemList();
			SetupHeroList();
			SetupSelectHeroDescWidget();
		}
	}

	private bool IsEnableCraft(string CraftItemCode)
	{
		IList<CraftItemData> categoryData = GameRoot.Instance.CraftItemDataContainer.GetCategoryData(CraftItemCode);
		if (categoryData == null)
		{
			return false;
		}
		foreach (CraftItemData item in categoryData)
		{
			foreach (CraftItemData.MaterialItemInfo materialItem in item.GetMaterialItemList())
			{
				if (materialItem != null && GlobalAutoBehaviour<AccountManager>.instance.ItemManager.GetItemCount(materialItem.ItemCode) < materialItem.ItemCount)
				{
					return false;
				}
			}
		}
		return true;
	}

	public void OnClickCraftItem(ItemIconComponent This)
	{
		if (!(This == null) || This.ItemData != null)
		{
			IsEnableCraft(This.ItemData.Code);
			string text = LocalizationManager.GetString(NShared.NLocalization.Common.INFORM);
			string empty = string.Empty;
			int reqSelectedItemCount = 0;
			CreatureStarData nextData = GameRoot.Instance.CreatureStarDataContainer.GetNextData(_SelectedPortrait.HeroInfo.Star, _SelectedPortrait.HeroInfo.Transcended);
			int itemCount = GlobalAutoBehaviour<AccountManager>.instance.ItemManager.GetItemCount(This.ItemData.Code);
			int num = 0;
			if (This.ItemData.Type == ItemType.AwakeChallenge)
			{
				num = nextData?.HeroAwakeItemCount ?? 0;
			}
			else if (This.ItemData.Type == ItemType.HeroEnhanceMaterial)
			{
				num = nextData?.HeroTranscendItemCount ?? 0;
			}
			reqSelectedItemCount = num - itemCount;
			empty = ((reqSelectedItemCount > 0) ? LocalizationManager.GetString(Inventory.INVENTORY_ENOUGH_CRAFT_MSG_NEW, This.ItemData.Name, reqSelectedItemCount) : LocalizationManager.GetString(Inventory.INVENTORY_ENOUGH_CRAFT_MSG));
			MessagePopup.Open(text, empty, delegate
			{
				SceneAutoBehaviour<InventoryManagement>.instance.OpenShortCutCraftWindow(This.ItemData, reqSelectedItemCount);
			}, null, Inventory.INVENTORY_CRAFT_MOVE_MSG, NShared.NLocalization.Common.COMMON_CANCEL);
		}
	}

	private void SetDefaultToggle()
	{
		if (_toggle == HeroAwakeType.Awaken)
		{
			int num = __Extension_Method_ICollection__.COUNT(GlobalAutoBehaviour<AccountManager>.instance.HeroManager.GetAwakeChallengeableHeroes(CreatureTagType.None));
			int num2 = __Extension_Method_ICollection__.COUNT(GlobalAutoBehaviour<AccountManager>.instance.HeroManager.GetTranscendChallengeableHeroes(CreatureTagType.None));
			if (num == 0 && num2 > 0)
			{
				_toggle = HeroAwakeType.Transcend;
			}
		}
	}

	bool IUserObserver.IsGlobal()
	{
		return false;
	}

	void IUserObserver<ItemInfo>.OnChanged(ItemInfo changedInfo)
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_ListFragmentItems); i++)
		{
			if (_ListFragmentItems[i].ItemIndex == changedInfo.ItemIndex)
			{
				ItemIconComponent itemIconComponent = _ListFragmentItems[i];
				int count = changedInfo.Count;
				Color? color = null;
				int num = count;
				itemIconComponent.SetItemCount(num, color);
				_ListFragmentItems[i].SetIconGrayScale(changedInfo.Count == 0);
				break;
			}
		}
		for (int j = 0; j < __Extension_Method_ICollection__.COUNT(_ListHeroPortrait); j++)
		{
			HeroPortraitComponent heroPortraitComponent = _ListHeroPortrait[j];
			bool flag = CanHeroTrialChallenge(heroPortraitComponent.HeroInfo);
			heroPortraitComponent.SetGrayScale(!flag);
			heroPortraitComponent.ShowShortOfFragment(!flag, _toggle);
		}
		if (_toggle == HeroAwakeType.Transcend && _SelectedPortrait != null)
		{
			CreatureStarData nextData = GameRoot.Instance.CreatureStarDataContainer.GetNextData(_SelectedPortrait.HeroInfo.Star, _SelectedPortrait.HeroInfo.Transcended);
			if (nextData != null)
			{
				int reqCount = ((changedInfo.ItemIndex == nextData.HeroTranscendItemIndex) ? nextData.HeroTranscendItemCount : nextData.HeroAwakeItemCount);
				for (int k = 0; k < __Extension_Method_ICollection__.COUNT(_ListTranscendChallengeItems); k++)
				{
					if (_ListTranscendChallengeItems[k].GetCurrentItemIndex() == changedInfo.ItemIndex)
					{
						_ListTranscendChallengeItems[k].RefreshItemCount(reqCount, changedInfo.Count);
						break;
					}
				}
				SetupSelectHeroDescWidget(initAwakeItemDesc: false);
			}
		}
		SprkRefreshTicketMaterials();
	}

	private void SprkRefreshTicketMaterials()
	{
		if ((object)_SelectedPortrait != null)
		{
			SetupAwakeChallengeItemDesc(_SelectedPortrait.HeroInfo);
		}
	}
}
