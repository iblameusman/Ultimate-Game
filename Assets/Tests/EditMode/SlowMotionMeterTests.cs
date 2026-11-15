using NUnit.Framework;
using UnityEngine;
using DodgeGame.Core;
using DodgeGame.Player;
using DodgeGame.Data;

namespace DodgeGame.Tests.EditMode
{
    [TestFixture]
    public class SlowMotionMeterTests
    {
        private GameObject meterObject;
        private SlowMotionMeter meter;

        [SetUp]
        public void SetUp()
        {
            GameEvents.ClearAllListeners();
            meterObject = new GameObject("TestSlowMotionMeter");
            meter = meterObject.AddComponent<SlowMotionMeter>();
            meter.Initialize();
        }

        [TearDown]
        public void TearDown()
        {
            GameEvents.ClearAllListeners();
            if (meterObject != null)
            {
                Object.DestroyImmediate(meterObject);
            }
        }

        [Test]
        public void InitialMeter_IsZeroAndNotReady()
        {
            meter.ResetMeter();
            Assert.AreEqual(0f, meter.CurrentMeter, "Initial meter must start at 0.");
            Assert.IsFalse(meter.IsReady, "Meter should not be ready at 0 charge.");
        }

        [Test]
        public void AddMeter_ClampsToMeterMax()
        {
            meter.ResetMeter();
            meter.AddMeter(meter.MeterMax + 50f);

            Assert.AreEqual(meter.MeterMax, meter.CurrentMeter, "Meter must clamp to MeterMax (100).");
            Assert.IsTrue(meter.IsReady, "Meter must be ready when maxed.");
        }

        [Test]
        public void DodgeEvent_AddsMeterCorrectly()
        {
            meter.ResetMeter();
            float initial = meter.CurrentMeter;

            GameEvents.TriggerPlayerDodged(1);

            Assert.Greater(meter.CurrentMeter, initial, "Dodge event must increase meter charge.");
            Assert.LessOrEqual(meter.CurrentMeter, meter.MeterMax, "Meter must not exceed MeterMax.");
        }

        [Test]
        public void HitEvent_SubtractsMeterAndClampsToZero()
        {
            meter.ResetMeter();
            meter.AddMeter(20f);

            GameEvents.TriggerPlayerHit(2);

            Assert.Less(meter.CurrentMeter, 20f, "Taking a hit must deduct meter charge.");

            // Test underflow clamp
            meter.ResetMeter();
            GameEvents.TriggerPlayerHit(1);
            Assert.AreEqual(0f, meter.CurrentMeter, "Meter must clamp at 0 and never become negative.");
        }

        [Test]
        public void OnMeterFull_FiresWhenChargeReachesMax()
        {
            meter.ResetMeter();
            bool fullFired = false;
            meter.OnMeterFull += () => fullFired = true;

            meter.AddMeter(meter.MeterMax);

            Assert.IsTrue(fullFired, "OnMeterFull event must fire when meter reaches max.");
            Assert.IsTrue(meter.IsReady, "IsReady must be true when meter is full.");
        }

        [Test]
        public void ConsumeMeter_ResetsMeterToZero()
        {
            meter.ResetMeter();
            meter.AddMeter(meter.MeterMax);
            Assert.IsTrue(meter.IsReady);

            bool consumed = meter.ConsumeMeter();

            Assert.IsTrue(consumed, "ConsumeMeter must succeed when meter is ready.");
            Assert.AreEqual(0f, meter.CurrentMeter, "ConsumeMeter must reset current meter to 0.");
            Assert.IsFalse(meter.IsReady, "Meter must not be ready after consumption.");
        }
    }
}
