using SiteParser.Core.BusinessLogic;
using SiteParser.Core.Interfaces;
using SiteParser.DAL.Interfaces;
using SiteParser.DAL.Models;
using System.Collections.Generic;
using System.Windows;

namespace SiteParser
{
    public partial class TreeWindow : Window
    {
        public string BaseUrl { get; set; }
        public int NestingLevel { get; set; }
        public bool GetExternal { get; set; }
        public List<Url> Urls { get; set; }

        public TreeWindow() 
        {
            InitializeComponent();
        }

        private void SiteTreeWindow_Loaded(object sender, RoutedEventArgs e)
        {
            ITreeBuilder treeBuilder = new TreeViewBuilder()
            {
                BaseUrl = BaseUrl,
                NestingLevel = NestingLevel,
                GetExternal = GetExternal,
                Tree = treeView,
                Urls = Urls
            };

            treeBuilder.BuildTree();
        }
    }
}
