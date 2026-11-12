using System.Collections.Generic;
using NShared.NJit;

namespace NShared;

public class JukeboxDataContainer : JitDataContainerDefault<string, JukeboxData>
{
	public const string TABLENAME = "JukeboxTable";

	public override TablePathDefine PathDefine => new TablePathDefine("JukeboxTable");

	public List<JukeboxData> GetJukeboxDatas()
	{
		List<JukeboxData> list = new List<JukeboxData>();
		IList<JukeboxData> allData = GetAllData();
		if (allData == null)
		{
			return list;
		}
		for (int i = 0; i < allData.Count; i++)
		{
			JukeboxData jukeboxData = allData[i];
			if (jukeboxData != null && jukeboxData.Index > 0)
			{
				list.Add(jukeboxData);
			}
		}
		list.Sort((JukeboxData a, JukeboxData b) => a.Index - b.Index);
		return list;
	}
}
