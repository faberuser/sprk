using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class LeftMenuButtonDataFormatter : IMessagePackFormatter<LeftMenuButtonData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, LeftMenuButtonData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(6);
		writer.Write(value.Index);
		resolver.GetFormatterWithVerify<LeftMenuButtonType>().Serialize(ref writer, value.LeftMenuButtonType, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.SubType, options);
		writer.Write(value.ShowIndex);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.TaskName, options);
		resolver.GetFormatterWithVerify<string[]>().Serialize(ref writer, value.TaskValue, options);
	}

	public LeftMenuButtonData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		LeftMenuButtonData leftMenuButtonData = new LeftMenuButtonData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				leftMenuButtonData.Index = reader.ReadInt32();
				break;
			case 1:
				leftMenuButtonData.LeftMenuButtonType = resolver.GetFormatterWithVerify<LeftMenuButtonType>().Deserialize(ref reader, options);
				break;
			case 2:
				leftMenuButtonData.SubType = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 3:
				leftMenuButtonData.ShowIndex = reader.ReadInt32();
				break;
			case 4:
				leftMenuButtonData.TaskName = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 5:
				leftMenuButtonData.TaskValue = resolver.GetFormatterWithVerify<string[]>().Deserialize(ref reader, options);
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return leftMenuButtonData;
	}
}
