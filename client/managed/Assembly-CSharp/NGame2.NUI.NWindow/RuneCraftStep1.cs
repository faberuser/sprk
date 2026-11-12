using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NEventTrigger;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NComponent2.NItemIconSub;
using NGame2.NUI.NHelper.NItem;
using NNguiExtension;
using NShared;
using NShared.NEvent;
using NVespa;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class RuneCraftStep1 : RuneCraftStepBase, ItemIconComponent.Listener, RuneItemDetailComponent.Listener, PopSubComponent.Listener
{
	[Header("------------------------------------------")]
	[Header("[RuneCraftStep1]")]
	public Transform RightUIRoot;

	public Transform MaterialItemRoot1;

	public Transform MaterialItemRoot2;

	public UILabel LabelRuneFragmentCount;

	public UILabel LabelMaterialPercentage;

	public UILabel Label_NotHaveRune;

	public UILabel Label_SelectedFilter;

	public UIButton ButtonNextStep;

	public UIScrollGrid Grid;

	public UIScrollGrid GridDetail;

	public UISimpleGrid GridFilter;

	public UISpriteSelector DetailModeIconSelector;

	[Header("[Effect]")]
	public MeshRenderer GaugeLeft;

	public MeshRenderer GaugeRight;

	public UIGame3DWidget EffectHitLeft;

	public UIGame3DWidget EffectHitRight;

	public UIGame3DWidget EffectFullGaugeLeft;

	public UIGame3DWidget EffectFullGaugeRight;

	public UIGame3DWidget EffectFullLoopLeft;

	public UIGame3DWidget EffectFullLoopRight;

	public UIPlayAssetSound GaugeSound;

	public GameObject LCircle;

	public GameObject RCircle;

	private int _curMaterial2Index;

	private int _maxGauge;

	private int _materialPercentage;

	private bool _isDetailMode;

	private bool _isEnoughFragmentCount;

	private bool _expandFilter;

	private ItemIconComponent _materialItemIconComp1;

	private ItemIconComponent _materialItemIconComp2;

	private Dictionary<int, Tuple<ItemIconComponent, RuneItemDetailComponent>> _dicItemIconComp = new Dictionary<int, Tuple<ItemIconComponent, RuneItemDetailComponent>>();

	private List<int> _listMaterialIndex = new List<int>();

	private Dictionary<string, int> _dicTag = new Dictionary<string, int>();

	public override void Init()
	{
		base.Init();
		UIHelper.EnableGameObject(OwnerWindow.ButtonHelp, enable: true);
		UIHelper.EnableGameObject(OwnerWindow.LabelPunishmentRuneCount, enable: true);
		UIHelper.EnableGameObject(RightUIRoot, enable: true);
		UIHelper.EnableGameObject(LCircle, enable: true);
		UIHelper.EnableGameObject(RCircle, enable: true);
		UIHelper.EnableGameObject(ButtonNextStep, enable: true);
		_isDetailMode = false;
		_isEnoughFragmentCount = false;
		_expandFilter = false;
		UIHelper.EnableGameObject(GridFilter, _expandFilter);
		_maxGauge = GameRoot.Instance.ConstantDataContainer.GetValueInt("PunishmentRuneMaxGauge");
		UIHelper.SetLabelText(Label_SelectedFilter, LocalizationManager.GetStringDirect("RUNE_CRAFT_SORT_ALL"));
		SetGaugeLeft(0f);
		SetGaugeRight(0f);
		SetupMaterialValues();
		CreateRuneFragment();
		CreateOwnedRuneItemIconComponent();
		UpdateDetailViewMode();
		UpdateMaterialPercentage(isAdded: true, null);
		UpdateButtonNextStep();
		SingletonAuto<EventTriggerManager>.instance.RaiseEvent(new OpenPunishmentRuneCraft());
	}

	private void SetupMaterialValues()
	{
		string[] valueArray = GameRoot.Instance.ConstantDataContainer.GetValueArray<string>("PunishmentRuneIngredientRuneTags");
		int[] valueArray2 = GameRoot.Instance.ConstantDataContainer.GetValueArray<int>("PunishmentRuneIngredientRuneGaugePerRune");
		if (valueArray.Length != valueArray2.Length)
		{
			VespaDebug.LogError("Not equal length : " + valueArray.Length + " and " + valueArray2.Length);
		}
		for (int i = 0; i < valueArray.Length; i++)
		{
			_dicTag.Add(valueArray[i], valueArray2[i]);
		}
	}

	private void UpdateDetailViewMode()
	{
		UIHelper.EnableGameObject(Grid, !_isDetailMode);
		UIHelper.EnableGameObject(GridDetail, _isDetailMode);
		UIHelper.ResetGridPosition(Grid);
		UIHelper.ResetGridPosition(GridDetail);
		DetailModeIconSelector.Select(_isDetailMode ? "Detail" : "Normal");
	}

	private void CreateRuneFragment()
	{
		int valueInt = GameRoot.Instance.ConstantDataContainer.GetValueInt("RuneFragmentItemIndex");
		ItemData data = GameRoot.Instance.ItemDataContainer.GetData(valueInt);
		_materialItemIconComp1 = ItemIconComponent.PrefabAuto.Factory.Create(MaterialItemRoot1);
		Vespa_Unity_Transform_Extension_Method.ResetLocalTM(_materialItemIconComp1.transform);
		_materialItemIconComp1.Open(data, this);
		EquipItemInfo[] equipItemsByItemType = GlobalAutoBehaviour<AccountManager>.instance.EquipItemManager.GetEquipItemsByItemType(ItemType.PunishmentRune);
		int key = ((!__Extension_Method_ICollection__.IsNullOrEmpty(equipItemsByItemType)) ? equipItemsByItemType.Length : 0);
		OwnerWindow.PunishmentRuneItemCountData = GameRoot.Instance.PunishmentRuneItemCountDataContainer.GetData(key);
		if (OwnerWindow.PunishmentRuneItemCountData != null)
		{
			int itemCount = GlobalAutoBehaviour<AccountManager>.instance.ItemManager.GetItemCount(data.Code);
			int runeFragmentCount = OwnerWindow.PunishmentRuneItemCountData.RuneFragmentCount;
			_isEnoughFragmentCount = runeFragmentCount <= itemCount;
			Color color = (_isEnoughFragmentCount ? OwnerWindow.ColorGoldEnough : OwnerWindow.ColorGoldNotEnough);
			UIHelper.SetLabelColor(LabelRuneFragmentCount, color);
			UIHelper.SetLabelText(LabelRuneFragmentCount, $"{itemCount} / {runeFragmentCount}");
			SetGaugeLeft(Mathf.Clamp01(itemCount / runeFragmentCount));
			if (_isEnoughFragmentCount)
			{
				EffectFullLoopLeft.Play(null);
			}
		}
	}

	private void ReleaseMaterialItemIconComponent()
	{
		FactoryBase.ReleaseComponent(_materialItemIconComp2);
		_materialItemIconComp2 = null;
	}

	private void CreateMaterialItemIconComponent(ItemData itemData)
	{
		_materialItemIconComp2 = ItemIconComponent.PrefabAuto.Factory.Create(MaterialItemRoot2);
		Vespa_Unity_Transform_Extension_Method.ResetLocalTM(_materialItemIconComp2.transform);
		_materialItemIconComp2.Open(itemData, this);
	}

	private void CreateOwnedRuneItemIconComponent()
	{
		GameRoot.Instance.ConstantDataContainer.GetValueArray<string>("PunishmentRuneIngredientRuneTags");
		foreach (ItemInfo item in GlobalAutoBehaviour<AccountManager>.instance.ItemManager.GetItemsByType(ItemType.Rune))
		{
			ItemData data = GameRoot.Instance.ItemDataContainer.GetData(item.ItemIndex);
			RuneItemData subData = data.GetSubData<RuneItemData>();
			if (CheckIntersectionTag(subData.Tag))
			{
				ItemIconComponent itemIconComponent = UIHelper.AddGridItem(Grid, ItemIconComponent.PrefabAuto);
				Vespa_Unity_Transform_Extension_Method.ResetLocalTM(itemIconComponent.transform);
				itemIconComponent.Open(data, this);
				int count = item.Count;
				Color? color = null;
				int num = count;
				itemIconComponent.SetItemCount(num, color);
				UIHelper.EnableGameObject(itemIconComponent, enable: true);
				RuneItemDetailComponent runeItemDetailComponent = UIHelper.AddGridItem(GridDetail, RuneItemDetailComponent.PrefabAuto);
				runeItemDetailComponent.Open(item, data, this);
				UIHelper.EnableGameObject(runeItemDetailComponent, enable: true);
				_dicItemIconComp.Add(item.ItemIndex, new Tuple<ItemIconComponent, RuneItemDetailComponent>(itemIconComponent, runeItemDetailComponent));
			}
		}
		Grid.Grid.sorting = UIGrid.Sorting.Custom;
		Grid.Grid.onCustomSort = delegate(Transform x, Transform y)
		{
			ItemIconComponent component = x.GetComponent<ItemIconComponent>();
			ItemIconComponent component2 = y.GetComponent<ItemIconComponent>();
			return ItemComparer.Sort(component, component2);
		};
		GridDetail.Grid.sorting = UIGrid.Sorting.Custom;
		GridDetail.Grid.onCustomSort = delegate(Transform x, Transform y)
		{
			RuneItemDetailComponent component = x.GetComponent<RuneItemDetailComponent>();
			return ItemComparer.Sort(comp2: y.GetComponent<RuneItemDetailComponent>().ItemIconComponent, comp1: component.ItemIconComponent);
		};
		UIHelper.RepositionGrid(Grid);
		UIHelper.RepositionGrid(GridDetail);
		UIHelper.EnableGameObject(Label_NotHaveRune, __Extension_Method_ICollection__.IsNullOrEmpty(_dicItemIconComp));
	}

	private bool CheckIntersectionTag(string[] targetTags)
	{
		if (__Extension_Method_ICollection__.IsNullOrEmpty(_dicTag) || __Extension_Method_ICollection__.IsNullOrEmpty(targetTags))
		{
			return false;
		}
		foreach (KeyValuePair<string, int> item in _dicTag)
		{
			string key = item.Key;
			string i_other = targetTags[0];
			if (Vespa_Common_Class_Extension_Method.EQUALS(key, i_other))
			{
				return true;
			}
		}
		return false;
	}

	private void UpdateMaterialPercentage(bool isAdded, RuneItemData runeItemData)
	{
		if (runeItemData == null)
		{
			_materialPercentage = 0;
		}
		else
		{
			if (!_dicTag.TryGetValue(runeItemData.Tag[0], out var value))
			{
				return;
			}
			if (isAdded)
			{
				_materialPercentage += value;
				_materialPercentage = Mathf.Min(_maxGauge, _materialPercentage);
			}
			else
			{
				_materialPercentage -= value;
				_materialPercentage = Mathf.Max(0, _materialPercentage);
			}
		}
		UIHelper.SetLabelText(LabelMaterialPercentage, (float)_materialPercentage * 0.1f + "%");
		SetGaugeRight((float)_materialPercentage * 0.001f);
		if (_materialPercentage >= 1000)
		{
			EffectFullGaugeRight.Play(null);
			EffectFullLoopRight.Play(null);
		}
	}

	private void SetGaugeLeft(float value)
	{
		if (!(GaugeLeft == null))
		{
			if (value >= 0.9999f)
			{
				value = 1.1f;
			}
			GaugeLeft.material.SetFloat(_gaugePropertyName, value);
		}
	}

	private void SetGaugeRight(float value)
	{
		if (!(GaugeRight == null))
		{
			if (value >= 0.9999f)
			{
				value = 1.1f;
			}
			GaugeRight.material.SetFloat(_gaugePropertyName, value);
		}
	}

	void ItemIconComponent.Listener.OnClick(ItemIconComponent itemIconComponent)
	{
		if (!Vespa_Common_Class_Extension_Method.EQUALS(_materialItemIconComp1, itemIconComponent) && !Vespa_Common_Class_Extension_Method.EQUALS(_materialItemIconComp2, itemIconComponent))
		{
			AddMaterialItemComponent(itemIconComponent);
		}
	}

	void PopSubComponent.Listener.OnClickPop(ItemIconComponent itemIconComponent)
	{
		RemoveMaterialItemComponent(itemIconComponent);
	}

	void RuneItemDetailComponent.Listener.OnClick(ItemIconComponent component)
	{
		AddMaterialItemComponent(component);
	}

	private void AddMaterialItemComponent(ItemIconComponent selectedComp)
	{
		if (_materialPercentage >= _maxGauge)
		{
			return;
		}
		int itemIndex = selectedComp.ItemIndex;
		ItemData itemData = selectedComp.ItemData;
		if (GlobalAutoBehaviour<AccountManager>.instance.ItemManager.Get(itemData.Index).Locked != 0)
		{
			return;
		}
		if (!OwnerWindow.DicMaterialCount.ContainsKey(itemIndex))
		{
			OwnerWindow.DicMaterialCount.Add(itemIndex, 0);
		}
		int num = OwnerWindow.DicMaterialCount[itemIndex];
		long itemCount = selectedComp.ItemCount;
		int num2 = num;
		if (num2 < itemCount)
		{
			int count = ++OwnerWindow.DicMaterialCount[itemIndex];
			_listMaterialIndex.Add(itemIndex);
			if (_curMaterial2Index != itemIndex)
			{
				ReleaseMaterialItemIconComponent();
				CreateMaterialItemIconComponent(itemData);
				_curMaterial2Index = itemIndex;
			}
			GaugeSound.Play();
			UpdatePopSubComponent(itemIndex, count, this);
			UpdateMaterialPercentage(isAdded: true, selectedComp.RuneItemData);
			UpdateButtonNextStep();
			EffectHitRight.Play(null);
		}
	}

	private void UpdatePopSubComponent(int itemIndex, int count, PopSubComponent.Listener listener = null)
	{
		if (_dicItemIconComp.ContainsKey(itemIndex))
		{
			_dicItemIconComp[itemIndex].Item1.SetPopSubCompnent(count, listener);
			_dicItemIconComp[itemIndex].Item2.SetPopSubCompnent(count, listener);
		}
	}

	private void RemoveMaterialItemComponent(ItemIconComponent selectedMaterialComp)
	{
		int itemIndex = selectedMaterialComp.ItemIndex;
		if (!OwnerWindow.DicMaterialCount.ContainsKey(itemIndex))
		{
			return;
		}
		int count = --OwnerWindow.DicMaterialCount[itemIndex];
		_listMaterialIndex.RemoveAt(_listMaterialIndex.LastIndexOf(itemIndex));
		int last = Vespa_Generic_IList_Extension_Method.GetLast(_listMaterialIndex);
		if (_curMaterial2Index != last)
		{
			_curMaterial2Index = last;
			ReleaseMaterialItemIconComponent();
			if (_curMaterial2Index != 0)
			{
				ItemData data = GameRoot.Instance.ItemDataContainer.GetData(_curMaterial2Index);
				CreateMaterialItemIconComponent(data);
			}
		}
		UpdatePopSubComponent(itemIndex, count, this);
		UpdateMaterialPercentage(isAdded: false, selectedMaterialComp.RuneItemData);
		UpdateButtonNextStep();
	}

	public override void Close()
	{
		base.Close();
		UIHelper.SetLabelText(LabelRuneFragmentCount, string.Empty);
		UIHelper.SetLabelText(LabelMaterialPercentage, string.Empty);
		UIHelper.SetLabelText(Label_NotHaveRune, string.Empty);
		UIHelper.SetLabelText(Label_SelectedFilter, string.Empty);
		foreach (KeyValuePair<int, Tuple<ItemIconComponent, RuneItemDetailComponent>> item in _dicItemIconComp)
		{
			FactoryBase.ReleaseComponent(item.Value.Item1);
			item.Value.Item2.Close();
			FactoryBase.ReleaseComponent(item.Value.Item2);
		}
		_dicItemIconComp.Clear();
		FactoryBase.ReleaseComponent(_materialItemIconComp1);
		_materialItemIconComp1 = null;
		ReleaseMaterialItemIconComponent();
		_listMaterialIndex.Clear();
		_dicTag.Clear();
		_curMaterial2Index = 0;
		_materialPercentage = 0;
		_isDetailMode = false;
		UIHelper.EnableGameObject(RightUIRoot, enable: false);
		SetGaugeLeft(0f);
		SetGaugeRight(0f);
		EffectHitLeft.Stop(immediatelyClear: true);
		EffectHitRight.Stop(immediatelyClear: true);
		EffectFullLoopLeft.Stop(immediatelyClear: true);
		EffectFullLoopRight.Stop(immediatelyClear: true);
		EffectFullGaugeLeft.Stop(immediatelyClear: true);
		EffectFullGaugeRight.Stop(immediatelyClear: true);
		UIHelper.EnableGameObject(LCircle, enable: false);
		UIHelper.EnableGameObject(RCircle, enable: false);
	}

	public void OnClickChangeViewMode()
	{
		_isDetailMode = !_isDetailMode;
		UpdateDetailViewMode();
	}

	public void OnClickFiler()
	{
		_expandFilter = !_expandFilter;
		UIHelper.EnableGameObject(GridFilter, _expandFilter);
	}

	public void OnClickFilterComponent(UILabel labelName, UILabel labelTag)
	{
		string text = string.Empty;
		if (labelTag != null)
		{
			text = labelTag.text;
		}
		UpdateFilterComponents(text);
		UIHelper.SetLabelText(Label_SelectedFilter, labelName.text);
		_expandFilter = !_expandFilter;
		UIHelper.EnableGameObject(GridFilter, _expandFilter);
	}

	private void UpdateButtonNextStep()
	{
		UIHelper.EnableButton(ButtonNextStep, _isEnoughFragmentCount && _materialPercentage >= _maxGauge);
	}

	private void UpdateFilterComponents(string tag)
	{
		foreach (KeyValuePair<int, Tuple<ItemIconComponent, RuneItemDetailComponent>> item3 in _dicItemIconComp)
		{
			ItemIconComponent item = item3.Value.Item1;
			RuneItemDetailComponent item2 = item3.Value.Item2;
			RuneItemData subData = item.ItemData.GetSubData<RuneItemData>();
			bool enable = String_Extension_Method.IsNullOrEmpty(tag) || ((!__Extension_Method_ICollection__.IsNullOrEmpty(subData.Tag) && Vespa_Common_Class_Extension_Method.EQUALS(tag, subData.Tag[0])) ? true : false);
			UIHelper.EnableGameObject(item, enable);
			UIHelper.EnableGameObject(item2, enable);
		}
		UIHelper.RepositionGrid(Grid);
		UIHelper.RepositionGrid(GridDetail);
	}

	public void OnClickNextStep()
	{
		UIHelper.EnableGameObject(ButtonNextStep, enable: false);
		if (_isEnoughFragmentCount && _materialPercentage >= _maxGauge)
		{
			OwnerWindow.CachedMaterialRuneIndices = _listMaterialIndex.ToArray();
			OwnerWindow.UpdateStep();
		}
	}
}
