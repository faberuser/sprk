using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class DungeonPassiveEnvironmentDataFormatter : IMessagePackFormatter<DungeonPassiveEnvironmentData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, DungeonPassiveEnvironmentData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(5);
		writer.Write(value.Index);
		resolver.GetFormatterWithVerify<TeamType>().Serialize(ref writer, value.TeamType, options);
		writer.Write(value.GroupIndex);
		resolver.GetFormatterWithVerify<string[]>().Serialize(ref writer, value.FilterValues, options);
		resolver.GetFormatterWithVerify<string[]>().Serialize(ref writer, value.StateValues, options);
	}

	public DungeonPassiveEnvironmentData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		DungeonPassiveEnvironmentData dungeonPassiveEnvironmentData = new DungeonPassiveEnvironmentData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				dungeonPassiveEnvironmentData.Index = reader.ReadInt32();
				break;
			case 1:
				dungeonPassiveEnvironmentData.TeamType = resolver.GetFormatterWithVerify<TeamType>().Deserialize(ref reader, options);
				break;
			case 2:
				dungeonPassiveEnvironmentData.GroupIndex = reader.ReadInt32();
				break;
			case 3:
				dungeonPassiveEnvironmentData.FilterValues = resolver.GetFormatterWithVerify<string[]>().Deserialize(ref reader, options);
				break;
			case 4:
				dungeonPassiveEnvironmentData.StateValues = resolver.GetFormatterWithVerify<string[]>().Deserialize(ref reader, options);
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return dungeonPassiveEnvironmentData;
	}
}
