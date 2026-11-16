using Pr6Client.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing.Drawing2D;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Pr6Client
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        HttpClient client = new HttpClient();

        public ObservableCollection<Serv> Servs { get; set; } = new();
        Serv C = new();
        public Serv? CurrentServ { get; set; } = new();

        public bool? activefilt { get; set; } = null;

        public int? minfilt { get; set; } = null;

        public MainWindow()
        {
            client.BaseAddress = new Uri("https://localhost:7129/");

            InitializeComponent();
            Current.DataContext = C;
            DataContext = this;
            GetAll();
        }

        public async void GetAll()
        {
            if (activefilt != null && minfilt != null)//всё разом
            {
                try
                {
                    var response = await client.GetFromJsonAsync<List<Serv>>($"api/v1/Services?IsActive={activefilt.ToString().ToLower()}&DurationMinutes={minfilt}");
                    Servs.Clear();
                    if (response != null)
                    {
                        foreach (var item in response)
                        {
                            Servs.Add(item);
                        }
                    }
                }
                catch (System.Net.Http.HttpRequestException)
                {
                    MessageBox.Show($"Связь с API сервером не установлена");
                }
                catch
                {
                    Servs.Clear();
                }
            }
            else
            {
                if (minfilt != null)//время
                {
                    try
                    {
                        var response = await client.GetFromJsonAsync<List<Serv>>($"api/v1/Services?DurationMinutes={minfilt}");
                        Servs.Clear();
                        if (response != null)
                        {
                            foreach (var item in response)
                            {
                                Servs.Add(item);
                            }
                        }
                    }
                    catch (System.Net.Http.HttpRequestException)
                    {
                        MessageBox.Show($"Связь с API сервером не установлена");
                    }
                    catch
                    {
                        Servs.Clear();
                    }
                }
                else
                {
                    if (activefilt != null)//активность
                    {
                        try
                        {
                            var response = await client.GetFromJsonAsync<List<Serv>>($"api/v1/Services?IsActive={activefilt.ToString().ToLower()}");
                            Servs.Clear();
                            if (response != null)
                            {
                                foreach (var item in response)
                                {
                                    Servs.Add(item);
                                }
                            }
                        }
                        catch (System.Net.Http.HttpRequestException)
                        {
                            MessageBox.Show($"Связь с API сервером не установлена");
                        }
                        catch
                        {
                            Servs.Clear();
                        }
                    }
                    else//пустое
                    {
                        try
                        {
                            var response = await client.GetFromJsonAsync<List<Serv>>("api/v1/Services");
                            Servs.Clear();
                            if (response != null)
                            {
                                foreach (var item in response)
                                {
                                    Servs.Add(item);
                                }
                            }
                        }
                        catch (System.Net.Http.HttpRequestException)
                        {
                            MessageBox.Show($"Связь с API сервером не установлена");
                        }
                    }
                }
            }
        }
        private void Change(object sender, SelectionChangedEventArgs e) //Заполнение полей
        {
            C = new();

            if (CurrentServ != null)
            {
                C = new Serv();
                C.Id = CurrentServ.Id;
                C.Name = CurrentServ.Name;
                C.Description = CurrentServ.Description;
                C.DurationMinutes = CurrentServ.DurationMinutes;
                C.Price = CurrentServ.Price;
                C.IsActive = CurrentServ.IsActive;
            }
            Current.DataContext = C;
        }

        private async void Button_Click(object sender, RoutedEventArgs e)//Patch частич
        {
            if (C.Id != 0)
            {
                try
                {
                    var patchData = new DopSer
                    {
                        Name = string.IsNullOrEmpty(C.Name) ? null : C.Name,
                        Description = string.IsNullOrEmpty(C.Description) ? null : C.Description,
                        DurationMinutes = (C.DurationMinutes == 0) ? null : C.DurationMinutes,
                        Price = (C.Price == 0m) ? null : C.Price,
                        IsActive = C.IsActive
                    };
                    HttpResponseMessage res = await client.PatchAsJsonAsync(
                        $"api/v1/services/{C.Id}",
                        patchData
                    );

                    if (res.IsSuccessStatusCode)
                    {
                        MessageBox.Show("Успешно изменено (Patch)");
                        GetAll();
                    }
                    else
                    {
                        string errorContent = await res.Content.ReadAsStringAsync();
                        MessageBox.Show($"Ошибка Patch: {res.StatusCode}\n{errorContent}");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Исключение при Patch: {ex.Message}");
                }
            }
            else
            {
                MessageBox.Show("Выберите элемент!");
            }
        }

        private async void ChangeWhole(object sender, RoutedEventArgs e)//Put полное
        {
            if (C.Id != 0)
            {
                try
                {
                    var putData = new Serv
                    {
                        Id = C.Id,
                        Name = C.Name ?? "",
                        Description = C.Description ?? "",
                        DurationMinutes = C.DurationMinutes,
                        Price = C.Price,
                        IsActive = C.IsActive
                    };

                    HttpResponseMessage res = await client.PutAsJsonAsync(
                        $"api/v1/services/{C.Id}",
                        putData
                    );

                    if (res.IsSuccessStatusCode)
                    {
                        MessageBox.Show("Успешно изменено (Put)");
                        GetAll();
                    }
                    else
                    {
                        string errorContent = await res.Content.ReadAsStringAsync();
                        MessageBox.Show($"Ошибка Put: {res.StatusCode}\n{errorContent}");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Исключение при Put: {ex.Message}");
                }
            }
            else
            {
                MessageBox.Show("Выберите элемент!");
            }
        }

        private async void Create(object sender, RoutedEventArgs e) //добавить
        {
            try
            {
                HttpResponseMessage res = await client.PostAsJsonAsync(
                    $"api/v1/services",
                    new
                    {
                        Name = C.Name,
                        Description = C.Description,
                        DurationMinutes = C.DurationMinutes,
                        Price = C.Price,
                        IsActive = C.IsActive
                    }
                    );

                if (res.IsSuccessStatusCode)
                {
                    MessageBox.Show("Успешно добавлено");
                    GetAll();
                }
                else
                {
                    MessageBox.Show("Неправильные данные");
                }
            }
            catch (System.Net.Http.HttpRequestException)
            {
                MessageBox.Show($"Связь с API сервером не установлена");
            }
            catch
            {
                MessageBox.Show("Неверные данные");
            }
        }

        private async void Delete(object sender, RoutedEventArgs e)
        {
            if (C.Id != 0)
            {
                HttpResponseMessage res = await client.DeleteAsync($"api/v1/services/{C.Id}");

                if (res.IsSuccessStatusCode)
                {
                    MessageBox.Show("Успешное удаление");
                }
                else
                {
                    MessageBox.Show("Ошибка удаления");
                }
                GetAll();
            }
            else
            {
                MessageBox.Show("Выберите элемент!");
            }
        }
        private void TimeFilterComboBox2_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (int.TryParse(TimeFilterComboBox2.Text, out int res))
            {
                minfilt = res;
            }
            else
            {
                minfilt = null;
            }
            GetAll();
        }

        private void ActiveFilter(object sender, SelectionChangedEventArgs e)
        {
            if (ActiveFilterComboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                string text = selectedItem.Content.ToString();
                switch (text)
                {
                    case "Активные": activefilt = true; break;
                    case "Неактивные": activefilt = false; break;
                    default: activefilt = null; break;
                }
            }
            else
            {
                activefilt = null;
            }
            GetAll();
        }
    }
}