import { BaseRepository } from "../baseRepository";
import { HttpRepository } from "../api/httpRepository";

export class PostsRepository extends BaseRepository {
  constructor(httpClient: HttpRepository) {
    super(httpClient);
  }
}
