using UnityEngine;

public class CameraScaler : MonoBehaviour
{
    private float referenceWidth = 1600f;
    private float referenceHeight = 720f;
    private float referenceOrthoSize = 5f;

    void Start()
    {
        Camera cam = GetComponent<Camera>();

        float screenRatio = (float)Screen.width / Screen.height;
        float referenceRatio = referenceWidth / referenceHeight;

        // Ajuste la taille orthographique selon la différence de ratio entre appareil et référence
        if (screenRatio >= referenceRatio)
        {
            // écran plus large que la référence -> on garde la hauteur fixe
            cam.orthographicSize = referenceOrthoSize;
        }
        else
        {
            // écran plus étroit -> on ajuste pour garder la largeur visible constante
            float scale = referenceRatio / screenRatio;
            cam.orthographicSize = referenceOrthoSize * scale;
        }
    }
}
