<template>
  <nav class="bg-white shadow-lg border-b border-gray-200">
    <!-- Desktop Navigation -->
    <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
      <div class="flex justify-between h-16">
        <div class="flex">
          <!-- Logo -->
          <div class="flex-shrink-0 flex items-center">
            <router-link :to="Routes.Home.path" class="text-xl font-bold text-blue-600">
              RoomEnglish
            </router-link>
          </div>
          
          <!-- Desktop Menu -->
          <div class="hidden sm:ml-6 sm:flex sm:space-x-8">
            <router-link
              v-for="item in menuItems"
              :key="item.name"
              :to="item.path"
              class="inline-flex items-center px-1 pt-1 border-b-2 text-sm font-medium transition-colors duration-200"
              :class="isActiveRoute(item.path) ? activeClass : inactiveClass"
              @mouseenter="item.hasChildren && toggleDropdown(item.name, true)"
              @mouseleave="item.hasChildren && toggleDropdown(item.name, false)"
            >
              <Icon :icon="item.icon" class="mr-2" />
              {{ $t(item.label) }}
              <Icon v-if="item.hasChildren" icon="mdi:chevron-down" class="ml-1" />
              
              <!-- Dropdown Menu -->
              <div
                v-if="item.hasChildren && dropdownOpen[item.name]"
                class="absolute top-16 left-0 mt-1 w-48 bg-white rounded-md shadow-lg py-1 z-50 border border-gray-200"
                @mouseenter="toggleDropdown(item.name, true)"
                @mouseleave="toggleDropdown(item.name, false)"
              >
                <router-link
                  v-for="child in item.children"
                  :key="child.name"
                  :to="child.path"
                  class="block px-4 py-2 text-sm text-gray-700 hover:bg-gray-100 transition-colors duration-200"
                >
                  <Icon :icon="child.icon" class="mr-2" />
                  {{ $t(child.label) }}
                </router-link>
              </div>
            </router-link>
          </div>
        </div>
        
        <!-- Right side menu -->
        <div class="hidden sm:ml-6 sm:flex sm:items-center space-x-4">
          <!-- Language Switcher -->
          <button
            @click="toggleLanguage"
            class="p-2 text-gray-500 hover:text-gray-700 hover:bg-gray-100 rounded-full transition-colors duration-200"
            :title="$t('menu.changeLanguage')"
          >
            <Icon icon="fluent-mdl2:locale-language" class="w-5 h-5" />
          </button>
          
          <!-- User Menu -->
          <div class="relative" @click="toggleUserMenu">
            <button
              class="flex items-center space-x-2 p-2 text-gray-500 hover:text-gray-700 hover:bg-gray-100 rounded-full transition-colors duration-200"
            >
              <Icon icon="mdi:account-circle" class="w-6 h-6" />
              <Icon icon="mdi:chevron-down" class="w-4 h-4" />
            </button>
            
            <!-- User Dropdown -->
            <div
              v-if="userMenuOpen"
              class="absolute right-0 top-12 mt-1 w-48 bg-white rounded-md shadow-lg py-1 z-50 border border-gray-200"
            >
              <router-link
                :to="Routes.Users.children.Profile.path"
                class="block px-4 py-2 text-sm text-gray-700 hover:bg-gray-100"
              >
                <Icon icon="mdi:account" class="mr-2" />
                {{ $t('menu.profile') }}
              </router-link>
              <router-link
                :to="Routes.Users.children.Settings.path"
                class="block px-4 py-2 text-sm text-gray-700 hover:bg-gray-100"
              >
                <Icon icon="mdi:cog" class="mr-2" />
                {{ $t('menu.settings') }}
              </router-link>
              <hr class="my-1">
              <button
                @click="logout"
                class="block w-full text-left px-4 py-2 text-sm text-red-600 hover:bg-gray-100"
              >
                <Icon icon="mdi:logout" class="mr-2" />
                {{ $t('menu.logout') }}
              </button>
            </div>
          </div>
        </div>
        
        <!-- Mobile menu button -->
        <div class="sm:hidden flex items-center">
          <button
            @click="toggleMobileMenu"
            class="inline-flex items-center justify-center p-2 rounded-md text-gray-400 hover:text-gray-500 hover:bg-gray-100"
            :aria-expanded="mobileMenuOpen"
          >
            <Icon v-if="!mobileMenuOpen" icon="mdi:menu" class="w-6 h-6" />
            <Icon v-else icon="mdi:close" class="w-6 h-6" />
          </button>
        </div>
      </div>
    </div>
    
    <!-- Mobile Navigation Menu -->
    <div v-if="mobileMenuOpen" class="sm:hidden bg-white border-t border-gray-200">
      <div class="px-2 pt-2 pb-3 space-y-1">
        <template v-for="item in menuItems" :key="item.name">
          <!-- Main menu item -->
          <div>
            <router-link
              :to="item.path"
              class="block px-3 py-2 rounded-md text-base font-medium transition-colors duration-200"
              :class="isActiveRoute(item.path) ? mobileActiveClass : mobileInactiveClass"
              @click="item.hasChildren ? toggleMobileSubmenu(item.name) : closeMobileMenu()"
            >
              <Icon :icon="item.icon" class="mr-2" />
              {{ $t(item.label) }}
              <Icon v-if="item.hasChildren" 
                    :icon="mobileSubmenuOpen[item.name] ? 'mdi:chevron-up' : 'mdi:chevron-down'" 
                    class="ml-auto" />
            </router-link>
            
            <!-- Mobile Submenu -->
            <div v-if="item.hasChildren && mobileSubmenuOpen[item.name]" class="ml-4 mt-1 space-y-1">
              <router-link
                v-for="child in item.children"
                :key="child.name"
                :to="child.path"
                class="block px-3 py-2 rounded-md text-sm text-gray-600 hover:bg-gray-100"
                @click="closeMobileMenu"
              >
                <Icon :icon="child.icon" class="mr-2" />
                {{ $t(child.label) }}
              </router-link>
            </div>
          </div>
        </template>
        
        <!-- Mobile User Menu -->
        <hr class="my-2">
        <div class="space-y-1">
          <router-link
            :to="Routes.Users.children.Profile.path"
            class="block px-3 py-2 rounded-md text-base font-medium text-gray-600 hover:bg-gray-100"
            @click="closeMobileMenu"
          >
            <Icon icon="mdi:account" class="mr-2" />
            {{ $t('menu.profile') }}
          </router-link>
          <router-link
            :to="Routes.Users.children.Settings.path"
            class="block px-3 py-2 rounded-md text-base font-medium text-gray-600 hover:bg-gray-100"
            @click="closeMobileMenu"
          >
            <Icon icon="mdi:cog" class="mr-2" />
            {{ $t('menu.settings') }}
          </router-link>
          <button
            @click="logout"
            class="block w-full text-left px-3 py-2 rounded-md text-base font-medium text-red-600 hover:bg-gray-100"
          >
            <Icon icon="mdi:logout" class="mr-2" />
            {{ $t('menu.logout') }}
          </button>
        </div>
      </div>
    </div>
  </nav>
