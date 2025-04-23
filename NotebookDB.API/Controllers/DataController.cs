using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NotebookDB.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using NotebookDB.API.Utility;
using NotebookDB.Database;
using NotebookDB.Server.Utility;
using System.Security.AccessControl;
using System.ComponentModel.DataAnnotations;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Text;
using System.Text.RegularExpressions;
using System.Runtime.CompilerServices;
using System.Net;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Net.Http.Headers;
using SendGrid.Helpers.Mail;

namespace CRM.Server.Controllers
{
    [ApiController]
    public class DataController : ControllerBase
    {
        private ApplicationDbContext _db;
        private UserManager<IdentityUser> _userManager;
        private IConfiguration _configuration;
        private readonly IActionContextAccessor _accessor;
        
        public DataController(ApplicationDbContext dbContext, UserManager<IdentityUser> userManager, IConfiguration configuration, IActionContextAccessor accessor)
        {
            _db = dbContext;
            _userManager = userManager;
            _configuration = configuration;
            _accessor = accessor;
        }

        /// <summary>
        /// Get the types that the user is authorized to view
        /// </summary>
        /// <param name="notebookId"></param>
        /// <returns></returns>
        [Authorize]
        [HttpGet]
        [Route("api/v1/user/notebooks")]
        async public Task<IActionResult> GetNotebooksByCurrentUser()
        {
            string userId = User.GetUserId();

            var notebooks = await _db.Notebooks
                                .Where(i => i.AuthorizedUsers.Any(x => x.UserId == userId))
                                .OrderBy(i => i.Name)
                                .ToListAsync();

            var response = new List<NotebookDB.Common.Notebook>();
            foreach(var type in notebooks)
            {
                response.Add(type.Convert<NotebookDB.Common.Notebook>());
            }

            return Ok(response);
        }

        //Create Type
        [Authorize]
        [HttpPost]
        [Route("api/v1/notebook")]
        async public Task<IActionResult> CreateNotebook([FromBody]NotebookDB.Common.CreateNotebook model)
        {
            string userId = User.GetUserId();

            NotebookDB.Database.Notebook newNotebook = new NotebookDB.Database.Notebook()
            {
                Name = model.Name
            };
            _db.Notebooks.Add(newNotebook);
            await _db.SaveChangesAsync();

            if(!string.IsNullOrEmpty(model.TemplateId))
            {
                var sourceNotebook = await _db.TemplateNotebooks
                    .AsNoTracking()
                    .Include(i => i.Fields)
                    .Include(i => i.Sections)
                    .FirstOrDefaultAsync(i => i.Id == model.TemplateId);

                if(sourceNotebook != null)
                {
                    newNotebook.AllowAttachments = sourceNotebook.AllowAttachments;

                    foreach(var section in sourceNotebook.Sections)
                    {
                        NotebookDB.Database.Section newSection = new NotebookDB.Database.Section()
                        {
                            Name = section.Name,
                            Columns = section.Columns,
                            Order = section.Order,
                            NotebookId = newNotebook.Id
                        };
                        _db.Sections.Add(newSection);
                        await _db.SaveChangesAsync();

                        foreach(var field in sourceNotebook.Fields.Where(x => x.SectionId == section.Id))
                        {
                            NotebookDB.Database.Field newField = new NotebookDB.Database.Field()
                            {
                                Name = field.Name,
                                Type = field.Type,
                                Column = field.Column,
                                Order = field.Order,
                                Options = field.Options,
                                Required = field.Required,
                                SearchOrder = field.SearchOrder,
                                SearchShow = field.SearchShow,
                                NotebookId = newNotebook.Id,
                                SectionId = newSection.Id
                            };
                            _db.Fields.Add(newField);
                            await _db.SaveChangesAsync();
                        }
                    }
                }
            }
            else if(model.Upload != null)
            {
                newNotebook.AllowAttachments = model.Upload.AllowAttachments;

                foreach(var section in model.Upload.Sections)
                {
                    NotebookDB.Database.Section newSection = new NotebookDB.Database.Section()
                    {
                        Name = section.Name,
                        Columns = section.Columns,
                        Order = section.Order,
                        NotebookId = newNotebook.Id
                    };
                    _db.Sections.Add(newSection);
                    await _db.SaveChangesAsync();

                    foreach(var field in model.Upload.Fields.Where(x => x.SectionId == section.Id))
                    {
                        NotebookDB.Database.Field newField = new NotebookDB.Database.Field()
                        {
                            Name = field.Name,
                            Type = field.Type,
                            Column = field.Column,
                            Order = field.Order,
                            Options = field.Options,
                            Required = field.Required,
                            SearchOrder = field.SearchOrder,
                            SearchShow = field.SearchShow,
                            NotebookId = newNotebook.Id,
                            SectionId = newSection.Id
                        };
                        _db.Fields.Add(newField);
                        await _db.SaveChangesAsync();
                    }
                }
            }

            NotebookDB.Database.NotebookAuthorizedUser authorizedUser = new NotebookDB.Database.NotebookAuthorizedUser()
            {
                IsNotebookOwner = true,
                IsNotebookAdmin = true,
                AllowInstanceEdits = true,
                Notebook = newNotebook,
                NotebookId = newNotebook.Id,
                UserId = userId
            };
            _db.NotebookAuthorizedUsers.Add(authorizedUser);

            await _db.SaveChangesAsync();

            return Ok(newNotebook.Convert<NotebookDB.Common.Notebook>());
        }

        //Get Type, to manage type
        [Authorize]
        [HttpGet]
        [Route("api/v1/notebook/{notebookId}")]
        async public Task<IActionResult> GetNotebookById(string notebookId)
        {
            string userId = User.GetUserId();
            var canAccess = await CanAccessNotebook(userId, notebookId);
            if (!canAccess)
                return BadRequest("You do not have permission to access this notebook.");

            var canAccessAdmin = await CanAccessNotebook(userId, notebookId, true);

            var query = _db.Notebooks
                            .Include(i => i.Fields)
                            .Include(i => i.Sections)
                            .Include(i => i.AuthorizedUsers)
                            .Where(i => i.Id == notebookId);

            var notebook = await query.FirstOrDefaultAsync();

            var response = notebook.Convert<NotebookDB.Common.Notebook>();
            response.IsCurrentUserAuthorizedAdmin = response.AuthorizedUsers.Any(x => x.UserId == userId && x.IsNotebookAdmin);
            response.IsCurrentUserEditor = response.AuthorizedUsers.Any(x => x.UserId == userId && x.AllowInstanceEdits);
            response.IsCurrentUserOwner = response.AuthorizedUsers.Any(x => x.UserId == userId && x.IsNotebookOwner);

            if (canAccessAdmin)
            {
                foreach (var user in response.AuthorizedUsers)
                {
                    var email = await _db.Users.Where(x => x.Id == user.UserId).Select(x => x.Email).FirstOrDefaultAsync();// await _accountService.GetUserEmailById(user.UserId);;
                    if (email != null)
                    {
                        user.Email = email;
                    }
                }
                response.AuthorizedUsers = response.AuthorizedUsers.Where(x => x.Email != null).ToList();
                foreach(var user in response.AuthorizedUsers)
                {
                    user.IsCurrentUser = user.UserId == userId;
                }
            }

            return Ok(response);
        }

