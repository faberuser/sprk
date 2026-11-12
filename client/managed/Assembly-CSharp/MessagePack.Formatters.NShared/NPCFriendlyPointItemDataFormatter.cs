using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class NPCFriendlyPointItemDataFormatter : IMessagePackFormatter<NPCFriendlyPointItemData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, NPCFriendlyPointItemData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(4);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.ItemCode, options);
		resolver.GetFormatterWithVerify<int[]>().Serialize(ref writer, value.HeroIndices, options);
		writer.Write(value.FriendlyPoint);
		writer.Write(value.EventGiftPoint);
	}

	public NPCFriendlyPointItemData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		NPCFriendlyPointItemData nPCFriendlyPointItemData = new NPCFriendlyPointItemData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				nPCFriendlyPointItemData.ItemCode = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 1:
				nPCFriendlyPointItemData.HeroIndices = resolver.GetFormatterWithVerify<int[]>().Deserialize(ref reader, options);
				break;
			case 2:
				nPCFriendlyPointItemData.FriendlyPoint = reader.ReadInt32();
				break;
			case 3:
				nPCFriendlyPointItemData.EventGiftPoint = reader.ReadInt32();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return nPCFriendlyPointItemData;
	}
}
