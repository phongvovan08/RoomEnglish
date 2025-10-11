import { type AxiosRequestConfig, type AxiosInstance } from "axios";

export class HttpRepository {
  private readonly instance: AxiosInstance;
  private readonly baseUrl: string;

  constructor(axiosInstance: AxiosInstance, baseUrl: string) {
    this.instance = axiosInstance;
    this.baseUrl = baseUrl;
  }

  async get<T = any, P = Record<string, any>>(
    route: string,
    params?: P,
    options?: AxiosRequestConfig
  ) {
    const { data } = await this.instance.get<T>(`${this.baseUrl}${route}`, {
      ...options,
      params,
    });
    return data;
  }

  async post<T = any, D = any>(
    route: string,
    payload: D,
    options?: AxiosRequestConfig
  ) {
    const { data } = await this.instance.post<T>(
      `${this.baseUrl}${route}`,
      payload,
      options
    );
    return data;
  }

  async put<T = any, D = any>(
    route: string,
    payload: D,
    options?: AxiosRequestConfig
  ) {
    const { data } = await this.instance.put<T>(
      `${this.baseUrl}${route}`,
      payload,
      options
    );
    return data;
  }

  async patch<T = any, D = any>(
    route: string,
    payload: D,
    options?: AxiosRequestConfig
  ) {
    const { data } = await this.instance.patch<T>(
      `${this.baseUrl}${route}`,
      payload,
      options
    );
    return data;
  }

  async delete<T = any, P = Record<string, any>>(
    route = "",
    params?: P,
    options?: AxiosRequestConfig
  ) {
    const { data } = await this.instance.delete<T>(`${this.baseUrl}${route}`, {
      ...options,
      params,
    });
    return data;
  }
}
