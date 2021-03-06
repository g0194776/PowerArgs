﻿using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using PowerArgs;
using PowerArgs.Cli;
using System;
using System.Linq;

namespace HelloWorld.Samples
{
    public class TablesPage : GridPage
    {
        CloudStorageAccount currentStorageAccount;
        private Button deleteButton;
        private Button addButton;
        public TablesPage()
        {
            Grid.VisibleColumns.Add(new ColumnViewModel(nameof(CloudTable.Name).ToConsoleString(Theme.DefaultTheme.H1Color)));
            Grid.NoDataMessage = "No tables";
            Grid.NoVisibleColumnsMessage = "Loading...";
            addButton = CommandBar.Add(new Button() { Text = "Add table" });
            addButton.Activated.SubscribeForLifetime(AddTable, LifetimeManager);

            deleteButton = CommandBar.Add(new Button() { Text = "Delete table", Shortcut = new KeyboardShortcut(ConsoleKey.Delete, null), CanFocus = false });
            deleteButton.Activated.SubscribeForLifetime(DeleteSelectedTable, LifetimeManager);

            
            Grid.SelectedItemActivated += NavigateToTable;

            CommandBar.Add(new NotificationButton(ProgressOperationManager));
            Grid.SubscribeForLifetime(nameof(Grid.SelectedItem), SelectedItemChanged, this.LifetimeManager);
        }

        private void NavigateToTable()
        {
            var tableName = (Grid.SelectedItem as CloudTable).Name;
            PageStack.Navigate("accounts/" + currentStorageAccount.Credentials.AccountName + "/tables/" + tableName);
        }

 

        protected override void OnLoad()
        {
            base.OnLoad();
            var accountName = RouteVariables["account"];
            var accountInfo = (from account in StorageAccountInfo.Load() where account.AccountName == accountName select account).FirstOrDefault();
            currentStorageAccount = new CloudStorageAccount(new Microsoft.WindowsAzure.Storage.Auth.StorageCredentials(accountName, accountInfo.Key), accountInfo.UseHttps);
            Grid.DataSource = new TableListDataSource(currentStorageAccount.CreateCloudTableClient(), Application.MessagePump);
        }

        private void AddTable()
        {
            Dialog.ShowTextInput("Enter table name".ToConsoleString(), (name) =>
            {
                if (name != null)
                {
                    var t = currentStorageAccount.CreateCloudTableClient().GetTableReference(name.ToString()).CreateAsync();
                    t.ContinueWith((tPrime) =>
                    {
                        if (Application != null)
                        {
                            PageStack.TryRefresh();
                        }
                    });
                }
            });
        }

        private void DeleteSelectedTable()
        {
            Dialog.ConfirmYesOrNo("Are you sure you want ot delete table " + (Grid.SelectedItem as CloudTable).Name + "?", () =>
            {
                var table = currentStorageAccount.CreateCloudTableClient().GetTableReference((Grid.SelectedItem as CloudTable).Name);
                var t = table.DeleteAsync();
                t.ContinueWith((tPrime) =>
                {
                    if (Application != null)
                    {
                        PageStack.TryRefresh();
                    }
                });
            });
        }

        private void SelectedItemChanged()
        {
            deleteButton.CanFocus = Grid.SelectedItem != null;
        }
    }
}
