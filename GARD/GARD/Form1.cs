using GARD.Models;
using MetroSet_UI.Forms;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace GARD
{
    public partial class Form1 : MetroSetForm
    {
        private int currentSubscriberId = -1;
        private readonly HttpClient client = new HttpClient();
        private int currentAdminId = -1;
        List<Subscriber> allSubscribers = new List<Subscriber>();
        List<Campaign> allCampaigns = new List<Campaign>();
        private int currentCampaignId = -1;
        public Form1()
        {
            InitializeComponent();
            ChartManager.CreateCharts(PageTabs);
            loadSubscribers();
            dgvsubscribers.CellClick += dgvsubscribers_CellClick;
            filterSubs.SelectedIndex = 0;
            sub_status.SelectedIndex = 0;
            this.Load += Form1_Load;
        }
        private async void Form1_Load(object sender, EventArgs e)
        {
        }
        async void loadSubscribers()
        {
            if (currentAdminId == -1) return;

            HttpResponseMessage response = await client.GetAsync($"http://localhost:5000/subscribers/{currentAdminId}");
            if (response.IsSuccessStatusCode)
            {
                string jsonData = await response.Content.ReadAsStringAsync();
                allSubscribers = JsonSerializer.Deserialize<List<Subscriber>>(jsonData);
                dgvsubscribers.DataSource = allSubscribers;
                dgvsubscribers.Columns["id"].HeaderText = "User ID";
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


        private async Task LoadCampaignsAsync()
        {
            if (currentAdminId == -1) return;
            HttpResponseMessage response = await client.GetAsync($"http://localhost:5000/campaigns/{currentAdminId}");
            if (response.IsSuccessStatusCode)
            {
                string jsonData = await response.Content.ReadAsStringAsync();
                var campaigns = JsonSerializer.Deserialize<List<Campaign>>(jsonData);

                campaigns_table.DataSource = campaigns;
                allCampaigns = campaigns;
                UserCampaigns.DataSource = allCampaigns;
                UserCampaigns.DisplayMember = "campaign_name";
                campaigns_table.Columns["id"].HeaderText = "Campaign ID";
                campaigns_table.Columns["campaign_name"].HeaderText = "Name";
                campaigns_table.Columns["content"].HeaderText = "Email Content";
                campaigns_table.Columns["sent_at"].HeaderText = "Created On";
            }
            else
            {
                MessageBox.Show("Failed to fetch campaigns: " + response.StatusCode);
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

        private async Task addSubscriberToDatabase(string status, string name, string email)
        {

            var subscriberData = new { admin_id = currentAdminId, status, name, email };
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
                MessageBox.Show("Failed to add subscriber: " + response.StatusCode);
            }
        }


        private async Task updateSubscriberInDatabase()
        {
            if (currentSubscriberId == -1)
            {
                MessageBox.Show("Select a subscriber to update.");
                return;
            }


            var subscriberData = new
            {
                id = currentSubscriberId,
                status = sub_status.Text.Trim(),
                name = sub_name.Text.Trim(),
                email = sub_email.Text.Trim()
            };

            string json = JsonSerializer.Serialize(subscriberData);
            StringContent content = new StringContent(json, Encoding.UTF8, "application/json");
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


        private async Task deleteSubscriberInDatabase()
        {
            if (currentSubscriberId == -1)
            {
                MessageBox.Show("Select a subscriber to delete.");
                return;
            }

            var confirmResult = MessageBox.Show("Are you sure to delete this subscriber?", "Confirm Delete", MessageBoxButtons.YesNo);
            if (confirmResult == DialogResult.Yes)
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


        private async void addSubscriber_Click(object sender, EventArgs e)
        {
            addSubscriber.Enabled = false;
            string status = sub_status.Text.Trim();
            string name = sub_name.Text.Trim();
            string email = sub_email.Text.Trim();

            if (string.IsNullOrEmpty(status) || string.IsNullOrEmpty(name) || string.IsNullOrEmpty(email))
            {
                MessageBox.Show("Please fill in all fields.");
                addSubscriber.Enabled = true;
                return;
            }

            string emailPattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
            if (!Regex.IsMatch(email, emailPattern))
            {
                MessageBox.Show("Please enter a valid email address.");
                addSubscriber.Enabled = true;
                return;
            }

            await addSubscriberToDatabase(status, name, email);
            addSubscriber.Enabled = true;
        }

        private async void updateSubscriber_Click(object sender, EventArgs e)
        {
            updateSubscriber.Enabled = false;
            await updateSubscriberInDatabase();
            updateSubscriber.Enabled = true;
        }

        private async void btnDeleteSubscriber_Click(object sender, EventArgs e)
        {
            btnDeleteSubscriber.Enabled = false;
            await deleteSubscriberInDatabase();
            btnDeleteSubscriber.Enabled = true;
        }

        private void filterSubs_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selectedStatus = filterSubs.SelectedItem.ToString().ToLower();
            dgvsubscribers.DataSource = selectedStatus == "all"
                ? allSubscribers
                : allSubscribers.Where(sub => sub.status?.ToLower() == selectedStatus).ToList();
        }

        private void clearFields_Click(object sender, EventArgs e)
        {
            sub_email.Text = "";
            sub_name.Text = "";
            sub_status.Text = "active";
        }

        private async void Save_Smtp_Settings_Click(object sender, EventArgs e)
        {

            var smtpSettings = new
            {
                admin_id = currentAdminId,
                smtp_email = smtp_email.Text.Trim(),
                smtp_password = smtp_password.Text.Trim(),
                smtp_server = smtp_server.Text.Trim(),
                smtp_port = smtp_port.Text.Trim(),
                smtp_ssl = smtp_ssl.Checked
            };

            string json = JsonSerializer.Serialize(smtpSettings);
            StringContent content = new StringContent(json, Encoding.UTF8, "application/json");
            HttpResponseMessage response = await client.PostAsync("http://localhost:5000/smtp/save", content);
            string responseContent = await response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode)
            {
                MessageBox.Show(responseContent);
            }
            else
            {
                MessageBox.Show(responseContent);
            }
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


        private void addCampaign_Click(object sender, EventArgs e)
        {
            panelCampaignEditor.Visible = !panelCampaignEditor.Visible;
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

        private void textBox1_TextChanged(object sender, EventArgs e) { }

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




        private async void saveCampaign_Click(object sender, EventArgs e)
        {
            saveCampaign.Enabled = false;
            string campaignName = txtCampaignName.Text.Trim();
            string contentText = campaignContent.Text.Trim();

            if (string.IsNullOrEmpty(campaignName) || string.IsNullOrEmpty(contentText))
            {
                MessageBox.Show("Please enter both campaign name and content.");
                return;
            }

            var newCampaign = new
            {
                admin_id = currentAdminId,
                name = campaignName,
                content = contentText
            };


            string json = JsonSerializer.Serialize(newCampaign);
            StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

            try
            {
                HttpResponseMessage response = await client.PostAsync("http://localhost:5000/campaigns/add", content);

                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show("Campaign saved successfully!");
                    await LoadCampaignsAsync();
                    txtCampaignName.Text = "";
                    campaignContent.Text = "";
                }
                else
                {
                    MessageBox.Show("Failed to save campaign: " + response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error occurred while saving campaign: " + ex.Message);
            }
            finally
            {
                saveCampaign.Enabled = false;
            }
        }


        private async Task deleteCampaignInDatabase()
        {
            if (currentCampaignId == -1)
            {
                MessageBox.Show("Select a campaign to delete.");
                return;
            }

            var confirmResult = MessageBox.Show("Are you sure you want to delete this campaign?", "Confirm Delete", MessageBoxButtons.YesNo);
            if (confirmResult == DialogResult.Yes)
            {

                HttpResponseMessage response = await client.DeleteAsync($"http://localhost:5000/campaigns/delete/{currentCampaignId}");
                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show("Campaign deleted successfully");
                    await LoadCampaignsAsync(); // Your method to refresh campaigns_table
                    currentCampaignId = -1;
                }
                else
                {
                    MessageBox.Show("Failed to delete campaign: " + response.StatusCode);
                }
            }
        }

        private void campaigns_table_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            DataGridViewRow row = campaigns_table.Rows[e.RowIndex];
            if (row.Cells["id"].Value != null && int.TryParse(row.Cells["id"].Value.ToString(), out int campaignId))
            {
                currentCampaignId = campaignId;

            }
            else
            {
                MessageBox.Show("Invalid campaign ID selected.");
            }
        }
        private async void deletecampaign_Click(object sender, EventArgs e)
        {
            await deleteCampaignInDatabase();
        }
        private async Task LoadSmtpSettingsAsync(int currentAdminId)
        {
            try
            {
                HttpResponseMessage response = await client.GetAsync($"http://localhost:5000/smtp/get/{currentAdminId}");
                string responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var smtpSettings = JsonSerializer.Deserialize<SmtpSettings>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (smtpSettings != null)
                    {
                        // Fill your form inputs
                        smtp_email.Text = smtpSettings.smtp_email ?? "";
                        smtp_password.Text = smtpSettings.smtp_password ?? "";
                        smtp_server.Text = smtpSettings.smtp_server ?? "";
                        smtp_port.Text = smtpSettings.smtp_port.ToString();
                        smtp_ssl.Checked = smtpSettings.smtp_ssl;
                    }
                }
                else
                {
                    MessageBox.Show(responseContent);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Exception: " + ex.Message);
            }
        }

        private async void SendCampaignButton_Click(object sender, EventArgs e)
        {
            SendCampaignButton.Enabled = false;

            if (currentAdminId == -1)
            {
                MessageBox.Show("Please login first.");
                SendCampaignButton.Enabled = true;
                return;
            }

            if (allSubscribers == null || allSubscribers.Count == 0)
            {
                MessageBox.Show("No subscribers found.");
                SendCampaignButton.Enabled = true;
                return;
            }
            if (UserCampaigns.SelectedItem == null)
            {
                MessageBox.Show("Please select a campaign to send.");
                SendCampaignButton.Enabled = true;
                return;
            }

            Campaign selectedCampaign = (Campaign)UserCampaigns.SelectedItem;

            if (target_audience.SelectedItem == null)
            {
                MessageBox.Show("Please select a target audience first.");
                SendCampaignButton.Enabled = true;
                return;
            }

            string selectedStatus = target_audience.SelectedItem.ToString().ToLower();

            // Filter subscribers based on selected status
            var targetSubscribers = selectedStatus == "all"
                ? allSubscribers.ToList()
                : allSubscribers.Where(sub => sub.status?.ToLower() == selectedStatus).ToList();

            if (targetSubscribers.Count == 0)
            {
                MessageBox.Show($"No subscribers found with status: {selectedStatus}");
                SendCampaignButton.Enabled = true;
                return;
            }

            var confirmResult = MessageBox.Show(
                $"Send campaign to {targetSubscribers.Count} subscribers with status '{selectedStatus}'?",
                "Confirm Send",
                MessageBoxButtons.YesNo);

            if (confirmResult != DialogResult.Yes)
            {
                SendCampaignButton.Enabled = true;
                return;
            }

            var activeSubscribers = targetSubscribers;

            // Setup progress bar
            SentProgress.Minimum = 0;
            SentProgress.Maximum = activeSubscribers.Count;
            SentProgress.Value = 0;

            int successCount = 0;
            int failureCount = 0;

            // Send emails to each subscriber
            foreach (var subscriber in activeSubscribers)
            {
                try
                {
                    Currently_Sending_Email.Text = subscriber.email; ;

                    // Refresh the UI
                    Application.DoEvents();

                    // Prepare email data
                    var emailData = new
                    {
                        admin_id = currentAdminId,
                        subject = selectedCampaign.campaign_name,
                        body = selectedCampaign.content,
                        subscriber_id = subscriber.id,
                        campaign_id = selectedCampaign.id,
                    };

                    // Send email via API
                    string json = JsonSerializer.Serialize(emailData);
                    StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

                    HttpResponseMessage response = await client.PostAsync($"http://localhost:5000/smtp/send/{subscriber.email}", content);

                    if (response.IsSuccessStatusCode)
                    {
                        successCount++;
                    }
                    else
                    {
                        failureCount++;
                        string errorResponse = await response.Content.ReadAsStringAsync();
                        Console.WriteLine($"Failed to send to {subscriber.email}: {errorResponse}");
                    }
                }
                catch (Exception ex)
                {
                    failureCount++;
                    Console.WriteLine($"Exception sending to {subscriber.email}: {ex.Message}");
                }
                SentProgress.Value++;

                // Small delay to prevent overwhelming the server
                await Task.Delay(100);
            }

            Currently_Sending_Email.Text = "Done!";

            // Show completion message
            MessageBox.Show($"Campaign sent!\nSuccessful: {successCount}\nFailed: {failureCount}");

            // Re-enable the button
            SendCampaignButton.Enabled = true;
            await LoadEmailLogsAsync();
        }
        private async Task LoadEmailLogsAsync()
        {
            if (currentAdminId == -1) return;

            try
            {
                HttpResponseMessage response = await client.GetAsync($"http://localhost:5000/smtp/email-logs/{currentAdminId}");

                if (response.IsSuccessStatusCode)
                {
                    string json = await response.Content.ReadAsStringAsync();
                    var emailLogs = JsonSerializer.Deserialize<List<EmailLog>>(json, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    EmailLogs.DataSource = emailLogs;
                }
                else
                {
                    MessageBox.Show($"Failed to load email logs: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading email logs: {ex.Message}");
            }
        }

    }
}


