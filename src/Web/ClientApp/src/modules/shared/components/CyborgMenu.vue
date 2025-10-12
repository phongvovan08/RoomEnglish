<template>
  <!-- Cyborg Gaming Navigation -->
  <header class="cyborg-header" :class="{ 'header-sticky': isSticky }">
    <div class="cyborg-container">
      <nav class="cyborg-nav">
        <!-- Logo -->
        <router-link :to="Routes.Home.path" class="logo">
          <Icon icon="mdi:gamepad-variant" class="w-8 h-8 mr-2" />
          <span class="logo-text">RoomEnglish</span>
        </router-link>
        
        <!-- Search Input -->
        <div class="search-input" v-if="!isMobile">
          <form @submit.prevent="handleSearch">
            <input 
              type="text" 
              :placeholder="$t('menu.search')"
              v-model="searchQuery"
              class="search-field"
            />
            <button type="submit" class="search-btn">
              <Icon icon="mdi:magnify" class="w-5 h-5" />
            </button>
          </form>
        </div>
        
        <!-- Desktop Navigation -->
        <ul class="nav-menu" v-if="!isMobile">
          <li v-for="item in menuItems" :key="item.name">
            <router-link 
              :to="item.path" 
              class="nav-link"
              :class="{ 'active': isActiveRoute(item.path) }"
              @click="item.hasChildren && toggleDropdown(item.name)"
            >
              <Icon :icon="item.icon" class="w-5 h-5 mr-2" />
              {{ $t(item.label) }}
              <Icon v-if="item.hasChildren" icon="mdi:chevron-down" class="w-4 h-4 ml-1" />
            </router-link>
            
            <!-- Dropdown Menu -->
            <div v-if="item.hasChildren && dropdownOpen[item.name]" class="dropdown-menu">
              <router-link
                v-for="child in item.children"
                :key="child.name"
                :to="child.path"
                class="dropdown-link"
                @click="closeDropdown(item.name)"
              >
                <Icon :icon="child.icon" class="w-4 h-4 mr-2" />
                {{ $t(child.label) }}
              </router-link>
            </div>
          </li>
          
          <!-- Authentication Section -->
          <li class="auth-section" v-if="!isAuthenticated">
            <router-link :to="Routes.Auth.children.Login.path" class="nav-link login-btn">
              <Icon icon="mdi:login" class="w-5 h-5 mr-2" />
              {{ $t('auth.login') }}
            </router-link>
            <router-link :to="Routes.Auth.children.Register.path" class="nav-link register-btn">
              <Icon icon="mdi:account-plus" class="w-5 h-5 mr-2" />
              {{ $t('auth.register') }}
            </router-link>
          </li>
          
          <!-- User Profile (when authenticated) -->
          <li class="profile-menu" v-else>
            <button @click="toggleUserMenu" class="profile-btn">
              <span class="user-name">{{ user?.email || $t('menu.profile') }}</span>
              <div class="profile-avatar-placeholder"></div>
            </button>
            
            <!-- User Dropdown -->
            <div v-if="userMenuOpen" class="user-dropdown">
              <router-link :to="Routes.Users.children.Profile.path" class="dropdown-link" @click="closeUserMenu">
                <Icon icon="mdi:account" class="w-4 h-4 mr-2" />
                {{ $t('menu.profile') }}
              </router-link>
              <router-link :to="Routes.Users.children.Settings.path" class="dropdown-link" @click="closeUserMenu">
                <Icon icon="mdi:cog" class="w-4 h-4 mr-2" />
                {{ $t('menu.settings') }}
              </router-link>
              <button @click="toggleLanguage" class="dropdown-link">
                <Icon icon="mdi:translate" class="w-4 h-4 mr-2" />
                {{ $t('menu.changeLanguage') }}
              </button>
              <hr class="dropdown-divider">
              <button @click="handleLogout" class="dropdown-link logout-btn">
                <Icon icon="mdi:logout" class="w-4 h-4 mr-2" />
                {{ $t('menu.logout') }}
              </button>
            </div>
          </li>
        </ul>
        
        <!-- Mobile Menu Trigger -->
        <button class="menu-trigger" @click="toggleMobileMenu" v-if="isMobile">
          <span></span>
          <span></span>
          <span></span>
        </button>
      </nav>
    </div>
    
    <!-- Mobile Navigation -->
    <div class="mobile-nav" :class="{ 'active': mobileMenuOpen }" v-if="isMobile">
      <!-- Mobile Search -->
      <div class="mobile-search">
        <form @submit.prevent="handleSearch">
          <input 
            type="text" 
            :placeholder="$t('menu.search')"
            v-model="searchQuery"
            class="cyborg-input"
          />
          <button type="submit" class="cyborg-btn">
            <Icon icon="mdi:magnify" class="w-5 h-5" />
          </button>
        </form>
      </div>
      
      <!-- Mobile Menu Items -->
      <ul class="mobile-menu">
        <li v-for="item in menuItems" :key="item.name">
          <router-link 
            :to="item.path" 
            class="mobile-link"
            :class="{ 'active': isActiveRoute(item.path) }"
            @click="closeMobileMenu"
          >
            <Icon :icon="item.icon" class="w-5 h-5 mr-3" />
            {{ $t(item.label) }}
          </router-link>
          
          <!-- Mobile Submenu -->
          <div v-if="item.hasChildren" class="mobile-submenu">
            <router-link
              v-for="child in item.children"
              :key="child.name"
              :to="child.path"
              class="mobile-sublink"
              @click="closeMobileMenu"
            >
              <Icon :icon="child.icon" class="w-4 h-4 mr-3" />
              {{ $t(child.label) }}
            </router-link>
          </div>
        </li>
      </ul>
      
      <!-- Mobile User Actions -->
      <div class="mobile-user-section">
        <!-- Auth buttons when not authenticated -->
        <div v-if="!isAuthenticated" class="mobile-auth">
          <router-link :to="Routes.Auth.children.Login.path" class="mobile-link login-btn" @click="closeMobileMenu">
            <Icon icon="mdi:login" class="w-5 h-5 mr-3" />
            {{ $t('auth.login') }}
          </router-link>
          <router-link :to="Routes.Auth.children.Register.path" class="mobile-link register-btn" @click="closeMobileMenu">
            <Icon icon="mdi:account-plus" class="w-5 h-5 mr-3" />
            {{ $t('auth.register') }}
          </router-link>
        </div>
        
        <!-- User menu when authenticated -->
        <div v-else class="mobile-user">
          <div class="mobile-user-info">
            <Icon icon="mdi:account-circle" class="w-6 h-6 mr-2" />
            <span>{{ user?.email || $t('menu.profile') }}</span>
          </div>
          <router-link :to="Routes.Users.children.Profile.path" class="mobile-link" @click="closeMobileMenu">
            <Icon icon="mdi:account" class="w-5 h-5 mr-3" />
            {{ $t('menu.profile') }}
          </router-link>
          <router-link :to="Routes.Users.children.Settings.path" class="mobile-link" @click="closeMobileMenu">
            <Icon icon="mdi:cog" class="w-5 h-5 mr-3" />
            {{ $t('menu.settings') }}
          </router-link>
          <button @click="toggleLanguage" class="mobile-link">
            <Icon icon="mdi:translate" class="w-5 h-5 mr-3" />
            {{ $t('menu.changeLanguage') }}
          </button>
          <button @click="handleLogout" class="mobile-link logout-btn">
            <Icon icon="mdi:logout" class="w-5 h-5 mr-3" />
            {{ $t('menu.logout') }}
          </button>
        </div>
      </div>
    </div>
  </header>
