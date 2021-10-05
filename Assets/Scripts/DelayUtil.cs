
using System;
using System.Collections;
using UnityEngine;

public class DelayUtil
{
    public static IEnumerator WaitAndDo (float time, Action action) {
        yield return new WaitForSeconds (time);
        action();
    }
}