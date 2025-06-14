using MetroSet_UI.Forms;
using System.Text;
using System.Text.Json;
using GARD.Models;

namespace GARD
{
    public partial class Form1: MetroSetForm
    {
        List<Campaign> allCampaigns = new List<Campaign>();
        private int currentCampaignId = -1;
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


        private async void saveCampaign_Click(object sender, EventArgs e)
        {
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
        private async void deletecampaign_Click(object sender, EventArgs e)
        {
            await deleteCampaignInDatabase();
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
        private void addCampaign_Click(object sender, EventArgs e)
        {
            panelCampaignEditor.Visible = !panelCampaignEditor.Visible;
        }

    }
}
