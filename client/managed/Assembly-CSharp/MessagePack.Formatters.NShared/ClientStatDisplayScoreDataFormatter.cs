using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class ClientStatDisplayScoreDataFormatter : IMessagePackFormatter<ClientStatDisplayScoreData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, ClientStatDisplayScoreData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(6);
		resolver.GetFormatterWithVerify<StatType>().Serialize(ref writer, value.StatType, options);
		resolver.GetFormatterWithVerify<CreatureTagType[]>().Serialize(ref writer, value.VisibleCreatureTag, options);
		writer.Write(value.EquipItemVisibleGroup);
		writer.Write(value.ShowMultedStatOnly);
		writer.Write(value.ShowOptionIndex);
		writer.Write(value.DisplayScore);
	}

	public ClientStatDisplayScoreData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		ClientStatDisplayScoreData clientStatDisplayScoreData = new ClientStatDisplayScoreData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				clientStatDisplayScoreData.StatType = resolver.GetFormatterWithVerify<StatType>().Deserialize(ref reader, options);
				break;
			case 1:
				clientStatDisplayScoreData.VisibleCreatureTag = resolver.GetFormatterWithVerify<CreatureTagType[]>().Deserialize(ref reader, options);
				break;
			case 2:
				clientStatDisplayScoreData.EquipItemVisibleGroup = reader.ReadInt32();
				break;
			case 3:
				clientStatDisplayScoreData.ShowMultedStatOnly = reader.ReadBoolean();
				break;
			case 4:
				clientStatDisplayScoreData.ShowOptionIndex = reader.ReadInt32();
				break;
			case 5:
				clientStatDisplayScoreData.DisplayScore = reader.ReadInt32();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return clientStatDisplayScoreData;
	}
}
