(function () {

	let buttonRegister = document.querySelector('#passwordlessRegister');
	if (buttonRegister) {
		buttonRegister.addEventListener('click', async function (event) {

			try {
				let data = new FormData();
				data.append('attType', 'direct');
				data.append('authType', '');
				data.append('userVerification', 'preferred');
				data.append('requireResidentKey', false);//only edge supports this right now

				let makeCredentialOptionsResponse = await fetch('/makeCredentialOptions', {
					method: 'POST',
					body: data,
					headers: {
						'Accept': 'application/json'
					}
				});

				if (makeCredentialOptionsResponse.ok) {
					let makeCredentialOptionsResponseObject = await makeCredentialOptionsResponse.json();
					let publicKeyCredentialCreationOptions = makeCredentialOptionsResponseObject.options;
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

					if (makeCredentialResponse.ok) {
						let makeCredentialResponseObject = await makeCredentialResponse.json();
						console.info('registered :)', makeCredentialResponseObject);
					} else {
						console.error(makeCredentialResponse.status);
					}
				} else {
					console.error(makeCredentialOptionsResponse.status);
				}
			} catch (err) {
				console.error(err.message || err);
			}

		}, false);
	}














































	let buttonAuthenticate = document.querySelector('#passwordlessAuthenticate');
	if (buttonAuthenticate) {
		buttonAuthenticate.addEventListener('click', async function (event) {

			try {
				let username = document.querySelector('#username-input').value;

				let data = new FormData();
				data.append('username', username);
				data.append('userVerification', 'preferred');

				let assertionOptionsResponse = await fetch('/assertionOptions', {
					method: 'POST',
					body: data,
					headers: {
						'Accept': 'application/json'
					}
				});

				if (assertionOptionsResponse.ok) {
					let assertionOptionsResponseObject = await assertionOptionsResponse.json();
					let makeAssertionOptions = assertionOptionsResponseObject.options;
					let sessionId = assertionOptionsResponseObject.sessionId;

					const challenge = makeAssertionOptions.challenge.replace(/-/g, '+').replace(/_/g, '/');
					makeAssertionOptions.challenge = Uint8Array.from(atob(challenge), c => c.charCodeAt(0));
					makeAssertionOptions.allowCredentials.forEach(function (listItem) {
						var fixedId = listItem.id.replace(/\_/g, '/').replace(/\-/g, '+');
						listItem.id = Uint8Array.from(atob(fixedId), c => c.charCodeAt(0));
					});

					const assertedCredential = await navigator.credentials.get({ publicKey: makeAssertionOptions });

					let authData = new Uint8Array(assertedCredential.response.authenticatorData);
					let clientDataJSON = new Uint8Array(assertedCredential.response.clientDataJSON);
					let rawId = new Uint8Array(assertedCredential.rawId);
					let sig = new Uint8Array(assertedCredential.response.signature);
					const data = {
						sessionId: sessionId,
						rawResponse: {
							id: assertedCredential.id,
							rawId: coerceToBase64Url(rawId),
							type: assertedCredential.type,
							extensions: assertedCredential.getClientExtensionResults(),
							response: {
								authenticatorData: coerceToBase64Url(authData),
								clientDataJson: coerceToBase64Url(clientDataJSON),
								signature: coerceToBase64Url(sig)
							}
						}
					};

					let makeAssertionResponse = await fetch('/makeAssertion', {
						method: 'POST',
						body: JSON.stringify(data),
						headers: {
							'Accept': 'application/json',
							'Content-Type': 'application/json'
						}
					});

					if (makeAssertionResponse.ok) {
						let makeAssertionResponseObject = await makeAssertionResponse.json();
						console.info('authenticated :)', makeAssertionResponseObject);
					} else {
						console.error(makeAssertionResponse.status);
					}

				} else {
					console.error(assertionOptionsResponse.status);
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