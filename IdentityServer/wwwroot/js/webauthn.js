(function () {

	var buttonRegister = document.querySelector("#passwordlessRegister");
	buttonRegister.addEventListener("click", async function (event) {

		const publicKeyCredentialCreationOptions = {
			challenge: Uint8Array.from("wwt5GgwgiB9aQ0wMBc7sNg", c => c.charCodeAt(0)),
			rp: {
				name: "IdentityExperiments",
				id: "localhost",
			},
			user: {
				id: Uint8Array.from("UZSL85T9AFC", c => c.charCodeAt(0)),
				name: "manuel.naujoks@stp-online.de",
				displayName: "MANUEL.NAUJOKS@stp-online.de",
			},
			pubKeyCredParams: [{ alg: -7, type: "public-key" }],
			//authenticatorSelection: {
			//	authenticatorAttachment: "cross-platform",
			//},
			timeout: 60000,
			attestation: "direct"
		};

		try {
			const credential = await navigator.credentials.create({
				publicKey: publicKeyCredentialCreationOptions
			});

			console.log(credential);
		} catch (err) {
			console.error("error", err.message || err);
		}

	}, false);


	var buttonAuthenticate = document.querySelector("#passwordlessAuthenticate");
	buttonAuthenticate.addEventListener("click", async function (event) {

		const publicKeyCredentialRequestOptions = {
			challenge: Uint8Array.from("wwt5GgwgiB9aQ0wMBc7sNg", c => c.charCodeAt(0)),
			allowCredentials: [{
				id: Uint8Array.from(credentialId, c => c.charCodeAt(0)),
				type: 'public-key',
				transports: ['usb', 'ble', 'nfc'],
			}],
			timeout: 60000,
		};

		try {
			const assertion = await navigator.credentials.get({
				publicKey: publicKeyCredentialRequestOptions
			});

			console.log(assertion);
		} catch (err) {
			console.error("error", err.message || err);
		}

	}, false);

})();