using System;
using UnityEngine;

namespace NVespa.NUtil;

[Serializable]
public class TransformData
{
	[SerializeField]
	public float posX;

	[SerializeField]
	public float posY;

	[SerializeField]
	public float posZ;

	[SerializeField]
	public float rotX;

	[SerializeField]
	public float rotY;

	[SerializeField]
	public float rotZ;

	[SerializeField]
	public float scaleX;

	[SerializeField]
	public float scaleY;

	[SerializeField]
	public float scaleZ;
}
