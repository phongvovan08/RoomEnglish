# vue3-clean-template

This template should help get you started developing with Vue 3 in Vite.

## Recommended IDE Setup

[VSCode](https://code.visualstudio.com/) + [Volar](https://marketplace.visualstudio.com/items?itemName=Vue.volar) (and disable Vetur).

## Type Support for `.vue` Imports in TS

TypeScript cannot handle type information for `.vue` imports by default, so we replace the `tsc` CLI with `vue-tsc` for type checking. In editors, we need [Volar](https://marketplace.visualstudio.com/items?itemName=Vue.volar) to make the TypeScript language service aware of `.vue` types.

## Customize configuration

See [Vite Configuration Reference](https://vite.dev/config/).

## Project Setup

```sh
npm install
```

### Compile and Hot-Reload for Development

```sh
npm run dev
```

### Type-Check, Compile and Minify for Production

```sh
npm run build
```

### Run Unit Tests with [Vitest](https://vitest.dev/)

```sh
npm run test:unit
```

### Lint with [ESLint](https://eslint.org/)

```sh
npm run lint
```

## Project Clean Architecture with Repositories pattern

### Layers

- #### Core/Domain - Entities, Interfaces, Use-Cases, Services
- #### Infrastructure - API, Repositories, Storage
- #### (Presenter) and UI - Modules

### Example Folder Structure

src/
│
├── core/ # Core Layer (Business Logic)
│ ├── entities/ # Domain Entities
│ ├── interfaces/ # Repository Interfaces (Abstractions)
│ └── services/ # UseCases (Specific Business UseCase) and Shared Services (LoggingService...)
│
├── infrastructure/ # Infrastructure Layer (Implementation Details)
│ ├── api/ # API Clients (e.g., Axios, Fetch)
│ ├── repositories/ # Repository Implementations
│ └── storage/ # Local Storage.
│
└── modules/ # Presentation Layer (UI)
[name]  
 ├── components/ # UI Components
├── views/ # Views
├── composables/ # Custom composable for encapsulate and reuse stateful logic.
├── layouts/ # Custom Layouts for module
├── types/ # Modules types
└── stores/ # State Management (Pinia)

### Summary of Interaction:

- Core: Defines what the application does (business logic).
- Infrastructure: This layer implements the abstractions defined in the Core layer. It includes API clients, repository implementations, and storage mechanisms.

  - API: Handles HTTP requests (e.g., Axios, Fetch).
  - Repositories: Implement the repository interfaces from Core layer
  - Storage: Manages local storage, cookies...

- Presentation/UI: Defines how users see and interact with the application.

## Pre-packed

### UI Frameworks

- [TailwindCss](https://tailwindcss.com/) - The instant on-demand atomic CSS engine.
  - Think of Tailwind CSS itself as your preprocessor — you shouldn't use Tailwind with Sass for the same reason you wouldn't use Sass with Stylus. Check [compatibility](https://tailwindcss.com/docs/compatibility)
  - **Don't add css preprocessor like Sass, Stylus**, utilize modern [css nesting](https://developer.mozilla.org/en-US/docs/Web/CSS/CSS_nesting/Using_CSS_nesting) for your styles instead

- [PrimeVue](https://primevue.org/) - Feature-rich UI components.
#### Important:
The components within the modules should not directly import any external libraries.

Instead, all external libraries should be used inside the shared/components directory. Some components will act as proxies between the module components and the external libraries.

This approach minimizes the impact of changes. For instance, if a library is no longer supported, you only need to rewrite the Proxy component in the shared/components directory, rather than making changes across multiple components in different modules.
### Icons

- [Iconify](https://iconify.design) - use icons from any icon sets [🔍Icônes](https://icones.netlify.app/)

### Plugins

- [Vue Router](https://github.com/vuejs/router)

- [Pinia](https://pinia.vuejs.org) - Intuitive, type safe, light and flexible Store for Vue using the composition api
- [`unplugin-auto-import`](https://github.com/antfu/unplugin-auto-import) - Directly use Vue Composition API and others without importing

- [Vue I18n](https://github.com/intlify/vue-i18n-next) - Internationalization
  - [`unplugin-vue-i18n`](https://github.com/intlify/bundle-tools/tree/main/packages/unplugin-vue-i18n) - unplugin for Vue I18n
- [VueUse](https://github.com/antfu/vueuse) - collection of useful composition APIs
- [`@vueuse/head`](https://github.com/vueuse/head) - manipulate document head reactively

### Coding Style

- Use Composition API with [`<script setup>` SFC syntax](https://github.com/vuejs/rfcs/pull/227)
- [ESLint](https://eslint.org/) and [Prettier](https://prettier.io/) for code format



## Composables introduction

### Rules of [composables](https://vuejs.org/guide/reusability/composables)

#### Name of composable

- It is a convention to name composable functions with camelCase names that start with "use".

#### Composables placement

- Composables should only be called in `<script setup>` or the `setup()` hook or another composable in order to
  - Lifecycle hooks can be registered to it.
  - Computed properties and watchers can be linked to it, so that they can be disposed when the instance is unmounted to prevent memory leaks

Examples of incorrect code for this rule:

```
function foo() {
  /* BAD: the lifecycle hook is in non-composable function */
  onMounted(() => {})
}

function useBaz() {
  function qux() {
    /* BAD: parent function is non-composable function */
    onMounted(() => {})
  }
}

export default defineComponent({
  async setup() {
    await fetch()

    /* BAD: the lifecycle hook is after `await` */
    onMounted(() => {})
  }
```

Examples of correct code for this rule:

```
function useFoo() {
  /* GOOD: composable is in another composable */
  useBar()
}

export default defineComponent({
  setup() {
    /* GOOD: composable is in setup() */
    useBaz()
  }
})
```

### Composables for code organization

- Composables can be extracted not only for reuse, but also for code organization. As the complexity of your components grow, you may end up with components that are too large to navigate and reason about. Composition API gives you the full flexibility to organize your component code into smaller functions based on logical concerns

```
<script setup>
import { useFeatureA } from './featureA.js'
import { useFeatureB } from './featureB.js'
import { useFeatureC } from './featureC.js'

const { foo, bar } = useFeatureA()
const { baz } = useFeatureB(foo)
const { qux } = useFeatureC(baz)
</script>
```

### useQuery as a promise handler for static options

- Utilize the useQuery composables with built-in cache.
  - Use only with non-frequently changes options
  - With some global options that need to be use across the app
  - Get requests with secondsToLive

In `useStaticOptions.ts`

```
function useStaticOptions() {
  const {execute, isLoading, data} = useQuery({
    key: 'example',
    promiseFn: (params) => exampleService.getStaticOptions(params)
  })

}

```


### usePromiseWrapper as a promise handler

- Handle promise in your app (get, post....)


In `useEditPost.ts`

```
function useEditPost() {
  const {execute, isLoading} = usePromiseWrapper({
    key: 'example',
    promiseFn: (params) => exampleService.editPost(params)
  })

}

```


### Remarks

- Composable must be camelCase names that start with "use".
- Composables can be used in `<script setup>` and other composables (should be placed in `<script setup` after all)
- Composables is not only for reuse, but also for code organization based on logical concerns

## Best Practises to follow

- Following eslint and sonarlints rules
- Trying not to use any as your type
- Using camelCase for your functions and variables

**Avoid these codes**

```
const model = ref<any>()
function createAbcXyz(apiModel: any) {}
```

- Avoid nested try catch, try to utilize `usePromiseWrapper` and `useQuery` composable to handle promises

**Avoid these codes**

In `ExampleService.ts`

```

function getData() {
  try {
    await promiseFn()
  }
  catch(err) {
    throw err
  }
}

```

In `useExampleComposable.ts`

```

function useExampleComposable() {
  function getData() {
    try {
      await exampleService.getData()
    }
    catch(err) {
      console.error(err)
    }
  }

}

```

**Using this instead**

In `ExampleService.ts`

```

function getData() {
  return promiseFn()
}

```

In `useExampleComposable.ts`

```

function useExampleComposable() {
  const {execute, isLoading, data} = useQuery({
    key: 'example',
    promiseFn: (params) => exampleService.getData(params)
  })

}

```

- Avoid using `==` when compares in source code, using `===` instead

**Bad**

```
if(a == b){
  // doSth
}
```

**Good**

```
if(a === b){
  // doSth
}
```

- `<script setup>` was an idea place for calling your api function

```
<script setup lang="ts">
import {usePosts} from '../composables/use-posts'

const {getPosts} = usePosts()

// directly in setup as it will be run before other life cycle hooks, take a look at life cycle diagram(https://vuejs.org/guide/essentials/lifecycle#lifecycle-diagram)
getPosts()
</script>
```

Maximize using derived states by `computed` instead of `ref()` or `reactive()`

**Bad**

```
<script setup lang="ts">

const state = ref()
const derivedState = ref()

watch(state, (val) => {
  derivedState.value = getDerivedState(val)
})

getPosts()
</script>

```

**Good**

```
<script setup lang="ts">

const state = ref()

const derivedState = computed(() => getDerivedState(state.value))
getPosts()
</script>

```
