using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class OrdealArenaNodeDataFormatter : IMessagePackFormatter<OrdealArenaNodeData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, OrdealArenaNodeData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(7);
		writer.Write(value.ChapterIndex);
		writer.Write(value.DungeonIndex);
		writer.Write(value.Index);
		writer.Write(value.Floor);
		resolver.GetFormatterWithVerify<EOrdealArenaNodeType>().Serialize(ref writer, value.NodeType, options);
		writer.Write(value.RewardIndex);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.DescKey, options);
	}

	public OrdealArenaNodeData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		OrdealArenaNodeData ordealArenaNodeData = new OrdealArenaNodeData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				ordealArenaNodeData.ChapterIndex = reader.ReadInt32();
				break;
			case 1:
				ordealArenaNodeData.DungeonIndex = reader.ReadInt32();
				break;
			case 2:
				ordealArenaNodeData.Index = reader.ReadInt32();
				break;
			case 3:
				ordealArenaNodeData.Floor = reader.ReadInt32();
				break;
			case 4:
				ordealArenaNodeData.NodeType = resolver.GetFormatterWithVerify<EOrdealArenaNodeType>().Deserialize(ref reader, options);
				break;
			case 5:
				ordealArenaNodeData.RewardIndex = reader.ReadInt32();
				break;
			case 6:
				ordealArenaNodeData.DescKey = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return ordealArenaNodeData;
	}
}
