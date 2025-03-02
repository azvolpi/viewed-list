using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.SqlClient;
using System.Dynamic;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;
using MySql.Data.MySqlClient;
using static System.Net.Mime.MediaTypeNames;


namespace ListMovie
{
    public partial class MainWindow : Window
    {
        public ObservableCollection<Movie> Movies { get; set; }
        private string selectedImagePath = "";
        public MainWindow()
        {
            InitializeComponent();
            var movieService = new MovieService();
            Movies = movieService.GetMovies();
            DataContext = this;
            ReplaceImage.Visibility = Visibility.Collapsed;
            LoadMovies();
        }
        private void LoadMovies()
        {
            var movieService = new MovieService();
            Movies = movieService.GetMovies();
            // Перезагружаем ListView
            MoviesListView.ItemsSource = Movies;
        }
        private void Edit_Click(object sender, RoutedEventArgs e)
        {
            int.TryParse(TitleInput.Tag.ToString(), out int id);
            string title = TitleInput.Text;
            int.TryParse(YearInput.Text, out int year);
            int.TryParse(EpisodesInput.Text, out int episodes);
            double.TryParse(RatingInput.Text, out double rating);
            string description = DescriptionInput.Text;
            string connectionString = "Server=(localdb)\\MSSqlLocalDB;Database=ListMovie;Integrated Security=True;";
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = "UPDATE Anime SET NameAnime = @NameAnime, YearAnime = @YearAnime, EpisodesAnime = @EpisodesAnime, " +
                       "DescriptionAnime = @DescriptionAnime, MarkAnime = @MarkAnime, ImageAnime = @ImageAnime WHERE id = @id;";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@id", id);
                    command.Parameters.AddWithValue("@NameAnime", title);
                    command.Parameters.AddWithValue("@YearAnime", year);
                    command.Parameters.AddWithValue("@EpisodesAnime", episodes);
                    command.Parameters.AddWithValue("@MarkAnime", rating);
                    command.Parameters.AddWithValue("@DescriptionAnime", description);
                    command.Parameters.AddWithValue("@ImageAnime", selectedImagePath);

                    command.ExecuteNonQuery();
                }
            }
            MessageBox.Show("Изменено!");
            EditOverlay.Visibility = Visibility.Collapsed;
            LoadMovies();
        }
        private void AddClick(object sender, RoutedEventArgs e)
        {
            EditOverlay.Visibility = Visibility.Visible;
            EditButton.Visibility = Visibility.Collapsed;
            AddButton.Visibility = Visibility.Visible;
            TitleInput.Text = "";
            TitleInput.Tag = "Введите название";
            YearInput.Text = "";
            EpisodesInput.Text = "";
            RatingInput.Text = "";
            DescriptionInput.Text = "";
            ReplaceImage.Source = null;
            ReplaceImageTextBox.Visibility = Visibility.Visible;
        }
        // Закрытие оверлея
        private void CloseOverlay_Click(object sender, RoutedEventArgs e)
        {
            EditOverlay.Visibility = Visibility.Collapsed;
        }
        private void ChooseImage_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Файлы изображений|*.jpg;*.png;*.jpeg;*.bmp;*.gif;*.webp",
                Title = "Выберите изображение"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                selectedImagePath = openFileDialog.FileName;
                ReplaceImage.Visibility = Visibility.Visible;
                ReplaceImageTextBox.Visibility = Visibility.Collapsed;
                BitmapImage bimage = new BitmapImage();
                bimage.BeginInit();
                bimage.UriSource = new Uri(selectedImagePath, UriKind.Absolute);
                bimage.EndInit();
                ReplaceImage.Source = bimage;
            }
        }
        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (MoviesListView == null) return; // Защита от ошибки, если список ещё не инициализирован

            string searchText = SearchBox.Text.ToLower(); // Приводим к нижнему регистру

            var filteredMovies = Movies.Where(movie =>
                movie.NameAnime.ToLower().Contains(searchText) ||
                movie.YearAnime.ToString().Contains(searchText) ||
                movie.DescriptionAnime.ToLower().Contains(searchText)
            ).ToList();

            MoviesListView.ItemsSource = filteredMovies; // Обновляем список
        }
        // Добавление данных в базу
        private void AddMovie_Click(object sender, RoutedEventArgs e)
        {
            string title = TitleInput.Text;
            int.TryParse(YearInput.Text, out int year);
            int.TryParse(EpisodesInput.Text, out int episodes);
            double.TryParse(RatingInput.Text, out double rating);
            string description = DescriptionInput.Text;

            if (string.IsNullOrEmpty(title) || year == 0 || episodes == 0 || string.IsNullOrEmpty(description))
            {
                MessageBox.Show("Заполните все поля!");
                return;
            }

            string connectionString = "Server=(localdb)\\MSSqlLocalDB;Database=ListMovie;Integrated Security=True;";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = "INSERT INTO Anime (NameAnime, YearAnime, EpisodesAnime, MarkAnime, DescriptionAnime, ImageAnime) " +
                               "VALUES (@Name, @Year, @Episodes, @Rating, @Description, @Image)";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Name", title);
                    command.Parameters.AddWithValue("@Year", year);
                    command.Parameters.AddWithValue("@Episodes", episodes);
                    command.Parameters.AddWithValue("@Rating", rating);
                    command.Parameters.AddWithValue("@Description", description);
                    command.Parameters.AddWithValue("@Image", selectedImagePath); // Сохраняем путь

                    command.ExecuteNonQuery();
                }
            }
            MessageBox.Show("Фильм добавлен!");
            EditOverlay.Visibility = Visibility.Collapsed;
            LoadMovies();
        }

        private void DeleteMovie_Click(object sender, RoutedEventArgs e)
        {
            // Получаем название фильма, которое передается через Tag
            var button = sender as Button;
            var id = button?.Tag.ToString();
            // Подключаемся к базе данных и удаляем фильм по названию
            string connectionString = "Server=(localdb)\\MSSqlLocalDB;Database=ListMovie;Integrated Security=True;";
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = "DELETE FROM Anime WHERE id = @id";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@id", id);
                    int rowsAffected = command.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        MessageBox.Show($"Аниме '{id}' удалено!");
                        LoadMovies();
                    }
                }
            }
        }

        private void EditMovie_Click(object sender, RoutedEventArgs e)
        {
            EditButton.Visibility = Visibility.Visible;
            AddButton.Visibility = Visibility.Collapsed;
            var button = sender as Button;
            var movieName = button?.Tag.ToString();

            if (string.IsNullOrEmpty(movieName))
            {
                MessageBox.Show("Не удалось найти фильм для редактирования.");
                return;
            }

            // Подключаемся к базе данных и получаем все данные фильма по названию
            string connectionString = "Server=(localdb)\\MSSqlLocalDB;Database=ListMovie;Integrated Security=True;";
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT * FROM Anime WHERE NameAnime = @NameAnime";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@NameAnime", movieName);
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            // Предзаполняем поля о фильме в оверлее
                            TitleInput.Tag = reader["id"].ToString();
                            TitleInput.Text = reader["NameAnime"].ToString();
                            YearInput.Text = reader["YearAnime"].ToString();
                            EpisodesInput.Text = reader["EpisodesAnime"].ToString();
                            DescriptionInput.Text = reader["DescriptionAnime"].ToString();
                            RatingInput.Text = reader["MarkAnime"].ToString();

                            // Если нужно, можно добавить картинку или другие поля, связанные с фильмом
                            // Например, для изображения:
                            selectedImagePath = reader["ImageAnime"].ToString(); // Сохраняем путь к картинке
                            ReplaceImage.Visibility = Visibility.Visible;
                            ReplaceImageTextBox.Visibility = Visibility.Collapsed;
                            BitmapImage bimage = new BitmapImage();
                            bimage.BeginInit();
                            bimage.UriSource = new Uri(selectedImagePath, UriKind.Absolute);
                            bimage.EndInit();
                            ReplaceImage.Source = bimage;
                        }
                    }
                }
            }

            // Открываем оверлей для редактирования
            EditOverlay.Visibility = Visibility.Visible;
        }
    }

    public class Movie
    {
        public int id { get; set; }
        public string NameAnime { get; set; }
        public int YearAnime { get; set; }
        public int EpisodesAnime { get; set; }
        public string DescriptionAnime { get; set; }
        public string ImageAnime { get; set; }
        public int MarkAnime { get; set; }
    }

    public class MovieService
    {
        private string connectionString = "Server=(localdb)\\MSSqlLocalDB;Database=ListMovie;Integrated Security=True;";

        public ObservableCollection<Movie> GetMovies()
        {
            var movies = new ObservableCollection<Movie>();

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT id, NameAnime, YearAnime, EpisodesAnime, DescriptionAnime, ImageAnime, MarkAnime FROM Anime";

                using (var command = new SqlCommand(query, connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        movies.Add(new Movie
                        {
                            id = reader.GetInt32(reader.GetOrdinal("id")),
                            NameAnime = reader.GetString(reader.GetOrdinal("NameAnime")),
                            YearAnime = reader.GetInt32(reader.GetOrdinal("YearAnime")),
                            EpisodesAnime = reader.GetInt32(reader.GetOrdinal("EpisodesAnime")),
                            DescriptionAnime = reader.GetString(reader.GetOrdinal("DescriptionAnime")),
                            ImageAnime = reader.GetString(reader.GetOrdinal("ImageAnime")),
                            MarkAnime = reader.GetInt32(reader.GetOrdinal("MarkAnime"))
                        });
                    }
                }
            }
            return movies;
        }
    }
}
