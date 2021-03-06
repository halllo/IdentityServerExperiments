﻿// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fido2NetLib;
using IdentityServer4.Events;
using IdentityServer4.Extensions;
using IdentityServer4.Services;
using IdentityServer4.Stores;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace IdentityServerHost.Quickstart.UI
{
    /// <summary>
    /// This sample controller allows a user to revoke grants given to clients
    /// </summary>
    [SecurityHeaders]
    [Authorize("default")]
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
            var grants = await _interaction.GetAllUserGrantsAsync();

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
                        Description = grant.Description,
                        Created = grant.CreationTime,
                        Expires = grant.Expiration,
                        IdentityGrantNames = resources.IdentityResources.Select(x => x.DisplayName ?? x.Name).ToArray(),
                        ApiGrantNames = resources.ApiScopes.Select(x => x.DisplayName ?? x.Name).ToArray()
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
                return new FidoCredentialDto[0];
                //throw new Exception("no such user");
            }

            // 2. Get registered credentials from database
            var existingCredentials = TestUsers.FidoCredentials.Where(c => string.Equals(c.SubjectId, subjectId, StringComparison.InvariantCultureIgnoreCase)).Select(c => c).ToList();
            var resultDtos = new List<FidoCredentialDto>();
            foreach (var cred in existingCredentials)
            {
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
                    PublicKey = Convert.ToBase64String(cred.PublicKey)
                };

                resultDtos.Add(resultDto);
            }

            return resultDtos.ToArray();
        }
    }
}
