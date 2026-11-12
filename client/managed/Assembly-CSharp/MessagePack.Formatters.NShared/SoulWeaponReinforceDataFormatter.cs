using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class SoulWeaponReinforceDataFormatter : IMessagePackFormatter<SoulWeaponReinforceData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, SoulWeaponReinforceData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(6);
		writer.Write(value.ReinforceLevel);
		writer.Write(value.StatRate);
		writer.Write(value.RequiredExp);
		writer.Write(value.SuccessRateMark);
		writer.Write(value.ReqGold);
		resolver.GetFormatterWithVerify<string[]>().Serialize(ref writer, value.UnAvailableItemCode, options);
	}

	public SoulWeaponReinforceData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		SoulWeaponReinforceData soulWeaponReinforceData = new SoulWeaponReinforceData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				soulWeaponReinforceData.ReinforceLevel = reader.ReadInt32();
				break;
			case 1:
				soulWeaponReinforceData.StatRate = reader.ReadInt32();
				break;
			case 2:
				soulWeaponReinforceData.RequiredExp = reader.ReadInt32();
				break;
			case 3:
				soulWeaponReinforceData.SuccessRateMark = reader.ReadInt32();
				break;
			case 4:
				soulWeaponReinforceData.ReqGold = reader.ReadInt32();
				break;
			case 5:
				soulWeaponReinforceData.UnAvailableItemCode = resolver.GetFormatterWithVerify<string[]>().Deserialize(ref reader, options);
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return soulWeaponReinforceData;
	}
}
