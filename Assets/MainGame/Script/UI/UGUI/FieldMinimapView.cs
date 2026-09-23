using UnityEngine;

namespace BaseBall.BallPlay
{
    public sealed class FieldMinimapView : MonoBehaviour
    {
        public FieldOverlayCanvas rendering;
        public Transform runners;
        // NGUI draws every team icon below every namebar, then every name.
        public Transform teams;
        public Transform namebars;
        public Transform names;
    }
}
