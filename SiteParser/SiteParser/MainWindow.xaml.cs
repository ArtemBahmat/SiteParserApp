using SiteGrabber;
using SiteParserCore.BusinessLogic;
using SiteParserCore.Helpers;
using SiteParserCore.Interfaces;
using SiteParserCore.Models;
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

        IParserManager _parserManagerInstance;
        IRepository _dBRepository = new Repository();
        DateTime _startDateTime;
        System.Windows.Threading.DispatcherTimer timer;

        public MainWindow()
        {
            InitializeComponent();
            AddSitesToComboBox();
        }

        private void AddSitesToComboBox()
        {
            urlComboBox.Items.Clear();

            List<Site> sites = _dBRepository.GetSites();

            foreach (var site in sites)
            {
                urlComboBox.Items.Add(site.Name);
            }

            urlComboBox.SelectedIndex = 0;
        }

        private void StartParse()
        {
            _parserManagerInstance = App.container.GetInstance<IParserManager>(); 
            _parserManagerInstance.Execute(_siteUrl, _maxThreadsCount, _nestingLevel, _parseAll);
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
                _parseAll = (bool)checkBox_external_parse.IsChecked;
                Thread AdditionalThread = new Thread(StartParse);
                AdditionalThread.Start();
                ShowWaitingMessage();
            }
            else
                MessageBox.Show("Incorrect url");
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            _parserManagerInstance.ParserInstance.IsAlive = false;
        }

        private void ShowWaitingMessage()
        {
            timer = new System.Windows.Threading.DispatcherTimer();

            timer.Tick += new EventHandler(timerTick);
            timer.Interval = new TimeSpan(0, 0, 0, 0, 100);
            timer.Start();

            MainGrid.Visibility = Visibility.Collapsed;
            WaitingGrid.Visibility = Visibility.Visible;
        }

        private void timerTick(object sender, EventArgs e)
        {
            if (_parserManagerInstance?.ParserInstance != null)
            {
                if (_parserManagerInstance.ParserInstance.IsAlive)
                {
                    int workerThreadsAvailable, completionThreadsAvailable;
                    int workerThreadsMax, completionThreadsMax;

                    ThreadPool.GetAvailableThreads(out workerThreadsAvailable, out completionThreadsAvailable);
                    ThreadPool.GetMaxThreads(out workerThreadsMax, out completionThreadsMax);

                    TimeSpan timeElapsed = DateTime.Now - _startDateTime;
                    timeElapsed = new TimeSpan(timeElapsed.Days, timeElapsed.Hours, timeElapsed.Minutes, timeElapsed.Seconds);
                    time_elapsed_msg.Content = $"Time elapsed: {timeElapsed}";
                    total_urls_count_msg.Content = $"Total urls found: {_parserManagerInstance.ParserInstance.TotalUrlsCount}";
                    parsed_urls_count_msg.Content = $"Parsed urls: {_parserManagerInstance.ParserInstance.ParsedUrlsCount}";
                    threads_count_msg.Content = $"Number of active parsing threads: {workerThreadsMax - workerThreadsAvailable}";
                }
                else
                {
                    int pagesCount = _parserManagerInstance.ParserInstance.ParsedUrlsCount;
                    int totalUrlsCount = _parserManagerInstance.ParserInstance.TotalUrlsCount;
                    Stop();
                    string text = $"Parsing finished at level: {myUpDown_Nesting_Level.Text} \n Total urls parsed: {totalUrlsCount} \n Time elapsed: {DateTime.Now - _startDateTime}";
                    string caption = $"Parsing summary info";
                    MessageBox.Show(text, caption);
                    TreeWindow treeWindow = new TreeWindow(urlComboBox.Text, _parserManagerInstance.ParserInstance.MaxNestingLevel, _parseAll);
                    treeWindow.Show();
                    AddSitesToComboBox();
                }
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
            timer.Stop();
            _parserManagerInstance.IsAlive = false;
        }

        private void BuildViewTree_Click(object sender, RoutedEventArgs e)
        {
            TreeWindow treeWindow = new TreeWindow(urlComboBox.Text, int.Parse(myUpDown_Nesting_Level.Text), (bool)checkBox_external_parse.IsChecked);
            treeWindow.Show();
        }

        private void BuildFileTree_Click(object sender, RoutedEventArgs e)
        {
            ITreeBuilder treeManager = new TreeFileBuilder(urlComboBox.Text, int.Parse(myUpDown_Nesting_Level.Text), (bool)checkBox_external_parse.IsChecked);
            bool success = treeManager.BuildTree();
            string text = success ? $"Tree was created successfully in file" : "Error while creating tree";
            MessageBox.Show(text, "finished");
        }
    }
}
