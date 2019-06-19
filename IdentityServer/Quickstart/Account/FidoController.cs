using System;
using System.Linq;
using System.Threading.Tasks;
using Fido2NetLib;
using Fido2NetLib.Objects;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using static Fido2NetLib.Fido2;

namespace IdentityServer.Quickstart.Account
{
	[Route("api/[controller]")]
	[ApiController]
	public class FidoController : ControllerBase
	{
		private Fido2 _lib;
		private IMetadataService _mds;
		private string _origin;

		public FidoController(IConfiguration config)
		{
			var MDSAccessKey = config["fido2:MDSAccessKey"];
			_mds = string.IsNullOrEmpty(MDSAccessKey) ? null : MDSMetadata.Instance(MDSAccessKey, config["fido2:MDSCacheDirPath"]);
			if (null != _mds)
			{
				if (false == _mds.IsInitialized())
					_mds.Initialize().Wait();
			}
			_origin = config["fido2:origin"];
			_lib = new Fido2(new Fido2.Configuration
			{
				ServerDomain = config["fido2:serverDomain"],
				ServerName = "Fido2 test",
				Origin = _origin,
				// Only create and use Metadataservice if we have an acesskey
				MetadataService = _mds,
				TimestampDriftTolerance = config.GetValue<int>("fido2:TimestampDriftTolerance")
			});
		}























































		/*
		 *                 _     _            
		 *                (_)   | |           
		 *  _ __ ___  __ _ _ ___| |_ ___ _ __ 
		 * | '__/ _ \/ _` | / __| __/ _ \ '__|
		 * | | |  __/ (_| | \__ \ ||  __/ |   
		 * |_|  \___|\__, |_|___/\__\___|_|   
		 *            __/ |                   
		 *           |___/                    
		 */

		[HttpPost]
		[Route("/makeCredentialOptions")]
		public JsonResult MakeCredentialOptions([FromForm] string username, [FromForm] string attType, [FromForm] string authType, [FromForm] bool requireResidentKey, [FromForm] string userVerification)
		{
			try
			{
				// 1. Get user from DB by username (in our example, auto create missing users)
				var user = DemoStorage.GetOrAddUser(username, () => new User
				{
					DisplayName = "Display " + username,
					Name = username,
					Id = Encoding.UTF8.GetBytes(username) // byte representation of userID is required
				});

				// 2. Get user existing keys by username
				var existingKeys = DemoStorage.GetCredentialsByUser(user).Select(c => c.Descriptor).ToList();

				// 3. Create options
				var authenticatorSelection = new AuthenticatorSelection
				{
					RequireResidentKey = requireResidentKey,
					UserVerification = userVerification.ToEnum<UserVerificationRequirement>()
				};

				if (!string.IsNullOrEmpty(authType))
					authenticatorSelection.AuthenticatorAttachment = authType.ToEnum<AuthenticatorAttachment>();

				var exts = new AuthenticationExtensionsClientInputs() { Extensions = true, UserVerificationIndex = true, Location = true, UserVerificationMethod = true, BiometricAuthenticatorPerformanceBounds = new AuthenticatorBiometricPerfBounds { FAR = float.MaxValue, FRR = float.MaxValue } };

				var options = _lib.RequestNewCredential(user, existingKeys, authenticatorSelection, attType.ToEnum<AttestationConveyancePreference>(), exts);

				// 4. Temporarily store options, session/in-memory cache/redis/db
				HttpContext.Session.SetString("fido2.attestationOptions", options.ToJson());

				// 5. return options to client
				return Json(options);
			}
			catch (Exception e)
			{
				return Json(new CredentialCreateOptions { Status = "error", ErrorMessage = FormatException(e) });
			}
		}

