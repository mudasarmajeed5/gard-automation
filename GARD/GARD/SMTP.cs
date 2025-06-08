using GARD.Models;
using MetroSet_UI.Forms;
using System.Text;
using System.Text.Json;
namespace GARD
{
    public partial class Form1: MetroSetForm
    {
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




    }

}