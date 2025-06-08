using OxyPlot;
using OxyPlot.Series;
using OxyPlot.WindowsForms;
using OxyPlot.Axes;

public class ChartManager
{
    public static void CreateCharts(TabControl tabControl)
    {
        var visualMetricsTab = new TabPage
        {
            Text = "Visual Metrics"
        };

        var tableLayoutPanel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            RowCount = 1,
            ColumnCount = 2,
            AutoSize = true,
            CellBorderStyle = TableLayoutPanelCellBorderStyle.None,
        };

        // Use fixed size columns so charts don't take full width
        tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));

        CreateDonutChart(tableLayoutPanel);
        CreateBarChart(tableLayoutPanel);

        visualMetricsTab.Controls.Add(tableLayoutPanel);
        tabControl.TabPages.Add(visualMetricsTab);
    }

    private static void CreateDonutChart(TableLayoutPanel layout)
    {
        var plotModel = new PlotModel { Title = "Emails Sent vs Opened vs Failures" };

        var pieSeries = new PieSeries
        {
            InsideLabelPosition = 0.7,
            AngleSpan = 360,
            StartAngle = 0,
            InnerDiameter = 0.4
        };

        pieSeries.Slices.Add(new PieSlice("Opened Emails", 40) { IsExploded = true, Fill = OxyColors.LightGreen });
        pieSeries.Slices.Add(new PieSlice("Emails Sent", 50) { Fill = OxyColors.LightBlue });
        pieSeries.Slices.Add(new PieSlice("Email Failures", 10) { Fill = OxyColors.LightCoral });

        plotModel.Series.Add(pieSeries);

        var plotView = new PlotView
        {
            Model = plotModel,
            Size = new Size(400, 400),
            Anchor = AnchorStyles.None,
            Margin = new Padding(0)
        };

        var containerPanel = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(0),
            AutoSize = false
        };

        // Manual center the chart
        plotView.Location = new Point(
            (containerPanel.Width - plotView.Width) / 2,
            (containerPanel.Height - plotView.Height) / 2
        );
        plotView.Anchor = AnchorStyles.Top;

        // Let the panel handle resize and keep centering
        containerPanel.Resize += (s, e) =>
        {
            plotView.Location = new Point(
                (containerPanel.Width - plotView.Width) / 2,
                (containerPanel.Height - plotView.Height) / 2
            );
        };

        containerPanel.Controls.Add(plotView);
        layout.Controls.Add(containerPanel, 0, 0);
    }

    private static void CreateBarChart(TableLayoutPanel layout)
    {
        var plotModelBar = new PlotModel { Title = "Emails Sent - Bar Chart" };

        var barSeries = new BarSeries
        {
            Title = "Emails Sent",
            LabelPlacement = LabelPlacement.Inside,
            LabelFormatString = "{0}",
            StrokeColor = OxyColors.Black,
            StrokeThickness = 1
        };

        // Consistent color scheme (green for opened, blue for sent, red for failures — we just use green here)
        barSeries.FillColor = OxyColors.LightGreen;

        // Sample data (replace with real stats later)
        barSeries.Items.Add(new BarItem(100));
        barSeries.Items.Add(new BarItem(150));
        barSeries.Items.Add(new BarItem(200));
        barSeries.Items.Add(new BarItem(250));
        barSeries.Items.Add(new BarItem(300));
        barSeries.Items.Add(new BarItem(350));
        barSeries.Items.Add(new BarItem(400));

        plotModelBar.Series.Add(barSeries);

        // Add axis with day labels
        plotModelBar.Axes.Add(new CategoryAxis
        {
            Position = AxisPosition.Left,
            Key = "DayAxis",
            ItemsSource = new[] { "7d", "6d", "5d", "4d", "3d", "2d", "1d" },
            IsTickCentered = true,
            TextColor = OxyColors.Gray,
            AxislineColor = OxyColors.LightGray
        });

        // Match the Y-axis to the barSeries
        barSeries.YAxisKey = "DayAxis";

        var plotViewBar = new PlotView
        {
            Model = plotModelBar,
            Size = new Size(400, 400),
            Anchor = AnchorStyles.None,
            Margin = new Padding(0)
        };

        var containerPanel = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(0),
            AutoSize = false
        };

        // Center the bar chart in the panel
        plotViewBar.Location = new Point(
            (containerPanel.Width - plotViewBar.Width) / 2,
            (containerPanel.Height - plotViewBar.Height) / 2
        );
        plotViewBar.Anchor = AnchorStyles.Top;

        containerPanel.Resize += (s, e) =>
        {
            plotViewBar.Location = new Point(
                (containerPanel.Width - plotViewBar.Width) / 2,
                (containerPanel.Height - plotViewBar.Height) / 2
            );
        };

        containerPanel.Controls.Add(plotViewBar);
        layout.Controls.Add(containerPanel, 1, 0);
    }


}