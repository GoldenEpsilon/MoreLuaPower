using System;
using System.Collections;
using MoonSharp.Interpreter;
using UnityEngine;

[MoonSharpUserData]
public class MPLHook : Item
{
	// Token: 0x06000681 RID: 1665 RVA: 0x000368CC File Offset: 0x00034ACC
	protected override IEnumerator EffectRoutine(DynValue result) {
		while (base.gameObject) {
			try {
				result.Coroutine.Resume(new object[]
				{
					this.itemObj
				});
			} catch (ScriptRuntimeException ex) {
				Debug.LogError(ex.DecoratedMessage);
			}
			if (result.Coroutine.State == CoroutineState.Dead) {
				break;
			}
			yield return null;
		}
		yield break;
	}
}