</template>

<script setup lang="ts">
import { Routes } from '@/router/constants'
import { loadLanguageAsync } from '@/plugins/ui/i18n'

// Reactive data
const mobileMenuOpen = ref(false)
const userMenuOpen = ref(false)
const dropdownOpen = ref<Record<string, boolean>>({})
const mobileSubmenuOpen = ref<Record<string, boolean>>({})

const { locale } = useI18n()
const route = useRoute()

// Menu configuration mapping to backend APIs
const menuItems = computed(() => [
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

// Computed styles
const activeClass = 'border-blue-500 text-gray-900'
const inactiveClass = 'border-transparent text-gray-500 hover:border-gray-300 hover:text-gray-700'
const mobileActiveClass = 'bg-blue-50 border-blue-500 text-blue-700'
const mobileInactiveClass = 'border-transparent text-gray-500 hover:bg-gray-50 hover:text-gray-700'

// Methods
const isActiveRoute = (path: string) => {
  return route.path.startsWith(path) && path !== '/'
}

const toggleMobileMenu = () => {
  mobileMenuOpen.value = !mobileMenuOpen.value
  if (mobileMenuOpen.value) {
    userMenuOpen.value = false
  }
}

const closeMobileMenu = () => {
  mobileMenuOpen.value = false
  mobileSubmenuOpen.value = {}
}

const toggleUserMenu = () => {
  userMenuOpen.value = !userMenuOpen.value
}

const toggleDropdown = (itemName: string, show: boolean) => {
  dropdownOpen.value[itemName] = show
}

const toggleMobileSubmenu = (itemName: string) => {
  mobileSubmenuOpen.value[itemName] = !mobileSubmenuOpen.value[itemName]
}

const toggleLanguage = () => {
  const newLocale = locale.value === 'en' ? 'fr' : 'en'
  loadLanguageAsync(newLocale)
}

const logout = () => {
  // Implement logout logic here
  console.log('Logout clicked')
  closeMobileMenu()
  userMenuOpen.value = false
}

// Close dropdowns when clicking outside
onMounted(() => {
  document.addEventListener('click', (event) => {
    const target = event.target as HTMLElement
    if (!target.closest('.relative')) {
      userMenuOpen.value = false
      dropdownOpen.value = {}
    }
  })
})

// Close mobile menu on route change
watch(() => route.path, () => {
  closeMobileMenu()
})
</script>

<style scoped>
/* Additional custom styles if needed */
.router-link-active {
  border-color: #3b82f6;
  color: #111827;
}
</style>