using System;
using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class AttendanceUrlDataKeyFormatter : IMessagePackFormatter<AttendanceUrlDataKey>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, AttendanceUrlDataKey value, MessagePackSerializerOptions options)
	{
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(2);
		writer.Write(value.Index);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.Language, options);
	}

	public AttendanceUrlDataKey Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			throw new InvalidOperationException("typecode is null, struct not supported");
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		int field_ = 0;
		string field_2 = null;
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				field_ = reader.ReadInt32();
				break;
			case 1:
				field_2 = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			default:
				reader.Skip();
				break;
			}
		}
		AttendanceUrlDataKey result = new AttendanceUrlDataKey(field_, field_2);
		reader.Depth--;
		return result;
	}
}