</template>

<script setup lang="ts">
import { Routes } from '@/router/constants'
import { loadLanguageAsync } from '@/plugins/ui/i18n'
import { useAuth } from '@/composables/useAuth'

// Reactive data
const searchQuery = ref('')
const dropdownOpen = ref<Record<string, boolean>>({})
const userMenuOpen = ref(false)
const mobileMenuOpen = ref(false)
const isSticky = ref(false)
const isMobile = ref(false)

const { locale } = useI18n()
const route = useRoute()

// Authentication
const { isAuthenticated, user, logout: authLogout } = useAuth()

// Menu configuration
const menuItems = computed(() => [
  {
    name: 'home',
    label: 'menu.home',
    path: Routes.Home.path,
    icon: 'mdi:home',
    hasChildren: false,
  },
  {
    name: 'dashboard',
    label: 'menu.dashboard',
    path: Routes.Dashboard.path,
    icon: 'mdi:view-dashboard',
    hasChildren: false,
  },
  {
    name: 'todoLists',
    label: 'menu.todoLists',
    path: Routes.TodoLists.path,
    icon: 'mdi:format-list-bulleted-square',
    hasChildren: true,
    children: [
      {
        name: 'todoListsList',
        label: 'menu.viewAll',
        path: Routes.TodoLists.children.List.path,
        icon: 'mdi:view-list',
      },
      {
        name: 'createTodoList',
        label: 'menu.create',
        path: Routes.TodoLists.children.Create.path,
        icon: 'mdi:plus-circle',
      },
    ],
  },
  {
    name: 'todoItems',
    label: 'menu.todoItems',
    path: Routes.TodoItems.path,
    icon: 'mdi:checkbox-marked-circle',
    hasChildren: true,
    children: [
      {
        name: 'todoItemsList',
        label: 'menu.viewAll',
        path: Routes.TodoItems.children.List.path,
        icon: 'mdi:view-list',
      },
      {
        name: 'createTodoItem',
        label: 'menu.create',
        path: Routes.TodoItems.children.Create.path,
        icon: 'mdi:plus-circle',
      },
    ],
  },
  {
    name: 'weatherForecasts',
    label: 'menu.weatherForecasts',
    path: Routes.WeatherForecasts.path,
    icon: 'mdi:weather-cloudy',
    hasChildren: false,
  },
  {
    name: 'posts',
    label: 'menu.posts',
    path: Routes.Posts.path,
    icon: 'mdi:post',
    hasChildren: false,
  },
])

