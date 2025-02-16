using Microsoft.Maui.Animations;
using System.ComponentModel;

namespace GameOfLife
{
    public class Cell
    {
        public bool Life { get; set; }
        public Cell(bool life)
        {
            Life = life;
        }
    }

    public partial class MainPage : ContentPage, INotifyPropertyChanged
    {
        const int Rows = 20;
        private const int Cols = 20;
        private bool _isRunning = false;
        Cell[,] cells = new Cell[Rows, Cols]; //основной массив
        private readonly BoxView[,] _cells = new BoxView[Rows, Cols];
        private readonly bool[,] _cellStates = new bool[Rows, Cols];
        private int _generationSpeed = 1;
        private List<Cell[,]> _historyOfCells = new();

        public MainPage()
        {
            InitializeComponent();
            CreateGrid();
            GenerationSpeed.Value = 1;
        }

        private void CreateGrid()
        {
            Field.RowDefinitions.Clear();
            Field.ColumnDefinitions.Clear();
            Field.Children.Clear();

            for (int i = 0; i < Rows; i++)
            {
                Field.RowDefinitions.Add(new RowDefinition { Height = GridLength.Star });
            }
            for (int j = 0; j < Cols; j++)
            {
                Field.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });
            }

            for (int i = 0; i < Rows; i++)
            {
                for (int j = 0; j < Cols; j++)
                {
                    var cell = new BoxView
                    {
                        BackgroundColor = Colors.Black,
                        Margin = 1
                    };

                    var row = i;
                    var col = j;

                    var tapGesture = new TapGestureRecognizer();
                    tapGesture.Tapped += (s, e) => ToggleCell(row, col);
                    cell.GestureRecognizers.Add(tapGesture);

                    _cells[i, j] = cell;

                    Field.Children.Add(cell);
                    Grid.SetRow(cell, i);
                    Grid.SetColumn(cell, j);
                }
            }
        }

        private void ToggleCell(int row, int col)
        {
            _cellStates[row, col] = !_cellStates[row, col];
            _cells[row, col].BackgroundColor = _cellStates[row, col] ? Colors.LightGreen : Colors.Black;
        }

        private void CreateCells()
        {
            for (int i = 0; i < Rows; i++)
            {
                for (int j = 0; j < Cols; j++)
                {
                    if (_cells[i, j].BackgroundColor == Colors.Black)
                        cells[i, j] = new Cell(false);
                    else
                        cells[i, j] = new Cell(true);
                }
            }
        }

        private bool CheckGeneration(Cell[,] new_cells)
        {
            for (int i = 0; i < Rows; i++)
            {
                for (int j = 0; j < Cols; j++)
                {
                    if (cells[i, j].Life != new_cells[i, j].Life)
                        return false;
                }
            }
            
            return true;
        }

        private void UpdateView()
        {
            for (int i = 0; i < Rows; i++)
            {
                for (int j = 0; j < Cols; j++)
                {
                    if (cells[i, j].Life)
                        _cells[i, j].BackgroundColor = Colors.LightGreen;
                    else
                        _cells[i, j].BackgroundColor = Colors.Black;
                }
            }
        }
        private Cell[,] CloneCells(Cell[,] new_cells)
        {
            for (int i = 0; i < Rows; i++)
            {
                for (int j = 0; j < Cols; j++)
                {
                    new_cells[i, j] = new Cell(cells[i, j].Life);
                }
            }
            return new_cells;
        }
        private async void StartClicked(object sender, EventArgs e)
        {
            if (!_isRunning)
            {
                _isRunning = true;
                CreateCells(); //теперь имеем массив с клетками f/t

                while (_isRunning)
                { 
                    Cell[,] new_cells = new Cell[Rows, Cols];
                    new_cells = CloneCells(new_cells);

                    int count_life_cells = 0;

                    for (int i = 0; i < Rows; i++)
                    {
                        for (int j = 0; j < Cols; j++)
                        {
                            bool[] LifeArray = new bool[8];
                            LifeArray[0] = i - 1 != -1 && cells[i - 1, j].Life;
                            LifeArray[1] = j - 1 != -1 && cells[i, j - 1].Life;
                            LifeArray[2] = i - 1 != -1 && j - 1 != -1 && cells[i - 1, j - 1].Life;
                            LifeArray[3] = i - 1 != -1 && j + 1 != 20 && cells[i - 1, j + 1].Life;
                            LifeArray[4] = i + 1 != 20 && j - 1 != -1 && cells[i + 1, j - 1].Life;
                            LifeArray[5] = i + 1 != 20 && cells[i + 1, j].Life;
                            LifeArray[6] = j + 1 != 20 && cells[i, j + 1].Life;
                            LifeArray[7] = i + 1 != 20 && j + 1 != 20 && cells[i + 1, j + 1].Life;
                            int aliveNeighbors = LifeArray.Count(x => x);

                            new_cells[i, j].Life = new_cells[i, j].Life ? (aliveNeighbors == 2 || aliveNeighbors == 3) : (aliveNeighbors == 3);
                            if (new_cells[i, j].Life)
                                count_life_cells++;
                        }
                    }
                    if (CheckGeneration(new_cells))
                    {
                        FinishGame();
                        break;
                    }

                    _historyOfCells.Add(cells);
                    if (CheckForСyclicity(_historyOfCells, new_cells))
                    {
                        FinishGame();
                        break;
                    }
                    
                    if (count_life_cells == 0)
                    {
                        FinishGame();
                        break;
                    }

                    cells = new_cells; //смена поколений
                    UpdateView();

                    await Task.Delay(5000 / _generationSpeed);
                }
            }
        }

        private void FinishGame()
        {
            DisplayAlert("Игра окончена", "", "Выйти");
            _isRunning = false;
        }

        private bool CheckForСyclicity(List<Cell[,]> _historyOfCells, Cell[,] new_cells)
        {
            foreach (var pastCells in _historyOfCells)
            {
                if (AreStatesEqual(pastCells, new_cells))
                    return true;
            }
            return false;
        }

        // Метод для проверки совпадения состояний (Life) всех клеток
        private bool AreStatesEqual(Cell[,] arr1, Cell[,] arr2)
        {
            int rows = arr1.GetLength(0);
            int cols = arr1.GetLength(1);

            if (rows != arr2.GetLength(0) || cols != arr2.GetLength(1))
                return false; // Разные размеры — точно не совпадают

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    if (arr1[i, j].Life != arr2[i, j].Life) // Сравниваем только Life
                        return false;
                }
            }
            return true;
        }

        private void GenerationSpeedChanged(object sender, ValueChangedEventArgs e)
        {   
            if (SpeedLabel != null)
            {
                _generationSpeed = (int)e.NewValue;
                SpeedLabel.Text = $"Текущая скорость развития поколений равна: {_generationSpeed}";
            }
        }

        private void PauseClicked(object sender, EventArgs e)
        {
            _isRunning = false;
        }

        private void RestartClicked(object sender, EventArgs e)
        {
            _isRunning = false;
            for (int i = 0; i < Rows; i++)
            {
                for (int j = 0; j < Cols; j++)
                {
                    _cellStates[i, j] = false;
                    _cells[i, j].BackgroundColor = Colors.Black;
                }
            }
        }

        private void InfoClicked(object sender, EventArgs e)
        {
            DisplayAlert(
                "Правила игры \"Жизнь\"",
                "🌍 Место действия игры\n" +
                "Игра разворачивается на размеченной клеточной плоскости, ограниченной размером экрана.\n\n" +

                "🔵 Состояние клеток\n" +
                "Каждая клетка может быть 🟢 живой или ⚫ мёртвой (пустой). У каждой клетки есть до 8 соседей.\n\n" +

                "🛠 Начальные условия\n" +
                "Распределение живых клеток в начале задаётся вручную и называется *первым поколением*.\n\n" +

                "📜 Правила жизни\n" +
                "• В пустой клетке, рядом с которой ровно 3 живые клетки, зарождается новая жизнь. 🌱\n" +
                "• Если у живой клетки 2 или 3 живые соседки, она продолжает жить. 💖\n" +
                "• В противном случае (если соседей < 2 или > 3), клетка умирает. 💀\n\n" +

                "🚧 Границы поля\n" +
                "Клетки, достигшие границы, подчиняются тем же правилам, что и остальные.\n\n" +

                "⏳ Условия завершения игры\n" +
                "Игра заканчивается, если:\n" +
                "✔ На поле не осталось живых клеток.\n" +
                "✔ Конфигурация перестала изменяться (стабильное состояние).\n" +
                "✔ Конфигурация повторяется через несколько шагов (цикличность). 🔄\n\n" +

                "⏩ Скорость смены поколений регулируется для удобства наблюдения.",
                "Ок"
            );
        }
    }
}
