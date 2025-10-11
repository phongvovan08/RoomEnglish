import type { ApiListResponse, RequestParams } from "@/domain/entities/api";

export interface IBaseRepository {
  getList: <P extends RequestParams, T>(
    route?: string,
    params?: P,
    config?: any
  ) => Promise<ApiListResponse<T>>;
  getById: <T>(id: string, config?: any) => Promise<T>;
  create: <D, T>(data: Partial<D>, config?: any) => Promise<T>;
  update: <D, T>(id: string, data: Partial<D>, config: any) => Promise<T>;
  delete: <T>(id: string) => Promise<T>;
  get: <T = any, P = Record<string, any>>(
    route?: string,
    params?: P,
    config?: any
  ) => Promise<T>;
}
