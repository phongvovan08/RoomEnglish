import './assets/main.css'
import './styles/cyborg-gaming.css'
import { addDependencies } from './plugins'
import { createApp } from 'vue'
import { useAuthStore } from './stores/auth'

import App from './App.vue'

const app = createApp(App)
addDependencies(app)

app.mount('#app')

// Initialize auth after mounting
const authStore = useAuthStore()
authStore.initializeAuth()
console.log('🔐 Auth initialized on app startup')
