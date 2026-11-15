using NUnit.Framework;
using UnityEngine;
using UnityEditor;
using DodgeGame.Data;

namespace DodgeGame.Tests.EditMode
{
    [TestFixture]
    public class ConfigValidationTests
    {
        [Test]
        public void PlayerConfig_HasValidNonNegativeValues()
        {
            var config = AssetDatabase.LoadAssetAtPath<PlayerConfig>("Assets/ScriptableObjects/SO_PlayerConfig.asset");
            Assert.IsNotNull(config, "SO_PlayerConfig.asset should exist.");

            Assert.Greater(config.MoveSpeed, 0f, "MoveSpeed must be positive.");
            Assert.Greater(config.Acceleration, 0f, "Acceleration must be positive.");
            Assert.Greater(config.Deceleration, 0f, "Deceleration must be positive.");
            Assert.Greater(config.RotationSpeed, 0f, "RotationSpeed must be positive.");
            Assert.Greater(config.JumpForce, 0f, "JumpForce must be positive.");
            Assert.Greater(config.MaxLives, 0, "MaxLives must be greater than zero.");
            Assert.Greater(config.InvulnerabilityDuration, 0f, "InvulnerabilityDuration must be positive.");
            Assert.GreaterOrEqual(config.CoyoteTimeDuration, 0f, "CoyoteTimeDuration must not be negative.");
            Assert.GreaterOrEqual(config.JumpBufferDuration, 0f, "JumpBufferDuration must not be negative.");
        }

        [Test]
        public void DashConfig_HasValidPositiveDurationsAndDistance()
        {
            var config = AssetDatabase.LoadAssetAtPath<DashConfig>("Assets/ScriptableObjects/SO_DashConfig.asset")
                      ?? Resources.Load<DashConfig>("SO_DashConfig");
            Assert.IsNotNull(config, "SO_DashConfig should exist.");

            Assert.Greater(config.Distance, 0f, "Dash Distance must be positive.");
            Assert.Greater(config.Duration, 0f, "Dash Duration must be positive (cannot be zero).");
            Assert.Greater(config.Cooldown, 0f, "Dash Cooldown must be positive.");
            Assert.GreaterOrEqual(config.DashInvulnerability, 0f, "DashInvulnerability must not be negative.");
            Assert.Greater(config.BaseSpeed, 0f, "Dash BaseSpeed must be positive.");
        }

        [Test]
        public void SlowMotionConfig_HasValidClampedTimeScaleAndMeterSettings()
        {
            var config = AssetDatabase.LoadAssetAtPath<SlowMotionConfig>("Assets/ScriptableObjects/SO_SlowMotionConfig.asset")
                      ?? Resources.Load<SlowMotionConfig>("SO_SlowMotionConfig");
            Assert.IsNotNull(config, "SO_SlowMotionConfig should exist.");

            Assert.Greater(config.TimeScale, 0f, "TimeScale must be greater than zero.");
            Assert.Less(config.TimeScale, 1.0f, "TimeScale must dilate time below 1.0.");
            Assert.Greater(config.Duration, 0f, "Slow motion Duration must be positive.");
            Assert.Greater(config.MeterPerDodge, 0f, "MeterPerDodge must be positive.");
            Assert.GreaterOrEqual(config.MeterPenalty, 0f, "MeterPenalty must not be negative.");
            Assert.Greater(config.MeterMax, 0f, "MeterMax must be positive.");
        }

        [Test]
        public void DimensionConfig_HasValidShiftDurationAndCooldown()
        {
            var config = AssetDatabase.LoadAssetAtPath<DimensionConfig>("Assets/ScriptableObjects/SO_DimensionConfig.asset");
            Assert.IsNotNull(config, "SO_DimensionConfig should exist.");

            Assert.Greater(config.ShiftDuration, 0f, "ShiftDuration must be positive.");
            Assert.Greater(config.ShiftCooldown, 0f, "ShiftCooldown must be positive.");
            Assert.Greater(config.ShiftMoveMultiplier, 0f, "ShiftMoveMultiplier must be positive.");
            Assert.Greater(config.TransitionDuration, 0f, "TransitionDuration must be positive.");
        }

        [Test]
        public void JuiceConfig_HasValidScreenShakeAndHitStopSettings()
        {
            var config = AssetDatabase.LoadAssetAtPath<JuiceConfig>("Assets/ScriptableObjects/SO_Juice.asset")
                      ?? Resources.Load<JuiceConfig>("SO_Juice");
            Assert.IsNotNull(config, "SO_Juice should exist.");

            Assert.Greater(config.HitStopDuration, 0f, "HitStopDuration must be positive.");
            Assert.Greater(config.ShakeAmplitude, 0f, "ShakeAmplitude must be positive.");
            Assert.Greater(config.ShakeDuration, 0f, "ShakeDuration must be positive.");
            Assert.GreaterOrEqual(config.SquashAndStretchAmount, 0f, "SquashAndStretchAmount must not be negative.");
            Assert.Greater(config.BallSpawnPopDuration, 0f, "BallSpawnPopDuration must be positive.");
        }

        [Test]
        public void BallAndHazardConfigs_HaveValidNonZeroGameplayValues()
        {
            var ballConfig = AssetDatabase.LoadAssetAtPath<BallConfig>("Assets/ScriptableObjects/SO_BallConfig.asset");
            Assert.IsNotNull(ballConfig, "SO_BallConfig should exist.");
            Assert.Greater(ballConfig.Speed, 0f, "Ball speed must be positive.");
            Assert.Greater(ballConfig.Radius, 0f, "Ball radius must be positive.");
            Assert.Greater(ballConfig.Damage, 0, "Ball damage must be positive.");

            var hazardConfig = AssetDatabase.LoadAssetAtPath<HazardConfig>("Assets/ScriptableObjects/SO_HazardConfig.asset");
            Assert.IsNotNull(hazardConfig, "SO_HazardConfig should exist.");
            Assert.Greater(hazardConfig.FireTileDuration, 0f, "FireTileDuration must be positive.");
            Assert.Greater(hazardConfig.Damage, 0, "Hazard damage must be positive.");
            Assert.Greater(hazardConfig.DamageCooldown, 0f, "DamageCooldown must be positive.");
        }

        [Test]
        public void TypographyConfig_HasValidFontSizesForAllRoles()
        {
            var config = AssetDatabase.LoadAssetAtPath<TypographyConfig>("Assets/ScriptableObjects/SO_Typography.asset")
                      ?? Resources.Load<TypographyConfig>("SO_Typography");
            Assert.IsNotNull(config, "SO_Typography should exist.");

            Assert.Greater(config.Display.Size, 0f, "Display role size must be positive.");
            Assert.Greater(config.Heading.Size, 0f, "Heading role size must be positive.");
            Assert.Greater(config.Button.Size, 0f, "Button role size must be positive.");
            Assert.Greater(config.Label.Size, 0f, "Label role size must be positive.");
            Assert.Greater(config.Body.Size, 0f, "Body role size must be positive.");
            Assert.Greater(config.Caption.Size, 0f, "Caption role size must be positive.");
        }
    }
}
