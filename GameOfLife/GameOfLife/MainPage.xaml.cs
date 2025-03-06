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
        const int Rows = 50;
        private const int Columns = 50;
        private bool _isRunning = false;
        public static Cell[,] Cells { get; set; } = new Cell[Rows, Columns]; //основной массив
        private readonly BoxView[,] _cells = new BoxView[Rows, Columns];
        private readonly bool[,] _cellStates = new bool[Rows, Columns];

        private int _generationSpeed = 1;
        public int GenerationSpeed
        {
            get => _generationSpeed;
            set
            {
                if (_generationSpeed != value)
                {
                    _generationSpeed = value;
                    OnPropertyChanged();
                }
            }
        }

        private int _minCells = 2;
        public int MinCells
        {
            get => _minCells;
            set
            {
                if (_minCells != value)
                {
                    _minCells = value;
                    OnPropertyChanged();
                }
            }
        }

        private int _maxCells = 3;
        public int MaxCells
        {
            get => _maxCells;
            set
            {
                if (_maxCells != value)
                {
                    _maxCells = value;
                    OnPropertyChanged();
                }
            }
        }

        private int _revivalCells = 3;
        public int RevivalCells
        {
            get => _revivalCells;
            set
            {
                if (_revivalCells != value)
                {
                    _revivalCells = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _noDelayMode;
        public bool NoDelayMode
        {
            get => _noDelayMode;
            set
            {
                if (_noDelayMode != value)
                {
                    _noDelayMode = value;
                    OnPropertyChanged();
                }
            }
        }

        private float _scale = 2.5f;
        new public float Scale
        {
            get => _scale;
            set
            {
                if (_scale != value)
                {
                    _scale = value;
                    FieldScaling();
                    OnPropertyChanged();
                }
            }
        }

        // Инициализация страницы
        public MainPage()
        {
            InitializeComponent();
            CreateGrid();
            BindingContext = this;
        }

        // Изменение масштаба
        public void FieldScaling()
        {
            for (int i = 0; i < Rows; i++)
            {
                Field.RowDefinitions[i].Height = 500 / Rows * Scale;
            }
            for (int j = 0; j < Columns; j++)
            {
                Field.ColumnDefinitions[j].Width = 500 / Columns * Scale;
            }
        }

        // Создание поля
        private void CreateGrid()
        {
            for (int i = 0; i < Rows; i++)
            {
                Field.RowDefinitions.Add(new RowDefinition { Height = 500/Rows*Scale });
            }
            for (int j = 0; j < Columns; j++)
            {
                Field.ColumnDefinitions.Add(new ColumnDefinition { Width = 500 / Columns * Scale });
            }

            for (int i = 0; i < Rows; i++)
            {
                for (int j = 0; j < Columns; j++)
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

        // Обработка нажатия на клеточку
        private void ToggleCell(int row, int col)
        {
            _cellStates[row, col] = !_cellStates[row, col];
            _cells[row, col].BackgroundColor = _cellStates[row, col] ? Colors.LightGreen : Colors.Black;
        }

        // Создание массива клеточек
        private void CreateCells()
        {
            for (int i = 0; i < Rows; i++)
            {
                for (int j = 0; j < Columns; j++)
                {
                    if (_cells[i, j].BackgroundColor == Colors.Black)
                        Cells[i, j] = new Cell(false);
                    else
                        Cells[i, j] = new Cell(true);
                }
            }
        }

        // Финиш если статичная картинка
        private bool CheckGeneration(Cell[,] new_cells)
        {
            for (int i = 0; i < Rows; i++)
            {
                for (int j = 0; j < Columns; j++)
                {
                    if (Cells[i, j].Life != new_cells[i, j].Life)
                        return false;
                }
            }
            
            return true;
        }

        // Обновление состояния игры
        private void UpdateView()
        {
            for (int i = 0; i < Rows; i++)
            {
                for (int j = 0; j < Columns; j++)
                {
                    if (Cells[i, j].Life)
                        _cells[i, j].BackgroundColor = Colors.LightGreen;
                    else
                        _cells[i, j].BackgroundColor = Colors.Black;
                }
            }
        }

        // Клонирование 
        private Cell[,] CloneCells(Cell[,] new_cells)
        {
            for (int i = 0; i < Rows; i++)
            {
                for (int j = 0; j < Columns; j++)
                {
                    new_cells[i, j] = new Cell(Cells[i, j].Life);
                }
            }
            return new_cells;
        }

        // Запуск игры
        private async void StartClicked(object sender, EventArgs e)
        {
            if (!_isRunning)
            {
                _isRunning = true;
                CreateCells();
                while (_isRunning)
                {
                    Cell[,] new_cells = new Cell[Rows, Columns];
                    new_cells = CloneCells(new_cells);

                    int count_life_cells = 0;

                    for (int i = 0; i < Rows; i++)
                    {
                        for (int j = 0; j < Columns; j++)
                        {
                            bool[] LifeArray = new bool[8];
                            LifeArray[0] = i - 1 != -1 && Cells[i - 1, j].Life;
                            LifeArray[1] = j - 1 != -1 && Cells[i, j - 1].Life;
                            LifeArray[2] = i - 1 != -1 && j - 1 != -1 && Cells[i - 1, j - 1].Life;
                            LifeArray[3] = i - 1 != -1 && j + 1 != Columns && Cells[i - 1, j + 1].Life;
                            LifeArray[4] = i + 1 != Rows && j - 1 != -1 && Cells[i + 1, j - 1].Life;
                            LifeArray[5] = i + 1 != Rows && Cells[i + 1, j].Life;
                            LifeArray[6] = j + 1 != Columns && Cells[i, j + 1].Life;
                            LifeArray[7] = i + 1 != Rows && j + 1 != Columns && Cells[i + 1, j + 1].Life;
                            int aliveNeighbors = LifeArray.Count(x => x);

                            new_cells[i, j].Life = new_cells[i, j].Life ? (aliveNeighbors >= MinCells && aliveNeighbors <= MaxCells) : (aliveNeighbors == RevivalCells);
                            if (new_cells[i, j].Life)
                                count_life_cells++;
                        }
                    }
                    if (CheckGeneration(new_cells))
                    {
                        FinishGame();
                        break;
                    }

                    if (count_life_cells == 0)
                    {
                        FinishGame();
                        break;
                    }

                    Cells = new_cells;
                    UpdateView();

                    if (!NoDelayMode)
                        await Task.Delay(5000 / GenerationSpeed);
                    else
                    {
                        await Task.Delay(50);
                    }
                }
            }
        }

        public void GenerationSpeedModeWasToggled(object sender, ToggledEventArgs e)
        {
            if (e.Value == true)
                NoDelayMode = true;
            else
                NoDelayMode = false;
        }

        private void FinishGame()
        {
            DisplayAlert("Игра окончена", "", "Выйти");
            _isRunning = false;
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
                for (int j = 0; j < Columns; j++)
                {
                    _cellStates[i, j] = false;
                    _cells[i, j].BackgroundColor = Colors.Black;
                }
            }
        }

        public async void SaveClicked(object sender, EventArgs e)
        {
            SaveInfo.SaveModel();
            await DisplayAlert("Уведомление", "Игра была успешно сохранена!", "Ok");
        }

        public void LoadClicked(object sender, EventArgs e)
        {
            SaveInfo.LoadModel();
            UpdateView();
        }

        public void InfoClicked(object sender, EventArgs e)
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


