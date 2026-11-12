using LitJson;
using UnityEngine;

namespace NVespa.NPrefab2;

public class PrefabResAuto<T> : IBuilder, ILoadJson, IJsonInterface, IMakeJson where T : Object
{
	private PrefabRes<T> _value;

	public string PrefabPath { get; private set; }

	public bool IsGlobal { get; private set; }

	public EPrefabType PrefabType { get; private set; }

	public PrefabRes<T> Value
	{
		get
		{
			ReservePrefab();
			return _value;
		}
	}

	public T Res
	{
		get
		{
			if (Value == null)
			{
				return null;
			}
			return Value.Res;
		}
	}

	public PrefabResAuto()
		: this(string.Empty, isGlobal: false, EPrefabType.Ngui)
	{
	}

	public PrefabResAuto(string path)
		: this(path, isGlobal: false, EPrefabType.Ngui)
	{
	}

	public PrefabResAuto(string path, bool isGlobal, EPrefabType prefabType)
	{
		PrefabPath = path;
		IsGlobal = isGlobal;
		PrefabType = prefabType;
	}

	public void ReservePrefab()
	{
		if (_value == null || _value.IsClosed)
		{
			_value = SingletonAuto<ContainerRes>.instance.Load<T>(PrefabPath, IsGlobal, PrefabType);
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
