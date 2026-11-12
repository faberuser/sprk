using System.Collections.Generic;
using NGame2.NUI.NComponent2;
using NShared;
using NVespa;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;

namespace NGame2.NUI.NWindow;

public class BatchRuneBreakPopup : BaseWindow, OptionGradeComponent.Listener
{
	public class LocalSettingData
	{
		private Dictionary<int, LocalAccountBool> _gradeSelectDic = new Dictionary<int, LocalAccountBool>();

		private Dictionary<int, LocalAccountBool> _partTypeSelectDic = new Dictionary<int, LocalAccountBool>();

		public void Init()
		{
			if (!SystemStatus.IsPlaying)
			{
				return;
			}
			string fullName = typeof(BatchRuneBreakPopup).FullName;
			if (GameRoot.isValidInstance && GameRoot.Instance.OptionSellDataContainer != null)
			{
				OptionSellData[] categoryData = GameRoot.Instance.OptionSellDataContainer.GetCategoryData(OptionSellCategory.RuneGrade);
				for (int i = 0; i < __Extension_Method_ICollection__.COUNT(categoryData); i++)
				{
					OptionSellData optionSellData = categoryData[i];
					LocalAccountBool value = new LocalAccountBool(i_default: false, fullName + "_gradeSelect" + optionSellData.Value);
					_gradeSelectDic.Add(optionSellData.Value, value);
				}
				OptionSellData[] categoryData2 = GameRoot.Instance.OptionSellDataContainer.GetCategoryData(OptionSellCategory.RuneEquipPartType);
				for (int j = 0; j < __Extension_Method_ICollection__.COUNT(categoryData2); j++)
				{
					OptionSellData optionSellData2 = categoryData2[j];
					_ = optionSellData2.Value;
					LocalAccountBool value2 = new LocalAccountBool(i_default: false, fullName + "_partTypeSelect" + optionSellData2.Value);
					_partTypeSelectDic.Add(optionSellData2.Value, value2);
				}
			}
		}

		public bool GetGradeSelect(int grade)
		{
			return Vespa_Generic_IDictionary_Extension_Method.GETVALUE(_gradeSelectDic, grade, null)?.Value ?? false;
		}

		public void SetGradeSelect(int grade, bool isSelect)
		{
			LocalAccountBool vALUE = Vespa_Generic_IDictionary_Extension_Method.GETVALUE(_gradeSelectDic, grade, null);
			if (vALUE != null)
			{
				vALUE.Value = isSelect;
			}
		}

		public bool GetPartTypeSelect(int partType)
		{
			return Vespa_Generic_IDictionary_Extension_Method.GETVALUE(_partTypeSelectDic, partType, null)?.Value ?? false;
		}

		public void SetPartTypeSelect(int partType, bool Select)
		{
			LocalAccountBool vALUE = Vespa_Generic_IDictionary_Extension_Method.GETVALUE(_partTypeSelectDic, partType, null);
			if (vALUE != null)
			{
				vALUE.Value = Select;
			}
		}
	}

	public interface Listener
	{
		void RequestItemInfos(List<int> gradeList, List<int> partTypeList, List<ItemInfo> ItemInfos);

		void OnClickBreakRune(IEnumerable<ItemInfo> runeItemInfos);
	}

	public const string PrefabPath = "Resources/Prefab2/Window/MessagePopup/BatchRuneBreakPopup";

	public UISimpleGrid GradeGrid;

	public UISimpleGrid EquipPartTypeGrid;

	public UIButton Button_Break;

	public UILabel LabelNumItem;

	public UILabel LabelRuneFragment;

	private LocalSettingData localSetting;

	private Listener _listener;

	private List<ItemInfo> _runeItemInfos = new List<ItemInfo>();

	private List<OptionGradeComponent> _gradeList = new List<OptionGradeComponent>();

	private List<OptionGradeComponent> _partTypeList = new List<OptionGradeComponent>();

