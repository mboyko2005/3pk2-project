using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using Newtonsoft.Json;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using System.Windows.Controls;

namespace Резюме
{
	public partial class MainWindow : Window
	{
		private List<WorkExperience> workExperiences = new List<WorkExperience>();

		public MainWindow()
		{
			InitializeComponent();
			FullNameTextBox.TextChanged += FullNameTextBox_TextChanged;
			BirthDatePicker.SelectedDateChanged += BirthDatePicker_SelectedDateChanged;
			SalaryTextBox.TextChanged += SalaryTextBox_TextChanged;
		}

		private void FullNameTextBox_TextChanged(object sender, TextChangedEventArgs e)
		{
			string text = FullNameTextBox.Text;
			int spaceCount = text.Count(char.IsWhiteSpace);
			if (spaceCount > 2)
			{
				FullNameTextBox.Text = Regex.Replace(text, @"\s+", " ");
				FullNameTextBox.CaretIndex = FullNameTextBox.Text.Length;
			}
			if (text.Any(char.IsDigit))
			{
				FullNameTextBox.Text = Regex.Replace(text, @"\d", "");
				FullNameTextBox.CaretIndex = FullNameTextBox.Text.Length;
			}
		}

		private void BirthDatePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
		{
			DateTime? selectedDate = BirthDatePicker.SelectedDate;
			if (selectedDate.HasValue && selectedDate.Value.ToString("dd.MM.yy").StartsWith("01.10.20"))
			{
				BirthDatePicker.SelectedDate = new DateTime(2025, 10, 1);
			}
		}

		private void SalaryTextBox_TextChanged(object sender, TextChangedEventArgs e)
		{
			string text = SalaryTextBox.Text;
			if (!string.IsNullOrWhiteSpace(text))
			{
				SalaryTextBox.Text = Regex.Replace(text, @"[^\d]", "");
				SalaryTextBox.CaretIndex = SalaryTextBox.Text.Length;
			}
		}

