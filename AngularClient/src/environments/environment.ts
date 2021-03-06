// The file contents for the current environment will overwrite these during build.
// The build system defaults to the dev environment which uses `environment.ts`, but if you do
// `ng build --env=prod` then `environment.prod.ts` will be used instead.
// The list of which env maps to which file can be found in `.angular-cli.json`.

export const environment = {
  production: false,
  backend_api: 'http://localhost:56706',

  oidc: {
    authority: 'https://localhost:44389/',
    clientId: 'angularclient',
    scope: 'openid profile book.read book.write'
  }

};
