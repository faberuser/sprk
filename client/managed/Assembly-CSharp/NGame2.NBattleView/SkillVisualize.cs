using System.Collections.Generic;
using NShared;
using UnityEngine;

namespace NGame2.NBattleView;

public class SkillVisualize : BattleViewGenericSingleton<SkillVisualize>
{
	public class RangeData
	{
		public Vector3 Position;

		public Vector3 Direction;

		public float MinRange;

		public float MaxRange;

		public float Angle;
	}

	public List<RangeData> RangeList = new List<RangeData>();

	public void BeginFireSkill(BattleCreature creature, SkillData skillData)
	{
	}

	public void BeginOperation(BattleCreature creature, SkillData skillData)
	{
	}
}
