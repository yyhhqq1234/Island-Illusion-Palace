using System;
using UnityEngine;

// Token: 0x02000002 RID: 2
public class CatFamiliarPositionSetter : MonoBehaviour
{
	// Token: 0x06000001 RID: 1 RVA: 0x00002050 File Offset: 0x00000250
	private void Start()
	{
		Transform[] componentsInChildren = this.positions.GetComponentsInChildren<Transform>();
		int num = UnityEngine.Random.Range(0, componentsInChildren.Length);
		this.cat.position = componentsInChildren[num].position;
	}

	// Token: 0x04000001 RID: 1
	public Transform cat;

	// Token: 0x04000002 RID: 2
	public GameObject positions;
}
