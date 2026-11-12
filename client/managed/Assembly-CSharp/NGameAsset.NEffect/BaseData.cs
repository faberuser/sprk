using System;
using JsonBin;

namespace NGameAsset.NEffect;

[Serializable]
public class BaseData : ILoadJson
{
	public float DefaultTimeLength = 1f;

	bool ILoadJson.Load(JsonRead json)
	{
		DefaultTimeLength = json["DefaultTimeLength"].GETFLOAT(DefaultTimeLength);
		return true;
	}
}
