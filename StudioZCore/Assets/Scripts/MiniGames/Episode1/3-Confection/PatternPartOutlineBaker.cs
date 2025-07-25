#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class PatternPartOutlineBaker : EditorWindow
{
    private int sampleEvery = 3;

    [MenuItem("Tools/MG3/Bake Pattern Parts Outline")]
    public static void Open()
    {
        GetWindow<PatternPartOutlineBaker>("Pattern Part Outline Baker");
    }

    private void OnGUI()
    {
        GUILayout.Label("Bake des contours (0..1) depuis le Physics Shape des sprites", EditorStyles.boldLabel);
        sampleEvery = EditorGUILayout.IntSlider("Sample Every", sampleEvery, 1, 10);

        if (GUILayout.Button("Baker pour les PatternPart_SO sélectionnés"))
        {
            BakeSelected(sampleEvery);
        }
    }

    private void BakeSelected(int step)
    {
        Object[] selection = Selection.objects;
        int bakedCount = 0;

        foreach (var obj in selection)
        {
            if (obj is PatternPart_SO part)
            {
                if (part.partSprite == null)
                {
                    Debug.LogWarning($"[{part.name}] Aucun sprite assigné, ignoré.");
                    continue;
                }

                BakeOne(part, step);
                bakedCount++;
            }
        }

        AssetDatabase.SaveAssets();
        Debug.Log($"Baking terminé. {bakedCount} PatternPart_SO mis à jour.");
    }

    private void BakeOne(PatternPart_SO part, int step)
    {
        Sprite sprite = part.partSprite;

        int shapeCount = sprite.GetPhysicsShapeCount();
        if (shapeCount == 0)
        {
            Debug.LogWarning($"[{part.name}] Le sprite n'a pas de Physics Shape. Ouvre le Sprite Editor et définis-en un.");
            return;
        }

        var baked = new List<Vector2>();
        var shape = new List<Vector2>();

        for (int s = 0; s < shapeCount; s++)
        {
            shape.Clear();
            sprite.GetPhysicsShape(s, shape);

            for (int i = 0; i < shape.Count; i += step)
            {
                Vector2 p = shape[i];
                Vector2 n = SpritePointToNormalized(sprite, p);
                baked.Add(n);
            }
        }

        Undo.RecordObject(part, "Bake Pattern Outline");
        part.bakedOutline01 = baked;
        EditorUtility.SetDirty(part);

        Debug.Log($"[{part.name}] {baked.Count} points baked.");
    }

    private Vector2 SpritePointToNormalized(Sprite sprite, Vector2 p)
    {
        // Sprite.bounds est en unités monde. PhysicsShape est aussi en unités monde relatives au sprite.
        var b = sprite.bounds;
        float nx = (p.x - b.min.x) / b.size.x;
        float ny = (p.y - b.min.y) / b.size.y;
        return new Vector2(nx, ny);
    }
}
#endif
