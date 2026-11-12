using System.Collections.Generic;
using JsonBin;
using NGameAsset.NCreature.NAnimationSample;
using NVespa;

namespace NGameAsset.NCreature;

public class BuilderContainer : SingletonAuto<BuilderContainer>
{
	public const string BundlePrefix = "Creature.";

	private JsonRead _rootRead;

	private Dictionary<string, Builder> _cache = new Dictionary<string, Builder>();

	public void DestroyOnSceneChanged()
	{
		lock (_cache)
		{
			foreach (KeyValuePair<string, Builder> item in _cache)
			{
				item.Value.DestroyOnSceneChanged();
			}
		}
		SingletonAuto<MeshRawData.Factory>.instance.DestroyOnSceneChanged();
	}

	public void ReserveOnSceneChanged()
	{
		SingletonAuto<MeshRawData.Factory>.instance.ReserveOnSceneChanged();
	}

	public void CreateAllBuilder()
	{
	}

	public Builder Get(string requestCode)
	{
		if (String_Extension_Method.IsNullOrEmpty(requestCode))
		{
			return null;
		}
		lock (_cache)
		{
			string text = "Creature." + requestCode;
			if (_cache.TryGetValue(text, out var value))
			{
				return value;
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
