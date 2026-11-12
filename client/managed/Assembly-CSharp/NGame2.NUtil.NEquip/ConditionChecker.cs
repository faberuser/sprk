using System.Collections.Generic;
using NGame2.NUtil.NEquip.NConditionChecker;
using NShared;
using NVespa;

namespace NGame2.NUtil.NEquip;

public class ConditionChecker : SingletonAuto<ConditionChecker>
{
	public abstract class ConditionBase
	{
		public abstract bool Check(EquipItemInfo equipItemInfo, string[] values);
	}

	private Dictionary<string, ConditionBase> _dic = new Dictionary<string, ConditionBase>();

	public ConditionChecker()
	{
		AddCondition<Grade>();
	}

	private void AddCondition<T>() where T : ConditionBase, new()
	{
		_dic.Add(typeof(T).Name, new T());
	}

	public bool Check(EquipItemInfo equipItemInfo, string[] values)
	{
		if (__Extension_Method_ICollection__.IsNullOrEmpty(values))
		{
			return true;
		}
		return Vespa_Generic_IDictionary_Extension_Method.GETVALUE(_dic, values[0], null)?.Check(equipItemInfo, values) ?? false;
	}
}
