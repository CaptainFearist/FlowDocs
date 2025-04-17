using File_Manager.Entities;
using File_Manager.MVVM.ViewModel;
using Microsoft.EntityFrameworkCore;

namespace File_Manager
{
    public class FileService
    {
        private readonly IT_DepartmentsContext _context;
        private readonly int _departmentId;
        private readonly int _userId;

        public FileService(int departmentId, int userId)
        {
            _departmentId = departmentId;
            _userId = userId;
            var optionsBuilder = new DbContextOptionsBuilder<IT_DepartmentsContext>();
            optionsBuilder.UseSqlServer("Data Source=HoneyPot\\SQLEXPRESS; " +
                            "Initial Catalog=IT_Departments;Integrated Security=True;" +
                            "MultipleActiveResultSets=True; TrustServerCertificate=True");
            _context = new IT_DepartmentsContext(optionsBuilder.Options);
        }

        public static readonly Dictionary<string, int> FileTypeMappings = new Dictionary<string, int>
        {
            { ".doc", 1 }, { ".docx", 1 }, { ".pdf", 1 }, { ".txt", 1 }, { ".rtf", 1 },
            { ".xls", 1 }, { ".xlsx", 1 }, { ".ppt", 1 }, { ".pptx", 1 },
            { ".jpg", 2 }, { ".jpeg", 2 }, { ".png", 2 }, { ".gif", 2 },
            { ".bmp", 2 }, { ".tiff", 2 }, { ".mp4", 3 }
        };

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

        public async Task AddFileAsync(string filePath)
        {
            var fileName = System.IO.Path.GetFileName(filePath);
            var fileExtension = System.IO.Path.GetExtension(filePath).ToLower();
            var uploadDate = DateTime.Now;

            if (FileTypeHelper.FileTypeMappings.TryGetValue(fileExtension, out int fileTypeId))
            {
                try
                {
                    byte[] fileBytes = System.IO.File.ReadAllBytes(filePath);
                    long fileSize = fileBytes.Length;

                    var newFile = new Entities.File
                    {
                        FileName = fileName,
                        FileContent = fileBytes,
                        FileSize = fileSize,
                        UploadDate = uploadDate,
                        FileTypeId = fileTypeId,
                        UserId = _userId
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
                catch (Exception ex)
                {
                    throw new InvalidOperationException($"Ошибка при чтении файла: {ex.Message}");
                }
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

        public async Task<Entities.File> DownloadFileAsync(string fileName)
        {
            return await _context.Files.FirstOrDefaultAsync(f => f.FileName == fileName);
        }
    }
}