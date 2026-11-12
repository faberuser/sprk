using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using NShared.NJit;

namespace NShared;

public class CensoredWordDataContainer : JitDataContainerDefault<string, CensoredWordData>
{
	public class CensoredRegex
	{
		public CensoredWordData Data;

		public Regex Regex;
	}

	public enum CensoredResult
	{
		None,
		Censored,
		Sensitive
	}

	public const string TABLENAME = "CensoredWordTable";

	private const char CensoredChar = '*';

	private const string WholeWordPattern = "\\b({0})(s?)\\b";

	private const string NormalWordPattern = "({0})";

	private const RegexOptions Options = RegexOptions.IgnoreCase | RegexOptions.CultureInvariant;

	private JitRef<List<CensoredWordData>> _similarCheckWordList = new JitRef<List<CensoredWordData>>();

	private string[] _censoredReplacements;

	private List<CensoredRegex> _censoredRegex = new List<CensoredRegex>();

	public override TablePathDefine PathDefine => new TablePathDefine("CensoredWordTable");

	public CensoredWordDataContainer()
	{
		base.Packer.RegisterAndBind(_similarCheckWordList);
	}

	protected override void OnLoadingValue_from_JsonData(IDictionary jsonDic, CensoredWordData value)
	{
		value.Word = Regex.Escape(value.Word);
	}

	protected override void OnEndLoadFromJsonData()
	{
		foreach (CensoredWordData item in base.BaseList.JsonLoad_GetOrNew())
		{
			if (item.SimilarCheck)
			{
				_similarCheckWordList.JsonLoad_GetOrNew().Add(item);
			}
		}
	}

	protected override void OnEndLoad_Final()
	{
		int num = 0;
		foreach (CensoredWordData allDatum in GetAllData())
		{
			num = Math.Max(allDatum.Word.Length, num);
			CensoredRegex item = new CensoredRegex
			{
				Regex = CreateRegex(allDatum),
				Data = allDatum
			};
			_censoredRegex.Add(item);
		}
		_censoredReplacements = new string[num];
		for (int i = 0; i < num; i++)
		{
			_censoredReplacements[i] = new string('*', i);
		}
	}

	private Regex CreateRegex(CensoredWordData data)
	{
		string word = data.Word;
		if (!data.MatchWholeWord)
		{
			return new Regex($"({word})", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
		}
		return new Regex($"\\b({word})(s?)\\b", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
	}

	public CensoredResult IsCensoredWord(string text, bool isChat = false)
	{
		if (string.IsNullOrEmpty(text))
		{
			return CensoredResult.None;
		}
		foreach (CensoredRegex item in _censoredRegex)
		{
			if ((!isChat || !item.Data.AllowChat) && item.Regex != null && item.Regex.IsMatch(text))
			{
				if (item.Data != null && item.Data.SensitiveWord)
				{
					return CensoredResult.Sensitive;
				}
				return CensoredResult.Censored;
			}
		}
		return CensoredResult.None;
	}

	public string ReplaceCensoredWord(string text, bool isChat)
	{
		if (string.IsNullOrEmpty(text))
		{
			return text;
		}
		List<string> list = new List<string>();
		foreach (CensoredRegex item2 in _censoredRegex)
		{
			if (item2.Data != null && (!isChat || !item2.Data.AllowChat) && item2.Regex.IsMatch(text))
			{
				int length = item2.Data.Word.Length;
				string item = item2.Regex.Replace(text, (length < _censoredReplacements.Length) ? _censoredReplacements[length] : _censoredReplacements[_censoredReplacements.Length - 1]);
				list.Add(item);
			}
		}
		if (list.Count <= 0)
		{
			return text;
		}
		if (list.Count == 1)
		{
			return list[0];
		}
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < text.Length; i++)
		{
			char value = text[i];
			foreach (string item3 in list)
			{
				if (item3[i].CompareTo('*') == 0)
				{
					value = '*';
					break;
				}
			}
			stringBuilder.Append(value);
		}
		return stringBuilder.ToString();
	}

	public IList<CensoredWordData> GetSimilarCheckWordList()
	{
		return _similarCheckWordList.Eval();
	}
}
