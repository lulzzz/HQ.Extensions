﻿// Copyright (c) HQ.IO Corporation. All rights reserved.
// Licensed under the Reciprocal Public License, Version 1.5. See LICENSE.md in the project root for license terms.

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using HQ.Cohort.Configuration;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace HQ.Cohort.Validators
{
	/// <summary>
	///     A default validator that contains equivalent validation logic as ASP.NET Core Identity, with the exception of
	///     optionally allowing registration without an email address. Any attempts to change the email address after
	///     the fact should still validate using the default logic.
	/// </summary>
	/// <typeparam name="TUser"></typeparam>
	public class DefaultEmailValidator<TUser> : IEmailValidator<TUser> where TUser : class
	{
		// ReSharper disable once StaticMemberInGenericType
		private static readonly EmailAddressAttribute EmailAddressAttribute = new EmailAddressAttribute();

		private readonly IdentityErrorDescriber _describer;
		private readonly IOptions<IdentityOptionsExtended> _options;

		public DefaultEmailValidator(IdentityErrorDescriber describer, IOptions<IdentityOptionsExtended> options)
		{
			_describer = describer;
			_options = options;
		}

		public async Task ValidateAsync(UserManager<TUser> manager, TUser user, ICollection<IdentityError> errors)
		{
			var email = await manager.GetEmailAsync(user);

			if (await CanRegisterWithoutEmail(manager, user, email))
				return;

			if (string.IsNullOrWhiteSpace(email) || !EmailAddressAttribute.IsValid(email))
			{
				errors.Add(_describer.InvalidEmail(email));
				return;
			}

			var exists = await manager.FindByEmailAsync(email);
			if (exists == null)
				return;

			if (manager.Options.User.RequireUniqueEmail &&
			    !string.Equals(await manager.GetUserIdAsync(exists), await manager.GetUserIdAsync(user)))
				errors.Add(_describer.DuplicateEmail(email));
		}

		private async Task<bool> CanRegisterWithoutEmail(UserManager<TUser> manager, TUser user, string email)
		{
			return !_options.Value.User.RequireEmailOnRegister &&
			       string.IsNullOrWhiteSpace(email) &&
			       await manager.GetUserIdAsync(user) == null;
		}
	}
}