using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fido2NetLib;
using Fido2NetLib.Objects;
using IdentityServer4.Quickstart.UI;
using Microsoft.AspNetCore.Authorization;
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
		[Authorize]
		[Route("/makeCredentialOptions")]
		public ActionResult MakeCredentialOptions([FromForm] string attType, [FromForm] string authType, [FromForm] bool requireResidentKey, [FromForm] string userVerification)
		{
			var subjectId = User.Claims.FirstOrDefault(c => c.Type == "sub")?.Value;
			try
			{
				// 1. Get user from DB by username
				var dbUser = TestUsers.Users.FirstOrDefault(u => string.Equals(u.SubjectId, subjectId, StringComparison.InvariantCultureIgnoreCase));
				if (dbUser == null)
				{
					return BadRequest("no such user");
				}
				var user = new Fido2NetLib.User
				{
					DisplayName = dbUser.Username,
					Name = dbUser.Username,
					Id = Encoding.UTF8.GetBytes(dbUser.Username)
				};

				// 2. Get user existing keys by username
				List<PublicKeyCredentialDescriptor> existingKeys = TestUsers.FidoCredentials.Where(c => c.UserId.SequenceEqual(user.Id)).Select(c => c.Descriptor).ToList();

				// 3. Create options
				var options = _lib.RequestNewCredential(
					user: user,
					excludeCredentials: existingKeys,
					authenticatorSelection: new AuthenticatorSelection
					{
						RequireResidentKey = requireResidentKey,
						UserVerification = userVerification.ToEnum<UserVerificationRequirement>(),
						AuthenticatorAttachment = string.IsNullOrEmpty(authType) ? (AuthenticatorAttachment?)null : authType.ToEnum<AuthenticatorAttachment>(),
					},
					attestationPreference: attType.ToEnum<AttestationConveyancePreference>(),
					extensions: new AuthenticationExtensionsClientInputs()
					{
						Extensions = true,
						UserVerificationIndex = true,
						Location = true,
						UserVerificationMethod = true,
						BiometricAuthenticatorPerformanceBounds = new AuthenticatorBiometricPerfBounds { FAR = float.MaxValue, FRR = float.MaxValue }
					});

				// 4. Temporarily store options, session/in-memory cache/redis/db
				var sessionId = IdentityModel.CryptoRandom.CreateRandomKeyString(64);
				TestUsers.FidoAttestationOptions[sessionId] = options.ToJson();

				// 5. return options to client
				return Ok(new MakeCredentialOptionsResponse
				{
					SessionId = sessionId,
					Options = options,
				});
			}
			catch (Exception e)
			{
				return BadRequest(new CredentialCreateOptions { Status = "error", ErrorMessage = FormatException(e) });
			}
		}
		public class MakeCredentialOptionsResponse
		{
			public string SessionId { get; set; }
			public CredentialCreateOptions Options { get; set; }
		}



		[HttpPost]
		[Authorize]
		[Route("/makeCredential")]
		public async Task<ActionResult> MakeCredential([FromBody] MakeCredentialRequest request)
		{
			var subjectId = User.Claims.FirstOrDefault(c => c.Type == "sub")?.Value;
			try
			{
				// 1. get the options we sent the client
				var jsonOptions = TestUsers.FidoAttestationOptions[request.SessionId];
				var options = CredentialCreateOptions.FromJson(jsonOptions);

				// 2. Verify and make the credentials
				var success = await _lib.MakeNewCredentialAsync(
					attestationResponse: request.AttestationResponse,
					origChallenge: options,
					isCredentialIdUniqueToUser: async (IsCredentialIdUniqueToUserParams args) =>
					{
						var users = TestUsers.FidoCredentials.Where(c => c.Descriptor.Id.SequenceEqual(args.CredentialId)).ToList();
						if (users.Count > 0) return false;

						return true;
					});

				// 3. Store the credentials in db
				TestUsers.FidoCredentials.Add(new TestUsers.StoredFidoCredential
				{
					UserId = options.User.Id,
					SubjectId = subjectId,
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
		public class MakeCredentialRequest
		{
			public string SessionId { get; set; }
			public AuthenticatorAttestationRawResponse AttestationResponse { get; set; }
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
				var user = TestUsers.Users.FirstOrDefault(u => string.Equals(u.Username, username, StringComparison.InvariantCultureIgnoreCase));
				if (user == null) return BadRequest("Username was not registered");//leaks information about registered users :(

				// 2. Get registered credentials from database
				IEnumerable<PublicKeyCredentialDescriptor> existingCredentials = TestUsers.FidoCredentials.Where(c => c.UserId.SequenceEqual(Encoding.UTF8.GetBytes(user.Username))).Select(c => c.Descriptor).ToList();

				// 3. Create options
				var options = _lib.GetAssertionOptions(
					allowedCredentials: existingCredentials,
					userVerification: string.IsNullOrEmpty(userVerification) ? UserVerificationRequirement.Discouraged : userVerification.ToEnum<UserVerificationRequirement>(),
					extensions: new AuthenticationExtensionsClientInputs()
					{
						SimpleTransactionAuthorization = "FIDO",
						GenericTransactionAuthorization = new TxAuthGenericArg { ContentType = "text/plain", Content = new byte[] { 0x46, 0x49, 0x44, 0x4F } },
						UserVerificationIndex = true,
						Location = true,
						UserVerificationMethod = true
					}
				);

				// 4. Temporarily store options, session/in-memory cache/redis/db
				var sessionId = IdentityModel.CryptoRandom.CreateRandomKeyString(64);
				TestUsers.FidoAttestationOptions[sessionId] = options.ToJson();

				// 5. Return options to client
				return Ok(new AssertionOptionsPostResponse
				{
					SessionId = sessionId,
					Options = options,
				});
			}

			catch (Exception e)
			{
				return BadRequest(new AssertionOptions { Status = "error", ErrorMessage = FormatException(e) });
			}
		}
		public class AssertionOptionsPostResponse
		{
			public string SessionId { get; set; }
			public AssertionOptions Options { get; set; }
		}



		[HttpPost]
		[Route("/makeAssertion")]
		public async Task<ActionResult> MakeAssertion([FromBody] MakeAssertionRequest request)
		{
			try
			{
				// 1. Get the assertion options we sent the client
				var jsonOptions = TestUsers.FidoAttestationOptions[request.SessionId];
				var options = AssertionOptions.FromJson(jsonOptions);

				// 2. Get registered credential from database
				var creds = TestUsers.FidoCredentials.Where(c => c.Descriptor.Id.SequenceEqual(request.RawResponse.Id)).FirstOrDefault();

				// 3. Get credential counter from database
				var storedCounter = creds.SignatureCounter;

				// 4. Create callback to check if userhandle owns the credentialId
				IsUserHandleOwnerOfCredentialIdAsync callback = async (args) =>
				{
					var storedCreds = TestUsers.FidoCredentials.Where(c => c.UserHandle.SequenceEqual(args.UserHandle));
					return storedCreds.Any(c => c.Descriptor.Id.SequenceEqual(args.CredentialId));
				};

				// 5. Make the assertion
				var res = await _lib.MakeAssertionAsync(
					assertionResponse: request.RawResponse,
					originalOptions: options,
					storedPublicKey: creds.PublicKey,
					storedSignatureCounter: storedCounter,
					isUserHandleOwnerOfCredentialIdCallback: callback);

				// 6. Store the updated counter
				TestUsers.FidoCredentials.Where(c => c.Descriptor.Id.SequenceEqual(res.CredentialId)).FirstOrDefault().SignatureCounter = res.Counter;

				// 7. return OK to client
				return Ok(res);
			}
			catch (Exception e)
			{
				return BadRequest(new AssertionVerificationResult { Status = "error", ErrorMessage = FormatException(e) });
			}
		}
		public class MakeAssertionRequest
		{
			public string SessionId { get; set; }
			public AuthenticatorAssertionRawResponse RawResponse { get; set; }
		}












































































		[HttpGet]
		[Authorize]
		[Route("/credentials")]
		public ActionResult Index()
		{
			var subjectId = User.Claims.FirstOrDefault(c => c.Type == "sub")?.Value;

			// 1. Get user from DB
			var dbUser = TestUsers.Users.FirstOrDefault(u => string.Equals(u.SubjectId, subjectId, StringComparison.InvariantCultureIgnoreCase));
			if (dbUser == null)
			{
				return BadRequest("no such user");
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
							throw new Fido2VerificationException(string.Format("Missing or unknown keytype {0}", kty.ToString()));
						}
				}

				resultDtos.Add(resultDto);
			}

			return Ok(resultDtos);
		}
		public class FidoCredentialDto
		{
			public string AttestationType { get; set; }
			public DateTime CreateDate { get; set; }
			public string Counter { get; set; }
			public string AAGUID { get; set; }
			public string Description { get; set; }
			public string PublicKey { get; set; }
		}





































		private string FormatException(Exception e)
		{
			return string.Format("{0}{1}", e.Message, e.InnerException != null ? " (" + e.InnerException.Message + ")" : "");
		}
	}
}