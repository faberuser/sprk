using System;
using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NHelper.NAffix;
using NShared;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;

namespace NGame2.NUI.NWindow.NPunish;

public class PunishmentLevelSelectPopup : BaseWindow, CountInputField2.IListener
{
	public interface IListenenr
	{
		void OnClickOpenBoss(PunishmentGroupData data, int level);
	}

	public static string PrefabPath = "Resources/Prefab2/Window/Raid/PunishmentLevelSelectPopup";

	private IListenenr _listener;

	private int _selectedGimmickIndex;

	private PunishmentGroupData _groupData;

	private int _groupIndex;

	private int _lastCleardLevel;

	private int _maxLevel;

	private int _currentLevel;

	private bool _reposition;

	public UILabel Label_Title;

	public UIScrollGrid AbilityScrollGrid;

	public UITable AbilityTable;

	public UIScrollGrid GimmickScrollGrid;

	public UITable GimmickTable;

	public CountInputField2 CountInputField;

	private List<PunishmentLevelAbilityComponent> _listLevelAbilityComp = new List<PunishmentLevelAbilityComponent>();

	private List<PunishmentLevelGimmickComponent> _listLevelGimmickComp = new List<PunishmentLevelGimmickComponent>();

	public void Open(IListenenr listener, int groupIndex)
	{
		if (listener != null)
		{
			Open(delegate
			{
				_listener = listener;
				_groupIndex = groupIndex;
				_groupData = GameRoot.Instance.PunishmentGroupDataContainer.GetData(groupIndex, EPunishmentDungeonType.Boss);
				Init();
			}, null);
		}
	}

	public override void OnClosing()
	{
		base.OnClosing();
		Clear();
		FactoryBase.ReleaseAndClearList(_listLevelGimmickComp);
		FactoryBase.ReleaseAndClearList(_listLevelAbilityComp);
	}

	private void Clear()
	{
		_selectedGimmickIndex = 0;
	}

	private void Init()
	{
		CreatureData data = GameRoot.Instance.CreatureDataContainer.GetData(_groupData.CreatureIndex);
		if (data != null)
		{
			UIHelper.SetLabelText(Label_Title, data.Name);
		}
		_lastCleardLevel = 0;
		OpenPunishmentRaidInfo openPunishmentRaidInfo = GlobalAutoBehaviour<AccountManager>.instance.OpenPunishmentRaidManager.Get(_groupIndex, EPunishmentDungeonType.Boss);
		if (openPunishmentRaidInfo != null)
		{
			_lastCleardLevel = openPunishmentRaidInfo.ClearLevel;
		}
		_maxLevel = _lastCleardLevel + 1;
		_currentLevel = Math.Max(_lastCleardLevel, 1);
		CountInputField2 countInputField = CountInputField;
		CountInputField2.CountInputFieldInfo obj = new CountInputField2.CountInputFieldInfo
		{
			MinCount = 1
		};
		int maxLevel = _maxLevel;
		obj.MaxCount = maxLevel;
		int currentLevel = _currentLevel;
		obj.InitCount = currentLevel;
		countInputField.Open(this, obj);
		Set2();
	}

	public void OnClickOpenRaid()
	{
		if (_listener != null)
		{
			_listener.OnClickOpenBoss(_groupData, _currentLevel);
		}
	}

	private void Set2()
	{
		if (_groupData == null)
		{
			return;
		}
		int affixGroupIndex = _groupData.AffixGroupIndex;
		IList<AffixData> affixDataListByGroupIndex = GameRoot.Instance.AffixDataContainer.GetAffixDataListByGroupIndex(affixGroupIndex);
		List<AffixData> list = new List<AffixData>();
		List<AffixData> list2 = new List<AffixData>();
		for (int i = 0; i < affixDataListByGroupIndex.Count; i++)
		{
			AffixData affixData = affixDataListByGroupIndex[i];
			if (affixData != null && AffixHelper.IsAutoApplyBattle(affixData, _currentLevel))
			{
				if (affixData.ShowType == EAffixShowType.Gimmick)
				{
					list2.Add(affixData);
				}
				else if (affixData.ShowType == EAffixShowType.InfinityLevel)
				{
					list.Add(affixData);
				}
			}
		}
		UIHelper.CreateOrReleaseCompnent(_listLevelAbilityComp, __Extension_Method_ICollection__.COUNT(list), AbilityTable.transform, PunishmentLevelAbilityComponent.PrefabAuto);
		UIHelper.CreateOrReleaseCompnent(_listLevelGimmickComp, __Extension_Method_ICollection__.COUNT(list2), GimmickTable.transform, PunishmentLevelGimmickComponent.PrefabAuto);
		for (int j = 0; j < __Extension_Method_ICollection__.COUNT(_listLevelAbilityComp); j++)
		{
			PunishmentLevelAbilityComponent punishmentLevelAbilityComponent = _listLevelAbilityComp[j];
			if (!(punishmentLevelAbilityComponent == null))
			{
				AffixData affixData2 = list[j];
				if (affixData2 != null)
				{
					punishmentLevelAbilityComponent.Open(affixData2, _currentLevel);
				}
			}
		}
		for (int k = 0; k < __Extension_Method_ICollection__.COUNT(_listLevelGimmickComp); k++)
		{
			PunishmentLevelGimmickComponent punishmentLevelGimmickComponent = _listLevelGimmickComp[k];
			if (!(punishmentLevelGimmickComponent == null))
			{
				AffixData affixData3 = list2[k];
				if (affixData3 != null)
				{
					punishmentLevelGimmickComponent.Open(affixData3, _currentLevel);
				}
			}
		}
		GimmickTable.onReposition = null;
		AbilityTable.onReposition = null;
		GimmickTable.Reposition();
		AbilityTable.Reposition();
		_reposition = true;
	}

	public override void LateUpdateDerived()
	{
		base.LateUpdateDerived();
		if (_reposition)
		{
			GimmickTable.onReposition = delegate
			{
				UIHelper.RepositionGrid(GimmickScrollGrid);
			};
			AbilityTable.onReposition = delegate
			{
				UIHelper.RepositionGrid(AbilityScrollGrid);
			};
			GimmickTable.Reposition();
			AbilityTable.Reposition();
			_reposition = false;
		}
	}

	void CountInputField2.IListener.ChangeCount()
	{
		long currentCount = CountInputField.CurrentCount;
		_currentLevel = checked((int)currentCount);
		Set2();
	}
}
