declare module 'vue-virtual-scroller' {
  import { DefineComponent } from 'vue'

  export interface RecycleScrollerMethods {
    scrollToItem(index: number): void
    scrollToPosition(position: number): void
    getScroll(): { start: number; end: number }
  }

  export const RecycleScroller: DefineComponent<{
    items: any[]
    itemSize: number
    keyField?: string
    buffer?: number
  }> & RecycleScrollerMethods

  export const DynamicScroller: DefineComponent<{
    items: any[]
    minItemSize: number
    keyField?: string
  }>

  export const DynamicScrollerItem: DefineComponent<{
    item: any
    active: boolean
    sizeDependencies?: any[]
    watchData?: boolean
  }>
}
