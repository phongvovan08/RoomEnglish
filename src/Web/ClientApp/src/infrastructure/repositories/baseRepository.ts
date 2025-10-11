import { HttpRepository } from "./api/httpRepository";
import type { ApiListResponse, RequestParams } from "@/domain/entities/api";
import type { AxiosRequestConfig } from "axios";

export class BaseRepository {
  constructor(public httpClient: HttpRepository) {}

  async getList<P extends RequestParams, T>(
    route = "",
    params?: P,
    config?: AxiosRequestConfig
  ) {
    return this.httpClient.get<ApiListResponse<T>>(route, params, config);
  }
  async getById<T>(id: string, config?: AxiosRequestConfig) {
    return this.httpClient.get<T>(`/${id}`, config);
  }
  async create<D, T>(data: Partial<D>, config?: AxiosRequestConfig) {
    return this.httpClient.post<T>("", data, config);
  }
  async update<D, T>(id: string, data: Partial<D>, config?: AxiosRequestConfig) {
    return this.httpClient.put<T>(`/${id}`, data, config);
  }
  async delete<T = unknown>(id: string) {
    return this.httpClient.delete<T>(id);
  }
  async get<T = any, P = Record<string, any>>(
    route = "",
    params?: P,
    config?: AxiosRequestConfig
  ) {
    return this.httpClient.get<T, P>(route, params, config);
  }
}