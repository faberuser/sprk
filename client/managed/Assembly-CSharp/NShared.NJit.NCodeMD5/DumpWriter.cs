using System;
using System.Security.Cryptography;
using System.Text;
using Mono.Reflection;
using NShared.NJit.NCommon;

namespace NShared.NJit.NCodeMD5;

public class DumpWriter
{
	private const bool _enableLogText = false;

	private SimpleBufferWriter _bufferWriter;

	private StringBuilder _sb;

	public byte[] MD5Bytes { get; private set; }

	public string DumpText { get; private set; }

	public DumpWriter()
	{
		_bufferWriter = SimpleBufferWriter.CreateWriter();
		_sb = new StringBuilder();
	}

	public void EndDump()
	{
		using (MD5 mD = MD5.Create())
		{
			MD5Bytes = mD.ComputeHash(_bufferWriter.ToArray());
			_bufferWriter.Dispose();
			_bufferWriter = null;
		}
		DumpText = _sb.ToString();
		_sb = null;
	}

	public void Write(string value)
	{
		if (_bufferWriter == null || _sb == null)
		{
			JitOption.JitLogError("Invalid DumpWriter.Write: _bufferWriter == null || _sb == null");
		}
		else
		{
			_bufferWriter.WriteStringByMsgPack(value);
		}
	}

	public void Write(Instruction instruction)
	{
		if (_bufferWriter == null || _sb == null)
		{
			JitOption.JitLogError("Invalid DumpWriter.Write: _bufferWriter == null || _sb == null");
		}
		else
		{
			instruction.Dump(_bufferWriter);
		}
	}

	public void AppendWriter(DumpWriter childWriter)
	{
		if (_bufferWriter == null || _sb == null)
		{
			JitOption.JitLogError("Invalid DumpWriter.AppendWriter: _bufferWriter == null || _sb == null");
		}
		else if (childWriter.MD5Bytes == null || childWriter.DumpText == null)
		{
			JitOption.JitLogError("Invalid childWriter: childWriter._md5Bytes == null || childWriter._dumpText == null");
		}
		else
		{
			_bufferWriter.WriteBytes(childWriter.MD5Bytes);
		}
	}

	public void DumpTextToFile(Type type, string dumpDir, string namePostfix)
	{
	}
}
