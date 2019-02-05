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
using System.Diagnostics;
using System.Linq;
using HQ.Extensions.Identity.Configuration;
using HQ.Extensions.Identity.Extensions;
using HQ.Extensions.Identity.Models;
using HQ.Extensions.Identity.Security;
using HQ.Extensions.Identity.Services;
using HQ.Extensions.Identity.Validators;
using HQ.Common.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HQ.Extensions.Identity
{
    public static class Add
    {
        private static readonly Action<IdentityOptionsExtended> Defaults;

        static Add()
        {
            Defaults = x =>
            {
                // Sensible defaults not set by ASP.NET Core Identity:
                x.Stores.ProtectPersonalData = true;
                x.Stores.MaxLengthForKeys = 128;
                x.User.RequireUniqueEmail = true;
            };
        }


        public static IdentityBuilder AddIdentityExtended<TUser, TRole>(this IServiceCollection services, IConfiguration configuration)
            where TUser : IdentityUserExtended
            where TRole : IdentityRoleExtended
        {
            AddIdentityPreamble(services);

            return services.AddIdentityCoreExtended<TUser, TRole>(configuration);
        }

        public static IdentityBuilder AddIdentityExtended<TUser, TRole>(this IServiceCollection services,
            Action<IdentityOptionsExtended> setupIdentityExtended = null)
            where TUser : IdentityUserExtended
            where TRole : IdentityRoleExtended
        {
            AddIdentityPreamble(services);

            return services.AddIdentityCoreExtended<TUser, TRole>(setupIdentityExtended: o =>
            {
                setupIdentityExtended?.Invoke(o);
            });
        }

        private static void AddIdentityPreamble(IServiceCollection services)
        {
            var authBuilder = services.AddAuthentication(o =>
            {
                o.DefaultScheme = IdentityConstants.ApplicationScheme;
                o.DefaultSignInScheme = IdentityConstants.ExternalScheme;
            });

            var cookiesBuilder = authBuilder.AddIdentityCookies(o => { });
        }

        public static IdentityBuilder AddIdentityCoreExtended<TUser, TRole>(this IServiceCollection services,
            IConfiguration configuration)
            where TUser : IdentityUserExtended
            where TRole : IdentityRoleExtended
        {
            services.Configure<IdentityOptions>(configuration);
            services.Configure<IdentityOptionsExtended>(configuration);

            return services.AddIdentityCoreExtended<TUser, TRole>(configuration.Bind, configuration.Bind);
        }

        public static IdentityBuilder AddIdentityCoreExtended<TUser, TRole>(this IServiceCollection services,
            Action<IdentityOptions> setupIdentity = null,
            Action<IdentityOptionsExtended> setupIdentityExtended = null)
            where TUser : IdentityUserExtended
            where TRole : IdentityRoleExtended
        {
            /*
                services.AddOptions().AddLogging();
                services.TryAddScoped<IUserValidator<TUser>, UserValidator<TUser>>();
                services.TryAddScoped<IPasswordValidator<TUser>, PasswordValidator<TUser>>();
                services.TryAddScoped<IPasswordHasher<TUser>, PasswordHasher<TUser>>();
                services.TryAddScoped<ILookupNormalizer, UpperInvariantLookupNormalizer>();
                services.TryAddScoped<IdentityErrorDescriber>();
                services.TryAddScoped<IUserClaimsPrincipalFactory<TUser>, UserClaimsPrincipalFactory<TUser>>();
                services.TryAddScoped<UserManager<TUser>, UserManager<TUser>>();
                if (setupAction != null)
                  services.Configure<IdentityOptions>(setupAction);
                return new IdentityBuilder(typeof (TUser), services);
             */
            var identityBuilder = services.AddIdentityCore<TUser>(o =>
            {
                var x = new IdentityOptionsExtended(o);
                Defaults(x);
                setupIdentityExtended?.Invoke(x);
                o.Apply(x);
            });

            if (setupIdentityExtended != null)
                services.Configure(setupIdentityExtended);

            if (setupIdentity != null)
                services.Configure(setupIdentity);

            identityBuilder.AddDefaultTokenProviders();
            identityBuilder.AddPersonalDataProtection<NoLookupProtector, NoLookupProtectorKeyRing>();
            identityBuilder.Services.AddSingleton<IPersonalDataProtector, DefaultPersonalDataProtector>();

            services.AddScoped<IEmailValidator<TUser>, DefaultEmailValidator<TUser>>();
            services.AddScoped<IPhoneNumberValidator<TUser>, DefaultPhoneNumberValidator<TUser>>();
            services.AddScoped<IUsernameValidator<TUser>, DefaultUsernameValidator<TUser>>();

            var validator = services.SingleOrDefault(x => x.ServiceType == typeof(IUserValidator<TUser>));
            var removed = services.Remove(validator);
            Debug.Assert(validator != null);
            Debug.Assert(removed);

            services.AddScoped<IUserValidator<TUser>, UserValidatorExtended<TUser>>();

            services.AddScoped<IUserService<TUser>, UserService<TUser>>();
            services.AddScoped<IRoleService<TRole>, RoleService<TRole>>();
            services.AddScoped<ISignInService<TUser>, SignInService<TUser>>();

            services.AddSingleton<IServerTimestampService, LocalServerTimestampService>();

            return identityBuilder;
        }
    }
}
