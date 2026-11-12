using System;
using System.Collections.Generic;
using NVespa;

namespace NShared;

public class SnapshotWritableBlock : IDisposable
{
	private static bool _checkSnapshotWritable = false;

	private static object _writableBlockListSync = new object();

	private static List<SnapshotWritableBlock> _writableBlockList = new List<SnapshotWritableBlock>();

	public SnapshotWritableBlock()
	{
		_writableBlockList.Add(this);
	}

	void IDisposable.Dispose()
	{
		_writableBlockList.Remove(this);
	}

	public static void BeginCheckSnapshotWritable()
	{
		_checkSnapshotWritable = true;
	}

	public static void EndCheckSnapshotWritable()
	{
		_checkSnapshotWritable = false;
	}

	public static void CheckSnapshotWritable()
	{
		if (_checkSnapshotWritable && _writableBlockList.Count == 0)
		{
			VespaDebug.LogError("Snapshot is not writable.");
		}
	}
}
