using SiteParser.Core.BusinessLogic;
using SiteParser.Core.Helpers;
using SiteParser.Core.Interfaces;
using SiteParser.DAL.Interfaces;
using SiteParser.DAL.Models;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows;

namespace SiteParser
{
    public partial class MainWindow : Window
    {
        private string _siteUrl;
        private int _maxThreadsCount;
        private int _nestingLevel;
        private bool _parseAll;

        private IParserManager _parserManager;
        private readonly IRepository<Site> _sitesRepository = App.Container.GetInstance<IRepository<Site>>();
        private readonly IRepository<Url> _urlsRepository = App.Container.GetInstance<IRepository<Url>>();
        private DateTime _startDateTime;
        private System.Windows.Threading.DispatcherTimer _timer;

        public MainWindow()
        {
            InitializeComponent();
            AddSitesToComboBox();
        }

        private void AddSitesToComboBox()
        {
            urlComboBox.Items.Clear();

            List<Site> sites = _sitesRepository.GetRange(null);

            foreach (Site site in sites)
            {
                urlComboBox.Items.Add(site.Name);
            }

            urlComboBox.SelectedIndex = 0;
        }

        private void StartParse()
        {
            _parserManager = App.Container.GetInstance<IParserManager>();
            _parserManager.Execute(_siteUrl, _maxThreadsCount, _nestingLevel, _parseAll);
        }

        private void SubmitButton_Click(object sender, RoutedEventArgs e)
        {
            string url = UrlHelper.GetCorrectUrl(urlComboBox.Text);

            if (!string.IsNullOrEmpty(url))
            {
                urlComboBox.Text = url;
                _startDateTime = DateTime.Now;
                _siteUrl = urlComboBox.Text;
                _maxThreadsCount = int.Parse(myUpDownNumberOfThreads.Text);
                _nestingLevel = int.Parse(myUpDown_Nesting_Level.Text);
                if (checkBox_external_parse.IsChecked != null) _parseAll = (bool)checkBox_external_parse.IsChecked;
                Thread additionalThread = new Thread(StartParse);
                additionalThread.Start();
                ShowWaitingMessage();
            }
            else
                MessageBox.Show("Incorrect url");
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            _parserManager.IsAlive = false;
        }

        private void ShowWaitingMessage()
        {
            _timer = new System.Windows.Threading.DispatcherTimer();
            _timer.Tick += TimerTick;
            _timer.Interval = new TimeSpan(0, 0, 0, 0, 300);
            _timer.Start();

            MainGrid.Visibility = Visibility.Collapsed;
            WaitingGrid.Visibility = Visibility.Visible;
        }

        private void TimerTick(object sender, EventArgs e)
        {
            if (_parserManager?.ParserInstance == null) return;
            if (_parserManager.IsAlive)
            {
                int workerThreadsAvailable, completionThreadsAvailable;
                int workerThreadsMax, completionThreadsMax;

                ThreadPool.GetAvailableThreads(out workerThreadsAvailable, out completionThreadsAvailable);
                ThreadPool.GetMaxThreads(out workerThreadsMax, out completionThreadsMax);

                TimeSpan timeElapsed = DateTime.Now - _startDateTime;
                timeElapsed = new TimeSpan(timeElapsed.Days, timeElapsed.Hours, timeElapsed.Minutes, timeElapsed.Seconds);
                time_elapsed_msg.Content = $"Time elapsed: {timeElapsed}";
                total_urls_count_msg.Content = $"Total urls found: {_parserManager.ParserInstance.TotalUrlsCount}";
                parsed_urls_count_msg.Content = $"Parsed urls: {_parserManager.ParserInstance.ParsedUrlsCount}";
                threads_count_msg.Content = $"Number of active parsing threads: {workerThreadsMax - workerThreadsAvailable}";
            }
            else
            {
                int totalUrlsCount = _parserManager.ParserInstance.TotalUrlsCount;
                Stop();
                string text = $"Parsing finished at level: {myUpDown_Nesting_Level.Text} {Environment.NewLine} Total urls parsed: {totalUrlsCount} {Environment.NewLine} Time elapsed: {DateTime.Now - _startDateTime}";
                string caption = "Parsing summary info";
                MessageBox.Show(text, caption);
                AddSitesToComboBox();
                BuildViewTree();
            }
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            Stop();
        }

        private void Stop()
        {
            MainGrid.Visibility = Visibility.Visible;
            WaitingGrid.Visibility = Visibility.Collapsed;
            _timer.Stop();
            _parserManager.IsAlive = false;
        }

        private void BuildViewTree_Click(object sender, RoutedEventArgs e)
        {
            BuildViewTree();
        }

        private void BuildViewTree()
        {
            Site site = _sitesRepository.Get(urlComboBox.Text);
            if (site == null) return;

            int nestingLevel = int.Parse(myUpDown_Nesting_Level.Text);
            bool getExternal = checkBox_external_parse.IsChecked != null && (bool)checkBox_external_parse.IsChecked;

            List<Url> urls = GetUrlsForTree(site.Id, getExternal, nestingLevel);
            TreeWindow treeWindow = new TreeWindow()
            {
                BaseUrl = urlComboBox.Text,
                NestingLevel = nestingLevel,
                GetExternal = getExternal,
                Urls = urls
            };
            treeWindow.Show();
        }

        private void BuildFileTree_Click(object sender, RoutedEventArgs e)
        {
            string baseUrl = urlComboBox.Text;
            if (string.IsNullOrEmpty(baseUrl)) return;   
            Site site = _sitesRepository.Get(baseUrl);
            if (site == null) return;            
            bool getExternal = checkBox_external_parse.IsChecked != null && (bool) checkBox_external_parse.IsChecked;
            int nestingLevel = int.Parse(myUpDown_Nesting_Level.Text);
            List<Url> urls = GetUrlsForTree(site.Id, getExternal, nestingLevel);

            ITreeBuilder treeBuilder = new TreeFileBuilder()
            {
                BaseUrl = baseUrl,
                GetExternal = getExternal,
                NestingLevel = nestingLevel,
                SiteId = site.Id,
                Urls = urls
            };

            bool success = treeBuilder.BuildTree();
            string text = success ? "Tree was created successfully in file" : "Error while creating tree";
            MessageBox.Show(text, "finished");
        }

        private List<Url> GetUrlsForTree(int siteId, bool getExternal, int nestingLevel)
        {
            Func<Url, bool> externalCondition = url => !url.IsExternal;
            Func<Url, bool> nestingCondition = url => url.SiteId == siteId && url.NestingLevel <= nestingLevel;
            Func<Url, bool> resultCondition = getExternal ? nestingCondition : url => nestingCondition(url) && externalCondition(url);

           return _urlsRepository.GetRange(resultCondition);
        }
    }
}
