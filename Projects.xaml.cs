using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Shapes;



namespace ProjectManager
{
    /// <summary>
    /// Логика взаимодействия для Projects.xaml
    /// </summary>
    public partial class Projects : Page
    {
        List<ProjectInfo> projectList = new List<ProjectInfo>();
        CancellationTokenSource cancellationTokenSource;
        public Projects()
        {
            InitializeComponent();


        }

        private void collectPrj(object sender, RoutedEventArgs e)
        {
            CollectFiles();
        }

        async void CollectFiles()
        {
            string rootFolder = "";

            var folderDialog = new OpenFolderDialog
            {
                // Set options here
            };

            if (folderDialog.ShowDialog() == true)
            {
                var folderName = folderDialog.FolderName;
                rootFolder = folderName;
                path.Text = rootFolder;
            }

            MessageBoxResult result = MessageBox.Show($"Начать сканирование на наличие проектов ?", "Подтвердите операцию", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                if (string.IsNullOrEmpty(rootFolder) || !Directory.Exists(rootFolder))
                {
                    MessageBox.Show("Неверный путь");
                    return;
                }

                projectList.Clear();


                cancellationTokenSource = new CancellationTokenSource();

                try
                {
                    await CollectFilesAsync(rootFolder, cancellationTokenSource.Token);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"An error occurred: {ex.Message}");
                }

                Dispatcher.Invoke(() => ProjectList.ItemsSource = projectList);
            }
            else 
            {
                MessageBox.Show("Операция отменена");
            }
          
        }

