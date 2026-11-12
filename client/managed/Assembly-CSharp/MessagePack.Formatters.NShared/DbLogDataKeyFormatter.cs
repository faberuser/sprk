using System;
using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class DbLogDataKeyFormatter : IMessagePackFormatter<DbLogDataKey>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, DbLogDataKey value, MessagePackSerializerOptions options)
	{
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(2);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.Category, options);
		writer.Write(value.Value);
	}

	public DbLogDataKey Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			throw new InvalidOperationException("typecode is null, struct not supported");
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		string category = null;
		int value = 0;
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				category = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 1:
				value = reader.ReadInt32();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		DbLogDataKey result = new DbLogDataKey(category, value);
		reader.Depth--;
		return result;
	}
}
