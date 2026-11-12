using System;
using System.Collections.Generic;
using NGame2.NUI;
using NGame2.NUI3.NCommon.NChangingStatList;
using NShared;
using NVespa;
using NVespa.NGlobal;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI3.NCommon;

public class ChangingStatListComponent : LinkFactoryComponent, InitOnCreate
{
	public abstract class GroupAbstract : MonoBehaviour
	{
		public UIWidget Root;
	}

	[Serializable]
	public class ColorInfo
	{
		public Color LabelColor = Color.white;

		public bool Gradient;

		public ColorInfo()
		{
		}

		public ColorInfo(Color color)
		{
			LabelColor = color;
		}
	}

	public const string PrefabPath = "Prefab3/Common/ChangingStatListComponent";

	public static PrefabAuto<ChangingStatListComponent> PrefabAuto = new PrefabAuto<ChangingStatListComponent>("Prefab3/Common/ChangingStatListComponent");

	public static ColorInfo DefaultStatColor = new ColorInfo
	{
		LabelColor = new Color32(byte.MaxValue, 102, 0, byte.MaxValue),
		Gradient = false
	};

	public static ColorInfo DefaultValueColor = new ColorInfo
	{
		LabelColor = new Color32(byte.MaxValue, 102, 0, byte.MaxValue),
		Gradient = false
	};

	public UIWidget RootWidget;

	public Transform TemplateRoot;

	public UITable RootTable;

	[Header("--[ Template:Stat ]")]
	public GroupStatUi Template_Stat;

	[Header("--[ Template:Option ]")]
	public GroupOptionUi Template_Option;

	private List<GroupAbstract> _createWidgetList = new List<GroupAbstract>();

	private bool _repositionTable;

	private int _repositionCount;

	private const int RepositionCount = 2;

	private UIPanel _parentPanel;

	public static ChangingStatListComponent Create(UIPanel panel, int margin = 0)
	{
		if (panel == null)
		{
			return null;
		}
		Vector4 finalClipRegion = panel.finalClipRegion;
		Vector2 vector = new Vector2(finalClipRegion.z, finalClipRegion.w);
		Vector3 localPosition = new Vector3(finalClipRegion.x, finalClipRegion.y + vector.y / 2f);
		ChangingStatListComponent changingStatListComponent = PrefabAuto.Factory.Create(panel.transform);
		if ((bool)changingStatListComponent.RootWidget)
		{
			changingStatListComponent.RootWidget.SetDimensions((int)vector.x - margin * 2, 2);
			changingStatListComponent.RootWidget.transform.localPosition = localPosition;
		}
		changingStatListComponent._parentPanel = panel;
		return changingStatListComponent;
	}

	void InitOnCreate.OnCreate()
	{
		Reset();
	}

	void InitOnCreate.OnRelease()
	{
		Reset();
	}

	private void Reset()
	{
		UIHelper.EnableGameObject(TemplateRoot, enable: false);
		foreach (GroupAbstract createWidget in _createWidgetList)
		{
			VespaObject.DestroyGoAndDetachFromParent(createWidget.gameObject);
		}
		_createWidgetList.Clear();
	}

	private void Update()
	{
		UpdateTable();
	}

	private void LateUpdate()
	{
		UpdateTable();
	}

	private void UpdateTable()
	{
		if (!_repositionTable)
		{
			return;
		}
		_repositionCount--;
		if (_repositionCount <= 0)
		{
			_repositionTable = false;
		}
		RootTable.Reposition();
		if (_parentPanel != null)
		{
			UIScrollView component = _parentPanel.GetComponent<UIScrollView>();
			if ((bool)component)
			{
				component.ResetPosition();
			}
		}
	}

	private void AddCreatedWidget(GroupAbstract group)
	{
		if (!(group == null))
		{
			group.Root.width = RootWidget.width;
			UIHelper.EnableGameObject(group.gameObject, enable: true);
			Vespa_Unity_Transform_Extension_Method.ParentResetTM(group.transform, RootTable.transform);
			_createWidgetList.Add(group);
			RootTable.Reposition();
			_repositionTable = true;
			_repositionCount = 2;
		}
	}

	public static void SetLabelGradient(UILabel label, bool gradient)
	{
		if (!(label == null))
		{
			label.applyGradient = gradient;
		}
	}

	private void CreateSkillOptionChange(string beforeValue, string afterValue, ColorInfo beforeValueColor, ColorInfo afterValueColor, bool isPlus)
	{
		GroupOptionUi groupOptionUi = Template_Option.CreateFromTemplate(beforeValue, afterValue, beforeValueColor, afterValueColor, isPlus);
		AddCreatedWidget(groupOptionUi);
	}

	private void AddChangedStat(Enum statType, long beforeValue, long afterValue, ColorInfo statColor, ColorInfo beforeValueColor, ColorInfo afterValueColor, bool isPlus)
	{
		string typeName = LocalizationManager.GetString(statType);
		string statDisplayValue = StatData.GetStatDisplayValue(statType, beforeValue, showSign: false);
		string statDisplayValue2 = StatData.GetStatDisplayValue(statType, afterValue, showSign: false);
		GroupStatUi groupStatUi = Template_Stat.CreateFromTemplate(typeName, statDisplayValue, statDisplayValue2, statColor, beforeValueColor, afterValueColor, isPlus);
		AddCreatedWidget(groupStatUi);
	}

