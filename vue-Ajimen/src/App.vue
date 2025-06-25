<template>
  <div id="app">
    <router-view /> <!-- ← ページ表示の切替 -->
  </div>
</template>

<script>
export default {
  name: "App",
};
</script>
<!-- <script setup>
import { RouterLink, RouterView } from 'vue-router'
    import HelloWorld from './components/HelloWorld.vue'
    import InventoryView from './views/InventoryView.vue'
</script>

<template>
    <InventoryView />
</template> -->