		private void ExportButton_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				SaveToJson();
				FillDocx();
				MessageBox.Show("Данные сохранены и экспортированы.");
			}
			catch (Exception ex)
			{
				MessageBox.Show($"Произошла ошибка: {ex.Message}");
			}
		}

		private void SaveToJson()
		{
			var resumeData = new
			{
				FullName = FullNameTextBox.Text,
				BirthDate = BirthDatePicker.SelectedDate?.ToString("dd.MM.yyyy"),
				Education = EducationTextBox.Text,
				HardSkills = HardSkillsTextBox.Text,
				SoftSkills = SoftSkillsTextBox.Text,
				DesiredSchedule = ScheduleComboBox.Text,
				DesiredSalary = SalaryTextBox.Text,
				WorkExperiences = workExperiences
			};

			string json = JsonConvert.SerializeObject(resumeData, Newtonsoft.Json.Formatting.Indented);
			File.WriteAllText("resume.json", json);
		}

		private void FillDocx()
		{
			string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
			string fileName = Path.Combine(desktopPath, $"{FullNameTextBox.Text}_резюме.docx");

			using (WordprocessingDocument doc = WordprocessingDocument.Create(fileName, WordprocessingDocumentType.Document))
			{
				MainDocumentPart mainPart = doc.AddMainDocumentPart();
				mainPart.Document = new Document();
				Body body = mainPart.Document.AppendChild(new Body());

				Paragraph title = CreateParagraph("Резюме");
				title.ParagraphProperties = new ParagraphProperties(new Justification() { Val = JustificationValues.Center });
				title.ParagraphProperties.Append(new Bold());
				title.ParagraphProperties.SpacingBetweenLines = new SpacingBetweenLines() { After = "200" };
				body.AppendChild(title);

				body.AppendChild(CreateHeading("Персональные данные:", 1));
				body.AppendChild(CreateParagraph($"ФИО: {FullNameTextBox.Text}"));
				body.AppendChild(CreateParagraph($"Дата рождения: {(BirthDatePicker.SelectedDate?.ToString("dd.MM.yyyy") ?? "")}"));
				body.AppendChild(CreateParagraph($"Образование: {EducationTextBox.Text}"));
				body.AppendChild(CreateParagraph($"Хардскиллы: {HardSkillsTextBox.Text}"));
				body.AppendChild(CreateParagraph($"Софтскиллы: {SoftSkillsTextBox.Text}"));

				if (workExperiences.Count > 0)
				{
					body.AppendChild(CreateHeading("Места работы:", 1));
					Table workExperienceTable = new Table();
					TableProperties tblProps = new TableProperties(
						new TableBorders(
							new TopBorder { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 6 },
							new BottomBorder { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 6 },
							new LeftBorder { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 6 },
							new RightBorder { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 6 }
						)
					);
					workExperienceTable.AppendChild<TableProperties>(tblProps);
					TableRow headerRow = new TableRow();
					headerRow.Append(
						new TableCell(new Paragraph(new Run(new Text("Название предприятия")))),
						new TableCell(new Paragraph(new Run(new Text("Дата начала")))),
						new TableCell(new Paragraph(new Run(new Text("Дата окончания")))),
						new TableCell(new Paragraph(new Run(new Text("Продолжительность работы"))))
					);
					workExperienceTable.AppendChild(headerRow);
					foreach (var experience in workExperiences)
					{
						TableRow row = new TableRow();
						row.Append(
							new TableCell(new Paragraph(new Run(new Text(experience.Name)))),
							new TableCell(new Paragraph(new Run(new Text(experience.StartDate?.ToString("dd.MM.yyyy") ?? "")))),
							new TableCell(new Paragraph(new Run(new Text(experience.EndDate?.ToString("dd.MM.yyyy") ?? "")))),
							new TableCell(new Paragraph(new Run(new Text(experience.DurationInDays))))
						);
						workExperienceTable.AppendChild(row);
					}
					body.AppendChild(workExperienceTable);
				}
			}
		}

		private Paragraph CreateHeading(string text, int level)
		{
			Paragraph paragraph = new Paragraph(new Run(new Text(text)));
			paragraph.ParagraphProperties = new ParagraphProperties(new ParagraphStyleId() { Val = "Heading" + level });
			return paragraph;
		}

		private Paragraph CreateParagraph(string text)
		{
			Paragraph paragraph = new Paragraph(new Run(new Text(text)));
			paragraph.ParagraphProperties = new ParagraphProperties(new SpacingBetweenLines() { After = "100" });
			return paragraph;
		}

		private void AddExperienceButton_Click(object sender, RoutedEventArgs e)
		{
			WorkExperience experience = new WorkExperience();
			workExperiences.Add(experience);

			StackPanel workExperiencePanel = new StackPanel();
			workExperiencePanel.Margin = new Thickness(5);

			Label nameLabel = new Label();
			nameLabel.Content = "Название предприятия:";
			TextBox nameTextBox = new TextBox();
			nameTextBox.Margin = new Thickness(5);
			workExperiencePanel.Children.Add(nameLabel);
			workExperiencePanel.Children.Add(nameTextBox);

			Label startDateLabel = new Label();
			startDateLabel.Content = "Дата начала работы:";
			DatePicker startDatePicker = new DatePicker();
			startDatePicker.Margin = new Thickness(5);
			workExperiencePanel.Children.Add(startDateLabel);
			workExperiencePanel.Children.Add(startDatePicker);

			Label endDateLabel = new Label();
			endDateLabel.Content = "Дата окончания работы:";
			DatePicker endDatePicker = new DatePicker();
			endDatePicker.Margin = new Thickness(5);
			workExperiencePanel.Children.Add(endDateLabel);
			workExperiencePanel.Children.Add(endDatePicker);

			ExperienceStackPanel.Children.Add(workExperiencePanel);

			Button saveButton = new Button();
			saveButton.Content = "Сохранить";
			saveButton.Click += (s, ev) =>
			{
				experience.Name = nameTextBox.Text;
				experience.StartDate = startDatePicker.SelectedDate;
				experience.EndDate = endDatePicker.SelectedDate;
				ExperienceStackPanel.Children.Remove(workExperiencePanel);
			};
			workExperiencePanel.Children.Add(saveButton);
		}
	}

	public class WorkExperience
	{
		public string Name { get; set; }
		public DateTime? StartDate { get; set; }
		public DateTime? EndDate { get; set; }

		public string DurationInDays
		{
			get
			{
				if (StartDate != null && EndDate != null)
				{
					TimeSpan duration = EndDate.Value - StartDate.Value;
					return duration.Days.ToString();
				}
				return "";
			}
		}
	}
}
