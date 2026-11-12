using System.Collections.Generic;
using System.Text;
using NGame2.NMessage;
using NShared;
using NVespa;

namespace NGame2.NUI.NWindow.NBattle;

public class BattleLog2 : BaseWindow, GlobalManager.IListener
{
	private class OperationStatus
	{
		public NShared.FireSkill.OperationInfo OperationInfo;

		public string EventMsg = string.Empty;

		public int ReserveOp;

		public int FlyingOp;

		public int BeginOp;

		public int EndOp;

		public int CancelOp;

		public int ResistOp;

		public void Dump(StringBuilder builder)
		{
			builder.Append($"        Operation No={OperationInfo.OperationNo}: ");
			builder.Append(EventMsg);
			builder.AppendLine();
		}
	}

	private class FireSkill
	{
		public NShared.FireSkill CallBack;

		public IncreaseDic<long, OperationStatus> OperationDic = new IncreaseDic<long, OperationStatus>();

		public void Dump(StringBuilder builder)
		{
			builder.AppendLine($"    [FireSkill({CallBack.GlobalTimestamp})]");
			for (int i = 0; i < OperationDic.Count; i++)
			{
				OperationDic.GetValueByIndex(i).Dump(builder);
			}
			builder.AppendLine("    [/FireSkill]");
		}

		public bool Listen(IMessage message)
		{
			if (OnMessage(message.CastOperation<ReserveOperation>()))
			{
				return true;
			}
			if (OnMessage(message.CastOperation<FlyingOperation>()))
			{
				return true;
			}
			if (OnMessage(message.CastOperation<BeginOperation>()))
			{
				return true;
			}
			if (OnMessage(message.CastOperation<EndOperation>()))
			{
				return true;
			}
			if (OnMessage(message.CastOperation<CancelOperation>()))
			{
				return true;
			}
			if (OnMessage(message.CastOperation<ResistOperation>()))
			{
				return true;
			}
			return false;
		}

		private bool OnMessage(OperationMessage<ReserveOperation> message)
		{
			if (message == null)
			{
				return false;
			}
			OperationStatus vALUE = OperationDic.GETVALUE(message.Callback.OperationUid, null);
			if (vALUE == null)
			{
				return false;
			}
			vALUE.ReserveOp++;
			if (!String_Extension_Method.IsNullOrEmpty(vALUE.EventMsg))
			{
				vALUE.EventMsg += ", ";
			}
			vALUE.EventMsg += $"Reserve({message.Callback.GlobalTimestamp})";
			return true;
		}

		private bool OnMessage(OperationMessage<FlyingOperation> message)
		{
			if (message == null)
			{
				return false;
			}
			OperationStatus vALUE = OperationDic.GETVALUE(message.Callback.OperationUid, null);
			if (vALUE == null)
			{
				return false;
			}
			vALUE.FlyingOp++;
			if (!String_Extension_Method.IsNullOrEmpty(vALUE.EventMsg))
			{
				vALUE.EventMsg += ", ";
			}
			vALUE.EventMsg += $"Flying({message.Callback.GlobalTimestamp})";
			return true;
		}

		private bool OnMessage(OperationMessage<BeginOperation> message)
		{
			if (message == null)
			{
				return false;
			}
			OperationStatus vALUE = OperationDic.GETVALUE(message.Callback.OperationUid, null);
			if (vALUE == null)
			{
				return false;
			}
			vALUE.BeginOp++;
			if (!String_Extension_Method.IsNullOrEmpty(vALUE.EventMsg))
			{
				vALUE.EventMsg += ", ";
			}
			vALUE.EventMsg += $"Begin({message.Callback.GlobalTimestamp})";
			return true;
		}

		private bool OnMessage(OperationMessage<EndOperation> message)
		{
			if (message == null)
			{
				return false;
			}
			OperationStatus vALUE = OperationDic.GETVALUE(message.Callback.OperationUid, null);
			if (vALUE == null)
			{
				return false;
			}
			vALUE.EndOp++;
			if (!String_Extension_Method.IsNullOrEmpty(vALUE.EventMsg))
			{
				vALUE.EventMsg += ", ";
			}
			vALUE.EventMsg += $"End({message.Callback.GlobalTimestamp})";
			return true;
		}

		private bool OnMessage(OperationMessage<CancelOperation> message)
		{
			if (message == null)
			{
				return false;
			}
			OperationStatus vALUE = OperationDic.GETVALUE(message.Callback.OperationUid, null);
			if (vALUE == null)
			{
				return false;
			}
			vALUE.CancelOp++;
			if (!String_Extension_Method.IsNullOrEmpty(vALUE.EventMsg))
			{
				vALUE.EventMsg += ", ";
			}
			vALUE.EventMsg += $"Cancel({message.Callback.GlobalTimestamp})";
			return true;
		}

