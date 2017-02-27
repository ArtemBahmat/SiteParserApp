using SiteParser.DAL.Models;
using System.Linq;
using System.Windows.Controls;

namespace SiteParser.Core.BusinessLogic
{
    public class TreeViewBuilder : TreeBuilder
    {
        public TreeView Tree { private get; set; }
        private int _itemsCount;

        public override bool BuildTree()
        {
            bool finished = false;

            if (Urls?.Count > 0)
            {
                Url url = new Url()
                {
                    Name = BaseUrl,
                    State = State.IsAwaiting,
                    IsExternal = false,
                    NestingLevel = 0,
                    SiteId = SiteId
                };

                FillTree(false, url);
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

            foreach (Url url in Urls.Where(u => u.State != State.IsInTree && u.ParentName == parentUrl.Name))
            {
                _itemsCount++;
                TreeViewItem nestedItem = new TreeViewItem {Header = url.Name};
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
