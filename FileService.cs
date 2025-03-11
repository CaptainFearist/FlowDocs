using File_Manager.Entities;
using File_Manager.MVVM.ViewModel;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace File_Manager
{
    public class FileService
    {
        private readonly IT_DepartmentsContext _context;
        private readonly int _departmentId;

        public FileService(int departmentId)
        {
            _departmentId = departmentId;
            var optionsBuilder = new DbContextOptionsBuilder<IT_DepartmentsContext>();
            optionsBuilder.UseSqlServer("Data Source=HoneyPot\\SQLEXPRESS; " +
                                            "Initial Catalog=IT_Departments;Integrated Security=True;" +
                                            "MultipleActiveResultSets=True; TrustServerCertificate=True");
            _context = new IT_DepartmentsContext(optionsBuilder.Options);
        }

        public async Task<List<FileInfoViewModel>> GetFilesAsync(string searchQuery = "")
        {
            var query = _context.DepartmentFiles
                .Where(df => df.DepartmentId == _departmentId)
                .Select(df => df.File);

            if (!string.IsNullOrWhiteSpace(searchQuery))
            {
                searchQuery = searchQuery.ToLower();
                query = query.Where(file => file.FileName.ToLower().Contains(searchQuery));
            }

            var files = await query.ToListAsync();
            return files.Select(file => new FileInfoViewModel
            {
                FileName = file.FileName,
                UploadDate = file.UploadDate
            }).ToList();
        }

        public async Task AddFileAsync(string filePath, int userId)
        {
            var fileName = System.IO.Path.GetFileName(filePath);
            var fileExtension = System.IO.Path.GetExtension(filePath).ToLower();
            var uploadDate = DateTime.Now;

            if (FileTypeHelper.FileTypeMappings.TryGetValue(fileExtension, out int fileTypeId))
            {
                var newFile = new Entities.File
                {
                    FileName = fileName,
                    FilePath = filePath,
                    UploadDate = uploadDate,
                    FileTypeId = fileTypeId,
                    UserId = userId
                };

                _context.Files.Add(newFile);
                await _context.SaveChangesAsync();

                var departmentFile = new DepartmentFile
                {
                    DepartmentId = _departmentId,
                    FileId = newFile.FileId
                };

                _context.DepartmentFiles.Add(departmentFile);
                await _context.SaveChangesAsync();
            }
            else
            {
                throw new InvalidOperationException("Неподдерживаемый тип файла");
            }
        }

        public async Task DeleteFileAsync(string fileName)
        {
            var file = await _context.Files.FirstOrDefaultAsync(f => f.FileName == fileName);
            if (file != null)
            {
                var departmentFile = await _context.DepartmentFiles
                    .FirstOrDefaultAsync(df => df.FileId == file.FileId && df.DepartmentId == _departmentId);

                if (departmentFile != null)
                {
                    _context.DepartmentFiles.Remove(departmentFile);
                }

                _context.Files.Remove(file);
                await _context.SaveChangesAsync();
            }
            else
            {
                throw new InvalidOperationException("Файл не найден");
            }
        }
    }
}