        //Edit Type
        [Authorize]
        [HttpPut]
        [Route("api/v1/notebook/{notebookId}")]
        async public Task<IActionResult> UpdateNotebookById([FromBody] NotebookDB.Common.Notebook model, string notebookId)
        {
            string userId = User.GetUserId();
            var canAccess = await CanAccessNotebook(userId, notebookId, true);
            if (!canAccess)
                return BadRequest();

            var notebook = await _db.Notebooks.Where(i => i.Id == notebookId)
                                    .Include(i => i.Fields)
                                    .FirstOrDefaultAsync();

            if (notebook == null)
                return BadRequest();

            //Update the fields
            notebook.Name = model.Name;
            notebook.AllowAttachments = model.AllowAttachments;

            await _db.SaveChangesAsync();

            return Ok(notebook.Convert<NotebookDB.Common.Notebook>());
        }

        //Delete Type
        [Authorize]
        [HttpDelete]
        [Route("api/v1/notebook/{notebookId}")]
        async public Task<IActionResult> DeleteNotebookById(string notebookId)
        {
            string userId = User.GetUserId();
            var canAccess = await CanAccessNotebook(userId, notebookId, true);
            if (!canAccess)
                return BadRequest();

            //Only an owner can delete a notebook
            var isOwner = await IsNotebookOwner(userId, notebookId);
            if (!isOwner)
                return BadRequest();

            var notebook = await _db.Notebooks.Where(i => i.Id == notebookId)
                                    .Include(i => i.Fields)
                                    .FirstOrDefaultAsync();

            var authorizations = _db.NotebookAuthorizedUsers.Where(o => o.NotebookId == notebookId);
            _db.NotebookAuthorizedUsers.RemoveRange(authorizations);

            var shards = _db.FileShards.Where(x => x.File.Instance.NotebookId == notebookId);
            _db.FileShards.RemoveRange(shards);
            await _db.SaveChangesAsync();

            var files = _db.Files.Where(x => x.Instance.NotebookId == notebookId);
            _db.Files.RemoveRange(files);
            await _db.SaveChangesAsync();

            var values = _db.Values.Where(x => x.Instance.NotebookId == notebookId);
            _db.Values.RemoveRange(values);

            var instances = _db.Instances.Where(x => x.NotebookId == notebookId);
            _db.Instances.RemoveRange(instances);

            var fields = _db.Fields.Where(x => x.NotebookId == notebookId);
            _db.Fields.RemoveRange(fields);

            var types = _db.Notebooks.Where(x => x.Id == notebookId);
            _db.Notebooks.RemoveRange(types);

            await _db.SaveChangesAsync();

            return Ok();
        }


        [Authorize]
        [HttpDelete]
        [Route("api/v1/notebook/{notebookId}/data")]
        async public Task<IActionResult> DeleteNotebookDataById(string notebookId)
        {
            string userId = User.GetUserId();
            var canAccess = await CanAccessNotebook(userId, notebookId, true);
            if (!canAccess)
                return BadRequest();

            //Only an owner can delete notebook data
            var isOwner = await IsNotebookOwner(userId, notebookId);
            if (!isOwner)
                return BadRequest();

            var shards = _db.FileShards.Where(x => x.File.Instance.NotebookId == notebookId);
            _db.FileShards.RemoveRange(shards);
            await _db.SaveChangesAsync();

            var files = _db.Files.Where(x => x.Instance.NotebookId == notebookId);
            _db.Files.RemoveRange(files);
            await _db.SaveChangesAsync();

            var values = _db.Values.Where(x => x.Instance.NotebookId == notebookId);
            _db.Values.RemoveRange(values);
            await _db.SaveChangesAsync();

            var instances = _db.Instances.Where(x => x.NotebookId == notebookId);
            _db.Instances.RemoveRange(instances);
            await _db.SaveChangesAsync();

            return Ok();
        }

        //Search Types. No paging. No sorting.




        //Create Type Field
        [Authorize]
        [HttpPost]
        [Route("api/v1/notebook/{notebookId}/field")]
        async public Task<IActionResult> CreateNotebookField([FromBody] NotebookDB.Common.Field model, string notebookId)
        {
            string userId = User.GetUserId();
            var canAccess = await CanAccessNotebook(userId, notebookId, true);
            if (!canAccess)
                return BadRequest();

            var typeExists = await _db.Notebooks.AnyAsync(x => x.Id == notebookId);
            if (!typeExists)
                return BadRequest();

            NotebookDB.Database.Field newField = model.Convert<NotebookDB.Database.Field>();
            newField.NotebookId = notebookId; //Enforce the TypeId for this Field
            _db.Fields.Add(newField);

            await _db.SaveChangesAsync();

            //Don't return an object. After the call, the full type can be reloaded.
            return Ok();
        }

        //Update Type Field
        [Authorize]
        [HttpPut]
        [Route("api/v1/notebook/{notebookId}/field/{fieldId}")]
        async public Task<IActionResult> UpdateNotebookField([FromBody] NotebookDB.Common.Field model, string notebookId, string fieldId)
        {
            string userId = User.GetUserId();
            var canAccess = await CanAccessNotebook(userId, notebookId, true);
            if (!canAccess)
                return BadRequest();

            var existingField = await _db.Fields.FirstOrDefaultAsync(x => x.NotebookId == notebookId && x.Id == fieldId);
            if (existingField == null)
                return BadRequest();

            existingField.Name = model.Name;
            existingField.Column = model.Column;
            existingField.Order = model.Order;
            existingField.Type = model.Type;
            existingField.Required = model.Type != NotebookDB.Common.Enumerations.FieldType.Label ? model.Required : false;
            existingField.Options = model.Options;
            existingField.SearchOrder = model.Type != NotebookDB.Common.Enumerations.FieldType.Label ? model.SearchOrder : 0;
            existingField.SearchShow = model.Type != NotebookDB.Common.Enumerations.FieldType.Label ? model.SearchShow : false;

            await _db.SaveChangesAsync();

            //Don't return an object. After the call, the full type can be reloaded.
            return Ok();
        }

