using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;

namespace NVespa.NTask;

public class TaskUtility
{
	public class CallStackData
	{
		public string MethodName;

		public string FileName;

		public int LineNumber;

		public string GetDescription(bool shortDescription)
		{
			string arg = (shortDescription ? Path.GetFileName(FileName) : FileName);
			return $"{MethodName}: {arg}({LineNumber})";
		}
	}

	public static List<CallStackData> GetTaskCallStack()
	{
		List<CallStackData> list = new List<CallStackData>();
		StackTrace stackTrace = new StackTrace(fNeedFileInfo: true);
		MethodBase methodBase = null;
		for (int i = 0; i < stackTrace.FrameCount; i++)
		{
			StackFrame frame = stackTrace.GetFrame(i);
			MethodBase method = frame.GetMethod();
			if (methodBase != null)
			{
				if (__Extension_Method_ICollection__.COUNT(method.ReflectedType.GetCustomAttributes(typeof(IgnoreTaskStackAttribute), inherit: true)) <= 0)
				{
					CallStackData callStackData = new CallStackData();
					callStackData.MethodName = method.Name;
					callStackData.FileName = frame.GetFileName();
					callStackData.LineNumber = frame.GetFileLineNumber();
					list.Add(callStackData);
				}
			}
			else if (typeof(ITask).IsAssignableFrom(method.ReflectedType))
			{
				methodBase = method;
			}
		}
		return list;
	}

	public static string GetTaskCallerDescription()
	{
		StackTrace stackTrace = new StackTrace(fNeedFileInfo: true);
		MethodBase methodBase = null;
		for (int i = 0; i < stackTrace.FrameCount; i++)
		{
			StackFrame frame = stackTrace.GetFrame(i);
			MethodBase method = frame.GetMethod();
			if (methodBase != null)
			{
				if (__Extension_Method_ICollection__.COUNT(method.ReflectedType.GetCustomAttributes(typeof(IgnoreTaskStackAttribute), inherit: true)) <= 0)
				{
					StringBuilder stringBuilder = new StringBuilder();
					stringBuilder.Append(method.Name);
					stringBuilder.AppendLine();
					stringBuilder.Append("(");
					stringBuilder.Append(Path.GetFileName(frame.GetFileName()));
					stringBuilder.Append(":");
					stringBuilder.Append(frame.GetFileLineNumber());
					stringBuilder.Append(")");
					return stringBuilder.ToString();
				}
			}
			else if (typeof(ITask).IsAssignableFrom(method.ReflectedType))
			{
				methodBase = method;
			}
		}
		return null;
	}
}
