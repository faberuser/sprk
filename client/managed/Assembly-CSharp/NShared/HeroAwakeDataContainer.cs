using System.Collections.Generic;
using NShared.NJit;

namespace NShared;

public class HeroAwakeDataContainer : JitDataContainerDefault<int, HeroAwakeData>
{
	public const string TABLENAME = "HeroAwakeTable";

	private JitDictionary<string, HeroAwakeData> _materialItemDicToAwakeData = new JitDictionary<string, HeroAwakeData>();

	public override TablePathDefine PathDefine => new TablePathDefine("HeroAwakeTable");

	public HeroAwakeDataContainer()
	{
		base.Packer.RegisterAndBind(_materialItemDicToAwakeData);
	}

	protected override void OnEndLoadFromJsonData()
	{
		base.OnEndLoadFromJsonData();
		foreach (KeyValuePair<int, HeroAwakeData> item in base.BaseDic.JsonLoad_Get())
		{
			AddMaterialItemToAwakeData(item.Value.AwakeMaterialItemCode1, item.Value);
			AddMaterialItemToAwakeData(item.Value.AwakeMaterialItemCode2, item.Value);
			AddMaterialItemToAwakeData(item.Value.AwakeMaterialItemCode3, item.Value);
			AddMaterialItemToAwakeData(item.Value.AwakeMaterialItemCode4, item.Value);
			AddMaterialItemToAwakeData(item.Value.TranscendMaterialItemCode0, item.Value);
			AddMaterialItemToAwakeData(item.Value.TranscendMaterialItemCode1, item.Value);
			AddMaterialItemToAwakeData(item.Value.TranscendMaterialItemCode2, item.Value);
			AddMaterialItemToAwakeData(item.Value.TranscendMaterialItemCode3, item.Value);
			AddMaterialItemToAwakeData(item.Value.TranscendMaterialItemCode4, item.Value);
		}
	}

	private void AddMaterialItemToAwakeData(string itemCode, HeroAwakeData newData)
	{
		if (!string.IsNullOrEmpty(itemCode))
		{
			if (!_materialItemDicToAwakeData.JsonLoad_Get().TryGetValue(itemCode, out var _))
			{
				_materialItemDicToAwakeData.JsonLoad_Get().Add(itemCode, newData);
			}
			else
			{
				JitOption.JitLogError($"HeroAwakeData(XXX MaterialItemCode={itemCode}): MaterialItem To Awake 의 값이 중복");
			}
		}
	}

	public HeroAwakeData GetByMaterialItemCode(string itemCode)
	{
		return _materialItemDicToAwakeData.GetOrDefault(itemCode);
	}
}
