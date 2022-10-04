using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoDisappearTool : MonoBehaviour
{
    [SerializeField] private bool needDestroy = false;
    [SerializeField] private float disappearWaitTime;

    private WaitForSeconds disappearWFS;

    private void Awake()
    {
        disappearWFS = new WaitForSeconds(disappearWaitTime);
    }

    private void OnEnable()
    {
        StartCoroutine(nameof(AutoDisappearCor));
    }

    IEnumerator AutoDisappearCor()
    {
        yield return disappearWFS;

        if (!needDestroy)
        {
            gameObject.SetActive(false);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }

    private void OnDestroy()
    {
        StopAllCoroutines();
    }
}
