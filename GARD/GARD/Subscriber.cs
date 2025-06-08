using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using GARD.Models;
using MetroSet_UI.Forms;

namespace GARD
{
    public partial class Form1 : MetroSetForm
    {
        private int currentAdminId = -1;
        List<Subscriber> allSubscribers = new List<Subscriber>();
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



    }
}
