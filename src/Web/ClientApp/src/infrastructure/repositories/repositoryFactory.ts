import { ERepositories } from "../constants";
import { PostsRepository } from "./posts/postsRepository";
import { HttpRepository } from "./api/httpRepository";
import { PublicClientApplication } from "@azure/msal-browser";
import axios from "axios";
import { MsalRepository } from "./msal/msalRepository";
import { LocalStorageRepository } from "./storages/localStorageRepository";

const msalInstance = new PublicClientApplication({
  auth: {
    clientId: import.meta.env.VITE_AUTH_CLIENT_ID,
    authority: import.meta.env.VITE_AUTH_AUTHORITY_URL,
  },

  cache: {
    cacheLocation: "localStorage",
  },
});
const msalRepository = new MsalRepository(
  msalInstance,
  import.meta.env.VITE_AUTH_SCOPES
);

const axiosInstance = axios.create({
  baseURL: import.meta.env.VITE_API_BASE_URL,
});
axiosInstance.interceptors.request.use(
  async (config) => {
    if (import.meta.env.VITE_USE_MSAL_CLIENT) {
      const token = await msalRepository.getToken();
      if (token && config.headers) {
        config.headers["Authorization"] = "Bearer " + token;
      }
    }

    return config;
  },
  (error) => {
    return Promise.reject(error as Error);
  }
);

export class RepositoryFactory {
  public static getRepository(type: ERepositories) {
    switch (type) {
      case ERepositories.Msal: {
        return msalRepository;
      }
      case ERepositories.Posts: {
        return new PostsRepository(new HttpRepository(axiosInstance, "/posts"));
      }
      case ERepositories.LocalStorage: {
        return new LocalStorageRepository();
      }
      default:
        throw new Error(`Invalid repository type: ${type}`);
    }
  }
}
