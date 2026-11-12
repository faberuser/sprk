using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class ServerGroupNameDataFormatter : IMessagePackFormatter<ServerGroupNameData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, ServerGroupNameData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(2);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.ServerGroup, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.ServerName, options);
	}

	public ServerGroupNameData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		ServerGroupNameData serverGroupNameData = new ServerGroupNameData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				serverGroupNameData.ServerGroup = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 1:
				serverGroupNameData.ServerName = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return serverGroupNameData;
	}
}
