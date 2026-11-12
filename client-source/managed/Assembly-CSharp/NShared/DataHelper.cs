namespace NShared;

public static class DataHelper
{
	public static ClientCreatureData GetClientCreatureDataByIndex(int creatureIndex, int costumeIndex)
	{
		CreatureData data = GameRoot.Instance.CreatureDataContainer.GetData(creatureIndex);
		if (data == null)
		{
			return null;
		}
		if (data.CodeName == null)
		{
			return null;
		}
		ClientCreatureData data2 = GameRoot.Instance.ClientCreatureDataContainer.GetData(data.CodeName, costumeIndex);
		if (data2 == null)
		{
			return GameRoot.Instance.ClientCreatureDataContainer.GetData(data.CodeName);
		}
		return data2;
	}
}
