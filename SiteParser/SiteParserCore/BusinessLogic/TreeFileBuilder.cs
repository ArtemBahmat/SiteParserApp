using SiteParserCore.Helpers;
using SiteParserCore.Interfaces;
using SiteParserCore.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace SiteParserCore.BusinessLogic
{
    public class TreeFileBuilder : TreeBuilderBase, ITreeBuilder
    {
        private readonly StringBuilder _strBuilder = new StringBuilder();

        public TreeFileBuilder(string baseUrl, int nestingLevel, bool getExternal) : base(baseUrl, nestingLevel, getExternal) { }

        public override bool BuildTree()
        {
            bool finished = false;
            bool saved = false;

            Initialize();

            if (Urls?.Count > 0)
            {
                FillTree(false, Url);
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
                string path = System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + "\\" + UrlHelper.GetDomainFromUrl(BaseUrl) + ".txt";

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
            IEnumerable<Url> list;

            if (!isRecursion)
            {
                ResetUrlState();
            }

            parentUrl.State = State.IsInTree;
            _strBuilder.Append($"{GetDashes(parentUrl.NestingLevel)} {parentUrl.Name}");
            _strBuilder.AppendLine();

            list = Urls.Where(u => u.State != State.IsInTree && u.ParentName == parentUrl.Name);

            if (list.Any())
            {
                foreach (var url in list)
                {
                    FillTree(true, url);
                }
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
            foreach (var url in Urls.Where(url => url.State != State.IsAwaiting))
            {
                url.State = State.IsAwaiting;
            }
        }
    }
}