        async Task CollectFilesAsync(string directory, CancellationToken cancellationToken)
        {
            try
            {
                string[] slnFiles = Directory.GetFiles(directory, "*.sln");
                foreach (var slnFile in slnFiles)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    string projectType = "Неизвестно";
                    Brush color = Brushes.Red;

                    if (Directory.GetFiles(directory, "*.cs").Length > 0)
                    {
                        projectType = "csharp";
                        color = Brushes.DarkGreen;
                    }
                    else if (Directory.GetFiles(directory, "*.py").Length > 0)
                    {
                        projectType = "python";
                        color = Brushes.Brown;
                    }
                    else if (Directory.GetFiles(directory, "*.cpp").Length > 0)
                    {
                        projectType = "C++";
                        color = Brushes.Indigo;
                    }
                    else if (Directory.GetFiles(directory, "*.java").Length > 0)
                    {
                        projectType = "Java";
                        color = Brushes.Orange;
                    }

                    projectList.Add(new ProjectInfo
                    {
                        Path = slnFile,
                        ProjectType = projectType,
                        Creation_Time = "Создано: " + File.GetCreationTime(slnFile).ToString(),
                        color = color
                    });


                    Dispatcher.Invoke(() =>
                    {
                      
                        status.Text = $"Сканируется: {slnFile}";
                    });

                    await Task.Delay(10, cancellationToken);
                }

                string[] subDirectories = Directory.GetDirectories(directory);
                foreach (var subDirectory in subDirectories)
                {
                    await CollectFilesAsync(subDirectory, cancellationToken);
                }
            }
            catch (UnauthorizedAccessException uae)
            {

                MessageBox.Show($"Доступ запрещён {directory}: {uae.Message}");
            }

            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при выполнении: {directory}: {ex.Message}");
            }
        }

        private void PauseScan()
        {
            if (cancellationTokenSource != null && !cancellationTokenSource.IsCancellationRequested)
            {
                cancellationTokenSource.Cancel();
            }
        }

        private void StopScan()
        {
            if (cancellationTokenSource != null)
            {
                cancellationTokenSource.Cancel();
                projectList.Clear();
              
                status.Text = "Сканирование остановлено.";
            }
        }

        private void Button_ClickPause(object sender, RoutedEventArgs e)
        {
            PauseScan();
        }

        private void Button_ClickStop(object sender, RoutedEventArgs e)
        {
            StopScan();
        }

        private void searching(object sender, TextChangedEventArgs e)
        {
            string searchText = Search.Text.ToLower();

            if (string.IsNullOrEmpty(searchText))
            {

                ProjectList.ItemsSource = projectList;
            }
            else
            {

                var filteredProjects = projectList.Where(project =>
                    project.Path.ToLower().Contains(searchText) ||
                    project.ProjectType.ToLower().Contains(searchText) ||
                    project.Creation_Time.ToLower().Contains(searchText)
                ).ToList();


                ProjectList.ItemsSource = filteredProjects;
            }
        }

        private void ContextMenuItem1ClickedOpenFolder(object sender, RoutedEventArgs e)
        {
            // Get the selected item from the ListView
            var selectedItem = this.ProjectList.SelectedItem as ProjectInfo;

            if (selectedItem != null)
            {
                // Get the file path from the selected item
                string filePath = selectedItem.Path;

                // Get the directory path containing the file
                string directoryPath = System.IO.Path.GetDirectoryName(filePath);

                // Open the directory containing the file
                try
                {
                    Process.Start("explorer.exe", directoryPath);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Не получилость открыть: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Элемент не выбран.", "Инфо", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void ContextMenuItem2Clicked(object sender, RoutedEventArgs e)
        {
            // Get the selected item from the ListView
            var selectedItem = this.ProjectList.SelectedItem as ProjectInfo;

            if (selectedItem != null)
            {
              
                string filePath = selectedItem.Path;

               
                string directoryPath = System.IO.Path.GetDirectoryName(filePath);

               
                MessageBoxResult result = MessageBox.Show($"Вы уверены, что хотите польностью удалить проект: '{directoryPath}' и всё его содержимое?", "Подтвердите операцию", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                       
                        Directory.Delete(directoryPath, true);

                   
                        projectList.Remove(selectedItem);

              
                        ProjectList.ItemsSource = null;
                        ProjectList.ItemsSource = projectList;

                        MessageBox.Show("Проект был полностью стёрт.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Не удалось удалить: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("Элемент не выбран", "Инфо", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }













    // async void CollectFiles()
    // {
    //
    //     string rootFolder = path.Text;
    //     string[] directories = Directory.GetDirectories(rootFolder, "*", SearchOption.AllDirectories);
    //
    //     
    //
    //     foreach (string directory in directories)
    //     {
    //         string[] slnFiles = Directory.GetFiles(directory, "*.sln");
    //         foreach (var slnFile in slnFiles)
    //         {
    //             string projectType = "Неизвестно";
    //             System.Windows.Media.Brush color = System.Windows.Media.Brushes.Red;
    //             if (Directory.GetFiles(directory, "*.cs").Length > 0)
    //             {
    //                 projectType = "csharp";
    //                 color = System.Windows.Media.Brushes.DarkGreen;
    //             }
    //             else if (Directory.GetFiles(directory, "*.py").Length > 0)
    //             {
    //                 projectType = "python";
    //                 color = System.Windows.Media.Brushes.Brown;
    //             }
    //             else if (Directory.GetFiles(directory, "*.cpp").Length > 0)
    //             {
    //                 projectType = "C++";
    //                 color = System.Windows.Media.Brushes.Indigo;
    //             }
    //             else if (Directory.GetFiles(directory, "*.java").Length > 0)
    //             {
    //                 projectType = "Java";
    //                 color = System.Windows.Media.Brushes.Orange;
    //             }
    //
    //             projectList.Add(new ProjectInfo { Path = slnFile, ProjectType = projectType, Creation_Time = "Создано: " + File.GetCreationTime(slnFile).ToString(), color = color });
    //         }
    //     }
    //
    //     progress.Maximum = projectList.Count;
    //
    //     for (int i = 0; i < projectList.Count; i++)
    //     {
    //         status.Text = projectList[i].Path;
    //         progress.Value++;
    //         await Task.Delay(10);
    //     }
    //
    //     ProjectList.ItemsSource = projectList;
    //
    //  
    //
    //
    // }


}

