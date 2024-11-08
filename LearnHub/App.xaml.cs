﻿using LearnHub.Data;
using Microsoft.EntityFrameworkCore;
﻿using LearnHub.Stores;
using LearnHub.ViewModels;
using System.Configuration;
using System.Data;
using System.Windows;
using LearnHub.ViewModels.AuthenticationViewModels;

namespace LearnHub
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private readonly string _connectionString = "Data Source=LearnHubSqlite.db";
        private readonly LearnHubDbContextFactory _dbContextFactory;


        public App()
        {
            _dbContextFactory = new LearnHubDbContextFactory(_connectionString);
        }
        protected override void OnStartup(StartupEventArgs e)
        {
            //Tự động cập nhật database hoặc tạo mới nếu chưa có từ migration mới nhất
            using (LearnHubDbContext context = _dbContextFactory.CreateDbContext())
            {
                context.Database.Migrate();
            }
            NavigationStore navigationStore = new NavigationStore();
            navigationStore.CurrentViewModel = new LoginViewModel();

            MainWindow = new MainWindow()
            {
                DataContext = new MainViewModel(navigationStore)
            };

            MainWindow.Show();
            base.OnStartup(e);
        }
    }
}
