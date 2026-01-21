namespace MyAppMVC.ViewModels
{
    public class ExportReportViewModel
    {
        public string ReportType { get; set; } = string.Empty;
        public string Format { get; set; } = "pdf";
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool IncludeCharts { get; set; } = true;
    }
}
