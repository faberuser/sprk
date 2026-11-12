using System;
using System.Diagnostics;
using System.Text;
using UnityEngine;

namespace NVespa;

public static class VLogMessage
{
	public interface ILogMessage
	{
		VLog.LogLevel LogLevel { get; }
	}

	public abstract class LogMessage<T> : ILogMessage where T : LogMessage<T>, new()
	{
		public const char ValueDeliminator = '.';

		public static readonly string DebugTag = Vespa_String_Extension_Method.ColorTag("[D]", Color.gray);

		public static readonly string InfoTag = Vespa_String_Extension_Method.ColorTag("[I]", Color.green);

		public static readonly string WarnTag = Vespa_String_Extension_Method.ColorTag("[W]", Color.yellow);

		public static readonly string ErrorTag = Vespa_String_Extension_Method.ColorTag("[E]", Color.red);

		public static readonly string FatalTag = Vespa_String_Extension_Method.ColorTag("[F]", new Color(0.36862746f, 3f / 85f, 0.101960786f));

		[ThreadStatic]
		private static T _instance;

		private readonly StringBuilder _builder = new StringBuilder();

		public abstract VLog.LogLevel LogLevel { get; }

		internal static T Init(string category)
		{
			if (_instance == null)
			{
				_instance = new T();
			}
			_instance.SetCategory(category);
			return _instance;
		}

		private void SetCategory(string category)
		{
			if (_builder.Length > 0)
			{
				Clear();
			}
			switch (LogLevel)
			{
			case VLog.LogLevel.Debug:
				_builder.Append(DebugTag);
				break;
			case VLog.LogLevel.Info:
				_builder.Append(InfoTag);
				break;
			case VLog.LogLevel.Warn:
				_builder.Append(WarnTag);
				break;
			case VLog.LogLevel.Error:
				_builder.Append(ErrorTag);
				break;
			case VLog.LogLevel.Fatal:
				_builder.Append(FatalTag);
				break;
			}
			if (!string.IsNullOrEmpty(category))
			{
				_builder.Append("[");
				_builder.Append(category);
				_builder.Append("]");
			}
		}

		private void Clear()
		{
			_builder.Clear();
		}

		private bool CheckCondition()
		{
			int minimumLevel = (int)VLog.MinimumLevel;
			if ((int)LogLevel < minimumLevel)
			{
				return false;
			}
			return true;
		}

		[MustUseReturnValue]
		public T Append<TValue>(TValue value, bool appendDeliminator = true)
		{
			if (_builder.Length > 0)
			{
				_builder.Append(" ");
			}
			_builder.Append(value);
			if (appendDeliminator)
			{
				_builder.Append('.');
			}
			return _instance;
		}

		[MustUseReturnValue]
		public T Append<TValue>(string key, TValue value, bool appendDeliminator = true)
		{
			if (_builder.Length > 0)
			{
				_builder.Append(" ");
			}
			_builder.Append(key);
			_builder.Append("=");
			_builder.Append(value);
			if (appendDeliminator)
			{
				_builder.Append('.');
			}
			return _instance;
		}

		[MustUseReturnValue]
		public T AppendFormat(string format, params object[] args)
		{
			if (_builder.Length > 0)
			{
				_builder.Append(" ");
			}
			_builder.AppendFormat(format, args);
			_builder.Append('.');
			return _instance;
		}

		[MustUseReturnValue]
		public T AppendLine()
		{
			_builder.AppendLine();
			return _instance;
		}

		public override string ToString()
		{
			return _builder.ToString();
		}

		protected void PrintInternal()
		{
			if (CheckCondition())
			{
				VLog.Print(this);
				Clear();
			}
		}
	}

	public class DebugMessage : LogMessage<DebugMessage>
	{
		public override VLog.LogLevel LogLevel => VLog.LogLevel.Debug;

		[Conditional("LOG_DEBUG")]
		public void Print()
		{
			PrintInternal();
		}
	}

	public class InfoMessage : LogMessage<InfoMessage>
	{
		public override VLog.LogLevel LogLevel => VLog.LogLevel.Info;

		[Conditional("LOG_INFO")]
		public void Print()
		{
			PrintInternal();
		}
	}

	public class WarnMessage : LogMessage<WarnMessage>
	{
		public override VLog.LogLevel LogLevel => VLog.LogLevel.Warn;

		[Conditional("LOG_WARN")]
		public void Print()
		{
			PrintInternal();
		}
	}

	public class ErrorMessage : LogMessage<ErrorMessage>
	{
		public override VLog.LogLevel LogLevel => VLog.LogLevel.Error;

		[Conditional("LOG_ERROR")]
		public void Print()
		{
			PrintInternal();
		}
	}

	public class FatalMessage : LogMessage<FatalMessage>
	{
		public override VLog.LogLevel LogLevel => VLog.LogLevel.Fatal;

		[Conditional("LOG_FATAL")]
		public void Print()
		{
			PrintInternal();
		}
	}
}