        //Delete Notebook Field
        [Authorize]
        [HttpDelete]
        [Route("api/v1/notebook/{notebookId}/field/{fieldId}")]
        async public Task<IActionResult> DeleteNotebookField(string notebookId, string fieldId)
        {
            string userId = User.GetUserId();
            var canAccess = await CanAccessNotebook(userId, notebookId, true);
            if (!canAccess)
                return BadRequest();

            var fieldExists = await _db.Fields.AnyAsync(x => x.NotebookId == notebookId && x.Id == fieldId);
            if (!fieldExists)
                return BadRequest();

            var values = _db.Values.Where(x => x.Instance.NotebookId == notebookId && x.FieldId == fieldId);
            _db.Values.RemoveRange(values);

            var fields = _db.Fields.Where(x => x.NotebookId == notebookId && x.Id == fieldId);
            _db.Fields.RemoveRange(fields);

            await _db.SaveChangesAsync();

            //Don't return an object. After the call, the full type can be reloaded.
            return Ok();
        }



        [Authorize]
        [HttpPut]
        [Route("api/v1/notebook/{notebookId}/fields")]
        async public Task<IActionResult> UpdateNotebookFields([FromBody] NotebookDB.Common.Notebook model, string notebookId)
        {
            string userId = User.GetUserId();
            var canAccess = await CanAccessNotebook(userId, notebookId, true);
            if (!canAccess)
                return BadRequest();

            var existingNotebook = await _db.Notebooks
                                            .Include(x => x.Sections)
                                            .Include(x => x.Fields)
                                            .FirstOrDefaultAsync(x => x.Id == notebookId);
            if (existingNotebook == null)
                return BadRequest();


            //Process existing sections. Update any new ones which STILL remain, remove the sections that have been deleted.
            List<string> sectionsProcessed = new List<string>();
            foreach (var section in existingNotebook.Sections)
            {
                section.NotebookId = notebookId;

                var existingSection = model.Sections.FirstOrDefault(x => x.Id == section.Id);
                if (existingSection != null)
                {
                    if(string.IsNullOrEmpty(existingSection.Name))
                        return BadRequest("Section Name is Required");

                    //Update existing field
                    section.Name = existingSection.Name;
                    section.Columns = existingSection.Columns;
                    section.Order = existingSection.Order;
                }
                else
                {
                    //Remove old field
                    _db.Sections.Remove(section);
                }

                sectionsProcessed.Add(section.Id);
            }
            
            //Add new section
            foreach (var section in model.Sections.Where(x => !sectionsProcessed.Contains(x.Id)))
            {
                var newSection = section.Convert<NotebookDB.Database.Section>();
                newSection.NotebookId = notebookId;

                if(string.IsNullOrEmpty(newSection.Name))
                    return BadRequest("Section Name is Required");

                _db.Sections.Add(newSection);
            }



            //Process existing fields. Update any new ones which STILL remain, remove the fields that have been deleted.
            List<string> fieldsProcessed = new List<string>();
            foreach (var field in existingNotebook.Fields)
            {
                var existingField = model.Fields.FirstOrDefault(x => x.Id == field.Id);
                if (existingField != null)
                {
                    if(string.IsNullOrEmpty(existingField.Name))
                        return BadRequest("Name is Required for Field");
                    if(string.IsNullOrEmpty(existingField.SectionId))
                        return BadRequest("Section is Required for Field");

                    //Update existing field
                    field.Name = existingField.Name;
                    field.Column = existingField.Column;
                    field.Order = existingField.Order;
                    field.Required = existingField.Type != NotebookDB.Common.Enumerations.FieldType.Label ? existingField.Required : false;
                    field.Options = existingField.Options;
                    field.SearchOrder = existingField.SearchOrder;
                    field.SearchShow = existingField.SearchShow;
                    field.SectionId = existingField.SectionId;
                }
                else
                {
                    var values = _db.Values.Where(x => x.Instance.NotebookId == notebookId && x.FieldId == field.Id);
                    _db.Values.RemoveRange(values);

                    //Remove old field
                    _db.Fields.RemoveRange(field);
                }

                fieldsProcessed.Add(field.Id);
            }

            //Add new field
            foreach (var field in model.Fields.Where(x => !fieldsProcessed.Contains(x.Id)))
            {
                var newField = field.Convert<NotebookDB.Database.Field>();
                newField.NotebookId = notebookId;
                newField.Required = newField.Type != NotebookDB.Common.Enumerations.FieldType.Label ? newField.Required : false;
                if(string.IsNullOrEmpty(newField.SectionId))
                    return BadRequest("Section is Required for Field");
                if(string.IsNullOrEmpty(newField.Name))
                    return BadRequest("Name is Required for Field");
                if(newField.Type == 0)
                    return BadRequest("Notebook is Required for Field");
                if(string.IsNullOrEmpty(newField.NotebookId))
                    return BadRequest("Notebook is Required for Field");

                _db.Fields.Add(newField);
            }

            await _db.SaveChangesAsync();

            //Don't return an object. After the call, the full type can be reloaded.
            return Ok();
        }

        [Authorize]
        [HttpGet]
        [Route("api/v1/notebook/{notebookId}/export")]
        async public Task<IActionResult> NotebookExport(string notebookId)
        {
            string userId = User.GetUserId();
            var canManage = await CanAccessNotebook(userId, notebookId, true);
            if(!canManage)
                return BadRequest();

            var instances = await _db.Instances.Where(x => x.NotebookId == notebookId)
                                .Include(x => x.Notebook)
                                .Include(x => x.Notebook.Fields)
                                .Include(x => x.Values).ThenInclude(x => x.Field).ToListAsync();

            var headers = instances.SelectMany(x => x.Notebook.Fields.Select(y => y.Name)).Distinct().ToList();

            List<string> export = new List<string>();
            var header = string.Join('|', headers);
            export.Add(header);

            foreach(var instance in instances)
            {
                StringBuilder builder = new StringBuilder();
                foreach(var fieldName in headers)
                {
                    var value = instance.Values.FirstOrDefault(x => x.Field.Name == fieldName);
                    if(value != null && !string.IsNullOrEmpty(value.Value))
                    {
                        var escapedValue = value.Value.Replace("|","\\|");
                        builder.Append(escapedValue);
                        builder.Append("|");
                    }
                    else
                    {
                        builder.Append("|");
                    }
                }
                builder = builder.Remove(builder.Length-1,1);
                export.Add(builder.ToString());
            }

            // Create a stream that will combine the byte arrays.
            var memoryStream = new MemoryStream();
            var bytes = Encoding.ASCII.GetBytes(string.Join(@"\n", export));
            memoryStream.Write(bytes, 0, bytes.Length);

            // Reset the position to the beginning of the stream before returning it.
            memoryStream.Position = 0;

            // Return the stream as a file download.
            return File(memoryStream, "application/octet-stream", DateTime.Now.Ticks + "_export.csv");
        }

