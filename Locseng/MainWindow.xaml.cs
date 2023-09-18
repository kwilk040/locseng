using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Ookii.Dialogs.Wpf;

namespace Locseng
{
    public partial class MainWindow
    {
        private readonly Indexer.Indexer _indexer;

        public MainWindow()
        {
            InitializeComponent();
            _indexer = Indexer.Indexer.Initialize();
        }

        private void QueryInput_OnKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Return) return;
            Result.Items.Clear();
            var query = QueryInput.Text;
            var keyValuePairs = _indexer.QueryIndex(query);

            foreach (var keyValuePair in keyValuePairs)
            {
                Result.Items.Add(keyValuePair.Key);
            }
        }

        private void Result_OnKeyUp(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Return:
                {
                    OpenFileInDefaultProgram();
                    break;
                }
                case Key.Q:
                    Result.Items.Clear();
                    break;
            }
        }

        private void Result_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            OpenFileInDefaultProgram();
        }

        private void OpenFileInDefaultProgram()
        {
            using var explorer = new Process();
            var path = Result.SelectedItem as string;
            explorer.StartInfo.FileName = "explorer";
            explorer.StartInfo.Arguments = "\"" + path + "\"";
            explorer.Start();
        }

        private void AddDirectoryButton_OnClick(object sender, RoutedEventArgs e)
        {
            var path = GetDirectoryPath();
            if (path is null) return;
            Task.Run(() => _indexer.AddDirectory(path));
        }

        private void RemoveDirectoryButton_OnClick(object sender, RoutedEventArgs e)
        {
            var path = GetDirectoryPath();
            if (path is null) return;
            Task.Run(() => _indexer.RemoveDirectory(path));
        }

        private static string? GetDirectoryPath()
        {
            var dialog = new VistaFolderBrowserDialog();
            var showDialog = dialog.ShowDialog();
            if (showDialog is not true) return null;
            return dialog.SelectedPath ?? null;
        }

        private void ListDirectoriesButton_OnClick(object sender, RoutedEventArgs e)
        {
            Result.Items.Clear();
            var directories = _indexer.GetDirectories();
            foreach (var directory in directories)
            {
                Result.Items.Add(directory);
            }
        }
    }
}