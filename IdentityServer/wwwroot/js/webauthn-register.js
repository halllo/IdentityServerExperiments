(function () {

	let buttonRegister = document.querySelector('#passwordlessRegister');
	if (buttonRegister) {
		buttonRegister.addEventListener('click', async function (event) {

			try {
				let data = new FormData();
				data.append('attType', 'direct');
				data.append('authType', '');
				data.append('userVerification', 'preferred');
				data.append('requireResidentKey', true);

				let makeCredentialOptionsResponse = await fetch('/makeCredentialOptions', {
					method: 'POST',
					body: data,
					headers: {
						'Accept': 'application/json'
					}
				});

				let makeCredentialOptionsResponseObject = await makeCredentialOptionsResponse.json();
				let publicKeyCredentialCreationOptions = makeCredentialOptionsResponseObject.options;
				if (publicKeyCredentialCreationOptions && publicKeyCredentialCreationOptions.status === 'ok') {
					let sessionId = makeCredentialOptionsResponseObject.sessionId;

					publicKeyCredentialCreationOptions.challenge = coerceToArrayBuffer(publicKeyCredentialCreationOptions.challenge);
					publicKeyCredentialCreationOptions.user.id = coerceToArrayBuffer(publicKeyCredentialCreationOptions.user.id);
					publicKeyCredentialCreationOptions.excludeCredentials = publicKeyCredentialCreationOptions.excludeCredentials.map((excludeCredential) => {
						excludeCredential.id = coerceToArrayBuffer(excludeCredential.id);
						return excludeCredential;
					});
					if (publicKeyCredentialCreationOptions.authenticatorSelection.authenticatorAttachment === null) publicKeyCredentialCreationOptions.authenticatorSelection.authenticatorAttachment = undefined;

					const newCredential = await navigator.credentials.create({ publicKey: publicKeyCredentialCreationOptions });

					let attestationObject = new Uint8Array(newCredential.response.attestationObject);
					let clientDataJSON = new Uint8Array(newCredential.response.clientDataJSON);
					let rawId = new Uint8Array(newCredential.rawId);
					const data = {
						sessionId: sessionId,
						attestationResponse: {
							id: newCredential.id,
							rawId: coerceToBase64Url(rawId),
							type: newCredential.type,
							extensions: newCredential.getClientExtensionResults(),
							response: {
								AttestationObject: coerceToBase64Url(attestationObject),
								clientDataJson: coerceToBase64Url(clientDataJSON)
							}
						}
					};

					let makeCredentialResponse = await fetch('/makeCredential', {
						method: 'POST',
						body: JSON.stringify(data),
						headers: {
							'Accept': 'application/json',
							'Content-Type': 'application/json'
						}
					});

					let makeCredentialResponseObject = await makeCredentialResponse.json();
					if (makeCredentialResponseObject.status === 'ok') {
						console.info('registered :)', makeCredentialResponseObject);
						location.reload(true);
					} else {
						console.error(makeCredentialResponseObject.errorMessage);
					}
				} else {
					console.error(makeCredentialOptionsResponseObject);
				}
			} catch (err) {
				console.error(err.message || err);
			}

		}, false);
	}



























































	let coerceToArrayBuffer = function (thing, name) {
		if (typeof thing === 'string') {
			// base64url to base64
			thing = thing.replace(/-/g, '+').replace(/_/g, '/');

			// base64 to Uint8Array
			var str = window.atob(thing);
			var bytes = new Uint8Array(str.length);
			for (var i = 0; i < str.length; i++) {
				bytes[i] = str.charCodeAt(i);
			}
			thing = bytes;
		}

		// Array to Uint8Array
		if (Array.isArray(thing)) {
			thing = new Uint8Array(thing);
		}

		// Uint8Array to ArrayBuffer
		if (thing instanceof Uint8Array) {
			thing = thing.buffer;
		}

		// error if none of the above worked
		if (!(thing instanceof ArrayBuffer)) {
			throw new TypeError("could not coerce '" + name + "' to ArrayBuffer");
		}

		return thing;
	};

	let coerceToBase64Url = function (thing, name) {
		// Array or ArrayBuffer to Uint8Array
		if (Array.isArray(thing)) {
			thing = Uint8Array.from(thing);
		}

		if (thing instanceof ArrayBuffer) {
			thing = new Uint8Array(thing);
		}

		// Uint8Array to base64
		if (thing instanceof Uint8Array) {
			var str = '';
			var len = thing.byteLength;

			for (var i = 0; i < len; i++) {
				str += String.fromCharCode(thing[i]);
			}
			thing = window.btoa(str);
		}

		if (typeof thing !== 'string') {
			throw new Error("could not coerce '" + name + "' to string");
		}

		// base64 to base64url
		// NOTE: "=" at the end of challenge is optional, strip it off here
		thing = thing.replace(/\+/g, '-').replace(/\//g, '_').replace(/=*$/g, '');

		return thing;
	};

})();