using SiteParserCore.BusinessLogic;
using SiteParserCore.Interfaces;
using System.Windows;

namespace SiteGrabber
{
    public partial class TreeWindow : Window
    {
        private string _baseUrl;
        private int _nestingLevel;
        private bool _getExternal;
        private ITreeBuilder _treeManager; 

        public TreeWindow(string baseUrl, int nestingLevel, bool getExternal)
        {
            _baseUrl = baseUrl;
            _nestingLevel = nestingLevel;
            _getExternal = getExternal;
            InitializeComponent();
        }

        private void SiteTreeWindow_Loaded(object sender, RoutedEventArgs e)
        {
            _treeManager = new TreeViewBuilder(_baseUrl, _nestingLevel, _getExternal, treeView);
            _treeManager.BuildTree();
        }
    }
}
