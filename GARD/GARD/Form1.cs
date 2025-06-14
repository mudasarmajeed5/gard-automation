using GARD.Models;
using MetroSet_UI.Forms;
using System.Text;
using System.Text.Json;

namespace GARD
{
    public partial class Form1 : MetroSetForm
    {
        private readonly HttpClient client = new HttpClient();
        private int currentAdminId = -1;

        public Form1()
        {
            InitializeComponent();
            dgvsubscribers.CellClick += dgvsubscribers_CellClick;
            filterSubs.SelectedIndex = 0;
            sub_status.SelectedIndex = 0; 
            this.Load += Form1_Load;
            pass_login.UseSystemPasswordChar = true;
            pass_sign.UseSystemPasswordChar = true;

        }
        private void Form1_Load(object sender, EventArgs e)
        {
            SetupEditorPanels();
        }
        private void UpdateStatsLabels()
        {
            total_campaigns_label.Text = (allCampaigns?.Count ?? 0).ToString();
            total_campaigns_label1.Text = (allCampaigns?.Count ?? 0).ToString();
            total_subscribers_label.Text = (allSubscribers?.Count ?? 0).ToString();
            total_email_logs_label.Text = countEmailLogs.ToString();
            total_email_logs_label1.Text = countEmailLogs.ToString();
            label22.Text = "89%";
        }
        private void clearFields_Click(object sender, EventArgs e)
        {
            sub_email.Text = "";
            sub_name.Text = "";
            sub_status.Text = "active";
        }

        private async void signup_sign_Click(object sender, EventArgs e)
        {
            string username = user_sign.Text.Trim();
            string email = email_sign.Text.Trim();
            string password = pass_sign.Text.Trim();

            // Password constraint check
            if (password.Length < 8 ||
                !password.Any(char.IsUpper) ||
                !password.Any(char.IsLower) ||
                !password.Any(char.IsDigit) ||
                !password.Any(ch => !char.IsLetterOrDigit(ch)))
            {
                MessageBox.Show("Password must be at least 8 characters long and include:\n- Uppercase letter\n- Lowercase letter\n- Number\n- Special character", "Weak Password", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var signupData = new
            {
                user_sign = username,
                email_sign = email,
                pass_sign = password
            };

            string json = JsonSerializer.Serialize(signupData);
            StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

            try
            {
                HttpResponseMessage response = await client.PostAsync("http://localhost:5000/signup", content);

                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show("Signup successful");
                    PageTabs.SelectedIndex = 1;
                }
                else
                {
                    MessageBox.Show("Signup failed: " + response.ReasonPhrase);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
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
                        await loadSubscribers();
                        await LoadCampaignsAsync();
                        await LoadEmailLogsAsync();

                        UpdateStatsLabels();
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

        private void SignUp_Click(object sender, EventArgs e)
        {
            PageTabs.SelectedIndex = 0;
        }

        
    }
}


