#region LICENSE

// Unless explicitly acquired and licensed from Licensor under another
// license, the contents of this file are subject to the Reciprocal Public
// License ("RPL") Version 1.5, or subsequent versions as allowed by the RPL,
// and You may not copy or use this file in either source code or executable
// form, except in compliance with the terms and conditions of the RPL.
// 
// All software distributed under the RPL is provided strictly on an "AS
// IS" basis, WITHOUT WARRANTY OF ANY KIND, EITHER EXPRESS OR IMPLIED, AND
// LICENSOR HEREBY DISCLAIMS ALL SUCH WARRANTIES, INCLUDING WITHOUT
// LIMITATION, ANY WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR
// PURPOSE, QUIET ENJOYMENT, OR NON-INFRINGEMENT. See the RPL for specific
// language governing rights and limitations under the RPL.

#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Security.Claims;
using System.Threading.Tasks;
using HQ.Extensions.Identity.Extensions;
using HQ.Extensions.Identity.Models;
using HQ.Rosetta;
using HQ.Rosetta.Queryable;
using HQ.Strings;
using Microsoft.AspNetCore.Identity;

namespace HQ.Extensions.Identity.Services
{
    public class RoleService<TRole> : IRoleService<TRole> where TRole : IdentityRoleExtended
    {
        private readonly RoleManager<TRole> _roleManager;
        private readonly IRoleStoreExtended<TRole> _roleStore;
        private readonly IQueryableProvider<TRole> _queryableProvider;
        
        public RoleService(RoleManager<TRole> roleManager, IRoleStoreExtended<TRole> roleStore, IQueryableProvider<TRole> queryableProvider)
        {
            _roleManager = roleManager;
            _roleStore = roleStore;
            _queryableProvider = queryableProvider;
        }

        public IQueryable<TRole> Roles => _roleManager.Roles;

        public Task<Operation<IEnumerable<TRole>>> GetAsync()
        {
            var all = _queryableProvider.SafeAll ?? Roles;
            return Task.FromResult(new Operation<IEnumerable<TRole>>(all));
        }

        public async Task<Operation<TRole>> CreateAsync(CreateRoleModel model)
        {
            var role = (TRole)FormatterServices.GetUninitializedObject(typeof(TRole));
            role.Name = model.Name;
            role.ConcurrencyStamp = model.ConcurrencyStamp ?? $"{Guid.NewGuid()}";
            role.NormalizedName = model.Name?.ToUpperInvariant();

            var result = await _roleManager.CreateAsync(role);
            return result.ToOperation(role);
        }

        public async Task<Operation> DeleteAsync(string id)
        {
            var operation = await FindByIdAsync(id);
            if (!operation.Succeeded)
                return operation;

            var deleted = await _roleManager.DeleteAsync(operation.Data);
            return deleted.ToOperation();
        }

        #region Find

        public async Task<Operation<TRole>> FindByIdAsync(string id)
        {
            var role = await _roleManager.FindByIdAsync(id);
            return role == null
                ? new Operation<TRole>(new Error(ErrorEvents.NotFound, ErrorStrings.Cohort_RoleNotFound, HttpStatusCode.NotFound))
                : new Operation<TRole>(role);
        }

        public async Task<Operation<TRole>> FindByNameAsync(string roleName)
        {
            var role = await _roleManager.FindByNameAsync(roleName);
            return role == null
                ? new Operation<TRole>(new Error(ErrorEvents.NotFound, ErrorStrings.Cohort_RoleNotFound, HttpStatusCode.NotFound))
                : new Operation<TRole>(role);
        }

        #endregion

        #region Claims

        public async Task<Operation<IList<Claim>>> GetClaimsAsync(TRole role)
        {
            var claims = await _roleManager.GetClaimsAsync(role);
            return new Operation<IList<Claim>>(claims);
        }

        public async Task<Operation<IList<Claim>>> GetAllRoleClaimsAsync()
        {
            var claims = await _roleStore.GetAllRoleClaimsAsync();
            return new Operation<IList<Claim>>(claims);
        }

        public async Task<Operation> AddClaimAsync(TRole role, Claim claim)
        {
            var result = await _roleManager.AddClaimAsync(role, claim);
            return result.ToOperation();
        }

        public async Task<Operation> RemoveClaimAsync(TRole role, Claim claim)
        {
            var result = await _roleManager.RemoveClaimAsync(role, claim);
            return result.ToOperation();
        }

        #endregion
    }
}
