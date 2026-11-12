using System;
using System.Collections.Generic;
using System.Linq;
using JsonBin.NBinary.NWrite;
using LitJson;

namespace JsonBin.NBinary;

public class JsonToBinary
{
	private class HashedDataBuilder : IDisposable
	{
		private static Queue<HashedDataBuilder> _unusedQueue = new Queue<HashedDataBuilder>();

		public Dictionary<string, JsonData> JsonDic = new Dictionary<string, JsonData>();

		public Dictionary<int, JsonData> RemapDic = new Dictionary<int, JsonData>();

		public List<HashedData> HashedList = new List<HashedData>();

		public List<int> OffsetList = new List<int>();

		public static HashedDataBuilder CreateBuilder()
		{
			if (_unusedQueue.Count > 0)
			{
				return _unusedQueue.Dequeue();
			}
			return new HashedDataBuilder();
		}

		private static void ReleaseBuilder(HashedDataBuilder builder)
		{
			_unusedQueue.Enqueue(builder);
		}

		private HashedDataBuilder()
		{
		}

		void IDisposable.Dispose()
		{
			JsonDic.Clear();
			RemapDic.Clear();
			for (int i = 0; i < HashedList.Count; i++)
			{
				HashedData.ReleaseData(HashedList[i]);
			}
			HashedList.Clear();
			OffsetList.Clear();
			ReleaseBuilder(this);
		}

		public void SetupHashedList()
		{
			int count = RemapDic.Count;
			for (int i = 0; i < count; i++)
			{
				HashedList.Add(HashedData.CreateData());
			}
			foreach (KeyValuePair<int, JsonData> item in RemapDic)
			{
				int num = item.Key % count;
				if (num < 0)
				{
					num += count;
				}
				HashedList[num].pairList.Add(item);
			}
		}

		public void SetupDataOffsetAndBytes(KeyContainer keyContainer)
		{
			for (int i = 0; i < HashedList.Count; i++)
			{
				HashedData hashedData = HashedList[i];
				hashedData.offset = 0;
				if (i > 0)
				{
					HashedData hashedData2 = HashedList[i - 1];
					hashedData.offset = hashedData2.offset + hashedData2.Buffer.Length;
				}
				hashedData.Buffer = hashedData.BuildHashedData(keyContainer);
			}
		}
	}

	private class HashedData
	{
		private static Queue<HashedData> _unusedQueue = new Queue<HashedData>();

		public List<KeyValuePair<int, JsonData>> pairList = new List<KeyValuePair<int, JsonData>>();

		public ReadonlyBuffer Buffer;

		public int offset;

		public static HashedData CreateData()
		{
			if (_unusedQueue.Count > 0)
			{
				return _unusedQueue.Dequeue();
			}
			return new HashedData();
		}

		public static void ReleaseData(HashedData data)
		{
			ReadonlyBuffer.ReleaseBuffer(data.Buffer);
			data.Buffer = null;
			data.pairList.Clear();
			data.offset = 0;
			_unusedQueue.Enqueue(data);
		}

		public ReadonlyBuffer BuildHashedData(KeyContainer keyContainer)
		{
			using SimpleBufferWriter simpleBufferWriter = new SimpleBufferWriter();
			JsonBinWriter jsonBinWriter = new JsonBinWriter(simpleBufferWriter);
			int count = pairList.Count;
			jsonBinWriter.WritePacked(count);
			for (int i = 0; i < count; i++)
			{
				KeyValuePair<int, JsonData> keyValuePair = pairList[i];
				jsonBinWriter.WritePacked(keyValuePair.Key);
				ReadonlyBuffer readonlyBuffer = Serialize(keyValuePair.Value, keyContainer);
				jsonBinWriter.WritePacked(readonlyBuffer.Length);
				jsonBinWriter.CopyDirect(readonlyBuffer);
			}
			return simpleBufferWriter.ToArray();
		}
	}

	public static uint Version = 539035656u;

	public static bool TryToJsonBinary(JsonData json, out byte[] resultBytes)
	{
		Dictionary<string, int> dictionary = new Dictionary<string, int>();
		CollectString(json, dictionary);
		List<KeyValuePair<string, int>> list = dictionary.ToList();
		list.Sort(delegate(KeyValuePair<string, int> lhs, KeyValuePair<string, int> rhs)
		{
			int num2 = rhs.Value.CompareTo(lhs.Value);
			return (num2 != 0) ? num2 : lhs.Key.Length.CompareTo(rhs.Key.Length);
		});
		KeyContainer keyContainer = new KeyContainer();
		for (int num = 0; num < list.Count; num++)
		{
			string key = list[num].Key;
			keyContainer.AddValueString(key);
		}
		using (ReadonlyBuffer readonlyBuffer = Serialize(json, keyContainer))
		{
			byte[] array = keyContainer.Serialize();
			using SimpleBufferWriter simpleBufferWriter = new SimpleBufferWriter();
			JsonBinWriter jsonBinWriter = new JsonBinWriter(simpleBufferWriter);
			jsonBinWriter.WriteRaw_UInt32(Version);
			jsonBinWriter.WritePacked(array.Length);
			jsonBinWriter.CopyDirect(array);
			jsonBinWriter.WritePacked(readonlyBuffer.Length);
			jsonBinWriter.CopyDirect(readonlyBuffer);
			resultBytes = simpleBufferWriter.ToArrayFinal();
		}
		return __Extension_Method_ICollection__.IsNullOrEmpty(KeyContainer.Errors_on_WriteJsonBin);
	}

