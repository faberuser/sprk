using System.Collections.Generic;
using NShared.NJit;

namespace NShared;

public class EquipOptionDataContainer : JitDataContainerDefault<int, EquipOptionData>
{
	public const string TABLENAME = "EquipOptionTable";

	private JitDictionary<string, EquipOptionData> _dataByCode = new JitDictionary<string, EquipOptionData>();

	private JitDictionary<OperationType, List<EquipOptionData>> _datasByOptionType = new JitDictionary<OperationType, List<EquipOptionData>>();

	public override TablePathDefine PathDefine => new TablePathDefine("EquipOptionTable");

	public EquipOptionDataContainer()
	{
		base.Packer.RegisterAndBind(_dataByCode);
		base.Packer.RegisterAndBind(_datasByOptionType);
	}

	protected override void OnEndLoadFromJsonData()
	{
		foreach (EquipOptionData item in base.BaseList.JsonLoad_GetOrNew())
		{
			if (_dataByCode.JsonLoad_Get().TryGetValue(item.OptionType.ToString(), out var _))
			{
				return;
			}
			_dataByCode.JsonLoad_Get().Add(item.OptionType.ToString(), item);
		}
		foreach (EquipOptionData item2 in base.BaseList.JsonLoad_GetOrNew())
		{
			_datasByOptionType.JsonLoad_AddToListValue(item2.OptionType, item2);
		}
	}

	public int GetOptionValue(int optionIndex, int star, int currentStep)
	{
		return GetData(optionIndex)?.GetOptionValue(star, currentStep) ?? 0;
	}

	public IList<EquipOptionData> GetEquipOptionList(OperationType operationType)
	{
		return __Extension_Method_NShared_ReadonlyEmpty__.GetEmptyIfNull(_datasByOptionType.GetOrDefault(operationType));
	}

	public EquipOptionData GetDataByOptionName(string optionName)
	{
		return _dataByCode.GetOrDefault(optionName);
	}
}
