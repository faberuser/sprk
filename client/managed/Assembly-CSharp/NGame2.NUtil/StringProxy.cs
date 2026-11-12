using System.Collections.Generic;
using System.Text;
using NGame2.NUtil.NStringProxy;

namespace NGame2.NUtil;

public class StringProxy
{
	private List<IBuilder> _builderList;

	private string _value;

	private bool _trimLastNewLine = true;

	public string Value
	{
		get
		{
			if (_value == null)
			{
				Build();
			}
			return _value;
		}
		set
		{
			Assign(new NGame2.NUtil.NStringProxy.StringBuilder(value, appendLine: false));
		}
	}

	public override string ToString()
	{
		return Value;
	}

	public void Reset()
	{
		_value = null;
		if (_builderList != null)
		{
			_builderList.Clear();
		}
	}

	public void Assign(IBuilder stringBuilder)
	{
		if (stringBuilder != null)
		{
			if (_builderList == null)
			{
				_builderList = new List<IBuilder>();
			}
			else
			{
				_builderList.Clear();
			}
			_builderList.Add(stringBuilder);
			_value = null;
		}
	}

	public void Append(IBuilder stringBuilder)
	{
		if (stringBuilder != null)
		{
			if (_builderList == null)
			{
				_builderList = new List<IBuilder>();
			}
			_builderList.Add(stringBuilder);
			_value = null;
		}
	}

	private void Build()
	{
		if (__Extension_Method_ICollection__.IsNullOrEmpty(_builderList))
		{
			_value = string.Empty;
			return;
		}
		System.Text.StringBuilder stringBuilder = new System.Text.StringBuilder();
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_builderList); i++)
		{
			IBuilder builder = _builderList[i];
			stringBuilder.Append(builder.Build());
			if (builder.AppendLine)
			{
				stringBuilder.AppendLine();
			}
		}
		_value = stringBuilder.ToString();
		if (_trimLastNewLine)
		{
			_value = _value.TrimEnd('\r', '\n');
		}
	}
}
