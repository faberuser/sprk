using System.Collections.Generic;
using LitJson;
using NGame2.NBattleField;
using NGame2.NBattleThread;
using NGame2.NNodeTree.NLink;
using NGame2.NNodeTree.NRuntime;
using NGame2.NNodeTree.NTypeValue;
using NShared;
using NShared.NCampaign;
using NVespa;

namespace NGame2.NNodeTree.NContents.NBattle;

public class IsActiveOperation : BattleBase, IKey, IExecute
{
	public class SkillIndexOperationNo : IBuilder, ILoadJson, IJsonInterface, IMakeJson
	{
		public long SkillIndex;

		public int OperationNo;

		bool ILoadJson.Load(JsonData json)
		{
			SkillIndex = json["SkillIndex"].GETLONG(SkillIndex);
			OperationNo = json["OperationNo"].GETINT(OperationNo);
			return true;
		}

		JsonData IMakeJson.MakeJson()
		{
			JsonData jsonData = new JsonData();
			jsonData.AddFieldLong("SkillIndex", SkillIndex);
			jsonData.AddFieldLong("OperationNo", OperationNo);
			return jsonData;
		}
	}

	public class LocalItem : ItemBase, IThreadAction
	{
		public bool IsEnd;

		public long uid;

		public List<SkillIndexOperationNo> OperationNoList;

		public bool IsActive;

		public bool IsWaiting { get; set; }

		void IThreadAction.Action(BattleInstance battleInstance, CampaignUpdater campaignUpdater)
		{
			BattleCreature creature = battleInstance.CreatureContainer.GetCreature((int)uid);
			_ = creature.OperationController;
			for (int i = 0; i < __Extension_Method_ICollection__.COUNT(OperationNoList); i++)
			{
				SkillIndexOperationNo skillIndexOperationNo = OperationNoList[i];
				if (skillIndexOperationNo != null && creature.OperationController.FindOperationInfo((int)skillIndexOperationNo.SkillIndex, skillIndexOperationNo.OperationNo) != null)
				{
					IsActive = true;
					break;
				}
			}
		}
	}

	public static string ContentsTypeName = "Battle/" + typeof(IsActiveOperation).Name;

	public Callee CalleeStart;

	public Caller CallerEndTrue;

	public Caller CallerEndFalse;

	public ArgumentSingleGeneric<long> CreatureUid;

	public List<SkillIndexOperationNo> SkillIndexOperationNoList = new List<SkillIndexOperationNo>();

	public IsActiveOperation(NodeData i_node)
		: base(i_node)
	{
		CalleeStart = new Callee(this, "ContentsStart", ActionStart);
		CallerEndTrue = new Caller(this, "True");
		CallerEndFalse = new Caller(this, "False");
		CreatureUid = new ArgumentSingleGeneric<long>(VariableKeyword.Myself);
	}

	public override void Wakeup(IVariable variable)
	{
		CreatureUid.GetValue(variable);
	}

	private void ActionStart(RuntimeInstance runtime, IVariable variable)
	{
		if (__Extension_Method_ICollection__.COUNT(SkillIndexOperationNoList) <= 0)
		{
			ActionEnd(isTrue: false, runtime, variable);
			return;
		}
		LocalItem localItem = runtime.ItemContainer.CreateItem<LocalItem>(this, variable);
		localItem.uid = CreatureUid.GetValue(variable);
		localItem.OperationNoList = SkillIndexOperationNoList;
		SingletonAuto<BattleFacade>.instance.AddThreadSync(localItem);
		runtime.ExecuteContainer.Pending(this, localItem);
	}

	private void ActionEnd(bool isTrue, RuntimeInstance runtime, IVariable variable)
	{
		if (isTrue)
		{
			CallerEndTrue.Call(runtime, variable);
		}
		else
		{
			CallerEndFalse.Call(runtime, variable);
		}
	}

	private void ActionEnd(LocalItem localItem)
	{
		localItem.IsEnd = true;
		ActionEnd(localItem.IsActive, localItem.Runtime, localItem.Variable);
	}

	void IExecute.UpdateExecute(ItemBase itemBase)
	{
		if (itemBase is LocalItem { IsEnd: false, IsWaiting: false } localItem)
		{
			ActionEnd(localItem);
		}
	}

	bool IExecute.IsExecuteEnd(ItemBase itemBase)
	{
		if (!(itemBase is LocalItem localItem))
		{
			return true;
		}
		return localItem.IsEnd;
	}

	void IExecute.RequestStopExecute(ItemBase itemBase, ERequestStop requestStop)
	{
		if (itemBase is LocalItem localItem)
		{
			ActionEnd(localItem);
		}
	}

	internal override string GetContentsTypeName()
	{
		return ContentsTypeName;
	}

	public override void LoadFromJson(JsonData json)
	{
		CalleeStart.LOADFROMJSON(json["CalleeStart"]);
		CallerEndTrue.LOADFROMJSON(json["CallerEndTrue"]);
		CallerEndFalse.LOADFROMJSON(json["CallerEndFalse"]);
		CreatureUid.LoadFromJson(json["CreatureUid"]);
		SkillIndexOperationNoList.LoadFromJson(json["SkillIndexOperationNoList"]);
	}

	public override JsonData MakeJson()
	{
		JsonData jsonData = new JsonData();
		jsonData.AddFieldString("type", this.GETCONTENTSTYPENAME());
		jsonData.AddField("CalleeStart", CalleeStart.MAKEJSON());
		jsonData.AddField("CallerEndTrue", CallerEndTrue.MAKEJSON());
		jsonData.AddField("CallerEndFalse", CallerEndFalse.MAKEJSON());
		jsonData.AddField("CreatureUid", CreatureUid.MakeJson());
		jsonData.AddFieldList("SkillIndexOperationNoList", SkillIndexOperationNoList);
		return jsonData;
	}
}
