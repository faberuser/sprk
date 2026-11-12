using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class PlayerArchiveDataFormatter : IMessagePackFormatter<PlayerArchiveData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, PlayerArchiveData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(15);
		writer.Write(value.MainIndex);
		writer.Write(value.SubIndex);
		writer.Write(value.ShowIndex);
		resolver.GetFormatterWithVerify<ArchiveMainType>().Serialize(ref writer, value.ArchiveMainType, options);
		resolver.GetFormatterWithVerify<ArchiveSubType>().Serialize(ref writer, value.ArchiveSubType, options);
		resolver.GetFormatterWithVerify<ArchiveSubType2>().Serialize(ref writer, value.ArchiveSubType2, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.ConditionKey1, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.ConditionCompare1, options);
		resolver.GetFormatterWithVerify<ItemType[]>().Serialize(ref writer, value.ConditionValue1, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.ConditionKey2, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.ConditionCompare2, options);
		resolver.GetFormatterWithVerify<int[]>().Serialize(ref writer, value.ConditionValue2, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.ConditionKey3, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.ConditionCompare3, options);
		resolver.GetFormatterWithVerify<int[]>().Serialize(ref writer, value.ConditionValue3, options);
	}

	public PlayerArchiveData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		PlayerArchiveData playerArchiveData = new PlayerArchiveData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				playerArchiveData.MainIndex = reader.ReadInt32();
				break;
			case 1:
				playerArchiveData.SubIndex = reader.ReadInt32();
				break;
			case 2:
				playerArchiveData.ShowIndex = reader.ReadInt32();
				break;
			case 3:
				playerArchiveData.ArchiveMainType = resolver.GetFormatterWithVerify<ArchiveMainType>().Deserialize(ref reader, options);
				break;
			case 4:
				playerArchiveData.ArchiveSubType = resolver.GetFormatterWithVerify<ArchiveSubType>().Deserialize(ref reader, options);
				break;
			case 5:
				playerArchiveData.ArchiveSubType2 = resolver.GetFormatterWithVerify<ArchiveSubType2>().Deserialize(ref reader, options);
				break;
			case 6:
				playerArchiveData.ConditionKey1 = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 7:
				playerArchiveData.ConditionCompare1 = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 8:
				playerArchiveData.ConditionValue1 = resolver.GetFormatterWithVerify<ItemType[]>().Deserialize(ref reader, options);
				break;
			case 9:
				playerArchiveData.ConditionKey2 = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 10:
				playerArchiveData.ConditionCompare2 = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 11:
				playerArchiveData.ConditionValue2 = resolver.GetFormatterWithVerify<int[]>().Deserialize(ref reader, options);
				break;
			case 12:
				playerArchiveData.ConditionKey3 = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 13:
				playerArchiveData.ConditionCompare3 = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 14:
				playerArchiveData.ConditionValue3 = resolver.GetFormatterWithVerify<int[]>().Deserialize(ref reader, options);
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return playerArchiveData;
	}
}