		[HttpPost]
		[Route("/makeCredential")]
		public async Task<ActionResult> MakeCredential([FromBody] AuthenticatorAttestationRawResponse attestationResponse)
		{
			try
			{
				// 1. get the options we sent the client
				var jsonOptions = HttpContext.Session.GetString("fido2.attestationOptions");
				var options = CredentialCreateOptions.FromJson(jsonOptions);

				// 2. Create callback so that lib can verify credential id is unique to this user
				IsCredentialIdUniqueToUserAsyncDelegate callback = async (IsCredentialIdUniqueToUserParams args) =>
				{
					var users = await DemoStorage.GetUsersByCredentialIdAsync(args.CredentialId);
					if (users.Count > 0) return false;

					return true;
				};

				// 2. Verify and make the credentials
				var success = await _lib.MakeNewCredentialAsync(attestationResponse, options, callback);

				// 3. Store the credentials in db
				DemoStorage.AddCredentialToUser(options.User, new StoredCredential
				{
					Descriptor = new PublicKeyCredentialDescriptor(success.Result.CredentialId),
					PublicKey = success.Result.PublicKey,
					UserHandle = success.Result.User.Id,
					SignatureCounter = success.Result.Counter,
					CredType = success.Result.CredType,
					RegDate = DateTime.Now,
					AaGuid = success.Result.Aaguid
				});

				// 4. return "ok" to the client
				return Ok(success);
			}
			catch (Exception e)
			{
				return BadRequest(new CredentialMakeResult { Status = "error", ErrorMessage = FormatException(e) });
			}
		}












































		/*
		 *  _             _       
		 * | |           (_)      
		 * | | ___   __ _ _ _ __  
		 * | |/ _ \ / _` | | '_ \ 
		 * | | (_) | (_| | | | | |
		 * |_|\___/ \__, |_|_| |_|
		 *           __/ |        
		 *          |___/         
		 */

		[HttpPost]
		[Route("/assertionOptions")]
		public ActionResult AssertionOptionsPost([FromForm] string username, [FromForm] string userVerification)
		{
			try
			{
				// 1. Get user from DB
				var user = DemoStorage.GetUser(username);
				if (user == null) throw new ArgumentException("Username was not registered");

				// 2. Get registered credentials from database
				var existingCredentials = DemoStorage.GetCredentialsByUser(user).Select(c => c.Descriptor).ToList();

				var exts = new AuthenticationExtensionsClientInputs() { SimpleTransactionAuthorization = "FIDO", GenericTransactionAuthorization = new TxAuthGenericArg { ContentType = "text/plain", Content = new byte[] { 0x46, 0x49, 0x44, 0x4F } }, UserVerificationIndex = true, Location = true, UserVerificationMethod = true };

				// 3. Create options
				var uv = string.IsNullOrEmpty(userVerification) ? UserVerificationRequirement.Discouraged : userVerification.ToEnum<UserVerificationRequirement>();
				var options = _lib.GetAssertionOptions(
					existingCredentials,
					uv,
					exts
				);

				// 4. Temporarily store options, session/in-memory cache/redis/db
				HttpContext.Session.SetString("fido2.assertionOptions", options.ToJson());

				// 5. Return options to client
				return Ok(options);
			}

			catch (Exception e)
			{
				return BadRequest(new AssertionOptions { Status = "error", ErrorMessage = FormatException(e) });
			}
		}

		[HttpPost]
		[Route("/makeAssertion")]
		public async Task<ActionResult> MakeAssertion([FromBody] AuthenticatorAssertionRawResponse clientResponse)
		{
			try
			{
				// 1. Get the assertion options we sent the client
				var jsonOptions = HttpContext.Session.GetString("fido2.assertionOptions");
				var options = AssertionOptions.FromJson(jsonOptions);

				// 2. Get registered credential from database
				var creds = DemoStorage.GetCredentialById(clientResponse.Id);

				// 3. Get credential counter from database
				var storedCounter = creds.SignatureCounter;

				// 4. Create callback to check if userhandle owns the credentialId
				IsUserHandleOwnerOfCredentialIdAsync callback = async (args) =>
				{
					var storedCreds = await DemoStorage.GetCredentialsByUserHandleAsync(args.UserHandle);
					return storedCreds.Exists(c => c.Descriptor.Id.SequenceEqual(args.CredentialId));
				};

				// 5. Make the assertion
				var res = await _lib.MakeAssertionAsync(clientResponse, options, creds.PublicKey, storedCounter, callback);

				// 6. Store the updated counter
				DemoStorage.UpdateCounter(res.CredentialId, res.Counter);

				// 7. return OK to client
				return Ok(res);
			}
			catch (Exception e)
			{
				return BadRequest(new AssertionVerificationResult { Status = "error", ErrorMessage = FormatException(e) });
			}
		}




















































































		private string FormatException(Exception e)
		{
			return string.Format("{0}{1}", e.Message, e.InnerException != null ? " (" + e.InnerException.Message + ")" : "");
		}
	}
}