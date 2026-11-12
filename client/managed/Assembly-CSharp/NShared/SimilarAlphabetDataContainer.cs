using System;
using NShared.NJit;

namespace NShared;

public class SimilarAlphabetDataContainer : JitDataContainerDefault<char, SimilarAlphabetData>
{
	public const string TABLENAME = "SimilarAlphabetTable";

	public override TablePathDefine PathDefine => new TablePathDefine("SimilarAlphabetTable");

	public bool IsSimilarWordCheck(string word, string checkWord, bool isIncludeCheck = false)
	{
		if (!isIncludeCheck && checkWord.Length != word.Length)
		{
			return false;
		}
		string text = checkWord.ToUpper();
		bool flag = false;
		int num = 0;
		int num2 = 0;
		char alphabet = word[num];
		char key = text[num2];
		while (num < word.Length)
		{
			SimilarAlphabetData data = GetData(key);
			if (data == null)
			{
				num++;
			}
			else if (Array.Exists(data.SimilarAlphabet, (char c) => c == alphabet))
			{
				if (!flag)
				{
					flag = true;
				}
				if (num2 + 1 >= text.Length || num + 1 >= word.Length)
				{
					break;
				}
				alphabet = word[++num];
				key = text[++num2];
			}
			else if (flag)
			{
				flag = false;
				num2 = 0;
				key = text[num2];
			}
			else
			{
				if (num + 1 >= word.Length)
				{
					break;
				}
				alphabet = word[++num];
			}
		}
		if (num2 >= text.Length)
		{
			return true;
		}
		return false;
	}
}
