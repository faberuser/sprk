using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class IconChatWindowDataFormatter : IMessagePackFormatter<IconChatWindowData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, IconChatWindowData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(5);
		writer.Write(value.Index);
		writer.Write(value.Order);
		resolver.GetFormatterWithVerify<RankPanelType>().Serialize(ref writer, value.UIPanelType, options);
		resolver.GetFormatterWithVerify<RankViewType>().Serialize(ref writer, value.ViewType, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.IconFile, options);
	}

	public IconChatWindowData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		IconChatWindowData iconChatWindowData = new IconChatWindowData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				iconChatWindowData.Index = reader.ReadInt32();
				break;
			case 1:
				iconChatWindowData.Order = reader.ReadInt32();
				break;
			case 2:
				iconChatWindowData.UIPanelType = resolver.GetFormatterWithVerify<RankPanelType>().Deserialize(ref reader, options);
				break;
			case 3:
				iconChatWindowData.ViewType = resolver.GetFormatterWithVerify<RankViewType>().Deserialize(ref reader, options);
				break;
			case 4:
				iconChatWindowData.IconFile = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return iconChatWindowData;
	}
}
