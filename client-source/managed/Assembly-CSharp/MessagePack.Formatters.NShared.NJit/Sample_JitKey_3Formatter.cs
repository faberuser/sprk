using System;
using NShared.NJit;

namespace MessagePack.Formatters.NShared.NJit;

public sealed class Sample_JitKey_3Formatter : IMessagePackFormatter<Sample_JitKey_3>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, Sample_JitKey_3 value, MessagePackSerializerOptions options)
	{
		writer.WriteArrayHeader(3);
		writer.Write(value.Field_0);
		writer.Write(value.Field_1);
		writer.Write(value.Field_2);
	}

	public Sample_JitKey_3 Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			throw new InvalidOperationException("typecode is null, struct not supported");
		}
		options.Security.DepthStep(ref reader);
		int num = reader.ReadArrayHeader();
		int field_ = 0;
		int field_2 = 0;
		int field_3 = 0;
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				field_ = reader.ReadInt32();
				break;
			case 1:
				field_2 = reader.ReadInt32();
				break;
			case 2:
				field_3 = reader.ReadInt32();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		Sample_JitKey_3 result = new Sample_JitKey_3(field_, field_2, field_3);
		reader.Depth--;
		return result;
	}
}
