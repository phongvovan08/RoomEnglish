import type { App } from 'vue'
import { addUIDependencies } from './ui'
import authPlugin from './auth'

export function addDependencies(app: App): void {
  addUIDependencies(app)
  app.use(authPlugin)
}
