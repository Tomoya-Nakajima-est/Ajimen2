<template>
  <div class="part-page">
    <PartTimeLayout />
    <main class="main-content">
      <ShiftTable />
    </main>
  </div>
</template>

<script setup>
import PartTimeLayout from '@/components/EmployeeLayout.vue'
import ShiftTable from '@/components/ShiftEditter.vue';
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

.part-page::before,
.part-page::after {
  content: '';
  position: absolute;
  background-color: black;
  z-index: 1;
}
.part-page::before {
  bottom: 60px;
  left: 0;
  width: 100%;
  height: 50px;
}
.part-page::after {
  bottom: 0;
  left: 120px;
  width: 50px;
  height: 100%;
}
.main-content {
  margin-left: 200px;
  padding: 20px;
  font-family: 'Georgia', serif;
}
</style>