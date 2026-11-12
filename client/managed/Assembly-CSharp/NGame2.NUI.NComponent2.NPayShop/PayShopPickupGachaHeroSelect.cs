using System;
using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NGame2.NAccount;
using NGame2.NUI.NWindow.NPayShop;
using NNguiExtension;
using NShared;
using NShared.NLocalization;
using NVespa;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NComponent2.NPayShop;

public class PayShopPickupGachaHeroSelect : MonoBehaviour, EquipGachaHeroIconComponent.IListener, PayShopEquipGachaHeroClassList.IListener
{
	public interface IListener
	{
		void OnFinishPickUp(int[] heroIndices);
	}

	private enum EAutoSelect
	{
		IDLE,
		RUN
	}

	public PayShopEquipGachaHeroClassList PayShopEquipGachaHeroClassList;

	public UILabel LabelTitle;

	public UILabelSelector CategoryLabelSelector;

	[Header("[Hero List Group]")]
	public UIScrollView HeroScrollView;

	public Transform OwnedHeroRoot;

	public Transform UnownedHeroRoot;

	public UIGrid OwnedHeroGrid;

	public UIGrid UnownedHeroGrid;

	public UILabel LabelHeroSelect;

	[Header("[Selected Hero Group]")]
	public UIScrollGrid HeroIconGrid;

	[Header("[Button Group]")]
	public UIButton Button_Random;

	public UIButton Button_Select;

	[Header("[ETC]")]
	public float GridSpace;

	public float RandomPickAnimationInterval = 0.2f;

	public float RandomPickAnimationPreDelay = 0.2f;

	private List<EquipGachaHeroIconComponent> _ownedHeroList = new List<EquipGachaHeroIconComponent>();

	private List<EquipGachaHeroIconComponent> _unownedHeroList = new List<EquipGachaHeroIconComponent>();

	private List<EquipGachaHeroIconComponent> _selectedHeroIconComponentList = new List<EquipGachaHeroIconComponent>();

	private List<int> _selectedHeroIndices = new List<int>();

	private LocalAccountString _storageSelectedHeroIndicesJson = new LocalAccountString(typeof(PayShopPickupGachaHeroSelect).FullName + "SelectedHeroIndices");

	private LocalAccountString _storageCachedTime = new LocalAccountString(typeof(PayShopPickupGachaHeroSelect).FullName + "CachedDate");

	private EAutoSelect _autoSelectState;

	private HighGachaCategoryType _highGachaCategory = HighGachaCategoryType.All;

	private IListener _listener;

	public NewEquipGachaData GachaData { get; private set; }

	public void Open(NewEquipGachaData data, IListener listener)
	{
		GachaData = data;
		_listener = listener;
		Init(data);
	}

	protected void Init(NewEquipGachaData data)
	{
		string gachaCategorySelectTitle = data.GachaCategorySelectTitle;
		if (String_Extension_Method.IsNullOrEmpty(gachaCategorySelectTitle))
		{
			gachaCategorySelectTitle = data.Name;
		}
		string stringDirect = LocalizationManager.GetStringDirect(gachaCategorySelectTitle);
		UIHelper.SetLabelText(LabelTitle, stringDirect);
		string text = LocalizationManager.GetString(Gacha.REMAIN_SELECT_HERO_MESSAGE, GachaData.PickupHeroCount);
		UIHelper.SetLabelText(LabelHeroSelect, text);
		PayShopEquipGachaHeroClassList.Open(this);
		PayShopEquipGachaHeroClassList.Select(HighGachaCategoryType.All);
		SetCategory(HighGachaCategoryType.All);
		PreSelectHero();
		UpdateButtonState();
	}

	public void Clear()
	{
		_highGachaCategory = HighGachaCategoryType.None;
		ReleaseAndClearHeroListComponents();
		ClearSelectedHero();
		PayShopEquipGachaHeroClassList.Clear();
		VespaCoroutine.StopAll(this);
		_autoSelectState = EAutoSelect.IDLE;
	}

