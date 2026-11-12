using System;
using System.Collections.Generic;
using NGame2.NDropBonusEvent;
using NGame2.NUI.NComponent2;
using NNguiExtension;
using NShared;
using NVespa;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class OptionSellConfirmPopup : BaseWindow, OptionGradeComponent.Listener, OptionSellComponent.Listener
{
	public class LocalSettingData
	{
		private LocalAccountInt _tierSelect;

		private LocalAccountInt _starSelect;

		private LocalAccountInt _tmStarSelect;

		private LocalAccountBool _enhanceStone;

		private LocalAccountBool _EnchantedItem;

		private Dictionary<int, LocalAccountBool> _gradeSelectDic = new Dictionary<int, LocalAccountBool>();

		public bool IsOnlyOneGrade;

		private const int Version = 2;

		private bool IsSell;

		public int TierSelect
		{
			get
			{
				return _tierSelect.Value;
			}
			set
			{
				_tierSelect.Value = value;
			}
		}

		public int StarSelect
		{
			get
			{
				return _starSelect.Value;
			}
			set
			{
				_starSelect.Value = value;
			}
		}

		public int TMStarSelect
		{
			get
			{
				return _tmStarSelect.Value;
			}
			set
			{
				_tmStarSelect.Value = value;
			}
		}

		public bool EnhanceStone
		{
			get
			{
				return _enhanceStone.Value;
			}
			set
			{
				_enhanceStone.Value = value;
			}
		}

		public bool EnchantedItem
		{
			get
			{
				if (_EnchantedItem == null)
				{
					return false;
				}
				return _EnchantedItem.Value;
			}
			set
			{
				if (_EnchantedItem != null)
				{
					_EnchantedItem.Value = value;
				}
			}
		}

		public void Init()
		{
			string text = typeof(OptionSellConfirmPopup).FullName + (IsSell ? "Sell" : "Break") + 2;
			_tierSelect = new LocalAccountInt(0, text + "_tierSelect");
			_starSelect = new LocalAccountInt(0, text + "_starSelect");
			_tmStarSelect = new LocalAccountInt(0, text + "_tmStarSelect");
			_enhanceStone = new LocalAccountBool(i_default: false, text + "_enhanceStone");
			_EnchantedItem = new LocalAccountBool(i_default: false, text + "_EnchantedItem");
			if (GameRoot.isValidInstance && GameRoot.Instance.OptionSellDataContainer != null)
			{
				OptionSellData[] categoryData = GameRoot.Instance.OptionSellDataContainer.GetCategoryData(OptionSellCategory.Grade);
				for (int i = 0; i < __Extension_Method_ICollection__.COUNT(categoryData); i++)
				{
					OptionSellData optionSellData = categoryData[i];
					LocalAccountBool value = new LocalAccountBool(i_default: false, text + "_gradeSelect" + optionSellData.Value);
					_gradeSelectDic.Add(optionSellData.Value, value);
				}
			}
		}

		public LocalSettingData(bool isSell)
		{
			IsSell = isSell;
		}

		public bool GetGradeSelect(int grade)
		{
			return Vespa_Generic_IDictionary_Extension_Method.GETVALUE(_gradeSelectDic, grade, null)?.Value ?? false;
		}

		public void SetGradeSelect(OptionSellData optionData, bool isSelect)
		{
			LocalAccountBool vALUE = Vespa_Generic_IDictionary_Extension_Method.GETVALUE(_gradeSelectDic, optionData.Value, null);
			if (vALUE == null)
			{
				return;
			}
			if (isSelect)
			{
				bool flag = optionData.SelectType == OptionSellSelectType.OnlyOne;
				if (flag || IsOnlyOneGrade)
				{
					foreach (KeyValuePair<int, LocalAccountBool> item in _gradeSelectDic)
					{
						item.Value.Value = false;
					}
					IsOnlyOneGrade = flag;
				}
			}
			vALUE.Value = isSelect;
		}
	}

	public interface Listener
	{
		void RequestItemInfos(LocalSettingData data, List<int> gradeList, int minTier, int minStar, ref List<EquipItemInfo> equipItemInfos, bool isSell);

		bool IsEquipItemExist(int option, bool isGrade);

		void OnClickSellEquip(IEnumerable<EquipItemInfo> equipItemInfos);

		void OnClickBreakEquip(IEnumerable<EquipItemInfo> equipItemInfos);
	}

	public const string PrefabPath = "Resources/Prefab2/Window/MessagePopup/OptionSellConfirmPopup";

	public UIScrollGrid GradeGrid3;

	public UISimpleGrid TierHead;

	public UIScrollGrid TierGrid2;

	public UISimpleGrid StarHead;

	public UIScrollGrid StarGrid2;

	public UIPopupRoot TierRoot;

	public UIPopupRoot StarRoot;

	public UIGrid OptionGrid;

	public UISprite SpriteEnhancedEquip;

	public UISprite SpriteEnhanceStone;

	public UISprite SpriteEnchantedItem;

	public UILabel LabelNumItem;

	public UILabel LabelGold;

	private LocalSettingData localSetting;

	[SerializeField]
	private UILabel LbBeforeBonusGold;

	[SerializeField]
	private UILabel LbAfterBonusGold;

	[SerializeField]
	private UILabelAligner LbAlignerGold;

	private Listener _listener;

	private bool _isSell;

	protected List<EquipItemInfo> _equipItemInfos = new List<EquipItemInfo>();

	protected List<OptionGradeComponent> _gradeList = new List<OptionGradeComponent>();

	protected OptionSellComponent _tierHead;

	protected List<OptionSellComponent> _tierList = new List<OptionSellComponent>();

	protected OptionSellComponent _starHead;

	protected List<OptionSellComponent> _starList = new List<OptionSellComponent>();

	private LocalSettingData LocalSetting
	{
		get
		{
			if (localSetting == null)
			{
				localSetting = new LocalSettingData(_isSell);
				localSetting.Init();
			}
			return localSetting;
		}
		set
		{
			localSetting = value;
		}
	}

	public override void Reset()
	{
		base.Reset();
		FactoryBase.ReleaseAndClearList(_gradeList);
		FactoryBase.SafeReleaseComponent(ref _tierHead);
		FactoryBase.SafeReleaseComponent(ref _starHead);
		FactoryBase.ReleaseAndClearList(_tierList);
		FactoryBase.ReleaseAndClearList(_starList);
	}

	public void Init(Listener listener, bool isSell)
	{
		_listener = listener;
		_isSell = isSell;
		localSetting = LocalSetting;
		InitUI();
	}

	private void InitUI()
	{
		InitChoiceOption();
		InitGradeList();
		InitTierList();
		InitStarList();
		UpdateList();
	}

	private void RefreshList()
	{
		foreach (OptionGradeComponent grade in _gradeList)
		{
			bool gradeSelect = LocalSetting.GetGradeSelect(grade.OptionData.Value);
			grade.ForceCheck(gradeSelect);
		}
		bool flag = false;
		List<int> selectedGrade = GetSelectedGrade();
		if (!__Extension_Method_ICollection__.IsNullOrEmpty(_tierHead.OptionData.GradeDependency))
		{
			int[] gradeDependency = _tierHead.OptionData.GradeDependency;
			foreach (int item in gradeDependency)
			{
				flag = selectedGrade.Contains(item);
				if (flag)
				{
					break;
				}
			}
		}
		else
		{
			flag = true;
		}
		if (!flag)
		{
			OptionSellData[] categoryData = GameRoot.Instance.OptionSellDataContainer.GetCategoryData(OptionSellCategory.Tier);
			if (!__Extension_Method_ICollection__.IsNullOrEmpty(categoryData))
			{
				int num = 0;
				bool flag2 = false;
				for (int j = 0; j < __Extension_Method_ICollection__.COUNT(categoryData); j++)
				{
					int[] gradeDependency = categoryData[j].GradeDependency;
					foreach (int item2 in gradeDependency)
					{
						if (selectedGrade.Contains(item2))
						{
							num = j;
							flag2 = true;
							break;
						}
					}
					if (flag2)
					{
						break;
					}
				}
				LocalSetting.TierSelect = num;
				_tierHead.Refresh(categoryData[num]);
			}
		}
		else
		{
			_tierHead.Refresh(_tierHead.OptionData);
		}
		foreach (OptionSellComponent tier in _tierList)
		{
			tier.Refresh(tier.OptionData);
		}
		if (selectedGrade.Count == 0)
		{
			OptionSellKey key = new OptionSellKey(OptionSellCategory.Star, 0);
			OptionSellData data = GameRoot.Instance.OptionSellDataContainer.GetData(key);
			_starHead.Refresh(data);
		}
		else
		{
			RefreshStarOptionSellComp(_starHead, selectedGrade);
		}
		foreach (OptionSellComponent star in _starList)
		{
			RefreshStarOptionSellComp(star, selectedGrade);
		}
	}

	private void RefreshStarOptionSellComp(OptionSellComponent comp, List<int> selectedGrade)
	{
		if (comp.OptionData.Category != OptionSellCategory.Star && comp.OptionData.Category != OptionSellCategory.TMStar)
		{
			return;
		}
		OptionSellCategory category = ((comp.OptionData.Category == OptionSellCategory.Star) ? OptionSellCategory.TMStar : OptionSellCategory.Star);
		OptionSellKey key = new OptionSellKey(category, comp.OptionData.Value);
		OptionSellData data = GameRoot.Instance.OptionSellDataContainer.GetData(key);
		if (data == null)
		{
			return;
		}
		bool flag = false;
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(data.GradeDependency); i++)
		{
			int item = data.GradeDependency[i];
			if (selectedGrade.Contains(item))
			{
				flag = true;
				break;
			}
		}
		if (flag)
		{
			comp.Refresh(data);
		}
		else
		{
			comp.Refresh(comp.OptionData);
		}
	}

	protected virtual void UpdateList()
	{
		RefreshList();
		List<int> list = new List<int>();
		for (int i = 0; i < _gradeList.Count; i++)
		{
			OptionGradeComponent optionGradeComponent = _gradeList[i];
			if (optionGradeComponent.IsChecked)
			{
				list.Add(optionGradeComponent.OptionData.Value);
			}
		}
		int minTier = ((!(_tierHead != null)) ? 1 : _tierHead.GetValue());
		int minStar = ((_starHead != null) ? _starHead.GetValue() : 0);
		_listener.RequestItemInfos(LocalSetting, list, minTier, minStar, ref _equipItemInfos, _isSell);
		UIHelper.SetLabelText(LabelNumItem, _equipItemInfos.Count.ToString());
		UpdateList(_equipItemInfos);
	}

	protected virtual void UpdateList(List<EquipItemInfo> equipItemInfos)
	{
		int num = 0;
		long num2 = num;
		int num3 = 0;
		long num4 = num3;
		checked
		{
			foreach (EquipItemInfo equipItemInfo in equipItemInfos)
			{
				ItemData data = GameRoot.Instance.ItemDataContainer.GetData(equipItemInfo.ItemIndex);
				if (data != null)
				{
					long num5 = num2;
					int sellGold = data.SellGold;
					num2 = num5 + sellGold;
					long num6 = num4;
					int checkEquipSellEventBonus = GlobalAutoBehaviour<DropBonusEventManager>.instance.GetCheckEquipSellEventBonus(data);
					num4 = num6 + checkEquipSellEventBonus;
				}
			}
			UIHelper.SetLabelText(LabelGold, Vespa_Int_Extension_Method.ToCommaString(num2));
			UIHelper.SetLabelText(LbBeforeBonusGold, Vespa_Int_Extension_Method.ToCommaString(num2));
			UIHelper.SetLabelText(LbAfterBonusGold, Vespa_Int_Extension_Method.ToCommaString(num4));
			SetLabelStatusEquipEventBonus();
		}
	}

	private void SetLabelStatusEquipEventBonus()
	{
		bool flag = GlobalAutoBehaviour<DropBonusEventManager>.instance.IsEffectiveEvent(EventBonusType.BonusEquipSellReward);
		UIHelper.EnableGameObject(LbBeforeBonusGold.transform.parent, flag);
		UIHelper.EnableGameObject(LabelGold, !flag);
		if (LbAlignerGold != null)
		{
			LbAlignerGold.Align();
		}
	}

	private List<int> GetSelectedGrade()
	{
		List<int> list = new List<int>();
		foreach (OptionGradeComponent grade in _gradeList)
		{
			if (LocalSetting.GetGradeSelect(grade.OptionData.Value))
			{
				list.Add(grade.OptionData.Value);
			}
		}
		return list;
	}

	private void InitChoiceOption()
	{
		UIHelper.EnableGameObject(SpriteEnhanceStone, LocalSetting.EnhanceStone);
		UIHelper.EnableGameObject(SpriteEnchantedItem, LocalSetting.EnchantedItem);
		UIHelper.SetLabelText(LabelNumItem, "0");
		if (_isSell)
		{
			UILabel labelGold = LabelGold;
			int num = 0;
			UIHelper.SetLabelText(labelGold, Vespa_Int_Extension_Method.ToCommaString(num));
			UIHelper.SetLabelText(LbBeforeBonusGold, "0");
			UIHelper.SetLabelText(LbAfterBonusGold, "0");
		}
		if (OptionGrid != null)
		{
			OptionGrid.Reposition();
		}
	}

	private void InitGradeList()
	{
		OptionSellData[] categoryData = GameRoot.Instance.OptionSellDataContainer.GetCategoryData(OptionSellCategory.Grade);
		if (__Extension_Method_ICollection__.IsNullOrEmpty(categoryData))
		{
			return;
		}
		Array.Sort(categoryData, delegate(OptionSellData a, OptionSellData b)
		{
			ItemGradeData data = GameRoot.Instance.ItemGradeDataContainer.GetData(a.Value);
			ItemGradeData data2 = GameRoot.Instance.ItemGradeDataContainer.GetData(b.Value);
			byte b2 = data?.Score ?? 1;
			byte value = data2?.Score ?? 1;
			return b2.CompareTo(value);
		});
		for (int num = 0; num < __Extension_Method_ICollection__.COUNT(categoryData); num++)
		{
			OptionSellData optionSellData = categoryData[num];
			bool gradeSelect = LocalSetting.GetGradeSelect(optionSellData.Value);
			if (gradeSelect && optionSellData.SelectType == OptionSellSelectType.OnlyOne)
			{
				LocalSetting.IsOnlyOneGrade = true;
			}
			OptionGradeComponent optionGradeComponent = UIHelper.AddGridItem(GradeGrid3, OptionGradeComponent.PrefabAuto);
			optionGradeComponent.Open(this, optionSellData, gradeSelect);
			_gradeList.Add(optionGradeComponent);
		}
		UIHelper.RepositionGrid(GradeGrid3);
	}

	private void InitTierList()
	{
		OptionSellData[] categoryData = GameRoot.Instance.OptionSellDataContainer.GetCategoryData(OptionSellCategory.Tier);
		if (!__Extension_Method_ICollection__.IsNullOrEmpty(categoryData))
		{
			if (!Vespa_Generic_IList_Extension_Method.IsValidIndex(categoryData, LocalSetting.TierSelect))
			{
				LocalSetting.TierSelect = 0;
			}
			int width = 0;
			UIWidget component = TierHead.GetComponent<UIWidget>();
			if (component != null)
			{
				width = component.width;
			}
			_tierHead = UIHelper.AddGridItem(TierHead, OptionSellComponent.PrefabAuto);
			_tierHead.Open(this, categoryData[LocalSetting.TierSelect], onHead: true, width);
			for (int i = 0; i < categoryData.Length; i++)
			{
				OptionSellComponent optionSellComponent = UIHelper.AddGridItem(TierGrid2, OptionSellComponent.PrefabAuto);
				optionSellComponent.Open(this, categoryData[i], onHead: false, width);
				_tierList.Add(optionSellComponent);
			}
			UIHelper.RepositionGrid(TierHead);
			UIHelper.RepositionGrid(TierGrid2);
		}
	}

	private void InitStarList()
	{
		OptionSellData[] categoryData = GameRoot.Instance.OptionSellDataContainer.GetCategoryData(OptionSellCategory.Star);
		if (__Extension_Method_ICollection__.IsNullOrEmpty(categoryData))
		{
			return;
		}
		OptionSellData[] categoryData2 = GameRoot.Instance.OptionSellDataContainer.GetCategoryData(OptionSellCategory.TMStar);
		if (!__Extension_Method_ICollection__.IsNullOrEmpty(categoryData2))
		{
			if (!Vespa_Generic_IList_Extension_Method.IsValidIndex(categoryData, LocalSetting.StarSelect))
			{
				LocalSetting.StarSelect = 0;
			}
			if (!Vespa_Generic_IList_Extension_Method.IsValidIndex(categoryData2, LocalSetting.TMStarSelect))
			{
				LocalSetting.TMStarSelect = 0;
			}
			int width = 0;
			UIWidget component = StarHead.GetComponent<UIWidget>();
			if (component != null)
			{
				width = component.width;
			}
			_starHead = UIHelper.AddGridItem(StarHead, OptionSellComponent.PrefabAuto);
			_starHead.Open(this, categoryData[LocalSetting.StarSelect], onHead: true, width);
			for (int i = 0; i < categoryData.Length; i++)
			{
				OptionSellComponent optionSellComponent = UIHelper.AddGridItem(StarGrid2, OptionSellComponent.PrefabAuto);
				optionSellComponent.Open(this, categoryData[i], onHead: false, width);
				_starList.Add(optionSellComponent);
			}
			UIHelper.RepositionGrid(StarHead);
			UIHelper.RepositionGrid(StarGrid2);
		}
	}

	public void OnClickEnhancedEquip()
	{
	}

	public void OnClickEnhanceStone()
	{
		LocalSetting.EnhanceStone = !LocalSetting.EnhanceStone;
		UIHelper.EnableGameObject(SpriteEnhanceStone, LocalSetting.EnhanceStone);
		UpdateList();
	}

	public void OnClickEnchantedItem()
	{
		LocalSetting.EnchantedItem = !LocalSetting.EnchantedItem;
		UIHelper.EnableGameObject(SpriteEnchantedItem, LocalSetting.EnchantedItem);
		UpdateList();
	}

	public void OnClickSell()
	{
		_listener.OnClickSellEquip(_equipItemInfos);
	}

	public void OnClickBreak()
	{
		_listener.OnClickBreakEquip(_equipItemInfos);
	}

	public void OnClickClose()
	{
		Close();
	}

	void OptionGradeComponent.Listener.OnClick(OptionSellData optionData, bool isChecked)
	{
		LocalSetting.SetGradeSelect(optionData, isChecked);
		UpdateList();
	}

	List<int> OptionSellComponent.Listener.GetSelectedGrade()
	{
		return GetSelectedGrade();
	}

	void OptionSellComponent.Listener.OnClick(OptionSellData optionData, bool onHead)
	{
		if (onHead)
		{
			switch (optionData.Category)
			{
			case OptionSellCategory.Tier:
				TierRoot.TogglePopup();
				break;
			case OptionSellCategory.Star:
			case OptionSellCategory.TMStar:
				StarRoot.TogglePopup();
				break;
			}
			return;
		}
		switch (optionData.Category)
		{
		case OptionSellCategory.Tier:
		{
			int num2 = _tierList.FindIndex((OptionSellComponent sellComponent) => sellComponent.OptionData == optionData);
			if (num2 >= 0)
			{
				LocalSetting.TierSelect = num2;
			}
			_tierHead.Refresh(optionData);
			TierRoot.HidePopup();
			break;
		}
		case OptionSellCategory.Star:
		case OptionSellCategory.TMStar:
		{
			int num = _starList.FindIndex((OptionSellComponent sellComponent) => sellComponent.OptionData == optionData);
			if (num >= 0)
			{
				LocalSetting.StarSelect = num;
				LocalSetting.TMStarSelect = num;
			}
			_starHead.Refresh(optionData);
			StarRoot.HidePopup();
			break;
		}
		}
		UpdateList();
	}
}
