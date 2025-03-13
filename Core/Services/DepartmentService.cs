using File_Manager.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace File_Manager
{
    public class DepartmentService
    {
        private readonly IT_DepartmentsContext _context;

        public DepartmentService(IT_DepartmentsContext context)
        {
            _context = context;
        }

        // Получить всех пользователей по идентификатору отдела
        public async Task<List<User>> GetUsersByDepartmentAsync(int departmentId)
        {
            return await _context.Users
                .Where(u => u.DepartmentId == departmentId)
                .ToListAsync();
        }

        // Получить все отделы
        public async Task<List<Department>> GetAllDepartmentsAsync()
        {
            return await _context.Departments.ToListAsync();
        }
    }

}
