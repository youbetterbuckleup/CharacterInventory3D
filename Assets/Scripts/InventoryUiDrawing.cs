using UnityEngine;

public static class InventoryUiDrawing
{
    public static readonly Color Cream = new Color(1f, 0.97f, 0.9f);
    public static readonly Color Ink = new Color(0.13f, 0.24f, 0.23f);
    public static readonly Color Teal = new Color(0.15f, 0.46f, 0.4f);
    public static readonly Color Mint = new Color(0.8f, 0.9f, 0.79f);
    public static readonly Color Pale = new Color(0.94f, 0.92f, 0.83f);
    public static readonly Color Muted = new Color(0.38f, 0.45f, 0.4f);

    public static void Shape(Rect rect, Color color, float radius)
    {
        GUI.DrawTexture(rect, Texture2D.whiteTexture, ScaleMode.StretchToFill,
            true, 0f, color, 0f, radius);
    }

    public static void Card(Rect rect, float radius)
    {
        Shape(new Rect(rect.x, rect.y + 6f, rect.width, rect.height), new Color(0.03f, 0.08f, 0.07f, 0.3f), radius);
        Shape(rect, Color.white, radius);
        Shape(new Rect(rect.x + 3f, rect.y + 3f, rect.width - 6f, rect.height - 6f), Cream, radius - 2f);
    }

    public static void Potion(Rect rect, Color color)
    {
        Matrix4x4 previous = GUI.matrix;
        GUI.matrix = previous * Matrix4x4.TRS(new Vector3(rect.x, rect.y, 0f),
            Quaternion.identity, new Vector3(rect.width / 64f, rect.height / 64f, 1f));
        Shape(new Rect(23f, 3f, 18f, 13f), Ink, 4f);
        Shape(new Rect(26f, 5f, 12f, 7f), new Color(0.87f, 0.62f, 0.33f), 2f);
        Shape(new Rect(20f, 12f, 24f, 22f), Ink, 5f);
        Shape(new Rect(9f, 25f, 46f, 36f), Ink, 14f);
        Shape(new Rect(24f, 15f, 16f, 17f), Cream, 3f);
        Shape(new Rect(13f, 27f, 38f, 30f), color, 11f);
        Shape(new Rect(19f, 32f, 5f, 11f), new Color(1f, 1f, 1f, 0.6f), 3f);
        Shape(new Rect(29f, 36f, 6f, 15f), Cream, 2f);
        Shape(new Rect(24f, 41f, 16f, 5f), Cream, 2f);
        GUI.matrix = previous;
    }
}
