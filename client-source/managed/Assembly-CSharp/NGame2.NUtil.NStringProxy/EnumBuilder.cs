using System;
using NVespa.NGlobal;
using UnityEngine;

namespace NGame2.NUtil.NStringProxy;

public class EnumBuilder : IBuilder
{
	private Enum _key;

	private string _directKey;

	private object[] _args;

	private string _tag;

	private Tuple<bool, Color> _encodeColor;

	private bool _appendLine;

	private bool _useDirectKey;

	bool IBuilder.AppendLine => _appendLine;

	public EnumBuilder Key(Enum key)
	{
		_key = key;
		_useDirectKey = false;
		return this;
	}

	public EnumBuilder DirectKey(string key)
	{
		_directKey = key;
		_useDirectKey = true;
		return this;
	}

	public EnumBuilder Params(params object[] args)
	{
		_args = args;
		return this;
	}

	public EnumBuilder AppendLine()
	{
		_appendLine = true;
		return this;
	}

	public EnumBuilder NguiTag(string tag)
	{
		_tag = tag;
		return this;
	}

	public EnumBuilder EncodeColor(Color color)
	{
		_encodeColor = Tuple.Create(i_item1: true, color);
		return this;
	}

	public string Build()
	{
		string text = ((!__Extension_Method_ICollection__.IsNullOrEmpty(_args)) ? (_useDirectKey ? LocalizationManager.GetStringDirect(_directKey, _args) : LocalizationManager.GetString(_key, _args)) : (_useDirectKey ? LocalizationManager.GetStringDirect(_directKey) : LocalizationManager.GetString(_key)));
		if (!String_Extension_Method.IsNullOrEmpty(_tag))
		{
			text = $"{_tag}{text}" + "[-]";
		}
		if (_encodeColor != null && _encodeColor.Item1)
		{
			text = NGUIText.EncodeColor(text, _encodeColor.Item2);
		}
		return text;
	}
}
