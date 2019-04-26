export const environment = {
  production: true,
  backend_api: 'https://manuels-identityserver-api.azurewebsites.net/',

  oidc: {
    authority: 'https://manuels-identityserver.azurewebsites.net/',
    clientId: 'angularclient',
    scope: 'openid profile book.read book.write',
  }

};
