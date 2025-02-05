using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace File_Manager
{
    public static class FileTypeHelper
    {
        public static readonly Dictionary<string, int> FileTypeMappings = new Dictionary<string, int>
        {
            { ".doc", 1 },
            { ".docx", 1 },
            { ".pdf", 1 },
            { ".txt", 1 },
            { ".rtf", 1 },
            { ".xls", 1 },
            { ".xlsx", 1 },
            { ".ppt", 1 },
            { ".pptx", 1 },
            { ".jpg", 2 },
            { ".jpeg", 2 },
            { ".png", 2 },
            { ".gif", 2 },
            { ".bmp", 2 },
            { ".tiff", 2 },
            { ".mp4", 3 }
        };
    }
}
