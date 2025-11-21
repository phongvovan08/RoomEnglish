<template>
  <div class="cyborg-container auth-container">
    <div class="auth-card cyborg-card">
      <div class="auth-header">
        <h1 class="cyborg-title">
          <Icon icon="mdi:account-plus" />
          Join RoomEnglish
        </h1>
        <p class="auth-subtitle">Create your gaming account</p>
      </div>

      <form @submit.prevent="handleRegister" class="auth-form">
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
              placeholder="Create a secure password"
              required
              autocomplete="new-password"
            />
            <button
              type="button"
              @click="togglePassword"
              class="password-toggle"
            >
              <Icon :icon="showPassword ? 'mdi:eye-off' : 'mdi:eye'" />
            </button>
          </div>
          <span v-if="errors.password" class="error-message">{{ errors.password }}</span>
          <div v-if="!errors.password && form.password" class="password-hint">
            <p class="hint-title">Password Requirements:</p>
            <ul class="hint-list">
              <li :class="{ 'valid': form.password.length >= 6 }">
                <Icon :icon="form.password.length >= 6 ? 'mdi:check-circle' : 'mdi:circle-outline'" />
                At least 6 characters
              </li>
              <li :class="{ 'valid': /\d/.test(form.password) }">
                <Icon :icon="/\d/.test(form.password) ? 'mdi:check-circle' : 'mdi:circle-outline'" />
                At least one digit (0-9)
              </li>
              <li :class="{ 'valid': /[a-z]/.test(form.password) }">
                <Icon :icon="/[a-z]/.test(form.password) ? 'mdi:check-circle' : 'mdi:circle-outline'" />
                At least one lowercase letter (a-z)
              </li>
              <li :class="{ 'valid': /[A-Z]/.test(form.password) }">
                <Icon :icon="/[A-Z]/.test(form.password) ? 'mdi:check-circle' : 'mdi:circle-outline'" />
                At least one uppercase letter (A-Z)
              </li>
              <li :class="{ 'valid': /[^a-zA-Z\d]/.test(form.password) }">
                <Icon :icon="/[^a-zA-Z\d]/.test(form.password) ? 'mdi:check-circle' : 'mdi:circle-outline'" />
                At least one special character (!@#$%^&*)
              </li>
            </ul>
          </div>
        </div>

        <!-- Confirm Password Field -->
        <div class="form-group">
          <label for="confirmPassword" class="form-label">
            <Icon icon="mdi:lock-check" />
            Confirm Password
          </label>
          <div class="password-input">
            <input
              id="confirmPassword"
              v-model="form.confirmPassword"
              :type="showConfirmPassword ? 'text' : 'password'"
              class="cyborg-input"
              :class="{ 'error': errors.confirmPassword }"
              placeholder="Confirm your password"
              required
              autocomplete="new-password"
            />
            <button
              type="button"
              @click="toggleConfirmPassword"
              class="password-toggle"
            >
              <Icon :icon="showConfirmPassword ? 'mdi:eye-off' : 'mdi:eye'" />
            </button>
          </div>
          <span v-if="errors.confirmPassword" class="error-message">{{ errors.confirmPassword }}</span>
        </div>

        <!-- Password Strength Indicator -->
        <div v-if="form.password" class="password-strength">
          <div class="strength-bar">
            <div 
              class="strength-fill" 
              :class="`strength-${passwordStrength.level}`"
              :style="{ width: `${passwordStrength.score * 25}%` }"
            ></div>
          </div>
          <span class="strength-text" :class="`strength-${passwordStrength.level}`">
            {{ passwordStrength.text }}
          </span>
        </div>

        <!-- Terms & Privacy -->
        <div class="form-group">
          <label class="checkbox-label">
            <input
              v-model="form.acceptTerms"
              type="checkbox"
              class="cyborg-checkbox"
              required
            />
            <span class="checkmark"></span>
            <span>
              I agree to the 
              <a href="/terms" target="_blank" class="link">Terms of Service</a>
              and 
              <a href="/privacy" target="_blank" class="link">Privacy Policy</a>
            </span>
          </label>
          <span v-if="errors.acceptTerms" class="error-message">{{ errors.acceptTerms }}</span>
        </div>

        <!-- Submit Button -->
        <button
          type="submit"
          class="cyborg-btn cyborg-btn-gradient auth-submit"
          :disabled="isLoading || !isFormValid"
        >
          <Icon v-if="isLoading" icon="mdi:loading" class="spinner" />
          <Icon v-else icon="mdi:account-plus" />
          {{ isLoading ? 'Creating Account...' : 'Create Account' }}
        </button>

        <!-- Login Link -->
        <div class="auth-footer">
          <p>Already have an account?</p>
          <router-link to="/auth/login" class="register-link">
            Login Here
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

const { register, isLoading } = useAuth()

const showPassword = ref(false)
const showConfirmPassword = ref(false)

const form = reactive({
  email: '',
  password: '',
  confirmPassword: '',
  acceptTerms: false
})

const errors = reactive({
  email: '',
  password: '',
  confirmPassword: '',
  acceptTerms: ''
})

const passwordStrength = computed(() => {
  const password = form.password
  if (!password) return { score: 0, level: 'weak', text: '' }

  let score = 0
  if (password.length >= 8) score++
  if (/[a-z]/.test(password)) score++
  if (/[A-Z]/.test(password)) score++
  if (/\d/.test(password)) score++
  if (/[^a-zA-Z\d]/.test(password)) score++

  const levels = ['weak', 'fair', 'good', 'strong']
  const texts = ['Weak', 'Fair', 'Good', 'Strong']
  
  const level = levels[Math.min(score - 1, 3)] || 'weak'
  const text = texts[Math.min(score - 1, 3)] || 'Weak'

  return { score, level, text }
})

const isFormValid = computed(() => {
  const hasValidPassword = form.password.length >= 6 &&
    /\d/.test(form.password) &&
    /[a-z]/.test(form.password) &&
    /[A-Z]/.test(form.password) &&
    /[^a-zA-Z\d]/.test(form.password)
  
  return (
    form.email.includes('@') && 
    hasValidPassword &&
    form.password === form.confirmPassword &&
    form.acceptTerms
  )
})

const togglePassword = () => {
  showPassword.value = !showPassword.value
}

const toggleConfirmPassword = () => {
  showConfirmPassword.value = !showConfirmPassword.value
}

const validateForm = () => {
  // Reset errors
  Object.keys(errors).forEach(key => {
    errors[key as keyof typeof errors] = ''
  })

  let isValid = true

  // Email validation
  if (!form.email) {
    errors.email = 'Email is required'
    isValid = false
  } else if (!form.email.includes('@')) {
    errors.email = 'Please enter a valid email'
    isValid = false
  }

  // Password validation
  if (!form.password) {
    errors.password = 'Password is required'
    isValid = false
  } else {
    const passwordErrors: string[] = []
    
    if (form.password.length < 6) {
      passwordErrors.push('Password must be at least 6 characters long')
    }
    if (!/\d/.test(form.password)) {
      passwordErrors.push('Password must contain at least one digit (0-9)')
    }
    if (!/[a-z]/.test(form.password)) {
      passwordErrors.push('Password must contain at least one lowercase letter (a-z)')
    }
    if (!/[A-Z]/.test(form.password)) {
      passwordErrors.push('Password must contain at least one uppercase letter (A-Z)')
    }
    if (!/[^a-zA-Z\d]/.test(form.password)) {
      passwordErrors.push('Password must contain at least one special character (!@#$%^&*)')
    }
    
    if (passwordErrors.length > 0) {
      errors.password = passwordErrors.join('. ')
      isValid = false
    }
  }

  // Confirm password validation
  if (!form.confirmPassword) {
    errors.confirmPassword = 'Please confirm your password'
    isValid = false
  } else if (form.password !== form.confirmPassword) {
    errors.confirmPassword = 'Passwords do not match'
    isValid = false
  }

  // Terms validation
  if (!form.acceptTerms) {
    errors.acceptTerms = 'You must accept the terms and conditions'
    isValid = false
  }

  return isValid
}

const handleRegister = async () => {
  if (!validateForm()) return

  // Clear previous errors
  Object.keys(errors).forEach(key => {
    errors[key as keyof typeof errors] = ''
  })

  try {
    await register({
      email: form.email,
      password: form.password,
      confirmPassword: form.confirmPassword
    })
    // Success - register() will handle redirect
  } catch (error: any) {
    console.error('Registration failed:', error)
    // Handle error in form fields
    handleServerError(error.message || 'Registration failed')
  }
}

const handleServerError = (errorMessage: string) => {
  const lowerMessage = errorMessage.toLowerCase()
  
  // Check if error is related to password
  if (lowerMessage.includes('password') ||
      lowerMessage.includes('digit') ||
      lowerMessage.includes('lower') ||
      lowerMessage.includes('uppercase') ||
      lowerMessage.includes('lowercase') ||
      lowerMessage.includes('nonalphanumeric') ||
      lowerMessage.includes('special character') ||
      lowerMessage.includes('tooshort') ||
      lowerMessage.includes('at least') ||
      lowerMessage.includes('characters')) {
    errors.password = errorMessage
  } 
  // Check if error is related to email
  else if (lowerMessage.includes('email') || 
           lowerMessage.includes('duplicate') ||
           lowerMessage.includes('already in use')) {
    errors.email = errorMessage
  } 
  // General error - show on password field as it's most common
  else {
    errors.password = errorMessage
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
  max-width: 450px;
  padding: 3rem;
  position: relative;
  z-index: 2;
  backdrop-filter: blur(20px);
  border: 1px solid rgba(231, 94, 141, 0.2);
  margin: 2rem 1rem;
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

.password-strength {
  margin-top: -0.5rem;
}

.strength-bar {
  width: 100%;
  height: 4px;
  background: var(--bg-secondary);
  border-radius: 2px;
  overflow: hidden;
  margin-bottom: 0.5rem;
}

.strength-fill {
  height: 100%;
  transition: all 0.3s ease;
  border-radius: 2px;
}

.strength-weak { background: #ef4444; }
.strength-fair { background: #f59e0b; }
.strength-good { background: #10b981; }
.strength-strong { background: #059669; }

.strength-text {
  font-size: 0.75rem;
  font-weight: 500;
}

.checkbox-label {
  display: flex;
  align-items: flex-start;
  gap: 0.5rem;
  cursor: pointer;
  color: var(--text-secondary);
  line-height: 1.4;
}

.cyborg-checkbox {
  appearance: none;
  width: 1rem;
  height: 1rem;
  min-width: 1rem;
  border: 1px solid var(--border-color);
  border-radius: 0.25rem;
  background: var(--bg-secondary);
  cursor: pointer;
  position: relative;
  transition: all 0.2s ease;
  margin-top: 0.1rem;
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

.link {
  color: var(--accent-pink);
  text-decoration: none;
}

.link:hover {
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
  font-weight: 500;
  display: flex;
  align-items: center;
  gap: 0.25rem;
}

.password-hint {
  margin-top: 0.5rem;
  padding: 0.75rem;
  background: rgba(116, 192, 252, 0.1);
  border: 1px solid rgba(116, 192, 252, 0.2);
  border-radius: 0.5rem;
  font-size: 0.75rem;
}

.hint-title {
  margin: 0 0 0.5rem 0;
  color: var(--text-secondary);
  font-weight: 600;
  font-size: 0.75rem;
  text-transform: uppercase;
  letter-spacing: 0.5px;
}

.hint-list {
  list-style: none;
  padding: 0;
  margin: 0;
  display: flex;
  flex-direction: column;
  gap: 0.25rem;
}

.hint-list li {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  color: var(--text-secondary);
  transition: color 0.2s ease;
}

.hint-list li.valid {
  color: #10b981;
}

.hint-list li .iconify {
  font-size: 1rem;
  flex-shrink: 0;
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
  left: -50%;
  width: 200%;
  height: 200%;
  background: radial-gradient(circle, rgba(102, 126, 234, 0.1) 0%, transparent 70%);
  animation: pulse 4s ease-in-out infinite reverse;
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
}
</style>