        [Authorize]
        [HttpPost]
        [Route("api/v1/notebook/{notebookId}/import")]
        async public Task<IActionResult> NotebookImport([FromBody] NotebookDB.Common.Download model, string notebookId)
        {
            string userId = User.GetUserId();
            var canManage = await CanAccessNotebook(userId, notebookId, true);
            if(!canManage)
                return BadRequest();

            var content = Encoding.ASCII.GetString(model.Data);
            var lines = content.Split("\\n");
            var header = Regex.Split(lines.FirstOrDefault(), @"(?<!\\)\|");
           
            var fields = await _db.Fields.Where(x => x.NotebookId == notebookId).ToListAsync();
            foreach(var line in lines.Skip(1))
            {
                NotebookDB.Database.Instance instance = new NotebookDB.Database.Instance()
                {
                    NotebookId = notebookId,
                    CreatedById = userId,
                    UpdatedById = userId
                };
                _db.Instances.Add(instance);
                await _db.SaveChangesAsync();

                var values = Regex.Split(line, @"(?<!\\)\|");
                for(int a = 0; a < header.Count(); a++)
                {
                    var field = fields.FirstOrDefault(x => x.Name == header[a]);
                    if(field != null)
                    {
                        if(field.Type == NotebookDB.Common.Enumerations.FieldType.Date)
                        {
                            DateTime parsedDate;
                            if(DateTime.TryParse(values[a], out parsedDate) == false)
                            {
                                //Break the loop and skip this value.
                                continue;
                            }
                        }

                        var unescapedValue = values[a].Replace("\\|","|");
                        NotebookDB.Database.InstanceValue value = new NotebookDB.Database.InstanceValue()
                        {
                            FieldId = field.Id,
                            InstanceId = instance.Id,
                            Value = unescapedValue
                        };
                        _db.Values.Add(value);
                        await _db.SaveChangesAsync();
                    }
                }
            }

            return Ok();
        }

        [Authorize]
        [HttpGet]
        [Route("api/v1/notebook/{notebookId}/schema/export")]
        async public Task<IActionResult> NotebookSchemaExport(string notebookId)
        {
            string userId = User.GetUserId();
            var canManage = await CanAccessNotebook(userId, notebookId, true);
            if(!canManage)
                return BadRequest();

            var notebook = await _db.Notebooks.Where(x => x.Id == notebookId)
                                .Include(x => x.Sections)
                                .Include(x => x.Fields)
                                .FirstOrDefaultAsync();

            var response = notebook.Convert<NotebookDB.Common.Notebook>();

            string export = JsonConvert.SerializeObject(response);

            // Create a stream that will combine the byte arrays.
            var memoryStream = new MemoryStream();
            var bytes = Encoding.ASCII.GetBytes(string.Join(@"\n", export));
            memoryStream.Write(bytes, 0, bytes.Length);

            // Reset the position to the beginning of the stream before returning it.
            memoryStream.Position = 0;

            // Return the stream as a file download.
            return File(memoryStream, "application/octet-stream", $"{DateTime.Now.Ticks}_{notebook.Name}_schema_export.json");
        }

        [Authorize]
        [HttpPost]
        [Route("api/v1/notebook/{notebookId}/authorized")]
        async public Task<IActionResult> CreateAuthorizedUserByType([FromBody] NotebookDB.Common.NotebookAuthorizedUser model, string notebookId)
        {
            string userId = User.GetUserId();
            var canAccess = await CanAccessNotebook(userId, notebookId, true);
            if (!canAccess)
                return BadRequest();

            var notebook = await _db.Notebooks.FirstOrDefaultAsync(o => o.Id == notebookId);
            if (notebook == null)
                return BadRequest();

            var userInfo = await _db.Users.Where(x => x.Email.ToLower() == model.Email.ToLower()).Select(x => x.Id).FirstOrDefaultAsync();// await _accountService.GetUserIdByUserEmail(model.Email);
            if (userInfo == null)
                return BadRequest("User not found");

            var authorizationAlreadyExistsForThisUser = await _db.NotebookAuthorizedUsers.AnyAsync(x => x.UserId == userInfo && x.NotebookId == notebookId);
            if(authorizationAlreadyExistsForThisUser)
                return BadRequest("Cannot add duplicate");

            NotebookDB.Database.NotebookAuthorizedUser authorizedUser = new NotebookDB.Database.NotebookAuthorizedUser()
            {
                NotebookId = notebookId,
                UserId = userInfo,
                IsNotebookAdmin = model.IsNotebookAdmin
            };

            _db.NotebookAuthorizedUsers.Add(authorizedUser);

            await _db.SaveChangesAsync();

            return Ok();
        }

        [Authorize]
        [HttpDelete]
        [Route("api/v1/notebook/{notebookId}/authorized/{authorizedUserId}")]
        async public Task<IActionResult> DeleteAuthorizedUserByType(string notebookId, string authorizedUserId)
        {
            string userId = User.GetUserId();
            var canAccess = await CanAccessNotebook(userId, notebookId);
            if (!canAccess)
                return BadRequest();    

            var currentUser = await _userManager.FindByIdAsync(userId);
            var isAdministrator = await _userManager.IsInRoleAsync(currentUser, "Administrator");
            
            if (userId == authorizedUserId && !isAdministrator)
                return BadRequest("Cannot remove yourself");

            var removeThese = _db.NotebookAuthorizedUsers.Where(o => o.NotebookId == notebookId && o.UserId == authorizedUserId);
            _db.NotebookAuthorizedUsers.RemoveRange(removeThese);

            await _db.SaveChangesAsync();

            return Ok();
        }

        [Authorize]
        [HttpGet]
        [Route("api/v1/notebook/{notebookId}/authorized/{authorizedUserId}/toggle/edits")]
        async public Task<IActionResult> ToggleAuthorizedUserByType(string notebookId, string authorizedUserId)
        {
            string userId = User.GetUserId();
            var canAccess = await CanAccessNotebook(userId, notebookId, true);
            if (!canAccess)
                return BadRequest();

            var isOwner = await IsNotebookOwner(authorizedUserId, notebookId);
            if(isOwner)
                return BadRequest("You cannot remove the admin or edit role from the owner.");

            if (userId == authorizedUserId)
                return BadRequest("Cannot toggle your own permissions for a 'folder'");

            var updateThis = await _db.NotebookAuthorizedUsers.Where(o => o.NotebookId == notebookId && o.UserId == authorizedUserId).FirstOrDefaultAsync();
            if (updateThis == null)
                return BadRequest();
            
            updateThis.AllowInstanceEdits = updateThis.AllowInstanceEdits ? false : true;

            if (updateThis.AllowInstanceEdits == false)
            {
                updateThis.IsNotebookAdmin = false;
                updateThis.IsNotebookOwner = false;
            }

            await _db.SaveChangesAsync();

            return Ok();
        }

