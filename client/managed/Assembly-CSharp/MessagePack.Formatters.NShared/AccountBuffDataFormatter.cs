using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class AccountBuffDataFormatter : IMessagePackFormatter<AccountBuffData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, AccountBuffData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(6);
		writer.Write(value.Index);
		writer.Write(value.SkillIndex);
		writer.Write(value.SkillLevel);
		writer.Write(value.SkillValue);
		resolver.GetFormatterWithVerify<BattleType[]>().Serialize(ref writer, value.BattleType, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.ToolTipDesc, options);
	}

	public AccountBuffData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		AccountBuffData accountBuffData = new AccountBuffData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				accountBuffData.Index = reader.ReadInt32();
				break;
			case 1:
				accountBuffData.SkillIndex = reader.ReadInt32();
				break;
			case 2:
				accountBuffData.SkillLevel = reader.ReadInt32();
				break;
			case 3:
				accountBuffData.SkillValue = reader.ReadInt32();
				break;
			case 4:
				accountBuffData.BattleType = resolver.GetFormatterWithVerify<BattleType[]>().Deserialize(ref reader, options);
				break;
			case 5:
				accountBuffData.ToolTipDesc = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return accountBuffData;
	}
}
