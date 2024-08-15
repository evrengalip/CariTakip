using Business.Abstract;
using Data.Abstract;
using Entities.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Business.Concrete
{
    public class EmployeeService : IEmployeeService
    {
        private readonly IEmployeeRepository _employeeRepository;

        public EmployeeService(IEmployeeRepository employeeRepository)
        {
            _employeeRepository = employeeRepository;
        }

        public async Task<IEnumerable<Employee>> GetAllEmployeesAsync()
        {
            return await _employeeRepository.GetAllEmployeesAsync();
        }

        public async Task<Employee> GetEmployeeByIdAsync(int id)
        {
            return await _employeeRepository.GetEmployeeByIdAsync(id);
        }

        public async Task AddEmployeeAsync(Employee employee)
        {
            if (string.IsNullOrWhiteSpace(employee.Name))
            {
                throw new ArgumentException("Employee name is required.");
            }

            var employees = await _employeeRepository.GetAllEmployeesAsync();
            if (employees.Any(e => e.Name.Equals(employee.Name, StringComparison.OrdinalIgnoreCase)))
            {
                throw new InvalidOperationException("An employee with this name already exists.");
            }

            await _employeeRepository.AddEmployeeAsync(employee);
        }

        public async Task UpdateEmployeeAsync(Employee employee)
        {
            if (await _employeeRepository.GetEmployeeByIdAsync(employee.EmployeeId) == null)
            {
                throw new KeyNotFoundException("Employee not found.");
            }

            await _employeeRepository.UpdateEmployeeAsync(employee);
        }

        public async Task DeleteEmployeeAsync(int id)
        {
            var employee = await _employeeRepository.GetEmployeeByIdAsync(id);
            if (employee == null)
            {
                throw new KeyNotFoundException("Employee not found.");
            }

            await _employeeRepository.DeleteEmployeeAsync(id);
        }
    }
}
