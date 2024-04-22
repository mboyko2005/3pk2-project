using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations; // Добавляем пространство имен для использования атрибутов валидации

namespace Резюме
{
	public class ResumeData
	{
		[Required(ErrorMessage = "Поле 'ФИО' обязательно для заполнения")]
		public string FullName { get; set; }

		[Required(ErrorMessage = "Поле 'Дата рождения' обязательно для заполнения")]
		[DataType(DataType.Date, ErrorMessage = "Некорректный формат даты")]
		[BirthDateRange(ErrorMessage = "Дата рождения должна быть в пределах разумного")]
		public DateTime? BirthDate { get; set; }

		public string Education { get; set; }
		public string HardSkills { get; set; }
		public string SoftSkills { get; set; }
		public string DesiredSchedule { get; set; }

		[Required(ErrorMessage = "Поле 'Желаемая зарплата' обязательно для заполнения")]
		[RegularExpression(@"^\d+$", ErrorMessage = "Желаемая зарплата должна быть числом")]
		public string DesiredSalary { get; set; }
		public List<WorkExperience> WorkExperiences { get; set; }
	}

	// Создаем собственный атрибут для проверки даты рождения
	public class BirthDateRangeAttribute : ValidationAttribute
	{
		protected override ValidationResult IsValid(object value, ValidationContext validationContext)
		{
			var birthDate = (DateTime)value;
			var minDate = DateTime.Now.AddYears(-100);
			var maxDate = DateTime.Now.AddYears(-16); // Примерно 16 лет является разумным возрастом для работы

			if (birthDate < minDate || birthDate > maxDate)
			{
				return new ValidationResult(ErrorMessage);
			}

			return ValidationResult.Success;
		}
	}
}
