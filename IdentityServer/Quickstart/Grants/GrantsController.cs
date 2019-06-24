// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fido2NetLib;
using Fido2NetLib.Objects;
using IdentityServer4.Events;
using IdentityServer4.Extensions;
using IdentityServer4.Services;
using IdentityServer4.Stores;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace IdentityServer4.Quickstart.UI
{
	/// <summary>
	/// This sample controller allows a user to revoke grants given to clients
	/// </summary>
	[SecurityHeaders]
	[Authorize]
	public class GrantsController : Controller
	{
		private readonly IIdentityServerInteractionService _interaction;
		private readonly IClientStore _clients;
		private readonly IResourceStore _resources;
		private readonly IEventService _events;
		private readonly IMetadataService _mds;

		public GrantsController(IIdentityServerInteractionService interaction,
			IClientStore clients,
			IResourceStore resources,
			IEventService events,
			IConfiguration config)
		{
			_interaction = interaction;
			_clients = clients;
			_resources = resources;
			_events = events;



			//var MDSAccessKey = config["fido2:MDSAccessKey"];
			//_mds = string.IsNullOrEmpty(MDSAccessKey) ? null : MDSMetadata.Instance(MDSAccessKey, config["fido2:MDSCacheDirPath"]);
			//if (null != _mds)
			//{
			//	if (false == _mds.IsInitialized())
			//		_mds.Initialize().Wait();
			//}
		}

		/// <summary>
		/// Show list of grants
		/// </summary>
		[HttpGet]
		public async Task<IActionResult> Index()
		{
			return View("Index", await BuildViewModelAsync());
		}

		/// <summary>
		/// Handle postback to revoke a client
		/// </summary>
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Revoke(string clientId)
		{
			await _interaction.RevokeUserConsentAsync(clientId);
			await _events.RaiseAsync(new GrantsRevokedEvent(User.GetSubjectId(), clientId));

			return RedirectToAction("Index");
		}

		private async Task<GrantsViewModel> BuildViewModelAsync()
		{
			var grants = await _interaction.GetAllUserConsentsAsync();

			var list = new List<GrantViewModel>();
			foreach (var grant in grants)
			{
				var client = await _clients.FindClientByIdAsync(grant.ClientId);
				if (client != null)
				{
					var resources = await _resources.FindResourcesByScopeAsync(grant.Scopes);

					var item = new GrantViewModel()
					{
						ClientId = client.ClientId,
						ClientName = client.ClientName ?? client.ClientId,
						ClientLogoUrl = client.LogoUri,
						ClientUrl = client.ClientUri,
						Created = grant.CreationTime,
						Expires = grant.Expiration,
						IdentityGrantNames = resources.IdentityResources.Select(x => x.DisplayName ?? x.Name).ToArray(),
						ApiGrantNames = resources.ApiResources.Select(x => x.DisplayName ?? x.Name).ToArray()
					};

					list.Add(item);
				}
			}

			return new GrantsViewModel
			{
				Grants = list,
				Fidos = GetFidoCredentials(),
			};
		}






		private FidoCredentialDto[] GetFidoCredentials()
		{
			var subjectId = User.Claims.FirstOrDefault(c => c.Type == "sub")?.Value;

			// 1. Get user from DB
			var dbUser = TestUsers.Users.FirstOrDefault(u => string.Equals(u.SubjectId, subjectId, StringComparison.InvariantCultureIgnoreCase));
			if (dbUser == null)
			{
				throw new Exception("no such user");
			}

			// 2. Get registered credentials from database
			var existingCredentials = TestUsers.FidoCredentials.Where(c => string.Equals(c.SubjectId, subjectId, StringComparison.InvariantCultureIgnoreCase)).Select(c => c).ToList();
			var resultDtos = new List<FidoCredentialDto>();
			foreach (var cred in existingCredentials)
			{
				var coseKey = PeterO.Cbor.CBORObject.DecodeFromBytes(cred.PublicKey);
				var kty = coseKey[PeterO.Cbor.CBORObject.FromObject(COSE.KeyCommonParameters.kty)].AsInt32();
				var desc = "";
				var icon = "";
				try
				{
					var entry = _mds.GetEntry(cred.AaGuid);
					desc = entry.MetadataStatement.Description.ToString();
					icon = entry.MetadataStatement.Icon.ToString();
				}
				catch { }

				var resultDto = new FidoCredentialDto
				{
					AttestationType = cred.CredType,
					CreateDate = cred.RegDate,
					Counter = cred.SignatureCounter.ToString(),
					AAGUID = cred.AaGuid.ToString(),
					Description = desc,
				};
				switch (kty)
				{
					case (int)COSE.KeyTypes.OKP:
						{
							var X = coseKey[PeterO.Cbor.CBORObject.FromObject(COSE.KeyTypeParameters.x)].GetByteString();
							resultDto.PublicKey = $"X: {BitConverter.ToString(X).Replace("-", "")}";
							break;
						}
					case (int)COSE.KeyTypes.EC2:
						{
							var X = coseKey[PeterO.Cbor.CBORObject.FromObject(COSE.KeyTypeParameters.x)].GetByteString();
							var Y = coseKey[PeterO.Cbor.CBORObject.FromObject(COSE.KeyTypeParameters.y)].GetByteString();
							resultDto.PublicKey = $"X: {BitConverter.ToString(X).Replace("-", "")}; Y: {BitConverter.ToString(Y).Replace("-", "")}";
							break;
						}
					case (int)COSE.KeyTypes.RSA:
						{
							var modulus = coseKey[PeterO.Cbor.CBORObject.FromObject(COSE.KeyTypeParameters.n)].GetByteString();
							var exponent = coseKey[PeterO.Cbor.CBORObject.FromObject(COSE.KeyTypeParameters.e)].GetByteString();
							resultDto.PublicKey = $"Modulus: {BitConverter.ToString(modulus).Replace("-", "")}; Exponent: {BitConverter.ToString(exponent).Replace("-", "")}";
							break;
						}
					default:
						{
							throw new Exception(string.Format("Missing or unknown keytype {0}", kty.ToString()));
						}
				}

				resultDtos.Add(resultDto);
			}

			return resultDtos.ToArray();
		}
	}
}