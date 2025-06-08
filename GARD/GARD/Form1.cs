using GARD.Models;
using MetroSet_UI.Forms;
using System.Text;
using System.Text.Json;

namespace GARD
{
    public partial class Form1 : MetroSetForm
    {
        private int currentSubscriberId = -1;
        private readonly HttpClient client = new HttpClient();
       
      
        public Form1()
        {
            InitializeComponent();
            ChartManager.CreateCharts(PageTabs);
            loadSubscribers();
            dgvsubscribers.CellClick += dgvsubscribers_CellClick;
            filterSubs.SelectedIndex = 0;
            sub_status.SelectedIndex = 0;
        }

        private void clearFields_Click(object sender, EventArgs e)
        {
            sub_email.Text = "";
            sub_name.Text = "";
            sub_status.Text = "active";
        }

        private async void signup_sign_Click(object sender, EventArgs e)
        {
            var signupData = new
            {
                user_sign = user_sign.Text.Trim(),
                email_sign = email_sign.Text.Trim(),
                pass_sign = pass_sign.Text.Trim(),
            };

            string json = JsonSerializer.Serialize(signupData);
            StringContent content = new StringContent(json, Encoding.UTF8, "application/json");
            HttpResponseMessage response = await client.PostAsync("http://localhost:5000/signup", content);

            if (response.IsSuccessStatusCode)
            {
                MessageBox.Show("Signup successful");
                PageTabs.SelectedIndex = 1;
            }
            else
            {
                MessageBox.Show("Failed to sign up: " + response.StatusCode);
            }
        }


        
        private void login_sign_Click(object sender, EventArgs e)
        {
            PageTabs.SelectedIndex = 1;
        }

        private void show_sign_CheckedChanged(object sender, EventArgs e)
        {
            pass_sign.UseSystemPasswordChar = !show_sign.Checked;
        }


        private void login_sign_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            PageTabs.SelectedIndex = 1;
        }


        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            pass_login.UseSystemPasswordChar = !checkBox1.Checked;
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            string username = user_login.Text.Trim();
            string password = pass_login.Text.Trim();

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Please fill in all fields.");
                return;
            }

            var loginData = new { username, password };
            string json = JsonSerializer.Serialize(loginData);
            StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

            try
            {
                HttpResponseMessage response = await client.PostAsync("http://localhost:5000/admins/login", content);
                if (response.IsSuccessStatusCode)
                {
                    string responseContent = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<LoginResponse>(responseContent);

                    if (result != null && result.admin_id > 0)
                    {
                        currentAdminId = result.admin_id;
                        PageTabs.SelectedIndex = 2;
                        await LoadSmtpSettingsAsync(currentAdminId);
                        loadSubscribers();
                        await LoadCampaignsAsync();
                        await LoadEmailLogsAsync();
                    }
                    else
                    {
                        MessageBox.Show("Invalid username or password.");
                    }
                }
                else
                {
                    MessageBox.Show("Failed to login: " + response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred: " + ex.Message);
            }
        }
  
    }
}


