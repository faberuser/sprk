using System.Collections.Generic;
using LitJson;
using NGame2.NBattleField;
using NVespa;

namespace NGame2.NCutScene;

public class ScriptGroup : IBuilder, ILoadJson, IJsonInterface, IMakeJson
{
	public enum ETouchType
	{
		Timeout,
		TimeoutOrTouch,
		Touch,
		TouchAndTouch,
		WaitNodeTree,
		PassToScript,
		WaitGroupPlay
	}

	public enum ESkipType
	{
		Default,
		StopThis,
		StopThisAndHide
	}

	public List<ScriptHolder> ScriptList = new List<ScriptHolder>();

	public int TouchTickLength;

	public ETouchType TouchType = ETouchType.Touch;

	public ESkipType SkipType;

	private ILocalTimer _localTimer;

	private int _elapsedTick;

	private List<ScriptHolder> _readyList = new List<ScriptHolder>();

	private List<ScriptHolder> _playingList = new List<ScriptHolder>();

	public bool IsEnd { get; private set; }

	public ScriptGroup(ILocalTimer localTimer)
	{
		_localTimer = localTimer;
	}

	public void Visit(IVisitor visitor)
	{
		for (int i = 0; i < ScriptList.Count; i++)
		{
			if (ScriptList[i].Value != null)
			{
				ScriptList[i].Value.Visit(visitor);
			}
		}
	}

	public void Begin()
	{
		IsEnd = false;
		_readyList.Clear();
		_playingList.Clear();
		_readyList.AddRange(ScriptList);
		_elapsedTick = 0;
		ScriptList.ForEach(delegate(ScriptHolder script)
		{
			script.Value.Reset();
		});
	}

	public void Update()
	{
		_elapsedTick += _localTimer.deltaTick;
		for (int i = 0; i < _readyList.Count; i++)
		{
			ScriptHolder scriptHolder = _readyList[i];
			if (_elapsedTick >= scriptHolder.Tick)
			{
				_readyList.RemoveAt(i);
				i--;
				_playingList.Add(scriptHolder);
				scriptHolder.Value.Begin(_localTimer, isSkip: false);
			}
		}
		for (int j = 0; j < _playingList.Count; j++)
		{
			if (_playingList[j].Value != null)
			{
				_playingList[j].Value.Update();
			}
		}
		if (TouchType == ETouchType.Timeout || TouchType == ETouchType.TimeoutOrTouch)
		{
			if (_elapsedTick > TouchTickLength)
			{
				IsEnd = true;
			}
		}
		else if (TouchType == ETouchType.WaitNodeTree)
		{
			if (!SingletonAuto<BattleFacade>.isValidInstance)
			{
				IsEnd = true;
			}
			else if (!SingletonAuto<BattleFacade>.instance.NodeTreePlaying())
			{
				IsEnd = true;
			}
		}
		else
		{
			if (TouchType != ETouchType.WaitGroupPlay)
			{
				return;
			}
			bool flag = true;
			foreach (ScriptHolder script in ScriptList)
			{
				if (!script.Value.IsEnd)
				{
					flag = false;
					break;
				}
			}
			if (flag)
			{
				IsEnd = true;
			}
		}
	}

	public void FinalizeGroup(FinalizeType finalizeType = FinalizeType.Normal)
	{
		for (int i = 0; i < _playingList.Count; i++)
		{
			_playingList[i].Value.End();
			_playingList[i].Value.FinalizeScript(finalizeType);
		}
		_playingList.Clear();
		for (int j = 0; j < _readyList.Count; j++)
		{
			_readyList[j].Value.Begin(_localTimer, isSkip: true);
			_readyList[j].Value.End();
			_readyList[j].Value.FinalizeScript(finalizeType);
		}
		_readyList.Clear();
		IsEnd = true;
	}

	public void OnTouch()
	{
		if (TouchType == ETouchType.Timeout || TouchType == ETouchType.WaitNodeTree)
		{
			return;
		}
		if (TouchType == ETouchType.TimeoutOrTouch)
		{
			FinalizeGroup();
		}
		else if (TouchType == ETouchType.Touch)
		{
			FinalizeGroup();
		}
		else if (TouchType == ETouchType.TouchAndTouch)
		{
			bool flag = true;
			for (int i = 0; i < _playingList.Count; i++)
			{
				if (!_playingList[i].Value.IsEnd)
				{
					_playingList[i].Value.End();
					flag = false;
				}
			}
			for (int j = 0; j < _readyList.Count; j++)
			{
				ScriptHolder scriptHolder = _readyList[j];
				_playingList.Add(scriptHolder);
				scriptHolder.Value.Begin(_localTimer, isSkip: true);
				scriptHolder.Value.End();
				flag = false;
			}
			_readyList.Clear();
			if (flag)
			{
				FinalizeGroup();
			}
		}
		else if (TouchType == ETouchType.PassToScript)
		{
			bool flag2 = true;
			for (int k = 0; k < ScriptList.Count; k++)
			{
				IScript value = ScriptList[k].Value;
				if (!value.IsEnd && value is IClickScript)
				{
					(value as IClickScript).OnClick();
					if (!value.IsEnd)
					{
						flag2 = false;
					}
				}
			}
			if (flag2)
			{
				FinalizeGroup();
			}
		}
		else
		{
			_ = TouchType;
			_ = 6;
		}
	}

	bool ILoadJson.Load(JsonData json)
	{
		JsonTools.ToList(json["ScriptList"], ScriptList);
		TouchTickLength = json.GetJsonAny("TouchTickLength", "DelayTick").GETINT(TouchTickLength);
		JsonData jsonData = json["TouchWait"];
		if (jsonData.IsBoolean())
		{
			TouchType = ((!jsonData.valueBool) ? ETouchType.TimeoutOrTouch : ETouchType.Touch);
		}
		else
		{
			TouchType = json["TouchType"].GETENUM(TouchType);
		}
		SkipType = json["SkipType"].GETENUM(SkipType);
		return true;
	}

	JsonData IMakeJson.MakeJson()
	{
		JsonData jsonData = new JsonData();
		jsonData.AddFieldList("ScriptList", ScriptList);
		jsonData.AddFieldLong("TouchTickLength", TouchTickLength);
		jsonData.AddFieldEnum("TouchType", TouchType);
		jsonData.AddFieldEnum("SkipType", SkipType);
		return jsonData;
	}
}
