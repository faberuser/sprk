using System.Collections.Generic;
using System.Text.RegularExpressions;
using NShared.NUtil;
using NVespa;

namespace NShared;

public class OperationValueExpression
{
	public delegate long VariableFuncDelegate(OperationInfo operationInfo, string key);

	public StatType[] StatTypes;

	public List<TokenInfo> RpnTokens { get; private set; }

	public VariableFuncDelegate VariableFunc { get; private set; }

	public static MatchCollection GetMatches(string stringValue)
	{
		string pattern = "\\$(?<FUNCTION>[a-zA-Z0-9]+).*\\((?<ARGUMENT>.*)\\)";
		return Regex.Matches(stringValue, pattern);
	}

	public static OperationValueExpression Create(string stringValue)
	{
		MatchCollection matches = GetMatches(stringValue);
		if (matches.Count == 0)
		{
			return null;
		}
		Match match = matches[0];
		string value = match.Groups["FUNCTION"].Value;
		string value2 = match.Groups["ARGUMENT"].Value;
		ExpressionData data = GameRoot.Instance.ExpressionDataContainer.GetData(value);
		if (data == null)
		{
			VespaDebug.LogError("Failed to find expression : " + value);
			return null;
		}
		string[] array = value2.Split(',', ';');
		Dictionary<string, long> constantArgumentDictionary = new Dictionary<string, long>();
		Dictionary<string, string> variableArgumentDictionary = new Dictionary<string, string>();
		for (int i = 0; i < array.Length; i++)
		{
			string key = "$" + (i + 1);
			string text = array[i].Trim();
			if (text.StartsWith("$"))
			{
				variableArgumentDictionary.Add(key, text);
				continue;
			}
			long result = 0L;
			long.TryParse(text, out result);
			constantArgumentDictionary.Add(key, result);
		}
		VariableFuncDelegate variableFunc = delegate(OperationInfo operationInfo, string text2)
		{
			long value3 = 0L;
			if (constantArgumentDictionary.TryGetValue(text2, out value3))
			{
				return value3;
			}
			string variable = text2;
			if (variableArgumentDictionary.ContainsKey(text2))
			{
				variable = variableArgumentDictionary[text2];
			}
			return CalculateVariable(variable, constantArgumentDictionary, operationInfo);
		};
		List<TokenInfo> rpnTokens = data.GetRpnTokens();
		StatType[] statTypes = data.GetStatTypes();
		return new OperationValueExpression
		{
			RpnTokens = rpnTokens,
			VariableFunc = variableFunc,
			StatTypes = statTypes
		};
	}

	private static long CalculateVariable(string variable, Dictionary<string, long> constantDictionary, OperationInfo operationInfo)
	{
		ParseVariableFuncAndArg(variable, out var funcName, out var arguments);
		if (arguments != null)
		{
			for (int i = 0; i < arguments.Length; i++)
			{
				string text = arguments[i];
				if (text != null && text.StartsWith("$") && constantDictionary.ContainsKey(text))
				{
					long num = constantDictionary[text];
					arguments[i] = num.ToString();
				}
			}
		}
		return OperationValueReaderFactory.Read(funcName, arguments, operationInfo);
	}

	private static void ParseVariableFuncAndArg(string variable, out string funcName, out string[] arguments)
	{
		funcName = variable;
		arguments = null;
		if (!string.IsNullOrEmpty(variable) && variable.EndsWith(")"))
		{
			int num = variable.IndexOf('(');
			if (num >= 0)
			{
				funcName = variable.Substring(0, num);
				string text = variable.Substring(num + 1, variable.Length - num - 2);
				arguments = text.Split(new char[1] { ',' });
			}
		}
	}
}
