using System;

namespace NShared;

[JsonSerializable(Javascript = true)]
public class GloryPointResultInfo : ICurrencyResultInfo
{
	CurrencyType ICurrencyResultInfo.CurrencyType => CurrencyType.GloryPoint;

	public GloryPointType Type { get; set; }

	public long AddValue { get; set; }

	public long NewValue { get; set; }

	public static GloryPointResultInfo Create(CurrencyResultInfo3 info)
	{
		GloryPointResultInfo gloryPointResultInfo = new GloryPointResultInfo();
		gloryPointResultInfo.AddValue = info.AddValue;
		gloryPointResultInfo.NewValue = info.NewValue;
		long num = info.GetFieldValue("Type");
		if (Enum.IsDefined(typeof(GloryPointType), num))
		{
			long num2 = num;
			gloryPointResultInfo.Type = (GloryPointType)checked((int)num2);
		}
		return gloryPointResultInfo;
	}
}