        [Authorize]
        [HttpGet]
        [Route("api/v1/notebook/{notebookId}/authorized/{authorizedUserId}/toggle/owner")]
        async public Task<IActionResult> TogglesdNotebookOwnerByType(string notebookId, string authorizedUserId)
        {
            string userId = User.GetUserId();
            var canAccess = await CanAccessNotebook(userId, notebookId, true);
            if (!canAccess)
                return BadRequest();

            var isOwner = await IsNotebookOwner(userId, notebookId);
            if(!isOwner)
                return BadRequest();

            if (userId == authorizedUserId)
                return BadRequest("Cannot toggle your own permissions for a 'folder'");

            var updateThis = await _db.NotebookAuthorizedUsers.Where(o => o.NotebookId == notebookId && o.UserId == authorizedUserId).FirstOrDefaultAsync();
            if (updateThis == null)
                return BadRequest();
            
            //Set the user as the owner.
            updateThis.IsNotebookOwner = updateThis.IsNotebookOwner ? false : true;
            if (updateThis.IsNotebookOwner == true)
            {
                updateThis.AllowInstanceEdits = true;
                updateThis.IsNotebookAdmin = true;
            }
            await _db.SaveChangesAsync();


            //Find the previous owner(s?) and remove their owner role
            var previousOwners = await _db.NotebookAuthorizedUsers.Where(x => x.NotebookId == notebookId && x.UserId != authorizedUserId && x.IsNotebookOwner == true).ToListAsync();
            foreach(var previousOwner in previousOwners)
            {
                previousOwner.IsNotebookOwner = false;
            }
            await _db.SaveChangesAsync();

            return Ok();
        }

        [Authorize]
        [HttpGet]
        [Route("api/v1/notebook/{notebookId}/authorized/{authorizedUserId}/toggle/admin")]
        async public Task<IActionResult> TogglesdAllowInstanceEditUserByType(string notebookId, string authorizedUserId)
        {
            string userId = User.GetUserId();
            var canAccess = await CanAccessNotebook(userId, notebookId, true);
            if (!canAccess)
                return BadRequest();

            if (userId == authorizedUserId)
                return BadRequest("Cannot toggle your own permissions for a 'folder'");

            var isOwner = await IsNotebookOwner(authorizedUserId, notebookId);
            if(isOwner)
                return BadRequest("You cannot remove the admin or edit role from the owner.");

            var updateThis = await _db.NotebookAuthorizedUsers.Where(o => o.NotebookId == notebookId && o.UserId == authorizedUserId).FirstOrDefaultAsync();
            if (updateThis == null)
                return BadRequest();

            updateThis.IsNotebookAdmin = updateThis.IsNotebookAdmin ? false : true;
            if (updateThis.IsNotebookAdmin == true)
            {
                updateThis.AllowInstanceEdits = true;
            }
            else
            {
                updateThis.IsNotebookOwner = false;
            }

            await _db.SaveChangesAsync();

            return Ok();
        }


























        //Create Instance
        [Authorize]
        [HttpPost]
        [Route("api/v1/notebook/{notebookId}/instance")]
        async public Task<IActionResult> CreateInstanceByType([FromBody] NotebookDB.Common.Instance model, string notebookId)
        {
            string userId = User.GetUserId();
            var canAccess = await CanAccessNotebook(userId, notebookId);
            if (!canAccess)
                return BadRequest();

            var canEdit = await CanEditNotebook(userId, notebookId);
            if(!canEdit)
                return BadRequest();

            NotebookDB.Database.Instance instance = new NotebookDB.Database.Instance()
            {
                NotebookId = notebookId,
                CreatedById = userId,
                UpdatedById = userId
            };
            foreach (var value in model.Values)
            {
                if(value.Field.Type == NotebookDB.Common.Enumerations.FieldType.Date && !string.IsNullOrEmpty(value.Value))
                {
                    var dateTimeValueAsTicks = DateTime.Parse(value.Value).Ticks;
                    instance.Values.Add(new NotebookDB.Database.InstanceValue()
                    {
                        InstanceId = instance.Id,
                        Value = dateTimeValueAsTicks.ToString(), //Fill in null values with empty string. This simplifies searching and sorting with the dynamic data.
                        FieldId = value.Field.Id
                    });
                }
                else
                {
                    instance.Values.Add(new NotebookDB.Database.InstanceValue()
                    {
                        InstanceId = instance.Id,
                        Value = value.Value ?? string.Empty, //Fill in null values with empty string. This simplifies searching and sorting with the dynamic data.
                        FieldId = value.Field.Id
                    });
                }
            }
            instance.UpdatedOn = instance.CreatedOn;

            _db.Instances.Add(instance);
            await _db.SaveChangesAsync();

            //Don't return an object. After the call, the full instance can be reloaded.
            return Ok(instance.Convert<NotebookDB.Common.Instance>());
        }

        //Get Instance
        [Authorize]
        [HttpGet]
        [Route("api/v1/notebook/{notebookId}/instance/{instanceId}")]
        async public Task<IActionResult> GetInstanceTypeById(string notebookId, string instanceId)
        {
            string userId = User.GetUserId();
            var canAccess = await CanAccessNotebook(userId, notebookId);
            if (!canAccess)
                return BadRequest("You do not have permission to access this record.");

            var instance = await _db.Instances
                                    .Include(i => i.Notebook).ThenInclude(v => v.Fields)
                                    .Include(i => i.Values).ThenInclude(v => v.Field)
                                    .FirstOrDefaultAsync(i => i.NotebookId == notebookId
                                                                && i.Id == instanceId);

            if (instance == null)
                return BadRequest();

            foreach (var field in instance.Notebook.Fields)
            {
                var exists = instance.Values.Any(v => v.FieldId == field.Id);
                if (!exists)
                {
                    instance.Values.Add(new NotebookDB.Database.InstanceValue()
                    {
                        InstanceId = instance.Id,
                        FieldId = field.Id,
                        Field = field
                    });
                }
            }

            var response = instance.Convert<NotebookDB.Common.Instance>();

            //These accountService methods populate the CreatedBy/UpdatedBy fields with userId infor from Auth0.
            //Have to do this after the mapping because the "Email" fields don't exist on the entities.
            if (response.CreatedById != null)
                response.CreatedByEmail = await _db.Users.Where(x => x.Id == userId).Select(x => x.Email).FirstOrDefaultAsync();// await _accountService.GetUserEmailById(response.CreatedById);
            if (response.UpdatedById != null)
                response.UpdatedByEmail = await _db.Users.Where(x => x.Id == userId).Select(x => x.Email).FirstOrDefaultAsync();// await _accountService.GetUserEmailById(response.UpdatedById);

            if(instance.Notebook.AllowAttachments)
            {
                //Do this so we don't query all the file data
                response.Files = await _db.Files.Where(x => x.InstanceId == instance.Id).Select(x => new NotebookDB.Common.File()
                {
                    Id = x.Id,
                    Name = x.Name,
                    MimeType = x.MimeType,
                    Uploaded = x.Uploaded,
                    FileSize = x.FileSize
                }).ToListAsync();
            }

            response.AllowEdits = await CanEditNotebook(userId, notebookId);

            return Ok(response);
        }

