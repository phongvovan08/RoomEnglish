<template>
  <div class="cyborg-container auth-container">
    <div class="auth-card cyborg-card">
      <div class="auth-header">
        <h1 class="cyborg-title">
          <Icon icon="mdi:account-lock" />
          Login to RoomEnglish
        </h1>
        <p class="auth-subtitle">Access your gaming dashboard</p>
      </div>

      <form @submit.prevent="handleLogin" class="auth-form">
        <!-- Email Field -->
        <div class="form-group">
          <label for="email" class="form-label">
            <Icon icon="mdi:email" />
            Email Address
          </label>
          <input
            id="email"
            v-model="form.email"
            type="email"
            class="cyborg-input"
            :class="{ 'error': errors.email }"
            placeholder="Enter your email"
            required
            autocomplete="email"
          />
          <span v-if="errors.email" class="error-message">{{ errors.email }}</span>
        </div>

        <!-- Password Field -->
        <div class="form-group">
          <label for="password" class="form-label">
            <Icon icon="mdi:lock" />
            Password
          </label>
          <div class="password-input">
            <input
              id="password"
              v-model="form.password"
              :type="showPassword ? 'text' : 'password'"
              class="cyborg-input"
              :class="{ 'error': errors.password }"
              placeholder="Enter your password"
              required
              autocomplete="current-password"
            />
            <button
              type="button"
              @click="togglePassword"
              class="password-toggle"
              :aria-label="showPassword ? 'Hide password' : 'Show password'"
            >
              <Icon :icon="showPassword ? 'mdi:eye-off' : 'mdi:eye'" />
            </button>
          </div>
          <span v-if="errors.password" class="error-message">{{ errors.password }}</span>
        </div>

        <!-- Remember Me & Forgot Password -->
        <div class="form-options">
          <label class="checkbox-label">
            <input
              v-model="form.rememberMe"
              type="checkbox"
              class="cyborg-checkbox"
            />
            <span class="checkmark"></span>
            Remember me
          </label>
          
          <router-link to="/auth/forgot-password" class="forgot-link">
            Forgot password?
          </router-link>
        </div>

        <!-- Submit Button -->
        <button
          type="submit"
          class="cyborg-btn cyborg-btn-gradient auth-submit"
          :disabled="isLoading || !isFormValid"
        >
          <Icon v-if="isLoading" icon="mdi:loading" class="spinner" />
          <Icon v-else icon="mdi:login" />
          {{ isLoading ? 'Logging in...' : 'Login' }}
        </button>

        <!-- Register Link -->
        <div class="auth-footer">
          <p>Don't have an account?</p>
          <router-link to="/auth/register" class="register-link">
            Create Account
          </router-link>
        </div>
      </form>

      <!-- Gaming Elements -->
      <div class="gaming-elements">
        <div class="circuit-lines"></div>
        <div class="glow-orb"></div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive, computed } from 'vue'
import { Icon } from '@iconify/vue'
import { useAuth } from '@/composables/useAuth'

const { login, isLoading } = useAuth()

const showPassword = ref(false)

const form = reactive({
  email: '',
  password: '',
  rememberMe: false
})

const errors = reactive({
  email: '',
  password: ''
})

const isFormValid = computed(() => {
  return form.email.includes('@') && form.password.length >= 6
})

const togglePassword = () => {
  showPassword.value = !showPassword.value
}

const validateForm = () => {
  errors.email = ''
  errors.password = ''

  if (!form.email) {
    errors.email = 'Email is required'
    return false
  }
  
  if (!form.email.includes('@')) {
    errors.email = 'Please enter a valid email'
    return false
  }

  if (!form.password) {
    errors.password = 'Password is required'
    return false
  }

  if (form.password.length < 6) {
    errors.password = 'Password must be at least 6 characters'
    return false
  }

  return true
}

const handleLogin = async () => {
  if (!validateForm()) return

  try {
    await login({
      email: form.email,
      password: form.password
    })
  } catch (error) {
    // Error handling is done in useAuth composable
    console.error('Login failed:', error)
  }
}
</script>

<style scoped>
.auth-container {
  min-height: 100vh;
  display: flex;
  align-items: center;
  justify-content: center;
  background: linear-gradient(135deg, var(--bg-primary) 0%, rgba(231, 94, 141, 0.1) 100%);
  position: relative;
  overflow: hidden;
}

.auth-card {
  width: 100%;
  max-width: 420px;
  padding: 3rem;
  position: relative;
  z-index: 2;
  backdrop-filter: blur(20px);
  border: 1px solid rgba(231, 94, 141, 0.2);
}

.auth-header {
  text-align: center;
  margin-bottom: 2.5rem;
}