	private LocalSettingData LocalSetting
	{
		get
		{
			if (localSetting == null)
			{
				localSetting = new LocalSettingData();
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
		FactoryBase.ReleaseAndClearList(_partTypeList);
	}

	public void Init(Listener listener)
	{
		localSetting = LocalSetting;
		_listener = listener;
		Setup();
	}

	private void Setup()
	{
		UILabel labelNumItem = LabelNumItem;
		int num = 0;
		UIHelper.SetLabelText(labelNumItem, Vespa_Int_Extension_Method.ToCommaString(num));
		OptionSellData[] categoryData = GameRoot.Instance.OptionSellDataContainer.GetCategoryData(OptionSellCategory.RuneGrade);
		if (__Extension_Method_ICollection__.IsNullOrEmpty(categoryData))
		{
			return;
		}
		OptionSellData[] categoryData2 = GameRoot.Instance.OptionSellDataContainer.GetCategoryData(OptionSellCategory.RuneEquipPartType);
		if (!__Extension_Method_ICollection__.IsNullOrEmpty(categoryData2))
		{
			for (int i = 0; i < __Extension_Method_ICollection__.COUNT(categoryData); i++)
			{
				OptionSellData optionSellData = categoryData[i];
				bool gradeSelect = LocalSetting.GetGradeSelect(optionSellData.Value);
				OptionGradeComponent optionGradeComponent = UIHelper.AddGridItem(GradeGrid, OptionGradeComponent.PrefabAuto);
				optionGradeComponent.Open(this, optionSellData, gradeSelect);
				_gradeList.Add(optionGradeComponent);
			}
			UIHelper.RepositionGrid(GradeGrid);
			for (int j = 0; j < __Extension_Method_ICollection__.COUNT(categoryData2); j++)
			{
				OptionSellData optionSellData2 = categoryData2[j];
				OptionGradeComponent optionGradeComponent2 = UIHelper.AddGridItem(EquipPartTypeGrid, OptionGradeComponent.PrefabAuto);
				bool partTypeSelect = LocalSetting.GetPartTypeSelect(optionSellData2.Value);
				optionGradeComponent2.Open(this, categoryData2[j], partTypeSelect);
				_partTypeList.Add(optionGradeComponent2);
			}
			UIHelper.RepositionGrid(EquipPartTypeGrid);
			UpdateList();
		}
	}

	private void UpdateList()
	{
		List<int> list = new List<int>();
		for (int i = 0; i < _gradeList.Count; i++)
		{
			OptionGradeComponent optionGradeComponent = _gradeList[i];
			if (optionGradeComponent.IsChecked)
			{
				list.Add(optionGradeComponent.OptionData.Value);
			}
		}
		List<int> list2 = new List<int>();
		for (int j = 0; j < _partTypeList.Count; j++)
		{
			OptionGradeComponent optionGradeComponent2 = _partTypeList[j];
			if (optionGradeComponent2.IsChecked)
			{
				list2.Add(optionGradeComponent2.OptionData.Value);
			}
		}
		if (_listener != null)
		{
			_listener.RequestItemInfos(list, list2, _runeItemInfos);
		}
		int num = 0;
		long num2 = num;
		int num3 = 0;
		long num4 = num3;
		for (int k = 0; k < __Extension_Method_ICollection__.COUNT(_runeItemInfos); k++)
		{
			ItemInfo itemInfo = _runeItemInfos[k];
			if (itemInfo == null)
			{
				continue;
			}
			ItemData data = GameRoot.Instance.ItemDataContainer.GetData(itemInfo.ItemIndex);
			if (data != null)
			{
				RuneItemBreakData data2 = GameRoot.Instance.RuneItemBreakDataContainer.GetData(data.Grade);
				if (data2 != null)
				{
					int num5 = itemInfo.Count * data2.MaxItemCount1;
					long num6 = num2;
					int count = itemInfo.Count;
					checked
					{
						num2 = num6 + count;
						long num7 = num4;
						int num8 = num5;
						num4 = num7 + num8;
					}
				}
			}
		}
		UIButton button_Break = Button_Break;
		long num9 = num2;
		int num10 = 0;
		UIHelper.EnableButton(button_Break, num9 > num10);
		UIHelper.SetLabelText(LabelNumItem, Vespa_Int_Extension_Method.ToCommaString(num2));
		UIHelper.SetLabelText(LabelRuneFragment, Vespa_Int_Extension_Method.ToCommaString(num4));
	}

	public void OnClickBreak()
	{
		if (_listener != null)
		{
			_listener.OnClickBreakRune(_runeItemInfos);
		}
	}

	public void OnClickClose()
	{
		Close();
	}

	void OptionGradeComponent.Listener.OnClick(OptionSellData optionData, bool isChecked)
	{
		if (optionData.Category == OptionSellCategory.RuneGrade)
		{
			LocalSetting.SetGradeSelect(optionData.Value, isChecked);
		}
		else if (optionData.Category == OptionSellCategory.RuneEquipPartType)
		{
			LocalSetting.SetPartTypeSelect(optionData.Value, isChecked);
		}
		UpdateList();
	}
}
