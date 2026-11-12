using System;
using System.Collections.Generic;
using NVespa;

namespace NShared.NUtil;

public class ExpressionCalculator
{
	private const string _plus = "+";

	private const string _minus = "-";

	private const string _multiply = "*";

	private const string _divide = "/";

	private const string _modulo = "%";

	private const string _exponent = "^";

	private const string _variablePrefix = "$";

	private static OperatorInfo _plusOperator = new OperatorInfo("+", 1);

	private static OperatorInfo _minusOperator = new OperatorInfo("-", 1);

	private static OperatorInfo _multiplyOperator = new OperatorInfo("*", 2);

	private static OperatorInfo _divideOperator = new OperatorInfo("/", 2);

	private static OperatorInfo _moduloOperator = new OperatorInfo("%", 2);

	private static OperatorInfo _exponentOperator = new OperatorInfo("^", 3);

	private static TokenInfo _leftParenthesis = new TokenInfo("(");

	private static TokenInfo _rightParenthesis = new TokenInfo(")");

	private static ConstantInfo _zeroConstant = new ConstantInfo(0L);

	private static ConstantPool _constantPool = new ConstantPool();

	private static OperatorInfo[] _operatorInfos = new OperatorInfo[6] { _plusOperator, _minusOperator, _multiplyOperator, _divideOperator, _moduloOperator, _exponentOperator };

	private static OperatorInfo GetOperatorInfo(string token)
	{
		for (int i = 0; i < _operatorInfos.Length; i++)
		{
			OperatorInfo operatorInfo = _operatorInfos[i];
			if (operatorInfo.Token == token)
			{
				return operatorInfo;
			}
		}
		return null;
	}

	private static List<string> TokenizeExpression(string inputExpression)
	{
		List<string> list = new List<string>();
		string text = "";
		bool flag = false;
		bool flag2 = false;
		for (int i = 0; i < inputExpression.Length; i++)
		{
			string text2 = inputExpression[i].ToString();
			if (text2 == "$")
			{
				flag = true;
				flag2 = false;
				text = "$";
			}
			else if (flag && text2 == _leftParenthesis.Token)
			{
				flag2 = true;
				text += text2;
			}
			else if (flag && flag2 && text2 == _rightParenthesis.Token)
			{
				flag = false;
				flag2 = false;
				text += text2;
			}
			else if (GetOperatorInfo(text2) != null || ((!flag || !flag2) && (text2 == _leftParenthesis.Token || text2 == _rightParenthesis.Token)))
			{
				text = text.Trim();
				if (!string.IsNullOrEmpty(text))
				{
					list.Add(text);
				}
				list.Add(text2);
				text = "";
				flag = false;
				flag2 = false;
			}
			else
			{
				text += text2;
			}
		}
		text = text.Trim();
		if (!string.IsNullOrEmpty(text))
		{
			list.Add(text);
		}
		return list;
	}

	private static List<TokenInfo> ToReversePolishNotation(IList<string> inputTokens)
	{
		List<TokenInfo> list = new List<TokenInfo>();
		Stack<TokenInfo> stack = new Stack<TokenInfo>();
		for (int i = 0; i < inputTokens.Count; i++)
		{
			string text = inputTokens[i];
			OperatorInfo operatorInfo = GetOperatorInfo(text);
			if (operatorInfo != null)
			{
				while (stack.Count > 0 && stack.Peek() is OperatorInfo operatorInfo2 && operatorInfo2.Precedence >= operatorInfo.Precedence)
				{
					list.Add(operatorInfo2);
					stack.Pop();
				}
				stack.Push(operatorInfo);
			}
			else if (text == _leftParenthesis.Token)
			{
				stack.Push(_leftParenthesis);
			}
			else if (text == _rightParenthesis.Token)
			{
				while (stack.Count > 0)
				{
					TokenInfo tokenInfo = stack.Pop();
					if (tokenInfo == _leftParenthesis)
					{
						break;
					}
					list.Add(tokenInfo);
				}
			}
			else if (text.StartsWith("$"))
			{
				list.Add(new TokenInfo(text));
			}
			else
			{
				long result = 0L;
				if (!long.TryParse(text, out result))
				{
					VespaDebug.LogError("Failed to parse to long : " + text);
				}
				list.Add(new ConstantInfo(result));
			}
		}
		while (stack.Count > 0)
		{
			TokenInfo item = stack.Pop();
			list.Add(item);
		}
		return list;
	}

	private static ConstantInfo Calculate(ConstantInfo lhs, ConstantInfo rhs, TokenInfo operatorInfo, ConstantPool pool)
	{
		switch (operatorInfo.Token)
		{
		case "+":
			return pool.Alloc(lhs.Value + rhs.Value);
		case "-":
			return pool.Alloc(lhs.Value - rhs.Value);
		case "*":
			return pool.Alloc(lhs.Value * rhs.Value);
		case "/":
			return pool.Alloc((rhs.Value != 0L) ? (lhs.Value / rhs.Value) : 0);
		case "%":
			return pool.Alloc(lhs.Value % rhs.Value);
		case "^":
			return pool.Alloc((long)System.Math.Pow(lhs.Value, rhs.Value));
		default:
			VespaDebug.LogError("Unsupported operator : " + operatorInfo.Token);
			return _zeroConstant;
		}
	}

	private static ConstantInfo ToConstant(TokenInfo tokenInfo, Func<string, long> variableFunc, ConstantPool pool)
	{
		if (tokenInfo == null)
		{
			return _zeroConstant;
		}
		if (tokenInfo is ConstantInfo)
		{
			return tokenInfo as ConstantInfo;
		}
		long value = variableFunc(tokenInfo.Token);
		return pool.Alloc(value);
	}

	public static List<TokenInfo> CreateReversePolishNotation(string expression)
	{
		return ToReversePolishNotation(TokenizeExpression(expression));
	}

	public static long Calculate(IList<TokenInfo> rpnTokens, Func<string, long> variableFunc)
	{
		Stack<TokenInfo> stack = new Stack<TokenInfo>();
		for (int i = 0; i < rpnTokens.Count; i++)
		{
			TokenInfo tokenInfo = rpnTokens[i];
			if (tokenInfo is OperatorInfo operatorInfo)
			{
				if (stack.Count >= 2)
				{
					ConstantInfo constantInfo = ToConstant(stack.Pop(), variableFunc, _constantPool);
					ConstantInfo constantInfo2 = ToConstant(stack.Pop(), variableFunc, _constantPool);
					ConstantInfo item = Calculate(constantInfo2, constantInfo, operatorInfo, _constantPool);
					stack.Push(item);
					_constantPool.Release(constantInfo);
					_constantPool.Release(constantInfo2);
				}
				else if (stack.Count == 1)
				{
					if (operatorInfo == _minusOperator)
					{
						ConstantInfo constantInfo3 = ToConstant(stack.Pop(), variableFunc, _constantPool);
						ConstantInfo item2 = Calculate(_zeroConstant, constantInfo3, operatorInfo, _constantPool);
						stack.Push(item2);
						_constantPool.Release(constantInfo3);
					}
					else
					{
						VespaDebug.LogError("Invalid unary op");
					}
				}
				else
				{
					VespaDebug.LogError("Empty stack");
				}
			}
			else
			{
				stack.Push(tokenInfo);
			}
		}
		if (!(stack.Pop() is ConstantInfo { Value: var value } constantInfo4))
		{
			return 0L;
		}
		_constantPool.Release(constantInfo4);
		return value;
	}
}
