<template>
  <div class="employee-view">
    <EmployeeLayout />
    <main class="main-content">
      <ShiftEditter />
    </main>
  </div>
</template>

<script setup>
import EmployeeLayout from '@/components/EmployeeLayout.vue'
import ShiftEditter from '@/components/ShiftEditter.vue';
</script>

<style scoped>
.employee-view {
  position: fixed;
  top: 0;
  left: 0;
  width: 100vw;
  height: 100vh;
  display: flex;
  justify-content: center;
  align-items: center;
  background-color: #460125;
  overflow: hidden;
  z-index: 0;
}
/*クロス線の追加*/ 
.employee-view::before,
.employee-view::after{
  content:'';
  position: absolute;
  background-color: black;
  z-index: 1;
}
/*横線 */
.employee-view::before{
  bottom: 60px;
  left: 0;
  width: 100%;
  height: 50px;
  transform: rotate(0deg);
}
/*縦線*/
.employee-view::after{
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