		private bool OnMessage(OperationMessage<ResistOperation> message)
		{
			if (message == null)
			{
				return false;
			}
			OperationStatus vALUE = OperationDic.GETVALUE(message.Callback.OperationUid, null);
			if (vALUE == null)
			{
				return false;
			}
			vALUE.CancelOp++;
			if (!String_Extension_Method.IsNullOrEmpty(vALUE.EventMsg))
			{
				vALUE.EventMsg += ", ";
			}
			vALUE.EventMsg += $"Resist({message.Callback.GlobalTimestamp})";
			return true;
		}
	}

	private class BeginFireSkill
	{
		public NShared.BeginFireSkill CallBack;

		public bool IsEnd;

		public float EndTime;

		public long EndTimeStamp;

		public List<FireSkill> FireSkillList = new List<FireSkill>();

		public void Dump(StringBuilder builder)
		{
			builder.AppendLine($"BeginFireSkill({CallBack.GlobalTimestamp}): Creature={CallBack.CreatureUid}, SkillId={CallBack.SkillId}");
			for (int i = 0; i < FireSkillList.Count; i++)
			{
				FireSkillList[i].Dump(builder);
			}
			if (IsEnd)
			{
				builder.AppendLine($"EndFireSkill({EndTimeStamp})");
			}
			else
			{
				builder.AppendLine("playing...");
			}
		}

		public bool Listen(IMessage message)
		{
			if (OnMessage(message.CastCallback<EndFireSkill>()))
			{
				return true;
			}
			if (OnMessage(message.CastCallback<NShared.FireSkill>()))
			{
				return true;
			}
			for (int i = 0; i < FireSkillList.Count; i++)
			{
				if (FireSkillList[i].Listen(message))
				{
					return true;
				}
			}
			return false;
		}

		public bool OnMessage(CallbackMessage<EndFireSkill> message)
		{
			if (message == null)
			{
				return false;
			}
			if (CallBack.CreatureUid != message.Callback.CreatureUid)
			{
				return false;
			}
			if (CallBack.SkillId != message.Callback.SkillId)
			{
				return false;
			}
			IsEnd = true;
			EndTime = MainTimer.time;
			EndTimeStamp = message.Callback.GlobalTimestamp;
			return true;
		}

		public bool OnMessage(CallbackMessage<NShared.FireSkill> message)
		{
			if (message == null)
			{
				return false;
			}
			if (CallBack.CreatureUid != message.Callback.CreatureUid)
			{
				return false;
			}
			if (CallBack.SkillId != message.Callback.SkillId)
			{
				return false;
			}
			FireSkill fireSkill = new FireSkill();
			fireSkill.CallBack = message.Callback;
			for (int i = 0; i < __Extension_Method_ICollection__.COUNT(message.Callback.OperationInfos); i++)
			{
				OperationStatus operationStatus = new OperationStatus();
				operationStatus.OperationInfo = message.Callback.OperationInfos[i];
				fireSkill.OperationDic.Add(operationStatus.OperationInfo.OperationUid, operationStatus);
			}
			FireSkillList.Add(fireSkill);
			return true;
		}
	}

	public const string PrefabPath = "Resources/Prefab2/Window/Battle/BattleLog2";

	private static WindowPrefab<BattleLog2> _instance = new WindowPrefab<BattleLog2>("Resources/Prefab2/Window/Battle/BattleLog2", GameUIRoot.ELayerType.UI);

	public UILabel LogLabel;

	private List<BeginFireSkill> _skillList = new List<BeginFireSkill>();

	public static BattleLog2 instance => _instance.instance;

	public static bool IsValidInstance => _instance.IsValidInstance;

	public static void CreateSingletonInstance()
	{
		_instance.CreateInstance();
	}

	public void OpenLog()
	{
		Open(delegate
		{
			Init();
		}, null);
	}

	public void Init()
	{
		SingletonAuto<GlobalManager>.instance.AddListerer(this);
	}

	void GlobalManager.IListener.Listen(IMessage message)
	{
		if (OnMessage(message.CastCallback<NShared.BeginFireSkill>()))
		{
			UpdateLabel();
			return;
		}
		for (int i = 0; i < _skillList.Count; i++)
		{
			if (_skillList[i].Listen(message))
			{
				UpdateLabel();
				break;
			}
		}
	}

	private bool OnMessage(CallbackMessage<NShared.BeginFireSkill> message)
	{
		if (message == null)
		{
			return false;
		}
		BeginFireSkill beginFireSkill = new BeginFireSkill();
		beginFireSkill.CallBack = message.Callback;
		_skillList.Insert(0, beginFireSkill);
		return true;
	}

	private void UpdateLabel()
	{
		if (_skillList.Count > 10)
		{
			_skillList.RemoveAt(_skillList.Count - 1);
		}
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < _skillList.Count; i++)
		{
			_skillList[i].Dump(stringBuilder);
			if (i < _skillList.Count - 1)
			{
				stringBuilder.AppendLine();
			}
		}
		UIHelper.SetLabelText(LogLabel, stringBuilder.ToString());
	}
}
