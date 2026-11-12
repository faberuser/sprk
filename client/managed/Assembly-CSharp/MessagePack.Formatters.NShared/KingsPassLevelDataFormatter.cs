using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class KingsPassLevelDataFormatter : IMessagePackFormatter<KingsPassLevelData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, KingsPassLevelData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		writer.WriteArrayHeader(4);
		writer.Write(value.LevelIndex);
		writer.Write(value.Level);
		writer.Write(value.ReqExp);
		writer.Write(value.LevelUpPrice);
	}

	public KingsPassLevelData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		int num = reader.ReadArrayHeader();
		KingsPassLevelData kingsPassLevelData = new KingsPassLevelData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				kingsPassLevelData.LevelIndex = reader.ReadInt32();
				break;
			case 1:
				kingsPassLevelData.Level = reader.ReadInt32();
				break;
			case 2:
				kingsPassLevelData.ReqExp = reader.ReadInt32();
				break;
			case 3:
				kingsPassLevelData.LevelUpPrice = reader.ReadInt32();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return kingsPassLevelData;
	}
}
