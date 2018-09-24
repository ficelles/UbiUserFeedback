using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
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
using UbiUserFeedback.Models;
using System.Net.Http.Formatting;

namespace UbiFeedbackApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        HttpClient _client = new HttpClient();

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;

            _ratingitems.Add(new RatingItem("1", 1));
            _ratingitems.Add(new RatingItem("2", 2));
            _ratingitems.Add(new RatingItem("3", 3));
            _ratingitems.Add(new RatingItem("4", 4));
            _ratingitems.Add(new RatingItem("5", 5));
            Rating = RatingItems.First();

            _filteritems.Add(new RatingItem("All", 0));
            _filteritems.Add(new RatingItem("1", 1));
            _filteritems.Add(new RatingItem("2", 2));
            _filteritems.Add(new RatingItem("3", 3));
            _filteritems.Add(new RatingItem("4", 4));
            _filteritems.Add(new RatingItem("5", 5));
            Filter = FilterItems.First();

            Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            ShowMessage("Start");

            _client.BaseAddress = new Uri("http://localhost:51269/api/products");
            _client.DefaultRequestHeaders.Accept.Clear();
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        async void GetFeedbackAsync()
        {
            var request = new HttpRequestMessage(HttpMethod.Get, new Uri("http://localhost:51269/api/feedbacks/"));
            request.Headers.Add("Ubi-Rating", Filter.Value.ToString());
            HttpResponseMessage response = await _client.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                var _message = await response.Content.ReadAsStringAsync();

                this.Dispatcher.Invoke(new Action(() =>
                {
                    ShowMessage("Response");
                    ShowMessage(_message);
                    ShowMessage("");
                }));

                var _objects = JsonConvert.DeserializeObject<List<object>>(_message);

                foreach (var _object in _objects)
                {
                    var _item = JsonConvert.DeserializeObject<Dictionary<string, string>>(_object.ToString());

                    string sessionid = _item["SessionID"] as string;
                    string userid = _item["UserID"] as string;
                    string comment = _item["Comment"] as string;
                    int rating = Convert.ToInt32(_item["Rating"]);
                    DateTime savedon = Convert.ToDateTime(_item["SavedOn"]);

                    this.Dispatcher.Invoke(new Action(() =>
                    {
                        ShowMessage("SessionID: " + sessionid);
                        ShowMessage("UserID: " + userid);
                        ShowMessage("Comment: " + comment);
                        ShowMessage("Rating: " + rating);
                        ShowMessage("SavedOn: " + savedon.ToShortDateString() + " " + savedon.ToShortTimeString());
                        ShowMessage("");
                    }));
                }
            }
            else
            {
                ShowMessage("Fail");
                ShowMessage(response.ToString());
            }
        }

        async void FindFeedbackAsync()
        {
            var request = new HttpRequestMessage(HttpMethod.Get, new Uri("http://localhost:51269/api/feedbacks/") + SessionID);
            request.Headers.Add("Ubi-UserId", UserID);
            HttpResponseMessage response = await _client.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                var _message = await response.Content.ReadAsStringAsync();

                this.Dispatcher.Invoke(new Action(() =>
                {
                    ShowMessage("Response");
                    ShowMessage(_message);
                    ShowMessage("");
                }));

                var _item = JsonConvert.DeserializeObject<Dictionary<string, string>>(_message);

                string sessionid = _item["SessionID"] as string;
                string userid = _item["UserID"] as string;
                string comment = _item["Comment"] as string;
                int rating = Convert.ToInt32(_item["Rating"]);
                DateTime savedon = Convert.ToDateTime(_item["SavedOn"]);

                this.Dispatcher.Invoke(new Action(() =>
                {
                    ShowMessage("SessionID: " + sessionid);
                    ShowMessage("UserID: " + userid);
                    ShowMessage("Comment: " + comment);
                    ShowMessage("Rating: " + rating);
                    ShowMessage("SavedOn: " + savedon.ToShortDateString() + " " + savedon.ToShortTimeString());
                    ShowMessage("");
                }));
            }
            else
            {
                ShowMessage("Fail");
                ShowMessage(response.ToString());
            }
        }

        async void AddOrUpdateFeedback()
        {
            var request = new HttpRequestMessage(HttpMethod.Get, new Uri("http://localhost:51269/api/feedbacks/") + SessionID);
            request.Headers.Add("Ubi-UserId", UserID);
            HttpResponseMessage response = await _client.SendAsync(request);
            var _message = await response.Content.ReadAsStringAsync();

            Feedback _feedback = new Feedback();
            _feedback.SessionID = SessionID;
            _feedback.UserID = UserID;
            _feedback.Comment = Comment;
            _feedback.Rating = Rating.Value;
            _feedback.SavedOn = DateTime.Now;

            if (!response.IsSuccessStatusCode)
            {
                AddFeedbackAsync(_feedback);
            }
            else
            {
                UpdateFeedbackAsync(_feedback);
            }
        }

        async void AddFeedbackAsync(Feedback _feedback)
        {
            HttpResponseMessage response = _client.PostAsJsonAsync(new Uri("http://localhost:51269/api/feedbacks/" + SessionID), _feedback).Result;

            var _message = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                this.Dispatcher.Invoke(new Action(() =>
                {
                    ShowMessage("Response");
                    ShowMessage(_message);
                    ShowMessage("");
                }));
            }
            else
            {
                ShowMessage("Add Fail");
                ShowMessage(response.ToString());
            }
        }

        async void UpdateFeedbackAsync(Feedback _feedback)
        {
            HttpResponseMessage response = await _client.PutAsJsonAsync(new Uri("http://localhost:51269/api/feedbacks/" + SessionID), _feedback);

            var _message = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                ShowMessage("Update Success");
            }
            else
            {
                ShowMessage("Update Fail");
                ShowMessage(response.ToString());
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void LocalPropertyChanged(String info)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));
        }

        public void ShowMessage(String _message)
        {
            this.Dispatcher.Invoke(new Action(() =>
            {
                _messages.Add(_message);
                LocalPropertyChanged("Messages");
            }));
        }

        private ObservableCollection<string> _messages = new ObservableCollection<string>();
        public ObservableCollection<string> Messages
        {
            get { return _messages; }
            set { _messages = value; }
        }

        private string _userid = string.Empty;
        public string UserID
        {
            get { return _userid; }
            set { _userid = value; }
        }

        private string _sessionid = string.Empty;
        public string SessionID
        {
            get { return _sessionid; }
            set { _sessionid = value; }
        }

        private RatingItem _rating;
        public RatingItem Rating
        {
            get { return _rating; }
            set { _rating = value; }
        }

        private string _comment = string.Empty;
        public string Comment
        {
            get { return _comment; }
            set { _comment = value; }
        }

        ObservableCollection<RatingItem> _ratingitems = new ObservableCollection<RatingItem>();
        public ObservableCollection<RatingItem> RatingItems
        {
            get { return _ratingitems; }
        }

        private RatingItem _filter;
        public RatingItem Filter
        {
            get { return _filter; }
            set { _filter = value; }
        }

        ObservableCollection<RatingItem> _filteritems = new ObservableCollection<RatingItem>();
        public ObservableCollection<RatingItem> FilterItems
        {
            get { return _filteritems; }
        }

        private void GetButton_Click(object sender, RoutedEventArgs e)
        {
            Cursor _cursor = this.Cursor;

            try
            {
                this.Dispatcher.Invoke((Action)(() =>
                {
                    this.Cursor = Cursors.Wait;
                    GetFeedbackAsync();
                }));
            }
            finally
            {
                this.Dispatcher.Invoke((Action)(() =>
                {
                    this.Cursor = _cursor;
                }));
            }
        }

        private void FindButton_Click(object sender, RoutedEventArgs e)
        {
            Cursor _cursor = this.Cursor;

            try
            {
                this.Dispatcher.Invoke((Action)(() =>
                {
                    this.Cursor = Cursors.Wait;
                    FindFeedbackAsync();
                }));
            }
            finally
            {
                this.Dispatcher.Invoke((Action)(() =>
                {
                    this.Cursor = _cursor;
                }));
            }
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            Cursor _cursor = this.Cursor;

            try
            {
                this.Dispatcher.Invoke((Action)(() =>
                {
                    this.Cursor = Cursors.Wait;
                    AddOrUpdateFeedback();
                }));
            }
            finally
            {
                this.Dispatcher.Invoke((Action)(() =>
                {
                    this.Cursor = _cursor;
                }));
            }
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }

    public class RatingItem
    {
        public string Description { get; set; }
        public int Value { get; set; }

        public RatingItem(string description, int value)
        {
            Description = description;
            Value = value;
        }

        public override string ToString()
        {
            return Description;
        }
    }
}
