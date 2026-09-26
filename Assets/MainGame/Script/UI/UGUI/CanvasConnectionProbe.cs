#if UNITY_EDITOR
using UnityEngine;
public sealed class CanvasConnectionProbe : MonoBehaviour
{
    public int presses, releases, clicks, completions;
    public void Press() { presses++; }
    public void Release() { releases++; }
    public void Click() { clicks++; }
    public void Complete() { completions++; }
}
#endif
