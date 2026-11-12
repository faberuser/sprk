using System;
using NShared.NJit;

namespace NShared;

public class GachaSelectItemDataContainer : JitDataContainerDefault<int, GachaSelectItemData>
{
	public const string TABLENAME = "GachaSelectItemTable";

	public override TablePathDefine PathDefine => new TablePathDefine("GachaSelectItemTable");

	public int GetGachaTicketIndex(int gachaIndex)
	{
		foreach (GachaSelectItemData allDatum in GetAllData())
		{
			if (Array.Exists(allDatum.GachaIndices, (int index) => index == gachaIndex))
			{
				return allDatum.ItemIndex;
			}
		}
		return 0;
	}
}
