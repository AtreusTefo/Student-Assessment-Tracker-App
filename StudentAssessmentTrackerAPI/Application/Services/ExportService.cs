using CsvHelper;
using CsvHelper.Configuration;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using StudentAssessmentTracker.Application.DTOs;
using StudentAssessmentTracker.Domain.Interfaces;
using System.Globalization;

namespace StudentAssessmentTracker.Application.Services
{
    /// <summary>Contract for exporting student report data to CSV or PDF.</summary>
    public interface IExportService
    {
        /// <summary>Returns a CSV byte array containing all students for the given teacher.</summary>
        Task<byte[]> ExportStudentsToCsvAsync(int teacherId);

        /// <summary>Returns a CSV byte array for a single student's assessment report.</summary>
        Task<byte[]> ExportStudentReportToCsvAsync(int studentId, int teacherId);

        /// <summary>Returns a PDF byte array for a single student's assessment report.</summary>
        Task<byte[]> ExportStudentReportToPdfAsync(int studentId, int teacherId);
    }

    /// <summary>Generates CSV and PDF export files for student reports.</summary>
    public class ExportService : IExportService
    {
        private readonly IStudentService _studentService;

        /// <summary>Initialises the service.</summary>
        public ExportService(IStudentService studentService)
        {
            _studentService = studentService;
            // Community licence — free for non-commercial and open-source use
            QuestPDF.Settings.License = LicenseType.Community;
        }

        /// <inheritdoc />
        public async Task<byte[]> ExportStudentsToCsvAsync(int teacherId)
        {
            var students = (await _studentService.GetAllStudentsAsync(teacherId)).ToList();

            using var ms = new MemoryStream();
            using var writer = new StreamWriter(ms);
            using var csv = new CsvWriter(writer, new CsvConfiguration(CultureInfo.InvariantCulture));

            csv.WriteHeader<StudentCsvRow>();
            csv.NextRecord();
            foreach (var s in students)
            {
                csv.WriteRecord(new StudentCsvRow
                {
                    StudentUniqueId = s.StudentUniqueId ?? string.Empty,
                    FirstName = s.FirstName ?? string.Empty,
                    LastName = s.LastName ?? string.Empty,
                    Email = s.Email ?? string.Empty,
                    Phone = s.Phone ?? string.Empty,
                    Grade = s.GradeName ?? string.Empty,
                    TotalScore = s.TotalScore,
                    MaxPossible = s.MaxPossible,
                    Percentage = s.Percentage,
                    PerformanceLevel = s.PerformanceLevel ?? string.Empty,
                    AssessmentCount = s.Assessments.Count()
                });
                csv.NextRecord();
            }
            await writer.FlushAsync();
            return ms.ToArray();
        }

        /// <inheritdoc />
        public async Task<byte[]> ExportStudentReportToCsvAsync(int studentId, int teacherId)
        {
            var student = await _studentService.GetStudentByIdAsync(studentId, teacherId);

            using var ms = new MemoryStream();
            using var writer = new StreamWriter(ms);
            using var csv = new CsvWriter(writer, new CsvConfiguration(CultureInfo.InvariantCulture));

            csv.WriteHeader<AssessmentCsvRow>();
            csv.NextRecord();
            foreach (var a in student.Assessments)
            {
                csv.WriteRecord(new AssessmentCsvRow
                {
                    AssessmentName = a.Name ?? string.Empty,
                    Score = a.Score,
                    MaxScore = a.MaxScore,
                    Percentage = a.MaxScore > 0 ? Math.Round((a.Score / a.MaxScore) * 100, 2) : 0,
                    DueDate = a.DueDate?.ToString("yyyy-MM-dd") ?? string.Empty,
                    IsAssigned = a.IsAssigned,
                    SubmissionCount = a.SubmissionCount
                });
                csv.NextRecord();
            }
            await writer.FlushAsync();
            return ms.ToArray();
        }

