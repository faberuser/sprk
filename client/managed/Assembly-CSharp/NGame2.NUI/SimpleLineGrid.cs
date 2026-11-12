using System.Collections.Generic;
using UnityEngine;

namespace NGame2.NUI;

public class SimpleLineGrid : MonoBehaviour
{
	public GameObject LineGameObj;

	[HideInInspector]
	public float Speed = 1f;

	[HideInInspector]
	public float LineLength = 1f;

	[HideInInspector]
	public float LineWidth = 0.05f;

	[HideInInspector]
	public int LineCount = 5;

	private List<LineRenderer> _lineRendererList;

	private LineRenderer _lineRenderer;

	private Vector3 position;

	private float _timeOffset;

	private void Start()
	{
		_lineRendererList = new List<LineRenderer>();
		_lineRendererList.Clear();
		position = base.transform.position;
		SetHorizontalLines();
		SetVerticalLines();
	}

	private void Update()
	{
	}

	private void InstantiateLine()
	{
		if (!(LineGameObj == null))
		{
			LineGameObj.SetActive(value: true);
			GameObject gameObject = Object.Instantiate(LineGameObj, position, Quaternion.identity);
			if (!(gameObject == null))
			{
				gameObject.transform.SetParent(base.transform);
				_lineRenderer = gameObject.GetComponent<LineRenderer>();
				_lineRenderer.startWidth = LineWidth;
				_lineRenderer.endWidth = LineWidth;
				_lineRenderer.useWorldSpace = false;
				LineGameObj.SetActive(value: false);
			}
		}
	}

	private void SetVerticalLines()
	{
		for (int i = 0; i < LineCount; i++)
		{
			InstantiateLine();
			for (int j = 0; j < 2; j++)
			{
				_lineRenderer.SetPosition(j, new Vector3(position.x + (float)i * (LineLength / (float)LineCount), position.y, position.z + (float)j * LineLength));
			}
		}
	}

	private void SetHorizontalLines()
	{
		for (int i = 0; i < LineCount; i++)
		{
			InstantiateLine();
			for (int j = 0; j < 2; j++)
			{
				_lineRenderer.SetPosition(j, new Vector3(position.x + (float)j * LineLength, position.y, position.z + (float)i * (LineLength / (float)LineCount)));
			}
			_lineRendererList.Add(_lineRenderer);
		}
	}

	private void MoveToUpLine()
	{
		if (__Extension_Method_ICollection__.IsNullOrEmpty(_lineRendererList))
		{
			return;
		}
		_timeOffset += Time.deltaTime * Speed;
		for (int i = 0; i < LineCount; i++)
		{
			if (!(_lineRendererList[i] == null))
			{
				for (int j = 0; j < 2; j++)
				{
					_lineRendererList[i].SetPosition(j, new Vector3(position.x + (float)j * LineLength, position.y, position.z + ((float)i * (LineLength / (float)LineCount) + _timeOffset) % LineLength));
				}
			}
		}
	}
}
