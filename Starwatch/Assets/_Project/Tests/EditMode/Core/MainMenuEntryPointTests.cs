using System.Collections.Generic;
using System.Reflection;
using ProjectStarwatch.Core.Bootstrap;
using ProjectStarwatch.Core.Run;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UIElements;

namespace ProjectStarwatch.Tests.EditMode.Core
{
    /// <summary>
    /// Main menu scenario list and seed/dice wiring against a stand-in visual tree.
    /// </summary>
    public sealed class MainMenuEntryPointTests
    {
        [Test]
        public void SetScenarios_SelectsDefaultAndDiceFillsSeed()
        {
            var root = BuildTree();
            var bridge = CreateBridge(root);

            bridge.SetScenarios(new List<string>
            {
                RunStartConfiguration.DefaultScenarioId,
                "other-scenario",
            });

            Assert.That(bridge.ScenarioId, Is.EqualTo(RunStartConfiguration.DefaultScenarioId));

            bridge.RollSeed();

            Assert.That(bridge.SeedFieldText, Is.Not.Empty);
            Assert.That(bridge.TryGetSeed(out var seed), Is.True);
            Assert.That(seed, Is.GreaterThan(0));
            Object.DestroyImmediate(bridge.gameObject);
        }

        [Test]
        public void TryGetSeed_ReadsOptionalSeedField()
        {
            var root = BuildTree();
            var bridge = CreateBridge(root);
            bridge.SetScenarios(new List<string> { "authored-vertical-slice" });
            root.Q<TextField>("seed-field").value = "42";

            Assert.That(bridge.ScenarioId, Is.EqualTo("authored-vertical-slice"));
            Assert.That(bridge.TryGetSeed(out var seed), Is.True);
            Assert.That(seed, Is.EqualTo(42));

            root.Q<TextField>("seed-field").value = string.Empty;
            Assert.That(bridge.TryGetSeed(out _), Is.False);
            Object.DestroyImmediate(bridge.gameObject);
        }

        private static VisualElement BuildTree()
        {
            var root = new VisualElement { name = "root" };
            var title = new VisualElement { name = "title-screen" };
            title.Add(new Button { name = "new-run-button" });
            title.Add(new Button { name = "quit-button" });
            root.Add(title);

            var panel = new VisualElement { name = "new-run-panel" };
            panel.Add(new DropdownField { name = "scenario-dropdown" });
            panel.Add(new TextField { name = "seed-field" });
            panel.Add(new Button { name = "dice-button" });
            panel.Add(new Button { name = "back-button" });
            panel.Add(new Button { name = "launch-button" });
            root.Add(panel);
            return root;
        }

        private static MainMenuViewBridge CreateBridge(VisualElement root)
        {
            var go = new GameObject("MainMenuTest");
            var bridge = go.AddComponent<MainMenuViewBridge>();
            var flags = BindingFlags.NonPublic | BindingFlags.Instance;
            var type = typeof(MainMenuViewBridge);
            type.GetField("_titleScreen", flags)?.SetValue(bridge, root.Q("title-screen"));
            type.GetField("_newRunPanel", flags)?.SetValue(bridge, root.Q("new-run-panel"));
            type.GetField("_scenarioDropdown", flags)?.SetValue(bridge, root.Q<DropdownField>("scenario-dropdown"));
            type.GetField("_seedField", flags)?.SetValue(bridge, root.Q<TextField>("seed-field"));
            type.GetField("_launchButton", flags)?.SetValue(bridge, root.Q<Button>("launch-button"));
            return bridge;
        }
    }
}
