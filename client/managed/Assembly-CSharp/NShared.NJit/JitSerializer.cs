using System.Collections.Generic;
using System.Diagnostics;
using MessagePack;
using MessagePack.Formatters;
using NShared.NJit.NCommon;
using NShared.NJit.NResolver;

namespace NShared.NJit;

public class JitSerializer : IFormatterResolver
{
	private class SerializeData
	{
		public object Obj;

		public byte[] Bytes;
	}

	private List<IJitContainer> _containerList = new List<IJitContainer>();

	private List<SerializeData> _referenceList = new List<SerializeData>();

	private Dictionary<object, int> _referenceDic = new Dictionary<object, int>();

	public MessagePackSerializerOptions MessagePackSerializerOptions { get; private set; }

	public JitStringSerializer StringSerializer { get; private set; }

	public JitSerializer()
	{
		MessagePackSerializerOptions = new MessagePackSerializerOptions(this);
		StringSerializer = new JitStringSerializer();
	}

	IMessagePackFormatter<T> IFormatterResolver.GetFormatter<T>()
	{
		if (StringSerializer is IMessagePackFormatter<T> result)
		{
			return result;
		}
		return MessagePackSerializer.DefaultOptions.Resolver.GetFormatter<T>();
	}

	public void Add(IJitContainer container)
	{
		_containerList.Add(container);
	}

	public int AddReference(object obj)
	{
		if (_referenceDic.TryGetValue(obj, out var value))
		{
			return value;
		}
		SerializeData serializeData = new SerializeData();
		serializeData.Obj = obj;
		value = _referenceList.Count;
		_referenceList.Add(serializeData);
		_referenceDic.Add(obj, value);
		return value;
	}

	public byte[] SerializeReference<T>(T value) where T : class
	{
		return SimpleBufferWriter.DirectWriteByMsgPack(AddReference(value));
	}

	public static T EvaluateReference<T>(ByteSpan span, JitPacker packer, out ByteSpan nextSpan) where T : class
	{
		int evalIndex = span.ReadByMsgPack_Int32(0, out nextSpan);
		return packer.Evaluate<T>(evalIndex);
	}

	public static void SkipEvaluateReference(ByteSpan span, out ByteSpan nextSpan)
	{
		span.ReadByMsgPack_Int32(0, out nextSpan);
	}

	public byte[] DoSerialize(byte[] codeHash, byte[] tableHash)
	{
		_referenceDic.Clear();
		_referenceList.Clear();
		for (int i = 0; i < _containerList.Count; i++)
		{
			AddReference(_containerList[i]);
		}
		for (int j = 0; j < _referenceList.Count; j++)
		{
			SerializeData serializeData = _referenceList[j];
			IDataResolver dataResolver = ResolverContainer.Get(serializeData.Obj.GetType());
			serializeData.Bytes = dataResolver.SerializeObject(serializeData.Obj, this);
		}
		SerializeData serializeData2 = new SerializeData();
		serializeData2.Obj = StringSerializer;
		serializeData2.Bytes = StringSerializer.DoSerialize();
		_referenceList.Add(serializeData2);
		for (int k = 0; k < _referenceList.Count; k++)
		{
			_ = _referenceList[k];
		}
		using SimpleBufferWriter simpleBufferWriter = SimpleBufferWriter.CreateWriter();
		if (codeHash == null)
		{
			simpleBufferWriter.WriteInt(0);
		}
		else
		{
			simpleBufferWriter.WriteInt(codeHash.Length);
			simpleBufferWriter.WriteBytes(codeHash);
		}
		simpleBufferWriter.WriteInt(tableHash.Length);
		simpleBufferWriter.WriteBytes(tableHash);
		simpleBufferWriter.WriteInt(_referenceList.Count);
		int num = 0;
		for (int l = 0; l < _referenceList.Count; l++)
		{
			SerializeData serializeData3 = _referenceList[l];
			simpleBufferWriter.WriteInt(num);
			num += serializeData3.Bytes.Length;
		}
		for (int m = 0; m < _referenceList.Count; m++)
		{
			SerializeData serializeData4 = _referenceList[m];
			simpleBufferWriter.WriteBytes(serializeData4.Bytes);
		}
		return simpleBufferWriter.ToArray();
	}

	[Conditional("UNITY_EDITOR")]
	private void AddSerializedData(SerializeData data)
	{
	}
}
