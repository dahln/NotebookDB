using NotebookDB.Common;
using NotebookDB.Common.Enumerations;
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace NotebookDB.Database
{
    public class NotebookAuthorizedUser
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();

        public bool IsNotebookOwner { get; set; }
        public bool AllowInstanceEdits { get; set; } = false;
        public bool IsNotebookAdmin { get; set; } = false;

        public string UserId { get; set; } = null!;

        public string NotebookId { get; set; } = null!;
        public Notebook Notebook { get; set; } = null!;
    }

    public class Notebook : IMapper
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; } = null!;
        public List<Section> Sections { get; set; } = new List<Section>();
        public List<Field> Fields { get; set; } = new List<Field>();
        public List<Instance> Instances { get; set; } = new List<Instance>();
        public List<NotebookAuthorizedUser> AuthorizedUsers { get; set; } = new List<NotebookAuthorizedUser>();

        public bool AllowAttachments { get; set; } = false;
    }

    public class Section : IMapper
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; } = null!;
        public int Columns { get; set; } = 1;
        public int Order { get; set; }

        public List<Field> Fields { get; set; } = new List<Field>();
        
        public string NotebookId { get; set; } = null!;
        public Notebook Notebook { get; set; } = null!;
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
        public Notebook Notebook { get; set; } = null!;

        public string SectionId { get; set; } = null!;
        public Section Section { get; set; } = null!;
    }

    public class Instance : IMapper
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public List<InstanceValue> Values { get; set; } = new List<InstanceValue>();

        public List<File> Files { get; set; } = new List<File>();

        public string NotebookId { get; set; } = null!;
        public Notebook Notebook { get; set; } = null!;

        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
        public string CreatedById{ get; set; } = null!;

        public DateTime UpdatedOn { get; set; } = DateTime.UtcNow;
        public string UpdatedById { get; set; } = null!;
    }

    public class InstanceValue : IMapper
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();

        public string Value { get; set; } = null!;
        
        public string InstanceId { get; set; } = null!;
        public Instance? Instance { get; set; } = null!;

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

        public string InstanceId { get; set; } = null!;
        public Instance Instance { get; set; } = null!;
    }

    public class FileShard
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public int Index { get; set; }
        public byte[] Data { get; set; }

        public string FileId { get; set; }
        public File File { get; set; }
    }



    public class TemplateNotebook : IMapper
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; } = null!;
        public string Icon { get; set; }
        
        public bool AllowAttachments { get; set; } = false;
        public List<TemplateSection> Sections { get; set; } = new List<TemplateSection>();
        public List<TemplateField> Fields { get; set; } = new List<TemplateField>();
    }

    public class TemplateSection : IMapper
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; } = null!;
        public int Columns { get; set; } = 1;
        public int Order { get; set; }

        public List<TemplateField> Fields { get; set; } = new List<TemplateField>();
        
        public string NotebookId { get; set; } = null!;
        public TemplateNotebook Notebook { get; set; } = null!;
    }

    public class TemplateField : IMapper
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
        public TemplateNotebook Notebook { get; set; } = null!;

        public string SectionId { get; set; } = null!;
        public TemplateSection Section { get; set; } = null!;
    }

    public class SystemSetting
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string? SendGridKey { get; set; }
        public string? SendGridSystemEmailAddress { get; set; }
        public bool RegistrationEnabled { get; set; } = true;
        public string? EmailDomainRestriction { get; set; }
    }
}
