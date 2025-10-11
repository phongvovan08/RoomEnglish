export interface IMsalRepository {
  acquireTokenSilent: () => Promise<unknown>
  acquireTokenRedirect: () => Promise<void>
  loginRedirect: (redirectStartPage?: string) => Promise<void>
  handleRedirectPromise: (redirectStartPage?: string) => Promise<void>
  getToken: () => Promise<string>
}
