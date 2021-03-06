using System;
using System.Collections.Generic;
using System.IO;
using Xunit;
using Shouldly;
using Payslip.Model;
using System.Linq;
using CsvHelper;
using Payslip.DataAccess;

namespace Payslip.UnitTests
{
    public sealed class CalculatorTests
    {
        [Fact]
        public void Calculate_EmployeeAnnualSalaryNegative_ReturnsValidationError()
        {
            // arrange
            var validationContext = new ValidationContext();
            const decimal annualSalary = -1.00M;
            var employee = new Employee(BuildRandomString(), BuildRandomString(), annualSalary, 0M, new PaymentPeriod(new DateTime(2018, 3, 1), new DateTime(2018, 3, 31)));
            // act
            BuildSutAndValidate(employee, validationContext);
            // assert
            validationContext.ValidationErrors.ShouldContain($"{nameof(annualSalary)} must not be a negative decimal, was: {annualSalary}");
        }

        private void BuildSutAndValidate(Employee employee, ValidationContext validationContext)
        {
            BuildCalculator().Calculate(
                new[] {employee},
                validationContext
            );
        }

        [Fact]
        public void Calculate_EmployeeAnnualSalaryNotInteger_ReturnsValidationError()
        {
            // arrange
            var validationContext = new ValidationContext();
            const decimal annualSalary = 1.01M;
            var employee = new Employee(BuildRandomString(), BuildRandomString(), annualSalary, 0M, new PaymentPeriod(new DateTime(2018, 3, 1), new DateTime(2018, 3, 31)));
            // act
            BuildSutAndValidate(employee, validationContext);
            // assert
            validationContext.ValidationErrors.ShouldContain($"{nameof(annualSalary)} must be a whole number, was: {annualSalary}");
        }

        [Fact]
        public void Ctor_TaxRatesNotInitialised_ThrowsArgumentException(){
            // arrange, act & assert
            Assert.Throws<ArgumentException>( () => new Calculator(new TaxRate[0]) );
        }

        [Fact]
        public void Calculate_MultiplePaymentPeriods_ReturnsPayslipForEachPeriod()
        {
            // arrange
            var validationContext = new ValidationContext();
            var input = new[] {
                new Employee{
                    FirstName = BuildRandomString(),
                    LastName = BuildRandomString(),
                    AnnualSalary = 60050M,
                    PaymentStartDate = new PaymentPeriod
                    {
                        StartDate = new DateTime(2018,03,01),
                        EndDate = new DateTime(2018, 03, 31),
                    },
                    SuperRate  = 0.09M,
                },
                new Employee{
                    FirstName = BuildRandomString(),
                    LastName = BuildRandomString(),
                    AnnualSalary = 60050M,
                    PaymentStartDate = new PaymentPeriod
                    {
                        StartDate = new DateTime(2018,03,01),
                        EndDate = new DateTime(2018, 06, 30),
                    },
                    SuperRate  = 0.09M
                }
            };
            // act
            var result = BuildCalculator().Calculate(input, validationContext);
            // assert 
            var payslips = result.ToArray();
            foreach (var payslip in payslips)
            {
                payslip.GrossIncome.ShouldBe(5004);
                payslip.IncomeTax.ShouldBe(922M);
                payslip.NetIncome.ShouldBe(4082M);
                payslip.Super.ShouldBe(450);
            }
            payslips.Count().ShouldBe(5);

            validationContext.IsValid.ShouldBeTrue();
        }

        private static string BuildRandomString()
        {
            return Guid.NewGuid()+"";
        }

        private Calculator BuildCalculator()
        {
            return new Calculator(Constants.TaxRates);
        }
    }
}
