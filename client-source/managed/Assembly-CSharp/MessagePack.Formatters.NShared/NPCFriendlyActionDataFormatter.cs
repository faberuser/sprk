using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class NPCFriendlyActionDataFormatter : IMessagePackFormatter<NPCFriendlyActionData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, NPCFriendlyActionData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(3);
		writer.Write(value.HeroIndex);
		writer.Write(value.ItemGrade);
		resolver.GetFormatterWithVerify<string[]>().Serialize(ref writer, value.ActionResponse, options);
	}

	public NPCFriendlyActionData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		NPCFriendlyActionData nPCFriendlyActionData = new NPCFriendlyActionData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				nPCFriendlyActionData.HeroIndex = reader.ReadInt32();
				break;
			case 1:
				nPCFriendlyActionData.ItemGrade = reader.ReadInt32();
				break;
			case 2:
				nPCFriendlyActionData.ActionResponse = resolver.GetFormatterWithVerify<string[]>().Deserialize(ref reader, options);
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return nPCFriendlyActionData;
	}
}
