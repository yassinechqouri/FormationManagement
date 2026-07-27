using ClosedXML.Excel;
using FormationManagement.Application.DTOs.Course;
using FormationManagement.Application.DTOs.Enrollment;
using FormationManagement.Application.Interfaces;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace FormationManagement.Web.Services;

/// <summary>
/// Turns admin report data into downloadable Excel/PDF files. Kept in the Web
/// project (rather than Application) since it depends on ClosedXML/QuestPDF,
/// which are presentation-format concerns, not business logic.
/// </summary>
public class ExportService
{
    // ---------------- Excel ----------------

    public byte[] BuildUsersExcel(IEnumerable<UserSummaryDto> users)
    {
        using var workbook = new XLWorkbook();
        var sheet = workbook.Worksheets.Add("Users");

        sheet.Cell(1, 1).Value = "Full Name";
        sheet.Cell(1, 2).Value = "Email";
        sheet.Cell(1, 3).Value = "Role(s)";
        sheet.Cell(1, 4).Value = "Created At";
        sheet.Row(1).Style.Font.Bold = true;

        var row = 2;
        foreach (var user in users)
        {
            sheet.Cell(row, 1).Value = user.FullName;
            sheet.Cell(row, 2).Value = user.Email;
            sheet.Cell(row, 3).Value = string.Join(", ", user.Roles);
            sheet.Cell(row, 4).Value = user.CreatedAt.ToString("yyyy-MM-dd");
            row++;
        }

        sheet.Columns().AdjustToContents();
        return ToBytes(workbook);
    }

    public byte[] BuildCoursesExcel(IEnumerable<CourseDto> courses)
    {
        using var workbook = new XLWorkbook();
        var sheet = workbook.Worksheets.Add("Courses");

        string[] headers = { "Title", "Category", "Trainer", "Level", "Price", "Duration (min)", "Published", "Enrollments" };
        for (var i = 0; i < headers.Length; i++)
            sheet.Cell(1, i + 1).Value = headers[i];
        sheet.Row(1).Style.Font.Bold = true;

        var row = 2;
        foreach (var course in courses)
        {
            sheet.Cell(row, 1).Value = course.Title;
            sheet.Cell(row, 2).Value = course.CategoryName;
            sheet.Cell(row, 3).Value = course.TrainerName;
            sheet.Cell(row, 4).Value = course.Level.ToString();
            sheet.Cell(row, 5).Value = course.Price;
            sheet.Cell(row, 6).Value = course.Duration;
            sheet.Cell(row, 7).Value = course.Published ? "Yes" : "No";
            sheet.Cell(row, 8).Value = course.EnrollmentCount;
            row++;
        }

        sheet.Columns().AdjustToContents();
        return ToBytes(workbook);
    }

    public byte[] BuildEnrollmentsExcel(IEnumerable<EnrollmentDto> enrollments)
    {
        using var workbook = new XLWorkbook();
        var sheet = workbook.Worksheets.Add("Enrollments");

        string[] headers = { "Learner", "Email", "Course", "Enrollment Date", "Progress (%)" };
        for (var i = 0; i < headers.Length; i++)
            sheet.Cell(1, i + 1).Value = headers[i];
        sheet.Row(1).Style.Font.Bold = true;

        var row = 2;
        foreach (var e in enrollments)
        {
            sheet.Cell(row, 1).Value = e.LearnerName;
            sheet.Cell(row, 2).Value = e.LearnerEmail;
            sheet.Cell(row, 3).Value = e.CourseTitle;
            sheet.Cell(row, 4).Value = e.EnrollmentDate.ToString("yyyy-MM-dd");
            sheet.Cell(row, 5).Value = e.Progress;
            row++;
        }

        sheet.Columns().AdjustToContents();
        return ToBytes(workbook);
    }

    private static byte[] ToBytes(XLWorkbook workbook)
    {
        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    // ---------------- PDF ----------------

    public byte[] BuildUsersPdf(IEnumerable<UserSummaryDto> users) =>
        BuildTablePdf("Users Report", new[] { "Full Name", "Email", "Role(s)", "Created At" },
            users.Select(u => new[] { u.FullName, u.Email, string.Join(", ", u.Roles), u.CreatedAt.ToString("yyyy-MM-dd") }));

    public byte[] BuildCoursesPdf(IEnumerable<CourseDto> courses) =>
        BuildTablePdf("Courses Report", new[] { "Title", "Category", "Trainer", "Level", "Price", "Published", "Enrollments" },
            courses.Select(c => new[] { c.Title, c.CategoryName, c.TrainerName, c.Level.ToString(), c.Price.ToString("C"), c.Published ? "Yes" : "No", c.EnrollmentCount.ToString() }));

    public byte[] BuildEnrollmentsPdf(IEnumerable<EnrollmentDto> enrollments) =>
        BuildTablePdf("Enrollments Report", new[] { "Learner", "Course", "Enrollment Date", "Progress" },
            enrollments.Select(e => new[] { e.LearnerName, e.CourseTitle, e.EnrollmentDate.ToString("yyyy-MM-dd"), $"{e.Progress}%" }));

    private static byte[] BuildTablePdf(string title, string[] headers, IEnumerable<string[]> rows)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(30);
                page.DefaultTextStyle(x => x.FontSize(10));

                page.Header().Text(title).FontSize(18).Bold();

                page.Content().PaddingTop(15).Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        foreach (var _ in headers)
                            columns.RelativeColumn();
                    });

                    table.Header(header =>
                    {
                        foreach (var h in headers)
                            header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text(h).Bold();
                    });

                    foreach (var row in rows)
                    {
                        foreach (var cell in row)
                            table.Cell().Padding(5).Text(cell);
                    }
                });

                page.Footer().AlignCenter().Text(x =>
                {
                    x.Span("Generated on ");
                    x.Span(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm")).Bold();
                    x.Span(" UTC — Formation Management System");
                });
            });
        });

        return document.GeneratePdf();
    }
}