        //Edit Instance
        [Authorize]
        [HttpPut]
        [Route("api/v1/notebook/{notebookId}/instance/{instanceId}")]
        async public Task<IActionResult> UpdateInstanceByType([FromBody] NotebookDB.Common.Instance model, string notebookId, string instanceId)
        {
            string userId = User.GetUserId();
            var canAccess = await CanAccessNotebook(userId, notebookId);
            if (!canAccess)
                return BadRequest();

            var canEdit = await CanEditNotebook(userId, notebookId);
            if (!canEdit)
                return BadRequest();

            var instance = await _db.Instances
                                    .Include(i => i.Notebook).ThenInclude(d => d.Fields)
                                    .Include(i => i.Values)
                                    .FirstOrDefaultAsync(i => i.NotebookId == notebookId
                                                                && i.Id == instanceId);

            if (instance == null)
                return BadRequest();

            instance.UpdatedOn = DateTime.UtcNow;
            instance.UpdatedById = userId;

            foreach (var field in instance.Notebook.Fields)
            {
                var value = instance.Values.FirstOrDefault(v => v.FieldId == field.Id);
                if (value != null)
                {
                    value.Value = model.Values.FirstOrDefault(v => v.Id == value.Id).Value;
                }
                else
                {
                    if(value.Field.Type == NotebookDB.Common.Enumerations.FieldType.Date && !string.IsNullOrEmpty(value.Value))
                    {
                        var dateTimeValueAsTicks = DateTime.Parse(value.Value).Ticks;
                        instance.Values.Add(new NotebookDB.Database.InstanceValue()
                        {
                            InstanceId = instance.Id,
                            FieldId = value.Field.Id,
                            Value = dateTimeValueAsTicks.ToString(), //Fill in null values with empty string. This simplifies searching and sorting with the dynamic data.
                        });
                    }
                    else
                    {
                        instance.Values.Add(new NotebookDB.Database.InstanceValue()
                        {
                            FieldId = field.Id,
                            InstanceId = instance.Id,
                            Value = value?.Value ?? string.Empty
                        });
                    }
                }
            }

            await _db.SaveChangesAsync();

            // instance.Files.ForEach(x => x.Data = null);

            var response = instance.Convert<NotebookDB.Common.Instance>();

            //Have to do this after the mapping because the "Email" fields don't exist on the entities.
            if (response.CreatedById != null)
                response.CreatedByEmail = await _db.Users.Where(x => x.Id == userId).Select(x => x.Email).FirstOrDefaultAsync();// await _accountService.GetUserEmailById(response.CreatedById);
            if (response.UpdatedById != null)
                response.UpdatedByEmail = await _db.Users.Where(x => x.Id == userId).Select(x => x.Email).FirstOrDefaultAsync();// await _accountService.GetUserEmailById(response.UpdatedById);

            response.AllowEdits = await CanEditNotebook(userId, notebookId);

            return Ok(response);
        }

        //Delete Instance
        [Authorize]
        [HttpDelete]
        [Route("api/v1/notebook/{notebookId}/instance/{instanceId}")]
        async public Task<IActionResult> DeleteInstanceByType(string notebookId, string instanceId)
        {
            string userId = User.GetUserId();
            var canAccess = await CanAccessNotebook(userId, notebookId);
            if (!canAccess)
                return BadRequest();

            var canEdit = await CanEditNotebook(userId, notebookId);
            if (!canEdit)
                return BadRequest();

            var instance = await _db.Instances
                                    .Include(i => i.Notebook).ThenInclude(d => d.Fields)
                                    .Include(i => i.Values)
                                    .FirstOrDefaultAsync(i => i.NotebookId == notebookId
                                                                && i.Id == instanceId);

            if (instance == null)
                return BadRequest();

            foreach (var value in instance.Values)
            {
                _db.Values.Remove(value);
            }

            var shards = _db.FileShards.Where(x => x.File.Instance.NotebookId == notebookId && x.File.InstanceId == instanceId);
            _db.FileShards.RemoveRange(shards);
            await _db.SaveChangesAsync();

            var files = _db.Files.Where(x => x.Instance.NotebookId == notebookId && x.InstanceId == instanceId);
            _db.Files.RemoveRange(files);
            await _db.SaveChangesAsync();

            _db.Instances.Remove(instance);

            await _db.SaveChangesAsync();

            return Ok();
        }

        //Search Instance
        [Authorize]
        [HttpPost]
        [Route("api/v1/notebook/{notebookId}/search")]
        async public Task<IActionResult> SearchInstancesByType([FromBody] Search model, string notebookId)
        {
            string userId = User.GetUserId();
            var canAccess = await CanAccessNotebook(userId, notebookId);
            if (!canAccess)
                return BadRequest();

            var query = _db.Instances
                            .Include(i => i.Notebook)
                            .Include(i => i.Values.Where(x => x.Field.SearchShow == true)).ThenInclude(v => v.Field)
                            .Where(i => i.NotebookId == notebookId);

            if (!string.IsNullOrEmpty(model.SortBy))
            {
                query = model.SortDirection == SortDirection.Ascending
                            ? query.OrderBy(c => c.Values.FirstOrDefault(v => v.FieldId == model.SortBy && v.Value != null).Value.ToLower())
                            : query.OrderByDescending(c => c.Values.FirstOrDefault(v => v.FieldId == model.SortBy && v.Value != null).Value.ToLower());
            }
            else
            {
                query = query.OrderByDescending(c => c.UpdatedOn);
            }

            if (!string.IsNullOrEmpty(model.FilterText))
            {
                query = query.Where(i => i.Values.Any(v => (v.Field.Type != NotebookDB.Common.Enumerations.FieldType.Image && v.Field.Type != NotebookDB.Common.Enumerations.FieldType.Label) && v.Value.ToLower().Contains(model.FilterText.ToLower())));
            }

            SearchResponse<NotebookDB.Common.Instance> response = new SearchResponse<NotebookDB.Common.Instance>();
            response.Total = await query.CountAsync();

            var dataResponse = await query.Skip(model.Page * model.PageSize)
                                        .Take(model.PageSize)
                                        .ToListAsync();

            foreach(var instance in dataResponse)
            {
                response.Results.Add(instance.Convert<NotebookDB.Common.Instance>());
            }

            return Ok(response);
        }

