using MetroSet_UI.Forms;
using System.Data;
using System.Net.Http.Headers;
using Newtonsoft.Json;
namespace GARD
{
    public partial class Form1 : MetroSetForm
    {
        public Form1()
        {
            InitializeComponent();
            ChartManager.CreateCharts(metroSetTabControl1);
            loadSubs();
        }
        async void loadSubs()
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.BaseAddress = new Uri("http://localhost:5000/");
                    client.Timeout = TimeSpan.FromSeconds(10);

                    var response = await client.GetAsync("api/subscribers");

                    if (response.IsSuccessStatusCode)
                    {
                        var jsonResponse = await response.Content.ReadAsStringAsync();
                        var subscribers = JsonConvert.DeserializeObject<List<Subscriber>>(jsonResponse);

                        dgvsubscribers.DataSource = subscribers;
                        dgvsubscribers.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

                        // Optional: Format datetime columns
                        dgvsubscribers.Columns["SubscribedAt"].DefaultCellStyle.Format = "g";
                        dgvsubscribers.Columns["UpdatedAt"].DefaultCellStyle.Format = "g";
                    }
                    else
                    {
                        MessageBox.Show($"Error: {response.StatusCode}");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }

        }
        public class Subscriber
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string Email { get; set; }
            public DateTime SubscribedAt { get; set; }
            public string Status { get; set; }
            public DateTime UpdatedAt { get; set; }
        }
    }
}