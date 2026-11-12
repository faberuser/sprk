using System.Collections;
using System.Collections.Generic;
using NShared.NJit;

namespace NShared;

public class SkillDataContainer : JitDataContainerDefault<int, SkillData>, ISnapshotMemberSerializer
{
	public const string TABLENAME = "SkillTable";

	private JitRef<List<SkillData>> _list_of_OnRageSkill = new JitRef<List<SkillData>>();

	private JitDictionaryWithKey<OperationDataKey, OperationData.LoadData> _operationLoadDic = new JitDictionaryWithKey<OperationDataKey, OperationData.LoadData>();

	private Dictionary<OperationDataKey, OperationData> _operationCache = new Dictionary<OperationDataKey, OperationData>();

	public override TablePathDefine PathDefine => new TablePathDefine("SkillTable");

	public SkillDataContainer()
	{
		base.Packer.RegisterAndBind(_operationLoadDic);
		base.Packer.RegisterAndBind(_list_of_OnRageSkill);
	}

	protected override void OnLoadingValue_from_JsonData(IDictionary jsonDic, SkillData value)
	{
		for (int i = 1; i <= 8; i++)
		{
			if (jsonDic.Contains("OperationType" + i))
			{
				OperationData.LoadData loadData = JsonMarshalerUsingReflection.Parse(typeof(OperationData.LoadData), jsonDic, "Operation", i.ToString()) as OperationData.LoadData;
				loadData.SkillIndex = value.Index;
				loadData.OperationNo = i;
				if (loadData.Type != OperationType.None && !_operationLoadDic.JsonLoad_Get().ContainsKey(loadData.GETKEY()))
				{
					_operationLoadDic.JsonLoad_Get().Add(loadData.GETKEY(), loadData);
				}
			}
		}
	}

	protected override void OnEndLoadFromJsonData()
	{
		base.OnEndLoadFromJsonData();
		foreach (SkillData item in base.BaseList.JsonLoad_GetOrNew())
		{
			if (item.TriggerType == SkillTriggerType.OnRage && !_list_of_OnRageSkill.JsonLoad_GetOrNew().Contains(item))
			{
				_list_of_OnRageSkill.JsonLoad_GetOrNew().Add(item);
			}
		}
	}

	public OperationData GetOperationData(int skillIndex, int operationNo)
	{
		lock (_operationCache)
		{
			OperationDataKey key = new OperationDataKey(skillIndex, operationNo);
			if (_operationCache.TryGetValue(key, out var value))
			{
				return value;
			}
			if (!_operationLoadDic.TryGetValue(key, out var value2))
			{
				return null;
			}
			value = new OperationData(value2);
			_operationCache.Add(key, value);
			return value;
		}
	}

	public IList<SkillData> GetOnRageSkillList()
	{
		return _list_of_OnRageSkill.Eval();
	}

	object ISnapshotMemberSerializer.From(string value, DumpedSnapshotContainer container)
	{
		if (!int.TryParse(value, out var result))
		{
			return null;
		}
		return GetData(result);
	}

	string ISnapshotMemberSerializer.To(object value)
	{
		if (!(value is SkillData skillData))
		{
			return null;
		}
		return skillData.GETKEY().ToString();
	}
}
