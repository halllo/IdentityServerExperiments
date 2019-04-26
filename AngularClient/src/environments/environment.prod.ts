export const environment = {
  production: true,
  backend_api: 'http://localhost:56706',

  oidc: {
    authority: 'http://localhost:56311/',
    clientId: 'angularclient',
    scope: 'openid profile book.read book.write',
    redirectUri: 'http://localhost:4200/',
  }

};
