using System.Collections.Generic;
using NShared.NJit;

namespace NShared;

public class ClientCustomizingPivotDataContainer : JitDataContainerDefault<ClientCustomizingPivotDataKey, ClientCustomizingPivotData>
{
	public const string TABLENAME = "ClientCustomizingPivotTable";

	private const int DefaultHeroIndex = 1;

	private HashSet<ClientCustomizingPivotDataKey> presetHeroCheckSet = new HashSet<ClientCustomizingPivotDataKey>();

	public override TablePathDefine PathDefine => new TablePathDefine("ClientCustomizingPivotTable");

	public ClientCustomizingPivotData GetData(int heroIndex, AccessoryPartPivotType pivotType)
	{
		ClientCustomizingPivotData data = GetData(new ClientCustomizingPivotDataKey(heroIndex, pivotType));
		presetHeroCheckSet.Clear();
		while (data != null && data.PresetHeroIndex != 0)
		{
			ClientCustomizingPivotDataKey item = new ClientCustomizingPivotDataKey(data.HeroIndex, data.PivotType);
			if (presetHeroCheckSet.Contains(item))
			{
				break;
			}
			presetHeroCheckSet.Add(item);
			data = GetData(new ClientCustomizingPivotDataKey(data.PresetHeroIndex, data.PivotType));
		}
		if (data == null)
		{
			return GetData(new ClientCustomizingPivotDataKey(1, pivotType));
		}
		return data;
	}
}
