using LitJson;
using NGame2.NBattleView;
using NGame2.NNodeTree.NLink;
using NGame2.NNodeTree.NRuntime;
using NGame2.NNodeTree.NTypeValue;
using NVespa;

namespace NGame2.NNodeTree.NContents.NBattle;

public class DestroySubCreature : ExecuteBase, IKey
{
	public class LocalItem : ItemBase, IVerifier
	{
		public bool IsEnd;

		public float BeginTime;

		bool IVerifier.IsValid()
		{
			return !IsEnd;
		}
	}

	public static string ContentsTypeName = "Battle/" + typeof(DestroySubCreature).Name;

	public Callee CalleeStart;

	public Caller CallerEnd;

	public bool UseMyselfDirect;

	public ArgumentSingleGeneric<long> MyselfUid;

	public ArgumentSingleGeneric<object> MyselfDirect;

	public DestroySubCreature(NodeData i_node)
		: base(i_node)
	{
		CalleeStart = new Callee(this, "ContentsStart", ActionStart);
		CallerEnd = new Caller(this, "EndCaller");
		MyselfDirect = new ArgumentSingleGeneric<object>(VariableKeyword.Myself);
		MyselfUid = new ArgumentSingleGeneric<long>(VariableKeyword.Myself);
	}

	public override void Wakeup(IVariable variable)
	{
	}

	private SubCreature GetMyselfSubCreature(LocalItem localItem, IVariable variable)
	{
		SubCreature subCreature = null;
		if (UseMyselfDirect)
		{
			subCreature = MyselfDirect.GetValue(variable) as SubCreature;
		}
		else
		{
			Creature bATTLECREATURE = variable.RUNTIMEINSTANCE().SCENECONTAINER().GETBATTLECREATURE((int)MyselfUid.GetValue(variable));
			if (bATTLECREATURE == null)
			{
				return null;
			}
			subCreature = bATTLECREATURE.SubCreature;
		}
		if (subCreature == null || subCreature.ActionSubCreature == null)
		{
			return null;
		}
		return subCreature;
	}

	private void ActionStart(RuntimeInstance runtime, IVariable variable)
	{
		LocalItem localItem = runtime.ItemContainer.CreateItem<LocalItem>(this, variable);
		SubCreature myselfSubCreature = GetMyselfSubCreature(localItem, variable);
		if (myselfSubCreature != null)
		{
			myselfSubCreature.ReleaseSubCreature();
			CallerEnd.Call(localItem.Runtime, localItem.Variable);
		}
	}

	internal override string GetContentsTypeName()
	{
		return ContentsTypeName;
	}

	public override void LoadFromJson(JsonData json)
	{
		CalleeStart.LOADFROMJSON(json["CalleeStart"]);
		CallerEnd.LOADFROMJSON(json["CallerEnd"]);
		UseMyselfDirect = json["UseMyselfDirect"].GETBOOLEAN(UseMyselfDirect);
		if (!MyselfUid.LoadFromJson(json["MyselfUid"]))
		{
			MyselfUid.Holder.Value = SingletonAuto<ValueFactory>.instance.CreateVariable<long>(VariableKeyword.Myself);
		}
		if (!MyselfDirect.LoadFromJson(json["MyselfDirect"]))
		{
			MyselfDirect.Holder.Value = SingletonAuto<ValueFactory>.instance.CreateVariable<long>(VariableKeyword.Myself);
		}
	}

	public override JsonData MakeJson()
	{
		JsonData jsonData = new JsonData();
		jsonData.AddFieldString("type", this.GETCONTENTSTYPENAME());
		jsonData.AddFieldBool("UseMyselfDirect", UseMyselfDirect);
		jsonData.AddField("MyselfDirect", MyselfDirect.MakeJson());
		jsonData.AddField("MyselfUid", MyselfUid.MakeJson());
		jsonData.AddField("CalleeStart", CalleeStart.MAKEJSON());
		jsonData.AddField("CallerEnd", CallerEnd.MAKEJSON());
		return jsonData;
	}
}
