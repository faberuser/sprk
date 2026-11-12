using LitJson;

namespace NVespa.NPrefab2;

public class PrefabGoAuto : IBuilder, ILoadJson, IJsonInterface, IMakeJson
{
	private PrefabGo _value;

	public string PrefabPath { get; private set; }

	public bool IsGlobal { get; private set; }

	public bool IsPooling { get; private set; }

	public bool TouchMaterial { get; private set; }

	public EPrefabType PrefabType { get; private set; }

	public PrefabGo Value
	{
		get
		{
			ReservePrefab();
			return _value;
		}
	}

	public FactoryGo Factory
	{
		get
		{
			if (Value != null)
			{
				return Value.Factory;
			}
			return null;
		}
	}

	public PrefabGoAuto()
		: this(string.Empty)
	{
	}

	public PrefabGoAuto(string path, bool isGlobal = false, EPrefabType prefabType = EPrefabType.Ngui, bool isPooling = true, bool touchMaterial = false)
	{
		PrefabPath = path;
		IsGlobal = isGlobal;
		IsPooling = isPooling;
		TouchMaterial = touchMaterial;
		PrefabType = prefabType;
	}

	public void ReservePrefab()
	{
		if (_value == null || _value.IsClosed)
		{
			_value = SingletonAuto<ContainerGo>.instance.Load(PrefabPath, IsGlobal, PrefabType, IsPooling, TouchMaterial);
		}
	}

	public void ReserveFactory(int count)
	{
		if (Factory != null)
		{
			Factory.ReserveComponent(count);
		}
	}

	bool ILoadJson.Load(JsonData json)
	{
		PrefabPath = json.GetJsonAny("PrefabPath", "ResPath").GETSTRING(PrefabPath);
		return true;
	}

	JsonData IMakeJson.MakeJson()
	{
		JsonData jsonData = new JsonData();
		jsonData.AddFieldString("PrefabPath", PrefabPath);
		return jsonData;
	}
}
