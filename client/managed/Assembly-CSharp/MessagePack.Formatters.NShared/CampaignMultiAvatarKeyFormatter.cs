using System;
using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class CampaignMultiAvatarKeyFormatter : IMessagePackFormatter<CampaignMultiAvatarKey>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, CampaignMultiAvatarKey value, MessagePackSerializerOptions options)
	{
		writer.WriteArrayHeader(2);
		writer.Write(value.ChapterIndex);
		writer.Write(value.LineIndex);
	}

	public CampaignMultiAvatarKey Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			throw new InvalidOperationException("typecode is null, struct not supported");
		}
		options.Security.DepthStep(ref reader);
		int num = reader.ReadArrayHeader();
		int chapterIndex = 0;
		int lineIndex = 0;
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				chapterIndex = reader.ReadInt32();
				break;
			case 1:
				lineIndex = reader.ReadInt32();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		CampaignMultiAvatarKey result = new CampaignMultiAvatarKey(chapterIndex, lineIndex);
		reader.Depth--;
		return result;
	}
}
