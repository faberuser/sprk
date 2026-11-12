using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class SoulWeaponUpgradeDataFormatter : IMessagePackFormatter<SoulWeaponUpgradeData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, SoulWeaponUpgradeData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		writer.WriteArrayHeader(3);
		writer.Write(value.Grade);
		writer.Write(value.UpgradeRate);
		writer.Write(value.ReqReinforceLevel);
	}

	public SoulWeaponUpgradeData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		int num = reader.ReadArrayHeader();
		SoulWeaponUpgradeData soulWeaponUpgradeData = new SoulWeaponUpgradeData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				soulWeaponUpgradeData.Grade = reader.ReadInt32();
				break;
			case 1:
				soulWeaponUpgradeData.UpgradeRate = reader.ReadInt32();
				break;
			case 2:
				soulWeaponUpgradeData.ReqReinforceLevel = reader.ReadInt32();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return soulWeaponUpgradeData;
	}
}
