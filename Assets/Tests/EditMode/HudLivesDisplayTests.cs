using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
using DodgeGame.Data;
using DodgeGame.UI;

namespace DodgeGame.Tests.EditMode
{
    [TestFixture]
    public class HudLivesDisplayTests
    {
        private GameObject rootGo;
        private LivesDisplayUI livesUI;
        private List<LivesDisplayUI.HeartSlot> slots;
        private HudTheme theme;

        [SetUp]
        public void Setup()
        {
            rootGo = new GameObject("TopRight_Group", typeof(RectTransform));
            livesUI = rootGo.AddComponent<LivesDisplayUI>();
            theme = ScriptableObject.CreateInstance<HudTheme>();

            var containerGo = new GameObject("HeartsRow", typeof(RectTransform));
            containerGo.transform.SetParent(rootGo.transform, false);

            slots = new List<LivesDisplayUI.HeartSlot>();
            for (int i = 0; i < 5; i++)
            {
                var slotGo = new GameObject($"Heart_{i}", typeof(RectTransform));
                slotGo.transform.SetParent(containerGo.transform, false);
                var slotRect = slotGo.GetComponent<RectTransform>();
                slotRect.sizeDelta = new Vector2(48f, 48f);

                var emptyGo = new GameObject("Empty", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                emptyGo.transform.SetParent(slotGo.transform, false);
                var emptyImg = emptyGo.GetComponent<Image>();
                emptyImg.color = theme.CooldownFill;

                var fullGo = new GameObject("Full", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(CanvasGroup));
                fullGo.transform.SetParent(slotGo.transform, false);
                var fullImg = fullGo.GetComponent<Image>();
                fullImg.color = theme.Danger;
                var fullCg = fullGo.GetComponent<CanvasGroup>();

                var slot = new LivesDisplayUI.HeartSlot
                {
                    Root = slotRect,
                    EmptyIcon = emptyImg,
                    FullIcon = fullImg,
                    FullCg = fullCg
                };
                slots.Add(slot);
            }

            // Wire via reflection
            var slotsField = typeof(LivesDisplayUI).GetField("heartSlots", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            slotsField?.SetValue(livesUI, slots);

            var themeField = typeof(LivesDisplayUI).GetField("theme", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            themeField?.SetValue(livesUI, theme);
        }

        [TearDown]
        public void Teardown()
        {
            if (rootGo != null) Object.DestroyImmediate(rootGo);
            if (theme != null) Object.DestroyImmediate(theme);
        }

        [Test]
        public void LivesDisplay_Matches_PlayerHealthLives_ForValues_0_Through_5()
        {
            for (int lives = 0; lives <= 5; lives++)
            {
                livesUI.SetLivesImmediate(lives);
                Assert.AreEqual(lives, livesUI.CurrentLives, $"CurrentLives should be {lives}.");

                for (int i = 0; i < 5; i++)
                {
                    bool shouldBeFull = i < lives;
                    Assert.AreEqual(shouldBeFull, slots[i].FullIcon.gameObject.activeSelf, $"Slot {i} full state mismatch for lives count {lives}.");
                    if (shouldBeFull)
                    {
                        Assert.AreEqual(1f, slots[i].FullCg.alpha, 0.05f);
                    }
                    else
                    {
                        Assert.AreEqual(0f, slots[i].FullCg.alpha, 0.05f);
                    }
                }
            }
        }

        [Test]
        public void HeartSlots_Are48px_Each()
        {
            for (int i = 0; i < slots.Count; i++)
            {
                Assert.AreEqual(48f, slots[i].Root.sizeDelta.x, 0.5f, $"Heart slot {i} width must be 48px.");
                Assert.AreEqual(48f, slots[i].Root.sizeDelta.y, 0.5f, $"Heart slot {i} height must be 48px.");
            }
        }

        [Test]
        public void HeartColors_MatchSpecification_DangerAndCooldownFill()
        {
            for (int i = 0; i < slots.Count; i++)
            {
                // Full heart: Danger color (#E5484D)
                Assert.AreEqual(theme.Danger.r, slots[i].FullIcon.color.r, 0.05f);
                Assert.AreEqual(theme.Danger.g, slots[i].FullIcon.color.g, 0.05f);
                Assert.AreEqual(theme.Danger.b, slots[i].FullIcon.color.b, 0.05f);

                // Lost heart: CooldownFill color (#9AA6AF)
                Assert.AreEqual(theme.CooldownFill.r, slots[i].EmptyIcon.color.r, 0.05f);
                Assert.AreEqual(theme.CooldownFill.g, slots[i].EmptyIcon.color.g, 0.05f);
                Assert.AreEqual(theme.CooldownFill.b, slots[i].EmptyIcon.color.b, 0.05f);
            }
        }
    }
}
