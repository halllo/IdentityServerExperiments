(function () {
	var button = document.querySelector("#cookieConsent button[data-cookie-string]");
	button.addEventListener("click", function (event) {
		document.cookie = button.dataset.cookieString;
		document.querySelector("#cookieConsent").classList.add("hidden");
	}, false);
})();