        [HttpPost]
        [Route("api/v1/notebook/{notebookId}/instance/{instanceId}/file")]
        [Authorize]
        async public Task<IActionResult> UploadFile(string notebookId, string instanceId, [FromBody] NotebookDB.Common.File model)
        {
            string userId = User.GetUserId();
            var canAccess = await CanAccessNotebook(userId, notebookId);
            if (!canAccess)
                return BadRequest();

            var canEdit = await CanEditNotebook(userId, notebookId);
            if (!canEdit)
                return BadRequest();

            var instance = await _db.Instances.Where(x => x.NotebookId == notebookId && x.Id == instanceId).FirstOrDefaultAsync();
            if (instance == null)
                return BadRequest("Instance not found");

            long maxAllowedSize = 100 * 1024 * 1024; //100mb
            if (model.FileSize > maxAllowedSize)
                return BadRequest("File must be less than 100mb.");

            try
            {
                NotebookDB.Database.File file = new NotebookDB.Database.File()
                {
                    Name = model.Name,
                    MimeType = model.MimeType,
                    InstanceId = instanceId,
                    FileSize = model.FileSize
                };

                _db.Files.Add(file);
                await _db.SaveChangesAsync();

                return Ok(new NotebookDB.Common.File()
                {
                    Id = file.Id,
                    Name = file.Name,
                    MimeType = file.MimeType,
                    FileSize = file.FileSize
                });
            }
            catch
            {
                return BadRequest();
            }
        }

        [HttpPut]
        [Route("api/v1/notebook/{notebookId}/instance/{instanceId}/file/{fileId}")]
        [Authorize]
        async public Task<IActionResult> UpdateFile(string notebookId, string instanceId, string fileId, [FromBody] NotebookDB.Common.FileRename model)
        {
            string userId = User.GetUserId();
            var canAccess = await CanAccessNotebook(userId, notebookId);
            if (!canAccess)
                return BadRequest();

            var canEdit = await CanEditNotebook(userId, notebookId);
            if (!canEdit)
                return BadRequest();

            var instance = await _db.Instances.Where(x => x.NotebookId == notebookId && x.Id == instanceId).FirstOrDefaultAsync();
            if (instance == null)
                return BadRequest("Instance not found");

            var file = await _db.Files.Where(x => x.InstanceId == instanceId && x.Id == fileId).FirstOrDefaultAsync();
            if(file == null)
                return BadRequest("File not found");

            if(string.IsNullOrEmpty(model.Name))
                model.Name = "[BLANK NAME]";

            file.Name = model.Name;

            await _db.SaveChangesAsync();

            return Ok();
        }

        [HttpPost]
        [Route("api/v1/notebook/{notebookId}/instance/{instanceId}/file/{fileId}")]
        [Authorize]
        public async Task<IActionResult> UploadShard([FromBody] NotebookDB.Common.FileShard content, string notebookId, string instanceId, string fileId)
        {
            string userId = User.GetUserId();
            var canAccess = await CanAccessNotebook(userId, notebookId);
            if (!canAccess)
                return BadRequest();

            var canEdit = await CanEditNotebook(userId, notebookId);
            if (!canEdit)
                return BadRequest();

            var instance = await _db.Instances.Where(x => x.NotebookId == notebookId && x.Id == instanceId).FirstOrDefaultAsync();
            if (instance == null)
                return BadRequest("Instance not found");

            var file = await _db.Files.Where(x => x.InstanceId == instanceId && x.Id == fileId).FirstOrDefaultAsync();
            if(file == null)
                return BadRequest("File not found");

            var fileShard = new NotebookDB.Database.FileShard
            {
                FileId = fileId,
                Index = content.Index,
                Data = content.Data
            };

            _db.FileShards.Add(fileShard);
            await _db.SaveChangesAsync();

            return Ok(new { fileShard.Index });
        }

        [HttpGet("api/v1/notebook/{notebookId}/instance/{instanceId}/streaming/{fileId}")]
        public async Task<IActionResult> GetVideo(string notebookId, string instanceId, string fileId)
        {
            string userId = User.GetUserId();
            var canAccess = await CanAccessNotebook(userId, notebookId);
            if (!canAccess)
                return BadRequest();

            var instance = await _db.Instances.Where(x => x.NotebookId == notebookId && x.Id == instanceId).FirstOrDefaultAsync();
            if (instance == null)
                return BadRequest("Instance not found");

            var file = await _db.Files.FirstOrDefaultAsync(f => f.Id == fileId && f.InstanceId == instanceId);
            if (file == null)
                return BadRequest("File not found");

            var rangeHeader = Request.Headers["Range"].ToString();
            var totalSize = await _db.FileShards.Where(fc => fc.FileId == fileId).SumAsync(fc => fc.Data.Length);
            
            if (!rangeHeader.StartsWith("bytes="))
            {
                return BadRequest("Invalid range header");
            }

            var range = rangeHeader.Substring("bytes=".Length).Split('-');
            if (range.Length < 1 || !long.TryParse(range[0], out long start))
            {
                return BadRequest("Invalid range header");
            }

            long? end = range.Length > 1 && long.TryParse(range[1], out long tempEnd) ? tempEnd : (long?)null;

            if (!end.HasValue || end.Value >= totalSize)
            {
                end = totalSize - 1;
            }

            if (start >= totalSize || start < 0 || end.Value < start)
            {
                return BadRequest("Invalid range values");
            }

            var shardsize = (await _db.FileShards.FirstOrDefaultAsync(fc => fc.FileId == fileId)).Data.Length;
            var startIndex = (int)(start / shardsize);
            var endIndex = (int)(end.Value / shardsize);

            var shards = await _db.FileShards
                                .Where(fc => fc.FileId == fileId && fc.Index >= startIndex && fc.Index <= endIndex)
                                .OrderBy(fc => fc.Index)
                                .ToListAsync();

            var data = new List<byte>();
            foreach (var shard in shards)
            {
                int relativeStart = shard.Index == startIndex ? (int)(start % shardsize) : 0;
                int relativeEnd = shard.Index == endIndex ? (int)(end.Value % shardsize) : shard.Data.Length - 1;
                int length = relativeEnd - relativeStart + 1;

                data.AddRange(shard.Data.Skip(relativeStart).Take(length));
            }

            var contentRange = $"bytes {start}-{start + data.Count - 1}/{totalSize}";

            Response.Headers.Append("Accept-Ranges", "bytes");
            Response.Headers.Append("Content-Range", contentRange);
            Response.StatusCode = 206; // Partial content
            Response.ContentType = "video/mp4";

            return File(data.ToArray(), "video/mp4", true);
        }


        [HttpGet]
        [Route("api/v1/notebook/{notebookId}/instance/{instanceId}/file/{fileId}/download")]
        [Authorize]
        async public Task<IActionResult> DownloadFile(string notebookId, string instanceId, string fileId)
        {
            string userId = User.GetUserId();
            var canAccess = await CanAccessNotebook(userId, notebookId);
            if (!canAccess)
                return BadRequest();

            var instance = await _db.Instances.Where(x => x.NotebookId == notebookId && x.Id == instanceId).FirstOrDefaultAsync();
            if (instance == null)
                return BadRequest("Instance not found");

            var file = await _db.Files.FirstOrDefaultAsync(f => f.Id == fileId && f.InstanceId == instanceId);
            if (file == null)
                return BadRequest("File not found");

            var fullData = await _db.FileShards
                        .Where(fc => fc.FileId == fileId)
                        .OrderBy(fc => fc.Index)
                        .ToListAsync(); // Load data into memory

            // Create a stream that will combine the byte arrays.
            var memoryStream = new MemoryStream();

            foreach (var byteArray in fullData)
            {
                memoryStream.Write(byteArray.Data, 0, byteArray.Data.Length);
            }

            // Reset the position to the beginning of the stream before returning it.
            memoryStream.Position = 0;

            // Return the stream as a file download.
            return File(memoryStream, "application/octet-stream", file.Name);
        }


