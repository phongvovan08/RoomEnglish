export interface UserProfile {
  id: string
  email: string
  firstName: string
  lastName: string
  displayName: string
  phone: string
  bio: string
  avatar: string
  createdAt: string
  lastLoginAt: string
}

export interface PersonalForm {
  firstName: string
  lastName: string
  displayName: string
  phone: string
  bio: string
}

export interface PasswordForm {
  currentPassword: string
  newPassword: string
  confirmPassword: string
}

export interface Preferences {
  emailNotifications: boolean
  dailyReminder: boolean
  soundEffects: boolean
  language: string
}
