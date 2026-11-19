using System;
using UnityEngine;

[ExecuteAlways]
public class CityParallax : MonoBehaviour
{
    [Serializable] public class Layer
    {
        public Sprite sprite;
        public float speed;
        public SpriteRenderer[] tiles;
        [NonSerialized] public float offset;
    }
    public Layer[] layers;
    public Camera view;
    public GeneradorObstaculos generator;

    void LateUpdate()
    {
        float travel = 0;
        if (Application.isPlaying && GameManager.Instance != null)
        {
            if (GameManager.Instance.PartidaActiva && generator != null)
                travel = generator.VelocidadActual * Time.deltaTime;
            else if (!GameManager.Instance.player.activeSelf)
                travel = 0.25f * Time.unscaledDeltaTime;
        }
        Arrange(travel);
    }
    public void Arrange(float travel = 0)
    {
        if (view == null || layers == null) return;
        float height = view.orthographicSize * 2f + 0.04f;
        float halfWidth = view.orthographicSize * view.aspect;
        foreach (var layer in layers)
        {
            if (layer.sprite == null || layer.tiles == null) continue;
            float scale = height / layer.sprite.bounds.size.y;
            float width = layer.sprite.bounds.size.x * scale;
            layer.offset = Mathf.Repeat(layer.offset + travel * layer.speed, width * 2);
            int first = Mathf.FloorToInt((-halfWidth + layer.offset) / width) - 1;
            for (int i = 0; i < layer.tiles.Length; i++)
            {
                var tile = layer.tiles[i]; if (tile == null) continue;
                int index = first + i;
                tile.sprite = layer.sprite;
                tile.flipX = (index & 1) != 0;
                tile.transform.localScale = Vector3.one * scale;
                tile.transform.position = new Vector3(view.transform.position.x + (index + 0.5f) * width - layer.offset,
                    view.transform.position.y, 0);
            }
        }
    }
}
