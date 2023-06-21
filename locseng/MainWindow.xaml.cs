using System.Diagnostics;
using System.Windows.Input;
using indexer;

namespace locseng
{
    public partial class MainWindow
    {
        private readonly Indexer _indexer;

        public MainWindow()
        {
            InitializeComponent();
            _indexer = new Indexer();
            _indexer.AddDirectory(@"C:\Users\Yves\Desktop\personal-wiki-master\src");
        }

        private void QueryInput_OnKeyDown(object sender, KeyEventArgs e)
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
                    QueryInput.Focus();
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
    }
} 
