﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shaker : MonoBehaviour
{
    Transform target;
    Vector3 startPosision;

    float pendingShakeDuration = 0f;
    bool isShaking = false;

    void Start()
    {
        target = GetComponent<Transform>();
        startPosision = target.localPosition;
    }

    public void Shake(float duration)
    {
        if (duration > 0)
        {
            pendingShakeDuration += duration;
        }
    }

    private void Update()
    {
        if (pendingShakeDuration > 0 && !isShaking)
        {
            StartCoroutine(DoShake());
        }
    }

    IEnumerator DoShake()
    {
        isShaking = true;

        var startTime = Time.realtimeSinceStartup;
        while (Time.realtimeSinceStartup < startTime + pendingShakeDuration)
        {
            var randomPoint = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), startPosision.z);
            target.localPosition = randomPoint;
            yield return null;
        }

        pendingShakeDuration = 0f;
        target.localPosition = startPosision;

        isShaking = false;
    }
}
