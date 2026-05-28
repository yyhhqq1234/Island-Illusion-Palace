using System;
using TMPro;
using UnityEngine;

// Token: 0x02000003 RID: 3
public class PickableObjectDebugName : MonoBehaviour
{
	// Token: 0x06000003 RID: 3 RVA: 0x0000208E File Offset: 0x0000028E
	public void SetText(string name, bool on)
	{
		this.TextName.text = name;
		this.TextName.gameObject.SetActive(on);
	}

	// Token: 0x04000003 RID: 3
	public TMP_Text TextName;
}
