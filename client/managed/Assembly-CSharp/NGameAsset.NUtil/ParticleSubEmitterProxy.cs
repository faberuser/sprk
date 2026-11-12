using System;
using System.Collections.Generic;
using NVespa;
using UnityEngine;

namespace NGameAsset.NUtil;

public class ParticleSubEmitterProxy : MonoBehaviour, IInspectorGuiDrawer
{
	[Serializable]
	public class SubEmitterDefine
	{
		public ParticleSystem SubEmitterPS;

		public ParticleSystemSubEmitterType SubEmitterType;

		public ParticleSystemSubEmitterProperties SubEmitterProperties;
	}

	[Serializable]
	public class OwnerDefine
	{
		public ParticleSystem OwnerPS;

		public List<SubEmitterDefine> SubEmitterList = new List<SubEmitterDefine>();
	}

	[HideInInspector]
	public List<OwnerDefine> OwnerList = new List<OwnerDefine>();

	private bool _restoredInPlaying;

	public static void DisableAndClearSubEmitter(ParticleSystem ps)
	{
		ParticleSystem.SubEmittersModule subEmitters = ps.subEmitters;
		subEmitters.enabled = false;
		int subEmittersCount = subEmitters.subEmittersCount;
		for (int i = 0; i < subEmittersCount; i++)
		{
			if (subEmitters.subEmittersCount <= 0)
			{
				break;
			}
			subEmitters.RemoveSubEmitter(0);
		}
	}

	public void RestoreSubEmittersInPlaying()
	{
		if (!_restoredInPlaying)
		{
			_restoredInPlaying = true;
			RestoreSubEmitters();
		}
	}

	private void RestoreSubEmitters()
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(OwnerList); i++)
		{
			OwnerDefine ownerDefine = OwnerList[i];
			if (ownerDefine.OwnerPS == null)
			{
				VespaDebug.LogError("Owner ParticleSystem is null: " + i);
				continue;
			}
			int num = 0;
			ParticleSystem.SubEmittersModule subEmitters = ownerDefine.OwnerPS.subEmitters;
			for (int j = 0; j < __Extension_Method_ICollection__.COUNT(ownerDefine.SubEmitterList); j++)
			{
				SubEmitterDefine subEmitterDefine = ownerDefine.SubEmitterList[j];
				if (subEmitterDefine.SubEmitterPS == null)
				{
					VespaDebug.LogError($"SubEmitter ParticleSystem is null: owner={i}, subEmitter={j}");
					continue;
				}
				if (subEmitters.subEmittersCount <= num)
				{
					subEmitters.AddSubEmitter(subEmitterDefine.SubEmitterPS, subEmitterDefine.SubEmitterType, subEmitterDefine.SubEmitterProperties);
				}
				else
				{
					subEmitters.SetSubEmitterSystem(num, subEmitterDefine.SubEmitterPS);
					subEmitters.SetSubEmitterType(num, subEmitterDefine.SubEmitterType);
					subEmitters.SetSubEmitterProperties(num, subEmitterDefine.SubEmitterProperties);
				}
				num++;
			}
			if (num == 0)
			{
				DisableAndClearSubEmitter(ownerDefine.OwnerPS);
				continue;
			}
			subEmitters.enabled = true;
			for (int num2 = subEmitters.subEmittersCount - 1; num2 >= num; num2--)
			{
				subEmitters.RemoveSubEmitter(num2);
			}
		}
		OwnerList.Clear();
	}
}
