using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MiniGames
{
    namespace Episode1
    {
        [RequireComponent(typeof(EventTrigger))]
        [RequireComponent(typeof(Image))]
        public class GridCell_PatternWeaving : MonoBehaviour
        {
            [System.Serializable]
            public class PatternData
            {
                public Texture2D texture;
                public Color color;
                public GridCell_PatternWeaving nextCell;

                public PatternData(Texture2D texture, Color color, GridCell_PatternWeaving nextCell)
                {
                    this.texture = texture;
                    this.color = color;
                    this.nextCell = nextCell;
                }
            }

            [Header("References")]
            [SerializeField] private EventTrigger eventTrigger;
            [SerializeField] private Image image;
            private MG4_PatternWeaving mgPatternWeaving;

            public Vector2Int cellPos;
            public List<PatternData> data = new List<PatternData>();
            public PatternData selectedData { get; private set; }
            private bool isNextCell = false;

            private void Start()
            {
                mgPatternWeaving = MG4_PatternWeaving.instance;
            }

            public void AddPatternData(Texture2D texture, Color color, GridCell_PatternWeaving nextCell = null)
            {
                if (!data.Exists(d => d.texture == texture))
                {
                    data.Add(new PatternData(texture, color, nextCell));
                }
            }

            public void SetColor(Color color)
            {
                image.color = color;
            }

            public void SetSelectedData(Texture2D texture)
            {
                selectedData = data.Find(d => d.texture == texture);
                eventTrigger.enabled = true;
                isNextCell = false;

                if (selectedData != null && selectedData.nextCell != null)
                {
                    selectedData.nextCell.SetSelectedData(texture);
                }
            }

            public void SetIsNextCell()
            {
                isNextCell = true;
                image.color = new Color(selectedData.color.r, selectedData.color.g, selectedData.color.b, 0.5f);
                mgPatternWeaving.nextCell = this;
                StartCoroutine(PulseOpacity());
            }

            public void OnPointerEnter()
            {
                if (isNextCell)
                {
                    image.color = selectedData.color;
                    isNextCell = false;
                    eventTrigger.enabled = false;
                    mgPatternWeaving.PerformAction(mgPatternWeaving.miniGameActionName.GoodWeaving);

                    if (selectedData.nextCell != null)
                    {
                        selectedData.nextCell.SetIsNextCell();
                    }
                    else
                    {
                        mgPatternWeaving.PatternFinished();
                    }
                }
                else if (Vector2Int.Distance(mgPatternWeaving.nextCell.cellPos, cellPos) > 1.5)
                {
                    mgPatternWeaving.PerformAction(mgPatternWeaving.miniGameActionName.BadWeaving);
                }
            }

            private IEnumerator PulseOpacity()
            {
                while (isNextCell)
                {
                    image.color = new Color(image.color.r, image.color.g, image.color.b, Mathf.Lerp(0.25f, 0.75f, Mathf.PingPong(Time.time, 1)));
                    yield return null;
                }
            }
        }
    }
}