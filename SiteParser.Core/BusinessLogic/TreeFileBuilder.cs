using SiteParser.Core.Helpers;
using SiteParser.Core.Interfaces;
using SiteParser.DAL.Models;
using SiteParser.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace SiteParser.Core.BusinessLogic
{
    public class TreeFileBuilder : TreeBuilder, ITreeBuilder
    {
        private readonly StringBuilder _strBuilder = new StringBuilder();

        public override bool BuildTree()
        {
            bool finished = false;
            bool saved = false;

            Url url = new Url()
            {
                Name = BaseUrl,
                State = State.IsAwaiting,
                IsExternal = false,
                NestingLevel = 0,
                SiteId = SiteId
            };

            if (Urls?.Count > 0)
            {
                FillTree(false, url);
                saved = SaveStringTree();
                finished = true;
            }

            return finished && saved;
        }

        private bool SaveStringTree()
        {
            bool success = false;

            try
            {
                string path = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + "\\" + UrlHelper.GetDomainFromUrl(BaseUrl) + ".txt";

                if (File.Exists(path))
                {
                    File.Delete(path);
                }

                using (StreamWriter file = new StreamWriter($"{path}"))
                {
                    file.WriteLine(_strBuilder.ToString());
                }

                success = true;
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
            }

            return success;
        }

        private void FillTree(bool isRecursion, Url parentUrl)
        {
            if (!isRecursion)
            {
                ResetUrlState();
            }

            parentUrl.State = State.IsInTree;
            _strBuilder.Append($"{GetDashes(parentUrl.NestingLevel)} {parentUrl.Name}");
            _strBuilder.AppendLine();

            List<Url> list = Urls.Where(u => u.State != State.IsInTree && u.ParentName == parentUrl.Name).ToList();

            if (list.Count == 0) return;
            foreach (Url url in list)
            {
                FillTree(true, url);
            }
        }

        private string GetDashes(int nestingLevel)
        {
            string result = string.Empty;

            for (int i = 0; i < nestingLevel; i++)
            {
                result += "-";
            }

            return result;
        }

        private void ResetUrlState()
        {
            foreach (Url url in Urls.Where(url => url.State != State.IsAwaiting))
            {
                url.State = State.IsAwaiting;
            }
        }
    }
}
