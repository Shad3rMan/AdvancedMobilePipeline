using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ClickTest : MonoBehaviour
{
    private Button[] buttons;
    [SerializeField]
    private RectTransform sprite;

    private int lastRow;
    private int lastCol;

    private void Awake()
    {
        buttons = GetComponentsInChildren<Button>();

        foreach (var item in buttons)
        {
            item.onClick.AddListener(OnButtonClick);
        }

        lastRow = 0;
        lastCol = 0;
    }

    private void OnButtonClick()
    {
        string name = EventSystem.current.currentSelectedGameObject.name.Replace("Button (", "").Replace(")", "");
        int id = int.Parse(name);

        int col = id % 3;
        int row = id / 3;

        if (row > lastRow)
        {
            lastRow++;
        }
        else if (row < lastRow)
        {
            lastRow--;
        }
        else if (col > lastCol)
        {
            lastCol++;
        }
        else if (col < lastCol)
        {
            lastCol--;
        }

        MoveTo(lastCol, lastRow);
    }

    private void MoveTo(int col, int row)
    {
        int id = row * 3 + col;
        sprite.position = buttons[id].GetComponent<RectTransform>().position;
    }
}
