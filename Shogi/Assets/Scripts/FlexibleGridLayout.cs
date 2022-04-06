using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

public class FlexibleGridLayout : LayoutGroup
{

    public enum FitType
    {
        Uniform,
        Width,
        Height,
        FixedRows,
        FixedColumns
    }

    public FitType fitType;
    public int Rows;
    public int Columns; 
    public Vector2 CellSize;
    public Vector2 Spacing;
    public bool FixedX;
    public bool FixedY;
    public override void CalculateLayoutInputHorizontal()
    {
        base.CalculateLayoutInputHorizontal();

        if (fitType == FitType.Uniform || fitType == FitType.Width || fitType == FitType.Height)
        {
            FixedX = false;
            FixedY = false;
            float sqRt = Mathf.Sqrt(transform.childCount);
            Rows = Mathf.CeilToInt(sqRt);
            Columns = Mathf.CeilToInt(sqRt);
        }

        switch (fitType)
        {
            case FitType.Width:
            case FitType.FixedColumns:
                Rows = Mathf.CeilToInt(transform.childCount / (float)Columns);
                break;
            case FitType.Height:
            case FitType.FixedRows:
                Columns = Mathf.CeilToInt(transform.childCount / (float)Rows);
                break;
            default:
                break;
        }

        float parentWidth = rectTransform.rect.width - (padding.left + padding.right);
        float parentHeight = rectTransform.rect.height - (padding.top + padding.bottom);

        float cellWidth = (parentWidth / (float)Columns) - (Spacing.x * (Columns - 1) / (float)Columns);
        float cellHeight = parentHeight / (float)Rows - (Spacing.y * (Rows - 1) / (float)Rows);

        CellSize.x = FixedX ? CellSize.x : cellWidth;
        CellSize.y = FixedY ? CellSize.y : cellHeight;

        int columnCount = 0;
        int rowCount = 0;

        for (int i = 0; i < rectChildren.Count; i++)
        {
            rowCount = i / Columns;
            columnCount = i % Columns;

            var item = rectChildren[i];

            var xPos = padding.left + (CellSize.x * columnCount) + (Spacing.x * columnCount);
            var yPos = padding.top + (CellSize.y * rowCount) + (Spacing.y * rowCount);

            SetChildAlongAxis(item, 0, xPos, CellSize.x);
            SetChildAlongAxis(item, 1, yPos, CellSize.y);
        }
    }

    public override void CalculateLayoutInputVertical()
    {
    }

    public override void SetLayoutHorizontal()
    {
    }

    public override void SetLayoutVertical()
    {
    }
}
