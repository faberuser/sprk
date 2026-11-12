using System;
using System.Collections.Generic;
using NShared.NOperationRuntime;

namespace NShared;

public class OperationTypeAttribute : Attribute
{
	public OperationApplyTargetFlag ApplyTargetFlag = OperationApplyTargetFlag.Alive;

	private static readonly Dictionary<OperationType, bool> _cachedIsPercentDictionary = new Dictionary<OperationType, bool>();

	public virtual bool IsPercent { get; set; }

	public virtual bool IsDamageOrHeal => false;

	private static OperationTypeAttribute GetOperationAttribute(OperationType operationType)
	{
		return EnumAttributeContainer<OperationType, OperationTypeAttribute>.Get(operationType);
	}

	public static bool IsPercentOperation(OperationType operationType)
	{
		lock (_cachedIsPercentDictionary)
		{
			bool value = false;
			if (_cachedIsPercentDictionary.TryGetValue(operationType, out value))
			{
				return value;
			}
		}
		bool flag = GetOperationAttribute(operationType)?.IsPercent ?? false;
		if (flag && GameRoot.Instance.OperationRuntimeFactory.Create(operationType) is SetStatBase setStatBase)
		{
			StatType[] statTypes = setStatBase.GetStatTypes();
			if (statTypes != null)
			{
				for (int i = 0; i < statTypes.Length; i++)
				{
					if (GameRoot.Instance.StatAttenuationDataContainer.GetData(statTypes[i]) != null)
					{
						flag = false;
						break;
					}
					if (GameRoot.Instance.MultedStatAttenuationDataContainer.GetData(statTypes[i]) != null)
					{
						flag = false;
						break;
					}
					if (GameRoot.Instance.AddedStatAttenuationDataContainer.GetData(statTypes[i]) != null)
					{
						flag = false;
						break;
					}
				}
			}
		}
		lock (_cachedIsPercentDictionary)
		{
			_cachedIsPercentDictionary[operationType] = flag;
			return flag;
		}
	}

	public static OperationApplyTargetFlag GetOperationFlag(OperationType operationType)
	{
		return GetOperationAttribute(operationType)?.ApplyTargetFlag ?? OperationApplyTargetFlag.None;
	}

	public static bool HasOperationFlag(OperationType operationType, OperationApplyTargetFlag flag)
	{
		return (GetOperationFlag(operationType) & flag) != 0;
	}

	public static bool IsDamageOrHearOperation(OperationType operationType)
	{
		return GetOperationAttribute(operationType)?.IsDamageOrHeal ?? false;
	}
}
