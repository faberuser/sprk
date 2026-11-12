using NShared.NJit;

namespace NShared;

public class ClientCreatureDataContainer : JitDataContainerDefault<ClientCreatureKey, ClientCreatureData>
{
	public const string TABLENAME = "ClientCreatureTable";

	private JitDictionary<string, ClientCreatureKey> _stringKey_to_dataKey = new JitDictionary<string, ClientCreatureKey>();

	public override TablePathDefine PathDefine => new TablePathDefine("ClientCreatureTable");

	public ClientCreatureDataContainer()
	{
		base.Packer.RegisterAndBind(_stringKey_to_dataKey);
	}

	protected override void OnEndLoadFromJsonData()
	{
		foreach (ClientCreatureData item in base.BaseList.JsonLoad_GetOrNew())
		{
			ClientCreatureKey kEY = item.GETKEY();
			_stringKey_to_dataKey.JsonLoad_Get().Add(kEY.GetStringKey(), kEY);
		}
	}

	public ClientCreatureData GetData(string stringKey)
	{
		if (!_stringKey_to_dataKey.TryGetValue(stringKey, out var value))
		{
			return null;
		}
		return GetData(value);
	}

	public ClientCreatureData GetData(string code, int costumeIndex)
	{
		if (costumeIndex != 0)
		{
			ClientCreatureData data = GetData(new ClientCreatureKey(code, costumeIndex));
			if (data != null)
			{
				return data;
			}
		}
		return GetData(new ClientCreatureKey(code, 0));
	}
}
