using System;
using System.Collections.Generic;
using System.Linq;
using NGame2.IAP;
using NGame2.NDropBonusEvent;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NManager;
using NShared;
using NShared.GetGachaItemGroupCode;
using NVespa;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NSingleton;
using NVespa.NUtil;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class ProbabilityPopup : BaseWindow, UIScrollItemBehaviour.IListener, ProbabilityTabComponent.IListener, IInspectorGuiDrawer
{
	private class GachaItemGroupCodeData
	{
		public string ItemGroupCode;

		public string ItemGroupCodeNameKey;

		public string BonusItemGroupCode;

		public string BonusItemGroupCodeNameKey;
	}

	public class ProbabilityInfoData
	{
		public List<ProbabilityBaseData> ProbabilityDataList;

		public List<ProbabilityBaseData> ProbabilityItemGroupList;

		public string Title;

		public bool IsGachaSelectShopTab;

		public bool IsShowItemGroupList;

		public List<ProbabilityBaseData> CurrentShowProbabilityList
		{
			get
			{
				if (!IsShowItemGroupList)
				{
					return ProbabilityDataList;
				}
				return ProbabilityItemGroupList;
			}
		}

		public bool IsExistItemGroupList => !__Extension_Method_ICollection__.IsNullOrEmpty(ProbabilityItemGroupList);
	}

	public const string PrefabPath = "Resources/Prefab2/Window/PayShop/PayShopProbabilityPopup";

	private static WindowPrefab<ProbabilityPopup> _windowPrefab;

	protected List<ProbabilityInfoComponent> _componentList = new List<ProbabilityInfoComponent>();

	public UILabel Label_Title;

	public UILabel Label_InfoCount;

	public UILabel LabelItem;

	public UILabel LabelPrice;

	public GameObject Button_Detail;

	public GameObject Button_Detail_Normal;

	public GameObject Button_Detail_Pressed;

	public UIScrollView RootScroll;

	public UIWidget RootWidget;

	public UIReuseScrollGrid ScrollGrid;

	public UIGrid ComponentGrid;

	public ProbabilityTabList TabList;

	private ProbabilityShopType _probabilityType;

	private List<ProbabilityInfoData> _probabilityInfoList;

	private int currentTab;

	private NewPayShopProductInfo _productInfo;

	private NewEquipGachaData _equipGachaData;

	private HighGachaCategoryType _highGachaCategory;

	private const string STAR_lOCAL_KEY = "UNIQUE_ITEM_STAR_COUNT";

	public static ProbabilityPopup instance => _windowPrefab.instance;

	public static bool IsValidInstance => _windowPrefab.IsOpen();

	private int tabCount => __Extension_Method_ICollection__.COUNT(_probabilityInfoList);

	public override void Reset()
	{
		base.Reset();
		ClearProbabilityData();
		RootWidget.ResetAndUpdateAnchors();
		ClearTab();
		InitLabel(isGachaSelectShopTab: false);
	}

	private void ClearProbabilityData()
	{
		ScrollGrid.ClearGridItem<ProbabilityInfoComponent>();
		Vespa_Generic_ICollection_Extension_Method.CLEAR(_probabilityInfoList);
		_probabilityInfoList = null;
	}

	void UIScrollItemBehaviour.IListener.ChangedItemIndex(Component comp, int index)
	{
		ProbabilityInfoComponent probabilityInfoComponent = comp as ProbabilityInfoComponent;
		if (!(probabilityInfoComponent == null))
		{
			ProbabilityBaseData probabilityBaseData = _probabilityInfoList[currentTab].CurrentShowProbabilityList[index];
			if (probabilityBaseData is GachaSelectShopProbabilityData)
			{
				probabilityInfoComponent.OpenGachaSelectShopTabInfo(probabilityBaseData);
			}
			else
			{
				probabilityInfoComponent.Open(probabilityBaseData, !_probabilityInfoList[currentTab].IsShowItemGroupList);
			}
		}
	}

	public static void Open(ProbabilityShopType probabilityType, NewPayShopProductInfo payShopProductInfo)
	{
		if (_windowPrefab == null)
		{
			_windowPrefab = new WindowPrefab<ProbabilityPopup>("Resources/Prefab2/Window/PayShop/PayShopProbabilityPopup", GameUIRoot.ELayerType.UI);
		}
		_windowPrefab.instance.OpenInternal(probabilityType, payShopProductInfo);
		GlobalAutoBehaviour<PopupManegement>.instance.Add(_windowPrefab);
	}

	private void OpenInternal(ProbabilityShopType probabilityType, NewPayShopProductInfo payShopProductInfo)
	{
		base.Open();
		_probabilityType = probabilityType;
		_productInfo = payShopProductInfo;
		if (__Extension_Method_ICollection__.IsNullOrEmpty(payShopProductInfo.ProbabilityIndices))
		{
			InitProduct();
		}
		else
		{
			InitProductByProbabilityIndices(payShopProductInfo.ProbabilityIndices);
		}
		InitTitle();
		InitInfoTitle();
		InitButton();
		InitTab();
	}

	public static void Open(ProbabilityShopType probabilityType, NewEquipGachaData equipGachaData, HighGachaCategoryType highGachaCategoryType)
	{
		if (_windowPrefab == null)
		{
			_windowPrefab = new WindowPrefab<ProbabilityPopup>("Resources/Prefab2/Window/PayShop/PayShopProbabilityPopup", GameUIRoot.ELayerType.UI);
		}
		_windowPrefab.instance.OpenInternal(probabilityType, equipGachaData, highGachaCategoryType);
		GlobalAutoBehaviour<PopupManegement>.instance.Add(_windowPrefab);
	}

	private void OpenInternal(ProbabilityShopType probabilityType, NewEquipGachaData equipGachaData, HighGachaCategoryType highGachaCategoryType)
	{
		base.Open();
		_probabilityType = probabilityType;
		_equipGachaData = equipGachaData;
		_highGachaCategory = highGachaCategoryType;
		UIHelper.SetLabelText(Label_Title, "");
		InitGacha();
	}

	public static void Open(ProbabilityShopType probabilityType, SelectShopInfo selectShopInfo)
	{
		if (_windowPrefab == null)
		{
			_windowPrefab = new WindowPrefab<ProbabilityPopup>("Resources/Prefab2/Window/PayShop/PayShopProbabilityPopup", GameUIRoot.ELayerType.UI);
		}
		_windowPrefab.instance.OpenInternal(probabilityType, selectShopInfo);
		GlobalAutoBehaviour<PopupManegement>.instance.Add(_windowPrefab);
	}

	private void OpenInternal(ProbabilityShopType probabilityType, SelectShopInfo selectShopInfo)
	{
		base.Open();
		_probabilityType = probabilityType;
		UIHelper.SetLabelText(Label_Title, "");
		InitGachaSelectShop(selectShopInfo);
	}

	private void Init()
	{
		_componentList.Clear();
		_probabilityInfoList = new List<ProbabilityInfoData>();
		ScrollGrid.ClearGridItem<ProbabilityInfoComponent>();
		currentTab = 0;
	}

	private void InitProductByProbabilityIndices(int[] mainIndices, string[] nameIndices = null)
	{
		Init();
		_probabilityInfoList = GetProbabilityDataList(_probabilityType, mainIndices, nameIndices);
		if (_probabilityInfoList[currentTab].IsExistItemGroupList)
		{
			_probabilityInfoList[currentTab].IsShowItemGroupList = true;
		}
		CreateProbabilityInfoList(_probabilityInfoList[currentTab].CurrentShowProbabilityList);
	}

	private void InitProduct()
	{
		if (_productInfo.ProductType == ProductType.Attendance)
		{
			return;
		}
		Init();
		if (_productInfo.Category == NewPayShopProductCategoryType.RightAway.ToString())
		{
			for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_productInfo.ItemInfos); i++)
			{
				PayShopProductItemInfo payShopProductItemInfo = _productInfo.ItemInfos[i];
				ItemData data = GameRoot.Instance.ItemDataContainer.GetData(payShopProductItemInfo.ItemIndex);
				if (data != null)
				{
					int key = GameRoot.Instance.PackageItemDataContainer.GetData(data.Code)?.RewardIndex1 ?? 0;
					RewardData data2 = GameRoot.Instance.RewardDataContainer.GetData(key);
					if (data2 != null)
					{
						string text = data.Name;
						List<RewardElementData> elementList = data2.GetElementList();
						List<ProbabilityBaseData> probabilityBaseDataList = GetProbabilityBaseDataList(elementList);
						_probabilityInfoList.Add(new ProbabilityInfoData
						{
							ProbabilityDataList = probabilityBaseDataList,
							Title = text
						});
					}
				}
			}
		}
		else
		{
			for (int j = 0; j < __Extension_Method_ICollection__.COUNT(_productInfo.UseItemCode); j++)
			{
				string key2 = _productInfo.UseItemCode[j];
				PackageItemData data3 = GameRoot.Instance.PackageItemDataContainer.GetData(key2);
				int key3 = data3?.RewardIndex1 ?? 0;
				RewardData data4 = GameRoot.Instance.RewardDataContainer.GetData(key3);
				if (data4 != null)
				{
					ItemData dataByCode = GameRoot.Instance.ItemDataContainer.GetDataByCode(data3.ItemCode);
					if (dataByCode != null)
					{
						string text2 = dataByCode.Name;
						List<RewardElementData> elementList2 = data4.GetElementList();
						List<ProbabilityBaseData> probabilityBaseDataList2 = GetProbabilityBaseDataList(elementList2);
						_probabilityInfoList.Add(new ProbabilityInfoData
						{
							ProbabilityDataList = probabilityBaseDataList2,
							Title = text2
						});
					}
				}
			}
		}
		if (_probabilityInfoList[currentTab].IsExistItemGroupList)
		{
			_probabilityInfoList[currentTab].IsShowItemGroupList = true;
		}
		CreateProbabilityInfoList(_probabilityInfoList[currentTab].CurrentShowProbabilityList);
	}

	private void InitGachaSelectShop(SelectShopInfo info)
	{
		if (info == null)
		{
			Close();
			return;
		}
		string categoryGroup = info.CategoryGroup;
		IList<GachaSelectShopData> equalCategoryList = GameRoot.Instance.GachaSelectShopDataContainer.GetEqualCategoryList(categoryGroup);
		if (__Extension_Method_ICollection__.IsNullOrEmpty(equalCategoryList))
		{
			Close();
			return;
		}
		Init();
		ProbabilityInfoData probabilityInfoData = new ProbabilityInfoData
		{
			Title = LocalizationManager.GetStringDirect("GACHASELECTSHOP_TAB_PRICERATE"),
			IsShowItemGroupList = false,
			IsGachaSelectShopTab = true
		};
		List<ProbabilityBaseData> list = new List<ProbabilityBaseData>(equalCategoryList.Count);
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(equalCategoryList); i++)
		{
			GachaSelectShopData gachaSelectShopData = equalCategoryList[i];
			if (gachaSelectShopData != null)
			{
				_ = gachaSelectShopData.Index;
				LocalizationManager.GetStringDirect((IAPWrapper.StoreType == IAPWrapper.EStoreType.AppStore) ? gachaSelectShopData.ItemGroupCodeNameKeyApple : gachaSelectShopData.ItemGroupCodeNameKeyOther);
				float num = (float)gachaSelectShopData.Rate * 0.001f * 100f;
				GachaSelectShopProbabilityData item = new GachaSelectShopProbabilityData
				{
					Index = gachaSelectShopData.Index,
					ComponentTitle = gachaSelectShopData.ItemGroupCodeNameKeyOther,
					ComponentRate = $"{num:N5}%"
				};
				list.Add(item);
			}
		}
		probabilityInfoData.ProbabilityDataList = list;
		_probabilityInfoList.Add(probabilityInfoData);
		for (int j = 0; j < __Extension_Method_ICollection__.COUNT(equalCategoryList); j++)
		{
			GachaSelectShopData gachaSelectShopData2 = equalCategoryList[j];
			if (gachaSelectShopData2 != null)
			{
				string itemGroupCodeNameKey = ((IAPWrapper.StoreType == IAPWrapper.EStoreType.AppStore) ? gachaSelectShopData2.ItemGroupCodeNameKeyApple : gachaSelectShopData2.ItemGroupCodeNameKeyOther);
				_probabilityInfoList.Add(GetProbabilityInfoData(gachaSelectShopData2.ItemGroupCode, itemGroupCodeNameKey));
			}
		}
		CreateProbabilityInfoList(_probabilityInfoList[currentTab].CurrentShowProbabilityList);
		InitTitle();
		InitInfoTitle();
		InitButton();
		InitTab();
		InitLabel(isGachaSelectShopTab: true);
	}

	private void InitLabel(bool isGachaSelectShopTab)
	{
		UIHelper.EnableGameObject(LabelItem, !isGachaSelectShopTab);
		UIHelper.EnableGameObject(LabelPrice, isGachaSelectShopTab);
	}

	private void InitGacha()
	{
		if (_equipGachaData.Type == EquipGachaType.Pickup)
		{
			InitPickupGacha();
			InitTitle();
			InitInfoTitle();
			InitButton();
			InitTab();
			return;
		}
		RequestGachaItemGroupCode(_equipGachaData.Index, _highGachaCategory, delegate(GachaItemGroupCodeData e)
		{
			InitNormalGacha(e);
			InitTitle();
			InitInfoTitle();
			InitButton();
			InitTab();
		});
	}

	private void InitPickupGacha()
	{
		GachaShopProbabilityData data = GameRoot.Instance.GachaShopProbabilityDataContainer.GetData(_equipGachaData.Index, HighGachaCategoryType.None);
		string[] gachaNames = data.GachaNames;
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(gachaNames); i++)
		{
			gachaNames[i] = LocalizationManager.GetStringDirect(gachaNames[i]);
		}
		InitProductByProbabilityIndices(data.ProbabilityIndices, gachaNames);
	}

	private void InitNormalGacha(GachaItemGroupCodeData gachaGroupData)
	{
		if (gachaGroupData != null)
		{
			Init();
			_probabilityInfoList.Add(GetProbabilityInfoData(gachaGroupData.ItemGroupCode, gachaGroupData.ItemGroupCodeNameKey));
			if (gachaGroupData.ItemGroupCode != gachaGroupData.BonusItemGroupCode)
			{
				_probabilityInfoList.Add(GetProbabilityInfoData(gachaGroupData.BonusItemGroupCode, gachaGroupData.BonusItemGroupCodeNameKey));
			}
			if (_probabilityInfoList[currentTab].IsExistItemGroupList)
			{
				_probabilityInfoList[currentTab].IsShowItemGroupList = true;
			}
			CreateProbabilityInfoList(_probabilityInfoList[currentTab].CurrentShowProbabilityList);
		}
	}

	private void InitTitle(string name = "")
	{
		if (String_Extension_Method.IsNullOrEmpty(name))
		{
			name = _probabilityInfoList[currentTab].Title;
		}
		UIHelper.SetLabelText(Label_Title, name);
	}

	private void InitInfoTitle()
	{
		if (!_probabilityInfoList[currentTab].IsGachaSelectShopTab)
		{
			UIHelper.EnableGameObject(Label_InfoCount, !_probabilityInfoList[currentTab].IsShowItemGroupList);
		}
		else
		{
			UIHelper.EnableGameObject(Label_InfoCount, enable: false);
		}
	}

	private void InitButton()
	{
		UIHelper.EnableGameObject(Button_Detail, _probabilityInfoList[currentTab].IsExistItemGroupList);
		UIHelper.EnableGameObject(Button_Detail_Normal, _probabilityInfoList[currentTab].IsShowItemGroupList);
		UIHelper.EnableGameObject(Button_Detail_Pressed, !_probabilityInfoList[currentTab].IsShowItemGroupList);
	}

	private ProbabilityInfoData GetProbabilityInfoData(string itemGroupCode, string itemGroupCodeNameKey)
	{
		ProbabilityInfoData probabilityInfoData = new ProbabilityInfoData();
		ItemGroupData data = GameRoot.Instance.ItemGroupDataContainer.GetData(itemGroupCode);
		List<RewardElementData> elementList = data.GetElementList();
		List<ProbabilityBaseData> probabilityBaseDataList = GetProbabilityBaseDataList(elementList, 1f, data.GetTotalRate());
		string stringDirect = LocalizationManager.GetStringDirect(itemGroupCodeNameKey);
		if (data.ShowDetail)
		{
			List<ProbabilityBaseData> probabilityItemGroupList = GetProbabilityItemGroupList(elementList, data.GetTotalRate());
			probabilityInfoData.ProbabilityItemGroupList = probabilityItemGroupList;
		}
		probabilityInfoData.ProbabilityDataList = probabilityBaseDataList;
		probabilityInfoData.Title = stringDirect;
		return probabilityInfoData;
	}

	private void RequestGachaItemGroupCode(int gachaIndex, HighGachaCategoryType highGachaCategoryType, Action<GachaItemGroupCodeData> successCallback)
	{
		NShared.GetGachaItemGroupCode.Request request = new NShared.GetGachaItemGroupCode.Request();
		request.GachaIndex = gachaIndex;
		request.HighGachaCategory = highGachaCategoryType;
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.GetGachaItemGroupCode.Response response)
		{
			if (response != null && response.BaseResult == BaseResultType.Success && response.Result == ResultType.Success)
			{
				GachaItemGroupCodeData i_argA = new GachaItemGroupCodeData
				{
					ItemGroupCode = response.ItemGroupCode,
					ItemGroupCodeNameKey = response.ItemGroupCodeNameKey,
					BonusItemGroupCode = response.BonusItemGroupCode,
					BonusItemGroupCodeNameKey = response.BonusItemGroupCodeNameKey
				};
				Vespa_System_Action_Extension_Method.fire(successCallback, i_argA);
			}
		});
	}

	private void CreateProbabilityInfoList(List<ProbabilityBaseData> probabilityDataList)
	{
		if (probabilityDataList != null)
		{
			ScrollGrid.CreateReuseItem(ProbabilityInfoComponent.PrefabAuto, _componentList, __Extension_Method_ICollection__.COUNT(probabilityDataList), this);
			ScrollGrid.RepositionGrid();
		}
	}

	private List<ProbabilityBaseData> GetProbabilityItemGroupList(List<RewardElementData> rewardElemDataList, float totalRate)
	{
		List<ProbabilityBaseData> list = new List<ProbabilityBaseData>();
		List<ProbabilityBaseData> list2 = new List<ProbabilityBaseData>();
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(rewardElemDataList); i++)
		{
			HashString itemCode = rewardElemDataList[i].ItemCode;
			ItemGroupData data = GameRoot.Instance.ItemGroupDataContainer.GetData(itemCode);
			if (data == null)
			{
				list2.Add(GetProbabilityBaseData(rewardElemDataList[i], 1f, totalRate));
				continue;
			}
			string stringDirect = LocalizationManager.GetStringDirect(data.ItemGroupNameKey);
			double num = System.Math.Truncate((float)rewardElemDataList[i].ItemRate / totalRate * 10000000f) * 1.0000000116860974E-07;
			string componentRate = $"{num * 100.0:N5}%";
			ProbabilityBaseData probabilityBaseData = new ProbabilityBaseData();
			probabilityBaseData.ComponentTitle = $"{stringDirect}";
			probabilityBaseData.ComponentCount = rewardElemDataList[i].ItemMax;
			probabilityBaseData.ComponentRate = componentRate;
			list.Add(probabilityBaseData);
		}
		if (!__Extension_Method_ICollection__.IsNullOrEmpty(list2))
		{
			list.AddRange(list2);
		}
		return list;
	}

	private List<ProbabilityInfoData> GetProbabilityDataList(ProbabilityShopType probabilityType, int[] mainIndices, string[] mainNames)
	{
		List<ProbabilityInfoData> list = new List<ProbabilityInfoData>();
		int num = 0;
		string text = "";
		foreach (int num2 in mainIndices)
		{
			if (_probabilityType == ProbabilityShopType.PayShop)
			{
				IList<ItemProbabilityData> probabilityInfoList = GameRoot.Instance.ItemProbabilityDataContainer.GetProbabilityInfoList(num2);
				if (probabilityInfoList == null)
				{
					continue;
				}
				ItemData data = GameRoot.Instance.ItemDataContainer.GetData(num2);
				if (data != null)
				{
					text = data.Name;
				}
				List<ProbabilityBaseData> probabilityDataList = probabilityInfoList.Select((ItemProbabilityData e) => new ProbabilityBaseData
				{
					ComponentTitle = e.ComponentTitle,
					ComponentCount = e.ComponentCount,
					ComponentRate = e.ComponentRate,
					Index = e.InformIndex
				}).ToList();
				list.Add(new ProbabilityInfoData
				{
					ProbabilityDataList = probabilityDataList,
					Title = text
				});
			}
			else if (_probabilityType == ProbabilityShopType.GachaShop)
			{
				IList<GachaProbabilityData> probabilityInfoList2 = GameRoot.Instance.GachaProbabilityDataContainer.GetProbabilityInfoList(num2);
				text = (Vespa_Generic_IList_Extension_Method.IsValidIndex(mainNames, num) ? mainNames[num] : "");
				if (probabilityInfoList2 == null)
				{
					continue;
				}
				List<ProbabilityBaseData> probabilityDataList2 = probabilityInfoList2.Select((GachaProbabilityData e) => new ProbabilityBaseData
				{
					ComponentTitle = e.ComponentTitle,
					ComponentCount = e.ComponentCount,
					ComponentRate = e.ComponentRate,
					Index = e.InformIndex
				}).ToList();
				list.Add(new ProbabilityInfoData
				{
					ProbabilityDataList = probabilityDataList2,
					Title = text
				});
			}
			num++;
		}
		return list;
	}

	private List<ProbabilityBaseData> GetProbabilityBaseDataList(List<RewardElementData> rewardElemDataList, float upperRate = 1f, float denominator = 1f)
	{
		List<ProbabilityBaseData> list = new List<ProbabilityBaseData>();
		foreach (RewardElementData rewardElemData in rewardElemDataList)
		{
			HashString itemCode = rewardElemData.ItemCode;
			ItemGroupData data = GameRoot.Instance.ItemGroupDataContainer.GetData(itemCode);
			if (data == null)
			{
				ProbabilityBaseData probabilityBaseData = GetProbabilityBaseData(rewardElemData, upperRate, denominator);
				list.Add(probabilityBaseData);
				continue;
			}
			int num = ((rewardElemData.ItemRate == 1000) ? 1 : rewardElemData.ItemRate);
			float upperRate2 = upperRate * ((float)num / denominator);
			List<RewardElementData> elementList = data.GetElementList();
			list.AddRange(GetProbabilityBaseDataList(elementList, upperRate2, data.GetTotalRate()));
		}
		return list;
	}

	private ProbabilityBaseData GetProbabilityBaseData(RewardElementData rewardElemData, float upperRate = 1f, float denominator = 1f)
	{
		CurrencyType currencyType = EnumTools.Parse(rewardElemData.ItemCode, CurrencyType.None);
		if (currencyType != CurrencyType.None)
		{
			string key = $"TOOLTIP_INFO_NAME_{currencyType.ToString().ToUpper()}";
			if (currencyType == CurrencyType.EventDungeonPoint || currencyType == CurrencyType.EventDungeonPoint2)
			{
				List<DropBonusEvent> allEffectiveEvent = GlobalAutoBehaviour<DropBonusEventManager>.instance.GetAllEffectiveEvent(EventBonusType.EventDungeon);
				int result = -1;
				if (allEffectiveEvent != null)
				{
					for (int i = 0; i < allEffectiveEvent.Count; i++)
					{
						DropBonusEvent dropBonusEvent = allEffectiveEvent[i];
						if (!String_Extension_Method.IsNullOrEmpty(dropBonusEvent.Condition))
						{
							int.TryParse(dropBonusEvent.Condition, out result);
							break;
						}
					}
				}
				if (result != -1)
				{
					CampaignChapterData data = GameRoot.Instance.CampaignChapterDataContainer.GetData(result);
					if (data != null)
					{
						string resultInfoIconSprite = data.GetResultInfoIconSprite(currencyType.ToString());
						if (!String_Extension_Method.IsNullOrEmpty(resultInfoIconSprite))
						{
							key = $"TOOLTIP_INFO_NAME_{resultInfoIconSprite.ToString().ToUpper()}";
						}
					}
				}
			}
			string stringDirect = LocalizationManager.GetStringDirect(key);
			int itemMax = rewardElemData.ItemMax;
			double num = System.Math.Truncate(upperRate * ((float)rewardElemData.ItemRate / denominator) * 10000000f) * 1.0000000116860974E-07;
			string componentRate = $"{num * 100.0:N5}%";
			return new ProbabilityBaseData
			{
				ComponentTitle = $"{stringDirect}",
				ComponentCount = itemMax,
				ComponentRate = componentRate
			};
		}
		ItemData dataByCode = GameRoot.Instance.ItemDataContainer.GetDataByCode(rewardElemData.ItemCode);
		if (dataByCode != null)
		{
			string stringDirect2 = LocalizationManager.GetStringDirect(dataByCode.Name);
			string arg = ((rewardElemData.ItemStar == 0) ? "" : LocalizationManager.GetStringDirect("UNIQUE_ITEM_STAR_COUNT", rewardElemData.ItemStar));
			int itemMax2 = rewardElemData.ItemMax;
			double num2 = System.Math.Truncate(upperRate * ((float)rewardElemData.ItemRate / denominator) * 10000000f) * 1.0000000116860974E-07;
			string componentRate2 = $"{num2 * 100.0:N5}%";
			return new ProbabilityBaseData
			{
				Index = dataByCode.Index,
				ComponentTitle = $"{stringDirect2} {arg}",
				ComponentCount = itemMax2,
				ComponentRate = componentRate2
			};
		}
		return null;
	}

	void ProbabilityTabComponent.IListener.OnClickTab(int index)
	{
		if (tabCount >= index)
		{
			currentTab = index;
			Vespa_Generic_ICollection_Extension_Method.CLEAR(_componentList);
			ScrollGrid.ClearGridItem<ProbabilityInfoComponent>();
			_probabilityInfoList[currentTab].IsShowItemGroupList = _probabilityInfoList[currentTab].IsExistItemGroupList;
			CreateProbabilityInfoList(_probabilityInfoList[currentTab].CurrentShowProbabilityList);
			InitTitle();
			InitInfoTitle();
			InitButton();
			InitLabel(_probabilityInfoList[currentTab].IsGachaSelectShopTab);
		}
	}

	private void InitTab()
	{
		if (!__Extension_Method_ICollection__.IsNullOrEmpty(_probabilityInfoList))
		{
			TabList.Open(ref _probabilityInfoList, this);
			TabList.CreateTabList();
		}
	}

	private void ClearTab()
	{
		TabList.ClearTab();
	}

	public void ResetScroll()
	{
		if (RootScroll != null)
		{
			SingletonUpdater<Executer>.instance.NextRun(delegate
			{
				RootScroll.ResetPosition();
			});
		}
	}

	public void OnClickClose()
	{
		Close();
	}

	public void OnClickShowDetail()
	{
		if (_probabilityInfoList[currentTab].IsExistItemGroupList)
		{
			Vespa_Generic_ICollection_Extension_Method.CLEAR(_componentList);
			ScrollGrid.ClearGridItem<ProbabilityInfoComponent>();
			_probabilityInfoList[currentTab].IsShowItemGroupList = !_probabilityInfoList[currentTab].IsShowItemGroupList;
			CreateProbabilityInfoList(_probabilityInfoList[currentTab].CurrentShowProbabilityList);
			UIHelper.EnableGameObject(Button_Detail_Normal, _probabilityInfoList[currentTab].IsShowItemGroupList);
			UIHelper.EnableGameObject(Button_Detail_Pressed, !_probabilityInfoList[currentTab].IsShowItemGroupList);
			InitTitle();
			InitInfoTitle();
		}
	}
}
