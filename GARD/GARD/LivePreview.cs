using FastColoredTextBoxNS;
using System.Text.Json;
using System.Text;

namespace GARD
{
    public partial class Form1 : MetroSet_UI.Forms.MetroSetForm
    {
        private FastColoredTextBox htmlEditor;
        private WebBrowser previewBrowser;


        private void SetupEditorPanels()
        {
            // Initialize FastColoredTextBox for HTML editing
            htmlEditor = new FastColoredTextBox
            {
                Name = "htmlEditor",
                Language = Language.HTML,
                AutoIndent = true,
                Dock = DockStyle.Fill,
                ReadOnly = false,
                Enabled = true,
                BackColor = System.Drawing.Color.White,
            };

            htmlEditor.KeyDown += (s, e) =>
            {
                if (e.Control && e.KeyCode == Keys.C)
                {
                    if (htmlEditor.SelectionLength > 0)
                    {
                        Clipboard.SetText(htmlEditor.SelectedText);
                        e.Handled = true;
                    }
                }
            };

            previewBrowser = new WebBrowser
            {
                Name = "previewBrowser",
                Dock = DockStyle.Fill
            };

            // Add controls to the appropriate panels of the SplitContainer
            editorPanel.Panel1.Controls.Add(htmlEditor);
            editorPanel.Panel2.Controls.Add(previewBrowser);

            // Set initial visibility
            copy_html.Visible = false;

            // Event hookup
            htmlEditor.TextChanged += (s, e) =>
            {
                previewBrowser.DocumentText = htmlEditor.Text;
            };
            generateButton.Click += async (s, e) => await GenerateEmailHtmlAsync();
        }

        private async Task GenerateEmailHtmlAsync()
        {
            string userPrompt = promptBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(userPrompt))
            {
                MessageBox.Show("Please enter a prompt.");
                return;
            }

            generateButton.Enabled = false;
            generateButton.Visible = false;
            copy_html.Visible = false;
            loadingLabel.Visible = true;

            try
            {
                string divHtml = await GetGeminiDivHtmlAsync(userPrompt);
                htmlEditor.Text = divHtml;
                previewBrowser.DocumentText = divHtml;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
            finally
            {
                loadingLabel.Visible = false;
                generateButton.Enabled = true;
                copy_html.Visible = true;

                if (!string.IsNullOrEmpty(htmlEditor.Text))
                {
                    Clipboard.SetText(htmlEditor.Text);
                }
            }
            generateButton.Visible = true;
        }

        private async Task<string> GetGeminiDivHtmlAsync(string userPrompt)
        {
            string apiKey = "AIzaSyCwFXt1s2ew-hY02qk1R_7v_doWB5hFpFY";
            string url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash:generateContent?key={apiKey}";

            string fullPrompt =
                "Generate only the raw inner HTML of a div container with inline styles for an email marketing campaign based on: " +
                userPrompt;
            
            var body = new
            {
                contents = new[]
                {
                    new
                    {
                        parts = new[]
                        {
                            new { text = fullPrompt }
                        }
                    }
                }
            };

            var json = JsonSerializer.Serialize(body);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            using var client = new HttpClient();
            var response = await client.PostAsync(url, content);
            var result = await response.Content.ReadAsStringAsync();

            var parsed = JsonDocument.Parse(result);

            if (!parsed.RootElement.TryGetProperty("candidates", out var candidates) || candidates.GetArrayLength() == 0)
                throw new Exception("No candidates returned from API.");

            var firstCandidate = candidates[0];

            if (!firstCandidate.TryGetProperty("content", out var contentElement))
                throw new Exception("Missing 'content' property.");

            if (!contentElement.TryGetProperty("parts", out var parts) || parts.GetArrayLength() == 0)
                throw new Exception("Missing 'parts' property or empty array.");

            if (!parts[0].TryGetProperty("text", out var textProperty))
                throw new Exception("Missing 'text' property.");

            string rawOutput = textProperty.GetString();

            return CleanOutput(rawOutput);
        }
        private void copy_html_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(htmlEditor.Text))
            {
                Clipboard.SetText(htmlEditor.Text);
                MessageBox.Show("Copied to clipboard!");
            }
        }

        private string CleanOutput(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw))
                return string.Empty;

            raw = raw.Trim();

            if (raw.StartsWith("```"))
            {
                int firstLineEnd = raw.IndexOf('\n');
                int lastBacktick = raw.LastIndexOf("```");
                if (lastBacktick > firstLineEnd && firstLineEnd > 0)
                {
                    return raw.Substring(firstLineEnd + 1, lastBacktick - firstLineEnd - 1).Trim();
                }
            }
            return raw;
        }
    }
}