	private static void CollectString(JsonData json, Dictionary<string, int> stringCountDic)
	{
		if (json.IsString())
		{
			string text = Vespa_Common_Class_Extension_Method.TOSTRING(json);
			if (text == null)
			{
				text = string.Empty;
			}
			if (!stringCountDic.TryGetValue(text, out var value))
			{
				stringCountDic.Add(text, 1);
			}
			else
			{
				stringCountDic[text] = value + 1;
			}
		}
		else if (json.IsArray())
		{
			for (int i = 0; i < json.Count; i++)
			{
				CollectString(json.GetFromArray(i), stringCountDic);
			}
		}
		else
		{
			if (!json.IsObject())
			{
				return;
			}
			foreach (KeyValuePair<string, JsonData> item in json.ToDictionary())
			{
				CollectString(item.Value, stringCountDic);
			}
		}
	}

	private static ReadonlyBuffer Serialize(JsonData json, KeyContainer keyContainer)
	{
		using SimpleBufferWriter simpleBufferWriter = new SimpleBufferWriter();
		JsonBinWriter jsonBinWriter = new JsonBinWriter(simpleBufferWriter);
		if (json == null)
		{
			jsonBinWriter.WritePacked_Nil();
		}
		else
		{
			switch (json.GetJsonType())
			{
			case LitJson.JsonType.None:
				jsonBinWriter.WritePacked_Nil();
				break;
			case LitJson.JsonType.Boolean:
				jsonBinWriter.WritePacked(json.GETBOOLEAN(i_default: false));
				break;
			case LitJson.JsonType.Long:
				jsonBinWriter.WritePacked(json.GETLONG(0L));
				break;
			case LitJson.JsonType.Double:
			{
				if (json.TryGetDouble(out var o_value))
				{
					jsonBinWriter.WritePacked((float)o_value);
				}
				else
				{
					jsonBinWriter.WritePacked(0);
				}
				break;
			}
			case LitJson.JsonType.String:
				BuildString(json, jsonBinWriter, keyContainer);
				break;
			case LitJson.JsonType.Array:
				BuildArray(json, jsonBinWriter, keyContainer);
				break;
			case LitJson.JsonType.Object:
				BuildObject(json, jsonBinWriter, keyContainer);
				break;
			}
		}
		return simpleBufferWriter.ToArray();
	}

	private static void BuildString(JsonData json, JsonBinWriter writer, KeyContainer keyContainer)
	{
		int stringIndex = keyContainer.AddValueString(json.GETSTRING(string.Empty));
		writer.WritePacked_IndexedString(stringIndex);
	}

	private static void BuildArray(JsonData json, JsonBinWriter writer, KeyContainer keyContainer)
	{
		if (json.Count == 0)
		{
			writer.WritePacked_Nil();
			return;
		}
		int num = 0;
		List<int> list = new List<int>();
		List<ReadonlyBuffer> list2 = new List<ReadonlyBuffer>();
		for (int i = 0; i < json.Count; i++)
		{
			ReadonlyBuffer readonlyBuffer = Serialize(json.GetFromArray(i), keyContainer);
			list.Add(num);
			list2.Add(readonlyBuffer);
			num += readonlyBuffer.Length;
		}
		writer.WritePacked_ArrayHeader(list);
		for (int j = 0; j < __Extension_Method_ICollection__.COUNT(list2); j++)
		{
			ReadonlyBuffer buffer = list2[j];
			writer.CopyDirect(buffer);
		}
		for (int k = 0; k < list2.Count; k++)
		{
			ReadonlyBuffer.ReleaseBuffer(list2[k]);
		}
		list2.Clear();
	}

	private static void BuildObject(JsonData json, JsonBinWriter writer, KeyContainer keyContainer)
	{
		if (json.Count == 0)
		{
			writer.WritePacked_Nil();
			return;
		}
		using HashedDataBuilder hashedDataBuilder = HashedDataBuilder.CreateBuilder();
		json.ToDictionary(hashedDataBuilder.JsonDic);
		foreach (KeyValuePair<string, JsonData> item in hashedDataBuilder.JsonDic)
		{
			int key = keyContainer.AddKey(item.Key);
			hashedDataBuilder.RemapDic.Add(key, item.Value);
		}
		hashedDataBuilder.SetupHashedList();
		hashedDataBuilder.SetupDataOffsetAndBytes(keyContainer);
		for (int i = 0; i < hashedDataBuilder.HashedList.Count; i++)
		{
			hashedDataBuilder.OffsetList.Add(hashedDataBuilder.HashedList[i].offset);
		}
		writer.WritePacked_ArrayHeader(hashedDataBuilder.OffsetList, isMap: true);
		for (int j = 0; j < hashedDataBuilder.HashedList.Count; j++)
		{
			writer.CopyDirect(hashedDataBuilder.HashedList[j].Buffer);
		}
	}
}
