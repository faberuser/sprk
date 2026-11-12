using System.Collections.Generic;
using NShared.NJit;

namespace NShared;

public class GameOptionDataContainer : JitDataContainerDefault<int, GameOptionData>
{
	public const string TABLENAME = "GameOptionTable";

	private JitDictionary<GameOptionTab, List<GameOptionData>> _lists_by_GameOptionTab = new JitDictionary<GameOptionTab, List<GameOptionData>>();

	private JitDictionary<int, List<int>> _indexLists_by_ParentNum = new JitDictionary<int, List<int>>();

	public override TablePathDefine PathDefine => new TablePathDefine("GameOptionTable");

	public GameOptionDataContainer()
	{
		base.Packer.RegisterAndBind(_lists_by_GameOptionTab);
		base.Packer.RegisterAndBind(_indexLists_by_ParentNum);
	}

	protected override void OnEndLoadFromJsonData()
	{
		foreach (GameOptionData item in base.BaseList.JsonLoad_GetOrNew())
		{
			_lists_by_GameOptionTab.JsonLoad_AddToListValue(item.GameOptionTab, item);
			if (item.ParentNum > 0)
			{
				_indexLists_by_ParentNum.JsonLoad_AddToListValue(item.ParentNum, item.Index);
			}
		}
	}

	public IList<GameOptionData> GetGameOptionDatas(GameOptionTab gameOptionTab)
	{
		return _lists_by_GameOptionTab.GetOrDefault(gameOptionTab);
	}

	public IList<int> GetChildList(int parentNum)
	{
		return _indexLists_by_ParentNum.GetOrDefault(parentNum);
	}
}
