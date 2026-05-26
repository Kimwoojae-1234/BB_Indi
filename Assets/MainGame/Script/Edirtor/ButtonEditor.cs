#if UNITY_EDITOR

using UnityEngine;
using UnityEditor; //유니티 에디터를 사용합니다.

[CustomEditor(typeof(buttonActionSmall))] 
public class ButtonEditor : Editor {

    public override void OnInspectorGUI()   //OnInspectorGUI 에 오버라이드 해 줍니다.
    {
        base.OnInspectorGUI();
        buttonActionSmall data = target as buttonActionSmall;

        if (GUILayout.Button("사이즈 세팅"))
        {
            if (data)
            {
                data.setData();
            }
        }

    }
}
#endif