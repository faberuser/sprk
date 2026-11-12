using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class HashStringFormatter : IMessagePackFormatter<HashString>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, HashString value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(2);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value._value, options);
		writer.Write(value.Length);
	}

	public HashString Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		HashString hashString = new HashString();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				hashString._value = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 1:
				reader.ReadInt32();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return hashString;
	}
}
