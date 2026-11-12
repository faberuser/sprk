using System;
using System.Collections.Generic;
using NGame2.NUtil.NModifyChecker;
using NVespa;

namespace NGame2.NUtil;

public class ModifyChecker : SingletonAuto<ModifyChecker>
{
	public abstract class ConditionBase
	{
		public abstract bool Check(string[] values);
	}

	private Dictionary<string, ConditionBase> _dic = new Dictionary<string, ConditionBase>();

	private Dictionary<string, bool> _modifyConditions = new Dictionary<string, bool>();

	public ModifyChecker()
	{
		AddCondition<FirstClearDungeon>();
		AddCondition<ClearAchievement>();
		AddCondition<ClearTutorial>();
		AddCondition<FirstInAppPurchases>();
	}

	private void AddCondition<T>() where T : ConditionBase, new()
	{
		_dic.Add(typeof(T).Name, new T());
	}

	public bool Check(string[] values)
	{
		if (__Extension_Method_ICollection__.IsNullOrEmpty(values))
		{
			return false;
		}
		return Vespa_Generic_IDictionary_Extension_Method.GETVALUE(_dic, values[0], null)?.Check(values) ?? false;
	}

	public void CheckStart<T>(string[] values) where T : ConditionBase
	{
		if (!__Extension_Method_ICollection__.IsNullOrEmpty(values))
		{
			string name = typeof(T).Name;
			if (_modifyConditions.ContainsKey(name))
			{
				_modifyConditions.Remove(name);
			}
			string[] modifyCondition = GetModifyCondition<T>(values);
			_modifyConditions.Add(name, Check(modifyCondition));
		}
	}

	public bool CheckResult<T>(string[] values) where T : ConditionBase
	{
		if (__Extension_Method_ICollection__.IsNullOrEmpty(values))
		{
			return false;
		}
		string name = typeof(T).Name;
		if (_modifyConditions.ContainsKey(name))
		{
			string[] modifyCondition = GetModifyCondition<T>(values);
			if (Check(modifyCondition) != _modifyConditions[name])
			{
				return true;
			}
		}
		return false;
	}

	public string[] GetModifyCondition<T>(string[] values) where T : ConditionBase
	{
		string[] array = new string[1] { typeof(T).Name };
		Array.Resize(ref array, values.Length + 1);
		Array.Copy(values, 0, array, 1, values.Length);
		return array;
	}
}
