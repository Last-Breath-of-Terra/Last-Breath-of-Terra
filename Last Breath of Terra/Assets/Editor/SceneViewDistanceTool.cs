using UnityEngine;
using UnityEditor;

[InitializeOnLoad]
public class SceneViewDistanceTool
{
    private static Vector3? firstPoint = null;
    private static Vector3? secondPoint = null;
    private static bool isMeasuring = false;

    static SceneViewDistanceTool()
    {
        SceneView.duringSceneGui += OnSceneGUI;
    }

    private static void OnSceneGUI(SceneView sceneView)
    {
        Event e = Event.current;

        if (e.type == EventType.KeyDown && e.keyCode == KeyCode.J)
        {
            isMeasuring = !isMeasuring;
            if (!isMeasuring)
            {
                firstPoint = null;
                secondPoint = null;
            }
            e.Use();
        }

        if (!isMeasuring) return;

        if (e.type == EventType.MouseDown && e.button == 0 && !e.alt)
        {
            Vector3 worldPosition = HandleUtility.GUIPointToWorldRay(e.mousePosition).origin;

            if (firstPoint == null)
            {
                firstPoint = worldPosition;
            }
            else
            {
                secondPoint = worldPosition;
            }
            e.Use();
        }

        if (firstPoint != null && secondPoint != null)
        {
            float distance = Vector3.Distance(firstPoint.Value, secondPoint.Value);

            Handles.color = Color.green;
            Handles.DrawLine(firstPoint.Value, secondPoint.Value);

            Vector3 midPoint = (firstPoint.Value + secondPoint.Value) / 2;
            Handles.Label(midPoint, $"Distance: {distance:F2} units");

            if (e.type == EventType.MouseDown && e.button == 1)
            {
                firstPoint = null;
                secondPoint = null;
                e.Use();
            }
        }

        sceneView.Repaint();
    }
}