        [HttpGet]
        [Route("api/v1/notebook/{notebookId}/instance/{instanceId}/file/{fileId}/view")]
        [Authorize]
        async public Task<IActionResult> ViewFile(string notebookId, string instanceId, string fileId)
        {
            string userId = User.GetUserId();
            var canAccess = await CanAccessNotebook(userId, notebookId);
            if (!canAccess)
                return BadRequest();

            var instance = await _db.Instances.Where(x => x.NotebookId == notebookId && x.Id == instanceId).FirstOrDefaultAsync();
            if (instance == null)
                return BadRequest("Instance not found");

            var file = await _db.Files.FirstOrDefaultAsync(f => f.Id == fileId && f.InstanceId == instanceId);
            if (file == null)
                return BadRequest("File not found");

            var fullData = await _db.FileShards
                        .Where(fc => fc.FileId == fileId)
                        .OrderBy(fc => fc.Index)
                        .ToListAsync(); // Load data into memory

            // Create a stream that will combine the byte arrays.
            var memoryStream = new MemoryStream();

            foreach (var byteArray in fullData)
            {
                memoryStream.Write(byteArray.Data, 0, byteArray.Data.Length);
            }

            // Reset the position to the beginning of the stream before returning it.
            memoryStream.Position = 0;

            // Set the Content-Disposition header to 'inline'
            var contentDisposition = new ContentDispositionHeaderValue("inline")
            {
                FileNameStar = file.Name // Use FileNameStar for UTF-8 support
            };
            Response.Headers[HeaderNames.ContentDisposition] = contentDisposition.ToString();

            // Return the file with the correct MIME type
            return new FileStreamResult(memoryStream, file.MimeType);
        }

        
        [HttpDelete]        
        [Route("api/v1/notebook/{notebookId}/instance/{instanceId}/file/{fileId}")]
        [Authorize]
        async public Task<IActionResult> DeleteFile(string notebookId, string instanceId, string fileId)
        {
            string userId = User.GetUserId();
            var canAccess = await CanAccessNotebook(userId, notebookId);
            if (!canAccess)
                return BadRequest();

            var canEdit = await CanEditNotebook(userId, notebookId);
            if (!canEdit)
                return BadRequest();

            var instance = await _db.Instances.Where(x => x.NotebookId == notebookId && x.Id == instanceId).FirstOrDefaultAsync();
            if (instance == null)
                return BadRequest("Instance not found");

            var shards = _db.FileShards.Where(x => x.FileId == fileId);
            _db.FileShards.RemoveRange(shards);

            var file = await _db.Files.FirstOrDefaultAsync(f => f.Id == fileId && f.InstanceId == instanceId);
            if (file == null)
                return BadRequest("File not found");

            _db.Files.RemoveRange(file);
            await _db.SaveChangesAsync();

            return Ok();
        }

        [HttpPost]
        [Route("api/v1/notebook/{sourceId}/template")]
        [Authorize(Roles = "Administrator")]
        public async Task<ActionResult> TemplateCreate([FromBody] NotebookDB.Common.TemplateCreate content, string sourceId)
        {
            var sourceNotebook = await _db.Notebooks
                .AsNoTracking()
                .Include(i => i.Fields)
                .Include(i => i.Sections)
                .FirstOrDefaultAsync(i => i.Id == sourceId);

            if(sourceNotebook == null)
                return BadRequest("Source Notebook not Found");

            //Add new template
            TemplateNotebook templateNotebook = new TemplateNotebook()
            {
                Name = sourceNotebook.Name,
                AllowAttachments = sourceNotebook.AllowAttachments,
                Icon = content.Icon
            };
            _db.TemplateNotebooks.Add(templateNotebook);
            await _db.SaveChangesAsync();

            foreach(var section in sourceNotebook.Sections)
            {
                TemplateSection templateSection = new TemplateSection()
                {
                    Name = section.Name,
                    Columns = section.Columns,
                    Order = section.Order,
                    NotebookId = templateNotebook.Id
                };
                _db.TemplateSections.Add(templateSection);
                await _db.SaveChangesAsync();

                foreach(var field in sourceNotebook.Fields.Where(x => x.SectionId == section.Id))
                {
                    TemplateField templateField = new TemplateField()
                    {
                        Name = field.Name,
                        Type = field.Type,
                        Column = field.Column,
                        Order = field.Order,
                        Options = field.Options,
                        Required = field.Required,
                        SearchOrder = field.SearchOrder,
                        SearchShow = field.SearchShow,
                        NotebookId = templateNotebook.Id,
                        SectionId = templateSection.Id
                    };
                    _db.TemplateFields.Add(templateField);
                    await _db.SaveChangesAsync();
                }
            }

            return Ok();
        }

        [HttpGet]
        [Route("api/v1/template")]
        [Authorize]
        public async Task<ActionResult> TemplatesGet()
        {
            var templates = await _db.TemplateNotebooks
                .AsNoTracking()
                .Select(x => new TemplateListItem() 
                {
                    Id = x.Id,
                    Name = x.Name,
                    Icon = x.Icon
                }).ToListAsync();

            return Ok(templates);
        }
        
        async private Task<bool> CanAccessNotebook(string userId, string notebookId, bool? isTypeAdmin = null)
        {
            //If we explicity require an admin to manage a folder, they must have the AllowFolderCreation as well.
            if (isTypeAdmin != null)
            {
                return await _db.NotebookAuthorizedUsers
                        .AnyAsync(o => o.NotebookId == notebookId &&
                                    o.UserId == userId &&
                                    o.IsNotebookAdmin == isTypeAdmin);
            }
            else
            {
                //Just check for what ever (true/false) is requested
                return await _db.NotebookAuthorizedUsers
                                .AnyAsync(o => o.NotebookId == notebookId &&
                                            o.UserId == userId);
            }
        }

        async private Task<bool> IsNotebookOwner(string userId, string notebookId)
        {
            //Just check for what ever (true/false) is requested
            return await _db.NotebookAuthorizedUsers
                            .AnyAsync(o => o.NotebookId == notebookId &&
                                        o.UserId == userId && o.IsNotebookOwner == true);
        }

        async private Task<bool> CanEditNotebook(string userId, string notebookId)
        {
            return await _db.NotebookAuthorizedUsers
                            .AnyAsync(o => o.NotebookId == notebookId &&
                                        o.UserId == userId &&
                                        o.AllowInstanceEdits == true);
        }

    }
}