using System.Collections.Generic;
using NShared.NOperationConditionAction;
using NVespa;

namespace NShared;

public class OperationConditionActionFactory
{
	private readonly Dictionary<OperationConditionActionType, IOperationConditionAction> _conditionDictionary = new Dictionary<OperationConditionActionType, IOperationConditionAction>();

	public OperationConditionActionFactory()
	{
		RegisterCreator<ModValue>(OperationConditionActionType.ModValue);
		RegisterCreator<SetValue>(OperationConditionActionType.SetValue);
		RegisterCreator<ModDurationMs>(OperationConditionActionType.ModDurationMs);
		RegisterCreator<SetDurationMs>(OperationConditionActionType.SetDurationMs);
		RegisterCreator<ModTickTimeMs>(OperationConditionActionType.ModTickTimeMs);
		RegisterCreator<SetTickTimeMs>(OperationConditionActionType.SetTickTimeMs);
	}

	private void RegisterCreator<T>(OperationConditionActionType conditionType) where T : IOperationConditionAction, new()
	{
		if (_conditionDictionary.ContainsKey(conditionType))
		{
			VespaDebug.LogError("Already registered operation condition action : " + conditionType);
			return;
		}
		_ = conditionType.ToString() != typeof(T).Name;
		_conditionDictionary.Add(conditionType, new T());
	}

	public IOperationConditionAction GetAction(OperationConditionActionType conditionType)
	{
		IOperationConditionAction value = null;
		_conditionDictionary.TryGetValue(conditionType, out value);
		return value;
	}
}
