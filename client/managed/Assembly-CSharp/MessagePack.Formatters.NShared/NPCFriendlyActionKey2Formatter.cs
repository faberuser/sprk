using System;
using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class NPCFriendlyActionKey2Formatter : IMessagePackFormatter<NPCFriendlyActionKey2>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, NPCFriendlyActionKey2 value, MessagePackSerializerOptions options)
	{
		writer.WriteArrayHeader(2);
		writer.Write(value.HeroIndex);
		writer.Write(value.ItemGrade);
	}

	public NPCFriendlyActionKey2 Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			throw new InvalidOperationException("typecode is null, struct not supported");
		}
		options.Security.DepthStep(ref reader);
		int num = reader.ReadArrayHeader();
		int heroIndex = 0;
		int itemGrade = 0;
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				heroIndex = reader.ReadInt32();
				break;
			case 1:
				itemGrade = reader.ReadInt32();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		NPCFriendlyActionKey2 result = new NPCFriendlyActionKey2(heroIndex, itemGrade);
		reader.Depth--;
		return result;
	}
}
