using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class InspectUserDataFormatter : IMessagePackFormatter<InspectUserData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, InspectUserData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(3);
		resolver.GetFormatterWithVerify<InspectUserCategory>().Serialize(ref writer, value.Category, options);
		resolver.GetFormatterWithVerify<InspectUserType>().Serialize(ref writer, value.Type, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.StringKey, options);
	}

	public InspectUserData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		InspectUserData inspectUserData = new InspectUserData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				inspectUserData.Category = resolver.GetFormatterWithVerify<InspectUserCategory>().Deserialize(ref reader, options);
				break;
			case 1:
				inspectUserData.Type = resolver.GetFormatterWithVerify<InspectUserType>().Deserialize(ref reader, options);
				break;
			case 2:
				inspectUserData.StringKey = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return inspectUserData;
	}
}
