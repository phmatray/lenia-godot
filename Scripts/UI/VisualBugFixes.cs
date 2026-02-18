using Godot;
using System.Collections.Generic;

namespace Lenia.UI
{
    /// <summary>
    /// Handles only runtime/dynamic visual fixes that cannot be done in Godot's theme system
    /// Static styling should be done in lenia_theme.tres and scene files
    /// </summary>
    public partial class VisualBugFixes : Node
    {
        public override void _Ready()
        {
            // Wait a frame for all UI to be initialized
            CallDeferred(nameof(ApplyRuntimeFixes));
        }
        
        private void ApplyRuntimeFixes()
        {
            // Only apply fixes that must be done at runtime
            FixDynamicZIndexing();
            FixAccessibilityLabels();
            EnsureMinimumSizes();
        }
        
        /// <summary>
        /// Fix z-indexing for dynamically created overlays/popups
        /// This cannot be done in theme as it depends on node names and runtime hierarchy
        /// </summary>
        private void FixDynamicZIndexing()
        {
            var controls = new List<Control>();
            GetAllNodesOfType(GetTree().Root, controls);
            
            foreach (var control in controls)
            {
                var nodeName = control.Name.ToString();
                
                // Fix popup/overlay z-index
                if (nodeName.Contains("Overlay") || nodeName.Contains("Popup"))
                {
                    if (control.ZIndex < 100)
                        control.ZIndex = 1000;
                }
                
                // Fix tooltip z-index
                if (nodeName.Contains("Tooltip"))
                {
                    if (control.ZIndex < 1000)
                        control.ZIndex = 2000;
                }
            }
        }
        
        /// <summary>
        /// Add accessibility labels to controls that lack them
        /// This is runtime content that cannot be predefined in scenes
        /// </summary>
        private void FixAccessibilityLabels()
        {
            var buttons = new List<Button>();
            GetAllNodesOfType(GetTree().Root, buttons);
            
            foreach (var button in buttons)
            {
                // Add accessible names for icon-only buttons
                if (string.IsNullOrEmpty(button.TooltipText) && button.Text.Length <= 2)
                {
                    switch (button.Text)
                    {
                        case "▶":
                            button.TooltipText = "Play simulation";
                            break;
                        case "⏸":
                            button.TooltipText = "Pause simulation";
                            break;
                        case "⏭":
                            button.TooltipText = "Step one frame";
                            break;
                        case "🔄":
                            button.TooltipText = "Reset simulation";
                            break;
                        case "📷":
                        case "📸":
                            button.TooltipText = "Take screenshot";
                            break;
                        case "🔊":
                            button.TooltipText = "Toggle audio";
                            break;
                        case "✨":
                            button.TooltipText = "Toggle particle effects";
                            break;
                    }
                }
            }
        }
        
        /// <summary>
        /// Ensure minimum sizes for clickability on dynamically created controls
        /// Only needed for controls created at runtime, scene controls should have proper sizes
        /// </summary>
        private void EnsureMinimumSizes()
        {
            var buttons = new List<Button>();
            GetAllNodesOfType(GetTree().Root, buttons);
            
            foreach (var button in buttons)
            {
                // Only fix controls that were created dynamically (not from scenes)
                if (string.IsNullOrEmpty(button.SceneFilePath) && button.CustomMinimumSize == Vector2.Zero)
                {
                    // Set minimum clickable size for dynamically created buttons
                    button.CustomMinimumSize = new Vector2(35, 30);
                    
                    // Icon buttons need larger size
                    if (button.Text.Length <= 2 && (button.Text.Contains("▶") || 
                        button.Text.Contains("⏸") || button.Text.Contains("⏭") ||
                        button.Text.Contains("↺") || button.Text.Contains("📷") ||
                        button.Text.Contains("🔊") || button.Text.Contains("✨") ||
                        button.Text.Contains("🔄")))
                    {
                        button.CustomMinimumSize = new Vector2(40, 40);
                    }
                }
            }
        }
        
        private void GetAllNodesOfType<T>(Node parent, List<T> list) where T : Node
        {
            if (parent is T node)
                list.Add(node);
            
            foreach (Node child in parent.GetChildren())
            {
                GetAllNodesOfType(child, list);
            }
        }
        
        /// <summary>
        /// Apply visual fixes to a specific node tree (for dynamically created content)
        /// </summary>
        public static void ApplyVisualFixes(Node rootNode)
        {
            var fixer = new VisualBugFixes();
            rootNode.AddChild(fixer);
        }
    }
}