using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NBattleField;
using NGame2.NUI.NComponent2.NBattle;
using NShared;
using NVespa;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;

namespace NGame2.NUI.NWindow.NBattle;

public class DungeonSkillWindow : BaseWindow
{
	public interface IListener
	{
		ReplaceSkillData GetReplacedSkillData(int slotIndex);
	}

	public const string PrefabPath = "Resources/Prefab2/Window/Battle/DungeonSkillWindow";

	public UISimpleGrid DungeonSkillGrid;

	public int MaxSkillSlot = 3;

	private IListener _iListener;

	private BattleType _battleType;

	private DungeonSkillComponent.ClickDungeonSkillCallback _onClickDungeonSkill;

	private List<int> _removeSkillIndexList = new List<int>();

	private Dictionary<int, Helper.SkillBasicInfo> _cachedSkillInfo = new Dictionary<int, Helper.SkillBasicInfo>();

	private IncreaseDic<int, BaseSkillComponent> _dungeonSkillDic = new IncreaseDic<int, BaseSkillComponent>();

	public BattleFacade.CreatureCacheData CreatureCache => SingletonAuto<BattleFacade>.instance.GetEnvironmentCreatureCache(TeamType.Left);

	public void Open(IListener listener, BattleType battleType, DungeonSkillComponent.ClickDungeonSkillCallback onClickDungeonSkill)
	{
		base.Open();
		_iListener = listener;
		_battleType = battleType;
		_onClickDungeonSkill = onClickDungeonSkill;
	}

	public override void Reset()
	{
		base.Reset();
		_cachedSkillInfo.Clear();
		_removeSkillIndexList.Clear();
		if (_dungeonSkillDic != null)
		{
			for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_dungeonSkillDic); i++)
			{
				BaseSkillComponent valueByIndex = _dungeonSkillDic.GetValueByIndex(i);
				valueByIndex.Close();
				FactoryBase.ReleaseComponent(valueByIndex);
			}
			_dungeonSkillDic.Clear();
		}
		if (DungeonSkillGrid != null)
		{
			DungeonSkillGrid.ClearGridItem<DungeonSkillComponent>();
		}
	}

	public override void UpdateDerived()
	{
		base.UpdateDerived();
		if (SingletonAuto<BattleFacade>.instance.IsValidBattle())
		{
			_removeSkillIndexList.Clear();
			if (__Extension_Method_ICollection__.COUNT(_dungeonSkillDic) != 0)
			{
				_removeSkillIndexList.AddRange(_dungeonSkillDic._dic.Keys);
			}
			bool flag = false;
			if (CreateDungeonSkillComponent())
			{
				flag = true;
			}
			if (UpdateDungeonSkillComponent())
			{
				flag = true;
			}
			if (flag)
			{
				UIHelper.RepositionGrid(DungeonSkillGrid);
			}
		}
	}

	private bool CreateDungeonSkillComponent()
	{
		BattleFacade.CreatureCacheData creatureCache = CreatureCache;
		if (creatureCache == null || creatureCache.TeamType != TeamType.Left)
		{
			return false;
		}
		if (!creatureCache.HasDungeonSkill)
		{
			return false;
		}
		bool result = false;
		int baseSkillIndex = creatureCache.CreatureData.BaseSkillIndex;
		for (int i = 1; i < 10; i++)
		{
			if (__Extension_Method_ICollection__.COUNT(_dungeonSkillDic) >= MaxSkillSlot)
			{
				break;
			}
			Helper.SkillBasicInfo skillBasicInfo = Helper.GetSkillBasicInfo(creatureCache.CreateInfo.HeroInfo, i);
			if (IsVaildActiveDungeonSkill(skillBasicInfo, baseSkillIndex))
			{
				if (_dungeonSkillDic.ContainsKey(skillBasicInfo.TableIndex))
				{
					_removeSkillIndexList.Remove(skillBasicInfo.TableIndex);
					continue;
				}
				DungeonSkillComponent dungeonSkillComponent = UIHelper.AddGridItem(DungeonSkillGrid, DungeonSkillComponent.PrefabAuto);
				dungeonSkillComponent.name = $"DungeonSkillComponent.{i}";
				dungeonSkillComponent.Open(i, skillBasicInfo, _onClickDungeonSkill, _battleType);
				_dungeonSkillDic.AddAndOverride(skillBasicInfo.TableIndex, dungeonSkillComponent);
				result = true;
			}
		}
		return result;
	}

	private bool UpdateDungeonSkillComponent()
	{
		bool result = false;
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_dungeonSkillDic); i++)
		{
			BaseSkillComponent valueByIndex = _dungeonSkillDic.GetValueByIndex(i);
			if (_removeSkillIndexList.Contains(valueByIndex.SkillIndex))
			{
				_dungeonSkillDic.RemoveByIndex(i);
				i--;
				valueByIndex.Close();
				FactoryBase.ReleaseComponent(valueByIndex);
				result = true;
			}
			else
			{
				valueByIndex.ChainUpdate();
				UpdateReplacedSkillIcon(valueByIndex);
			}
		}
		return result;
	}

	private void UpdateReplacedSkillIcon(BaseSkillComponent component)
	{
		int skillSlotIndex = component.SkillSlotIndex;
		ReplaceSkillData replacedSkillData = _iListener.GetReplacedSkillData(skillSlotIndex);
		if (!_cachedSkillInfo.TryGetValue(replacedSkillData.SkillIndex, out var value))
		{
			value = Helper.CreateSkillBasicInfo(CreatureCache.CreateInfo.HeroInfo, skillSlotIndex, replacedSkillData.SkillIndex);
			_cachedSkillInfo[replacedSkillData.SkillIndex] = value;
		}
		component.SetReplaceSkillData(replacedSkillData, value);
	}

	public bool IsShowingTooltip()
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_dungeonSkillDic); i++)
		{
			BaseSkillComponent valueByIndex = _dungeonSkillDic.GetValueByIndex(i);
			if (valueByIndex != null && valueByIndex.IsShowingTooltip())
			{
				return true;
			}
		}
		return false;
	}

	private bool IsVaildActiveDungeonSkill(Helper.SkillBasicInfo skillInfo, int baseSkillIndex)
	{
		if (skillInfo.TableIndex == 0 || skillInfo.TableIndex == baseSkillIndex)
		{
			return false;
		}
		if (skillInfo.TableData == null || skillInfo.TableData.TriggerType != SkillTriggerType.None)
		{
			return false;
		}
		return true;
	}
}
