export const environment = {
  production: true,
  backend_api: 'http://localhost:50996',

  msalConfig: {
    tenant: 'manuelnaujoks0aadsampleb2c.onmicrosoft.com',
    clientId: 'eca0fbcb-2a58-44d0-b34a-5f2b740273c2', // application id
    signUpSignInPolicy: 'B2C_1_signup-signin',
    redirectUri: 'http://localhost:4200/',
    scopes: [
      'https://manuelnaujoks0aadsampleb2c.onmicrosoft.com/einwilligungserklaerung/user_impersonation'
    ],
  }

};
