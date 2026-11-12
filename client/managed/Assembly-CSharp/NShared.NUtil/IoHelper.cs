using System;
using System.IO;

namespace NShared.NUtil;

public class IoHelper
{
	public static void ClearDirectoryRecursive(string directoryName)
	{
		DirectoryInfo directoryInfo = new DirectoryInfo(directoryName);
		try
		{
			FileInfo[] files = directoryInfo.GetFiles();
			foreach (FileInfo obj in files)
			{
				obj.IsReadOnly = false;
				obj.Delete();
			}
		}
		catch (Exception)
		{
		}
		DirectoryInfo[] directories = directoryInfo.GetDirectories();
		foreach (DirectoryInfo directoryInfo2 in directories)
		{
			ClearDirectoryRecursive(directoryInfo2.FullName);
			try
			{
				directoryInfo2.Delete();
			}
			catch (Exception)
			{
			}
		}
	}
}
