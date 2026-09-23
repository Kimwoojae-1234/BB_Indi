using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace BaseBall.BallPlay.UGUI
{
    [Serializable]
    public sealed class GameUIAction
    {
        [Serializable] public sealed class Parameter { public UnityEngine.Object obj; public string field; }
        public MonoBehaviour mTarget;
        public string mMethodName;
        public Parameter[] mParameters;
        public bool oneShot;
        private MethodInfo method;
        public void Invoke()
        {
            if (mTarget == null || string.IsNullOrEmpty(mMethodName)) return;
            if (method == null)
            {
                foreach (var candidate in mTarget.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
                    if (candidate.Name == mMethodName && candidate.GetParameters().Length == (mParameters?.Length ?? 0))
                    {
                        if (method != null) throw new AmbiguousMatchException(mTarget.GetType().Name + "." + mMethodName);
                        method = candidate;
                    }
                if (method == null) throw new MissingMethodException(mTarget.GetType().Name, mMethodName);
            }
            var args = new object[mParameters?.Length ?? 0];
            for (int i = 0; i < args.Length; i++)
            {
                var parameter = mParameters[i];
                object value = parameter.obj;
                if (value != null && !string.IsNullOrEmpty(parameter.field))
                {
                    var type = value.GetType();
                    var property = type.GetProperty(parameter.field, BindingFlags.Public | BindingFlags.Instance);
                    var field = type.GetField(parameter.field, BindingFlags.Public | BindingFlags.Instance);
                    if (property != null) value = property.GetValue(value);
                    else if (field != null) value = field.GetValue(value);
                    else throw new MissingMemberException(type.Name, parameter.field);
                }
                args[i] = value;
            }
            method.Invoke(mTarget, args);
        }
        public static void InvokeAll(List<GameUIAction> actions)
        {
            foreach (var action in actions.ToArray())
            {
                action.Invoke();
                if (action.oneShot) actions.Remove(action);
            }
        }
    }
}
