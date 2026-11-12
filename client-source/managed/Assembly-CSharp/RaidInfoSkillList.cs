using System.Collections.Generic;
using NGame2.NUI.NComponent2;
using NShared;
using NVespa.NPrefab2;
using UnityEngine;

public class RaidInfoSkillList : MonoBehaviour
{
	public struct BossSkillInfo
	{
		public int Index { get; private set; }

		public int Star { get; private set; }

		public int Lv { get; private set; }

		public SkillData SkillData { get; private set; }

		public BossSkillInfo(int index, int star, int lv, SkillData skillData)
		{
			Index = index;
			Star = star;
			Lv = lv;
			SkillData = skillData;
		}
	}

	public UITable TableSkillList;

	public UIScrollView ScrollViewSkillList;

	private CreatureData _bossData;

	private HeroInfo _bossInfo;

	private IRaidData _raidData;

	private List<BossSkillInfo> _dataList = new List<BossSkillInfo>();

	private List<RaidSkillComponent> _compList = new List<RaidSkillComponent>();

	protected virtual PrefabAuto<RaidSkillComponent> _prefabAuto => RaidSkillComponent.PrefabAuto;

	public void Open(HeroInfo monsterInfo, IRaidData raidData)
	{
		SetData(monsterInfo, raidData);
		UpdateUI();
	}

	public void SetData(HeroInfo monsterInfo, IRaidData raidData)
	{
		_bossInfo = monsterInfo;
		_raidData = raidData;
		_bossData = monsterInfo.CreatureData;
		UpdateListData();
	}

	public void UpdateUI()
	{
		UpdateListUI();
	}

	public void Clear()
	{
		_dataList.Clear();
		FactoryBase.ReleaseAndClearList(_compList);
	}

	private void OnEnable()
	{
		RefreshListUi();
	}

	private void UpdateListData()
	{
		if (_bossData == null)
		{
			return;
		}
		List<int> list = new List<int>();
		int num = 1;
		while (true)
		{
			int skillIndexBySlot = _bossData.GetSkillIndexBySlot(num);
			if (skillIndexBySlot == 0)
			{
				break;
			}
			list.Add(skillIndexBySlot);
			num++;
		}
		if (__Extension_Method_ICollection__.COUNT(_bossData.ShowInfoSkill) > 0)
		{
			list.AddRange(_bossData.ShowInfoSkill);
		}
		if (_raidData != null && __Extension_Method_ICollection__.COUNT(_raidData.ShowInfoSkill) > 0)
		{
			list.AddRange(_raidData.ShowInfoSkill);
		}
		_dataList.Clear();
		int i = 0;
		for (int count = list.Count; i < count; i++)
		{
			SkillData data = GameRoot.Instance.SkillDataContainer.GetData(list[i]);
			if (data != null)
			{
				_dataList.Add(new BossSkillInfo(_bossData.Index, _bossInfo.Star, _bossInfo.Level, data));
			}
		}
	}

	private void UpdateListUI()
	{
		if (!(TableSkillList == null) && !(ScrollViewSkillList == null))
		{
			FactoryBase.ReleaseAndClearList(_compList);
			int i = 0;
			for (int count = _dataList.Count; i < count; i++)
			{
				BossSkillInfo bossSkillInfo = _dataList[i];
				RaidSkillComponent raidSkillComponent = _prefabAuto.Factory.Create(TableSkillList.transform);
				raidSkillComponent.Open(bossSkillInfo.SkillData, bossSkillInfo.Index, bossSkillInfo.Star, bossSkillInfo.Lv);
				raidSkillComponent.SetBgIndex(__Extension_Method_ICollection__.COUNT(_compList));
				raidSkillComponent.Resize();
				_compList.Add(raidSkillComponent);
			}
			TableSkillList.repositionNow = true;
			ScrollViewSkillList.ResetPosition();
		}
	}

	private void RefreshListUi()
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_compList); i++)
		{
			RaidSkillComponent raidSkillComponent = _compList[i];
			if (!(raidSkillComponent == null))
			{
				raidSkillComponent.Resize();
			}
		}
		if (TableSkillList != null)
		{
			TableSkillList.Reposition();
			TableSkillList.repositionNow = true;
		}
		if (ScrollViewSkillList != null)
		{
			ScrollViewSkillList.ResetPosition();
		}
	}
}
