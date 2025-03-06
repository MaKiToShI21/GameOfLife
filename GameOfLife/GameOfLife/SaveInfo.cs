using Newtonsoft.Json;
using System.Collections.ObjectModel;
using System.Xml;


namespace GameOfLife
{
    public class SavedData
    {
        public Cell[,] CellSave { get; set; }

    }

    public class SaveInfo
    {
        private static readonly string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "DataOfGameOfLife.json");
        public static void SaveModel()
        {

            try
            {
                var dataToSave = new SavedData
                {
                    CellSave = MainPage.Cells
                };


                string jsonData = JsonConvert.SerializeObject(dataToSave, Newtonsoft.Json.Formatting.Indented);
                File.WriteAllText(filePath, jsonData);

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка сохранения: {ex.Message}");
            }
        }

        public static void LoadModel()
        {

            if (File.Exists(filePath))
            {
                string jsonData = File.ReadAllText(filePath);
                var loadedData = JsonConvert.DeserializeObject<SavedData>(jsonData);

                if (loadedData != null)
                {
                    MainPage.Cells = loadedData.CellSave;
                }

            }

        }

    }
}