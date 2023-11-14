using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
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
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace WebReq
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>

    class weatherItem : INotifyPropertyChanged
    {
        string date_v, weekDay_v, desc_v, image_p;
        public string date
        {
            get
            {
                return date_v;
            }
            set
            {
                date_v = value;
                OnPropertyChanged();
            }
        }
        public string image
        {
            get
            {
                return image_p;
            }
            set
            {
                image_p = value;
                OnPropertyChanged();
            }
        }
        public string weekDay
        {
            get
            {
                return weekDay_v;
            }
            set
            {
                weekDay_v = value;
                OnPropertyChanged();
            }
        }
        public string description
        {
            get
            {
                return desc_v;
            }
            set
            {
                desc_v = value;
                OnPropertyChanged();
            }
        }
        public weatherItem()
        {
            date = "Date Test";
            weekDay = "WeekDay Test";
            description = "Desc Test";
        }
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }
    }

    class uiElement
    {
        public Label date;
        public Label weekDay;
        public Label description;
        public Image image;
    }

    public partial class MainWindow : Window
    {
        List<weatherItem> lst = new List<weatherItem>();
        public string Temp { get; set; }
        weatherItem wIt;

        public MainWindow()
        {
            InitializeComponent();
            Temp = "default";
            wIt = new weatherItem();

            
            /*Binding myBinding = new Binding();
            myBinding.Source = wIt;
            myBinding.Mode = BindingMode.TwoWay;*/
            this.DataContext = wIt;
            // задание привязки из кода
            string[] propertyName = { "date", "weekDay", "description" };

            Label[,] components = { { t_dt, t_weekday, t_des }, { t_dt1, t_weekday1, t_des1 }, 
                { t_dt2, t_weekday2, t_des2 }, { t_dt3, t_weekday3, t_des3 } };
            Image[] imagename = { t_img, t_img1, t_img2, t_img3 };
            Binding myBinding2;
            for (int j=0;j<4;j++)
            {
                wIt = new weatherItem();
                for (int i = 0; i<propertyName.Length;i++)
                { 
                    myBinding2 = new Binding();
                    myBinding2.Source = wIt;
                    myBinding2.Path = new PropertyPath(propertyName[i]);
                    myBinding2.Mode = BindingMode.TwoWay;
                    myBinding2.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
                    BindingOperations.SetBinding(components[j,i], Label.ContentProperty, myBinding2);
                }
                myBinding2 = new Binding();
                myBinding2.Source = wIt;
                myBinding2.Path = new PropertyPath("image");
                myBinding2.Mode = BindingMode.TwoWay;
                myBinding2.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
                BindingOperations.SetBinding(imagename[j], Image.SourceProperty, myBinding2);
                lst.Add(wIt);
            }
            // привязка свойства задана
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            GetData();
        }
        void GetData()
        {
            WebRequest wr = WebRequest.Create(tbaddress.Text);
            WebResponse response = wr.GetResponse();
            string json = "";
            using (Stream stream = response.GetResponseStream())
            {
                using (StreamReader reader = new StreamReader(stream))
                {
                    tbresult.Text = json;
                    json = reader.ReadToEnd();
                }
            }
            response.Close();
            tbresult.Clear();
            try
            {

                JObject jobj = JObject.Parse(json);
                JToken jt = jobj["list"];
                bool firstDay = true;
                Dictionary<string, double> d = new Dictionary<string, double>(); // Создаем словарь
                int itemCount = 0;
                int date0 = 0;

                foreach (JToken item in jt) //идет по всем записям
                {
                    
                    JToken list0 = item;
                    JToken dt_t = list0["dt"];
                    int dt = dt_t.ToObject<int>(); // Достал значение dt из запаковки
                    DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
                    dateTime = dateTime.AddSeconds(dt);
                    tbresult.Text += $"\n Запись с датой {dateTime.Day}-{dateTime.Month}-{dateTime.Year} Время {dateTime.Hour} : {dateTime.Minute}";
                    //wIt.date = $"{dateTime.Day}-{dateTime.Month}-{dateTime.Year}";
                    //wIt.weekDay = $"{dateTime.DayOfWeek}";
                    int dateDay = dateTime.DayOfYear;
                    if (firstDay)
                    {
                        date0 = dateTime.DayOfYear; // Сегодняшний день
                        firstDay = false;
                    }
                    int dateDif = dateDay - date0;
                    //if (dateDif >= 0 && dateDif)
                    /*if (itemCount == 0)
                    {
                        wIt.date = $"{dateTime.Day}-{dateTime.Month}-{dateTime.Year}";
                        wIt.weekDay = $"{dateTime.DayOfWeek}";
                    }*/
                    
                    JToken main = list0["main"]; // Достаем значение main из запаковки
                    JToken temp = main["temp"]; // Достаем значение температуры из запаковки
                    double t = temp.ToObject<double>();
                    t = t - 273.15; // в градусы цельсия
                    t = Math.Round(t * 10) / 10.0; // Округляем значение температуры до десятых
                    tbresult.Text += $"\n Температура {t} градуса";
                    JToken weather = item["weather"]; // Достаем значения погоды из запаковки

                    JToken weather0 = weather[0];
                    JToken id = weather0["id"];
                    int id_code = id.ToObject<int>();
                    JToken icon = weather0["icon"]; // Достаем значение иконки из запаковки
                    string icon_text = icon.ToObject<string>();
                    string filename = $"../../Res/{icon_text}@2x.png";
                    string[] weekDays = {"Sun","Mon","Tue","Wen","Thu","Fri","Sat"};
                    if (dateTime.Hour > 9 && dateTime.Hour < 21)
                    {
                        wIt = lst[dateDif];
                        wIt.date = $"{dateTime.Day}-{dateTime.Month}-{dateTime.Year}";
                        wIt.description = $"t={t}C";
                        wIt.weekDay = weekDays[(int)dateTime.DayOfWeek];
                        wIt.image = filename;
                        /*switch (dateDif)
                        {
                            case 0:
                                {
                                    //t_dt.Content = $"{dateTime.Day}-{dateTime.Month}-{dateTime.Year}";
                                     wIt.date = $"{dateTime.Day}-{dateTime.Month}-{dateTime.Year}";
                                    wIt.description = $"t={t}C";
                                    wIt.weekDay =weekDays[(int)dateTime.DayOfWeek];
                                  //  t_des.Content = $"t={t}C";
                                    break;
                                }
                            case 1:
                                {
                                    t_dt1.Content = $"{dateTime.Day}-{dateTime.Month}-{dateTime.Year}";
                                    t_des1.Content = $"t={t}C";
                                    break;
                                }
                            case 2:
                                {
                                    t_dt2.Content = $"{dateTime.Day}-{dateTime.Month}-{dateTime.Year}";
                                    t_des2.Content = $"t={t}C";
                                    break;
                                }
                        }*/
                    }
                    if (itemCount == 0)

                    {
                        t_img.Source = new BitmapImage(new Uri(filename, UriKind.Relative));
                        Debug.WriteLine(filename);
                    }
                    itemCount++;
                }

            }catch
            {
                tbresult.Text = "Incorrect json file";
            }

        }

        async private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            HttpClient client = new HttpClient();
            HttpRequestMessage request = new HttpRequestMessage();
            request.RequestUri = new Uri(tbaddress.Text);
            request.Method = HttpMethod.Get;
            request.Headers.Add("Accept", "application/json");

            HttpResponseMessage response = await client.SendAsync(request);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                HttpContent responseContent = response.Content;
                var json = await responseContent.ReadAsStringAsync();
                tbresult.Text = json;
            }
        }


        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            uiElement el = new uiElement();
            el.date = t_dt1;
            el.weekDay = t_weekday1;
            el.image = t_img1;
            el.description = t_des1;
            //lst.Add(el);
            GetData();
        }
    }
}
