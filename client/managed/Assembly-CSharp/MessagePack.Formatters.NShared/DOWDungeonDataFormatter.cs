using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class DOWDungeonDataFormatter : IMessagePackFormatter<DOWDungeonData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, DOWDungeonData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(3);
		writer.Write(value.DungeonIndex);
		resolver.GetFormatterWithVerify<int[]>().Serialize(ref writer, value.OpenDOWs, options);
		resolver.GetFormatterWithVerify<CreatureTagType>().Serialize(ref writer, value.ClassType, options);
	}

	public DOWDungeonData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		DOWDungeonData dOWDungeonData = new DOWDungeonData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				dOWDungeonData.DungeonIndex = reader.ReadInt32();
				break;
			case 1:
				dOWDungeonData.OpenDOWs = resolver.GetFormatterWithVerify<int[]>().Deserialize(ref reader, options);
				break;
			case 2:
				dOWDungeonData.ClassType = resolver.GetFormatterWithVerify<CreatureTagType>().Deserialize(ref reader, options);
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return dOWDungeonData;
	}
}
