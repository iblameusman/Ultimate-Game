using UnityEngine;

namespace DodgeGame.Data
{
    [CreateAssetMenu(fileName = "NewDimensionConfig", menuName = "DodgeGame/DimensionConfig")]
    public class DimensionConfig : ScriptableObject
    {
        [Header("Timing")]
        [Tooltip("Duration in seconds that the dimension shift remains active.")]
        [SerializeField] private float shiftDuration = 1.5f;

        [Tooltip("Cooldown in seconds after a shift ends before it can be triggered again.")]
        [SerializeField] private float shiftCooldown = 5.0f;

        [Header("Movement")]
        [Tooltip("Movement speed multiplier applied to the player while shifted (e.g. 0.7 = 70% speed).")]
        [SerializeField] private float shiftMoveMultiplier = 0.7f;

        [Header("Visuals")]
        [Tooltip("Alpha opacity applied to player mesh renderer while shifted.")]
        [SerializeField] private float shiftAlpha = 0.4f;

        [Tooltip("Color tint applied to arena floor and walls while dimension shift is active.")]
        [SerializeField] private Color shiftTint = new Color(0.25f, 0.75f, 1.0f, 1.0f);

        [Tooltip("Transition duration in seconds when blending visual tints on shift start/end.")]
        [SerializeField] private float transitionDuration = 0.2f;

        public float ShiftDuration => shiftDuration;
        public float ShiftCooldown => shiftCooldown;
        public float ShiftMoveMultiplier => shiftMoveMultiplier;
        public float ShiftAlpha => shiftAlpha;
        public Color ShiftTint => shiftTint;
        public float TransitionDuration => transitionDuration;
    }
}
