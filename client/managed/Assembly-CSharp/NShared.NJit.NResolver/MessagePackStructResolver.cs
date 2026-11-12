using System;
using MessagePack;
using NShared.NJit.NCommon;

namespace NShared.NJit.NResolver;

public class MessagePackStructResolver<T> : IDataResolverT<T>, IDataResolver where T : struct
{
	Type IDataResolver.GetResolveType()
	{
		return typeof(T);
	}

	byte[] IDataResolver.SerializeObject(object obj, JitSerializer jitSerializer)
	{
		if (!(obj is T))
		{
			return SimpleBufferWriter.ErrorWrite(0);
		}
		return SerializeValue((T)obj, jitSerializer);
	}

	public byte[] SerializeValue(T value, JitSerializer jitSerializer)
	{
		using SimpleBufferWriter simpleBufferWriter = SimpleBufferWriter.CreateWriter();
		byte[] array = MessagePackSerializer.Serialize(value, jitSerializer.MessagePackSerializerOptions);
		simpleBufferWriter.WriteInt32ByMsgPack(array.Length);
		simpleBufferWriter.WriteBytes(array);
		return simpleBufferWriter.ToArray();
	}

	public byte[] SerializeReference(T value, JitSerializer jitSerializer)
	{
		return SerializeValue(value, jitSerializer);
	}

	public T Evaluate(ByteSpan span, JitPacker packer)
	{
		ByteSpan nextSpan;
		return EvaluateReference(span, packer, out nextSpan);
	}

	public void EvaluateLater(T target, ByteSpan span, JitPacker packer)
	{
	}

	public T EvaluateReference(ByteSpan span, JitPacker packer, out ByteSpan nextSpan)
	{
		ByteSpan nextSpan2;
		int index = span.ReadByMsgPack_Int32(0, out nextSpan2);
		ArraySegment<byte> arraySegment = nextSpan2.SliceSegment(0);
		nextSpan = nextSpan2.Slice(index);
		return MessagePackSerializer.Deserialize<T>(arraySegment, packer.MessagePackSerializerOptions);
	}

	public void SkipEvaluateReference(ByteSpan span, out ByteSpan nextSpan)
	{
		ByteSpan nextSpan2;
		int index = span.ReadByMsgPack_Int32(0, out nextSpan2);
		nextSpan = nextSpan2.Slice(index);
	}
}
