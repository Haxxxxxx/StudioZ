using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(EventTrigger))]
[RequireComponent(typeof(Image))]
public class GridCell_PatternWeaving : MonoBehaviour
{
    [System.Serializable]
    public class MotifData
    {
        public Texture2D motif;
        public Color color;
        public GridCell_PatternWeaving nextCell;

        public MotifData(Texture2D motif, Color color, GridCell_PatternWeaving nextCell)
        {
            this.motif = motif;
            this.color = color;
            this.nextCell = nextCell;
        }
    }

    [SerializeField] private EventTrigger eventTrigger;
    [SerializeField] private Image image;

    public Vector2Int cellPos;
    [SerializeField] public List<MotifData> data = new List<MotifData>();
    public MotifData selectedData { get; private set; }
    private bool isNextCell = false;


    public void AddMotifData(Texture2D motif, Color color, GridCell_PatternWeaving nextCell = null)
    {
        if (!data.Exists(d => d.motif == motif))
        {
            data.Add(new MotifData(motif, color, nextCell));
        }
    }

    public void SetColor(Color color)
    {
        image.color = color;
    }

    public void SetSelectedData(Texture2D texture2D)
    {
        selectedData = data.Find(d => d.motif == texture2D);

        if (selectedData != null && selectedData.nextCell != null)
        {
            selectedData.nextCell.SetSelectedData(texture2D);
        }
    }

    public void SetIsNextCell()
    {
        isNextCell = true;
        image.color = new Color(selectedData.color.r, selectedData.color.g, selectedData.color.b, 0.5f);
        MG_PatternWeaving.instance.nextCell = this;
    }

    public void OnPointerEnter()
    {
        if (isNextCell)
        {
            image.color = selectedData.color;
            isNextCell = false;
            eventTrigger.enabled = false;

            if (selectedData.nextCell != null)
            {
                selectedData.nextCell.SetIsNextCell();
            }
        }
    }
}
