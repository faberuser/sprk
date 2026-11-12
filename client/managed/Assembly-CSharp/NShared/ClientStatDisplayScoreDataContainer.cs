using System.Collections.Generic;
using NShared.NJit;

namespace NShared;

public class ClientStatDisplayScoreDataContainer : JitDataContainerDefault<StatType, ClientStatDisplayScoreData>
{
	public const string TABLENAME = "ClientStatDisplayScoreTable";

	public override TablePathDefine PathDefine => new TablePathDefine("ClientStatDisplayScoreTable");

	protected override void OnEndLoadFromJsonData()
	{
		int num = 0;
		using List<ClientStatDisplayScoreData>.Enumerator enumerator = base.BaseList.JsonLoad_GetOrNew().GetEnumerator();
		while (enumerator.MoveNext())
		{
			num = (enumerator.Current.DisplayScore = num + 1);
		}
	}
}
