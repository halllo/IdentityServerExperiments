(function () {

	let buttonAuthenticate = document.querySelector('#passwordlessAuthenticate');
	if (buttonAuthenticate) {
		buttonAuthenticate.addEventListener('click', async function (event) {

			try {

				let data = new FormData();
				//let username = document.querySelector('#username-input').value;
				//data.append('username', username);
				//data.append('userVerification', 'preferred');

				let assertionOptionsResponse = await fetch('/assertionOptions', {
					method: 'POST',
					body: data,
					headers: {
						'Accept': 'application/json'
					}
				});

				let assertionOptionsResponseObject = await assertionOptionsResponse.json();
				let makeAssertionOptions = assertionOptionsResponseObject.options;
				if (makeAssertionOptions && makeAssertionOptions.status === 'ok') {
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

					let login_returnUrl = document.querySelector('#ReturnUrl').value;
					let login_rememberLogin = document.querySelector('#RememberLogin').value;

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
						},
						returnUrl: login_returnUrl,
						rememberLogin: login_rememberLogin
					};

					let makeAssertionResponse = await fetch('/makeAssertion', {
						method: 'POST',
						body: JSON.stringify(data),
						headers: {
							'Accept': 'application/json',
							'Content-Type': 'application/json'
						}
					});

					if (makeAssertionResponse.status !== 400) {
						let makeAssertionResponseObject = await makeAssertionResponse.json();
						if (makeAssertionResponseObject.status === 'ok') {
							let redirectUri = makeAssertionResponseObject.returnUrl;
							if (redirectUri === '~/') {
								redirectUri = location.origin;
							}
							console.info('authenticated :) redirect to ' + redirectUri, makeAssertionResponseObject);
							location.assign(redirectUri);
						} else {
							console.error(makeAssertionResponseObject.errorMessage);
						}
					} else {
						console.error(await makeAssertionResponse.json());
					}
				} else {
					console.error(assertionOptionsResponseObject);
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