// Methods
const isActiveRoute = (path: string) => {
  return route.path.startsWith(path) && path !== '/'
}

const handleSearch = () => {
  if (searchQuery.value.trim()) {
    console.log('Search:', searchQuery.value)
    // Implement search functionality
  }
}

const toggleDropdown = (itemName: string) => {
  dropdownOpen.value[itemName] = !dropdownOpen.value[itemName]
  // Close other dropdowns
  Object.keys(dropdownOpen.value).forEach(key => {
    if (key !== itemName) {
      dropdownOpen.value[key] = false
    }
  })
}

const closeDropdown = (itemName: string) => {
  dropdownOpen.value[itemName] = false
}

const toggleUserMenu = () => {
  userMenuOpen.value = !userMenuOpen.value
}

const closeUserMenu = () => {
  userMenuOpen.value = false
}

const toggleMobileMenu = () => {
  mobileMenuOpen.value = !mobileMenuOpen.value
}

const closeMobileMenu = () => {
  mobileMenuOpen.value = false
}

const toggleLanguage = () => {
  const newLocale = locale.value === 'en' ? 'fr' : 'en'
  loadLanguageAsync(newLocale)
  closeUserMenu()
}

const handleLogout = async () => {
  try {
    await authLogout()
    closeUserMenu()
    closeMobileMenu()
  } catch (error) {
    console.error('Logout error:', error)
  }
}

// Handle responsive behavior
const handleResize = () => {
  isMobile.value = window.innerWidth <= 768
  if (!isMobile.value) {
    mobileMenuOpen.value = false
  }
}

// Handle scroll for sticky header
const handleScroll = () => {
  isSticky.value = window.scrollY > 100
}

// Lifecycle
onMounted(() => {
  handleResize()
  window.addEventListener('resize', handleResize)
  window.addEventListener('scroll', handleScroll)
  
  // Close menus when clicking outside
  document.addEventListener('click', (e) => {
    const target = e.target as HTMLElement
    if (!target.closest('.profile-menu')) {
      userMenuOpen.value = false
    }
    if (!target.closest('.menu-trigger') && !target.closest('.mobile-nav')) {
      mobileMenuOpen.value = false
    }
  })
})

onUnmounted(() => {
  window.removeEventListener('resize', handleResize)
  window.removeEventListener('scroll', handleScroll)
})

// Close mobile menu on route change
watch(() => route.path, () => {
  closeMobileMenu()
})
</script>

<style scoped>
/* Cyborg Gaming Header Styles */
.cyborg-header {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  z-index: 1000;
  background-color: var(--primary-bg);
  border-bottom: 1px solid var(--border-color);
  transition: var(--transition-normal);
  padding: 15px 0;
}

.cyborg-header.header-sticky {
  background-color: rgba(30, 30, 30, 0.95);
  backdrop-filter: blur(10px);
  padding: 10px 0;
}

