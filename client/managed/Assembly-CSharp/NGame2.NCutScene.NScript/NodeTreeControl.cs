using LitJson;
using NGame2.NBattleField;
using NGame2.NNodeTree;
using NGame2.NNodeTree.NUtil;
using NVespa;

namespace NGame2.NCutScene.NScript;

public class NodeTreeControl : IScript, IBuilder, ILoadJson, IJsonInterface, IMakeJson
{
	public enum EControlType
	{
		Play,
		Stop,
		Next,
		SetFlowNumber
	}

	public EControlType ControlType;

	public LinkNodeTree2 LinkNodeTree = new LinkNodeTree2();

	public int FlowNumber;

	private ILocalTimer _localTimer;

	private bool _isEnd;

	bool IScript.IsEnd => _isEnd;

	void IScript.Visit(IVisitor visitor)
	{
		visitor.TryVisit(this);
	}

	void IScript.Begin(ILocalTimer localTimer, bool isSkip)
	{
		_localTimer = localTimer;
		_isEnd = false;
	}

	void IScript.Update()
	{
		if (_isEnd)
		{
			return;
		}
		if (_localTimer == null)
		{
			_isEnd = true;
		}
		else if (!SingletonAuto<BattleFacade>.isValidInstance)
		{
			_isEnd = true;
		}
		else if (ControlType == EControlType.Play)
		{
			LinkNodeTree2.EGetResult oResult;
			TreeData tree = LinkNodeTree.GetTree(out oResult);
			if (tree == null)
			{
				if (oResult == LinkNodeTree2.EGetResult.Invalid)
				{
					_isEnd = true;
				}
			}
			else
			{
				SingletonAuto<BattleFacade>.instance.NodeTreeStart(tree);
				_isEnd = true;
			}
		}
		else if (ControlType == EControlType.Stop)
		{
			SingletonAuto<BattleFacade>.instance.NodeTreeStop();
			_isEnd = true;
		}
		else if (ControlType == EControlType.Next)
		{
			SingletonAuto<BattleFacade>.instance.NodeTreeFlowNumber++;
			_isEnd = true;
		}
		else if (ControlType == EControlType.SetFlowNumber)
		{
			SingletonAuto<BattleFacade>.instance.NodeTreeFlowNumber = FlowNumber;
			_isEnd = true;
		}
		else
		{
			_isEnd = true;
		}
	}

	void IScript.End()
	{
	}

	void IScript.FinalizeScript(FinalizeType finalizeType)
	{
		_isEnd = true;
		_ = _localTimer;
	}

	void IScript.Reset()
	{
	}

	bool ILoadJson.Load(JsonData json)
	{
		ControlType = json["ControlType"].GETENUM(ControlType);
		LinkNodeTree.LOADFROMJSON(json["LinkNodeTree"]);
		FlowNumber = json["FlowNumber"].GETINT(FlowNumber);
		return true;
	}

	JsonData IMakeJson.MakeJson()
	{
		JsonData jsonData = new JsonData();
		jsonData.AddFieldEnum("ControlType", ControlType);
		jsonData.AddFieldJsonMake("LinkNodeTree", LinkNodeTree);
		jsonData.AddFieldLong("FlowNumber", FlowNumber);
		return jsonData;
	}
}
