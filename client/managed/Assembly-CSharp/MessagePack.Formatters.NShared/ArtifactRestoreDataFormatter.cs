using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class ArtifactRestoreDataFormatter : IMessagePackFormatter<ArtifactRestoreData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, ArtifactRestoreData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(4);
		writer.Write(value.Index);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.MaterialItemCode, options);
		writer.Write(value.MaterialItemCount);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.ArtifactItemGroup, options);
	}

	public ArtifactRestoreData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		ArtifactRestoreData artifactRestoreData = new ArtifactRestoreData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				artifactRestoreData.Index = reader.ReadInt32();
				break;
			case 1:
				artifactRestoreData.MaterialItemCode = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 2:
				artifactRestoreData.MaterialItemCount = reader.ReadInt32();
				break;
			case 3:
				artifactRestoreData.ArtifactItemGroup = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return artifactRestoreData;
	}
}