.cyborg-nav {
  display: flex;
  align-items: center;
  justify-content: space-between;
}

/* Logo */
.logo {
  display: flex;
  align-items: center;
  color: var(--text-primary);
  font-size: 28px;
  font-weight: 700;
  text-decoration: none;
  transition: var(--transition-normal);
}

.logo:hover {
  color: var(--accent-pink);
}

.logo-text {
  background: var(--gradient-primary);
  -webkit-background-clip: text;
  -webkit-text-fill-color: transparent;
  background-clip: text;
}

/* Search Input */
.search-input {
  flex: 1;
  max-width: 400px;
  margin: 0 40px;
}

.search-input form {
  position: relative;
}

.search-field {
  width: 100%;
  background-color: var(--secondary-bg);
  border: 2px solid var(--border-color);
  border-radius: 23px;
  padding: 12px 50px 12px 20px;
  color: var(--text-primary);
  font-size: 14px;
  transition: var(--transition-normal);
}

.search-field:focus {
  border-color: var(--accent-pink);
  outline: none;
}

.search-field::placeholder {
  color: var(--text-muted);
}

.search-btn {
  position: absolute;
  right: 15px;
  top: 50%;
  transform: translateY(-50%);
  background: none;
  border: none;
  color: var(--text-secondary);
  cursor: pointer;
  transition: var(--transition-normal);
}

.search-btn:hover {
  color: var(--accent-pink);
}

/* Navigation Menu */
.nav-menu {
  display: flex;
  align-items: center;
  gap: 30px;
  list-style: none;
  margin: 0;
  padding: 0;
}

.nav-menu li {
  position: relative;
}

.nav-link {
  display: flex;
  align-items: center;
  color: var(--text-secondary);
  text-decoration: none;
  padding: 10px 0;
  font-weight: 500;
  transition: var(--transition-normal);
}

.nav-link:hover,
.nav-link.active {
  color: var(--accent-pink);
}

/* Dropdown Menu */
.dropdown-menu {
  position: absolute;
  top: 100%;
  left: 0;
  min-width: 200px;
  background-color: var(--card-bg);
  border: 1px solid var(--border-color);
  border-radius: var(--radius-sm);
  padding: 15px;
  margin-top: 10px;
  box-shadow: var(--shadow-card);
  z-index: 1001;
}

.dropdown-link {
  display: flex;
  align-items: center;
  color: var(--text-secondary);
  text-decoration: none;
  padding: 10px 15px;
  border-radius: 8px;
  transition: var(--transition-normal);
  border: none;
  background: none;
  width: 100%;
  text-align: left;
  cursor: pointer;
  font-size: 14px;
}

.dropdown-link:hover {
  background-color: var(--secondary-bg);
  color: var(--accent-pink);
}

.dropdown-divider {
  border: none;
  height: 1px;
  background-color: var(--border-color);
  margin: 10px 0;
}

.logout-btn:hover {
  color: #ff4757 !important;
}

/* Profile Menu */
.profile-menu {
  position: relative;
}

.profile-btn {
  display: flex;
  align-items: center;
  background: none;
  border: none;
  color: var(--text-secondary);
  cursor: pointer;
  padding: 10px;
  font-weight: 500;
  transition: var(--transition-normal);
}

.profile-btn:hover {
  color: var(--accent-pink);
}

.profile-avatar {
  width: 40px;
  height: 40px;
  border-radius: 50%;
  margin-left: 10px;
  border: 2px solid var(--accent-secondary);
}

.profile-avatar-placeholder {
  width: 40px;
  height: 40px;
  border-radius: 50%;
  margin-left: 10px;
  border: 2px solid var(--accent-secondary);
  background: var(--gradient-primary);
  display: flex;
  align-items: center;
  justify-content: center;
  color: white;
  font-size: 16px;
  font-weight: 600;
}

.profile-avatar-placeholder::before {
  content: "U";
}

.user-dropdown {
  position: absolute;
  top: 100%;
  right: 0;
  min-width: 180px;
  background-color: var(--card-bg);
  border: 1px solid var(--border-color);
  border-radius: var(--radius-sm);
  padding: 15px;
  margin-top: 10px;
  box-shadow: var(--shadow-card);
  z-index: 1001;
}

