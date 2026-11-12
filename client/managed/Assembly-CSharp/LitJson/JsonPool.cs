using System.Collections.Generic;
using LitJson.NInternal;
using NVespa;

namespace LitJson;

public class JsonPool : SingletonAuto<JsonPool>
{
	private Queue<JsonData> _jsonQueue = new Queue<JsonData>();

	private Queue<JsonValueBool> _boolQueue = new Queue<JsonValueBool>();

	private Queue<JsonValueDouble> _doubleQueue = new Queue<JsonValueDouble>();

	private Queue<JsonValueLong> _longQueue = new Queue<JsonValueLong>();

	private Queue<JsonValueString> _stringQueue = new Queue<JsonValueString>();

	private Queue<JsonValueArray> _arrayQueue = new Queue<JsonValueArray>();

	private Queue<JsonValueObject> _objectQueue = new Queue<JsonValueObject>();

	protected override bool ThreadSafe => true;

	public string Dump()
	{
		return string.Empty;
	}

	public void EnqueueJsonValue(JsonValueBool value)
	{
		lock (_boolQueue)
		{
			_boolQueue.Enqueue(value);
		}
	}

	public void EnqueueJsonValue(JsonValueDouble value)
	{
		lock (_doubleQueue)
		{
			_doubleQueue.Enqueue(value);
		}
	}

	public void EnqueueJsonValue(JsonValueLong value)
	{
		lock (_longQueue)
		{
			_longQueue.Enqueue(value);
		}
	}

	public void EnqueueJsonValue(JsonValueString value)
	{
		lock (_stringQueue)
		{
			_stringQueue.Enqueue(value);
		}
	}

	public void EnqueueJsonValue(JsonValueArray value)
	{
		lock (_arrayQueue)
		{
			_arrayQueue.Enqueue(value);
		}
	}

	public void EnqueueJsonValue(JsonValueObject value)
	{
		lock (_objectQueue)
		{
			_objectQueue.Enqueue(value);
		}
	}

	public void ReleaseJsonValue(JsonData.IJsonValue jsonValue)
	{
		jsonValue?.ReleaseSelf();
	}

	public JsonValueBool CreateJsonValueBool(bool value)
	{
		JsonValueBool jsonValueBool = null;
		lock (_boolQueue)
		{
			if (_boolQueue.Count > 0)
			{
				jsonValueBool = _boolQueue.Dequeue();
			}
		}
		if (jsonValueBool == null)
		{
			jsonValueBool = new JsonValueBool();
		}
		jsonValueBool.Value = value;
		return jsonValueBool;
	}

	public JsonValueDouble CreateJsonValueDouble(double value)
	{
		JsonValueDouble jsonValueDouble = null;
		lock (_doubleQueue)
		{
			if (_doubleQueue.Count > 0)
			{
				jsonValueDouble = _doubleQueue.Dequeue();
			}
		}
		if (jsonValueDouble == null)
		{
			jsonValueDouble = new JsonValueDouble();
		}
		jsonValueDouble.Value = value;
		return jsonValueDouble;
	}

	public JsonValueLong CreateJsonValueLong(long value)
	{
		JsonValueLong jsonValueLong = null;
		lock (_longQueue)
		{
			if (_longQueue.Count > 0)
			{
				jsonValueLong = _longQueue.Dequeue();
			}
		}
		if (jsonValueLong == null)
		{
			jsonValueLong = new JsonValueLong();
		}
		jsonValueLong.Value = value;
		return jsonValueLong;
	}

	public JsonValueString CreateJsonValueString(string value)
	{
		JsonValueString jsonValueString = null;
		lock (_stringQueue)
		{
			if (_stringQueue.Count > 0)
			{
				jsonValueString = _stringQueue.Dequeue();
			}
		}
		if (jsonValueString == null)
		{
			jsonValueString = new JsonValueString();
		}
		jsonValueString.Value = value;
		return jsonValueString;
	}

	public JsonValueArray CreateJsonValueArray()
	{
		JsonValueArray jsonValueArray = null;
		lock (_arrayQueue)
		{
			if (_arrayQueue.Count > 0)
			{
				jsonValueArray = _arrayQueue.Dequeue();
			}
		}
		if (jsonValueArray == null)
		{
			jsonValueArray = new JsonValueArray();
		}
		return jsonValueArray;
	}

	public JsonValueObject CreateJsonValueObject()
	{
		JsonValueObject jsonValueObject = null;
		lock (_objectQueue)
		{
			if (_objectQueue.Count > 0)
			{
				jsonValueObject = _objectQueue.Dequeue();
			}
		}
		if (jsonValueObject == null)
		{
			jsonValueObject = new JsonValueObject();
		}
		return jsonValueObject;
	}

	public JsonData CreateJsonBool(bool value)
	{
		JsonData jsonData = CreateJsonData();
		jsonData.SetBoolean(value);
		return jsonData;
	}

	public JsonData CreateJsonDouble(double value)
	{
		JsonData jsonData = CreateJsonData();
		jsonData.SetDouble(value);
		return jsonData;
	}

	public JsonData CreateJsonLong(long value)
	{
		JsonData jsonData = CreateJsonData();
		jsonData.SetLong(value);
		return jsonData;
	}

	public JsonData CreateJsonString(string value)
	{
		JsonData jsonData = CreateJsonData();
		jsonData.SetString(value);
		return jsonData;
	}

	public JsonData CreateJsonData()
	{
		JsonData jsonData = null;
		lock (_jsonQueue)
		{
			if (_jsonQueue.Count > 0)
			{
				jsonData = _jsonQueue.Dequeue();
			}
		}
		if (jsonData != null)
		{
			jsonData.Reset();
			return jsonData;
		}
		return new JsonData();
	}

	public void SafeRelease(ref JsonData json)
	{
		JsonData json2 = json;
		json = null;
		ReleaseJsonData(json2);
	}

	public void ReleaseJsonData(JsonData json)
	{
		if (json == null || !json.ReleaseAll())
		{
			return;
		}
		lock (_jsonQueue)
		{
			_jsonQueue.Enqueue(json);
		}
	}

	public void OnSceneChanged()
	{
		lock (_jsonQueue)
		{
			_jsonQueue.Clear();
		}
		lock (_boolQueue)
		{
			_boolQueue.Clear();
		}
		lock (_doubleQueue)
		{
			_doubleQueue.Clear();
		}
		lock (_longQueue)
		{
			_longQueue.Clear();
		}
		lock (_stringQueue)
		{
			_stringQueue.Clear();
		}
		lock (_arrayQueue)
		{
			_arrayQueue.Clear();
		}
		lock (_objectQueue)
		{
			_objectQueue.Clear();
		}
	}
}
