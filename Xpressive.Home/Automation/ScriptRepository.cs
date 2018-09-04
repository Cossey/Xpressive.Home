﻿using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using NPoco;
using Should.Core.Assertions;
using Xpressive.Home.Contracts.Automation;

namespace Xpressive.Home.Automation
{
    internal class ScriptRepository : IScriptRepository
    {
        private readonly DbConnection _dbConnection;

        public ScriptRepository(DbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public async Task SaveAsync(Script script)
        {
            Assert.NotNull(script);

            if (Guid.Empty.Equals(script.Id))
            {
                await InsertAsync(script);
            }
            else
            {
                await UpdateAsync(script);
            }
        }

        public async Task<Script> GetAsync(Guid id)
        {
            using (var database = new Database(_dbConnection))
            {
                return await database.SingleOrDefaultByIdAsync<Script>(id);
            }
        }

        public async Task<IEnumerable<Script>> GetAsync(IEnumerable<Guid> ids)
        {
            var scripts = await GetAsync();
            var scriptIds = new HashSet<Guid>(ids);
            return scripts.Where(s => scriptIds.Contains(s.Id));
        }

        public async Task<IEnumerable<Script>> GetAsync()
        {
            using (var database = new Database(_dbConnection))
            {
                return await database.FetchAsync<Script>("select * from Script");
            }
        }

        public async Task EnableAsync(Script script)
        {
            Assert.NotNull(script);

            using (var database = new Database(_dbConnection))
            {
                script.IsEnabled = true;
                await database.UpdateAsync(script, new[] { "IsEnabled" });
            }
        }

        public async Task DisableAsync(Script script)
        {
            Assert.NotNull(script);

            using (var database = new Database(_dbConnection))
            {
                script.IsEnabled = false;
                await database.UpdateAsync(script, new[] { "IsEnabled" });
            }
        }

        public async Task DeleteAsync(Guid id)
        {
            using (var database = new Database(_dbConnection))
            {
                var dto = await database.SingleOrDefaultByIdAsync<Script>(id);

                if (dto != null)
                {
                    await database.DeleteAsync(dto);
                }
            }
        }

        public async Task DeleteAsync(Script script)
        {
            Assert.NotNull(script);

            using (var database = new Database(_dbConnection))
            {
                await database.DeleteAsync(script);
            }
        }

        private async Task InsertAsync(Script script)
        {
            Assert.NotNull(script);

            script.Id = Guid.NewGuid();

            using (var database = new Database(_dbConnection))
            {
                await database.InsertAsync(script);
            }
        }

        private async Task UpdateAsync(Script script)
        {
            Assert.NotNull(script);

            using (var database = new Database(_dbConnection))
            {
                await database.UpdateAsync(script, new[] { "Name", "JavaScript", "IsEnabled" });
            }
        }
    }
}
