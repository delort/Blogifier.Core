﻿using Blogifier.Core.Data.Domain;
using Blogifier.Core.Data.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Blogifier.Core.Data.Repositories
{
    public class CustomRepository : Repository<CustomField>, ICustomRepository
    {
        BlogifierDbContext _db;

        public CustomRepository(BlogifierDbContext db) : base(db)
        {
            _db = db;
        }

        public Task<Dictionary<string, string>> GetCustomFields(CustomType customType, int parentId)
        {
            var fields = new Dictionary<string, string>();
            IQueryable<CustomField> dbFields;

            if(parentId == 0)
            {
                dbFields = _db.CustomFields.Where(f => f.CustomType == customType).OrderBy(f => f.Title);
            }
            else
            {
                dbFields = _db.CustomFields.Where(f => f.CustomType == customType && f.ParentId == parentId).OrderBy(f => f.Title);
            }

            if (dbFields != null && dbFields.Count() > 0)
            {
                foreach (var field in dbFields)
                {
                    fields.Add(field.CustomKey, field.CustomValue);
                }
            }
            return Task.Run(() => fields);
        }

        public Task SetCustomField(CustomType customType, int parentId, string key, string value)
        {
            var dbField = _db.CustomFields
                .Where(f => f.CustomType == customType && f.ParentId == parentId && f.CustomKey == key)
                .FirstOrDefault();

            if (dbField != null)
            {
                dbField.CustomValue = value;
            }
            else
            {
                _db.CustomFields.Add(new CustomField
                {
                    CustomKey = key,
                    CustomValue = value,
                    Title = key,
                    CustomType = customType,
                    ParentId = parentId
                });
            }
            return _db.SaveChangesAsync();
        }
    }
}