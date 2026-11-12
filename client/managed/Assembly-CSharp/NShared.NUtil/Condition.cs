using System;
using System.Collections.Generic;

namespace NShared.NUtil;

public class Condition
{
	public class CompareTokenInfo
	{
		public CheckType CheckType { get; set; }

		public string CompareToken { get; set; }
	}

	public static Dictionary<string, Condition> _activeConditions = new Dictionary<string, Condition>();

	public string ConditionString;

	public string AttributeName;

	public CheckType CheckType;

	public string CompareValue;

	private static CompareTokenInfo[] CompareTokenInfos = new CompareTokenInfo[6]
	{
		new CompareTokenInfo
		{
			CheckType = CheckType.NotEqual,
			CompareToken = "!="
		},
		new CompareTokenInfo
		{
			CheckType = CheckType.Equal,
			CompareToken = "=="
		},
		new CompareTokenInfo
		{
			CheckType = CheckType.MoreOrEqual,
			CompareToken = ">="
		},
		new CompareTokenInfo
		{
			CheckType = CheckType.More,
			CompareToken = ">"
		},
		new CompareTokenInfo
		{
			CheckType = CheckType.LessOrEqual,
			CompareToken = "<="
		},
		new CompareTokenInfo
		{
			CheckType = CheckType.Less,
			CompareToken = "<"
		}
	};

	public Condition()
	{
	}

	public Condition(string condition, string attributeName, CheckType checkType, string compareValue)
	{
		ConditionString = condition;
		AttributeName = attributeName;
		CheckType = checkType;
		CompareValue = compareValue;
	}

	public Condition(string condition, string attributeName, string compareValue)
		: this(condition, attributeName, CheckType.Equal, compareValue)
	{
	}

	private static bool CheckCompareResult(CheckType checkType, int compareResult)
	{
		return checkType switch
		{
			CheckType.Equal => compareResult == 0, 
			CheckType.NotEqual => compareResult != 0, 
			CheckType.More => compareResult > 0, 
			CheckType.MoreOrEqual => compareResult >= 0, 
			CheckType.Less => compareResult < 0, 
			CheckType.LessOrEqual => compareResult <= 0, 
			_ => false, 
		};
	}

	public static bool CheckCompareResult<T>(CheckType checkType, T checkValue, T testValue) where T : IComparable
	{
		if (checkValue == null && testValue == null && checkType == CheckType.Equal)
		{
			return false;
		}
		if (checkValue == null)
		{
			return false;
		}
		if (testValue == null)
		{
			return false;
		}
		return CheckCompareResult(checkType, testValue.CompareTo(checkValue));
	}

	public static Condition GetCondition(string conditionString)
	{
		lock (_activeConditions)
		{
			if (string.IsNullOrEmpty(conditionString) || conditionString.ToLower() == "none")
			{
				return null;
			}
			if (!_activeConditions.ContainsKey(conditionString))
			{
				Condition condition = CreateFromString(conditionString);
				if (condition == null)
				{
					return null;
				}
				_activeConditions.Add(conditionString, condition);
			}
			return _activeConditions[conditionString];
		}
	}

	public static Condition CreateFromString(string conditionString)
	{
		CompareTokenInfo[] compareTokenInfos = CompareTokenInfos;
		foreach (CompareTokenInfo compareTokenInfo in compareTokenInfos)
		{
			int num = conditionString.IndexOf(compareTokenInfo.CompareToken);
			if (num >= 0)
			{
				return new Condition
				{
					ConditionString = conditionString,
					CheckType = compareTokenInfo.CheckType,
					AttributeName = conditionString.Substring(0, num).Trim(),
					CompareValue = conditionString.Substring(num + compareTokenInfo.CompareToken.Length).Trim()
				};
			}
		}
		return null;
	}

	public override string ToString()
	{
		CompareTokenInfo[] compareTokenInfos = CompareTokenInfos;
		foreach (CompareTokenInfo compareTokenInfo in compareTokenInfos)
		{
			if (compareTokenInfo.CheckType == CheckType)
			{
				return AttributeName + compareTokenInfo.CompareToken + CompareValue;
			}
		}
		return base.ToString();
	}
}
