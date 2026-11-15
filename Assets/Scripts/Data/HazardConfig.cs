using UnityEngine;

namespace DodgeGame.Data
{
    [CreateAssetMenu(fileName = "HazardConfig", menuName = "DodgeGame/Hazard Config")]
    public class HazardConfig : ScriptableObject
    {
        [Header("Fire Tile")]
        [Tooltip("Lifetime of a fire tile before returning to the pool (seconds).")]
        [SerializeField] private float fireTileDuration = 4f;

        [Tooltip("Damage dealt to the player on contact.")]
        [SerializeField] private int damage = 1;

        [Tooltip("Cooldown between repeated damage ticks on the same tile (seconds).")]
        [SerializeField] private float damageCooldown = 1f;

        [Tooltip("Duration of fade out over the end of the tile's lifetime (seconds).")]
        [SerializeField] private float fadeDuration = 1f;

        public float FireTileDuration => fireTileDuration;
        public int Damage => damage;
        public float DamageCooldown => damageCooldown;
        public float FadeDuration => fadeDuration;
    }
}