	protected void SetCategory(HighGachaCategoryType category)
	{
		if (_highGachaCategory == category || GachaData == null)
		{
			return;
		}
		_highGachaCategory = category;
		ReleaseAndClearHeroListComponents();
		if (CategoryLabelSelector != null)
		{
			CategoryLabelSelector.SelectEnum(category);
		}
		List<int> list = new List<int>();
		if (category == HighGachaCategoryType.All)
		{
			list.AddRange(GachaData.PickupHeroIndices);
		}
		else
		{
			EquipGachaClientData data = GameRoot.Instance.EquipGachaClientDataContainer.GetData(GachaData.GachaCategoryGroupIndex, category);
			if (data != null)
			{
				for (int i = 0; i < __Extension_Method_ICollection__.COUNT(data.UniqueWeaponHeroIndex); i++)
				{
					int num = data.UniqueWeaponHeroIndex[i];
					if (TypeIterator.Contains(GachaData.PickupHeroIndices, num))
					{
						list.Add(num);
					}
				}
			}
		}
		foreach (int item in list)
		{
			bool flag = GlobalAutoBehaviour<AccountManager>.instance.GetHeroInfo(item) != null;
			EquipGachaHeroIconComponent equipGachaHeroIconComponent = CreateGachaHeroIcon(item, flag ? OwnedHeroGrid : UnownedHeroGrid, this);
			if (!(equipGachaHeroIconComponent == null))
			{
				if (flag)
				{
					_ownedHeroList.Add(equipGachaHeroIconComponent);
				}
				else
				{
					_unownedHeroList.Add(equipGachaHeroIconComponent);
				}
				if (_selectedHeroIndices.Contains(item))
				{
					equipGachaHeroIconComponent.Select(isSelect: true);
				}
			}
		}
		ResetAllGrid();
	}

	public EquipGachaHeroIconComponent CreateGachaHeroIcon(int heroIndex, UIGrid parent, EquipGachaHeroIconComponent.IListener listener = null)
	{
		CreatureData data = GameRoot.Instance.CreatureDataContainer.GetData(heroIndex);
		if (data == null)
		{
			return null;
		}
		EquipGachaHeroIconComponent equipGachaHeroIconComponent = EquipGachaHeroIconComponent.PrefabAuto.Factory.Create(parent.transform);
		if (equipGachaHeroIconComponent == null)
		{
			return null;
		}
		equipGachaHeroIconComponent.Open(data, listener);
		return equipGachaHeroIconComponent;
	}

	private void UpdateButtonState()
	{
		UIHelper.EnableButton(Button_Random, __Extension_Method_ICollection__.COUNT(_selectedHeroIconComponentList) < GachaData.PickupHeroCount, grayscaleIfDisable: true);
		UIHelper.EnableButton(Button_Select, __Extension_Method_ICollection__.COUNT(_selectedHeroIconComponentList) == GachaData.PickupHeroCount, grayscaleIfDisable: true);
	}

	private void ReleaseAndClearHeroListComponents()
	{
		FactoryBase.ReleaseAndClearList(_ownedHeroList);
		FactoryBase.ReleaseAndClearList(_unownedHeroList);
	}

	private void ResetAllGrid()
	{
		UIHelper.EnableGameObject(OwnedHeroRoot, !__Extension_Method_ICollection__.IsNullOrEmpty(_ownedHeroList));
		UIHelper.EnableGameObject(UnownedHeroRoot, !__Extension_Method_ICollection__.IsNullOrEmpty(_unownedHeroList));
		OwnedHeroGrid.Reposition();
		UnownedHeroGrid.Reposition();
		if (!__Extension_Method_ICollection__.IsNullOrEmpty(_ownedHeroList))
		{
			int gridChildCount = OwnedHeroGrid.GetGridChildCount();
			int maxPerLine = OwnedHeroGrid.maxPerLine;
			float num = Mathf.Ceil((float)gridChildCount / (float)maxPerLine) * OwnedHeroGrid.cellHeight + GridSpace;
			UnownedHeroRoot.localPosition = Vector2.down * num;
		}
		else
		{
			UnownedHeroRoot.localPosition = Vector2.zero;
		}
		HeroScrollView.ResetPosition();
	}

