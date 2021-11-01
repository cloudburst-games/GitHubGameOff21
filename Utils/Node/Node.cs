// Node utility class
using Godot;
using System;

namespace BaseProject.Utils
{
    public static class Node
    {
        // Hide children - nodes that can hide are canvasitem or spatial nodes
        public static void RecursiveModVisibility(Godot.Node parent, bool show)
        {
            // There is a bug with LineEdit - the context menu appears. This bypasses the bug.
            // However, if we need to hide or show LineEdits we should make their parents Spatials or CanvasItems
            if (parent is LineEdit) 
                return;
            if (parent is CanvasItem p)
                p.Visible = show;

            foreach (Godot.Node n in parent.GetChildren()){
                if (n.GetChildCount() > 0)
                    RecursiveModVisibility(n, show);

                if (n is CanvasItem c)
                {
                    c.Visible = show;
                }
                else if (n is Spatial s)
                {
                    s.Visible = show;;
                }
            }

        }

        public static void Reparent(Godot.Node node, Godot.Node newParent)
        {
            node.GetParent().RemoveChild(node);
            newParent.AddChild(node);
        }

    }
}