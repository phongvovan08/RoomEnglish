import type { App } from 'vue'
import { addUIDependencies } from './ui'

export function addDependencies(app: App): void {
  addUIDependencies(app)
}