	private void SelectHeroIcon(int heroIndex)
	{
		if (__Extension_Method_ICollection__.COUNT(_selectedHeroIconComponentList) != GachaData.PickupHeroCount)
		{
			EquipGachaHeroIconComponent item = CreateGachaHeroIcon(heroIndex, HeroIconGrid.Grid, this);
			if (!_selectedHeroIconComponentList.Contains(item))
			{
				_selectedHeroIconComponentList.Add(item);
			}
			if (!_selectedHeroIndices.Contains(heroIndex))
			{
				_selectedHeroIndices.Add(heroIndex);
			}
			EquipGachaHeroIconComponent equipGachaHeroIconComponent = FindHeroListComponent(heroIndex);
			if (equipGachaHeroIconComponent != null)
			{
				equipGachaHeroIconComponent.Select(isSelect: true);
			}
		}
	}

	private void UnselectHeroIcon(int heroIndex)
	{
		if (_selectedHeroIndices.Contains(heroIndex))
		{
			_selectedHeroIndices.Remove(heroIndex);
		}
		EquipGachaHeroIconComponent comp = _selectedHeroIconComponentList.Find((EquipGachaHeroIconComponent icon) => icon.HeroIndex == heroIndex);
		if (comp != null)
		{
			_selectedHeroIconComponentList.Remove(comp);
			FactoryBase.SafeReleaseComponent(ref comp);
		}
		EquipGachaHeroIconComponent equipGachaHeroIconComponent = FindHeroListComponent(heroIndex);
		if (equipGachaHeroIconComponent != null)
		{
			equipGachaHeroIconComponent.Select(isSelect: false);
		}
	}

	private EquipGachaHeroIconComponent FindHeroListComponent(int heroIndex)
	{
		EquipGachaHeroIconComponent equipGachaHeroIconComponent = null;
		equipGachaHeroIconComponent = _ownedHeroList.Find((EquipGachaHeroIconComponent x) => x.HeroIndex == heroIndex);
		if (equipGachaHeroIconComponent != null)
		{
			return equipGachaHeroIconComponent;
		}
		equipGachaHeroIconComponent = _unownedHeroList.Find((EquipGachaHeroIconComponent x) => x.HeroIndex == heroIndex);
		_ = equipGachaHeroIconComponent != null;
		return equipGachaHeroIconComponent;
	}

	private void ClearSelectedHero()
	{
		foreach (int selectedHeroIndex in _selectedHeroIndices)
		{
			EquipGachaHeroIconComponent equipGachaHeroIconComponent = FindHeroListComponent(selectedHeroIndex);
			if (equipGachaHeroIconComponent != null)
			{
				equipGachaHeroIconComponent.Select(isSelect: false);
			}
		}
		FactoryBase.ReleaseAndClearList(_selectedHeroIconComponentList);
		Vespa_Generic_ICollection_Extension_Method.CLEAR(_selectedHeroIndices);
	}

	private void PreSelectHero()
	{
		string value = _storageSelectedHeroIndicesJson.Value;
		string value2 = _storageCachedTime.Value;
		if (String_Extension_Method.IsNullOrEmpty(value) || String_Extension_Method.IsNullOrEmpty(value2) || !Vespa_String_Extension_Method2.TryParseNetworkTime2(value2, out var o_dateTime))
		{
			return;
		}
		DateTime minValue = DateTime.MinValue;
		DateTime maxValue = DateTime.MaxValue;
		if (o_dateTime < minValue || maxValue < o_dateTime)
		{
			return;
		}
		int[] array = ParseHeroIndices(value);
		if (__Extension_Method_ICollection__.COUNT(array) <= 0)
		{
			return;
		}
		int[] array2 = array;
		foreach (int num in array2)
		{
			if (GachaData == null || __Extension_Method_ICollection__.COUNT(GachaData.PickupHeroIndices) <= 0 || TypeIterator.Contains(GachaData.PickupHeroIndices, num))
			{
				SelectHeroIcon(num);
			}
		}
		UIHelper.RepositionGrid(HeroIconGrid);
	}

	private int[] ParseHeroIndices(string heroIndicesJson)
	{
		int[] array = ConstantDataContainer.ParesJsonArray<int>(heroIndicesJson);
		if (array == null)
		{
			return null;
		}
		return array;
	}

