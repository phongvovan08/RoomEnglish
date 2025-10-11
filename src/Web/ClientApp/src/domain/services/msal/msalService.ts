import type { MsalRepository } from "@/infrastructure/repositories/msal/msalRepository";

export class MsalService {
  constructor(private readonly repository: MsalRepository) {}

  get activeUser(){
    return this.repository.activeAccount
  }
  checkIsAuthenticated(redirectStartPage?: string) {
    return this.repository.handleRedirectPromise(redirectStartPage)
  }
}
