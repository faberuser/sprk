using System;

namespace NShared;

public class JsonSerializableAttribute : Attribute
{
	public bool Serialize { get; set; }

	public bool Deserialize { get; set; }

	public bool Javascript { get; set; }

	public bool ManualField { get; set; }

	public bool ParseField { get; set; }

	public JsonSerializableAttribute()
	{
		Serialize = true;
		Deserialize = true;
		Javascript = false;
		ManualField = false;
		ParseField = false;
	}
}