/* Mobile Menu Trigger */
.menu-trigger {
  display: flex;
  flex-direction: column;
  gap: 4px;
  background: none;
  border: none;
  cursor: pointer;
  padding: 8px;
}

.menu-trigger span {
  width: 25px;
  height: 3px;
  background-color: var(--text-primary);
  transition: var(--transition-normal);
  border-radius: 2px;
}

.menu-trigger:hover span {
  background-color: var(--accent-pink);
}

/* Mobile Navigation */
.mobile-nav {
  position: fixed;
  top: 70px;
  left: 0;
  right: 0;
  background-color: var(--primary-bg);
  border-top: 1px solid var(--border-color);
  max-height: 0;
  overflow: hidden;
  transition: max-height 0.3s ease;
  z-index: 999;
}

.mobile-nav.active {
  max-height: calc(100vh - 70px);
  overflow-y: auto;
}

.mobile-search {
  padding: 20px;
  border-bottom: 1px solid var(--border-color);
}

.mobile-search form {
  display: flex;
  gap: 10px;
}

.mobile-search input {
  flex: 1;
}

.mobile-menu {
  list-style: none;
  margin: 0;
  padding: 0;
}

.mobile-link {
  display: flex;
  align-items: center;
  color: var(--text-secondary);
  text-decoration: none;
  padding: 20px;
  border-bottom: 1px solid var(--border-color);
  transition: var(--transition-normal);
  border: none;
  background: none;
  width: 100%;
  text-align: left;
  cursor: pointer;
  font-size: 16px;
}

.mobile-link:hover,
.mobile-link.active {
  background-color: var(--secondary-bg);
  color: var(--accent-pink);
}

.mobile-submenu {
  background-color: var(--secondary-bg);
  padding-left: 20px;
}

.mobile-sublink {
  display: flex;
  align-items: center;
  color: var(--text-muted);
  text-decoration: none;
  padding: 15px 20px;
  border-bottom: 1px solid var(--border-color);
  transition: var(--transition-normal);
  font-size: 14px;
}

.mobile-sublink:hover {
  color: var(--accent-pink);
}

.mobile-user-section {
  border-top: 2px solid var(--border-color);
  margin-top: 20px;
}

/* Responsive Design */
@media (min-width: 769px) {
  .menu-trigger,
  .mobile-nav {
    display: none;
  }
}

@media (max-width: 768px) {
  .search-input,
  .nav-menu {
    display: none;
  }
  
  .cyborg-nav {
    justify-content: space-between;
  }
  
  .logo {
    font-size: 24px;
  }
}

/* Animations */
@keyframes fadeIn {
  from {
    opacity: 0;
    transform: translateY(-10px);
  }
  to {
    opacity: 1;
    transform: translateY(0);
  }
}

.dropdown-menu,
.user-dropdown {
  animation: fadeIn 0.3s ease;
}

/* Authentication Section */
.auth-section {
  display: flex;
  align-items: center;
  gap: 15px;
}

.login-btn {
  background: transparent !important;
  border: 1px solid var(--accent-color);
  border-radius: 20px;
  padding: 8px 16px !important;
  transition: all 0.3s ease;
}

.login-btn:hover {
  background: var(--accent-color) !important;
  color: var(--primary-bg) !important;
  transform: translateY(-2px);
  box-shadow: 0 4px 15px rgba(236, 96, 144, 0.3);
}

.register-btn {
  background: linear-gradient(135deg, var(--accent-color), var(--secondary-accent)) !important;
  border: none;
  border-radius: 20px;
  padding: 8px 16px !important;
  transition: all 0.3s ease;
}

.register-btn:hover {
  transform: translateY(-2px);
  box-shadow: 0 4px 20px rgba(236, 96, 144, 0.4);
  background: linear-gradient(135deg, #f06292, var(--secondary-accent)) !important;
}

/* Mobile auth styles */
.mobile-auth {
  display: flex;
  flex-direction: column;
  gap: 10px;
  margin-bottom: 20px;
  padding: 20px;
  border-bottom: 1px solid rgba(255, 255, 255, 0.1);
}

.mobile-user-info {
  display: flex;
  align-items: center;
  padding: 15px 20px;
  background: rgba(236, 96, 144, 0.1);
  border-radius: 10px;
  margin-bottom: 15px;
  color: var(--accent-color);
  font-weight: 600;
}

.user-name {
  font-weight: 600;
  color: var(--accent-color);
}
</style>