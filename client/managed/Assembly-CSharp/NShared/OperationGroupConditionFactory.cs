using System.Collections.Generic;
using NShared.NOperationGroupCondition;
using NVespa;

namespace NShared;

public class OperationGroupConditionFactory
{
	private readonly Dictionary<OperationGroupConditionType, IOperationGroupCondition> _conditionDictionary = new Dictionary<OperationGroupConditionType, IOperationGroupCondition>();

	public OperationGroupConditionFactory()
	{
		RegisterCreator<Random>(OperationGroupConditionType.Random);
	}

	private void RegisterCreator<T>(OperationGroupConditionType conditionType) where T : IOperationGroupCondition, new()
	{
		if (_conditionDictionary.ContainsKey(conditionType))
		{
			VespaDebug.LogError("Already registered operation group condition : " + conditionType);
			return;
		}
		_ = conditionType.ToString() != typeof(T).Name;
		_conditionDictionary.Add(conditionType, new T());
	}

	public IOperationGroupCondition GetCondition(OperationGroupConditionType conditionType)
	{
		IOperationGroupCondition value = null;
		_conditionDictionary.TryGetValue(conditionType, out value);
		return value;
	}
}