.auth-header h1 {
  margin-bottom: 0.5rem;
  background: linear-gradient(135deg, var(--accent-pink), var(--accent-blue));
  -webkit-background-clip: text;
  -webkit-text-fill-color: transparent;
  background-clip: text;
}

.auth-subtitle {
  color: var(--text-secondary);
  margin: 0;
  font-size: 1rem;
}

.auth-form {
  display: flex;
  flex-direction: column;
  gap: 1.5rem;
}

.form-group {
  display: flex;
  flex-direction: column;
}

.form-label {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  margin-bottom: 0.5rem;
  color: var(--text-primary);
  font-weight: 500;
  font-size: 0.875rem;
  text-transform: uppercase;
  letter-spacing: 0.5px;
}

.cyborg-input {
  width: 100%;
  padding: 0.875rem 1rem;
  background: rgba(30, 30, 30, 0.8);
  border: 1px solid var(--border-color);
  border-radius: 0.5rem;
  color: var(--text-primary);
  transition: all 0.3s ease;
  font-size: 1rem;
}

.cyborg-input:focus {
  outline: none;
  border-color: var(--accent-pink);
  box-shadow: 0 0 0 3px rgba(231, 94, 141, 0.1);
  background: rgba(30, 30, 30, 0.9);
}

.cyborg-input.error {
  border-color: #ef4444;
  box-shadow: 0 0 0 3px rgba(239, 68, 68, 0.1);
}

.password-input {
  position: relative;
}

.password-toggle {
  position: absolute;
  right: 0.75rem;
  top: 50%;
  transform: translateY(-50%);
  background: none;
  border: none;
  color: var(--text-secondary);
  cursor: pointer;
  padding: 0.25rem;
  border-radius: 0.25rem;
  transition: color 0.2s ease;
}

.password-toggle:hover {
  color: var(--accent-pink);
}

.form-options {
  display: flex;
  justify-content: space-between;
  align-items: center;
  font-size: 0.875rem;
}

.checkbox-label {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  cursor: pointer;
  color: var(--text-secondary);
}

.cyborg-checkbox {
  appearance: none;
  width: 1rem;
  height: 1rem;
  border: 1px solid var(--border-color);
  border-radius: 0.25rem;
  background: var(--bg-secondary);
  cursor: pointer;
  position: relative;
  transition: all 0.2s ease;
}

.cyborg-checkbox:checked {
  background: var(--accent-pink);
  border-color: var(--accent-pink);
}

.cyborg-checkbox:checked::after {
  content: '✓';
  position: absolute;
  top: 50%;
  left: 50%;
  transform: translate(-50%, -50%);
  color: white;
  font-size: 0.75rem;
  font-weight: bold;
}

.forgot-link {
  color: var(--accent-pink);
  text-decoration: none;
  transition: color 0.2s ease;
}

.forgot-link:hover {
  color: var(--accent-blue);
}

.auth-submit {
  width: 100%;
  padding: 1rem;
  margin-top: 1rem;
  font-size: 1rem;
  font-weight: 600;
  text-transform: uppercase;
  letter-spacing: 1px;
}

.auth-submit:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

.auth-footer {
  text-align: center;
  margin-top: 1.5rem;
  color: var(--text-secondary);
}

.register-link {
  color: var(--accent-pink);
  text-decoration: none;
  font-weight: 600;
  margin-left: 0.5rem;
  transition: color 0.2s ease;
}

.register-link:hover {
  color: var(--accent-blue);
}

.error-message {
  color: #ef4444;
  font-size: 0.75rem;
  margin-top: 0.25rem;
}

.spinner {
  animation: spin 1s linear infinite;
}

@keyframes spin {
  from { transform: rotate(0deg); }
  to { transform: rotate(360deg); }
}

/* Gaming Elements */
.gaming-elements {
  position: absolute;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  pointer-events: none;
  z-index: 1;
}

.circuit-lines {
  position: absolute;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background-image: 
    linear-gradient(90deg, transparent 98%, rgba(231, 94, 141, 0.1) 100%),
    linear-gradient(0deg, transparent 98%, rgba(231, 94, 141, 0.1) 100%);
  background-size: 20px 20px;
  opacity: 0.3;
}

.glow-orb {
  position: absolute;
  top: -50%;
  right: -50%;
  width: 200%;
  height: 200%;
  background: radial-gradient(circle, rgba(231, 94, 141, 0.1) 0%, transparent 70%);
  animation: pulse 4s ease-in-out infinite;
}

@keyframes pulse {
  0%, 100% { transform: scale(0.8); opacity: 0.5; }
  50% { transform: scale(1.2); opacity: 0.8; }
}

@media (max-width: 480px) {
  .auth-card {
    padding: 2rem 1.5rem;
    margin: 1rem;
  }
  
  .form-options {
    flex-direction: column;
    gap: 0.75rem;
    align-items: flex-start;
  }
}
</style>