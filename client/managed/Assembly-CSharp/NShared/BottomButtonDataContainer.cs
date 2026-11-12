using System.Collections;
using NShared.NJit;

namespace NShared;

public class BottomButtonDataContainer : JitDataContainerDefault<BottomButtonKey, BottomButtonData>
{
	public const string TABLENAME = "BottomButtonTable";

	public override TablePathDefine PathDefine => new TablePathDefine("BottomButtonTable");

	protected override void OnLoadingValue_from_JsonData(IDictionary jsonDic, BottomButtonData value)
	{
		value.SubType = (string.IsNullOrEmpty(value.SubType) ? string.Empty : value.SubType);
	}

	public BottomButtonData GetByTutorialIndex(int index)
	{
		foreach (BottomButtonData allDatum in GetAllData())
		{
			if (allDatum.TutorialIndex == index)
			{
				return allDatum;
			}
		}
		return null;
	}

	public BottomButtonData GetData(ERightMenuButton type)
	{
		return GetData(type, string.Empty);
	}

	public BottomButtonData GetData(ERightMenuButton type, string subType)
	{
		return GetData(new BottomButtonKey(type, subType));
	}
}
