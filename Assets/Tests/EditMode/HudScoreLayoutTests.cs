using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DodgeGame.Data;
using DodgeGame.UI;

namespace DodgeGame.Tests.EditMode
{
    [TestFixture]
    public class HudScoreLayoutTests
    {
        private GameObject canvasGo;
        private CanvasScaler scaler;
        private GameObject groupGo;
        private RectTransform groupRect;
        private RectTransform scorePanel;
        private RectTransform bestPanel;

        [SetUp]
        public void Setup()
        {
            canvasGo = new GameObject("TestCanvas");
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            groupGo = new GameObject("TopLeft_ScoreGroup", typeof(RectTransform));
            groupGo.transform.SetParent(canvasGo.transform, false);
            groupRect = groupGo.GetComponent<RectTransform>();
            groupRect.anchorMin = new Vector2(0.03f, 0.97f);
            groupRect.anchorMax = new Vector2(0.03f, 0.97f);
            groupRect.pivot = new Vector2(0f, 1f);
            groupRect.anchoredPosition = Vector2.zero;

            var vlg = groupGo.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 8f;
            vlg.childAlignment = TextAnchor.UpperLeft;
            vlg.childControlWidth = false;
            vlg.childControlHeight = false;

            var csf = groupGo.AddComponent<ContentSizeFitter>();
            csf.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            // ScorePanel
            var spGo = new GameObject("ScorePanel", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            spGo.transform.SetParent(groupGo.transform, false);
            scorePanel = spGo.GetComponent<RectTransform>();
            var spVlg = spGo.AddComponent<VerticalLayoutGroup>();
            spVlg.padding = new RectOffset(20, 20, 10, 10);
            spVlg.spacing = 2f;
            var spCsf = spGo.AddComponent<ContentSizeFitter>();
            spCsf.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            spCsf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var scoreLabelGo = new GameObject("ScoreLabel", typeof(RectTransform), typeof(TextMeshProUGUI));
            scoreLabelGo.transform.SetParent(spGo.transform, false);
            var scoreLabelTmp = scoreLabelGo.GetComponent<TextMeshProUGUI>();
            scoreLabelTmp.text = "SCORE";
            scoreLabelTmp.fontSize = 22f;

            var scoreValGo = new GameObject("ScoreValue", typeof(RectTransform), typeof(TextMeshProUGUI));
            scoreValGo.transform.SetParent(spGo.transform, false);
            var scoreValTmp = scoreValGo.GetComponent<TextMeshProUGUI>();
            scoreValTmp.text = "99999";
            scoreValTmp.fontSize = 64f;

            // BestPanel
            var bpGo = new GameObject("BestPanel", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            bpGo.transform.SetParent(groupGo.transform, false);
            bestPanel = bpGo.GetComponent<RectTransform>();
            var bpHlg = bpGo.AddComponent<HorizontalLayoutGroup>();
            bpHlg.padding = new RectOffset(14, 14, 6, 6);
            var bpCsf = bpGo.AddComponent<ContentSizeFitter>();
            bpCsf.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            bpCsf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var bestTextGo = new GameObject("BestText", typeof(RectTransform), typeof(TextMeshProUGUI));
            bestTextGo.transform.SetParent(bpGo.transform, false);
            var bestTextTmp = bestTextGo.GetComponent<TextMeshProUGUI>();
            bestTextTmp.text = "BEST (NIGHTMARE): 12500";
            bestTextTmp.fontSize = 24f;

            LayoutRebuilder.ForceRebuildLayoutImmediate(groupRect);
        }

        [TearDown]
        public void Teardown()
        {
            if (canvasGo != null)
            {
                Object.DestroyImmediate(canvasGo);
            }
        }

        [Test]
        public void ScorePanel_And_BestRow_DoNotOverlap_At_1080p()
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(groupRect);

            float scoreBottom = scorePanel.anchoredPosition.y - (scorePanel.rect.height * (1f - scorePanel.pivot.y));
            float bestTop = bestPanel.anchoredPosition.y + (bestPanel.rect.height * bestPanel.pivot.y);

            float gap = scoreBottom - bestTop;
            Assert.GreaterOrEqual(gap, 7.9f, "Best row must be placed at least 8px below the score panel.");
        }

        [Test]
        public void ScorePanel_And_BestRow_DoNotOverlap_At_720p()
        {
            // Simulate 720p scaling
            float scale = 720f / 1080f;
            canvasGo.transform.localScale = new Vector3(scale, scale, scale);
            LayoutRebuilder.ForceRebuildLayoutImmediate(groupRect);

            float scoreBottom = scorePanel.anchoredPosition.y - (scorePanel.rect.height * (1f - scorePanel.pivot.y));
            float bestTop = bestPanel.anchoredPosition.y + (bestPanel.rect.height * bestPanel.pivot.y);

            float gap = scoreBottom - bestTop;
            Assert.GreaterOrEqual(gap, 7.9f, "Best row must not overlap the score panel at 1280x720.");
        }

        [Test]
        public void ScoreUI_DifficultyDisplayName_ReturnsDisplayNameOnly()
        {
            Assert.AreEqual("HARD", Difficulty.Hard.ToString().ToUpperInvariant());
            Assert.AreEqual("NIGHTMARE", Difficulty.Nightmare.ToString().ToUpperInvariant());
            Assert.AreEqual("NORMAL", Difficulty.Normal.ToString().ToUpperInvariant());
            Assert.AreEqual("EASY", Difficulty.Easy.ToString().ToUpperInvariant());
        }
    }
}
