export const environment = {
  production: true,
  backend_api: 'http://localhost:56706',

  oidc: {
    authority: 'http://localhost:56311/',
    clientId: 'spa',
    scope: 'openid profile',
    redirectUri: 'http://localhost:4200/',
  }

};
