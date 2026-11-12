using System;
using System.IO;

namespace NShared.NJit;

public class JitSyncLoader
{
	public byte[] Load(TablePathDefine pathDefine)
	{
		byte[] bytes = PreloadInAppBytes.GetBytes(pathDefine.Path);
		if (bytes != null)
		{
			return bytes;
		}
		string jitTablePath = pathDefine.JitTablePath;
		if (!File.Exists(jitTablePath))
		{
			return null;
		}
		try
		{
			return File.ReadAllBytes(jitTablePath);
		}
		catch (Exception ex)
		{
			JitOption.JitLogError("Failed to load jit file : " + ex);
			return null;
		}
	}
}
