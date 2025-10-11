import {
  PublicClientApplication,
  EventType,
  InteractionRequiredAuthError,
} from "@azure/msal-browser";
import type { AuthenticationResult } from "@azure/msal-browser";
import type { IMsalRepository } from "@/domain/interfaces/iMsalRepository";

export class MsalRepository implements IMsalRepository {
  protected scopeId: string;

  public instance: PublicClientApplication;

  constructor(msalInstance: PublicClientApplication, scope: string) {
    this.scopeId = scope;
    this.instance = msalInstance;
    this.instance.addEventCallback((event) => {
      if (event.eventType === EventType.LOGIN_SUCCESS && event.payload) {
        const payload = event.payload as AuthenticationResult;
        this.instance.setActiveAccount(payload.account);
      }
    });
  }
  public get requestWithScopes() {
    return {
      scopes: [this.scopeId],
    };
  }
  public get activeAccount() {
    return this.instance.getActiveAccount();
  }
  public get allAccounts() {
    return this.instance.getAllAccounts();
  }
  async acquireTokenSilent() {
    await this.instance.initialize();
    return this.instance.acquireTokenSilent(this.requestWithScopes);
  }
  async acquireTokenRedirect() {
    await this.instance.initialize();
    return this.instance.acquireTokenRedirect(this.requestWithScopes);
  }
  async loginRedirect(redirectStartPage?: string) {
    return this.instance.loginRedirect({
      ...this.requestWithScopes,
      redirectStartPage,
    });
  }
  // If your application uses redirects for interaction, handleRedirectPromise must be called and awaited on each page load before determining if a user is signed in or not
  /**
   * @param redirectStartPage page to redirect after loginRedirect success

   */
  async handleRedirectPromise(redirectStartPage?: string) {
    await this.instance.initialize();
    const authenticationResult = await this.instance.handleRedirectPromise()
    if (!authenticationResult) {
      await this.loginRedirect(redirectStartPage);
    }
  }
  async getToken() {
    let accessToken = "";
    const tokenRequest = { scopes: [this.scopeId] };
    try {
      const res = await this.instance.acquireTokenSilent(tokenRequest);
      accessToken = res.accessToken;
    } catch (error) {
      if (error instanceof InteractionRequiredAuthError) {
        await this.instance.acquireTokenRedirect(tokenRequest);
      }
      throw error;
    }
    return accessToken;
  }
}
