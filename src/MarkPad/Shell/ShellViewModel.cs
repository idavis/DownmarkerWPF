﻿using System;
using Caliburn.Micro;
using MarkPad.About;
using MarkPad.Document;
using MarkPad.Framework.Events;
using MarkPad.MDI;
using MarkPad.Services.Interfaces;
using MarkPad.Settings;

namespace MarkPad.Shell
{
    internal class ShellViewModel : Conductor<IScreen>, IHandle<FileOpenEvent>
    {
        private readonly IEventAggregator eventAggregator;
        private readonly IDialogService dialogService;
        private readonly IWindowManager windowService;
        private readonly Func<DocumentViewModel> documentCreator;
        private readonly Func<SettingsViewModel> settingsCreator;
        private readonly Func<AboutViewModel> aboutCreator;

        public ShellViewModel(
            IDialogService dialogService,
            IWindowManager windowService,
            IEventAggregator eventAggregator,
            MDIViewModel mdi,
            Func<DocumentViewModel> documentCreator,
            Func<SettingsViewModel> settingsCreator,
            Func<AboutViewModel> aboutCreator)
        {
            this.eventAggregator = eventAggregator;
            this.dialogService = dialogService;
            this.windowService = windowService;
            MDI = mdi;
            this.documentCreator = documentCreator;
            this.settingsCreator = settingsCreator;
            this.aboutCreator = aboutCreator;

            ActivateItem(mdi);
        }

        public override string DisplayName
        {
            get { return "MarkPad"; }
            set { }
        }

        public MDIViewModel MDI { get; private set; }

        public void NewDocument()
        {
            MDI.Open(documentCreator());
        }

        public void NewJekyllDocument()
        {
            var creator = documentCreator();
            creator.Document.BeginUpdate();
            creator.Document.Text = CreateJekyllHeader();
            creator.Document.EndUpdate();
            MDI.Open(creator);
        }

        private static string CreateJekyllHeader()
        {
            const string permalink = "new-page.html";
            const string title = "New Post";
            const string description = "Some Description";
            var date = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss zzz");
            return string.Format("---\r\nlayout: post\r\ntitle: {0}\r\npermalink: {1}\r\ndescription: {2}\r\ndate: {3}\r\ntags: \"some tags here\"\r\n---\r\n\r\n", title, permalink, description, date);
        }

        public void OpenDocument()
        {
            var path = dialogService.GetFileOpenPath("Open a markdown document.", Constants.ExtensionFilter + "|Any File (*.*)|*.*");
            if (path == null)
                return;

            foreach (var p in path)
                eventAggregator.Publish(new FileOpenEvent(p));
        }

        public void OpenDocument(string path)
        {
            var doc = documentCreator();
            doc.Open(path);
            MDI.Open(doc);
        }

        public void SaveDocument()
        {
            var doc = MDI.ActiveItem as DocumentViewModel;
            if (doc != null)
            {
                doc.Save();
            }
        }

        public void SaveAllDocuments()
        {
            foreach (DocumentViewModel doc in MDI.Items)
            {
                doc.Save();
            }
        }

        public void Handle(FileOpenEvent message)
        {
            OpenDocument(message.Path);
        }

        public void ShowSettings()
        {
            windowService.ShowDialog(settingsCreator());
        }

        public void ShowAbout()
        {
            windowService.ShowDialog(aboutCreator());
        }

        public void ToggleWebView()
        {
            var doc = MDI.ActiveItem as DocumentViewModel;
            if (doc != null)
            {
                doc.DistractionFree = !doc.DistractionFree;
            }
        }

        public void PrintDocument()
        {
            var doc = MDI.ActiveItem as DocumentViewModel;
            if (doc != null)
            {
                doc.Print();
            }
        }
    }
}