        /// <inheritdoc />
        public async Task<byte[]> ExportStudentReportToPdfAsync(int studentId, int teacherId)
        {
            var student = await _studentService.GetStudentByIdAsync(studentId, teacherId);
            var assessments = student.Assessments.ToList();

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(30, Unit.Point);
                    page.DefaultTextStyle(x => x.FontSize(10));

                    page.Header().Element(ComposeHeader);
                    page.Content().Element(c => ComposeContent(c, student, assessments));
                    page.Footer().AlignCenter().Text(text =>
                    {
                        text.Span("Generated on ").FontSize(8).FontColor(Colors.Grey.Medium);
                        text.Span(DateTime.UtcNow.ToString("dd MMM yyyy HH:mm") + " UTC").FontSize(8).FontColor(Colors.Grey.Medium);
                    });
                });
            });

            return document.GeneratePdf();
        }

        private static void ComposeHeader(IContainer container)
        {
            container.Column(col =>
            {
                col.Item().Text("Student Assessment Tracker")
                    .Bold().FontSize(18).FontColor(Colors.Blue.Darken2);
                col.Item().Text("Student Performance Report")
                    .FontSize(12).FontColor(Colors.Grey.Darken1);
                col.Item().PaddingTop(4).LineHorizontal(1).LineColor(Colors.Blue.Lighten2);
            });
        }

        private static void ComposeContent(IContainer container, StudentDto student, List<StudentAssessmentDto> assessments)
        {
            container.Column(col =>
            {
                col.Spacing(8);

                // Student info
                col.Item().PaddingTop(8).Table(t =>
                {
                    t.ColumnsDefinition(c => { c.RelativeColumn(1); c.RelativeColumn(2); });

                    void InfoRow(string label, string value)
                    {
                        t.Cell().Text(label).SemiBold();
                        t.Cell().Text(value);
                    }

                    InfoRow("Student ID:", student.StudentUniqueId ?? "-");
                    InfoRow("Full Name:", $"{student.FirstName} {student.LastName}");
                    InfoRow("Email:", student.Email ?? "-");
                    InfoRow("Grade:", student.GradeName ?? "-");
                    InfoRow("Performance Level:", student.PerformanceLevel ?? "-");
                    InfoRow("Overall Percentage:", $"{student.Percentage:F1}%");
                    InfoRow("Total Score:", $"{student.TotalScore} / {student.MaxPossible}");
                });

                col.Item().PaddingTop(4).LineHorizontal(0.5f).LineColor(Colors.Grey.Lighten2);

                // Assessments table
                col.Item().Text("Assessments").Bold().FontSize(12);

                if (!assessments.Any())
                {
                    col.Item().Text("No assessments recorded.").Italic().FontColor(Colors.Grey.Medium);
                }
                else
                {
                    col.Item().Table(t =>
                    {
                        t.ColumnsDefinition(c =>
                        {
                            c.RelativeColumn(3);
                            c.RelativeColumn(1);
                            c.RelativeColumn(1);
                            c.RelativeColumn(1);
                            c.RelativeColumn(2);
                        });

                        // Header
                        void HeaderCell(string text) =>
                            t.Cell().Background(Colors.Blue.Lighten4).Padding(4)
                             .Text(text).Bold().FontSize(9);

                        HeaderCell("Assessment");
                        HeaderCell("Score");
                        HeaderCell("Max");
                        HeaderCell("%");
                        HeaderCell("Due Date");

                        foreach (var a in assessments)
                        {
                            var pct = a.MaxScore > 0 ? Math.Round((a.Score / a.MaxScore) * 100, 1) : 0;
                            var bgColor = pct >= 75 ? Colors.Green.Lighten5
                                        : pct >= 50 ? Colors.Yellow.Lighten5
                                        : Colors.Red.Lighten5;

                            void DataCell(string text) =>
                                t.Cell().Background(bgColor).Padding(3).Text(text).FontSize(9);

                            DataCell(a.Name ?? string.Empty);
                            DataCell(a.Score.ToString("F1"));
                            DataCell(a.MaxScore.ToString("F1"));
                            DataCell($"{pct:F1}%");
                            DataCell(a.DueDate?.ToString("dd MMM yyyy") ?? "-");
                        }
                    });
                }
            });
        }

        // ── CSV row models ──────────────────────────────────────────────────

        private sealed class StudentCsvRow
        {
            public string StudentUniqueId { get; set; } = string.Empty;
            public string FirstName { get; set; } = string.Empty;
            public string LastName { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
            public string Phone { get; set; } = string.Empty;
            public string Grade { get; set; } = string.Empty;
            public decimal TotalScore { get; set; }
            public decimal MaxPossible { get; set; }
            public decimal Percentage { get; set; }
            public string PerformanceLevel { get; set; } = string.Empty;
            public int AssessmentCount { get; set; }
        }

        private sealed class AssessmentCsvRow
        {
            public string AssessmentName { get; set; } = string.Empty;
            public decimal Score { get; set; }
            public decimal MaxScore { get; set; }
            public decimal Percentage { get; set; }
            public string DueDate { get; set; } = string.Empty;
            public bool IsAssigned { get; set; }
            public int SubmissionCount { get; set; }
        }
    }
}
