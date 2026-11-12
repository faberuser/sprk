using System.Collections.Generic;
using JsonBin;
using NVespa;

namespace NGameAsset.NEffect;

public class BuilderContainer : SingletonAuto<BuilderContainer>
{
	public const string BundlePrefix = "Effect.";

	private JsonRead _rootRead;

	private Dictionary<string, Builder> _cache = new Dictionary<string, Builder>();

	public Builder Get(string requestCode)
	{
		if (String_Extension_Method.IsNullOrEmpty(requestCode))
		{
			return null;
		}
		lock (_cache)
		{
			string text = "Effect." + requestCode;
			if (_cache.TryGetValue(text, out var value))
			{
				return value;
			}
			if (!_rootRead.IsValid)
			{
				return null;
			}
			JsonRead json = _rootRead[text];
			if (!json.IsObject())
			{
				return null;
			}
			value = new Builder();
			value.Open(text, json);
			_cache.Add(text, value);
			return value;
		}
	}

	public void Load(JsonRead json)
	{
		_rootRead = json;
	}
}
