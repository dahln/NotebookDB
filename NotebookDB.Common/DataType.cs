using NotebookDB.Common.Enumerations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NotebookDB.Common
{
    public class CreateNotebook
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; }
        public string? TemplateId { get; set; } = null!;
        public Notebook? Upload { get; set; } = null!;

    }
    public class Notebook : IMapper
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; } = null!;
        public List<Section> Sections { get; set; } = new List<Section>();
        public List<Field> Fields { get; set; } = new List<Field>();
        public List<NotebookAuthorizedUser> AuthorizedUsers { get; set; } = new List<NotebookAuthorizedUser>();

        public bool IsCurrentUserAuthorizedAdmin { get; set; }
        public bool IsCurrentUserEditor { get; set; }
        public bool IsCurrentUserOwner { get; set; }

        public bool AllowAttachments { get; set; } = false;
    }

    public class NotebookAuthorizedUser : IMapper
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public bool AllowInstanceEdits { get; set; } = false;
        public bool IsNotebookAdmin { get; set; } = false;
        public bool IsNotebookOwner { get; set; } = false;
        
        public string UserId { get; set; } = null!;
        public string Email { get; set; } = null!;

        public string NotebookId { get; set; } = null!;

        public bool IsCurrentUser { get; set; } = false;
    }

    public class Folder
    {
        public string Id { get; set; } = null!;
        public string Name { get; set; } = null!;
    }

    public class Section : IMapper
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; } = null!;
        public int Columns { get; set; } = 1;
        public int Order { get; set; }
    }

    public class Field : IMapper
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; } = null!;
        public FieldType Type { get; set; }

        public int Column { get; set; } = 1;
        public int Order { get; set; }
        public string? Options { get; set; }
        public bool Required { get; set; } = true;
        public bool SearchShow { get; set; } = false;
        public int SearchOrder { get; set; } = 1;

        public string NotebookId { get; set; } = null!;
        public string? SectionId { get; set; } = null!;
    }

    public class Instance : IMapper
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public List<InstanceValue> Values { get; set; } = new List<InstanceValue>();
        public List<File> Files { get; set; } = new List<File>();

        public string NotebookId { get; set; } = null!;

        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
        public string? CreatedById { get; set; } = null!;
        public string? CreatedByEmail { get; set; } = null!;

        public DateTime UpdatedOn { get; set; } = DateTime.UtcNow;
        public string? UpdatedById { get; set; } = null!;
        public string? UpdatedByEmail { get; set; } = null!;

        public bool AllowEdits { get; set; } = false;
    }

    public class InstanceValue : IMapper
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();

        //This value can be nullable. The DB will insert an empty value, but allow the client side to be null.
        public string? Value { get; set; } = null!;
        public string FieldId { get; set; } = null!;
        public Field Field { get; set; } = null!;
    }

    public class File
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; } = null!;
        public string MimeType { get; set; } = null!;
        public long FileSize { get; set; }
        public DateTime Uploaded { get; set; } = DateTime.UtcNow;
    }

    public class Download
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; } = null!;
        public string MimeType { get; set; } = null!;
        public long FileSize { get; set; }
        public DateTime Uploaded { get; set; } = DateTime.UtcNow;
        public byte[]? Data { get; set; }
    }

    public class FileShard
    {
        public int Index { get; set; }
        public byte[] Data { get; set; }
    }

    public class FileRename()
    {
        public string Name { get; set; }
    }

    public class TemplateCreate
    {
        public string Id { get; set; }
        public string Icon { get; set; }
    }

    public enum TemplateCreationOption
    {
        FromTemplate = 0,
        ManualCreation = 1,
        FromImport = 2
    }
}
