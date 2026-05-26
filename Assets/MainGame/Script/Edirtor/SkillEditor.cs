#if UNITY_EDITOR
using UnityEngine;
using UnityEditor; //유니티 에디터를 사용합니다.

[CustomEditor (typeof (BaseBall.BallPlay.SkillEffectDisplayManager))] //여기에서 커스텀 에디터를 붙이기 위한 스크립트를 지정합니다.
public class SkillEditor: Editor {    // 모노가 아니라 Editor입니다.

    public override void OnInspectorGUI()   //OnInspectorGUI 에 오버라이드 해 줍니다.
    {
        base.OnInspectorGUI();
        BaseBall.BallPlay.SkillEffectDisplayManager saveData = target as BaseBall.BallPlay.SkillEffectDisplayManager;

        if (GUILayout.Button("저장"))
        {
            if (saveData)
            {
                saveData.saveData();
            }
        }

        if (GUILayout.Button("업데이트"))
        {
            if (saveData)
            {
                saveData.loadData();
            }
        }
    }
}


[CustomEditor(typeof(BaseBall.BallPlay.fieldSkillDisplayManager))] //여기에서 커스텀 에디터를 붙이기 위한 스크립트를 지정합니다.
public class FieldSkillEditor : Editor
{    // 모노가 아니라 Editor입니다.

    public override void OnInspectorGUI()   //OnInspectorGUI 에 오버라이드 해 줍니다.
    {
        base.OnInspectorGUI();
        BaseBall.BallPlay.fieldSkillDisplayManager saveData = target as BaseBall.BallPlay.fieldSkillDisplayManager;

        if (GUILayout.Button("필드저장"))
        {
            if (saveData)
            {
                saveData.saveData();
            }
        }

        if (GUILayout.Button("필드업데이트"))
        {
            if (saveData)
            {
                saveData.loadData();
            }
        }
    }
}
#endif