	private bool IsAllSelected()
	{
		if (GachaData == null)
		{
			return false;
		}
		return __Extension_Method_ICollection__.COUNT(_selectedHeroIndices) == GachaData.PickupHeroCount;
	}

	private void SaveSelectedHeroInidices(int[] selectedHeroindices)
	{
		string value = Json.Serialize(selectedHeroindices);
		_storageSelectedHeroIndicesJson.Value = value;
		DateTime currentServerLocalTime = GlobalAutoBehaviour<AccountManager>.instance.GetCurrentServerLocalTime();
		_storageCachedTime.Value = Vespa_DateTime_Extension_Method.ToNetworkString2(currentServerLocalTime);
	}

	private IEnumerator CoroutineRandomPickHero()
	{
		_autoSelectState = EAutoSelect.RUN;
		List<int> list = new List<int>();
		int[] pickupHeroIndices = GachaData.PickupHeroIndices;
		foreach (int item in pickupHeroIndices)
		{
			if (!_selectedHeroIndices.Contains(item))
			{
				list.Add(item);
			}
		}
		List<int> randomPickHeroIndices = new List<int>();
		int num = GachaData.PickupHeroCount - __Extension_Method_ICollection__.COUNT(_selectedHeroIndices);
		for (int j = 0; j < num; j++)
		{
			int maxExclusive = __Extension_Method_ICollection__.COUNT(list);
			int index = UnityEngine.Random.Range(0, maxExclusive);
			int item2 = list[index];
			_selectedHeroIndices.Add(item2);
			randomPickHeroIndices.Add(item2);
			list.RemoveAt(index);
		}
		yield return new WaitForSeconds(RandomPickAnimationPreDelay);
		for (int k = 0; k < __Extension_Method_ICollection__.COUNT(randomPickHeroIndices); k++)
		{
			int heroIndex = randomPickHeroIndices[k];
			SelectHeroIcon(heroIndex);
			UIHelper.RepositionGrid(HeroIconGrid);
			if (k < __Extension_Method_ICollection__.COUNT(randomPickHeroIndices))
			{
				yield return new WaitForSeconds(RandomPickAnimationInterval);
			}
		}
		UIHelper.RepositionGrid(HeroIconGrid);
		UpdateButtonState();
		_autoSelectState = EAutoSelect.IDLE;
	}

	public void OnClick_ResetSelection()
	{
		if (_autoSelectState != EAutoSelect.RUN)
		{
			ClearSelectedHero();
			UpdateButtonState();
		}
	}

	public void OnClick_AutoSelect()
	{
		if (GachaData != null && _autoSelectState != EAutoSelect.RUN && !IsAllSelected())
		{
			string title = LocalizationManager.GetString(NShared.NLocalization.Common.INFORM);
			string message = LocalizationManager.GetString(Gacha.ASK_RANDOM_SELECT_HERO);
			PayShopMessage3ButtonPopup.Open(title, message, delegate
			{
				VespaCoroutine.Start(this, CoroutineRandomPickHero());
				UIHelper.EnableButton(Button_Random, enable: false);
			});
		}
	}

	public void OnClick_ConfimeSummon()
	{
		if (_autoSelectState != EAutoSelect.RUN && GachaData != null && IsAllSelected())
		{
			if (_listener != null)
			{
				_listener.OnFinishPickUp(_selectedHeroIndices.ToArray());
			}
			SaveSelectedHeroInidices(_selectedHeroIndices.ToArray());
		}
	}

	void EquipGachaHeroIconComponent.IListener.OnClickHeroIcon(int heroIndex)
	{
		if (GachaData != null && _autoSelectState != EAutoSelect.RUN)
		{
			if (_selectedHeroIndices.Contains(heroIndex))
			{
				UnselectHeroIcon(heroIndex);
			}
			else
			{
				SelectHeroIcon(heroIndex);
			}
			UpdateButtonState();
			UIHelper.RepositionGrid(HeroIconGrid);
		}
	}

	void PayShopEquipGachaHeroClassList.IListener.OnClickTab(HighGachaCategoryType type)
	{
		SetCategory(type);
	}
}
