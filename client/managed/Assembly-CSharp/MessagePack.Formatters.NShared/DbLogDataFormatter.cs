using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class DbLogDataFormatter : IMessagePackFormatter<DbLogData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, DbLogData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(4);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.Category, options);
		writer.Write(value.Value);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.Name, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.Type, options);
	}

	public DbLogData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		DbLogData dbLogData = new DbLogData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				dbLogData.Category = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 1:
				dbLogData.Value = reader.ReadInt32();
				break;
			case 2:
				dbLogData.Name = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 3:
				dbLogData.Type = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return dbLogData;
	}
}
