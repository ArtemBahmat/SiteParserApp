using SiteParserCore.Models;
using System.Linq;
using System.Windows.Controls;

namespace SiteParserCore.BusinessLogic
{
    public class TreeViewBuilder : TreeBuilderBase
    {
        private TreeView Tree { get; }
        private int _itemsCount;

        public TreeViewBuilder(string baseUrl, int nestingLevel, bool getExternal, TreeView treeView) : base(baseUrl, nestingLevel, getExternal)
        {
            Tree = treeView;
        }

        public override bool BuildTree()
        {
            bool finished = false;

            Initialize();

            if (Urls?.Count > 0)
            {
                FillTree(false, Url);
                finished = true;
            }

            return finished;
        }

        private void FillTree(bool isRecursion, Url parentUrl, TreeViewItem item = null)
        {
            TreeViewItem baseItem = null;

            if (item == null)
            {
                baseItem = new TreeViewItem {IsExpanded = true};
                item = new TreeViewItem {Header = parentUrl.Name};
                baseItem.Items.Add(item);
                _itemsCount++;
            }

            item.Header = parentUrl.Name;
            item.IsExpanded = true;

            foreach (var url in Urls.Where(u => u.State != State.IsInTree && u.ParentName == parentUrl.Name))
            {
                _itemsCount++;
                var nestedItem = new TreeViewItem {Header = url.Name};
                item.Items.Add(nestedItem);
                FillTree(true, url, nestedItem);
            }

            if (isRecursion) return;
            if (baseItem == null) return;
            baseItem.Header = $"{_itemsCount} urls";
            Tree.Items.Add(baseItem);
        }
    }
}
