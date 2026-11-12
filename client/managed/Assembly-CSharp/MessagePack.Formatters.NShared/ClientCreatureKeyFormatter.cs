using System;
using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class ClientCreatureKeyFormatter : IMessagePackFormatter<ClientCreatureKey>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, ClientCreatureKey value, MessagePackSerializerOptions options)
	{
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(2);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.Code, options);
		writer.Write(value.CostumeIndex);
	}

	public ClientCreatureKey Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			throw new InvalidOperationException("typecode is null, struct not supported");
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		string code = null;
		int costumeIndex = 0;
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				code = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 1:
				costumeIndex = reader.ReadInt32();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		ClientCreatureKey result = new ClientCreatureKey(code, costumeIndex);
		reader.Depth--;
		return result;
	}
}
