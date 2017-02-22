﻿using SiteParserCore.Models;
using System.Collections.Generic;
using System.Linq;

namespace SiteParserCore.Helpers
{
    public class UrlValidator
    {
        static HashSet<string> _fileTypes = new HashSet<string> { ".exe", ".djvu", ".pdf", ".zip", ".rar", ".tar", ".arj", ".arc", ".lzh", ".gz", ".z", ".tar", ".doc", ".docx", ".xls", ".xlsx", ".ppt", ".pptx", ".js", ".css", ".cs", ".config", ".ico", ".jpg", ".jpe", ".gif", ".xbm", ".jpeg", ".bmp", ".png", ".mp3", ".wav", ".mid", ".au", ".aif", ".ra", ".ram", ".wma", ".avi", ".mov", ".mpg", ".mpe", ".mpeg", ".asf", ".mp4", ".asx", ".wmv", ".swf" };
        private string _baseUrl;
        private string _domain;
        private bool _parseExternalLinks;

        public UrlValidator(string baseUrl, bool parseExternalLinks)
        {
            if (!string.IsNullOrWhiteSpace(baseUrl))
            {
                _baseUrl = baseUrl;
                _parseExternalLinks = parseExternalLinks;
                _domain = UrlHelper.GetDomainFromUrl(_baseUrl);
            }
        }

        public bool IsValid(Url url)   
        {
            bool result = false;

            if (!(string.IsNullOrWhiteSpace(url.Name) || url.Name.StartsWith("#") || url.Name.Contains("mailto:")))
            {
                url.Name = UrlHelper.GetAbsoluteUrl(url.Name, url.ParentName ?? _baseUrl);
                url.IsExternal = !url.Name.Contains(_domain);
                string urlNameCopy = url.Name;
                result = url.Name.IsASCII() && (_parseExternalLinks || !url.IsExternal) && !_fileTypes.Any(x => urlNameCopy.EndsWith(x));
            }

            return result;
        }
    }
}
