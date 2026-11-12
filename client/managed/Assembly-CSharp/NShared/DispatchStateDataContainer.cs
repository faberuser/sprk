using System;
using System.Collections.Generic;
using NShared.NJit;

namespace NShared;

public class DispatchStateDataContainer : JitDataContainerDefault<int, DispatchStateData>
{
	public const string TABLENAME = "DispatchStateTable";

	private JitDictionary<DispatchState, List<DispatchStateData>> _table = new JitDictionary<DispatchState, List<DispatchStateData>>();

	private Random _randomSelector = new Random(DateTime.Now.Second);

	public override TablePathDefine PathDefine => new TablePathDefine("DispatchStateTable");

	public DispatchStateDataContainer()
	{
		base.Packer.RegisterAndBind(_table);
	}

	protected override void OnEndLoadFromJsonData()
	{
		foreach (DispatchStateData item in base.BaseList.JsonLoad_GetOrNew())
		{
			_table.JsonLoad_AddToListValue(item.DispatchState, item);
		}
	}

	public DispatchStateData GetRandomDataInState(DispatchState state)
	{
		List<DispatchStateData> orDefault = _table.GetOrDefault(state);
		if (orDefault == null)
		{
			return null;
		}
		int index = _randomSelector.Next(0, orDefault.Count);
		return orDefault[index];
	}
}
