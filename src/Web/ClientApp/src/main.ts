import './assets/main.css'
import './styles/cyborg-gaming.css'
import { addDependencies } from './plugins'
import { createApp } from 'vue'

import App from './App.vue'

const app = createApp(App)
addDependencies(app)

app.mount('#app')
