using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class AchievementTextDataFormatter : IMessagePackFormatter<AchievementTextData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, AchievementTextData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(5);
		writer.Write(value.Index);
		writer.Write(value.Step);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.AchievementNameTextKey, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.AchievementDescTextKey, options);
		writer.Write(value.DisplayValue);
	}

	public AchievementTextData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		AchievementTextData achievementTextData = new AchievementTextData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				achievementTextData.Index = reader.ReadInt32();
				break;
			case 1:
				achievementTextData.Step = reader.ReadInt32();
				break;
			case 2:
				achievementTextData.AchievementNameTextKey = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 3:
				achievementTextData.AchievementDescTextKey = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 4:
				achievementTextData.DisplayValue = reader.ReadInt32();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return achievementTextData;
	}
}
