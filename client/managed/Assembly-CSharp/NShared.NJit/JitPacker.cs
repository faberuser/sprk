using System.Collections.Generic;
using System.IO;
using MessagePack;
using MessagePack.Formatters;
using NShared.NJit.NCommon;
using NShared.NJit.NResolver;

namespace NShared.NJit;

public class JitPacker : IFormatterResolver
{
	public struct PackData
	{
		public int Count;

		public ByteSpan OffsetSpan;

		public ByteSpan DataSpan;

		public ByteSpan GetReferenceSpan(int refIndex)
		{
			if (refIndex >= Count)
			{
				return default(ByteSpan);
			}
			int index = OffsetSpan.ReadInt(4 * refIndex);
			return DataSpan.Slice(index);
		}
	}

	public struct MD5Hash
	{
		public int Length;

		public ByteSpan HashSpan;

		public void Read(ByteSpan span, out ByteSpan nextSpan)
		{
			Length = span.ReadInt(0);
			HashSpan = span.Slice(4);
			nextSpan = HashSpan.Slice(Length);
		}
	}

	private List<IJitContainer> _containerList = new List<IJitContainer>();

	private MD5Hash _tableMD5;

	private MD5Hash _codeMD5;

	private ByteSpan _rootSpan;

	private PackData _packData;

	private Dictionary<int, object> _evalDic = new Dictionary<int, object>();

	public MessagePackSerializerOptions MessagePackSerializerOptions { get; private set; }

	public JitStringDeserializer StringDeserializer { get; private set; }

	public JitPacker()
	{
		MessagePackInitializer.RegisterResolver();
		MessagePackSerializerOptions = new MessagePackSerializerOptions(this);
		StringDeserializer = new JitStringDeserializer();
	}

	IMessagePackFormatter<T> IFormatterResolver.GetFormatter<T>()
	{
		if (StringDeserializer is IMessagePackFormatter<T> result)
		{
			return result;
		}
		return MessagePackSerializer.DefaultOptions.Resolver.GetFormatter<T>();
	}

	public void Reset()
	{
		_rootSpan = default(ByteSpan);
		_packData = default(PackData);
		_evalDic.Clear();
		for (int i = 0; i < _containerList.Count; i++)
		{
			_containerList[i].Reset();
		}
		StringDeserializer.Reset();
	}

	public void RegisterAndBind(IJitContainer container)
	{
		_containerList.Add(container);
		container.Bind(this);
	}

	public bool CheckMD5(byte[] md5HashInput, bool isTable)
	{
		if (md5HashInput == null)
		{
			return false;
		}
		MD5Hash mD5Hash = (isTable ? _tableMD5 : _codeMD5);
		if (md5HashInput.Length != mD5Hash.Length)
		{
			return false;
		}
		for (int i = 0; i < mD5Hash.Length; i++)
		{
			if (mD5Hash.HashSpan.ReadByte(i) != md5HashInput[i])
			{
				return false;
			}
		}
		return true;
	}

	public void Deserialize(byte[] bytes)
	{
		ByteSpan readSpan = new ByteSpan(bytes);
		Deserialize(readSpan);
	}

	public void Deserialize(ByteSpan readSpan)
	{
		lock (_evalDic)
		{
			_evalDic.Clear();
			for (int i = 0; i < _containerList.Count; i++)
			{
				_evalDic.Add(i, _containerList[i]);
			}
			_codeMD5.Read(readSpan, out readSpan);
			_tableMD5.Read(readSpan, out readSpan);
			_rootSpan = readSpan;
			_packData.Count = _rootSpan.ReadInt(0);
			_packData.OffsetSpan = _rootSpan.Slice(4);
			_packData.DataSpan = _packData.OffsetSpan.Slice(4 * _packData.Count);
			for (int j = 0; j < _containerList.Count; j++)
			{
				IJitContainer jitContainer = _containerList[j];
				ByteSpan referenceSpan = _packData.GetReferenceSpan(j);
				jitContainer.Deserialize(referenceSpan);
			}
			ByteSpan referenceSpan2 = _packData.GetReferenceSpan(_packData.Count - 1);
			StringDeserializer.Init(referenceSpan2);
		}
	}

	public void Serialize(TablePathDefine pathDefine, byte[] tableHash, byte[] codeHash)
	{
		string jitTablePath = pathDefine.JitTablePath;
		byte[] bytes = SerializeBytes(jitTablePath, tableHash, codeHash, pathDefine.Version);
		string directoryName = Path.GetDirectoryName(jitTablePath);
		if (!Directory.Exists(directoryName))
		{
			Directory.CreateDirectory(directoryName);
		}
		File.WriteAllBytes(jitTablePath, bytes);
	}

	public byte[] SerializeBytes(string jitTablePath, byte[] tableHash, byte[] codeHash, int version)
	{
		JitSerializer jitSerializer = new JitSerializer();
		for (int i = 0; i < _containerList.Count; i++)
		{
			jitSerializer.Add(_containerList[i]);
		}
		return jitSerializer.DoSerialize(codeHash, tableHash);
	}

	public V Evaluate<V>(int evalIndex) where V : class
	{
		lock (_evalDic)
		{
			if (_evalDic.TryGetValue(evalIndex, out var value))
			{
				return value as V;
			}
			ByteSpan referenceSpan = _packData.GetReferenceSpan(evalIndex);
			IDataResolverT<V> dataResolver = Proxy<V>.DataResolver;
			V val = dataResolver.Evaluate(referenceSpan, this);
			_evalDic.Add(evalIndex, val);
			dataResolver.EvaluateLater(val, referenceSpan, this);
			return val;
		}
	}
}
