using System;
using NShared.NJit;

namespace NShared;

public class BoosterIconActionDataContainer : JitDataContainerDefault<string, BoosterIconActionData>
{
	public const string TABLENAME = "BoosterIconActionTable";

	public override TablePathDefine PathDefine => new TablePathDefine("BoosterIconActionTable");

	public BoosterIconActionData GetData(EventBonusType boosterType)
	{
		foreach (BoosterIconActionData allDatum in GetAllData())
		{
			if ((EventBonusType)Enum.Parse(typeof(EventBonusType), allDatum.EventBonusType) == boosterType)
			{
				return allDatum;
			}
		}
		return null;
	}
}
