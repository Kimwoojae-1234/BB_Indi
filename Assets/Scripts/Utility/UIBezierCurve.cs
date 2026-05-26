using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class UIBezierCurve : MonoBehaviour
{
    [SerializeField]
    private Vector3 BezierPosition_0;
    [SerializeField]
    private Vector3 BezierPosition_1;
    [SerializeField]
    private Vector3 BezierPosition_2;
    [SerializeField]
    private Vector3 BezierPosition_3;
    [SerializeField]
    private float MoveDuration = 1.0f;
    private bool MoveStart = false;
    private float MoveTimeCount = 0.0f;
    private Action EndCallback = null;
    private float CallBackManualTime = 1.0f;
    private AnimationCurve moveCurve = null;


    public void SetBezierPosition_0(Vector3 TargetWorldPosition)
    {
        BezierPosition_0 = TargetWorldPosition;
    }
    public void SetBezierPosition_1(Vector3 TargetWorldPosition)
    {
        BezierPosition_1 = TargetWorldPosition;
    }
    public void SetBezierPosition_2(Vector3 TargetWorldPosition)
    {
        BezierPosition_2 = TargetWorldPosition;
    }
    public void SetBezierPosition_3(Vector3 TargetWorldPosition)
    {
        BezierPosition_3 = TargetWorldPosition;
    }

    public void SetDurationTime(float duration_time)
    {
        MoveDuration = duration_time;
    }

    public void SetEndCallBackFunc(Action func, float ManualTime = 1.0f)
    {
        EndCallback = func;
        CallBackManualTime = ManualTime;
    }

    public Vector3 GetBezierPosition_0()
    {
        return BezierPosition_0;
    }

    public Vector3 GetBezierPosition_1()
    {
        return BezierPosition_1;
    }

    public Vector3 GetBezierPosition_2()
    {
        return BezierPosition_2;
    }

    public Vector3 GetBezierPosition_3()
    {
        return BezierPosition_3;
    }

    public void Play(AnimationCurve moveCurve)
    {
        MoveStart = true;
        MoveTimeCount = 0.0f;
        this.moveCurve = moveCurve;
    }

    private Vector3 GetPointOnBezierCurve(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
    {
        float u = 1f - t;
        float t2 = t * t;
        float u2 = u * u;
        float u3 = u2 * u;
        float t3 = t2 * t;

        Vector3 result =
            (u3) * p0 +
            (3f * u2 * t) * p1 +
            (3f * u * t2) * p2 +
            (t3) * p3;

        return result;
    }

    // Update is called once per frame
    void Update()
    {
        if(MoveStart)
        {
            MoveTimeCount += Time.deltaTime / MoveDuration;
            this.transform.position = GetPointOnBezierCurve(BezierPosition_0, BezierPosition_1, BezierPosition_2, BezierPosition_3, moveCurve.Evaluate(MoveTimeCount / MoveDuration));
            this.transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, -20);
            if (MoveTimeCount >= MoveDuration)
            {
                MoveStart = false;
                MoveTimeCount = 0.0f;
                if (EndCallback != null)
                    EndCallback();
            }
        }
    }
}
