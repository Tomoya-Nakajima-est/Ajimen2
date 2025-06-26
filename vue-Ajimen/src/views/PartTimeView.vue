
<template>
<div class="part-page">
  <PartTimeLayout />
  <main class="main-content">

  </main>
  <aside class="right-panel">
    <ShiftStatus />
  </aside>
  </div>
</template>

<script setup>
import PartTimeLayout from '@/components/PartTimeLayout.vue'
import ShiftStatus from '../components/ShiftStatus.vue';
</script>

<style scoped>
.part-page {
  position: fixed;
  top: 0;
  left: 0;
  width: 100vw;
  height: 100vh;
  display: flex;
  justify-content: center;
  align-items: center;
  background-color: #5c9291;
  overflow: hidden;
  z-index: 0;
}
/*クロス線の追加*/ 
.part-page::before,
.part-page::after{
  content:'';
  position: absolute;
  background-color: black;
  z-index: 1;
}
/*横線*/
.part-page::before{
  bottom: 60px;
  left: 0;
  width: 100%;
  height: 50px;
  transform: rotate(0deg);
}

/*縦線*/
.part-page::after{
  bottom: 0px;
  left: 120px;
  width: 50px;
  height: 100%;
  transform: rotate(0deg);
}
.main-content {
  margin-left: 200px;
  padding: 20px;
  font-family: 'Georgia', serif;
}
</style>