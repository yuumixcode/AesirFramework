using Sirenix.OdinInspector;

namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    [AesirExample]
    public class TableMatrixExampleSO : OdinAttributeExampleSO<TableMatrixExampleSO>
    {
        [Title("No Parameters")]
        [TableMatrix]
        public string[,] matrix = new string[2, 3];

        [Title("Parameter: Transpose")]
        [TableMatrix(Transpose = true)]
        public string[,] transposedMatrix = new string[2, 3];

        [Title("Parameter: IsReadOnly")]
        [TableMatrix(IsReadOnly = true)]
        public string[,] readOnlyMatrix = new string[2, 3];

        [Title("Parameter: ResizableColumns")]
        [TableMatrix(ResizableColumns = false)]
        public string[,] fixedColumnsMatrix = new string[2, 3];

        [Title("Parameter: HorizontalTitle")]
        [TableMatrix(HorizontalTitle = "Horizontal Title")]
        public string[,] horizontalTitleMatrix = new string[2, 3];

        [Title("Parameter: VerticalTitle")]
        [TableMatrix(VerticalTitle = "Vertical Title")]
        public string[,] verticalTitleMatrix = new string[2, 3];

        [Title("Parameter: RowHeight")]
        [TableMatrix(RowHeight = 40)]
        public string[,] rowHeightMatrix = new string[2, 3];

        [Title("Parameter: SquareCells")]
        [TableMatrix(SquareCells = true)]
        public string[,] squareCellsMatrix = new string[2, 3];

        [Title("Parameter: HideColumnIndices")]
        [TableMatrix(HideColumnIndices = true)]
        public string[,] hideColumnIndicesMatrix = new string[2, 3];

        [Title("Parameter: HideRowIndices")]
        [TableMatrix(HideRowIndices = true)]
        public string[,] hideRowIndicesMatrix = new string[2, 3];

        public override void AesirInspectorReset()
        {
            matrix = new string[2, 3];
            transposedMatrix = new string[2, 3];
            readOnlyMatrix = new string[2, 3];
            fixedColumnsMatrix = new string[2, 3];
            horizontalTitleMatrix = new string[2, 3];
            verticalTitleMatrix = new string[2, 3];
            rowHeightMatrix = new string[2, 3];
            squareCellsMatrix = new string[2, 3];
            hideColumnIndicesMatrix = new string[2, 3];
            hideRowIndicesMatrix = new string[2, 3];
        }
    }
}
