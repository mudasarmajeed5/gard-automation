using MetroSet_UI.Forms;
using System.Text.Json;
using System.Text;
using System.Text.RegularExpressions;
namespace GARD
{
    public partial class Form1 : MetroSetForm
    {
        private int currentSubscriberId = -1;
        List<Subscriber> allSubscribers = new List<Subscriber>();
        public Form1()
        {
            InitializeComponent();
            ChartManager.CreateCharts(metroSetTabControl1);
            loadSubscribers();
            dgvsubscribers.CellClick += dgvsubscribers_CellClick;
            filterSubs.SelectedIndex = 0;
            sub_status.SelectedIndex = 0;
        }

        async void loadSubscribers()
        {
            using (HttpClient client = new HttpClient())
            {
                HttpResponseMessage response = await client.GetAsync("http://localhost:5000/subscribers");
                if (response.IsSuccessStatusCode)
                {
                    string jsonData = await response.Content.ReadAsStringAsync();
                    var subscribers = JsonSerializer.Deserialize<List<Subscriber>>(jsonData);
                    allSubscribers = subscribers;
                    dgvsubscribers.DataSource = allSubscribers;
                    dgvsubscribers.DataSource = subscribers;
                    dgvsubscribers.Columns["id"].HeaderText = "Subscriber ID";
                    dgvsubscribers.Columns["name"].HeaderText = "Name";
                    dgvsubscribers.Columns["email"].HeaderText = "Email";
                    dgvsubscribers.Columns["subscribed_at"].HeaderText = "Subscribed At";
                    dgvsubscribers.Columns["status"].HeaderText = "Status";
                    dgvsubscribers.Columns["updated_at"].HeaderText = "Updated At";
                }
                else
                {
                    MessageBox.Show("Failed to fetch data: " + response.StatusCode);
                }
            }
        }

        private void dgvsubscribers_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = dgvsubscribers.Rows[e.RowIndex];
                currentSubscriberId = Convert.ToInt32(row.Cells["id"].Value);
                sub_status.Text = row.Cells["status"].Value?.ToString() ?? "";
                sub_name.Text = row.Cells["name"].Value?.ToString() ?? "";
                sub_email.Text = row.Cells["email"].Value?.ToString() ?? "";
            }
        }


        async void addSubscriberToDatabase(string status, string name, string email)
        {
            using (HttpClient client = new HttpClient())
            {
                var subscriberData = new
                {
                    status = status,
                    name = name,
                    email = email
                };
                string json = JsonSerializer.Serialize(subscriberData);
                StringContent content = new StringContent(json, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await client.PostAsync("http://localhost:5000/subscribers/add", content);
                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show("Subscriber added successfully");
                    loadSubscribers();
                }
                else
                {
                    MessageBox.Show("Failed to fetch data: " + response.StatusCode);
                }
            }
        }

        async void updateSubscriberInDatabase()
        {
            if (currentSubscriberId == -1)
            {
                MessageBox.Show("Select a subscriber to update.");
                return;
            }

            using (HttpClient client = new HttpClient())
            {
                var subscriberData = new
                {
                    id = currentSubscriberId,
                    status = sub_status.Text.Trim(),
                    name = sub_name.Text.Trim(),
                    email = sub_email.Text.Trim()
                };

                string json = JsonSerializer.Serialize(subscriberData);
                StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

                // Your backend should have an update endpoint like /subscribers/update
                HttpResponseMessage response = await client.PutAsync("http://localhost:5000/subscribers/update", content);

                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show("Subscriber updated successfully");
                    loadSubscribers();
                }
                else
                {
                    MessageBox.Show("Failed to update subscriber: " + response.StatusCode);
                }
            }
        }

        async void deleteSubscriberInDatabase()
        {
            if (currentSubscriberId == -1)
            {
                MessageBox.Show("Select a subscriber to delete.");
                return;
            }

            var confirmResult = MessageBox.Show("Are you sure to delete this subscriber?",
                                                 "Confirm Delete",
                                                 MessageBoxButtons.YesNo);
            if (confirmResult == DialogResult.Yes)
            {
                using (HttpClient client = new HttpClient())
                {
                    HttpResponseMessage response = await client.DeleteAsync($"http://localhost:5000/subscribers/delete/{currentSubscriberId}");

                    if (response.IsSuccessStatusCode)
                    {
                        MessageBox.Show("Subscriber deleted successfully");
                        loadSubscribers();
                        currentSubscriberId = -1;
                    }
                    else
                    {
                        MessageBox.Show("Failed to delete subscriber: " + response.StatusCode);
                    }
                }
            }
        }

        private void addSubscriber_Click(object sender, EventArgs e)
        {
            string status = sub_status.Text.Trim();
            string name = sub_name.Text.Trim();
            string email = sub_email.Text.Trim();

            if (string.IsNullOrEmpty(status))
            {
                MessageBox.Show("Please enter status.");
                return;
            }
            if (string.IsNullOrEmpty(name))
            {
                MessageBox.Show("Please enter name.");
                return;
            }
            if (string.IsNullOrEmpty(email))
            {
                MessageBox.Show("Please enter email.");
                return;
            }

            // Basic email format check
            string emailPattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
            if (!Regex.IsMatch(email, emailPattern))
            {
                MessageBox.Show("Please enter a valid email address.");
                return;
            }

            addSubscriberToDatabase(status, name, email);
        }

        private void updateSubscriber_Click(object sender, EventArgs e)
        {
            updateSubscriberInDatabase();
        }
        private void btnDeleteSubscriber_Click(object sender, EventArgs e)
        {
            deleteSubscriberInDatabase();
        }

        private void filterSubs_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selectedStatus = filterSubs.SelectedItem.ToString().ToLower();

            if (selectedStatus == "all")
            {
                dgvsubscribers.DataSource = allSubscribers;
            }
            else
            {
                var filtered = allSubscribers
                    .Where(sub => sub.status != null && sub.status.ToLower() == selectedStatus)
                    .ToList();

                dgvsubscribers.DataSource = filtered;
            }
        }

    }
    public class Subscriber
    {
        public int id { get; set; }
        public string name { get; set; }
        public string email { get; set; }
        public string subscribed_at { get; set; }
        public string status { get; set; }
        public string updated_at { get; set; }
    }
}