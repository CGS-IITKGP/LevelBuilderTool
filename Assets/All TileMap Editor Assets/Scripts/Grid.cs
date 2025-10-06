using System.Collections.Generic;
using UnityEngine;

public class Grid : MonoBehaviour
{
    public GridTileData gridTileData;
    public List<Layer> layers = new();
    public int selectedLayer = 0;

    //temp
    public int startX = -5;
    public int startZ = -5;
    public int endX = 5;
    public int endZ = 5;

    private void OnDrawGizmos()
    {
        Layer layer = layers[selectedLayer];
        float tileWidth = layer.tileWidth;
        float gridY = layer.gridYPos;
        Vector3 offset = layer.currentBrushPosition - new Vector3(tileWidth / 2f, 0, tileWidth / 2f);

        // --- Get mouse world position in Scene view ---
        Vector3 mouseWorldPos = offset;

        // --- Define fade parameters ---
        float maxFadeDistance = 5f;   // distance at which lines are fully invisible
        float minFadeDistance = 1.5f;  // distance for full opacity

        // --- Draw grid lines with fading ---
        for (int i = startX; i <= endX; i++)
        {
            for (int j = startZ; j <= endZ; j++)
            {
                Vector3 worldPos = new Vector3(i * tileWidth, gridY, j * tileWidth) + offset;
                float dist = Vector3.Distance(mouseWorldPos, worldPos);

                // Calculate alpha using inverse lerp
                float alpha = Mathf.InverseLerp(maxFadeDistance, minFadeDistance, dist);
                alpha = Mathf.Clamp01(alpha);

                Gizmos.color = new Color(1, 1, 1, alpha);

                // Draw horizontal and vertical lines around this point
                if (i < endX)
                {
                    Vector3 right = new Vector3((i + 1) * tileWidth, gridY, j * tileWidth) + offset;
                    Gizmos.DrawLine(worldPos, right);
                }

                if (j < endZ)
                {
                    Vector3 forward = new Vector3(i * tileWidth, gridY, (j + 1) * tileWidth) + offset;
                    Gizmos.DrawLine(worldPos, forward);
                }
            }
        }
    }

}
