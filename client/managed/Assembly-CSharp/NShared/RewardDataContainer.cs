using System.Collections;
using System.Collections.Generic;
using NShared.NJit;

namespace NShared;

public class RewardDataContainer : JitDataContainerDefault<int, RewardData>
{
	public const string TABLENAME = "RewardTable";

	public override TablePathDefine PathDefine => new TablePathDefine("RewardTable");

	protected override void OnLoadingValue_from_JsonData(IDictionary jsonDic, RewardData value)
	{
		value.SubDataList = new List<RewardData.SubData>();
		for (int i = 1; i <= 100; i++)
		{
			string text = $"Item{i}";
			if (jsonDic.Contains(text + "Code") && JsonMarshalerUsingReflection.Parse(typeof(RewardData.SubData), jsonDic, text, string.Empty) is RewardData.SubData subData && !(subData.Code == null) && subData.Code.Length != 0)
			{
				subData.No = i;
				value.SubDataList.Add(subData);
			}
		}
	}
}
