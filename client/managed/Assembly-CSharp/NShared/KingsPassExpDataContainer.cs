using System.Collections.Generic;
using NShared.NJit;

namespace NShared;

public class KingsPassExpDataContainer : JitDataContainerDefault<KingsPassExpKey, KingsPassExpData>
{
	public class KingsPassExpInfo : Tuple<StaminaType, int>
	{
		public KingsPassExpInfo(StaminaType type, int count)
			: base(type, count)
		{
		}
	}

	public const string TABLENAME = "KingsPassExpTable";

	private JitDictionary<int, List<KingsPassExpData>> _lists_by_ExpIndex = new JitDictionary<int, List<KingsPassExpData>>();

	public override TablePathDefine PathDefine => new TablePathDefine("KingsPassExpTable");

	public KingsPassExpDataContainer()
	{
		base.Packer.RegisterAndBind(_lists_by_ExpIndex);
	}

	protected override void OnEndLoadFromJsonData()
	{
		foreach (KingsPassExpData item in base.BaseList.JsonLoad_GetOrNew())
		{
			_lists_by_ExpIndex.JsonLoad_AddToListValue(item.ExpIndex, item);
		}
	}

	public IList<KingsPassExpData> GetExpInfos(int index)
	{
		return _lists_by_ExpIndex.GetOrDefault(index);
	}

	public KingsPassExpData GetData(int index, StaminaType staminaType)
	{
		return GetData(new KingsPassExpKey(index, staminaType));
	}
}