	private void RemoveDuplicatedStatChange(List<StatType> changeList, StatData beforeStatData, StatData afterStatData)
	{
		List<StatType> list = new List<StatType>();
		for (int i = 1; i < changeList.Count; i++)
		{
			StatType statType = changeList[i];
			ClientStatDisplayScoreData data = GameRoot.Instance.ClientStatDisplayScoreDataContainer.GetData(statType);
			if (data == null)
			{
				continue;
			}
			int equipItemVisibleGroup = data.EquipItemVisibleGroup;
			if (equipItemVisibleGroup == 0)
			{
				continue;
			}
			for (int j = 0; j < i; j++)
			{
				StatType statType2 = changeList[j];
				ClientStatDisplayScoreData data2 = GameRoot.Instance.ClientStatDisplayScoreDataContainer.GetData(statType2);
				if (data2 == null)
				{
					continue;
				}
				int equipItemVisibleGroup2 = data2.EquipItemVisibleGroup;
				if (equipItemVisibleGroup == equipItemVisibleGroup2)
				{
					long stat = beforeStatData.GetStat(statType);
					long stat2 = beforeStatData.GetStat(statType2);
					long stat3 = afterStatData.GetStat(statType);
					long stat4 = afterStatData.GetStat(statType2);
					if (stat == stat2 && stat3 == stat4)
					{
						list.Add(changeList[i]);
					}
				}
			}
		}
		foreach (StatType item in list)
		{
			changeList.Remove(item);
		}
	}

	public bool AddStatChange(StatData beforeStatData, StatData afterStatData, ColorInfo statColor = null, ColorInfo beforeValueColor = null, ColorInfo afterPlusValueColor = null, ColorInfo afterMinusValueColor = null)
	{
		if (beforeStatData == null || afterStatData == null)
		{
			return false;
		}
		List<StatType> list = new List<StatType>();
		List<StatType> list2 = new List<StatType>();
		List<StatType> list3 = new List<StatType>();
		StatType[] visibleStats = StatData.GetVisibleStats();
		foreach (StatType statType in visibleStats)
		{
			long stat = beforeStatData.GetStat(statType);
			long stat2 = afterStatData.GetStat(statType);
			if (stat != 0L || stat2 != 0L)
			{
				if (stat2 > stat)
				{
					list.Add(statType);
				}
				else if (stat2 < stat)
				{
					list3.Add(statType);
				}
				else
				{
					list2.Add(statType);
				}
			}
		}
		RemoveDuplicatedStatChange(list, beforeStatData, afterStatData);
		RemoveDuplicatedStatChange(list2, beforeStatData, afterStatData);
		RemoveDuplicatedStatChange(list3, beforeStatData, afterStatData);
		bool result = false;
		foreach (StatType item in list)
		{
			long stat3 = beforeStatData.GetStat(item);
			long stat4 = afterStatData.GetStat(item);
			AddChangedStat(item, stat3, stat4, statColor, beforeValueColor, afterPlusValueColor, isPlus: true);
			result = true;
		}
		foreach (StatType item2 in list2)
		{
			long stat5 = beforeStatData.GetStat(item2);
			long stat6 = afterStatData.GetStat(item2);
			AddChangedStat(item2, stat5, stat6, statColor, beforeValueColor, beforeValueColor, isPlus: false);
			result = true;
		}
		foreach (StatType item3 in list3)
		{
			long stat7 = beforeStatData.GetStat(item3);
			long stat8 = afterStatData.GetStat(item3);
			AddChangedStat(item3, stat7, stat8, statColor, beforeValueColor, afterMinusValueColor, isPlus: false);
			result = true;
		}
		return result;
	}

	public void AddOptionChange(EquipItemOptionInfo optionInfo, int beforeStar, int afterStar, ColorInfo statColor = null, ColorInfo beforeValueColor = null, ColorInfo afterPlusValueColor = null, ColorInfo afterMinusValueColor = null)
	{
		if (optionInfo == null)
		{
			return;
		}
		EquipOptionData data = GameRoot.Instance.EquipOptionDataContainer.GetData(optionInfo.OptionIndex);
		if (data != null)
		{
			int optionValue = data.GetOptionValue(beforeStar, optionInfo.OptionStep);
			int optionValue2 = data.GetOptionValue(afterStar, optionInfo.OptionStep);
			if (optionValue < optionValue2)
			{
				AddChangedStat(data.OptionType, optionValue, optionValue2, statColor, beforeValueColor, afterPlusValueColor, isPlus: true);
			}
			else if (optionValue > optionValue2)
			{
				AddChangedStat(data.OptionType, optionValue, optionValue2, statColor, beforeValueColor, afterMinusValueColor, isPlus: false);
			}
		}
	}

	public bool AddOptionChangeSkill(int optionIndex, int beforeStar, int afterStar, ColorInfo beforeValueColor = null, ColorInfo afterPlusValueColor = null, ColorInfo afterMinusValueColor = null)
	{
		EquipOptionData data = GameRoot.Instance.EquipOptionDataContainer.GetData(optionIndex);
		if (data == null)
		{
			VespaDebug.LogError("Option data not found : " + optionIndex);
			return false;
		}
		if (data.OptionType != OperationType.UseSkill)
		{
			return false;
		}
		int optionValueMax = data.GetOptionValueMax(beforeStar);
		int optionValueMax2 = data.GetOptionValueMax(afterStar);
		string optionDisplay = StatData.GetOptionDisplay(data.OptionType, optionValueMax, opened: true, 0, appendColor: false);
		string optionDisplay2 = StatData.GetOptionDisplay(data.OptionType, optionValueMax2, opened: true, 0, appendColor: false);
		if (optionValueMax < optionValueMax2)
		{
			CreateSkillOptionChange(optionDisplay, optionDisplay2, beforeValueColor, afterPlusValueColor, isPlus: true);
		}
		else if (optionValueMax > optionValueMax2)
		{
			CreateSkillOptionChange(optionDisplay, optionDisplay2, beforeValueColor, afterMinusValueColor, isPlus: false);
		}
		return true;
	}
}
