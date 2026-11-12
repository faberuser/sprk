using System.Collections.Generic;
using MessagePack;
using NShared.NOperationValueReaderRuntime;
using NShared.NUtil;

namespace NShared;

[MessagePackObject(false)]
public class ExpressionData : IDataKeySelector<int>
{
	private List<TokenInfo> _cachedRpnTokens;

	private StatType[] _chacedStatTypes;

	private string[] _chachedStateTypes;

	private bool _isExpressionInitilize;

	int IDataKeySelector<int>.Key => Index;

	[Key(0)]
	public int Index { get; set; }

	[Key(1)]
	public string Name { get; set; }

	[Key(2)]
	public string Expression { get; set; }

	public override string ToString()
	{
		return Name + "(" + Index + ")";
	}

	public void SetExpressionData()
	{
		lock (this)
		{
			if (!_isExpressionInitilize)
			{
				_cachedRpnTokens = ExpressionCalculator.CreateReversePolishNotation(Expression);
				SetStatType();
				_isExpressionInitilize = true;
			}
		}
	}

	public List<TokenInfo> GetRpnTokens()
	{
		SetExpressionData();
		return _cachedRpnTokens;
	}

	public StatType[] GetStatTypes()
	{
		SetExpressionData();
		return _chacedStatTypes;
	}

	public string[] GetStateTypes()
	{
		SetExpressionData();
		return _chachedStateTypes;
	}

	private void SetStatType()
	{
		if (_cachedRpnTokens == null)
		{
			return;
		}
		List<StatType> list = new List<StatType>();
		List<string> list2 = new List<string>();
		foreach (TokenInfo cachedRpnToken in _cachedRpnTokens)
		{
			if (!cachedRpnToken.ToString().Contains("$"))
			{
				continue;
			}
			string text = cachedRpnToken.ToString();
			if (text.Contains("("))
			{
				text = text[..text.IndexOf("(")];
			}
			IOperationValueReader operationValueReader = OperationValueReaderFactory.GetOperationValueReader(text);
			if (operationValueReader == null)
			{
				continue;
			}
			if (operationValueReader is IStatReader statReader)
			{
				StatType statType = statReader.GetStatType();
				if (statType != StatType.None && !list.Contains(statType))
				{
					list.Add(statType);
				}
			}
			else if (operationValueReader is IStateReader stateReader)
			{
				string state = stateReader.GetState();
				if (!string.IsNullOrEmpty(state) && !list2.Contains(state))
				{
					list2.Add(state);
				}
			}
		}
		if (list.Count > 0)
		{
			_chacedStatTypes = list.ToArray();
		}
		if (list2.Count > 0)
		{
			_chachedStateTypes = list2.ToArray();
		}
	}
}
